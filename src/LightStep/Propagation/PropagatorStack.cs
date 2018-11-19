using System;
using System.Collections.Generic;
using LightStep.Logging;
using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    /// <inheritdoc />
    public class PropagatorStack : IPropagator
    {
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        /// <inheritdoc />
        public PropagatorStack(IFormat<ITextMap> format)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));
            _logger.Trace($"Creating new PropagatorStack with format {format}");
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
                {
                    _logger.Debug($"Injecting context to {propagator.GetType()}");
                    propagator.Inject(context, format, carrier);
                }
            );
        }

        /// <inheritdoc />
        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            for (var i = Propagators.Count - 1; i >= 0; i--)
            {
                _logger.Debug($"Trying to extract from {Propagators[i].GetType()}");
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
            _logger.Trace($"Adding {propagator.GetType()} to PropagatorStack.");
            Propagators.Add(propagator);
            return this;
        }
    }
}