using System;
using System.IO;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using OpenTracing.Util;
using global::Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole;

namespace LightStep.CSharpTestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            var tracer = new Tracer(new Options());
            GlobalTracer.Register(tracer);
            
            for (var i = 0; i < 500; i++)
                using (var scope = tracer.BuildSpan("testParent").WithTag("testSpan", "true").StartActive(true))
                {
                    scope.Span.Log("test");
                    tracer.ActiveSpan.Log($"iteration {i}");
                    
                    Thread.Sleep(new Random().Next(5, 10));
                    var innerSpan = tracer.BuildSpan("childSpan").Start();
                    innerSpan.SetTag("innerTestTag", "true");
                    
                    Thread.Sleep(new Random().Next(10, 20));
                    innerSpan.Finish();
                }
            tracer.Flush();
            Console.ReadKey();
        }
        
    }
}