using System;
using System.Collections.Generic;
using OpenTracing;
using OpenTracing.Tag;

namespace LightStep.Tracer
{
    public class SpanBuilder : ISpanBuilder
    {
     private readonly Tracer _tracer;
        private readonly string _operationName;

        // not initialized to save allocations in case there are no references.
        private List<Tuple<string, ISpanContext>> _references;

        // not initialized to save allocations in case there are no tags.
        private IDictionary<string, object> _tags;

        private DateTimeOffset? _startTimestamp;

        public SpanBuilder(Tracer tracer, string operationName)
        {
            if (tracer == null)
            {
                throw new ArgumentNullException(nameof(tracer));
            }
            if (string.IsNullOrWhiteSpace(operationName))
            {
                throw new ArgumentNullException(nameof(operationName));
            }

            _tracer = tracer;
            _operationName = operationName;
        }

        public ISpanBuilder AsChildOf(ISpanContext spanContext)
        {
            return AddReference(References.ChildOf, spanContext);
        }

        public ISpanBuilder AsChildOf(ISpan span)
        {
            return AsChildOf(span?.Context);
        }

        public ISpanBuilder FollowsFrom(ISpanContext spanContext)
        {
            return AddReference(References.FollowsFrom, spanContext);
        }

        public ISpanBuilder FollowsFrom(ISpan span)
        {
            return FollowsFrom(span?.Context);
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if (string.IsNullOrWhiteSpace(referenceType))
            {
                throw new ArgumentNullException(nameof(referenceType));
            }

            if (referencedContext != null)
            {
                if (_references == null)
                {
                    _references = new List<Tuple<string, ISpanContext>>();
                }

                _references.Add(Tuple.Create(referenceType, referencedContext));
            }

            return this;
        }

        public ISpanBuilder IgnoreActiveSpan()
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(StringTag tag, string value)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset startTimestamp)
        {
            _startTimestamp = startTimestamp;
            return this;
        }

        public IScope StartActive()
        {
            throw new NotImplementedException();
        }

        public IScope StartActive(bool finishSpanOnDispose)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            if (_tags == null)
            {
                _tags = new Dictionary<string, object>();
            }

            _tags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            if (_tags == null)
            {
                _tags = new Dictionary<string, object>();
            }

            _tags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(BooleanTag tag, bool value)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(IntOrStringTag tag, string value)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(IntTag tag, int value)
        {
            throw new NotImplementedException();
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            if (_tags == null)
            {
                _tags = new Dictionary<string, object>();
            }

            _tags[key] = value;
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            if (_tags == null)
            {
                _tags = new Dictionary<string, object>();
            }

            _tags[key] = value;
            return this;
        }

        public ISpan Start()
        {
            return _tracer.StartSpan(_operationName, _startTimestamp, _references, _tags);
        }   
    }
}