using System;
using System.Collections.Generic;
using System.Linq;

namespace LightStep
{
    /// <inheritdoc />
    public sealed class LightStepSpanRecorder : ISpanRecorder
    {
        private List<SpanData> Spans { get; } = new List<SpanData>();

        /// <inheritdoc />
        public void RecordSpan(SpanData span)
        {
            lock (Spans)
            {
                Spans.Add(span);    
            }
            
        }

        /// <inheritdoc />
        public List<SpanData> GetSpanBuffer()
        {
            lock (Spans)
            {
                var currentSpans = Spans.ToList();
                Spans.Clear();
                return currentSpans;
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
    }
}