using System;
using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    public class ConsolePropagator : IPropagator
    {
        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            Console.WriteLine($"Inject({context}, {format}, {carrier})");
        }

        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            Console.WriteLine($"Extract({format}, {carrier}");
            return null;
        }
    }
}
