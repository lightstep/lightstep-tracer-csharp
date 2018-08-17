using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using LightStep.Collector;
using OpenTracing;

namespace LightStep.Collector
{
    public partial class SpanContext
    {
        public SpanContext MakeSpanContextFromOtSpanContext(LightStep.SpanContext ctx)
        {
            SpanId = Convert.ToUInt64(ctx.SpanId);
            TraceId = Convert.ToUInt64(ctx.TraceId);
            ctx.GetBaggageItems().ToList().ForEach(baggage => Baggage.Add(baggage.Key, baggage.Value));
            return this;
        }
    }

    public partial class Span
    {
        public Span MakeSpanFromSpanData(SpanData span)
        {
            DurationMicros = Convert.ToUInt64(span.Duration.Ticks);
            OperationName = span.OperationName;
            SpanContext = new SpanContext().MakeSpanContextFromOtSpanContext(span.Context);
            StartTimestamp = Timestamp.FromDateTime(span.StartTimestamp.UtcDateTime);
            foreach (var logData in span.LogData)
            {
                Logs.Add(new Log().MakeLogFromLogData(logData));
            }
            foreach (var keyValuePair in span.Tags)
            {
                Tags.Add(new KeyValue().MakeKeyValueFromKvp(keyValuePair));
            }

            if (!string.IsNullOrWhiteSpace(span.Context.ParentSpanId))
            {
                References.Add(new Reference().MakeReferenceFromParentSpanId(span.Context.ParentSpanId));
            }
            
            return this;
        }
    }

    public partial class Reference
    {
        public Reference MakeReferenceFromParentSpanId(string id)
        {
           
            var reference = new Reference();
            reference.Relationship = Types.Relationship.ChildOf;
            reference.SpanContext = new SpanContext {SpanId = Convert.ToUInt64(id)};

            return reference;
        }
    }

    public partial class Log
    {
        public Log MakeLogFromLogData(LogData log)
        {
            Timestamp = Timestamp.FromDateTime(log.Timestamp.DateTime.ToUniversalTime());
            foreach (var keyValuePair in log.Fields)
            {                
                Fields.Add(new KeyValue().MakeKeyValueFromKvp(keyValuePair));
            }

            return this;
        }
    }

    public partial class KeyValue
    {
        //TODO: this is incomplete, needs to be able to convert more stuff.
        public KeyValue MakeKeyValueFromKvp(KeyValuePair<string, object> input)
        {
            Key = input.Key;
            if (input.Value.GetType().IsNumericDatatype())
            {
                DoubleValue = Convert.ToDouble(input.Value);
            }
            if (input.Value.GetType().IsBooleanDatatype())
            {
                BoolValue = Convert.ToBoolean(input.Value);
            }
            else
            {
                StringValue = Convert.ToString(input.Value);
            }
            return this;
        }
    }
}