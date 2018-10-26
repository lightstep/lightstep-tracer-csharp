using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LightStep.Collector;
using LightStep.Propagation;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;

namespace LightStep
{
    /// <inheritdoc />
    public sealed class Tracer : ITracer
    {
        private readonly object _lock = new object();
        private readonly Options _options;
        private readonly IPropagator _propagator;
        private readonly LightStepHttpClient _httpClient;
        private readonly ISpanRecorder _spanRecorder;
        private readonly Timer _reportLoop;

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

            var protocol = _options.Satellite.UsePlaintext ? "http" : "https";
            // TODO: refactor to support changing proto
            var url =
                $"{protocol}://{_options.Satellite.SatelliteHost}:{_options.Satellite.SatellitePort}/{LightStepConstants.SatelliteReportPath}";
            _httpClient = new LightStepHttpClient(url, _options);

            // assignment to a variable here is to suppress warnings that we're not awaiting an async method
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
                // TODO: add retry logic so as to not drop spans on unreachable satellite
                List<SpanData> currentSpans = new List<SpanData>();
                lock (_lock)
                {
                    currentSpans = _spanRecorder.GetSpanBuffer();
                }
                var data = _httpClient.Translate(currentSpans);
                var resp = await _httpClient.SendReport(data);
                if (resp.Errors.Count > 0) Console.WriteLine($"Errors sending report to LightStep: {resp.Errors}");
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