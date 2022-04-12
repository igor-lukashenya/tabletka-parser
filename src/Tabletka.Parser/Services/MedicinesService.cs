using Tabletka.Parser.Models;
using Tabletka.Parser.Parsers.Abstractions;
using Tabletka.Parser.Services.Abstractions;

namespace Tabletka.Parser.Services;

public class MedicinesService : IMedicinesService
{
    private readonly IHtmlParser _htmlParser;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MedicinesService> _logger;

    public MedicinesService(ILogger<MedicinesService> logger, IHttpClientFactory httpClientFactory,
        IHtmlParser htmlParser)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _htmlParser = htmlParser;
    }

    public async Task<IReadOnlyCollection<Medicine>> GetMedicines(RequiredMedicine requiredMedicine,
        CancellationToken stoppingToken = default)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient(HttpClients.Tabletka);
            var response = await client.GetAsync($"/search?request={requiredMedicine.Name}&region=1001", stoppingToken);
            var content = await response.Content.ReadAsStringAsync(stoppingToken);
            var medicines = _htmlParser.Parse(content);
            return medicines.Select(medicine =>
                {
                    medicine.Date = DateTime.Now;
                    return medicine;
                })
                .Where(result =>
                    requiredMedicine.Produces == null || requiredMedicine.Produces.Contains(result.Produce))
                .Where(result => requiredMedicine.Forms == null || requiredMedicine.Forms.Contains(result.Form))
                .Where(medicine =>
                    !requiredMedicine.StrictName || string.Equals(medicine.Name, requiredMedicine.Name))
                .ToList();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during getting results for {@Medicine}", requiredMedicine);
            return ArraySegment<Medicine>.Empty;
        }
    }
}