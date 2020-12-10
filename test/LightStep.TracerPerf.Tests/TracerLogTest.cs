using NUnit.Framework;
using Serilog;

namespace LightStep.TracerPerf.Tests
{
    public class TracerLogTest : TracerTestBase
    {
        [SetUp]
        public void SetUp()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Log is setup!");
        }

        [Test]
        public void TestExecute([Values("NoFinishNoDispose", "ExplicitFinish", "FinishOnDispose", "DisposeNoFinish")]
            string tracingMethod)
        {
            Execute(TracingMethods[tracingMethod]);
        }
    }
}