using OpenTracing.Util;
using System;
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
            var lsKey = "7462e3fc8a93e171b1524cec623700f5";
            var lsSettings = new SatelliteOptions("collector.lightstep.com");
            var lsOptions = new Options(lsKey, lsSettings);
            lsOptions.UseHttp2 = false;
            lsOptions.ReportPeriod = new TimeSpan(0, 0, 10);
            var tracer = new Tracer(lsOptions);
            
            GlobalTracer.Register(tracer);

            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
            Console.WriteLine("Hello World!");
            using (var scope = tracer.BuildSpan("main").StartActive())
            {
                Console.WriteLine(scope.Span.Context.SpanId);
                var client = new HttpWorker();
                client.Get("https://jsonplaceholder.typicode.com/todos/1");
            }

            //tracer.Flush();
            Console.ReadKey();

            // Go to http://aka.ms/dotnet-get-started-console to continue learning how to build a console app! 
        }


    }
}
