using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LightStep.Collector;
using LightStep.Propagation;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;
using LightStep.Logging;

namespace LightStep
{
    /// <inheritdoc />
    public sealed class Tracer : ITracer
    {
        private readonly object _lock = new object();
        internal readonly Options _options;
        private readonly IPropagator _propagator;
        private readonly ILightStepHttpClient _httpClient;
        private readonly IReportTranslator _translator;
        private ISpanRecorder _spanRecorder;
        private readonly Timer _reportLoop;
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();
        private int currentDroppedSpanCount;
        private bool _firstReportHasRun;

        /// <inheritdoc />
        public Tracer(Options options) : this(new AsyncLocalScopeManager(), Propagators.TextMap, options,
            new LightStepSpanRecorder(), null)
        {
        }

        /// <inheritdoc />
        public Tracer(Options options, ISpanRecorder spanRecorder) : this(new AsyncLocalScopeManager(),
            Propagators.TextMap, options, spanRecorder, null)
        {
        }

        /// <inheritdoc />
        public Tracer(Options options, IScopeManager scopeManager) : this(scopeManager, Propagators.TextMap, options,
            new LightStepSpanRecorder(), null)
        {
        }

        /// <inheritdoc />
        public Tracer(Options options, ISpanRecorder spanRecorder, IPropagator propagator) : this(
            new AsyncLocalScopeManager(), propagator, options, spanRecorder, null)
        {
        }
        /// <inheritdoc />
        public Tracer(Options options, ISpanRecorder spanRecorder, ILightStepHttpClient client) : this(
            new AsyncLocalScopeManager(), Propagators.TextMap, options, spanRecorder, client)
        {
        }
        /// <inheritdoc />
        public Tracer(Options options, IPropagator propagator, ILightStepHttpClient client) : this(
            new AsyncLocalScopeManager(), propagator, options, new LightStepSpanRecorder(), client)
        {
        }
        /// <inheritdoc />
        public Tracer(Options options, ISpanRecorder spanRecorder, IReportTranslator translator,
            ILightStepHttpClient client) : this(
            new AsyncLocalScopeManager(), Propagators.TextMap, options, spanRecorder, client, translator)
        {
        }
        /// <inheritdoc />
        private Tracer(IScopeManager scopeManager, IPropagator propagator, Options options, ISpanRecorder spanRecorder, ILightStepHttpClient client, IReportTranslator translator = null)
        {
            ScopeManager = scopeManager;
            _spanRecorder = spanRecorder;
            _propagator = propagator;
            _options = options;
            _logger.Debug(
                $"Creating new tracer with GUID {_options.TracerGuid}. Project Access Token: {_options.AccessToken}, Report Period: {_options.ReportPeriod}, Report Timeout: {_options.ReportTimeout}.");
            var protocol = _options.Satellite.UsePlaintext ? "http" : "https";
            var url =
                $"{protocol}://{_options.Satellite.SatelliteHost}:{_options.Satellite.SatellitePort}/{LightStepConstants.SatelliteReportPath}";
            _httpClient = client ?? new LightStepHttpClient(url, _options);
            _translator = translator ?? new ReportTranslator(_options);
            _logger.Debug($"Tracer is reporting to {url}.");
            _reportLoop = new Timer(async e => await Flush().ConfigureAwait(false), null, TimeSpan.Zero, _options.ReportPeriod);
            _firstReportHasRun = false;
        }

        /// <inheritdoc />
        public IScopeManager ScopeManager { get; }

        /// <inheritdoc />
        public ISpan ActiveSpan => ScopeManager?.Active?.Span;

        /// <inheritdoc />
        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(this, operationName);
        }

        /// <inheritdoc />
        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            _propagator.Inject((SpanContext) spanContext, format, carrier);
            if (_options.EnableMetaEventLogging) {
                this.BuildSpan(LightStepConstants.MetaEvent.InjectOperation)
                    .IgnoreActiveSpan()
                    .WithTag(LightStepConstants.MetaEvent.MetaEventKey, true)
                    .WithTag(LightStepConstants.MetaEvent.SpanIdKey, spanContext.SpanId)
                    .WithTag(LightStepConstants.MetaEvent.TraceIdKey, spanContext.TraceId)
                    .WithTag(LightStepConstants.MetaEvent.PropagationFormatKey, format.GetType().ToString())
                    .Start()
                    .Finish();
            }
        }

        /// <inheritdoc />
        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            var ctx = _propagator.Extract(format, carrier);
            if (_options.EnableMetaEventLogging) {
                this.BuildSpan(LightStepConstants.MetaEvent.ExtractOperation)
                    .IgnoreActiveSpan()
                    .WithTag(LightStepConstants.MetaEvent.MetaEventKey, true)
                    .WithTag(LightStepConstants.MetaEvent.SpanIdKey, ctx.SpanId)
                    .WithTag(LightStepConstants.MetaEvent.TraceIdKey, ctx.TraceId)
                    .WithTag(LightStepConstants.MetaEvent.PropagationFormatKey, format.GetType().ToString())
                    .Start()
                    .Finish();
            }
            return ctx;
        }

        /// <summary>
        ///     Transmits the current contents of the span buffer to the LightStep Satellite.
        ///     Note that this creates a copy of the current spans and clears the span buffer!
        /// </summary>
        public async Task Flush()
        {
            if (_options.Run)
            {
                if (_options.EnableMetaEventLogging && _firstReportHasRun == false)
                {
                    BuildSpan(LightStepConstants.MetaEvent.TracerCreateOperation)
                        .IgnoreActiveSpan()
                        .WithTag(LightStepConstants.MetaEvent.MetaEventKey, true)
                        .WithTag(LightStepConstants.MetaEvent.TracerGuidKey, _options.TracerGuid)
                        .Start()
                        .Finish();
                    _firstReportHasRun = true;
                }
                // save current spans and clear the buffer
                ISpanRecorder currentBuffer;
                lock (_lock)
                {
                    currentBuffer = _spanRecorder.GetSpanBuffer();
                    _spanRecorder = new LightStepSpanRecorder();
                    _logger.Trace($"{currentBuffer.GetSpanCount()} spans in buffer.");
                }
                
                /**
                 * there are two ways spans can be dropped:
                 * 1. the buffer drops a span because it's too large, malformed, etc.
                 * 2. the report failed to be sent to the satellite.
                 * since flush is async and there can potentially be any number of buffers in flight to the satellite,
                 * we need to set the current drop count on the tracer to be the amount of dropped spans from the buffer
                 * plus the existing dropped spans, then mutate the current buffer to this new total value.
                 */
                currentDroppedSpanCount += currentBuffer.DroppedSpanCount;
                currentBuffer.DroppedSpanCount = currentDroppedSpanCount;
                
                try
                {
                    // since translate can throw exceptions, place it in the try and drop spans as appropriate
                    var data = _translator.Translate(currentBuffer);
                    var resp = await _httpClient.SendReport(data).ConfigureAwait(false);
                    
                    if (resp.Errors.Count > 0)
                    {
                        _logger.Warn($"Errors in report: {resp.Errors}");
                    }
                    // if the satellite is in developer mode, set the tracer to development mode as well
                    // don't re-enable if it's already enabled though
                    // TODO: iterate through all commands to find devmode flag
                    if (resp.Commands.Count > 0 && resp.Commands[0].DevMode && _options.EnableMetaEventLogging == false)
                    {
                        _logger.Info("Enabling meta event logging");
                        _options.EnableMetaEventLogging = true;
                    }

                    lock (_lock)
                    {
                        _logger.Trace($"Resetting tracer dropped span count as the last report was successful.");
                        currentDroppedSpanCount = 0;  
                    }
                    
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is OperationCanceledException || ex is Exception)
                {
                    lock (_lock)
                    {
                        _logger.Warn($"Adding {currentBuffer.GetSpanCount()} spans to dropped span count (current total: {currentDroppedSpanCount})");
                        currentDroppedSpanCount += currentBuffer.GetSpanCount();
                        if (this._options.ExceptionHandlerRegistered)
                        {
                            this._options.ExceptionHandler.Invoke(ex);
                        }
                    }
                }
                
                
            }
        }

        internal void AppendFinishedSpan(SpanData span)
        {
            lock (_lock)
            {
                if (_spanRecorder.GetSpanCount() < _options.ReportMaxSpans )
                {
                    _spanRecorder.RecordSpan(span);
                }
                else
                {
                    _spanRecorder.RecordDroppedSpans(1);
                    _logger.Warn($"Dropping span due to too many spans in buffer.");
                }
            }
        }
    }
}