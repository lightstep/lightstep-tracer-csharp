using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Google.Protobuf;
using LightStep.Logging;
using Google.Protobuf.WellKnownTypes;

namespace LightStep.Collector
{
    /// <summary>
    ///     Contains methods to communicate to a LightStep Satellite via Proto over HTTP.
    /// </summary>
    public class LightStepHttpClient : ILightStepHttpClient
    {
        private readonly Options _options;
        private HttpClient _client;
        private readonly string _url;
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        ///     Create a new client.
        /// </summary>
        /// <param name="url">URL to send results to.</param>
        /// <param name="options">An <see cref="Options" /> object.</param>
        /// <param name="client">An <see cref="HttpClient" /> object. If none is provide, a default client will be used.</param>
        public LightStepHttpClient(string url, Options options)
        {
            _url = url;
            _options = options;
            _client = new HttpClient() {Timeout = _options.ReportTimeout};
        }

        internal HttpRequestMessage CreateStringRequest(ReportRequest report)
        {
            var jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true));
            var jsonReport = jsonFormatter.Format(report);
            var request = new HttpRequestMessage(HttpMethod.Post, _url)
            {
                Version = _options.UseHttp2 ? new Version(2, 0) : new Version(1, 1),
                Content = new StringContent(jsonReport)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        internal HttpRequestMessage CreateBinaryRequest(ReportRequest report)
        {
            var binaryReport = report.ToByteArray();
            var request = new HttpRequestMessage(HttpMethod.Post, _url)
            {
                Version = _options.UseHttp2 ? new Version(2, 0) : new Version(1, 1),
                Content = new ByteArrayContent(binaryReport)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return request;
        }

        internal HttpRequestMessage BuildRequest(ReportRequest report)
        {
            HttpRequestMessage requestMessage = (_options.Transport & TransportOptions.JsonProto) != 0 ? CreateStringRequest(report) : CreateBinaryRequest(report);

            // add LightStep access token to request header
            requestMessage.Content.Headers.Add(LightStepConstants.AccessTokenConstant, report.Auth.AccessToken);

            return requestMessage;

        }

        /// <summary>
        ///     Send a report of spans to the LightStep Satellite.
        /// </summary>
        /// <param name="report">An <see cref="ReportRequest" /></param>
        /// <returns>A <see cref="ReportResponse" />. This is usually not very interesting.</returns>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        public async Task<ReportResponse> SendReport(ReportRequest report)
        {
            // force net45 to attempt tls12 first and fallback appropriately
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            
            var requestMessage = BuildRequest(report);

            ReportResponse responseValue;

            try
            {
                var response = await _client.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                responseValue = ReportResponse.Parser.ParseFrom(responseData);
                _logger.Trace($"Report HTTP Response {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                _logger.WarnException("Exception caught while sending report, resetting HttpClient", ex);
                _client.Dispose();
                _client = new HttpClient {Timeout = _options.ReportTimeout};
                throw;
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                _logger.WarnException("Timed out sending report to satellite", ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.WarnException("Unknown error sending report.", ex);
                throw;
            }
            
            return responseValue;
        }
    }
}