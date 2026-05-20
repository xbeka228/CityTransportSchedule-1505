using System;

namespace CityTransportSchedule.Models;

public sealed class ScheduleEntry
{
    public int Id { get; set; }
    public int RouteId { get; set; }
    public int VehicleId { get; set; }
    public int DriverId { get; set; }
    public string RouteNumber { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public string TimeRange => $"{DepartureTime:dd.MM.yyyy HH:mm} - {ArrivalTime:HH:mm}";
    public string Title => $"Маршрут {RouteNumber}";
}
