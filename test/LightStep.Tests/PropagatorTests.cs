using System;
using System.Collections.Generic;
using System.Linq;
using LightStep.Propagation;
using OpenTracing.Propagation;
using Xunit;

namespace LightStep.Tests
{
    public class PropagatorTests
    {
        [Fact]
        public void PropagatorStackShouldThrowOnNullConstructor()
        {
            Assert.Throws<ArgumentNullException>(() => new PropagatorStack(null));
        }

        [Fact]
        public void PropagatorStackShouldHandleEmptyConstructor()
        {
            var ps = new PropagatorStack(BuiltinFormats.TextMap);
            Assert.True(ps.Propagators.Count == 0);
            Assert.Equal(ps.Format, BuiltinFormats.TextMap);
        }

        [Fact]
        public void PropagatorStackShouldHandleCustomFormatConstructor()
        {
            var customFmt = new CustomFormatter();
            var ps = new PropagatorStack(customFmt);
            Assert.True(ps.Propagators.Count == 0);
            Assert.Equal(ps.Format, customFmt);
        }

        [Fact]
        public void PropagatorStackShouldThrowOnNullPropagator()
        {
            var ps = new PropagatorStack(BuiltinFormats.TextMap);
            Assert.Throws<ArgumentNullException>(() => ps.AddPropagator(null));
        }

        [Fact]
        public void PropagatorStackShouldAddPropagator()
        {
            var b3Propagator = new B3Propagator();
            var ps = new PropagatorStack(BuiltinFormats.HttpHeaders);
            
            Assert.Equal(ps.AddPropagator(Propagators.HttpHeadersPropagator), ps);
            Assert.Equal(ps.AddPropagator(b3Propagator), ps);
            Assert.Equal(2, ps.Propagators.Count);
            Assert.Equal(ps.Propagators[0], Propagators.HttpHeadersPropagator);
            Assert.Equal(ps.Propagators[1], b3Propagator);
        }

        [Fact]
        public void PropagatorStackShouldInjectExtractAllPropagators()
        {
            var ps = new PropagatorStack(BuiltinFormats.TextMap);
            var httpPropagator = new HttpHeadersPropagator();
            var b3Propagator = new B3Propagator();
            var textPropagator = new TextMapPropagator();
            
            ps.AddPropagator(httpPropagator);
            ps.AddPropagator(b3Propagator);
            ps.AddPropagator(textPropagator);

            var carrier = new Dictionary<string, string>();
            var context = new SpanContext("0", "0");
            
            ps.Inject(context, BuiltinFormats.TextMap, new TextMapInjectAdapter(carrier));

            var propagators = new List<IPropagator> {httpPropagator, b3Propagator, textPropagator};

            foreach (var t in propagators)
            {
                var extractedContext =
                    t.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(carrier));
                Assert.NotNull(extractedContext);
                Assert.Equal(context.TraceId, extractedContext.TraceId);
                Assert.Equal(context.SpanId, extractedContext.SpanId);
            }
        }
    }

    internal class CustomFormatter : IFormat<ITextMap>
    {
        // dummy class for testing custom formatters
    }
}