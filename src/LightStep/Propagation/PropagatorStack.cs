using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTracing.Propagation;

namespace LightStep.Propagation
{
    /// <inheritdoc />
    public class PropagatorStack : IPropagator
    {
        public IFormat<ITextMap> Format { get; }
        public List<IPropagator> Propagators { get; private set; }
        
        public PropagatorStack(IFormat<ITextMap> format)
        {
            if (format == null)
            {
                throw new ArgumentNullException(nameof(format));
            }

            Format = format;
            Propagators = new List<IPropagator>();
        }

        public PropagatorStack AddPropagator(IPropagator propagator)
        {
            if (propagator == null)
            {
                throw new ArgumentNullException(nameof(propagator));
            }
            
            Propagators.Add(propagator);
            return this;
        }
        
        public void Inject<TCarrier>(SpanContext context, IFormat<TCarrier> format, TCarrier carrier)
        {
            Propagators.ForEach(propagator => propagator.Inject(context, format, carrier));
        }

        public SpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
        {
            for (int i = Propagators.Count - 1; i >= 0; i--)
            {
                var context = Propagators[i].Extract(format, carrier);
                if (context != null)
                {
                    return context;
                }
            }

            return null;
        }
    }
}