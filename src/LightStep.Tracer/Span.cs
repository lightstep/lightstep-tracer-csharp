using System;
using System.Collections.Generic;
using OpenTracing;
using OpenTracing.Tag;

namespace LightStep.Tracer
{
    public class Span : ISpan
    {
        private readonly ISpanRecorder _spanRecorder;

        private readonly SpanContext _context;

        public ISpanContext Context => _context;

        public string OperationName { get; private set; }
        public DateTimeOffset StartTimestamp { get; }
        public DateTimeOffset? FinishTimestamp { get; private set; }

        public IDictionary<string, object> Tags { get; } = new Dictionary<string, object>();
        public IList<LogData> Logs { get; } = new List<LogData>();

        internal Span(
            ISpanRecorder spanRecorder,
            SpanContext context,
            string operationName,
            DateTimeOffset startTimestamp,
            IDictionary<string, object> tags)
        {
            if (spanRecorder == null)
            {
                throw new ArgumentNullException(nameof(spanRecorder));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (string.IsNullOrWhiteSpace(operationName))
            {
                throw new ArgumentNullException(operationName);
            }

            _spanRecorder = spanRecorder;

            _context = context;
            OperationName = operationName.Trim();
            StartTimestamp = startTimestamp;

            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    Tags.Add(tag);
                }
            }
        }

        public virtual ISpan SetOperationName(string operationName)
        {
            if (string.IsNullOrWhiteSpace(operationName))
            {
                throw new ArgumentNullException(nameof(operationName));
            }

            OperationName = operationName;
            return this;
        }

        public virtual ISpan SetTag(string key, bool value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            Tags[key] = value;
            return this;
        }

        public virtual ISpan SetTag(string key, double value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            Tags[key] = value;
            return this;
        }

        public virtual ISpan SetTag(BooleanTag tag, bool value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key))
            {
                throw new ArgumentNullException(nameof(tag.Key));
            }

            Tags[tag.Key] = value;
            return this;
        }

        public virtual ISpan SetTag(IntOrStringTag tag, string value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key))
            {
                throw new ArgumentNullException(nameof(tag.Key));
            }

            Tags[tag.Key] = value;
            return this;
        }

        public virtual ISpan SetTag(IntTag tag, int value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key))
            {
                throw new ArgumentNullException(nameof(tag.Key));
            }

            Tags[tag.Key] = value;
            return this;
        }

        public virtual ISpan SetTag(StringTag tag, string value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key))
            {
                throw new ArgumentNullException(nameof(tag.Key));
            }

            Tags[tag.Key] = value;
            return this;
        }

        public virtual ISpan SetTag(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            Tags[key] = value;
            return this;
        }

        public virtual ISpan SetTag(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            Tags[key] = value;
            return this;
        }
        

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return Log(DateTimeOffset.UtcNow, fields);
        }

        public ISpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            Logs.Add(new LogData(timestamp, fields));
            return this;
        }

        public ISpan Log(string eventName)
        {
            return Log(DateTimeOffset.UtcNow, eventName);
        }

        public ISpan Log(DateTimeOffset timestamp, string eventName)
        {
            return Log(timestamp, new Dictionary<string, object> { { "event", eventName }});
        }

        public ISpan SetBaggageItem(string key, string value)
        {
            _context.SetBaggageItem(key, value);
            return this;
        }

        public string GetBaggageItem(string key)
        {
            return _context.GetBaggageItem(key);
        }

        public void Finish()
        {
            Finish(DateTimeOffset.UtcNow);
        }

        public void Finish(DateTimeOffset finishTimestamp)
        {
            if (FinishTimestamp.HasValue)
                return;

            FinishTimestamp = finishTimestamp;
            OnFinished();
        }

        public void Dispose()
        {
            Finish();
        }

        protected void OnFinished()
        {
            var spanData = new SpanData()
            {
                Context = this.TypedContext(),
                OperationName = OperationName,
                StartTimestamp = StartTimestamp,
                Duration = FinishTimestamp.Value - StartTimestamp,
                Tags = Tags,
                LogData = Logs,
            };

            _spanRecorder.RecordSpan(spanData);
        }
    }
}