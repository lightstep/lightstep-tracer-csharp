# Changelog

_v0.4.2_
- Addresses an issue where an unhandled exception could occur if a span duration was negative.

_v0.4.1_
- Addresses an issue where we would attempt to parse all strings as JSON objects when serializing spans to the wire. We now use a more performant method to determine if an input string is JSON.

_v0.4.0_
- Addresses an issue where duplicate request headers were sent to the LightStep Satellite.
- Updated the default host and port of the Tracer.
- Removed the requirement for an access token when creating an Options object.

_v0.3.0_
- Add support for meta event reporting to the LightStep SaaS.

_v0.2.0_
- In order to align with other LightStep tracer implementations, `Inject` and `Extract` methods in `TextMapPropagator` have changed:
  - `Inject` will now convert `TraceId` and `SpanId` to a hexadecimal string in the carrier.
  - `Extract` will now convert the incoming `TraceId` and `SpanId` from a hexadecimal string into a `uint64`. 

_v0.1.2_
- Increase the verbosity level for certain frequent operations:
  - Inject/Extract logging from DEBUG to TRACE
  - Span buffer count and reset span message from DEBUG to TRACE
- Certain `LightStepHttpClient` methods were `public` rather than `internal`, this has been corrected -
  - `Translate`
  - `SendReport`

_v0.1.1_
- Guard against conversion of malformed `SpanData` by dropping bad spans.

_v0.1.0_
- `TransportOptions` are now available. You can select JSON or Binary Protobufs.
- We now build using [Cake](https://cakebuild.net).
- All value types are now properly handled in span serialization.
- The platform version detection code has been improved and should be resilient to dynamic injection scenarios.
- Logging now exists for many operations via the LibLog library. See the README for more details.
- Error handling around dropped reports has been improved, and we now report dropped spans to LightStep.
- The `LightStepHttpClient` now respects the `ReportTimeout` value. **Breaking Change**
  - Previously, this value was not respected by the HTTP client and was using the default 100s timeout.
- The `Options` class has been refactored to support a more fluent style of configuration. **Breaking Change**
  - By default, `SatelliteOptions` are now created when `Options` is created that points to the LightStep public satellite pool.
  - Please see the readme for more information on configuring the tracer.
- Tags which are passed with an empty value will now set the string value "null" when reporting to LightStep.
 
_v0.0.8_
- Addresses an issue where the `Tracer` would not `Flush` regularly.
- Addresses an issue where tags with null values would cause the tracer to not report spans.
- Addresses an issue where the `LightStepHttpClient` would stop sending spans if it received a non-success status code from the LightStep Satellite.
- *Change* The `LightStepHttpClient` will prefer HTTP/1.1 to HTTP/2. Change ths using `Options.UseHttp2`.
- The NuGet package now includes PDB files.
- The `Options` object now exposes a property `Run` that will determine if the Tracer should flush spans.

_v0.0.7_
- When instantiating a `Tracer`, you can now pass additional tags in `Options` to apply to all spans. You can also override existing LightStep span tags.

_v0.0.6_
- Addresses an issue where the signing key wasn't actually being used to sign the assembly.
- Removes unneded `net452` target framework from LightStep project.
- Reduced C# target version to 7.0 to allow builds on unpatched VS2017.

_v0.0.5_
- Addresses an issue where incoming headers using base16 would cause exceptions on conversion.

_v0.0.4_
- Expose a new option to force HTTP/1.1 rather than attempting to use HTTP/2 (`Options.UseHttp2`) which defaults to `true`.
- The LightStep Assembly is now signed via Strong Naming.
- Addresses an issue where OpenTracing Header key values were not set correctly.
- Addresses an issue where span duration was improperly reported as ticks rather than microseconds.
- Addresses an issue where on older .NET versions, secure connections would fail due to TLS negotiation. We now force all versions to attempt to negotiate TLS 1.2, then fall back.
- Addresses an issue where rapid creation of Spans in a short timeframe belonging to the same Trace could result in Span ID collisions.

_v0.0.3_
- Addresses an issue where we would always try to connect to the Satellite via `http`, even if `usePlaintext` was set to `false`.

_v0.0.2_ 
- Add support for parsing B3 Headers (such as those used by Zipkin)
- Support `net45` and `netstandard2`

_v0.0.1_
- Initial Release
