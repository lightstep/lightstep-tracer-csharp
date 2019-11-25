using System;
using System.IO;
using Google.Protobuf;
using LightStep.Logging;
using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    public class EnvoyPropagator : IPropagator
    {
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        /// <inheritdoc />
        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            _logger.Trace($"Injecting {context} of {format.GetType()} to {carrier.GetType()}");
            if (carrier is IBinary s)
            {
                var ctx = new BinaryCarrier
                {
                    BasicCtx = new BasicTracerCarrier
                    {
                        SpanId = context.SpanIdValue,
                        TraceId = context.TraceIdValue,
                        Sampled = true
                    }
                };
                foreach (var item in context.GetBaggageItems()) ctx.BasicCtx.BaggageItems.Add(item.Key, item.Value);
                var ctxArray = ctx.ToByteArray();
                var ctxStream = new MemoryStream(ctxArray);
                s.Set(ctxStream);
            }
        }

        /// <inheritdoc />
        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            if (carrier is IBinary s)
            {
                var ctx = BinaryCarrier.Parser.ParseFrom(s.Get());
                var traceId = ctx.BasicCtx.TraceId;
                var spanId = ctx.BasicCtx.SpanId;
                var baggage = new Baggage();

                foreach (var item in ctx.BasicCtx.BaggageItems)
                {
                    baggage.Set(item.Key, item.Value);
                }

                return new SpanContext(traceId, spanId, baggage);
            }

            return null;
        }
    }
}