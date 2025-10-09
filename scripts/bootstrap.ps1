Param(
  [string]$Solution = "Arkana"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Run([string]$cmd){
  Write-Host "→ $cmd" -ForegroundColor Cyan
  iex $cmd
}

New-Item -ItemType Directory -Force -Path src | Out-Null
New-Item -ItemType Directory -Force -Path aspire | Out-Null

if (-not (Test-Path "$Solution.sln")) {
  Run "dotnet new sln -n $Solution"
}

if (-not (Test-Path 'src/Gateway/Gateway.csproj')) {
  Run "dotnet new webapi -n Gateway -o src/Gateway"
  Run "dotnet add src/Gateway package Yarp.ReverseProxy"
  Run "dotnet add src/Gateway package Serilog.AspNetCore"
}

if (-not (Test-Path 'src/ChatApi/ChatApi.csproj')) {
  Run "dotnet new webapi -n ChatApi -o src/ChatApi"
  Run "dotnet add src/ChatApi package Serilog.AspNetCore"
}

if (-not (Test-Path 'src/Admin/Admin.csproj')) {
  # Pure WebAssembly (no server)
  Run "dotnet new blazorwasm -n Admin -o src/Admin"
}

if (-not (Test-Path 'src/Chat/Chat.csproj')) {
  # Chat UI (pure WASM)
  Run "dotnet new blazorwasm -n Chat -o src/Chat"
}

if (-not (Test-Path 'src/Shared/Shared.csproj')) {
  Run "dotnet new classlib -n Shared -o src/Shared"
}

if (-not (Test-Path 'aspire/AppHost/AppHost.csproj')) {
  Run "dotnet new aspire-apphost -n AppHost -o aspire/AppHost"
}

if (-not (Test-Path 'aspire/ServiceDefaults/ServiceDefaults.csproj')) {
  Run "dotnet new aspire-servicedefaults -n ServiceDefaults -o aspire/ServiceDefaults"
}

# Wire references
Run "dotnet add src/Gateway reference aspire/ServiceDefaults/ServiceDefaults.csproj"
Run "dotnet add src/ChatApi reference aspire/ServiceDefaults/ServiceDefaults.csproj"
Run "dotnet add aspire/AppHost reference src/Gateway/Gateway.csproj"
Run "dotnet add aspire/AppHost reference src/ChatApi/ChatApi.csproj"
Run "dotnet add aspire/AppHost reference aspire/ServiceDefaults/ServiceDefaults.csproj"

# Add to solution
Run "dotnet sln $Solution.sln add src/Gateway/Gateway.csproj"
Run "dotnet sln $Solution.sln add src/ChatApi/ChatApi.csproj"
Run "dotnet sln $Solution.sln add src/Admin/Admin.csproj"
Run "dotnet sln $Solution.sln add src/Chat/Chat.csproj"
Run "dotnet sln $Solution.sln add src/Shared/Shared.csproj"
Run "dotnet sln $Solution.sln add aspire/AppHost/AppHost.csproj"
Run "dotnet sln $Solution.sln add aspire/ServiceDefaults/ServiceDefaults.csproj"

# Overwrite AppHost Program.cs to register Gateway and Redis
$appHostProgram = @'
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
'@
Set-Content -Path "aspire/AppHost/Program.cs" -Value $appHostProgram -Encoding UTF8

# Overwrite ChatApi Program.cs with minimal streaming endpoints
$chatApiProgram = @'
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
'@
Set-Content -Path "src/ChatApi/Program.cs" -Value $chatApiProgram -Encoding UTF8

Write-Host "Bootstrap complete. Next: dotnet build; run AppHost for Aspire orchestration." -ForegroundColor Green
