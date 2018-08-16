using System;
using System.Collections.Generic;
using LightStep.Tracer.Propagation;
using OpenTracing;
using OpenTracing.Propagation;

namespace LightStep.Tracer
{
    public class Tracer : ITracer
    {
        private readonly ISpanContextFactory _spanContextFactory;
        private readonly ISpanRecorder _spanRecorder;

        private readonly TextMapCarrierHandler _textMapCarrierHandler = new TextMapCarrierHandler();

        public Tracer(
            ISpanContextFactory spanContextFactory,
            ISpanRecorder spanRecorder)
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

            var span = new Span(_spanRecorder, spanContext, operationName, startTimestamp ?? DateTimeOffset.UtcNow, tags);

            return span;
        }

        public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
        {
            // TODO add other formats (and maybe don't use if/else :D )

            var typedContext = (SpanContext)spanContext;

            if (format.Equals(BuiltinFormats.TextMap))
            {
                _textMapCarrierHandler.MapContextToCarrier(typedContext, (ITextMap) carrier);
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
                return _textMapCarrierHandler.MapCarrierToContext((ITextMap) carrier);
            }
            
            throw new FormatException($"The format '{format}' is not supported.");
        }
    }
}