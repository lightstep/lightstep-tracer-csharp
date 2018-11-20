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
        private readonly Options _options;
        private readonly IPropagator _propagator;
        private readonly LightStepHttpClient _httpClient;
        private ISpanRecorder _spanRecorder;
        private readonly Timer _reportLoop;
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

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
        }

        /// <inheritdoc />
        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            return _propagator.Extract(format, carrier);
        }

        /// <summary>
        ///     Transmits the current contents of the span buffer to the LightStep Satellite.
        ///     Note that this creates a copy of the current spans and clears the span buffer!
        /// </summary>
        public async void Flush()
        {
            if (_options.Run)
            {
                // save current spans and clear the buffer
                ISpanRecorder currentBuffer;
                lock (_lock)
                {
                    currentBuffer = _spanRecorder.GetSpanBuffer();
                    _spanRecorder = new LightStepSpanRecorder();
                    _logger.Debug($"{currentBuffer.GetSpans().Count()} spans in buffer.");
                }
                
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
                        _spanRecorder.DroppedSpanCount = 0;    
                    }
                    
                }
                catch (HttpRequestException)
                {
                    lock (_lock)
                    {
                        _spanRecorder.RecordDroppedSpans(currentBuffer.GetSpans().Count() + currentBuffer.DroppedSpanCount);
                    }
                }
                
                
            }
        }

        internal void AppendFinishedSpan(SpanData span)
        {
            lock (_lock)
            {
                _spanRecorder.RecordSpan(span);
            }
        }
    }
}