# lightstep-tracer-csharp
The LightStep distributed tracing library for C#

# Installation
```
$ dotnet pack
```

This will create a .nupkg file at `./src/LightStep/bin/Debug/LightStep.1.0.0.nupkg`. You can import this into another C# project.

# Basic Usage
There are two main ways to initialize the LightStep tracer, either via `using` or registering it as the global tracer.

With `using`:
```c#
var lightStepAccessToken = "ACCESS_TOKEN";
var lightStepOptions = new Options() // see LightStep.Options for more info on tuning.
using (var tracer = new Tracer(new SpanContextFactory(), new LightStepSpanRecorder(), new Options(lightStepAccessToken, lightStepOptions))
{
    tracer.BuildSpan() //see opentracing-csharp documentation for more info
    ...
}
```

With 'GlobalTracer'
```c#
var lightStepAccessToken = "ACCESS_TOKEN";
var lightStepOptions = new Options()
var tracer = new Tracer(new SpanContextFactory(), new LightStepSpanRecorder(), new Options(lightStepAccessToken, lightStepOptions))
GlobalTracer.Register(tracer);
tracer.BuildSpan()
...
```

Generally, it's preferable to create the tracer once in your applications initialization, register it as the global tracer, and use that.

