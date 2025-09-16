using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.Net.Http.Headers;

namespace AIKernelClient.Services
{

    public sealed class BearerHandler : DelegatingHandler
    {
        private readonly string _token;

        public BearerHandler(string token, HttpMessageHandler? innerHandler = null)
        {
            _token = token ?? throw new ArgumentNullException(nameof(token));
            // ensure an inner handler exists
            InnerHandler = innerHandler ?? new HttpClientHandler();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            // set/overwrite the Authorization header for every request
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return base.SendAsync(req, ct);
        }
    }

}
