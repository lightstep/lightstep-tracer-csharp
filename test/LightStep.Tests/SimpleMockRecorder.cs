using System.Collections.Generic;

namespace LightStep.Tests
{
    public class SimpleMockRecorder : ISpanRecorder
    {
        private List<SpanData> Spans { get; } = new List<SpanData>();

        public void RecordSpan(SpanData span)
        {
            Spans.Add(span);
        }

        public IEnumerable<SpanData> GetSpanBuffer()
        {
            return Spans;
        }

        public void ClearSpanBuffer()
        {
            Spans.Clear();
        }
    }
}