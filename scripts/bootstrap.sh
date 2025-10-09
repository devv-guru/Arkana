#!/usr/bin/env bash
set -euo pipefail

SOLUTION=${1:-Arkana}

echo "→ Ensuring folders"
mkdir -p src aspire

if [ ! -f "$SOLUTION.sln" ]; then
  echo "→ dotnet new sln -n $SOLUTION"
  dotnet new sln -n "$SOLUTION"
fi

if [ ! -f src/Gateway/Gateway.csproj ]; then
  echo "→ dotnet new webapi -n Gateway -o src/Gateway"
  dotnet new webapi -n Gateway -o src/Gateway
  dotnet add src/Gateway package Yarp.ReverseProxy
  dotnet add src/Gateway package Serilog.AspNetCore
fi

if [ ! -f src/ChatApi/ChatApi.csproj ]; then
  echo "→ dotnet new webapi -n ChatApi -o src/ChatApi"
  dotnet new webapi -n ChatApi -o src/ChatApi
  dotnet add src/ChatApi package Serilog.AspNetCore
fi

if [ ! -f src/Admin/Admin.csproj ]; then
  echo "→ dotnet new blazorwasm -n Admin -o src/Admin"
  dotnet new blazorwasm -n Admin -o src/Admin
fi

if [ ! -f src/Chat/Chat.csproj ]; then
  echo "→ dotnet new blazorwasm -n Chat -o src/Chat"
  dotnet new blazorwasm -n Chat -o src/Chat
fi

if [ ! -f src/Shared/Shared.csproj ]; then
  echo "→ dotnet new classlib -n Shared -o src/Shared"
  dotnet new classlib -n Shared -o src/Shared
fi

if [ ! -f aspire/AppHost/AppHost.csproj ]; then
  echo "→ dotnet new aspire-apphost -n AppHost -o aspire/AppHost"
  dotnet new aspire-apphost -n AppHost -o aspire/AppHost
fi

if [ ! -f aspire/ServiceDefaults/ServiceDefaults.csproj ]; then
  echo "→ dotnet new aspire-servicedefaults -n ServiceDefaults -o aspire/ServiceDefaults"
  dotnet new aspire-servicedefaults -n ServiceDefaults -o aspire/ServiceDefaults
fi

dotnet add src/Gateway reference aspire/ServiceDefaults/ServiceDefaults.csproj
dotnet add src/ChatApi reference aspire/ServiceDefaults/ServiceDefaults.csproj
dotnet add aspire/AppHost reference src/Gateway/Gateway.csproj
dotnet add aspire/AppHost reference src/ChatApi/ChatApi.csproj
dotnet add aspire/AppHost reference aspire/ServiceDefaults/ServiceDefaults.csproj

dotnet sln "$SOLUTION.sln" add src/Gateway/Gateway.csproj || true
dotnet sln "$SOLUTION.sln" add src/ChatApi/ChatApi.csproj || true
dotnet sln "$SOLUTION.sln" add src/Admin/Admin.csproj || true
dotnet sln "$SOLUTION.sln" add src/Chat/Chat.csproj || true
dotnet sln "$SOLUTION.sln" add src/Shared/Shared.csproj || true
dotnet sln "$SOLUTION.sln" add aspire/AppHost/AppHost.csproj || true
dotnet sln "$SOLUTION.sln" add aspire/ServiceDefaults/ServiceDefaults.csproj || true

cat > aspire/AppHost/Program.cs <<'EOF'
using AppHost.Common;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache").WithLifetime(ContainerLifetime.Persistent);

var gateway = builder.AddProject<Projects.Gateway>("arkana-gateway", launchProfileName: "http")
    .WithExternalHttpEndpoints();

var chatApi = builder.AddProject<Projects.ChatApi>("arkana-chatapi", launchProfileName: "http")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache);

builder.Build().Run();
EOF

cat > src/ChatApi/Program.cs <<'EOF'
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/api/chat/messages", async (HttpContext http, SendMessageRequest request) =>
{
    http.Response.Headers["Content-Type"] = "application/x-ndjson";
    http.Response.StatusCode = StatusCodes.Status200OK;
    var writer = http.Response.BodyWriter;
    await WriteNdjsonAsync(writer, new { type = "message.delta", data = "Processing request..." });
    await Task.Delay(200);
    await WriteNdjsonAsync(writer, new { type = "message.delta", data = $"You said: {request.Content}" });
    await Task.Delay(200);
    await WriteNdjsonAsync(writer, new { type = "message.completed" });
});

app.MapGet("/api/chat/stream", async (HttpContext http, string conversationId) =>
{
    http.Response.Headers["Content-Type"] = "text/event-stream";
    http.Response.StatusCode = StatusCodes.Status200OK;
    var stream = http.Response.Body;
    var enc = Encoding.UTF8;
    for (var i = 0; i < 3; i++)
    {
        var payload = $"event: ping\n" +
                      $"data: {{\"conversationId\":\"{conversationId}\",\"i\":{i}}}\n\n";
        var bytes = enc.GetBytes(payload);
        await stream.WriteAsync(bytes, 0, bytes.Length);
        await stream.FlushAsync();
        await Task.Delay(1000);
    }
});

app.Run();

static async Task WriteNdjsonAsync(PipeWriter writer, object obj)
{
    var json = JsonSerializer.Serialize(obj);
    var data = Encoding.UTF8.GetBytes(json + "\n");
    writer.Write(data);
    await writer.FlushAsync();
}

public record SendMessageRequest(string? ConversationId, string Content);
EOF

echo "Bootstrap complete. Next: dotnet build; dotnet run --project aspire/AppHost"
