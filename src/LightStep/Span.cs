using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using LightStep.Logging;
using OpenTracing;
using OpenTracing.Tag;

namespace LightStep
{
    /// <inheritdoc />
    public sealed class Span : ISpan
    {
        private readonly SpanContext _context;
        private readonly List<Exception> _errors = new List<Exception>();
        private readonly object _lock = new object();
        private readonly List<LogData> _logs = new List<LogData>();
        private readonly List<Reference> _references;
        private readonly Dictionary<string, object> _tags;

        private readonly Tracer _tracer;
        private bool _finished;
        private DateTimeOffset _finishTimestamp;

        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        ///     Create a new Span.
        /// </summary>
        /// <param name="tracer">Tracer that will record the span.</param>
        /// <param name="operationName">Operation name being recorded by the span.</param>
        /// <param name="startTimestamp">The <see cref="DateTimeOffset" /> at which the span begun.</param>
        /// <param name="tags">Tags for the span.</param>
        /// <param name="references">References to other spans.</param>
        public Span(
            Tracer tracer,
            string operationName,
            DateTimeOffset startTimestamp,
            IDictionary<string, object> tags,
            IReadOnlyCollection<Reference> references)
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
                _context = new SpanContext(parentContext.TraceId, GetRandomId(), MergeBaggages(_references),
                    parentContext.SpanId);
                ParentId = parentContext.SpanId;
            }
            if (_tracer._options.EnableMetaEventLogging && Utilities.IsNotMetaSpan(this))
            {
                _tracer.BuildSpan(LightStepConstants.MetaEvent.TracerCreateOperation)
                    .IgnoreActiveSpan()
                    .WithTag(LightStepConstants.MetaEvent.MetaEventKey, true)
                    .WithTag(LightStepConstants.MetaEvent.SpanIdKey, _context.SpanId)
                    .WithTag(LightStepConstants.MetaEvent.TraceIdKey, _context.TraceId)
                    .Start()
                    .Finish();
            }
            
        }

        /// <summary>
        ///     SpanID of this span's parent.
        /// </summary>
        private string ParentId { get; }

        /// <summary>
        ///     The start time of the span.
        /// </summary>
        private DateTimeOffset StartTimestamp { get; }

        /// <summary>
        ///     The finish time of the span.
        /// </summary>
        /// <exception cref="InvalidOperationException">A span must be finished before setting the finish timestamp.</exception>
        private DateTimeOffset FinishTimestamp
        {
            get
            {
                if (_finishTimestamp == DateTimeOffset.MinValue)
                    throw new InvalidOperationException("Must call Finish() before FinishTimestamp");

                return _finishTimestamp;
            }
        }

        /// <summary>
        ///     The operation name of the span.
        /// </summary>
        private string OperationName { get; set; }

        public Dictionary<string, object> Tags => new Dictionary<string, object>(_tags);
        public List<LogData> Logs => new List<LogData>(_logs);
        public List<Exception> Errors => new List<Exception>(_errors);

        /// <summary>
        ///     The span's <see cref="SpanContext" />
        /// </summary>
        private SpanContext Context
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

        /// <inheritdoc />
        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
        {
            return Log(DateTimeOffset.Now, fields);
        }

        /// <inheritdoc />
        public ISpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            lock (_lock)
            {
                CheckIfSpanFinished("Adding logs {0} at {1} to already finished span.", fields, timestamp);
                _logs.Add(new LogData(timestamp, fields));
                return this;
            }
        }

        /// <inheritdoc />
        public ISpan Log(string eventName)
        {
            return Log(DateTimeOffset.Now, eventName);
        }

        /// <inheritdoc />
        public ISpan Log(DateTimeOffset timestamp, string eventName)
        {
            return Log(timestamp, new Dictionary<string, object> {{"event", eventName}});
        }

        /// <inheritdoc />
        public ISpan SetBaggageItem(string key, string value)
        {
            lock (_lock)
            {
                CheckIfSpanFinished("Adding baggage [{0}:{1}] to finished span.", key, value);
                _context.SetBaggageItem(key, value);
                return this;
            }
        }

        /// <inheritdoc />
        public string GetBaggageItem(string key)
        {
            lock (_lock)
            {
                return _context.GetBaggageItem(key);
            }
        }

        /// <inheritdoc />
        public void Finish()
        {
            Finish(DateTimeOffset.UtcNow);
        }

        /// <inheritdoc />
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

        private static string GetRandomId()
        {
            var provider = new RNGCryptoServiceProvider();
            var buffer = new byte[64];
            provider.GetBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0).ToString();
        }

        private static SpanContext FindPreferredParentRef(IList<Reference> references)
        {
            if (!references.Any()) return null;

            foreach (var reference in references)
                if (References.ChildOf.Equals(reference.ReferenceType))
                    return reference.Context;

            return references.First().Context;
        }

        private static Baggage MergeBaggages(IList<Reference> references)
        {
            var baggage = new Baggage();
            foreach (var reference in references)
                if (reference.Context.GetBaggageItems() != null)
                    foreach (var bagItem in reference.Context.GetBaggageItems())
                        baggage.Set(bagItem.Key, bagItem.Value);

            return baggage;
        }

        private void CheckIfSpanFinished(string format, params object[] args)
        {
            if (_finished)
            {
                var ex = new InvalidOperationException(string.Format(format, args));
                _errors.Add(ex);
                _logger.Error(ex.Message);
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

        private void OnFinished()
        {
            var spanData = new SpanData
            {
                Context = this.TypedContext(),
                OperationName = OperationName,
                StartTimestamp = StartTimestamp,
                Duration = FinishTimestamp - StartTimestamp,
                Tags = _tags,
                LogData = _logs
            };

            _tracer.AppendFinishedSpan(spanData);
            if(_tracer._options.EnableMetaEventLogging && Utilities.IsNotMetaSpan(this))
            {
                _tracer.BuildSpan(LightStepConstants.MetaEvent.SpanFinishOperation)
                    .IgnoreActiveSpan()
                    .WithTag(LightStepConstants.MetaEvent.MetaEventKey, true)
                    .WithTag(LightStepConstants.MetaEvent.SpanIdKey, this.TypedContext().SpanId)
                    .WithTag(LightStepConstants.MetaEvent.TraceIdKey, this.TypedContext().TraceId)
                    .Start()
                    .Finish();
            } 
        }

        #region Setters

        /// <inheritdoc />
        public ISpan SetOperationName(string operationName)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(operationName)) throw new ArgumentNullException(nameof(operationName));

                CheckIfSpanFinished("Setting operationName [{0}] on finished span.", operationName);
                OperationName = operationName;
                return this;
            }
        }

        /// <inheritdoc />
        public ISpan SetTag(string key, bool value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            return SetObjectTag(key, value);
        }

        /// <inheritdoc />
        public ISpan SetTag(string key, double value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            return SetObjectTag(key, value);
        }

        /// <inheritdoc />
        public ISpan SetTag(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            return SetObjectTag(key, value);
        }

        /// <inheritdoc />
        public ISpan SetTag(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            return SetObjectTag(key, value);
        }

        /// <inheritdoc />
        public ISpan SetTag(BooleanTag tag, bool value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key)) throw new ArgumentNullException(nameof(tag.Key));

            return SetObjectTag(tag.Key, value);
        }

        /// <inheritdoc />
        public ISpan SetTag(IntOrStringTag tag, string value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key)) throw new ArgumentNullException(nameof(tag.Key));

            return SetObjectTag(tag.Key, value);
        }

        /// <inheritdoc />
        public ISpan SetTag(IntTag tag, int value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key)) throw new ArgumentNullException(nameof(tag.Key));

            return SetObjectTag(tag.Key, value);
        }

        /// <inheritdoc />
        public ISpan SetTag(StringTag tag, string value)
        {
            if (string.IsNullOrWhiteSpace(tag.Key)) throw new ArgumentNullException(nameof(tag.Key));

            return SetObjectTag(tag.Key, value);
        }

        #endregion
    }
}