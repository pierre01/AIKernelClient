using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public sealed class ApiKeyAuthHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public ApiKeyAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) // 
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var hdr))
            return Task.FromResult(AuthenticateResult.NoResult());

        var parts = hdr.ToString().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            var token = parts[1];
            var expected = Environment.GetEnvironmentVariable("MCP_API_KEY");
            if (!string.IsNullOrWhiteSpace(expected) && token == expected)
            {
                var id = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "chatgpt-connector") }, Scheme.Name);
                var ticket = new AuthenticationTicket(new ClaimsPrincipal(id), Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
        return Task.FromResult(AuthenticateResult.Fail("Invalid or missing bearer token."));
    }
}
