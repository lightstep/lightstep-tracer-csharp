using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Google.Protobuf;
using LightStep.Logging;

namespace LightStep.Collector
{
    /// <summary>
    ///     Contains methods to communicate to a LightStep Satellite via Proto over HTTP.
    /// </summary>
    public class LightStepHttpClient
    {
        private readonly Options _options;
        private HttpClient _client;
        private readonly string _url;
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        /// <summary>
        ///     Create a new client.
        /// </summary>
        /// <param name="satelliteUrl">URL to send results to.</param>
        /// <param name="options">An <see cref="Options" /> object.</param>
        public LightStepHttpClient(string url, Options options)
        {
            _url = url;
            _options = options;
            _client = new HttpClient();
        }

        /// <summary>
        ///     Send a report of spans to the LightStep Satellite.
        /// </summary>
        /// <param name="report">An <see cref="ReportRequest" /></param>
        /// <returns>A <see cref="ReportResponse" />. This is usually not very interesting.</returns>
        public async Task<ReportResponse> SendReport(ReportRequest report)
        {
            // force net45 to attempt tls12 first and fallback appropriately
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/octet-stream"));
            var reportsByteArray = report.ToByteArray();

            var request = new HttpRequestMessage(HttpMethod.Post, _url)
            {
                Version = _options.UseHttp2 ? new Version(2, 0) : new Version(1, 1),
                Content = new ByteArrayContent(reportsByteArray)
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            
            ReportResponse responseValue = new ReportResponse();
            
            try
            {
                var response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStreamAsync();
                responseValue = ReportResponse.Parser.ParseFrom(responseData);
                _logger.Debug($"Report HTTP Response {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                _logger.WarnException("Exception caught while sending report, resetting HttpClient", ex);
                _client.Dispose();
                _client = new HttpClient();
            }
            
            return responseValue;
        }

        /// <summary>
        ///     Translate SpanData to a protobuf ReportRequest for sending to the Satellite.
        /// </summary>
        /// <param name="spans">An enumerable of <see cref="SpanData" /></param>
        /// <returns>A <see cref="ReportRequest" /></returns>
        public ReportRequest Translate(IEnumerable<SpanData> spans)
        {
            _logger.Debug($"Serializing spans to proto.");
            var timer = new Stopwatch();
            timer.Start();

            var request = new ReportRequest
            {
                Reporter = new Reporter
                {
                    ReporterId = _options.TracerGuid
                },
                Auth = new Auth {AccessToken = _options.AccessToken}
            };
            _options.Tags.ToList().ForEach(t => request.Reporter.Tags.Add(new KeyValue().MakeKeyValueFromKvp(t)));
            spans.ToList().ForEach(span => request.Spans.Add(new Span().MakeSpanFromSpanData(span)));

            timer.Stop();
            _logger.Debug($"Serialization complete in {timer.ElapsedMilliseconds}ms. Request size: {request.CalculateSize()}b.");
            return request;
        }
    }
}