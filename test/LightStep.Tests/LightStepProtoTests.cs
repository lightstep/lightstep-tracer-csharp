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
            var tracerOptions = new Options("TEST").WithStatellite(satelliteOptions).WithAutomaticReporting(false);
            return new Tracer(tracerOptions, spanRecorder);
        }

        private LightStepHttpClient GetClient()
        {
            var satelliteOptions = new SatelliteOptions("localhost", 80, true);
            var tracerOptions = new Options("TEST").WithStatellite(satelliteOptions).WithAutomaticReporting(false);
            return new LightStepHttpClient("http://localhost:80", tracerOptions);
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
            var spans = new List<SpanData> { recorder.GetSpanBuffer().First() };
            var translatedSpans = client.Translate(spans);
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
    }
}