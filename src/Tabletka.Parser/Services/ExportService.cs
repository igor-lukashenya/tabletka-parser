using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Options;
using Tabletka.Parser.Models;
using Tabletka.Parser.Options;
using Tabletka.Parser.Services.Abstractions;

namespace Tabletka.Parser.Services;

public class ExportService : IExportService
{
    private readonly IOptions<ExportOptions> _exportOptions;
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger, IOptions<ExportOptions> exportOptions)
    {
        _logger = logger;
        _exportOptions = exportOptions;
    }

    public async Task ExportResults(string name, IEnumerable<Medicine> results,
        CancellationToken cancellationToken = default)
    {
        var exportOptions = _exportOptions.Value;
        var fileNameDateFormat = exportOptions.FileNameDateFormat;
        var fileName = $"{name} {DateTime.Now.ToString(fileNameDateFormat)}.csv";
        var filePath = Path.Combine(_exportOptions.Value.TargetDirectory, fileName);
        await using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        var culture = !string.IsNullOrWhiteSpace(exportOptions.Culture)
            ? CultureInfo.GetCultureInfo(exportOptions.Culture)
            : CultureInfo.CurrentCulture;
        await using var csv = new CsvWriter(writer, culture);
        var options = new TypeConverterOptions { Formats = new[] { exportOptions.DateFormat } };
        csv.Context.TypeConverterOptionsCache.AddOptions<DateOnly>(options);
        var orderedResults = results.OrderBy(medicine => medicine.Name)
            .ToList();
        await csv.WriteRecordsAsync(orderedResults, cancellationToken);
        _logger.LogInformation("Results saved to: {File}", filePath);
    }
}