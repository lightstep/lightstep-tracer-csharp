using LightStep.CSharpAspectTestApp.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LightStep.CSharpAspectTestApp
{
    class HttpWorker
    {
        private HttpClient httpClient;

        public HttpWorker()
        {
            httpClient = new HttpClient();
        }
        
        [Traceable]
        public async void Get(string url)
        {
            var content = await GetString(url);
            Write(content);
        }

        [Traceable]
        public static void Write(string content)
        {
            Console.WriteLine(content);
        }

        [Traceable]
        public async Task<string> GetString(string url)
        {
            var content = await httpClient.GetStringAsync(url);
            return content;
        }
    }
}
