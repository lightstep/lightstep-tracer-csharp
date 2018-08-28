using System;
using System.Threading;
using LightStep;
using OpenTracing;
using OpenTracing.Tag;
using OpenTracing.Util;

namespace LightStep.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // substitute your own LS API Key here
            var lsKey = "TEST_TOKEN";
            var lsSettings = new SatelliteOptions("localhost", 9996, false);
            var tracer = new Tracer(new Options(lsKey, lsSettings));
            GlobalTracer.Register(tracer);

            for (var i = 0; i < 50; i++)
            {
                using (IScope scope = tracer.BuildSpan("testParent").WithTag("testSpan", "true").StartActive(true))
                {
                    Console.WriteLine("sleeping for a bit");
                    Thread.Sleep(new Random().Next(5, 10));
                    var innerSpan = tracer.BuildSpan("childSpan").Start();
                    Console.WriteLine("sleeping more...");
                    Thread.Sleep(new Random().Next(10, 20));
                    innerSpan.Finish();
                }
                tracer.Flush();
            }
        }
    }
}