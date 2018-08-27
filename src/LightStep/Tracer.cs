using System;
using System.Collections.Generic;
using LightStep.Collector;
using LightStep.Propagation;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Util;

namespace LightStep
{
    public class Tracer : ITracer, IDisposable
    {
        private readonly ISpanContextFactory _spanContextFactory;
        private readonly ISpanRecorder _spanRecorder;

        private readonly TextMapCarrierHandler _textMapCarrierHandler = new TextMapCarrierHandler();
        private readonly Options _options;

        
        public Tracer(
            ISpanContextFactory spanContextFactory,
            ISpanRecorder spanRecorder,
            Options options)
        {
            if (spanContextFactory == null)
            {
                throw new ArgumentNullException(nameof(spanContextFactory));
            }

            if (spanRecorder == null)
            {
                throw new ArgumentNullException(nameof(spanRecorder));
            }

            _spanContextFactory = spanContextFactory;
            _spanRecorder = spanRecorder;
            _options = options;
            
        }

        public void Close()
        {
            Dispose();
        }

        public void Flush()
        {
            var url =
                $"http://{_options.Satellite.Item1}:{_options.Satellite.Item2}/{LightStepConstants.SatelliteReportPath}";
            using (var client = new LightStepHttpClient(url, _options))
            {
                var data = client.Translate(_spanRecorder.GetSpanBuffer());
                var resp = client.SendReport(data);
                Console.WriteLine($"resp: {resp.Commands} {resp.Errors}");
                _spanRecorder.ClearSpanBuffer();
            }
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(this, operationName);
        }

        public IScopeManager ScopeManager { get; }
        public ISpan ActiveSpan { get; }

        public ISpan StartSpan(
            string operationName,
            DateTimeOffset? startTimestamp,
            IList<Tuple<string, ISpanContext>> references,
            IDictionary<string, object> tags)
        {
            var spanContext = _spanContextFactory.CreateSpanContext(references);

            var span = new Span(_spanRecorder, spanContext, operationName, startTimestamp ?? DateTime.UtcNow, tags);

            return span;
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            // TODO add other formats (and maybe don't use if/else :D )

            var typedContext = (SpanContext)spanContext;
            
            if (format.Equals(BuiltinFormats.TextMap))
            {
                TextMapCarrierHandler.MapContextToCarrier(typedContext, (ITextMap) carrier);
            }
            else
            {
                throw new FormatException($"The format '{format}' is not supported.");
            }
        }

        public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            // TODO add other formats (and maybe don't use if/else :D )

            if (format.Equals(BuiltinFormats.TextMap))
            {
                return TextMapCarrierHandler.MapCarrierToContext((ITextMap) carrier);
            }
            
            throw new FormatException($"The format '{format}' is not supported.");
        }

        public void Dispose()
        {
            Flush();
        }
    }
}