using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing;

namespace LightStep.Tracer
{
    public class SpanContextFactory : ISpanContextFactory
    {
        private const short TraceEverything = 100;

        private readonly short _samplingRate;
        private readonly Random _samplingRandom = new Random();

        public SpanContextFactory()
            : this (TraceEverything)
        {
        }

        public SpanContextFactory(short sampleRate)
        {
            if (sampleRate < 0 || sampleRate > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, "Value must be between 0 and 100");
            }

            _samplingRate = sampleRate;
        }

        private bool ShouldSample()
        {
            if (_samplingRate == 100)
            {
                return true;
            }
            
            var random = _samplingRandom.Next(0, 100);
            return random % _samplingRate == 0;
        }

        public SpanContext CreateSpanContext(IList<Tuple<string, ISpanContext>> references)
        {
            string traceId = references?.FirstOrDefault()?.Item2?.TypedContext()?.TraceId.ToString() ?? Guid.NewGuid().ToString();
            var spanId = Guid.NewGuid().ToString();
            var shouldSample = ShouldSample();

            var baggage = new Baggage();
            if (references != null)
            {
                foreach (var reference in references)
                {
                    var typedContext = (SpanContext)reference.Item2;
                    baggage.Merge(typedContext.GetBaggageItems());
                }
            }

            return new SpanContext(traceId, spanId, shouldSample, baggage);
        }
    }
}