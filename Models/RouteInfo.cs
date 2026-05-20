namespace CityTransportSchedule.Models;

public sealed class RouteInfo
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string StartPoint { get; set; } = string.Empty;
    public string EndPoint { get; set; } = string.Empty;
    public int IntervalMinutes { get; set; }
    public bool IsActive { get; set; }

    public string Direction => $"{StartPoint} - {EndPoint}";
    public string DisplayName => $"{Number}: {Direction}";

    public override string ToString() => DisplayName;
}
