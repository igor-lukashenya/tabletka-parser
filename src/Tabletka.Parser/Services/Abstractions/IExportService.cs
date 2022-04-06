using Tabletka.Parser.Models;

namespace Tabletka.Parser.Services.Abstractions;

public interface IExportService
{
    Task ExportResults(string name, IEnumerable<Medicine> results, CancellationToken cancellationToken = default);
}