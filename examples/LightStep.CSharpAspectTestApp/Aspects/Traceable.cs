using OpenTracing.Util;
using PostSharp.Aspects;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightStep.CSharpAspectTestApp.Aspects
{
    [PSerializable]
    public sealed class Traceable : OnMethodBoundaryAspect
    {
        OpenTracing.IScope scope;
        public override void OnEntry(MethodExecutionArgs args)
        {
            Console.WriteLine($"method {args.Method.Name} is being called");
            scope = GlobalTracer.Instance.BuildSpan(args.Method.Name).StartActive();
            Console.WriteLine($"new span id {scope.Span.Context.SpanId}");
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            Console.WriteLine("The {0} method executed successfully.", args.Method.Name);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            Console.WriteLine($"finishing span id {scope.Span.Context.SpanId}");
            scope.Span.Finish();
        }

        public override void OnException(MethodExecutionArgs args)
        {
            scope.Span.Log(args.Exception.StackTrace);
            Console.WriteLine("An exception was thrown in {0}.", args.Method.Name);
        }
        /**
        public object CreateInstance(AdviceArgs adviceArgs)
        {
            return this.MemberwiseClone();
        }

        public void RuntimeInitializeInstance()
        {
            
        }
    */
    }
}
