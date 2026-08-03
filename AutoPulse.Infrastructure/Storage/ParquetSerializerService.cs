using AutoPulse.Domain.Entities.Sql;
using Parquet;
using Parquet.Data;
using Parquet.Schema;

namespace AutoPulse.Infrastructure.Storage;

public class ParquetSerializerService : IParquetSerializerService
{
    public async Task<byte[]> SerializeToParquetAsync(IEnumerable<TelemetryRecord> records, CancellationToken cancellationToken = default)
    {
        var recordList = records as List<TelemetryRecord> ?? records.ToList();

        // 1. Definición del Esquema Parquet con DataFields
        var idField = new DataField<string>("Id");
        var vehicleIdField = new DataField<string>("VehicleId");
        var speedField = new DataField<double>("Speed");
        var odometerField = new DataField<double>("Odometer");
        var batteryLevelField = new DataField<double>("BatteryLevel");
        var latitudeField = new DataField<double>("Latitude");
        var longitudeField = new DataField<double>("Longitude");
        var statusField = new DataField<string>("Status");
        var timestampField = new DataField<DateTimeOffset>("Timestamp");

        var schema = new ParquetSchema(
            idField,
            vehicleIdField,
            speedField,
            odometerField,
            batteryLevelField,
            latitudeField,
            longitudeField,
            statusField,
            timestampField
        );

        // 2. Extracción de Columnas en Arrays
        int count = recordList.Count;
        var idArray = new string[count];
        var vehicleIdArray = new string[count];
        var speedArray = new double[count];
        var odometerArray = new double[count];
        var batteryLevelArray = new double[count];
        var latitudeArray = new double[count];
        var longitudeArray = new double[count];
        var statusArray = new string[count];
        var timestampArray = new DateTimeOffset[count];

        for (int i = 0; i < count; i++)
        {
            var item = recordList[i];
            idArray[i] = item.Id.ToString();
            vehicleIdArray[i] = item.VehicleId.ToString();
            speedArray[i] = item.Speed;
            odometerArray[i] = item.Odometer;
            batteryLevelArray[i] = item.BatteryLevel;
            latitudeArray[i] = item.Latitude;
            longitudeArray[i] = item.Longitude;
            statusArray[i] = item.Status;
            timestampArray[i] = item.Timestamp;
        }

        // 3. Serialización a Stream de Parquet con Snappy Compression
        await using var ms = new MemoryStream();
        await using (var writer = await ParquetWriter.CreateAsync(schema, ms, cancellationToken: cancellationToken))
        {
            writer.CompressionMethod = CompressionMethod.Snappy;

            using var groupWriter = writer.CreateRowGroup();
            await groupWriter.WriteColumnAsync(new DataColumn(idField, idArray), cancellationToken);
            await groupWriter.WriteColumnAsync(new DataColumn(vehicleIdField, vehicleIdArray), cancellationToken);
            await groupWriter.WriteColumnAsync(new DataColumn(speedField, speedArray), cancellationToken);
            await groupWriter.WriteColumnAsync(new DataColumn(odometerField, odometerArray), cancellationToken);
            await groupWriter.WriteColumnAsync(new DataColumn(batteryLevelField, batteryLevelArray), cancellationToken);
            await groupWriter.WriteColumnAsync(new DataColumn(latitudeField, latitudeArray), cancellationToken);
            await groupWriter.WriteColumnAsync(new DataColumn(longitudeField, longitudeArray), cancellationToken);
            await groupWriter.WriteColumnAsync(new DataColumn(statusField, statusArray), cancellationToken);
            await groupWriter.WriteColumnAsync(new DataColumn(timestampField, timestampArray), cancellationToken);
        }

        return ms.ToArray();
    }
}
