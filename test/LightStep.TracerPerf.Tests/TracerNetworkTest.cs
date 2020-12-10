using System;
using System.Threading;
using NUnit.Framework;

namespace LightStep.TracerPerf.Tests
{
    [SingleThreaded]
    public class TracerNetworkTest : TracerTestBase
    {
        [SetUp]
        public void SetUp()
        {
            Iter = 10000;
            Chunk = 1000;
            BufferSize = 2000;
            ReportPeriod = .5;
        }

        [Test, Order(1)]
        public void TestExecute_Localhost()
        {
            Host = "localhost";
            Execute(ExplicitFinish);
            Thread.Sleep(TimeSpan.FromSeconds(20));
        }

        [Test, Order(2)]
        public void TestExecute_GarbageHost()
        {
            Host = "garbage";
            Execute(FinishOnDispose);
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }
    }
}