using System;
using System.Collections.Generic;
using System.Linq;
using OpenTracing.Propagation;
using Xunit;
using LightStep.Collector;

namespace LightStep.Tests
{
    public class LightStepProtoTests
    {
        private Tracer GetTracer(ISpanRecorder recorder = null)
        {
            var spanRecorder = recorder ?? new SimpleMockRecorder();
            var satelliteOptions = new SatelliteOptions("localhost", 80, true);
            var tracerOptions = new Options("TEST").WithSatellite(satelliteOptions).WithAutomaticReporting(false);
            return new Tracer(tracerOptions, spanRecorder);
        }

        private LightStepHttpClient GetClient(TransportOptions t = TransportOptions.BinaryProto)
        {
            var satelliteOptions = new SatelliteOptions("localhost", 80, true);
            var tracerOptions = new Options("TEST").WithSatellite(satelliteOptions).WithAutomaticReporting(false).WithTransport(t);
            return new LightStepHttpClient("http://localhost:80", tracerOptions);
        }

        [Fact]
        public void ReportShouldBeJsonWithJsonOption()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder);
            var span = tracer.BuildSpan("test").Start();
            span.Finish();

            var client = GetClient(TransportOptions.JsonProto);
            var translatedSpans = client.Translate(recorder.GetSpanBuffer());
            var report = client.BuildRequest(translatedSpans);
            Assert.Equal("application/json", report.Content.Headers.ContentType.MediaType);
            var contentString = report.Content.ReadAsStringAsync().Result;
            Assert.Contains("test", contentString);
        }

        [Fact]
        public void ReportShouldBeBinaryWithoutJsonOption()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder);
            var span = tracer.BuildSpan("test").Start();
            span.Finish();

            var client = GetClient();
            var translatedSpans = client.Translate(recorder.GetSpanBuffer());
            var report = client.BuildRequest(translatedSpans);
            Assert.Equal("application/octet-stream", report.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void InternalMetricsShouldExist()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder);
            var span = tracer.BuildSpan("test").Start();
            span.Finish();

            var client = GetClient();

            var translatedSpans = client.Translate(recorder.GetSpanBuffer());
            Assert.Equal("spans.dropped", translatedSpans.InternalMetrics.Counts[0].Name);
        }

        [Fact]
        public void DroppedSpanCountShouldSerializeCorrectly()
        {
            var mockBuffer = new SimpleMockRecorder();
            mockBuffer.RecordDroppedSpans(1);

            var client = GetClient();
            var translatedBuffer = client.Translate(mockBuffer.GetSpanBuffer());
            
            Assert.Equal(1, translatedBuffer.InternalMetrics.Counts[0].IntValue);
        }

        [Fact]
        public void DroppedSpanCountShouldIncrementOnBadSpan()
        {
            var recorder = new SimpleMockRecorder();
            var badSpan = new SpanData {
                Duration = new TimeSpan(-1),
                OperationName = "badSpan"
            };
            recorder.RecordSpan(badSpan);
            var client = GetClient();
            var translatedBuffer = client.Translate(recorder.GetSpanBuffer());
            Assert.Equal(1, translatedBuffer.InternalMetrics.Counts[0].IntValue);
        }

        [Fact]
        public void ConverterShouldConvertValues()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder);
            var span = tracer.BuildSpan("testOperation")
                .WithTag("boolTrueTag", true)
                .WithTag("boolFalseTag", false)
                .WithTag("intTag", 0)
                .WithTag("stringTag", "test")
                .WithTag("doubleTag", 0.1)
                .WithTag("nullTag", null)
                .WithTag("jsonTag", @"{""key"":""value""}")
                .Start();
            span.Finish();

            var client = GetClient();
            
            var translatedSpans = client.Translate(recorder.GetSpanBuffer());
            var translatedSpan = translatedSpans.Spans[0];

            foreach (var tag in translatedSpan.Tags)
            {
                switch (tag.Key)
                {
                    case "boolTrueFlag":
                        Assert.True(tag.BoolValue);
                        break;
                    case "boolFalseFlag":
                        Assert.False(tag.BoolValue);
                        break;
                    case "intTag":
                        Assert.Equal(0, tag.IntValue);
                        break;
                    case "stringTag":
                        Assert.Equal("test", tag.StringValue);
                        break;
                    case "doubleTag":
                        Assert.Equal(0.1, tag.DoubleValue);
                        break;
                    case "nullTag":
                        Assert.Equal("null", tag.StringValue);
                        break;
                    case "jsonTag":
                        Assert.Equal(@"{""key"":""value""}", tag.JsonValue);
                        break;
                    default:
                        continue;
                }
            }
        }

        [Fact]
        public void SpanContextSpanIdShouldConvert()
        {
            var spanContext = new SpanContext(4659, 4660);
            var protoSpanContext = new LightStep.Collector.SpanContext().MakeSpanContextFromOtSpanContext(spanContext);
            Assert.Equal(4659ul, protoSpanContext.TraceId);
            Assert.Equal(4660ul, protoSpanContext.SpanId);
        }
    }
}