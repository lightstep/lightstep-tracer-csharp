using System;
using System.Collections.Generic;
using System.Linq;
using LightStep.Collector;
using OpenTracing;
using OpenTracing.Tag;

namespace LightStep
{
    public sealed class Span : ISpan
    {
        private readonly object _lock = new object();

        private readonly Tracer _tracer;
        private SpanContext _context;
        private DateTimeOffset _finishTimestamp;
        private bool _finished;
        private readonly Dictionary<string, object> _tags;
        private readonly List<Reference> _references;
        private readonly List<LogData> _logs = new List<LogData>();
        private readonly List<Exception> _errors = new List<Exception>();
        
        public string ParentId { get; }
        
        public DateTimeOffset StartTimestamp { get; }
        public DateTimeOffset FinishTimestamp
        {
            get
            {
                if (_finishTimestamp == DateTimeOffset.MinValue)
                {
                    throw new InvalidOperationException("Must call Finish() before FinishTimestamp");
                }

                return _finishTimestamp;
            }
        }
        
        public string OperationName { get; private set; }
        
        public Dictionary<string, object> Tags => new Dictionary<string, object>(_tags);
        public List<LogData> Logs => new List<LogData>(_logs);
        public List<Exception> Errors => new List<Exception>(_errors);

        public SpanContext Context
        {
            get
            {
                lock (_lock)
                {
                    return _context;
                }
            }
        }

        ISpanContext ISpan.Context => Context;
        
        public Span(
            Tracer tracer,
            string operationName,
            DateTimeOffset startTimestamp,
            IDictionary<string, object> tags,
            List<Reference> references)
        {
            _tracer = tracer;
            OperationName = operationName;
            StartTimestamp = startTimestamp;

            _tags = tags == null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(tags);
            
            _references = references == null
                ? new List<Reference>()
                : references.ToList();
            
            
            var parentContext = FindPreferredParentRef(_references);
            
            OperationName = operationName.Trim();
            StartTimestamp = startTimestamp;

            if (parentContext == null)
            {
                // we are a root span
                _context = new SpanContext(GetRandomId(), GetRandomId(), new Baggage());
                ParentId = null;
            }
            else
            {
                // we are a child span
                _context = new SpanContext(parentContext.TraceId, GetRandomId(), MergeBaggages(_references));
                ParentId = parentContext.SpanId;
            }
        }

        private static string GetRandomId()
        {
            return new Random().NextUInt64().ToString();
        }

        private static SpanContext FindPreferredParentRef(IList<Reference> references)
        {
            if (!references.Any())
            {
                return null;
            }

            foreach (var reference in references)
            {
                if (References.ChildOf.Equals(reference.ReferenceType))
                {
                    return reference.Context;
                }
            }

            return references.First().Context;
        }
        
        private static Baggage MergeBaggages(IList<Reference> references)
        {
            var baggage = new Baggage();
            foreach (var reference in references)
            {
                if (reference.Context.GetBaggageItems() != null)
                {
                    foreach (var bagItem in reference.Context.GetBaggageItems())
                    {
                        baggage.Set(bagItem.Key, bagItem.Value);
                    }
                }
            }

            return baggage;
        }

        private void CheckIfSpanFinished(string format, params object[] args)
        {
            if (_finished)
            {
                var ex = new InvalidOperationException(string.Format(format, args));
                _errors.Add(ex);
                throw ex;
            }
        }
        
        private ISpan SetObjectTag(string key, object value)
        {
            lock (_lock)
            {
                CheckIfSpanFinished("Setting tag [{0}:{1}] on finished span", key, value);
                _tags[key] = value;
                return this;
            }
        }

        #region Setters
        
        public ISpan SetOperationName(string operationName)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(operationName))
                {
                    throw new ArgumentNullException(nameof(operationName));
                }
            
                CheckIfSpanFinished("Setting operationName [{0}] on finished span.", operationName);
                OperationName = operationName;
                return this;
            }      
        }

        public ISpan SetTag(string key, bool value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return SetObjectTag(key, value);
        }

        public ISpan SetTag(string key, double value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return SetObjectTag(key, value);
        }
        
        public ISpan SetTag(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return SetObjectTag(key, value);
        }

        public ISpan SetTag(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return SetObjectTag(key, value);
        }

        public ISpan SetTag(BooleanTag tag, bool value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key))
            {
                throw new ArgumentNullException(nameof(tag.Key));
            }

            return SetObjectTag(tag.Key, value);
        }

        public ISpan SetTag(IntOrStringTag tag, string value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key))
            {
                throw new ArgumentNullException(nameof(tag.Key));
            }

            return SetObjectTag(tag.Key, value);
        }

        public ISpan SetTag(IntTag tag, int value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key))
            {
                throw new ArgumentNullException(nameof(tag.Key));
            }

            return SetObjectTag(tag.Key, value);
        }

        public ISpan SetTag(StringTag tag, string value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key))
            {
                throw new ArgumentNullException(nameof(tag.Key));
            }

            return SetObjectTag(tag.Key, value);
        }
        #endregion

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return Log(DateTimeOffset.UtcNow, fields);
        }

        public ISpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            lock (_lock)
            {
                CheckIfSpanFinished("Adding logs {0} at {1} to already finished span.", fields, timestamp);
                Logs.Add(new LogData(timestamp, fields));
                return this;  
            }   
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
            lock (_lock)
            {
                CheckIfSpanFinished("Adding baggage [{0}:{1}] to finished span.", key, value);
                _context.SetBaggageItem(key, value);
                return this;    
            }
        }

        public string GetBaggageItem(string key)
        {
            lock (_lock)
            {
                return _context.GetBaggageItem(key);    
            }
        }

        public void Finish()
        {
            Finish(DateTimeOffset.UtcNow);
        }

        public void Finish(DateTimeOffset finishTimestamp)
        {
            lock (_lock)
            {
                CheckIfSpanFinished("Tried to finish already finished span");
                _finishTimestamp = finishTimestamp;
                _finished = true;
                OnFinished();
            }
        }

        private void OnFinished()
        {
            var spanData = new SpanData
            {
                Context = this.TypedContext(),
                OperationName = OperationName,
                StartTimestamp = StartTimestamp,
                Duration = FinishTimestamp - StartTimestamp,
                Tags = Tags,
                LogData = Logs,
            };

            _tracer.AppendFinishedSpan(spanData);
        }
    }
}