using System.Collections.Generic;

namespace LightStep.Tracer.Tests
{
    public class SimpleMockRecorder : ISpanRecorder
    {
        public List<SpanData> Spans { get; } = new List<SpanData>();

        public void RecordSpan(SpanData span)
        {
            Spans.Add(span);
        }
    }
}