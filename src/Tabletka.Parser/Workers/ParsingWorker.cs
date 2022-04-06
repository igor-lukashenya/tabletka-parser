using Microsoft.Extensions.Options;
using Tabletka.Parser.Models;
using Tabletka.Parser.Options;
using Tabletka.Parser.Services.Abstractions;

namespace Tabletka.Parser.Workers;

public class ParsingWorker : BackgroundService
{
    private readonly IExportService _exportService;
    private readonly ILogger<ParsingWorker> _logger;
    private readonly IOptions<MedicineOptions> _medicineOptions;
    private readonly IMedicinesService _medicinesService;

    public ParsingWorker(ILogger<ParsingWorker> logger,
        IOptions<MedicineOptions> medicineOptions,
        IMedicinesService medicinesService,
        IExportService exportService)
    {
        _logger = logger;
        _medicineOptions = medicineOptions;
        _medicinesService = medicinesService;
        _exportService = exportService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Parsing starting at: {Time}", DateTimeOffset.Now);

        foreach (var list in _medicineOptions.Value.Lists)
        {
            var listResults = new List<Medicine>();
            foreach (var requiredMedicine in list.Items)
            {
                var medicineResults = await _medicinesService.GetMedicines(requiredMedicine, stoppingToken);
                listResults.AddRange(medicineResults);
            }

            await _exportService.ExportResults(list.Name, listResults, stoppingToken);
        }

        _logger.LogInformation("Parsing finishing at: {Time}", DateTimeOffset.Now);
    }
}