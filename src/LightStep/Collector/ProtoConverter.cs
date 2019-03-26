﻿using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using LightStep.Logging;

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
            try
            {
                SpanId = Convert.ToUInt64(ctx.SpanId);
            }
            catch (FormatException)
            {
                SpanId = Convert.ToUInt64(ctx.SpanId, 16);
            }

            try
            {
                TraceId = Convert.ToUInt64(ctx.TraceId);
            }
            catch (FormatException)
            {
                TraceId = Convert.ToUInt64(ctx.TraceId, 16);
            }
            
            ctx.GetBaggageItems().ToList().ForEach(baggage => Baggage.Add(baggage.Key, baggage.Value));
            return this;
        }
    }

    /// <inheritdoc />
    public partial class Span
    {
        const long TicksPerMicrosecond = 10;
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();
        /// <summary>
        ///     Converts a <see cref="SpanData" /> to a <see cref="Span" />
        /// </summary>
        /// <param name="span">A SpanData</param>
        /// <returns>Proto Span</returns>
        public Span MakeSpanFromSpanData(SpanData span)
        {
            // ticks are not equal to microseconds, so convert
            try {
                DurationMicros = Convert.ToUInt64(span.Duration.Ticks / TicksPerMicrosecond);
            } catch (System.OverflowException e) {
                // if we fail here, set the duration to 1us.
                _logger.WarnException("Caught an exception while serializing span duration. This can occur if your span finished before it started.", e);
                DurationMicros = 1;
            }
            OperationName = span.OperationName;
            SpanContext = new SpanContext().MakeSpanContextFromOtSpanContext(span.Context);
            StartTimestamp = Timestamp.FromDateTime(span.StartTimestamp.UtcDateTime);
            foreach (var logData in span.LogData) Logs.Add(new Log().MakeLogFromLogData(logData));
            foreach (var keyValuePair in span.Tags) Tags.Add(new KeyValue().MakeKeyValueFromKvp(keyValuePair));

            if (!string.IsNullOrWhiteSpace(span.Context.ParentSpanId))
                References.Add(Reference.MakeReferenceFromParentSpanId(span.Context.ParentSpanId));

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