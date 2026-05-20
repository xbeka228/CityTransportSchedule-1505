using Avalonia.Controls;
using CityTransportSchedule.Models;
using CityTransportSchedule.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace CityTransportSchedule;

public partial class MainWindow : Window
{
    private readonly TransportDatabase _database = new();
    private readonly ObservableCollection<ScheduleEntry> _scheduleEntries = new();
    private RouteInfo[] _routes = [];
    private Vehicle[] _vehicles = [];
    private Driver[] _drivers = [];
    private readonly string[] _statuses = ["Жоспарланған", "Жолда", "Кідіріс", "Аяқталды"];

    public MainWindow()
    {
        InitializeComponent();
        ConfigureStaticSources();
        ScheduleList.ItemsSource = _scheduleEntries;
        LoadData();
    }

    private void Login_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (LoginBox.Text == "admin" && PasswordBox.Text == "1234")
        {
            LoginView.IsVisible = false;
            AppView.IsVisible = true;
            LoginErrorText.Text = string.Empty;
            ShowStatus("Персонал кірді.");
            return;
        }

        LoginErrorText.Text = "Логин немесе құпиясөз дұрыс емес.";
    }

    private void Logout_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AppView.IsVisible = false;
        LoginView.IsVisible = true;
        PasswordBox.Text = string.Empty;
        LoginErrorText.Text = string.Empty;
    }

    private void ConfigureStaticSources()
    {
        StatusBox.ItemsSource = _statuses;
        StatusBox.SelectedIndex = 0;
        StatusFilterBox.ItemsSource = new[] { "Барлық мәртебе" }.Concat(_statuses).ToArray();
        StatusFilterBox.SelectedIndex = 0;
        VehicleStatusBox.ItemsSource = new[] { "Желіде", "Резерв", "Жөндеуде" };
        VehicleStatusBox.SelectedIndex = 0;
        ShiftBox.ItemsSource = new[] { "Таңғы", "Күндізгі", "Кешкі", "Түнгі" };
        ShiftBox.SelectedIndex = 0;
        DepartureBox.Text = DateTime.Today.AddHours(7).AddMinutes(30).ToString("dd.MM.yyyy HH:mm");
        ArrivalBox.Text = DateTime.Today.AddHours(8).AddMinutes(15).ToString("dd.MM.yyyy HH:mm");
    }

    private void LoadData()
    {
        _routes = _database.GetRoutes().ToArray();
        _vehicles = _database.GetVehicles().ToArray();
        _drivers = _database.GetDrivers().ToArray();

        RouteBox.ItemsSource = _routes;
        VehicleBox.ItemsSource = _vehicles;
        DriverBox.ItemsSource = _drivers;
        RouteFilterBox.ItemsSource = new[] { "Барлық маршрут" }.Concat(_routes.Select(route => route.Number)).ToArray();

        SelectFirstIfEmpty(RouteBox);
        SelectFirstIfEmpty(VehicleBox);
        SelectFirstIfEmpty(DriverBox);
        if (RouteFilterBox.SelectedIndex < 0)
        {
            RouteFilterBox.SelectedIndex = 0;
        }

        RouteCountText.Text = _routes.Length.ToString(CultureInfo.InvariantCulture);
        VehicleCountText.Text = _vehicles.Length.ToString(CultureInfo.InvariantCulture);
        DriverCountText.Text = _drivers.Length.ToString(CultureInfo.InvariantCulture);

        LoadSchedule();
    }

    private void LoadSchedule()
    {
        var routeFilter = RouteFilterBox.SelectedIndex > 0 ? RouteFilterBox.SelectedItem?.ToString() : null;
        var statusFilter = StatusFilterBox.SelectedIndex > 0 ? StatusFilterBox.SelectedItem?.ToString() : null;
        var entries = _database.GetSchedule(routeFilter, statusFilter);

        _scheduleEntries.Clear();
        foreach (var entry in entries)
        {
            _scheduleEntries.Add(entry);
        }

        ScheduleCountText.Text = _scheduleEntries.Count.ToString(CultureInfo.InvariantCulture);
        StatusText.Text = $"Соңғы жаңарту: {DateTime.Now:HH:mm:ss}";
    }

    private void AddSchedule_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (RouteBox.SelectedItem is not RouteInfo route ||
            VehicleBox.SelectedItem is not Vehicle vehicle ||
            DriverBox.SelectedItem is not Driver driver ||
            StatusBox.SelectedItem is not string status)
        {
            ShowStatus("Маршрут, көлік және жүргізушіні таңдаңыз.");
            return;
        }

        if (!TryReadDateTime(DepartureBox.Text, out var departure) ||
            !TryReadDateTime(ArrivalBox.Text, out var arrival) ||
            arrival <= departure)
        {
            ShowStatus("Уақыт форматы дұрыс емес немесе келу уақыты шығудан ерте.");
            return;
        }

        _database.AddScheduleEntry(route.Id, vehicle.Id, driver.Id, departure, arrival, status, NotesBox.Text ?? string.Empty);
        NotesBox.Text = string.Empty;
        LoadSchedule();
        ShowStatus("Рейс қосылды.");
    }

    private void AddRoute_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RouteNumberBox.Text) ||
            string.IsNullOrWhiteSpace(StartPointBox.Text) ||
            string.IsNullOrWhiteSpace(EndPointBox.Text))
        {
            ShowStatus("Маршрут деректерін толық енгізіңіз.");
            return;
        }

        _database.AddRoute(RouteNumberBox.Text, StartPointBox.Text, EndPointBox.Text, Convert.ToInt32(IntervalBox.Value ?? 1));
        RouteNumberBox.Text = StartPointBox.Text = EndPointBox.Text = string.Empty;
        LoadData();
        ShowStatus("Маршрут қосылды.");
    }

    private void AddVehicle_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(BoardNumberBox.Text) ||
            string.IsNullOrWhiteSpace(PlateNumberBox.Text) ||
            string.IsNullOrWhiteSpace(VehicleTypeBox.Text))
        {
            ShowStatus("Көлік деректерін толық енгізіңіз.");
            return;
        }

        _database.AddVehicle(
            BoardNumberBox.Text,
            PlateNumberBox.Text,
            VehicleTypeBox.Text,
            Convert.ToInt32(CapacityBox.Value ?? 1),
            VehicleStatusBox.SelectedItem?.ToString() ?? "Желіде");
        BoardNumberBox.Text = PlateNumberBox.Text = VehicleTypeBox.Text = string.Empty;
        LoadData();
        ShowStatus("Көлік қосылды.");
    }

    private void AddDriver_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DriverNameBox.Text) ||
            string.IsNullOrWhiteSpace(DriverPhoneBox.Text) ||
            string.IsNullOrWhiteSpace(LicenseBox.Text))
        {
            ShowStatus("Жүргізуші деректерін толық енгізіңіз.");
            return;
        }

        _database.AddDriver(
            DriverNameBox.Text,
            DriverPhoneBox.Text,
            LicenseBox.Text,
            ShiftBox.SelectedItem?.ToString() ?? "Таңғы");
        DriverNameBox.Text = DriverPhoneBox.Text = string.Empty;
        LicenseBox.Text = "D";
        LoadData();
        ShowStatus("Жүргізуші қосылды.");
    }

    private void DeleteSchedule_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button { Tag: int id })
        {
            _database.DeleteScheduleEntry(id);
            LoadSchedule();
            ShowStatus("Рейс өшірілді.");
        }
    }

    private void Refresh_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => LoadData();

    private void Filter_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (ScheduleList is not null)
        {
            LoadSchedule();
        }
    }

    private static void SelectFirstIfEmpty(ComboBox box)
    {
        if (box.SelectedIndex < 0 && box.ItemCount > 0)
        {
            box.SelectedIndex = 0;
        }
    }

    private static bool TryReadDateTime(string? value, out DateTime result)
    {
        return DateTime.TryParseExact(value, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ||
               DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out result);
    }

    private void ShowStatus(string message) => StatusText.Text = message;
}
