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
            var lsKey = ConfigurationManager.AppSettings["lsKey"];
            var lsSettings = new SatelliteOptions("collector.lightstep.com");
            var lsOptions = new Options(lsKey, lsSettings);
            lsOptions.UseHttp2 = false;
            lsOptions.ReportPeriod = new TimeSpan(0, 0, 10);
            var tracer = new Tracer(lsOptions);
            
            GlobalTracer.Register(tracer);

            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.

            DoThing();
            
            //tracer.Flush();
            Console.ReadKey();

            // Go to http://aka.ms/dotnet-get-started-console to continue learning how to build a console app! 
        }

        [Traceable]
        static void DoThing()
        {
            var client = new HttpWorker();
            client.Get("https://jsonplaceholder.typicode.com/todos/1");
        }

    }
}
