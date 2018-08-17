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
            var lsKey = "49e45413c56dec0372b1ed47cf92315f";
            var lsSettings = ("localhost", 9996, false);
            var tracer = new Tracer(new SpanContextFactory(), new LightStepSpanRecorder(), new Options(lsKey, lsSettings));
            GlobalTracer.Register(tracer);

            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine("starting a sample span");
                var parentSpan = tracer.BuildSpan("outer").Start();
                var span = tracer.BuildSpan("sample").AsChildOf(parentSpan).Start();

                span.Log($"iteration {i}");
                Thread.Sleep(new Random().Next(0, 50));
                span.SetTag("testapp", "true");
        
                Console.WriteLine("span should be finished");
                span.Finish();
                parentSpan.Finish(); 
                tracer.Flush();
            }
            
            // TODO: locking NYI so this will crash!
            using (IScope scope = tracer.BuildSpan("work").StartActive(finishSpanOnDispose: true))
            {
               Tags.Error.Set(scope.Span, true);
                scope.Span.Log("error!");
            }
        }
    }
}