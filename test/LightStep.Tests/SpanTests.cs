using System.Linq;
using System.Threading.Tasks;
using OpenTracing.Tag;
using Xunit;

namespace LightStep.Tests
{
    public class SpanTests
    {
        private Tracer GetTracer(ISpanRecorder recorder = null)
        {
            var spanRecorder = recorder ?? new SimpleMockRecorder();
            var satelliteOptions = new SatelliteOptions("localhost", 80, true);
            var tracerOptions = new Options("TEST", satelliteOptions);

            return new Tracer(tracerOptions, spanRecorder);
        }

        [Fact]
        public void SpansShouldAllowThreadSafeAccess()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder);
            var span = tracer.BuildSpan("testOperation").Start();

            var t1 = Task.Run(() =>
            {
                span.Log("t1 run");
                span.SetTag("sameTag", "t1");
                span.SetTag("t1Tag", "t1");
            });
            var t2 = Task.Run(() =>
            {
                span.Log("t2 run");
                span.SetTag("sameTag", "t2");
                span.SetTag("t2Tag", "t2");
            });

            t1.Wait();
            t2.Wait();

            span.Finish();

            var finishedSpan = recorder.GetSpanBuffer().First();

            // we expect there to be 2 logs and 3 tags
            Assert.True(finishedSpan.LogData.Count == 2);
            Assert.True(finishedSpan.Tags.Count == 3);
        }

        [Fact]
        public void SpansShouldBuildProperStringKeyTags()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder);
            var span = tracer.BuildSpan("testOperation")
                .WithTag("boolTrueTag", true)
                .WithTag("boolFalseTag", false)
                .WithTag("intTag", 0)
                .WithTag("stringTag", "test")
                .WithTag("doubleTag", 0.1)
                .Start();
            span.Finish();
            var finishedSpan = recorder.GetSpanBuffer().First();

            Assert.True((bool) finishedSpan.Tags["boolTrueTag"]);
            Assert.False((bool) finishedSpan.Tags["boolFalseTag"]);
            Assert.Equal(0, finishedSpan.Tags["intTag"]);
            Assert.Equal("test", finishedSpan.Tags["stringTag"]);
            Assert.Equal(0.1, finishedSpan.Tags["doubleTag"]);
        }

        [Fact]
        public void SpansShouldBuildProperTypedKeyTags()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder);
            var span = tracer.BuildSpan("testOperation")
                .WithTag(new BooleanTag("testBoolTag"), true)
                .WithTag(new IntTag("testIntTag"), 1)
                .WithTag(new StringTag("testStringTag"), "test")
                .WithTag(new IntOrStringTag("testIntOrStringTagAsString"), "string")
                .WithTag(new IntOrStringTag("testIntOrStringTagAsInt"), 1)
                .Start();
            span.Finish();
            var finishedSpan = recorder.GetSpanBuffer().First();

            Assert.True((bool) finishedSpan.Tags["testBoolTag"]);
            Assert.Equal(1, finishedSpan.Tags["testIntTag"]);
            Assert.Equal("test", finishedSpan.Tags["testStringTag"]);
            Assert.Equal("string", finishedSpan.Tags["testIntOrStringTagAsString"]);
            Assert.Equal(1, finishedSpan.Tags["testIntOrStringTagAsInt"]);
        }

        [Fact]
        public void SpansShouldRecordLogs()
        {
            var recorder = new SimpleMockRecorder();
            var tracer = GetTracer(recorder);
            var span = tracer.BuildSpan("testOperation").Start();
            span.Log("hello world!");
            span.Finish();

            var finishedSpan = recorder.GetSpanBuffer().First();
            // project the sequence of logdata into an array with one item, the aforementioned log message
            var finishedSpanLogData = finishedSpan.LogData.Select(
                item => item.Fields.First(
                    f => f.Value.Equals("hello world!")
                ).Value).ToArray()[0];

            Assert.Equal("hello world!", (string) finishedSpanLogData);
        }
    }
}