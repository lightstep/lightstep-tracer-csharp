using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using LightStep.Logging;

namespace LightStep.Collector
{
    class ReportTranslator : IReportTranslator
    {
        private readonly Options _options;
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        public ReportTranslator(Options options)
        {
            _options = options;
        }

        /// <summary>
        ///     Translate SpanData to a protobuf ReportRequest for sending to the Satellite.
        /// </summary>
        /// <param name="spans">An enumerable of <see cref="SpanData" /></param>
        /// <returns>A <see cref="ReportRequest" /></returns>
        public ReportRequest Translate(ISpanRecorder spanBuffer)
        {
            _logger.Trace($"Serializing {spanBuffer.GetSpans().Count()} spans to proto.");
            var timer = new Stopwatch();
            timer.Start();

            var request = new ReportRequest
            {
                Reporter = new Reporter
                {
                    ReporterId = _options.TracerGuid
                },
                Auth = new Auth { AccessToken = _options.AccessToken }
            };
            _options.Tags.ToList().ForEach(t => request.Reporter.Tags.Add(new KeyValue().MakeKeyValueFromKvp(t)));
            spanBuffer.GetSpans().ToList().ForEach(span => {
                try
                {
                    request.Spans.Add(new Span().MakeSpanFromSpanData(span));
                }
                catch (Exception ex)
                {
                    _logger.WarnException("Caught exception converting spans.", ex);
                    spanBuffer.DroppedSpanCount++;
                }
            });

            var metrics = new InternalMetrics
            {
                StartTimestamp = Timestamp.FromDateTime(spanBuffer.ReportStartTime.ToUniversalTime()),
                DurationMicros = Convert.ToUInt64((spanBuffer.ReportEndTime - spanBuffer.ReportStartTime).Ticks / 10),
                Counts = { new MetricsSample() { Name = "spans.dropped", IntValue = spanBuffer.DroppedSpanCount } }
            };
            request.InternalMetrics = metrics;

            timer.Stop();
            _logger.Trace($"Serialization complete in {timer.ElapsedMilliseconds}ms. Request size: {request.CalculateSize()}b.");

            return request;
        }
    }
}
