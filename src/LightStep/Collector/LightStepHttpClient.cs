using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Google.Protobuf;

namespace LightStep.Collector
{
    /// <summary>
    ///     Contains methods to communicate to a LightStep Satellite via Proto over HTTP.
    /// </summary>
    public class LightStepHttpClient
    {
        private readonly Options _options;
        private readonly HttpClient _client;
        private readonly string _url;

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
        public ReportResponse SendReport(ReportRequest report)
        {
            
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var request = new HttpRequestMessage(HttpMethod.Post, _url)
            {
                Version = _options.UseHttp2 ? new Version(1, 1) : new Version(2, 0),
                Content = new ByteArrayContent(report.ToByteArray())
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = _client.SendAsync(request).Result;
            var responseData = response.Content.ReadAsStreamAsync().Result;
            return ReportResponse.Parser.ParseFrom(responseData);
        }

        /// <summary>
        ///     Translate SpanData to a protobuf ReportRequest for sending to the Satellite.
        /// </summary>
        /// <param name="spans">An enumerable of <see cref="SpanData" /></param>
        /// <returns>A <see cref="ReportRequest" /></returns>
        public ReportRequest Translate(IEnumerable<SpanData> spans)
        {
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
            return request;
        }
    }
}