using System.Collections.Generic;
using OpenTracing;

namespace LightStep
{
    /// <inheritdoc />
    public class SpanContext : ISpanContext
    {
        private readonly Baggage _baggage = new Baggage();

        /// <inheritdoc />
        public SpanContext(ulong traceId, ulong spanId, Baggage baggage = null, ulong parentId = 0L, string originalTraceId = null)
        {
            TraceIdValue = traceId;
            OriginalTraceId = originalTraceId ?? TraceId;
            SpanIdValue = spanId;
            ParentSpanIdValue = parentId;
            _baggage.Merge(baggage);
        }

        /// <summary>
        ///     The parent span ID, if any.
        /// </summary>
        public ulong ParentSpanIdValue { get; }

        public string ParentSpanId => ParentSpanIdValue.ToString("x");

        /// <summary>
        ///     The trace ID represetned as a ulong (UInt64).
        /// </summary>
        public ulong TraceIdValue { get; }

        /// <inheritdoc />
        public string TraceId => TraceIdValue.ToString("x");

        /// <summary>
        ///     The original trace ID used to create this context.
        ///     <para>This may be a 64 or 128 bit hex value.</para>
        /// </summary>
        public string OriginalTraceId { get; }

        /// <summary>
        ///     The Span ID represented as a ulong (UInt64).
        /// </summary>
        public ulong SpanIdValue { get; }

        /// <inheritdoc />
        public string SpanId => SpanIdValue.ToString("x");

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return _baggage.GetAll();
        }

        /// <summary>
        ///     Get a single item from the baggage.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetBaggageItem(string key)
        {
            return _baggage.Get(key);
        }

        /// <summary>
        ///     Set an item on the baggage.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ISpanContext SetBaggageItem(string key, string value)
        {
            _baggage.Set(key, value);
            return this;
        }

        public override string ToString()
        {
            return $"[traceId: {TraceId}, spanId: {SpanId}, parentId: {ParentSpanId ?? "none"}";
        }
    }
}