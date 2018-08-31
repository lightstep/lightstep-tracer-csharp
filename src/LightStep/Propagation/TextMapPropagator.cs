using System;
using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    /// <inheritdoc />
    public class TextMapPropagator : IPropagator
    {
        /// <inheritdoc />
        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            if (carrier is ITextMap text)
            {
                foreach (var entry in context.GetBaggageItems()) text.Set(Keys.BaggagePrefix + entry.Key, entry.Value);

                text.Set(Keys.SpanId, context.SpanId);
                text.Set(Keys.TraceId, context.TraceId);
            }
            else
            {
                throw new InvalidOperationException($"Unknown carrier {carrier.GetType()}");
            }
        }

        /// <inheritdoc />
        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            string traceId = null;
            string spanId = null;
            var baggage = new Baggage();
            if (carrier is ITextMap text)
                foreach (var entry in text)
                    if (Keys.TraceId.Equals(entry.Key))
                    {
                        traceId = entry.Value;
                    }
                    else if (Keys.SpanId.Equals(entry.Key))
                    {
                        spanId = entry.Value;
                    }
                    else if (entry.Key.StartsWith(Keys.BaggagePrefix))
                    {
                        var key = entry.Key.Substring(Keys.BaggagePrefix.Length);
                        baggage.Set(key, entry.Value);
                    }
            else
                throw new InvalidOperationException($"Unknown carrier {carrier.GetType()}");

            if (!string.IsNullOrEmpty(traceId) && !string.IsNullOrEmpty(spanId))
                return new SpanContext(traceId, spanId, baggage);

            return null;
        }
    }
}