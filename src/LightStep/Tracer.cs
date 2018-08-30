using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightStep.Collector;
using LightStep.Propagation;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;

namespace LightStep
{
    public sealed class Tracer : ITracer
    {
        private readonly object _lock = new object();

        private readonly ISpanRecorder _spanRecorder;
        private readonly IPropagator _propagator;
        private readonly IScopeManager _scopeManager;
        private readonly Options _options;

        public IScopeManager ScopeManager => _scopeManager;

        public ISpan ActiveSpan => _scopeManager?.Active?.Span;
        
        public Tracer(Options options) : this(new AsyncLocalScopeManager(), Propagators.TextMap, options, new LightStepSpanRecorder())
        {
        }

        public Tracer(Options options, ISpanRecorder spanRecorder) : this(new AsyncLocalScopeManager(),
            Propagators.TextMap, options, spanRecorder)
        {
        }
        
        public Tracer(Options options, IScopeManager scopeManager) : this(scopeManager, Propagators.TextMap, options, new LightStepSpanRecorder())
        {
        }
        
        private Tracer(IScopeManager scopeManager, IPropagator propagator, Options options, ISpanRecorder spanRecorder)
        {
            _scopeManager = scopeManager;
            _spanRecorder = spanRecorder;
            _propagator = propagator;
            _options = options;
            // assignment to a variable here is to suppress warnings that we're not awaiting an async method
            var reportLoop = DoReportLoop(_options.ReportPeriod);
        }
        
        public void Flush()
        {
            // save current spans and clear the buffer
            // TODO: add retry logic so as to not drop spans on unreachable satellite
            var currentSpans = _spanRecorder.GetSpanBuffer().ToList();
            _spanRecorder.ClearSpanBuffer();
            var url =
                $"http://{_options.Satellite.SatelliteHost}:{_options.Satellite.SatellitePort}/{LightStepConstants.SatelliteReportPath}";
            using (var client = new LightStepHttpClient(url, _options))
            {
                var data = client.Translate(currentSpans);
                var resp = client.SendReport(data);
                if (resp.Errors.Count > 0)
                {
                    Console.WriteLine($"Errors sending report to LightStep: {resp.Errors}");
                }
            }     
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(this, operationName);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
           _propagator.Inject((SpanContext)spanContext, format, carrier);
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            return _propagator.Extract(format, carrier);
        }

        internal void AppendFinishedSpan(SpanData span)
        {
            lock (_lock)
            {
                _spanRecorder.RecordSpan(span);
            }
        }

        private async Task DoReportLoop(TimeSpan reportingPeriod)
        {
            while (true)
            {
                Flush();   
                await Task.Delay(reportingPeriod);
            }
        }
    }
}
