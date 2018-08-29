using System.Collections.Generic;
using OpenTracing;

namespace LightStep
{
    public class SpanContext : ISpanContext
    {
        private readonly Baggage _baggage = new Baggage();

        public string TraceId { get; }
        public string SpanId { get; }
        public string ParentSpanId { get; }

        public SpanContext(string traceId, string spanId, Baggage baggage = null, string parentId = null)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentSpanId = parentId;
            _baggage.Merge(baggage);
        }

        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems()
        {
            return _baggage.GetAll();
        }

        public string GetBaggageItem(string key)
        {
            return _baggage.Get(key);
        }

        public ISpanContext SetBaggageItem(string key, string value)
        {
            _baggage.Set(key, value);
            return this;
        }
    }
}
