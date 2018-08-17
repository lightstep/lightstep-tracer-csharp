using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LightStep.Collector;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using OpenTracing.Tag;
using Type = System.Type;

namespace LightStep.Collector
{    
    public class LightStepHttpClient : IDisposable
    {
        private readonly string _url;
        private readonly Options _options;

        public LightStepHttpClient(string satelliteUrl, Options options)
        {
            _url = satelliteUrl;
            _options = options;
        }
        
        public async Task<ReportResponse> SendReport(ReportRequest report)
        {
            var client = new HttpClient();
            
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            var request = new HttpRequestMessage(HttpMethod.Post, _url)
            {
                Version = new Version(2, 0),
                Content = new ByteArrayContent(report.ToByteArray()),
            };
            
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            Console.WriteLine(ReportRequest.Parser.ParseFrom(report.ToByteArray()));

            var response = client.SendAsync(request).Result;
            var responseData = await response.Content.ReadAsStreamAsync();
            return ReportResponse.Parser.ParseFrom(responseData);     
        }

        public ReportRequest Translate(IEnumerable<SpanData> spans)
        {
            var request = new ReportRequest
            {
                Reporter = new Reporter
                {
                    ReporterId = _options.TracerGuid
                },
                Auth = new Auth() {AccessToken = _options.AccessToken}
            };
            _options.Tags.ToList().ForEach(t => request.Reporter.Tags.Add(new KeyValue().MakeKeyValueFromKvp(t)));
            spans.ToList().ForEach(span => request.Spans.Add(new Span().MakeSpanFromSpanData(span)));
            return request;
        }

        public void Dispose()
        {
            
        }
    }
}