using System;
using LightStep.Logging;
using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    /// <inheritdoc />
    public class TextMapPropagator : IPropagator
    {
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();
        /// <inheritdoc />
        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            _logger.Trace($"Injecting {context} of {format.GetType()} to {carrier.GetType()}");
            if (carrier is ITextMap text)
            {
                foreach (var entry in context.GetBaggageItems()) text.Set(Keys.BaggagePrefix + entry.Key, entry.Value);

                text.Set(Keys.SpanId, Convert.ToUInt64(context.SpanId).ToString("X"));
                text.Set(Keys.TraceId, Convert.ToUInt64(context.TraceId).ToString("X"));
                text.Set(Keys.SampledId, "true");
            }
            else
            {
                _logger.Warn($"Unknown carrier during inject.");
                throw new InvalidOperationException($"Unknown carrier {carrier.GetType()}");
            }
        }

        /// <inheritdoc />
        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            _logger.Trace($"Extracting {format.GetType()} from {carrier.GetType()}");
            string traceId = null;
            string spanId = null;
            var baggage = new Baggage();
            if (carrier is ITextMap text)
                foreach (var entry in text)
                    if (Keys.TraceId.Equals(entry.Key))
                    {
                        traceId = Convert.ToUInt64(entry.Value, 16).ToString();
                    }
                    else if (Keys.SpanId.Equals(entry.Key))
                    {
                        spanId = Convert.ToUInt64(entry.Value, 16).ToString();
                    }
                    else if (entry.Key.StartsWith(Keys.BaggagePrefix))
                    {
                        var key = entry.Key.Substring(Keys.BaggagePrefix.Length);
                        baggage.Set(key, entry.Value);
                    }

            if (!string.IsNullOrEmpty(traceId) && !string.IsNullOrEmpty(spanId))
            {
                _logger.Trace($"Existing trace/spanID found, returning SpanContext.");
                return new SpanContext(traceId, spanId, baggage);
            }

            return null;
        }
    }
}