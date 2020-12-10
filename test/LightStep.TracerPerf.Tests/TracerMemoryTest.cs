using System;
using NUnit.Framework;

namespace LightStep.TracerPerf.Tests
{
    [SingleThreaded]
    public class TracerMemoryTest : TracerTestBase
    {
        [SetUp]
        public void SetUp()
        {
            Iter = 100;
            Chunk = 10000;
        }

        [Test]
        public void TestExecute_NoFinishNoDispose()
        {
            Console.WriteLine(nameof(NoFinishNoDispose));
            var heapInfo = Execute(NoFinishNoDispose);
            foreach (var num in heapInfo)
            {
                Console.WriteLine(num);
            }
        }

        [Test]
        public void TestExecute_ExplicitFinish()
        {
            Console.WriteLine(nameof(ExplicitFinish));
            var heapInfo = Execute(ExplicitFinish);
            foreach (var num in heapInfo)
            {
                Console.WriteLine(num);
            }
        }

        [Test]
        public void TestExecute_FinishOnDispose()
        {
            Console.WriteLine(nameof(FinishOnDispose));
            var heapInfo = Execute(FinishOnDispose);
            foreach (var num in heapInfo)
            {
                Console.WriteLine(num);
            }
        }

        [Test]
        public void TestExecute_DisposeNoFinish()
        {
            Console.WriteLine(nameof(DisposeNoFinish));
            var heapInfo = Execute(DisposeNoFinish);
            foreach (var num in heapInfo)
            {
                Console.WriteLine(num);
            }
        }
    }
}