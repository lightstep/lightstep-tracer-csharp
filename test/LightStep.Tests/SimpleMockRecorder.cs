using System;
using System.Collections.Generic;
using System.Linq;

namespace LightStep.Tests
{
    public class SimpleMockRecorder : ISpanRecorder
    {
        private List<SpanData> Spans { get; } = new List<SpanData>();

        public DateTime ReportStartTime { get; } = DateTime.Now;
        public DateTime ReportEndTime { get; set; }
        public int DroppedSpanCount { get; set; }

        public void RecordSpan(SpanData span)
        {
            Spans.Add(span);
        }

        public ISpanRecorder GetSpanBuffer()
        {
            ReportEndTime = DateTime.Now;
            return this;
        }

        public void ClearSpanBuffer()
        {
            Spans.Clear();
        }

        public void RecordDroppedSpans(int count)
        {
            DroppedSpanCount += count;
        }

        public IEnumerable<SpanData> GetSpans()
        {
            return Spans;
        }

        public int GetSpanCount()
        {
            return Spans.Count();
        }
    }
}