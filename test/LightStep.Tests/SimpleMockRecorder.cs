using System;
using System.Collections.Generic;

namespace LightStep.Tests
{
    public class SimpleMockRecorder : ISpanRecorder
    {
        private List<SpanData> Spans { get; } = new List<SpanData>();

        public DateTime ReportStartTime { get; }
        public DateTime ReportEndTime { get; set; }
        public int DroppedSpanCount { get; set; }

        public void RecordSpan(SpanData span)
        {
            Spans.Add(span);
        }

        ISpanRecorder ISpanRecorder.GetSpanBuffer()
        {
            throw new System.NotImplementedException();
        }

        public ISpanRecorder GetSpanBuffer()
        {
            return this;
        }

        public void ClearSpanBuffer()
        {
            Spans.Clear();
        }

        public void RecordDroppedSpans(int count)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<SpanData> GetSpans()
        {
            return Spans;
        }
    }
}