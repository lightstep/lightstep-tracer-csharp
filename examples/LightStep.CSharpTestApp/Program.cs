using System;
using System.IO;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using OpenTracing.Util;

namespace LightStep.CSharpTestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // substitute your own LS API Key here
            var lightStepSatellite = new SatelliteOptions("localhost", 9996, true);
            var lightStepOptions = new Options("TEST_TOKEN").WithStatellite(lightStepSatellite);
            var tracer = new Tracer(lightStepOptions);
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