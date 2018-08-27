using System.Collections.Generic;

namespace LightStep
{
    public interface ISpanRecorder
    {
        void RecordSpan(SpanData span);
        IEnumerable<SpanData> GetSpanBuffer();
        void ClearSpanBuffer();
    }
}