using System;
using System.Collections.Generic;
using System.Linq;
using LightStep.Logging;

namespace LightStep
{
    /// <inheritdoc />
    public sealed class LightStepSpanRecorder : ISpanRecorder
    {
        private List<SpanData> Spans { get; } = new List<SpanData>();
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();
        
        /// <inheritdoc />
        public DateTime ReportStartTime { get; } = DateTime.Now;

        /// <inheritdoc />
        public DateTime ReportEndTime { get; set; }

        /// <inheritdoc />
        public int DroppedSpanCount { get; set; }
        
        /// <inheritdoc />
        public void RecordSpan(SpanData span)
        {
            lock (Spans)
            {
                Spans.Add(span);    
            }
        }

        /// <inheritdoc />
        public ISpanRecorder GetSpanBuffer()
        {
            lock (Spans)
            {
                ReportEndTime = DateTime.Now;
                return this;
            }
        }

        /// <inheritdoc />
        public void ClearSpanBuffer()
        {
            lock (Spans)
            {
                Spans.Clear();    
            }
        }

        /// <inheritdoc />
        public void RecordDroppedSpans(int count)
        {
            DroppedSpanCount += count;
        }

        /// <inheritdoc />
        public IEnumerable<SpanData> GetSpans()
        {
            lock (Spans)
            {
                return Spans.ToList();
            }
        }

        public int GetSpanCount()
        {
            return Spans.Count;
        }
    }
}