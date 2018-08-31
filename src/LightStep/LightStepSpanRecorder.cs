using System.Collections.Generic;
using System.Linq;
using OpenTracing;

namespace LightStep
{
    /// <inheritdoc />
    public sealed class LightStepSpanRecorder : ISpanRecorder
    {
        private List<SpanData> Spans { get; }= new List<SpanData>();

        /// <inheritdoc />
        public void RecordSpan(SpanData span)
        {
            Spans.Add(span);
        }

        /// <inheritdoc />
        public IEnumerable<SpanData> GetSpanBuffer()
        {
            return Spans;
        }

        /// <inheritdoc />
        public void ClearSpanBuffer()
        {
            Spans.Clear();
        }

        
    }
}
