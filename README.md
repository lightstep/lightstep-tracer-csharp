# lightstep-tracer-csharp
The LightStep distributed tracing library for C#

# Installation
```
$ dotnet pack
```

This will create a .nupkg file at `./src/LightStep/bin/Debug/LightStep.1.0.0.nupkg`. You can import this into another C# project.

# Basic Usage
It's recommended to initialize the tracer once at the beginning of your application and assign it as the global tracer, as follows:
```c#
var lightStepAccessToken = "ACCESS_TOKEN";
var lightStepOptions = new Options()
var tracer = new Tracer(new SpanContextFactory(), new LightStepSpanRecorder(), new Options(lightStepAccessToken, lightStepOptions))
GlobalTracer.Register(tracer);
tracer.BuildSpan()
...
```

# Compatability Notes
If you're using .NET Standard 1.3, then you need to set the following values per-process:
```c#
LS_HOSTNAME
LS_COMPONENT
LS_COMMANDLINE
```
