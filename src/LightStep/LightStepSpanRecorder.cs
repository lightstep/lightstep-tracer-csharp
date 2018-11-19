using System;
using System.Collections.Generic;
using System.Linq;
using LightStep.Logging;

namespace LightStep
{
    /// <inheritdoc />
    public sealed class LightStepSpanRecorder : ISpanRecorder
    {
        private List<SpanData> Spans { get; } = new List<SpanData>();
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        /// <inheritdoc />
        public void RecordSpan(SpanData span)
        {
            _logger.Trace($"Waiting for lock in SpanRecorder.");
            lock (Spans)
            {
                _logger.Trace($"Lock freed, adding new span: {span}");
                Spans.Add(span);    
            }
            
        }

        /// <inheritdoc />
        public List<SpanData> GetSpanBuffer()
        {
            _logger.Trace("Waiting for lock in SpanRecorder");
            lock (Spans)
            {
                _logger.Trace("Lock freed, getting span buffer.");
                var currentSpans = Spans.ToList();
                Spans.Clear();
                return currentSpans;
            }
            
        }

        /// <inheritdoc />
        public void ClearSpanBuffer()
        {
            _logger.Trace("Waiting for lock in SpanRecorder.");
            lock (Spans)
            {
                _logger.Trace("Lock freed, clearing span buffer.");
                Spans.Clear();    
            }
            
        }
    }
}