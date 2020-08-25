namespace LightStep.Collector
{
    public interface IReportTranslator
    {
        /// <summary>
        ///     Translate SpanData to a protobuf ReportRequest for sending to the Satellite.
        /// </summary>
        /// <param name="spans">An enumerable of <see cref="SpanData" /></param>
        /// <returns>A <see cref="ReportRequest" /></returns>
        ReportRequest Translate(ISpanRecorder spanBuffer);
    }
}