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
        public const string BaggagePrefix = "ot-bg-";

        /// <summary>
        ///     OpenTracing TraceId Prefix
        /// </summary>
        public const string TraceId = "ot-traceid";

        /// <summary>
        ///     OpenTracing SpanId Prefix
        /// </summary>
        public const string SpanId = "ot-spanid";
    }
}