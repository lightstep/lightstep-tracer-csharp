using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing;
using OpenTracing.Tag;

namespace LightStep
{
    /// <inheritdoc />
    public class SpanBuilder : ISpanBuilder
    {
        private readonly Tracer _tracer;
        private readonly string _operationName;
        private DateTimeOffset _startTimestamp = DateTimeOffset.MinValue;
        private readonly List<Reference> _references = new List<Reference>();
        private Dictionary<string, object> _tags = new Dictionary<string, object>();
        private bool _ignoreActiveSpan;

        /// <inheritdoc />
        public SpanBuilder(Tracer tracer, string operationName)
        {
            if (string.IsNullOrWhiteSpace(operationName))
            {
                throw new ArgumentNullException(nameof(operationName));
            }

            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _operationName = operationName;
        }

        /// <inheritdoc />
        public ISpanBuilder AsChildOf(ISpanContext parent)
        {
            if (parent == null)
            {
                return this;
            }
            
            return AddReference(References.ChildOf, parent);
        }

        /// <inheritdoc />
        public ISpanBuilder AsChildOf(ISpan parent)
        {
            if (parent == null)
            {
                return this;
            }
            
            return AsChildOf(parent.Context);
        }

        private ISpanBuilder FollowsFrom(ISpanContext spanContext)
        {
            return AddReference(References.FollowsFrom, spanContext);
        }
        
        /// <summary>
        /// Indicates that this span follows from a prior span.
        /// </summary>
        /// <param name="span">An <see cref="ISpan"/></param>
        /// <returns></returns>
        public ISpanBuilder FollowsFrom(ISpan span)
        {
            return FollowsFrom(span?.Context);
        }

        /// <inheritdoc />
        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if (string.IsNullOrWhiteSpace(referenceType))
            {
                throw new ArgumentNullException(nameof(referenceType));
            }

            if (referencedContext != null)
            {
                _references.Add(new Reference((SpanContext)referencedContext, referenceType));
            }

            return this;
        }

        /// <inheritdoc />
        public ISpanBuilder IgnoreActiveSpan()
        {
            _ignoreActiveSpan = true;
            return this;
        }

        /// <inheritdoc />
        public ISpanBuilder WithTag(StringTag tag, string value)
        {
            _tags[tag.Key] = value;
            return this;
        }

        /// <inheritdoc />
        public ISpanBuilder WithStartTimestamp(DateTimeOffset startTimestamp)
        {
            _startTimestamp = startTimestamp;
            return this;
        }

        /// <inheritdoc />
        public IScope StartActive()
        {
            return StartActive(true);
        }

        /// <inheritdoc />
        public IScope StartActive(bool finishSpanOnDispose)
        {
            var span = Start();
            return _tracer.ScopeManager.Activate(span, finishSpanOnDispose);
        }

        /// <inheritdoc />
        public ISpanBuilder WithTag(string key, bool value)
        {
            if (_tags == null)
            {
                _tags = new Dictionary<string, object>();
            }

            _tags[key] = value;
            return this;
        }

        /// <inheritdoc />
        public ISpanBuilder WithTag(string key, double value)
        {
            if (_tags == null)
            {
                _tags = new Dictionary<string, object>();
            }

            _tags[key] = value;
            return this;
        }

        /// <inheritdoc />
        public ISpanBuilder WithTag(BooleanTag tag, bool value)
        {
            _tags[tag.Key] = value;
            return this;
        }

        /// <inheritdoc />
        public ISpanBuilder WithTag(IntOrStringTag tag, string value)
        {
            _tags[tag.Key] = value;
            return this;
        }

        /// <inheritdoc />
        public ISpanBuilder WithTag(IntTag tag, int value)
        {
            _tags[tag.Key] = value;
            return this;
        }

        /// <inheritdoc />
        public ISpanBuilder WithTag(string key, int value)
        {
            if (_tags == null)
            {
                _tags = new Dictionary<string, object>();
            }

            _tags[key] = value;
            return this;
        }

        /// <inheritdoc />
        public ISpanBuilder WithTag(string key, string value)
        {
            if (_tags == null)
            {
                _tags = new Dictionary<string, object>();
            }

            _tags[key] = value;
            return this;
        }

        /// <inheritdoc />
        public ISpan Start()
        {
            if (_startTimestamp == DateTimeOffset.MinValue)
            {
                _startTimestamp = DateTimeOffset.Now;
            }
            
            var activeSpanContext = _tracer.ActiveSpan?.Context;
            if (!_references.Any() && !_ignoreActiveSpan && activeSpanContext != null)
            {
                AddReference(References.ChildOf, activeSpanContext);
            }
            return new Span(_tracer, _operationName, _startTimestamp, _tags, _references);
        }   
    }
}