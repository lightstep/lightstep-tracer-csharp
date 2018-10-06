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
        public override void OnEntry(MethodExecutionArgs args)
        {
            // when we enter a method, build a new span and mark it as active
            var scope = GlobalTracer.Instance.BuildSpan(args.Method.Name).StartActive();
            Console.WriteLine($"new span id {scope.Span.Context.SpanId}");
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            // only finish a span in OnExit, as it gets called as a finalizer
            Console.WriteLine("The {0} method executed successfully.", args.Method.Name);
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            // when a method exits (either successfully or unsuccessfully), dispose of the scope to free it on our singleton tracer
            var scope = GlobalTracer.Instance.ScopeManager.Active;
            Console.WriteLine($"finishing span id {scope.Span.Context.SpanId}");
            scope.Dispose();
        }

        public override void OnException(MethodExecutionArgs args)
        {
            // if an exception occurs, get the active scope then set the exception log, add tags, whatever.
            var scope = GlobalTracer.Instance.ScopeManager.Active;
            Console.WriteLine("An exception was thrown in {0}.", args.Method.Name);
            scope.Span.Log(args.Exception.StackTrace);         
        }  
    }
}
