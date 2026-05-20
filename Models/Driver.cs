namespace CityTransportSchedule.Models;

public sealed class Driver
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string LicenseCategory { get; set; } = string.Empty;
    public string Shift { get; set; } = string.Empty;

    public override string ToString() => $"{FullName} ({Shift})";
}
