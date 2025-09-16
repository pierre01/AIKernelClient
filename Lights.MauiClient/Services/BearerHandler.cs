using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIKernelClient.Services
{
    public sealed class BearerHandler : DelegatingHandler
    {
        private readonly string _token;
        public BearerHandler(string token) => _token = token;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            return base.SendAsync(req, ct);
        }
    }
}
