namespace LightStep.Propagation
{
    /// <summary>
    ///     Static references to each propagator available.
    /// </summary>
    public static class Propagators
    {
        /// <summary>
        ///     Writes context values to the console, used for local development.
        /// </summary>
        public static readonly IPropagator Console = new ConsolePropagator();

        /// <summary>
        ///     A key:value store for string pairs.
        /// </summary>
        public static readonly IPropagator TextMap = new TextMapPropagator();
        
        /// <summary>
        ///    Supports B3 headers, such as those used in Zipkin or StageMonitor.
        /// </summary>
        public static readonly IPropagator B3Propagator = new B3Propagator();
        
        /// <summary>
        ///     Supports HTTP Header Propagation
        /// </summary>
        public static readonly IPropagator HttpHeadersPropagator = new HttpHeadersPropagator();
    }
}