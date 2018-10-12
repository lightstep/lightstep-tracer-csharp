# lightstep-tracer-csharp
The LightStep distributed tracing library for C#

[![NuGet](https://img.shields.io/nuget/v/LightStep.svg)](https://www.nuget.org/packages/LightStep) [![CircleCI](https://circleci.com/gh/lightstep/lightstep-tracer-csharp.svg?style=svg)](https://circleci.com/gh/lightstep/lightstep-tracer-csharp)

# Development Requirements
- C# 7
- .NET Core 2+
- .NET 4.5

This translates to requiring at least Visual Studio 2017 (15.0).
You may need [PostSharp](https://www.postsharp.net/) to work on the `LightStep.CSharpAspectTestApp`.

# Installation
Install the package via NuGet into your solution, or use `Install-Package LightStep`.

# Basic Usage
It's recommended to initialize the tracer once at the beginning of your application and assign it as the global tracer, as follows:
```c#
// substitute your own LS API Key here
var lsKey = "TEST_TOKEN";

// substitute your satellite endpoint (host, port) here.
var lsSettings = new SatelliteOptions("localhost", 9996, false);

// create a new tracer and register it
var tracer = new Tracer(new Options(lsKey, lsSettings));
GlobalTracer.Register(tracer);
...
```

Once you've initialized a tracer, you can begin to create and emit spans.

Please refer to the [OpenTracing C# documentation](https://github.com/opentracing/opentracing-csharp) for information on how to create spans.

You can also refer to the `examples` folder for sample code and projects. 




