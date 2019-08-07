using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LightStep.Propagation;
using OpenTracing.Propagation;
using Xunit;

namespace LightStep.Tests
{
    public class PropagatorTests
    {
        [Fact]
        public void PropagatorStackInTracerShouldInjectAndExtract()
        {
            var ps = new PropagatorStack(BuiltinFormats.HttpHeaders);
            ps.AddPropagator(new B3Propagator());
            ps.AddPropagator(new HttpHeadersPropagator());
            ps.AddPropagator(new TextMapPropagator());

            var sr = new SimpleMockRecorder();
            var satOpts = new SatelliteOptions("localhost", 80, true);
            var tracerOpts = new Options("TEST").WithSatellite(satOpts).WithAutomaticReporting(false);

            var tracer = new Tracer(tracerOpts, sr, ps);

            var span = tracer.BuildSpan("propTest").Start();

            var traceId = span.TypedContext().TraceId;
            var spanId = span.TypedContext().SpanId;

            var hexTraceId = Convert.ToUInt64(traceId).ToString("X");
            var hexSpanId = Convert.ToUInt64(spanId).ToString("X");

            var data = new Dictionary<string, string>();

            tracer.Inject(span.Context, BuiltinFormats.HttpHeaders, new TextMapInjectAdapter(data));

            Assert.Equal(hexTraceId, data["ot-tracer-traceid"]);
            Assert.Equal(hexTraceId, data["X-B3-TraceId"]);
            Assert.Equal(hexSpanId, data["ot-tracer-spanid"]);
            Assert.Equal(hexSpanId, data["X-B3-SpanId"]);

            span.Finish();

            var ctx = tracer.Extract(BuiltinFormats.HttpHeaders, new TextMapExtractAdapter(data));

            Assert.Equal(ctx.SpanId, spanId);
            Assert.Equal(ctx.TraceId, traceId);
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
        public void PropagatorStackShouldHandleCustomFormatConstructor()
        {
            var customFmt = new CustomFormatter();
            var ps = new PropagatorStack(customFmt);
            Assert.True(ps.Propagators.Count == 0);
            Assert.Equal(ps.Format, customFmt);
        }

        [Fact]
        public void PropagatorStackShouldHandleEmptyConstructor()
        {
            var ps = new PropagatorStack(BuiltinFormats.TextMap);
            Assert.True(ps.Propagators.Count == 0);
            Assert.Equal(ps.Format, BuiltinFormats.TextMap);
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

        [Fact]
        public void EnvoyPropagatorShouldDecodeABinaryContext()
        {
            var envoyPropagator = new EnvoyPropagator();
            var bs = Convert.FromBase64String("EhQJQwUbbwmQEc4RPaEuilTou0QYAQ==");
            var streamCarrier = new MemoryStream(bs);

            var extractedContext = envoyPropagator.Extract(BuiltinFormats.Binary, new BinaryExtractAdapter(streamCarrier));
            Assert.NotNull(extractedContext);
        }

        [Fact]
        public void EnvoyPropagatorShouldEncodeASpanContext()
        {
            var ctx = new SpanContext("1", "1");
            var envoyPropagator = new EnvoyPropagator();
            var carrierStream = new MemoryStream();

            envoyPropagator.Inject(ctx, BuiltinFormats.Binary, new BinaryInjectAdapter(carrierStream));
            
            Assert.NotNull(carrierStream);
            Assert.True(carrierStream.Length > 0);

            var extractedContext =
                envoyPropagator.Extract(BuiltinFormats.Binary, new BinaryExtractAdapter(carrierStream));
            Assert.NotNull(extractedContext);
            Assert.Equal("1", extractedContext.SpanId);
            Assert.Equal("1", extractedContext.TraceId);
        }

        [Fact]
        public void PropagatorStackShouldThrowOnNullConstructor()
        {
            Assert.Throws<ArgumentNullException>(() => new PropagatorStack(null));
        }

        [Fact]
        public void PropagatorStackShouldThrowOnNullPropagator()
        {
            var ps = new PropagatorStack(BuiltinFormats.TextMap);
            Assert.Throws<ArgumentNullException>(() => ps.AddPropagator(null));
        }
    }

    internal class CustomFormatter : IFormat<ITextMap>
    {
        // dummy class for testing custom formatters
    }
}