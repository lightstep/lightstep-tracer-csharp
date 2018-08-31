namespace LightStep
{
    /// <summary>
    /// Constants and other values used by LightStep.
    /// </summary>
    public static class LightStepConstants
    {
        public static readonly string ParentSpanGuidKey = "parent_span_guid";
        public static readonly string GuidKey = "lightstep.guid";
        public static readonly string HostnameKey = "lightstep.hostname";
        public static readonly string ComponentNameKey = "lightstep.component_name";
        public static readonly string CommandLineKey = "lightstep.command_line";

        public static readonly string TracerPlatformKey = "lightstep.tracer_platform";
        public static readonly string TracerPlatformValue = "csharp";
        public static readonly string TracerPlatformVersionKey = "lightstep.tracer_platform_version";
        public static readonly string TracerVersionKey = "lightstep.tracer_version";

        public static readonly string SatelliteReportPath = "api/v2/reports";
    }
}
