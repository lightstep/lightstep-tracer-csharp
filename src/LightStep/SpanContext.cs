using System.Collections.Generic;
using OpenTracing;

namespace LightStep
{
    /// <inheritdoc />
    public class SpanContext : ISpanContext
    {
        private readonly Baggage _baggage = new Baggage();

        /// <inheritdoc />
        public SpanContext(string traceId, string spanId, Baggage baggage = null, string parentId = null)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentSpanId = parentId;
            _baggage.Merge(baggage);
        }

        /// <summary>
        ///     The parent span ID, if any.
        /// </summary>
        public string ParentSpanId { get; }

        /// <inheritdoc />
        public string TraceId { get; }

        /// <inheritdoc />
        public string SpanId { get; }

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
    }
}