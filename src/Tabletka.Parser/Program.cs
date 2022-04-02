using Microsoft.Extensions.Options;
using Tabletka.Parser;
using Tabletka.Parser.Options;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, collection) =>
    {
        collection.Configure<ApiOptions>(context.Configuration);
        collection.Configure<MedicineOptions>(context.Configuration);
        collection.Configure<ExportOptions>(context.Configuration);
        
        collection.AddHttpClient("Default", (provider, client)  =>
        {
            var apiOptions = provider.GetRequiredService<IOptions<ApiOptions>>();
            client.BaseAddress = new Uri(apiOptions.Value.BaseAddress);
        });
        
        collection.AddHostedService<ParsingWorker>();
    })
    .UseConsoleLifetime()
    .Build();

await host.RunAsync();