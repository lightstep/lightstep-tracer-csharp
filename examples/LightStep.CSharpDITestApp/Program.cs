using System;
using System.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using OpenTracing;
using LightStep;

namespace LightStep.CSharpDITestApp
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // replace these options with your satellite and api key
            var tracerOptions = new Options("TEST_TOKEN").WithSatellite(new SatelliteOptions("localhost", 9996, true));
            var container = new WindsorContainer();
            // during registration, pass your options to the concrete LightStep Tracer implementation
            container.Register(Component.For<ITracer>().ImplementedBy<Tracer>().DependsOn(Dependency.OnValue("options", tracerOptions)));            
            var tracer = container.Resolve<ITracer>();
            
            // create some spans
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
            
            // note that OpenTracing.ITracer does not have flush as a method, so to manually flush you'll need to
            // get a cast of the tracer. 
            Tracer t = (Tracer) tracer;
            t.Flush();
        }
    }
}