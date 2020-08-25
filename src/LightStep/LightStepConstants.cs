namespace LightStep
{
    /// <summary>
    ///     Constants and other values used by LightStep.
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

        public static readonly string AccessTokenConstant = "Lightstep-Access-Token";

        public static class MetaEvent {
            public static readonly string MetaEventKey = "lightstep.meta_event";
            public static readonly string PropagationFormatKey = "lightstep.propagation_format";
            public static readonly string TraceIdKey = "lightstep.trace_id";
            public static readonly string SpanIdKey = "lightstep.span_id";
            public static readonly string TracerGuidKey = "lightstep.tracer_guid";
            public static readonly string ExtractOperation = "lightstep.extract_span";
            public static readonly string InjectOperation = "lightstep.inject_span";
            public static readonly string SpanStartOperation = "lightstep.span_start";
            public static readonly string SpanFinishOperation = "lightstep.span_finish";
            public static readonly string TracerCreateOperation = "lightstep.tracer_create";
        }
        
    }
}