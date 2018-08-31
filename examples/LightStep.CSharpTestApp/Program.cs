using System;
using System.Threading;
using OpenTracing.Util;

namespace LightStep.TestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // substitute your own LS API Key here
            var lsKey = "TEST_TOKEN";
            var lsSettings = new SatelliteOptions("localhost", 9996, false);
            var tracer = new Tracer(new Options(lsKey, lsSettings));
            GlobalTracer.Register(tracer);

            for (var i = 0; i < 500; i++)
                using (var scope = tracer.BuildSpan("testParent").WithTag("testSpan", "true").StartActive(true))
                {
                    scope.Span.Log("test");
                    tracer.ActiveSpan.Log($"iteration {i}");
                    Console.WriteLine("sleeping for a bit");
                    Thread.Sleep(new Random().Next(5, 10));
                    var innerSpan = tracer.BuildSpan("childSpan").Start();
                    innerSpan.SetTag("innerTestTag", "true");
                    Console.WriteLine("sleeping more...");
                    Thread.Sleep(new Random().Next(10, 20));
                    innerSpan.Finish();
                }

            tracer.Flush();
        }
    }
}