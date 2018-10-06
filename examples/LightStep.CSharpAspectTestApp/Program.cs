using LightStep.CSharpAspectTestApp.Aspects;
using OpenTracing.Util;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightStep.CSharpAspectTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // create your tracer options, initialize it, assign it to the GlobalTracer
            var lsKey = ConfigurationManager.AppSettings["lsKey"];
            var lsSettings = new SatelliteOptions("collector.lightstep.com");
            var lsOptions = new Options(lsKey, lsSettings);
            lsOptions.UseHttp2 = false;
            var tracer = new Tracer(lsOptions);
            
            GlobalTracer.Register(tracer);

            // do some work in parallel, this work also includes awaited calls
            Parallel.For(1, 100, i => DoThing(i));

            // block until you enter a key
            Console.ReadKey();
        }

        [Traceable]
        static void DoThing(int idx)
        {
            var client = new HttpWorker();
            client.Get($"https://jsonplaceholder.typicode.com/todos/{idx}");
        }

    }
}
