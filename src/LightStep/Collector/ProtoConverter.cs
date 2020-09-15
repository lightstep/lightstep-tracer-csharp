using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;

namespace LightStep.Collector
{
    /// <inheritdoc />
    public partial class SpanContext
    {
        /// <summary>
        ///     Converts a <see cref="LightStep.SpanContext" /> to a <see cref="SpanContext" />
        /// </summary>
        /// <param name="ctx">A SpanContext</param>
        /// <returns>Proto SpanContext</returns>
        public SpanContext MakeSpanContextFromOtSpanContext(LightStep.SpanContext ctx)
        {
            SpanId = ctx.SpanIdValue;
            TraceId = ctx.TraceIdValue;
            
            ctx.GetBaggageItems().ToList().ForEach(baggage => Baggage.Add(baggage.Key, baggage.Value));
            return this;
        }
    }

    /// <inheritdoc />
    public partial class Span
    {
        const long TicksPerMicrosecond = 10;
        /// <summary>
        ///     Converts a <see cref="SpanData" /> to a <see cref="Span" />
        /// </summary>
        /// <param name="span">A SpanData</param>
        /// <returns>Proto Span</returns>
        public Span MakeSpanFromSpanData(SpanData span)
        {
            // ticks are not equal to microseconds, so convert
            DurationMicros = Convert.ToUInt64(Math.Abs(span.Duration.Ticks) / TicksPerMicrosecond);
            OperationName = span.OperationName;
            SpanContext = new SpanContext().MakeSpanContextFromOtSpanContext(span.Context);
            StartTimestamp = Timestamp.FromDateTime(span.StartTimestamp.UtcDateTime);
            foreach (var logData in span.LogData) Logs.Add(new Log().MakeLogFromLogData(logData));
            foreach (var keyValuePair in span.Tags) Tags.Add(new KeyValue().MakeKeyValueFromKvp(keyValuePair));

            if (span.Context.ParentSpanIdValue != 0L)
                References.Add(Reference.MakeReferenceFromParentSpanIdValue(span.Context.ParentSpanIdValue));

            return this;
        }
    }

    /// <inheritdoc />
    public partial class Reference
    {
        /// <summary>
        ///     Converts a ParentSpanId string into a <see cref="Reference" />
        /// </summary>
        /// <param name="id">A ParentSpanId as a string</param>
        /// <returns>Proto Reference</returns>
        public static Reference MakeReferenceFromParentSpanId(string id)
        {
            var reference = new Reference();
            reference.Relationship = Types.Relationship.ChildOf;
            ulong spanId;
            try
            {
                spanId = Convert.ToUInt64(id);
            }
            catch (FormatException)
            {
                spanId = Convert.ToUInt64(id, 16);
            }
            reference.SpanContext = new SpanContext {SpanId = spanId};

            return reference;
        }

        public static Reference MakeReferenceFromParentSpanIdValue(ulong value)
        {
            var reference = new Reference
            {
                Relationship = Types.Relationship.ChildOf, SpanContext = new SpanContext {SpanId = value}
            };
            return reference;
        }
    }

    /// <inheritdoc />
    public partial class Log
    {
        /// <summary>
        ///     Converts a <see cref="LogData" /> into a <see cref="Log" />
        /// </summary>
        /// <param name="log">A LogData object</param>
        /// <returns>Proto Log</returns>
        public Log MakeLogFromLogData(LogData log)
        {
            Timestamp = Timestamp.FromDateTime(log.Timestamp.DateTime.ToUniversalTime());
            foreach (var keyValuePair in log.Fields) Fields.Add(new KeyValue().MakeKeyValueFromKvp(keyValuePair));

            return this;
        }
    }

    /// <inheritdoc />
    public partial class KeyValue
    {
        /// <summary>
        ///     Converts a <see cref="KeyValuePair{TKey,TValue}" /> into a <see cref="KeyValue" />
        /// </summary>
        /// <param name="input">A KeyValuePair used in Tags, etc.</param>
        /// <returns>Proto KeyValue</returns>
        public KeyValue MakeKeyValueFromKvp(KeyValuePair<string, object> input)
        {
            Key = input.Key;
            if (input.Value == null) StringValue = "null";
            else if (input.Value.IsFloatDataType()) DoubleValue = Convert.ToDouble(input.Value);
            else if (input.Value.IsIntDataType()) IntValue = Convert.ToInt64(input.Value);
            else if (input.Value.IsBooleanDataType()) BoolValue = Convert.ToBoolean(input.Value);
            else if (input.Value.IsJson()) JsonValue = Convert.ToString(input.Value);
            else StringValue = Convert.ToString(input.Value);
            
            return new KeyValue(this);
        }
    }
}