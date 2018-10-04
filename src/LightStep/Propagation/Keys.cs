namespace LightStep.Propagation
{
    /// <summary>
    ///     Key prefixes used in baggage/traces/etc.
    /// </summary>
    public static class Keys
    {
        /// <summary>
        ///     OpenTracing Baggage Prefix
        /// </summary>
        public const string BaggagePrefix = "ot-baggage-";

        /// <summary>
        ///     OpenTracing TraceId Prefix
        /// </summary>
        public const string TraceId = "ot-tracer-traceid";

        /// <summary>
        ///     OpenTracing SpanId Prefix
        /// </summary>
        public const string SpanId = "ot-tracer-spanid";

        /// <summary>
        ///     OpenTracing Sampled Prefix
        /// </summary>
        public const string SampledId = "ot-tracer-sampled";
    }
}