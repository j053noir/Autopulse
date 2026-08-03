namespace AutoPulse.Infrastructure.Storage;

public interface IColdStorageProvider
{
    Task<string> UploadParquetFileAsync(string objectPath, byte[] data, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> ListArchivedFilesAsync(string prefix, CancellationToken cancellationToken = default);
}
