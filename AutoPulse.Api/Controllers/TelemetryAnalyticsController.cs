using AutoPulse.Infrastructure.Storage;
using Microsoft.AspNetCore.Mvc;

namespace AutoPulse.Api.Controllers;

[ApiController]
[Route("api/telemetry/archive")]
public class TelemetryAnalyticsController(IColdStorageProvider coldStorageProvider) : ControllerBase
{
    /// <summary>
    /// Consulta de metadatos analíticos de los archivos de telemetría archivados en Cold Storage (Apache Parquet).
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetArchivedSummary([FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
    {
        string prefix = "telemetry/";
        if (year.HasValue)
        {
            prefix += $"year={year.Value:D4}/";
            if (month.HasValue)
            {
                prefix += $"month={month.Value:D2}/";
            }
        }

        var archivedFiles = (await coldStorageProvider.ListArchivedFilesAsync(prefix, cancellationToken)).ToList();

        var response = new
        {
            TotalArchivedFiles = archivedFiles.Count,
            FilterPrefix = prefix,
            Format = "Apache Parquet (Snappy Compressed)",
            ArchivedFiles = archivedFiles
        };

        return Ok(response);
    }
}
