using Tabletka.Parser.Models;

namespace Tabletka.Parser.Services.Abstractions;

public interface IMedicinesService
{
    Task<IReadOnlyCollection<Medicine>> GetMedicines(RequiredMedicine requiredMedicine,
        CancellationToken stoppingToken = default);
}