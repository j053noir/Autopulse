using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoPulse.Infrastructure.Storage;

public class GcsColdStorageProvider(IConfiguration configuration, ILogger<GcsColdStorageProvider> logger) : IColdStorageProvider
{
    private readonly string _bucketName = configuration["Gcp:Storage:TelemetryArchiveBucket"] ?? "autopulse-telemetry-archive";

    public async Task<string> UploadParquetFileAsync(string objectPath, byte[] data, CancellationToken cancellationToken = default)
    {
        try
        {
            var storageClient = await StorageClient.CreateAsync();
            using var stream = new MemoryStream(data);

            var obj = await storageClient.UploadObjectAsync(
                _bucketName,
                objectPath,
                "application/x-parquet",
                stream,
                cancellationToken: cancellationToken
            );

            logger.LogInformation("Archivo Parquet subido exitosamente a GCS: gs://{Bucket}/{ObjectPath} ({Size} bytes)", _bucketName, objectPath, data.Length);
            return obj.MediaLink ?? $"gs://{_bucketName}/{objectPath}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al subir el archivo Parquet a Cold Storage en la ruta {ObjectPath}", objectPath);
            throw;
        }
    }

    public async Task<IEnumerable<string>> ListArchivedFilesAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var storageClient = await StorageClient.CreateAsync();
            var objects = storageClient.ListObjectsAsync(_bucketName, prefix);
            var results = new List<string>();

            await foreach (var obj in objects.WithCancellation(cancellationToken))
            {
                results.Add(obj.Name);
            }

            return results;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Atención: No se pudo consultar la lista de archivos en GCS bucket {Bucket}", _bucketName);
            return Array.Empty<string>();
        }
    }
}
