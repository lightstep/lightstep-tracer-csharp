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

                text.Set(Keys.SpanId, context.SpanId);
                text.Set(Keys.TraceId, context.TraceId);
                text.Set(Keys.Sampled, "true");
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
            return Extract(format, carrier, StringComparison.Ordinal);
        }

        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier, StringComparison comparison)
        {
            _logger.Trace($"Extracting {format.GetType()} from {carrier.GetType()}");
            if (carrier is ITextMap text)
            {
                ulong? traceId = null;
                ulong? spanId = null;
                var baggage = new Baggage();

                foreach (var entry in text)
                {
                    if (Keys.TraceId.Equals(entry.Key, comparison))
                    {
                        traceId = Convert.ToUInt64(entry.Value, 16);
                    }
                    else if (Keys.SpanId.Equals(entry.Key, comparison))
                    {
                        spanId = Convert.ToUInt64(entry.Value, 16);
                    }
                    else if (entry.Key.StartsWith(Keys.BaggagePrefix, comparison))
                    {
                        var key = entry.Key.Substring(Keys.BaggagePrefix.Length);
                        baggage.Set(key, entry.Value);
                    }
                }

                if (traceId.HasValue && spanId.HasValue)
                {
                    _logger.Trace($"Existing trace/spanID found, returning SpanContext.");
                    return new SpanContext(traceId.Value, spanId.Value);
                }
            }

            return null;
        }
    }
}