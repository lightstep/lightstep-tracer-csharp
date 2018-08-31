using System;
using System.Collections.Generic;

namespace LightStep
{
    /// <summary>
    /// A single log event
    /// </summary>
    public struct LogData
    {
        /// <summary>
        /// Create a new log event
        /// </summary>
        /// <param name="timestamp">A <see cref="DateTimeOffset"/> from the beginning of the span</param>
        /// <param name="fields">An enumerable of <see cref="KeyValuePair{TKey,TValue}"/> to log.</param>
        public LogData(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Timestamp = timestamp;
            Fields = fields;
        }

        /// <summary>
        /// The time since the beginning of the span.
        /// </summary>
        public DateTimeOffset Timestamp { get; }

        /// <summary>
        /// The log fields.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> Fields { get; }
    }
}
