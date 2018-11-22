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
            _logger.Trace($"Waiting for lock in SpanRecorder.");
            lock (Spans)
            {
                _logger.Trace($"Lock freed, adding new span: {span}");
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
            _logger.Trace("Waiting for lock in SpanRecorder.");
            lock (Spans)
            {
                _logger.Trace("Lock freed, clearing span buffer.");
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
    }
}