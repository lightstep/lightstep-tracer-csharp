using System;
using System.Collections.Generic;

namespace LightStep
{
    /// <summary>
    ///     An internal representation of a span.
    /// </summary>
    public class SpanData
    {
        /// <summary>
        ///     The Span Context
        /// </summary>
        public SpanContext Context { get; internal set; }

        /// <summary>
        ///     The span operation
        /// </summary>
        public string OperationName { get; internal set; }

        /// <summary>
        ///     The start of the span.
        /// </summary>
        public DateTimeOffset StartTimestamp { get; internal set; }

        /// <summary>
        ///     How long the span ran.
        /// </summary>
        public TimeSpan Duration { get; internal set; }

        /// <summary>
        ///     Tags for the span.
        /// </summary>
        public IDictionary<string, object> Tags { get; internal set; }

        /// <summary>
        ///     Logs emitted as part of the span.
        /// </summary>
        public IList<LogData> LogData { get; internal set; }

        public override string ToString()
        {
            return
                $"[{OperationName}] ctx: {Context}, startTime: {StartTimestamp}, duration: {Duration.TotalMilliseconds}";
        }
    }
}