using System;
using Xunit;

namespace LightStep.Tracer.Tests
{
    public class TracerTests
    {
        private Tracer GetTracer(ISpanRecorder recorder = null)
        {
            var spanRecorder = recorder ?? new SimpleMockRecorder();
            var spanContextFactory = new SpanContextFactory();
            return new Tracer(spanContextFactory, spanRecorder);
        }
        [Fact]
        public void Tracer_ReturnSpanWhenStartSpanCalled()
        {
            var tracer = GetTracer();

            var span = tracer.BuildSpan("TestOp").Start();
            
            Assert.NotNull(span);
        }
    }
}