# Changelog
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
