using System;
using System.Linq;
using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    public class TextMapCarrierHandler
    {
        public static void MapContextToCarrier(SpanContext context, ITextMap carrier)
        {
            carrier.Set(BaggageKeys.TraceId, context.TraceId);
            carrier.Set(BaggageKeys.SpanId, context.SpanId);

            foreach (var kvp in context.GetBaggageItems())
            {
                carrier.Set(BaggageKeys.BaggagePrefix + kvp.Key, kvp.Value);
            }
        }

        public static SpanContext MapCarrierToContext(ITextMap carrier)
        {
            // we can't create a reference without a trace-id
            // TODO: should this throw an exception rather than returning a null ctx?
            var traceId = carrier.FirstOrDefault(x => x.Key.Equals(BaggageKeys.TraceId)).Value;
            if (string.IsNullOrWhiteSpace(traceId))
            {
                return null;
            }
            
            // something is seriously wrong if we have a trace-id but no span-id
            var spanId = carrier.FirstOrDefault(x => x.Key.Equals(BaggageKeys.SpanId)).Value;
            if (string.IsNullOrWhiteSpace(spanId))
            {
                return null;
            }
            
            var baggage = new Baggage();

            foreach (var kvp in carrier)
            {
                if (kvp.Key.StartsWith(BaggageKeys.BaggagePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    var key = kvp.Key.Substring(BaggageKeys.BaggagePrefix.Length);
                    baggage.Set(key, kvp.Value);
                }
            }

            return new SpanContext(traceId, spanId, baggage);
        }
    }
}