using AutoPulse.Infrastructure.Persistence.Sql;
using AutoPulse.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Infrastructure.BackgroundJobs;

public class TelemetryArchivalWorker(
    IServiceProvider serviceProvider,
    ILogger<TelemetryArchivalWorker> logger) : BackgroundService
{
    private const int BatchSize = 50000;
    private const int RetentionDays = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("🚀 Telemetry Archival & Data Tiering Worker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessArchivalAsync(stoppingToken);

                // Espera de 24 horas hasta la siguiente ejecución (programado para ejecución nocturna)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error crítico durante la ejecución del job de archivado de telemetría.");
                // Esperar 1 hora antes de reintentar si ocurre un fallo
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    public async Task ProcessArchivalAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AutoPulseDbContext>();
        var parquetSerializer = scope.ServiceProvider.GetRequiredService<IParquetSerializerService>();
        var coldStorageProvider = scope.ServiceProvider.GetRequiredService<IColdStorageProvider>();

        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-RetentionDays);

        logger.LogInformation("Iniciando escaneo de registros de telemetría anteriores a {CutoffDate}", cutoffDate);

        bool hasMoreRecords = true;

        while (hasMoreRecords && !cancellationToken.IsCancellationRequested)
        {
            // 1. Query por Lotes (Batching) para proteger la RAM
            var recordsToArchive = await dbContext.TelemetryRecords
                .AsNoTracking()
                .Where(t => t.Timestamp < cutoffDate)
                .OrderBy(t => t.Timestamp)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (recordsToArchive.Count == 0)
            {
                logger.LogInformation("No se encontraron más registros de telemetría antiguos para archivar.");
                hasMoreRecords = false;
                break;
            }

            logger.LogInformation("Procesando lote de {Count} registros de telemetría para archivado.", recordsToArchive.Count);

            // Agrupar los registros por fecha (año/mes/día) para guardar particionado en Cold Storage Hive-style
            var groupedByDate = recordsToArchive.GroupBy(r => r.Timestamp.Date);

            foreach (var group in groupedByDate)
            {
                var dayRecords = group.ToList();
                var date = group.Key;

                // 2. Serialización Parquet con Snappy Compression
                byte[] parquetBytes = await parquetSerializer.SerializeToParquetAsync(dayRecords, cancellationToken);

                // Ruta particionada Hive-style: telemetry/year=YYYY/month=MM/telemetry_DD.parquet
                string partitionPath = $"telemetry/year={date.Year:D4}/month={date.Month:D2}/telemetry_{date.Day:D2}_{Guid.NewGuid().ToString("N")[..8]}.parquet";

                // 3. Subida Atómica a GCS/S3 (Si esto falla, la excepción impedirá borrar los registros)
                await coldStorageProvider.UploadParquetFileAsync(partitionPath, parquetBytes, cancellationToken);

                // 4. Purga No Bloqueante en PostgreSQL vía ExecuteDeleteAsync
                var recordIdsToPurge = dayRecords.Select(r => r.Id).ToList();

                int deletedCount = await dbContext.TelemetryRecords
                    .Where(t => recordIdsToPurge.Contains(t.Id))
                    .ExecuteDeleteAsync(cancellationToken);

                logger.LogInformation("Purga completada exitosamente. Se eliminaron {DeletedCount} registros de PostgreSQL tras confirmar el archivado en {PartitionPath}.", deletedCount, partitionPath);
            }
        }
    }
}
