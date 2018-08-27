using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing;

namespace LightStep
{
    public class SpanContextFactory : ISpanContextFactory
    {
        private const short TraceEverything = 100;

        public SpanContextFactory()
            : this (TraceEverything)
        {
        }

        public SpanContextFactory(short sampleRate)
        {
        }

        public SpanContext CreateSpanContext(IList<Tuple<string, ISpanContext>> references)
        {
            string traceId = references?.FirstOrDefault()?.Item2?.TypedContext()?.TraceId ?? new Random().NextUInt64().ToString();
            var spanId = new Random().NextUInt64().ToString();
            
            var baggage = new Baggage();
            if (references != null)
            {
                foreach (var reference in references)
                {
                    var typedContext = (SpanContext)reference.Item2;
                    baggage.Merge(typedContext.GetBaggageItems());
                }
                return new SpanContext(traceId, spanId, baggage, references.First().Item2.SpanId);
            }
            return new SpanContext(traceId, spanId, baggage);
        }
    }
}