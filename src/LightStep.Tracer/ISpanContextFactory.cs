using System;
using System.Collections.Generic;
using OpenTracing;

namespace LightStep.Tracer
{
    public interface ISpanContextFactory
    {
        SpanContext CreateSpanContext(IList<Tuple<string, ISpanContext>> references);
    }
}