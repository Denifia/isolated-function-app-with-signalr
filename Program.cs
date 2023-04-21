using isolatedWorkerSignalr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((builder, services) =>
    {
        services.AddHostedService<BackgroundWorker>();

        var functionUri = builder.Configuration.GetValue<Uri>("FunctionsUri");
        services.AddHttpClient("function-api", config =>
        {
            config.BaseAddress = functionUri;
        });
    })
    .Build();

host.Run();
