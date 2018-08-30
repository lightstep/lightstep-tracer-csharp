using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    public interface IPropagator
    {
        void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier);
        SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier);
    }
}
