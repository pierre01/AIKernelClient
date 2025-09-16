// Program.cs — .NET 8, HTTPS, MCP over HTTP transport, API-key Bearer auth on /mcp

using Lights.McpServer;
using LightsAPICommon;
using LightsAPICommon.Serialization;
using Microsoft.Extensions.AI;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

// + auth usings
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

internal partial class Program
{
    [RequiresUnreferencedCode("Calls Microsoft.Extensions.DependencyInjection.McpServerBuilderExtensions.WithToolsFromAssembly(Assembly, JsonSerializerOptions)")]
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        // --- HTTPS ---
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(3001, listenOptions =>
            {
                listenOptions.UseHttps(); // HTTPS
            });
        });

        // --- JSON / source-gen context ---
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, LightsJsonContext.Default);
        });

        // Combine resolvers so AOT metadata is available for *all* involved types
        var toolSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = LightsJsonContext.Default
        };

        // --- MCP server ---
        builder.Services.AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly(serializerOptions: toolSerializerOptions);

        // Auth
        const string ApiKeyScheme = "ApiKey";
        builder.Services.AddAuthentication(ApiKeyScheme)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(ApiKeyScheme, _ => { });
        builder.Services.AddAuthorization(o => o.AddPolicy("McpClient", p => p.RequireAuthenticatedUser()));


        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        // -- Secure the MCP endpoints under /mcp via a route group
        var mcpGroup = app.MapGroup("/mcp").RequireAuthorization("McpClient");
        mcpGroup.MapMcp();   // <— call MapMcp on the group; all routes get the prefix + auth


        app.Run();
    }
}


