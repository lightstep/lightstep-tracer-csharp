namespace LightStep.Tracer
{
    public interface ISpanRecorder
    {
        void RecordSpan(SpanData span);
    }
}