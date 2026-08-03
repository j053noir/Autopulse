using AutoPulse.Domain.Entities.Sql;

namespace AutoPulse.Infrastructure.Storage;

public interface IParquetSerializerService
{
    Task<byte[]> SerializeToParquetAsync(IEnumerable<TelemetryRecord> records, CancellationToken cancellationToken = default);
}
