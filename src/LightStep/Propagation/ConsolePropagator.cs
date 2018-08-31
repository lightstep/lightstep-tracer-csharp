using System;
using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    /// <inheritdoc />
    public class ConsolePropagator : IPropagator
    {
        /// <inheritdoc />
        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            Console.WriteLine($"Inject({context}, {format}, {carrier})");
        }

        /// <inheritdoc />
        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            Console.WriteLine($"Extract({format}, {carrier}");
            return null;
        }
    }
}
