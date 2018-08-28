using System;
using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    public class TextMapPropagator : IPropagator
    {
        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            if (carrier is ITextMap text)
            {
                foreach (var entry in context.GetBaggageItems())
                {
                    text.Set(BaggageKeys.BaggagePrefix + entry.Key, entry.Value);
                }
                
                text.Set(BaggageKeys.SpanId, context.SpanId);
                text.Set(BaggageKeys.TraceId, context.TraceId);
            }
            else
            {
                throw new InvalidOperationException($"Unknown carrier {carrier.GetType()}");
            }
        }

        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            string traceId = null;
            string spanId = null;
            var baggage = new Baggage();
            if (carrier is ITextMap text)
            {
                foreach (var entry in text)
                {
                    if (BaggageKeys.TraceId.Equals(entry.Key))
                    {
                        traceId = entry.Value;
                    }
                    else if (BaggageKeys.SpanId.Equals(entry.Key))
                    {
                        spanId = entry.Value;
                    }
                    else if (entry.Key.StartsWith(BaggageKeys.BaggagePrefix))
                    {
                        var key = entry.Key.Substring(BaggageKeys.BaggagePrefix.Length);
                        baggage.Set(key, entry.Value);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException($"Unknown carrier {carrier.GetType()}");
            }

            if (!string.IsNullOrEmpty(traceId) && !string.IsNullOrEmpty(spanId))
            {
                return new SpanContext(traceId, spanId, baggage);
            }

            return null;
        }
    }
}