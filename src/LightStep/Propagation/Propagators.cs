namespace LightStep.Propagation
{
    /// <summary>
    /// Static references to each propagator available.
    /// </summary>
    public static class Propagators
    {
        /// <summary>
        /// Writes context values to the console, used for local development.
        /// </summary>
        public static readonly IPropagator Console = new ConsolePropagator();
        /// <summary>
        /// A key:value store for string pairs. 
        /// </summary>
        public static readonly IPropagator TextMap = new TextMapPropagator();
    }
}
