using System.Collections.Generic;
using System.Linq;
using OpenTracing;

namespace LightStep
{
    public sealed class LightStepSpanRecorder : ISpanRecorder
    {
        private List<SpanData> Spans { get; }= new List<SpanData>();
        
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
