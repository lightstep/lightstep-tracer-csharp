using System.Collections.Generic;
using OpenTracing;

namespace LightStep.Tracer
{
    public class SpanContext : ISpanContext
    {
        private readonly Baggage _baggage = new Baggage();

        public string TraceId { get; }
        public string SpanId { get; }
        public bool Sampled { get; }

        public SpanContext(string traceId, string spanId, bool sampled, Baggage baggage = null)
        {
            TraceId = traceId;
            SpanId = spanId;
            Sampled = sampled;
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