using System;
using System.Collections.Generic;

namespace LightStep
{
    public struct LogData
    {
        public LogData(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Timestamp = timestamp;
            Fields = fields;
        }

        public DateTimeOffset Timestamp { get; }

        public IEnumerable<KeyValuePair<string, object>> Fields { get; }
    }
}
