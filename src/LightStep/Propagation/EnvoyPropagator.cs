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
        private const string HeaderName = "X-OT-Span-Context";
        
        /// <inheritdoc />
        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {/*
            _logger.Trace($"Injecting {context} of {format.GetType()} to {carrier.GetType()}");
            if (carrier is Stream)
            {
                var ctx = new BinaryCarrier
                {
                    BasicCtx =
                    {
                        SpanId = Convert.ToUInt64(context.SpanId), TraceId = Convert.ToUInt64(context.TraceId)
                    }
                };
                foreach (var item in context.GetBaggageItems()) ctx.BasicCtx.BaggageItems.Add(item.Key, item.Value);
                carrier = ctx.ToByteArray();
            }*/
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            if (carrier is Stream)
            {
                // we need to coerce the carrier into a stream as it can't be cast through the pattern match (but it can only ever be a stream)
                var h = (object) carrier;
                var st = (Stream) h;
                var ctx = BinaryCarrier.Parser.ParseFrom(st);
                var traceId = ctx.BasicCtx.TraceId;
                var spanId = ctx.BasicCtx.SpanId;
                var baggage = new Baggage();
                foreach (var item in ctx.BasicCtx.BaggageItems)
                {
                    baggage.Set(item.Key, item.Value);
                }
                return new SpanContext(traceId.ToString(), spanId.ToString(), baggage);
            }

            return null;
        }
    }
}