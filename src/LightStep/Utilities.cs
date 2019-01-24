using System;

namespace LightStep
{
    /// <summary>
    ///     Utilities and other helpers.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        ///     Get a random uint64.
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static ulong NextUInt64(this Random rand)
        {
            var buffer = new byte[sizeof(ulong)];
            rand.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public static bool IsNotMetaSpan(Span span) 
        {
            return span.Tags.ContainsKey("lightstep.meta_event");
        }
    }
}