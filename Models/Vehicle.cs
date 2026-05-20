namespace CityTransportSchedule.Models;

public sealed class Vehicle
{
    public int Id { get; set; }
    public string BoardNumber { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;

    public string DisplayName => $"{BoardNumber} ({PlateNumber})";

    public override string ToString() => DisplayName;
}
