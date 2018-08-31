using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    /// <summary>
    /// Implements the <see cref="Inject{TCarrier}"/> and <see cref="Extract{TCarrier}"/> methods for passing contexts
    /// between process boundaries.
    /// </summary>
    public interface IPropagator
    {
        /// <summary>
        /// Injects a <see cref="SpanContext"/> into a <paramref name="carrier"/>.
        /// </summary>
        /// <param name="context">The <see cref="SpanContext"/> instance to inject into the <paramref name="carrier"/></param>
        /// <param name="format">The <see cref="IFormat{TCarrier}"/> of the <paramref name="carrier"/></param>
        /// <param name="carrier">The carrier for the <see cref="SpanContext"/></param>
        /// <typeparam name="TCarrier">The <paramref name="carrier"/> type, which also parametrizes the <paramref name="format"/></typeparam>
        void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier);
        /// <summary>
        /// Extracts a <see cref="SpanContext"/> from a <paramref name="carrier"/>.
        /// </summary>
        /// <param name="format">The <see cref="IFormat{TCarrier}"/> or the <paramref name="carrier"/></param>
        /// <param name="carrier">The <see cref="IFormat{TCarrier}"/> of the <paramref name="carrier"/></param>
        /// <typeparam name="TCarrier">The <paramref name="carrier"/> type, which also parametrizes the <paramref name="format"/></typeparam>
        /// <returns>The <see cref="SpanContext"/> instance to create a span.</returns>
        SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier);
    }
}
