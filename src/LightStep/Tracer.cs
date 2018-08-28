using System;
using System.Collections.Generic;
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

        private readonly ISpanRecorder _spanRecorder = new LightStepSpanRecorder();
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
        
        public Tracer(IScopeManager scopeManager, IPropagator propagator, Options options, ISpanRecorder spanRecorder)
        {
            _scopeManager = scopeManager;
            _spanRecorder = spanRecorder;
            _propagator = propagator;
            _options = options;
        }

        public void Flush()
        {
            lock (_lock)
            {
                var url =
                    $"http://{_options.Satellite.SatelliteHost}:{_options.Satellite.SatellitePort}/{LightStepConstants.SatelliteReportPath}";
                using (var client = new LightStepHttpClient(url, _options))
                {
                    var data = client.Translate(_spanRecorder.GetSpanBuffer());
                    var resp = client.SendReport(data);
                    if (resp.Errors.Count > 0)
                    {
                        Console.WriteLine($"Errors sending report to LightStep: {resp.Errors}");
                    }
                    _spanRecorder.ClearSpanBuffer();
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
    }
}