using OpenTracing;

namespace LightStep
{
    /// <summary>
    ///     Extensions methods for retrieving a span context.
    /// </summary>
    public static class TypedContextExtensions
    {
        /// <summary>
        ///     Get a strongly typed <see cref="SpanContext" /> from a <see cref="ISpan" />
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static SpanContext TypedContext(this ISpan span)
        {
            return (SpanContext) span?.Context;
        }

        /// <summary>
        ///     Get a strongly typed <see cref="SpanContext" /> from an <see cref="ISpanContext" />
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static SpanContext TypedContext(this ISpanContext context)
        {
            return (SpanContext) context;
        }
    }
}