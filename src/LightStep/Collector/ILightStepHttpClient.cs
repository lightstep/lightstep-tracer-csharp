using System.Net.Http;
using System.Threading.Tasks;

namespace LightStep.Collector
{
    public interface ILightStepHttpClient
    {
        /// <summary>
        ///     Send a report of spans to the LightStep Satellite.
        /// </summary>
        /// <param name="report">An <see cref="ReportRequest" /></param>
        /// <returns>A <see cref="ReportResponse" />. This is usually not very interesting.</returns>
        Task<ReportResponse> SendReport(ReportRequest report);
    }
}