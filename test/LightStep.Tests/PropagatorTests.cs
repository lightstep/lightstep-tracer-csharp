using System;
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
    }

    internal class CustomFormatter : IFormat<ITextMap>
    {
        // dummy class for testing custom formatters
    }
}