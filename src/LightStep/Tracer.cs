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
        public readonly Options _options;
        private readonly IPropagator _propagator;
        private readonly LightStepHttpClient _httpClient;
        private ISpanRecorder _spanRecorder;
        private readonly Timer _reportLoop;
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();
        private int currentDroppedSpanCount;
        private bool _firstReportHasRun;

        /// <inheritdoc />
        public Tracer(Options options) : this(new AsyncLocalScopeManager(), Propagators.TextMap, options,
            new LightStepSpanRecorder())
        {
        }

        /// <inheritdoc />
        public Tracer(Options options, ISpanRecorder spanRecorder) : this(new AsyncLocalScopeManager(),
            Propagators.TextMap, options, spanRecorder)
        {
        }

        /// <inheritdoc />
        public Tracer(Options options, IScopeManager scopeManager) : this(scopeManager, Propagators.TextMap, options,
            new LightStepSpanRecorder())
        {
        }

        /// <inheritdoc />
        public Tracer(Options options, ISpanRecorder spanRecorder, IPropagator propagator) : this(
            new AsyncLocalScopeManager(), propagator, options, spanRecorder)
        {
        }

        private Tracer(IScopeManager scopeManager, IPropagator propagator, Options options, ISpanRecorder spanRecorder)
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
            _httpClient = new LightStepHttpClient(url, _options);
            _logger.Debug($"Tracer is reporting to {url}.");          
            _reportLoop = new Timer(e => Flush(), null, TimeSpan.Zero, _options.ReportPeriod);
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
                this.BuildSpan("lightstep.inject_span")
                    .IgnoreActiveSpan()
                    .WithTag("lightstep.meta_event", true)
                    .WithTag("lightstep.span_id", spanContext.SpanId)
                    .WithTag("lightstep.trace_id", spanContext.TraceId)
                    .WithTag("lightstep.propagation_format", format.GetType().ToString())
                    .Start()
                    .Finish();
            }
        }

        /// <inheritdoc />
        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            var ctx = _propagator.Extract(format, carrier);
            if (_options.EnableMetaEventLogging) {
                this.BuildSpan("lightstep.extract_span")
                    .IgnoreActiveSpan()
                    .WithTag("lightstep.meta_event", true)
                    .WithTag("lightstep.span_id", ctx.SpanId)
                    .WithTag("lightstep.trace_id", ctx.TraceId)
                    .WithTag("lightstep.propagation_format", format.GetType().ToString())
                    .Start()
                    .Finish();
            }
            return ctx;
        }

        /// <summary>
        ///     Transmits the current contents of the span buffer to the LightStep Satellite.
        ///     Note that this creates a copy of the current spans and clears the span buffer!
        /// </summary>
        public async void Flush()
        {
            if (_options.Run)
            {
                if (_firstReportHasRun == false)
                {
                    BuildSpan("lightstep.tracer_create")
                        .IgnoreActiveSpan()
                        .WithTag("lightstep.meta_event", true)
                        .WithTag("lightstep.tracer_guid", _options.TracerGuid)
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
                    _logger.Trace($"{currentBuffer.GetSpans().Count()} spans in buffer.");
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
                var data = _httpClient.Translate(currentBuffer);

                try
                {
                    var resp = await _httpClient.SendReport(data);
                    if (resp.Errors.Count > 0)
                    {
                        _logger.Warn($"Errors in report: {resp.Errors}");
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
                        _logger.Warn($"Adding {currentBuffer.GetSpans().Count()} spans to dropped span count (current total: {currentDroppedSpanCount})");
                        currentDroppedSpanCount += currentBuffer.GetSpans().Count();
                    }
                }
                
                
            }
        }

        internal void AppendFinishedSpan(SpanData span)
        {
            lock (_lock)
            {
                if (_spanRecorder.GetSpans().Count() < _options.ReportMaxSpans )
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