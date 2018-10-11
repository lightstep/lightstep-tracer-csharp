using System;
using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    /// <inheritdoc />
    public class B3Propagator : IPropagator
    {
        private const string TraceIdName = "X-B3-TraceId";
        private const string SpanIdName = "X-B3-SpanId";
        private const string SampledName = "X-B3-Sampled";

        /// <inheritdoc />
        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            ulong traceId;
            ulong spanId;

            try
            {
                traceId = Convert.ToUInt64(context.TraceId);
            }
            catch (FormatException)
            {
                traceId = Convert.ToUInt64(context.TraceId, 16);
            }

            try
            {
                spanId = Convert.ToUInt64(context.SpanId);
            }
            catch (FormatException)
            {
                spanId = Convert.ToUInt64(context.SpanId, 16);
            }
            
            if (carrier is ITextMap text)
            {
                text.Set(TraceIdName, traceId.ToString("X"));
                text.Set(SpanIdName, spanId.ToString("X"));
                text.Set(SampledName, "true");
            }
        }

        /// <inheritdoc />
        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            string traceId = null;
            string spanId = null;

            if (carrier is ITextMap text)
                foreach (var entry in text)
                    if (TraceIdName.Equals(entry.Key))
                        traceId = entry.Value;
                    else if (SpanIdName.Equals(entry.Key)) spanId = entry.Value;

            if (!string.IsNullOrEmpty(traceId) && !string.IsNullOrEmpty(spanId))
                return new SpanContext(traceId, spanId);

            return null;
        }
    }
}