using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    /** TODO: this is a blind copy of the Java client; once HTTP carrier encoding has been defined, update this as well
     * 
     */
    /// <inheritdoc />
    public class HttpHeadersPropagator : IPropagator
    {
        /// <inheritdoc />
        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            var textMapPropagator = new TextMapPropagator();
            textMapPropagator.Inject(context, format, carrier);
        }

        /// <inheritdoc />
        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            var textMapPropagator = new TextMapPropagator();
            return textMapPropagator.Extract(format, carrier);
        }
    }
}