# lightstep-tracer-csharp [Deprecated]

> ❗ **This instrumentation is no longer recommended**. Please review [documentation on setting up and configuring the OpenTelemetry client for .NET](https://github.com/open-telemetry/opentelemetry-dotnet) for more information on migrating.

In August 2023, [Lightstep became ServiceNow
Cloud](https://docs.lightstep.com/docs/banner-faq) Observability. To ease the
transition, all code artifacts will continue to use the Lightstep name. You
don't need to do anything to keep using this repository.

The LightStep distributed tracing library for C#

[![NuGet](https://img.shields.io/nuget/v/LightStep.svg)](https://www.nuget.org/packages/LightStep) [![CircleCI](https://circleci.com/gh/lightstep/lightstep-tracer-csharp.svg?style=svg)](https://circleci.com/gh/lightstep/lightstep-tracer-csharp) [![codecov](https://codecov.io/gh/lightstep/lightstep-tracer-csharp/branch/master/graph/badge.svg)](https://codecov.io/gh/lightstep/lightstep-tracer-csharp)

# Installation
Install the package via NuGet into your solution, or use `Install-Package LightStep` / `dotnet add package LightStep`.

# Basic Usage
It's recommended to initialize the tracer once at the beginning of your application and assign it as the global tracer, as follows:
```c#
var tracerOptions = new Options();
var tracer = new Tracer(tracerOptions);
GlobalTracer.Register(tracer);
```

Once you've initialized a tracer, you can begin to create and emit spans.

Please refer to the [OpenTracing C# documentation](https://github.com/opentracing/opentracing-csharp) for information on how to create spans.

You can also refer to the `examples` folder for sample code and projects. 

# Advanced Usage

There's several options that can be adjusted when instantiating a `LightStepTracer`.

## `Options`
| Method | Description |
| -------- | ----------- |
| WithTags(IDictionary<string, object>)   | Default tags to apply to all spans created by the tracer.  |
| WithReportPeriod(TimeSpan)  | How frequently the Tracer should batch and send Spans to LightStep (5s default) |
| WithReportTimeout(TimeSpan)  | Timeout for sending spans to the Satellite (30s default)  |
| WithToken(string) | The LightStep Project Access Token |
| WithSatellite(SatelliteOptions) | A SatelliteOptions object that specifies the host, port, and if we should use HTTPS |
| WithHttp2(bool) | If this is true, we use HTTP/2 to communicate with the Satellite. We reccomend you enable this option if you're on a modern version of .NET (4.6.1+ or .NET Core) |
| WithAutomaticReporting(bool) | If false, disables the automatic flushing of buffered spans. |
| WithMaxBufferedSpans(int) | The maximum amount of spans to record in a single buffer. |
| WithTransport(enum) | Which transport to use when sending spans to the Satellite. |

## `SatelliteOptions`
| Property | Description |
| -------- | ----------- |
| SatelliteHost | The hostname of a Satelite (i.e., `collector.lightstep.com`)
| SatellitePort | The port number where the Satellite is listening for HTTP traffic (defaults to 443)
| UsePlaintext | Should we use HTTP or HTTPS traffic? (Defaults to HTTPS)

The C# Tracer will prefer TLS 1.2 when available on all .NET Runtime versions, but should fall back to TLS 1.1 or 1.0 in that order.

The following is an example of overriding the LightStep Component Name and adding a new custom tag for all spans -

```csharp
var satelliteOptions = new SatelliteOptions("satellite.mydomain.com");
var overrideTags = new Dictionary<string, object> 
{
    {LightStepConstants.ComponentNameKey, "test_component"},
    {"my_tag", "foobar"}
};
var tracerOptions = new Options("TEST_TOKEN").WithSatellite(satelliteOptions).WithTags(overrideTags);
var tracer = new Tracer(tracerOptions);
```

## Logging
This tracer uses [LibLog](https://github.com/damianh/LibLog), a transparent logging abstraction that provides built-in support for NLog, Log4Net, Serilog, and Loupe.
If you use a logging provider that isn't identified by LibLog, see [this gist](https://gist.github.com/damianh/fa529b8346a83f7f49a9) on how to implement a custom logging provider.

## Integration Notes
You may notice that there's a lot of overloads for creating a `Tracer`! You have the flexibility to override and re-implement much of this library. In 0.10.0+, the `ILightStepHttpClient` interface has
been decoupled from report translation, allowing you control over the exact mechanism by which the tracer reports spans to Lightstep. You can reference [this issue](https://github.com/lightstep/lightstep-tracer-csharp/issues/92)
for more information and a discussion about why you might want to do this.

For most users, sticking with the defaults is fine. You should also look at the `Tracer(Options, IPropagator, ILightStepHttpClient)` ctor for custom integration - the span recorder and span translator overloads are not terribly interesting.
Creating and managing propagators allows you to either select a built-in propagator (textmap, B3, etc.), create a propagator 'stack' (if you potentially have multiple input or output trace context formats), or write your own propagator.
