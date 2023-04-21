using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Text.Json;

namespace isolatedWorkerSignalr;

public class BackgroundWorker : BackgroundService
{
    private HubConnection? _connection;
    private readonly HttpClient _httpClient;

    public BackgroundWorker(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("function-api");
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        
        var negotiateMessage = new HttpRequestMessage(HttpMethod.Get, "api/negotiate");
        var response = _httpClient.Send(negotiateMessage, cancellationToken);
        var info = JsonSerializer.Deserialize<SignalRConnectionInfo>(response.Content.ReadAsStream(cancellationToken)) ?? new();

        _connection = new HubConnectionBuilder()
            .WithUrl(info.Url, (options) =>
            {
                options.AccessTokenProvider = () => Task.FromResult((string?)info.AccessToken);
            })
            .Build();

        _connection.On<string>("method1", (message) =>
        {
            Console.WriteLine($"Client received message from hub: {message}");
        });

        await _connection.StartAsync(cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                await NotifyViaHttp(stoppingToken);
                await NotifyViaSignalR(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            Debug.Assert(true);
            Console.WriteLine(ex.Message);
        }
    }

    private async Task NotifyViaHttp(CancellationToken stoppingToken)
    {
        Console.WriteLine("Client notifying hub to broadcast via http.");
        var request = new HttpRequestMessage(HttpMethod.Get, "api/broadcast-message");
        await _httpClient!.SendAsync(request, stoppingToken);
    }

    private async Task NotifyViaSignalR(CancellationToken stoppingToken)
    {
        Console.WriteLine("Client notifying hub to broadcast via signalR.");
        await _connection!.InvokeAsync("method2", stoppingToken);
    }
}