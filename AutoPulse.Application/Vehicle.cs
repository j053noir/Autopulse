namespace AutoPulse.Application;

public class Vehicle
{
    public string Marque { get; private set; }
    public string? Model { get; private set; }
    public int? Year { get; private set; }

    public Vehicle(string? marque, string? model, int? year)
    {
        Marque = marque ?? throw new ArgumentNullException("marque is required");
        Model = model ?? throw new ArgumentNullException("model is required");
        Year = year ?? throw new ArgumentNullException("year is required");
    }
}
