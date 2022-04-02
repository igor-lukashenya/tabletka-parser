using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.TypeConversion;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Tabletka.Parser.Models;
using Tabletka.Parser.Options;

namespace Tabletka.Parser;

public class ParsingWorker : BackgroundService
{
    private readonly ILogger<ParsingWorker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<MedicineOptions> _medicineOptions;
    private readonly IOptions<ExportOptions> _exportOptions;

    public ParsingWorker(ILogger<ParsingWorker> logger, 
        IHttpClientFactory httpClientFactory, 
        IOptions<MedicineOptions> medicineOptions, 
        IOptions<ExportOptions> exportOptions)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _medicineOptions = medicineOptions;
        _exportOptions = exportOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Parsing starting at: {Time}", DateTimeOffset.Now);
        var currentDate = DateOnly.FromDateTime(DateTime.Now);
        var results = await GetResults(currentDate, stoppingToken);
        await ExportResults(results, stoppingToken);
        _logger.LogInformation("Parsing finishing at: {Time}", DateTimeOffset.Now);
    }

    private async Task<List<Result>> GetResults(DateOnly date, CancellationToken stoppingToken = default)
    {
        using var client = _httpClientFactory.CreateClient("Default");
        var results = new List<Result>();
        foreach (var item in _medicineOptions.Value.Items)
        {
            try
            {
                var response = await client.GetAsync($"/search?request={item}&region=1001", stoppingToken);
                var content = await response.Content.ReadAsStringAsync(stoppingToken);
                var html = new HtmlDocument();
                html.LoadHtml(content);
                var table = html.GetElementbyId("base-select");
                var rows = table.Descendants("tr")
                    .Where(r => r.HasClass("tr-border"))
                    .ToList();
                results.AddRange(rows.Select(row => Result.Create(row, date)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during getting results for {Medicine}", item);
            }
        }
        return results;
    }

    private async Task ExportResults(IEnumerable<Result> results, CancellationToken cancellationToken = default)
    {
        var fileName = $"{DateTime.Now:dd-MM-yyyy HH-mm-ss}.csv";
        var filePath = Path.Combine(_exportOptions.Value.TargetDirectory, fileName);
        await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        await using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);
        var options = new TypeConverterOptions { Formats =  new [] {"dd/MM/yyyy"}};
        csv.Context.TypeConverterOptionsCache.AddOptions<DateOnly>(options);
        await csv.WriteRecordsAsync(results, cancellationToken);
        _logger.LogInformation("Results saved to: {File}", filePath);
    }
}