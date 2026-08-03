namespace AutoPulse.Domain.Entities.Sql;

public class TelemetryRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VehicleId { get; set; }
    public double Speed { get; set; }
    public double Odometer { get; set; }
    public double BatteryLevel { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Status { get; set; } = "Active";
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
