using System;
using System.Collections.Generic;
using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    /// <inheritdoc />
    public class PropagatorStack : IPropagator
    {
        /// <inheritdoc />
        public PropagatorStack(IFormat<ITextMap> format)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));

            Format = format;
            Propagators = new List<IPropagator>();
        }

        /// <summary>
        ///     The format of the propagators
        /// </summary>
        public IFormat<ITextMap> Format { get; }

        /// <summary>
        ///     A list of propagators to attempt to match.
        /// </summary>
        public List<IPropagator> Propagators { get; }

        /// <inheritdoc />
        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            Propagators.ForEach(propagator =>
                propagator.Inject(context, format, carrier)
            );
        }

        /// <inheritdoc />
        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            for (var i = Propagators.Count - 1; i >= 0; i--)
            {
                var context = Propagators[i].Extract(format, carrier);
                if (context != null) return context;
            }

            return null;
        }

        /// <summary>
        ///     Add a new propagator to a stack.
        /// </summary>
        /// <param name="propagator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PropagatorStack AddPropagator(IPropagator propagator)
        {
            if (propagator == null) throw new ArgumentNullException(nameof(propagator));

            Propagators.Add(propagator);
            return this;
        }
    }
}