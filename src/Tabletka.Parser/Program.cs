using Microsoft.Extensions.Options;
using Tabletka.Parser;
using Tabletka.Parser.Options;
using Tabletka.Parser.Parsers;
using Tabletka.Parser.Parsers.Abstractions;
using Tabletka.Parser.Services;
using Tabletka.Parser.Services.Abstractions;
using Tabletka.Parser.Workers;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, collection) =>
    {
        collection.Configure<ApiOptions>(context.Configuration.GetSection(ApiOptions.Section));
        collection.Configure<MedicineOptions>(context.Configuration.GetSection(MedicineOptions.Section));
        collection.Configure<ExportOptions>(context.Configuration.GetSection(ExportOptions.Section));
        collection.Configure<ParsingOptions>(context.Configuration.GetSection(ParsingOptions.Section));

        collection.AddHttpClient(HttpClients.Tabletka, (provider, client) =>
        {
            var apiOptions = provider.GetRequiredService<IOptions<ApiOptions>>();
            client.BaseAddress = new Uri(apiOptions.Value.BaseAddress);
        });

        collection.AddTransient<IHtmlParser, HtmlParser>();
        collection.AddTransient<IExportService, ExportService>();
        collection.AddTransient<IMedicinesService, MedicinesService>();

        collection.AddHostedService<ParsingWorker>();
    })
    .UseConsoleLifetime()
    .Build()
    .RunAsync();