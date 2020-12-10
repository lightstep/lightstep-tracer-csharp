using System;
using System.Collections.Generic;
using System.Linq;
using LightStep;
using OpenTracing;

namespace LightStep.TracerPerf.Tests
{
    public class TracerTestBase
    {
        protected string Host { get; set; } = Environment.GetEnvironmentVariable("LS_HOST") ?? "localhost" ;
        protected int Port { get; set; } = 8360;
        protected double ReportPeriod { get; set; } = .5;
        protected int BufferSize { get; set; } = 200;
        protected string Token { get; set; } = Environment.GetEnvironmentVariable("LS_KEY") ?? "developer";
        protected long Iter { get; set; } = 10;
        protected long Chunk { get; set; } = 10;
        protected Tracer Tracer;

        protected static readonly Action<ITracer> NoFinishNoDispose = t => t.BuildSpan("test").StartActive(false);

        protected static Action<ITracer> ExplicitFinish => t =>
        {
            t.BuildSpan("test").StartActive(true);
            t.ActiveSpan.Finish();
        };

        protected static readonly Action<ITracer> FinishOnDispose = t =>
        {
            var scope = t.BuildSpan("test").StartActive(true);
            scope.Dispose();
        };

        protected static readonly Action<ITracer> DisposeNoFinish = t =>
        {
            var scope = t.BuildSpan("test").StartActive(false);
            scope.Dispose();
        };

        protected readonly Dictionary<string, Action<ITracer>> TracingMethods = new Dictionary<string, Action<ITracer>>()
        {
            { "NoFinishNoDispose", NoFinishNoDispose },
            { "ExplicitFinish", ExplicitFinish },
            { "FinishOnDispose", FinishOnDispose },
            { "DisposeNoFinish", DisposeNoFinish },
        };

        private void Init()
        {
            var overrideTags = new Dictionary<string, object>
            {
                {
                    LightStepConstants.ComponentNameKey, "ServiceName"
                },
            };
            if (Environment.GetEnvironmentVariable("LS_PORT") != null)
            {
                Port = int.Parse(Environment.GetEnvironmentVariable("LS_PORT"));
            }

            var usePlaintext = true;
            if (Environment.GetEnvironmentVariable("LS_USE_PLAINTEXT") != null)
            {
                usePlaintext = false;
            }
            var satelliteOptions = new SatelliteOptions(Host, Port, usePlaintext);
            Options options = new Options(Token)
                .WithSatellite(satelliteOptions)
                .WithTags(overrideTags)
                .WithMaxBufferedSpans(BufferSize)
                .WithReportPeriod(TimeSpan.FromSeconds(ReportPeriod));
            Tracer = new Tracer(options);
        }

        protected List<long> Execute(Action<ITracer> buildSpan)
        {
            Init();
            var heapInfo = new List<long>();
            for (var i = 0; i < Iter; i++)
            {
                for (var j = 0; j < Chunk; j++)
                {
                    buildSpan(Tracer);
                }
                var gcMemoryInfo = GC.GetTotalMemory(true);
                heapInfo.Add(gcMemoryInfo);
                Console.WriteLine(gcMemoryInfo);
                Tracer.Flush();
            }

            Tracer = null;
            var min = Enumerable.Min(heapInfo);
            return heapInfo.Select(e => e - min).ToList();
        }
    }
}