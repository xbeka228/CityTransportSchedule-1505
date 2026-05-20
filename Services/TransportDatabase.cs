using System;
using System.Collections.Generic;
using System.IO;
using CityTransportSchedule.Models;
using Microsoft.Data.Sqlite;

namespace CityTransportSchedule.Services;

public sealed class TransportDatabase
{
    private readonly string _connectionString;

    public TransportDatabase(string databasePath = "city_transport.db")
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(databasePath)
        }.ToString();

        Initialize();
    }

    public IReadOnlyList<RouteInfo> GetRoutes()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Number, StartPoint, EndPoint, IntervalMinutes, IsActive
            FROM Routes
            ORDER BY Number;
            """;

        using var reader = command.ExecuteReader();
        var routes = new List<RouteInfo>();
        while (reader.Read())
        {
            routes.Add(new RouteInfo
            {
                Id = reader.GetInt32(0),
                Number = reader.GetString(1),
                StartPoint = reader.GetString(2),
                EndPoint = reader.GetString(3),
                IntervalMinutes = reader.GetInt32(4),
                IsActive = reader.GetInt32(5) == 1
            });
        }

        return routes;
    }

    public IReadOnlyList<Vehicle> GetVehicles()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, BoardNumber, PlateNumber, Type, Capacity, Status
            FROM Vehicles
            ORDER BY BoardNumber;
            """;

        using var reader = command.ExecuteReader();
        var vehicles = new List<Vehicle>();
        while (reader.Read())
        {
            vehicles.Add(new Vehicle
            {
                Id = reader.GetInt32(0),
                BoardNumber = reader.GetString(1),
                PlateNumber = reader.GetString(2),
                Type = reader.GetString(3),
                Capacity = reader.GetInt32(4),
                Status = reader.GetString(5)
            });
        }

        return vehicles;
    }

    public IReadOnlyList<Driver> GetDrivers()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, FullName, Phone, LicenseCategory, Shift
            FROM Drivers
            ORDER BY FullName;
            """;

        using var reader = command.ExecuteReader();
        var drivers = new List<Driver>();
        while (reader.Read())
        {
            drivers.Add(new Driver
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Phone = reader.GetString(2),
                LicenseCategory = reader.GetString(3),
                Shift = reader.GetString(4)
            });
        }

        return drivers;
    }

    public IReadOnlyList<ScheduleEntry> GetSchedule(string? routeFilter, string? statusFilter)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT s.Id, s.RouteId, s.VehicleId, s.DriverId, r.Number,
                   r.StartPoint || ' - ' || r.EndPoint AS Direction,
                   v.BoardNumber || ' (' || v.PlateNumber || ')' AS VehicleName,
                   d.FullName, s.DepartureTime, s.ArrivalTime, s.Status, s.Notes
            FROM ScheduleEntries s
            JOIN Routes r ON r.Id = s.RouteId
            JOIN Vehicles v ON v.Id = s.VehicleId
            JOIN Drivers d ON d.Id = s.DriverId
            WHERE (@route = '' OR r.Number = @route)
              AND (@status = '' OR s.Status = @status)
            ORDER BY datetime(s.DepartureTime), r.Number;
            """;
        command.Parameters.AddWithValue("@route", routeFilter ?? string.Empty);
        command.Parameters.AddWithValue("@status", statusFilter ?? string.Empty);

        using var reader = command.ExecuteReader();
        var entries = new List<ScheduleEntry>();
        while (reader.Read())
        {
            entries.Add(new ScheduleEntry
            {
                Id = reader.GetInt32(0),
                RouteId = reader.GetInt32(1),
                VehicleId = reader.GetInt32(2),
                DriverId = reader.GetInt32(3),
                RouteNumber = reader.GetString(4),
                Direction = reader.GetString(5),
                VehicleName = reader.GetString(6),
                DriverName = reader.GetString(7),
                DepartureTime = DateTime.Parse(reader.GetString(8)),
                ArrivalTime = DateTime.Parse(reader.GetString(9)),
                Status = reader.GetString(10),
                Notes = reader.GetString(11)
            });
        }

        return entries;
    }

    public void AddScheduleEntry(int routeId, int vehicleId, int driverId, DateTime departure, DateTime arrival, string status, string notes)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO ScheduleEntries (RouteId, VehicleId, DriverId, DepartureTime, ArrivalTime, Status, Notes)
            VALUES (@routeId, @vehicleId, @driverId, @departure, @arrival, @status, @notes);
            """;
        command.Parameters.AddWithValue("@routeId", routeId);
        command.Parameters.AddWithValue("@vehicleId", vehicleId);
        command.Parameters.AddWithValue("@driverId", driverId);
        command.Parameters.AddWithValue("@departure", departure.ToString("O"));
        command.Parameters.AddWithValue("@arrival", arrival.ToString("O"));
        command.Parameters.AddWithValue("@status", status.Trim());
        command.Parameters.AddWithValue("@notes", notes.Trim());
        command.ExecuteNonQuery();
    }

    public void AddRoute(string number, string startPoint, string endPoint, int intervalMinutes)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Routes (Number, StartPoint, EndPoint, IntervalMinutes, IsActive)
            VALUES (@number, @startPoint, @endPoint, @intervalMinutes, 1);
            """;
        command.Parameters.AddWithValue("@number", number.Trim());
        command.Parameters.AddWithValue("@startPoint", startPoint.Trim());
        command.Parameters.AddWithValue("@endPoint", endPoint.Trim());
        command.Parameters.AddWithValue("@intervalMinutes", intervalMinutes);
        command.ExecuteNonQuery();
    }

    public void AddVehicle(string boardNumber, string plateNumber, string type, int capacity, string status)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Vehicles (BoardNumber, PlateNumber, Type, Capacity, Status)
            VALUES (@boardNumber, @plateNumber, @type, @capacity, @status);
            """;
        command.Parameters.AddWithValue("@boardNumber", boardNumber.Trim());
        command.Parameters.AddWithValue("@plateNumber", plateNumber.Trim());
        command.Parameters.AddWithValue("@type", type.Trim());
        command.Parameters.AddWithValue("@capacity", capacity);
        command.Parameters.AddWithValue("@status", status.Trim());
        command.ExecuteNonQuery();
    }

    public void AddDriver(string fullName, string phone, string licenseCategory, string shift)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Drivers (FullName, Phone, LicenseCategory, Shift)
            VALUES (@fullName, @phone, @licenseCategory, @shift);
            """;
        command.Parameters.AddWithValue("@fullName", fullName.Trim());
        command.Parameters.AddWithValue("@phone", phone.Trim());
        command.Parameters.AddWithValue("@licenseCategory", licenseCategory.Trim());
        command.Parameters.AddWithValue("@shift", shift.Trim());
        command.ExecuteNonQuery();
    }

    public void DeleteScheduleEntry(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM ScheduleEntries WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private void Initialize()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS Routes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Number TEXT NOT NULL UNIQUE,
                StartPoint TEXT NOT NULL,
                EndPoint TEXT NOT NULL,
                IntervalMinutes INTEGER NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS Vehicles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                BoardNumber TEXT NOT NULL UNIQUE,
                PlateNumber TEXT NOT NULL,
                Type TEXT NOT NULL,
                Capacity INTEGER NOT NULL,
                Status TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Drivers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Phone TEXT NOT NULL,
                LicenseCategory TEXT NOT NULL,
                Shift TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ScheduleEntries (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RouteId INTEGER NOT NULL REFERENCES Routes(Id),
                VehicleId INTEGER NOT NULL REFERENCES Vehicles(Id),
                DriverId INTEGER NOT NULL REFERENCES Drivers(Id),
                DepartureTime TEXT NOT NULL,
                ArrivalTime TEXT NOT NULL,
                Status TEXT NOT NULL,
                Notes TEXT NOT NULL DEFAULT ''
            );
            """;
        command.ExecuteNonQuery();

        SeedIfEmpty(connection);
    }

    private static void SeedIfEmpty(SqliteConnection connection)
    {
        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM Routes;";
        if (Convert.ToInt32(countCommand.ExecuteScalar()) > 0)
        {
            return;
        }

        using var seedCommand = connection.CreateCommand();
        seedCommand.CommandText = """
            INSERT INTO Routes (Number, StartPoint, EndPoint, IntervalMinutes, IsActive) VALUES
            ('7', 'Теміржол вокзалы', 'Орталық алаң', 12, 1),
            ('15', 'Әуежай', 'Университет қалашығы', 18, 1),
            ('28', 'Саяхат автобекеті', 'Индустриялық аймақ', 15, 1);

            INSERT INTO Vehicles (BoardNumber, PlateNumber, Type, Capacity, Status) VALUES
            ('A-104', '777 ABC 02', 'Автобус', 85, 'Желіде'),
            ('B-221', '221 KAZ 02', 'Электробус', 72, 'Желіде'),
            ('T-018', '018 TRN 02', 'Троллейбус', 65, 'Резерв');

            INSERT INTO Drivers (FullName, Phone, LicenseCategory, Shift) VALUES
            ('Айдос Сәрсенов', '+7 701 120 45 18', 'D', 'Таңғы'),
            ('Мадина Омарова', '+7 707 882 31 90', 'D', 'Күндізгі'),
            ('Ержан Қасымов', '+7 775 664 10 44', 'D', 'Кешкі');

            INSERT INTO ScheduleEntries (RouteId, VehicleId, DriverId, DepartureTime, ArrivalTime, Status, Notes) VALUES
            (1, 1, 1, '2026-05-15T07:20:00', '2026-05-15T08:05:00', 'Жоспарланған', 'Қалыпты интервал'),
            (2, 2, 2, '2026-05-15T08:10:00', '2026-05-15T09:00:00', 'Жолда', 'Әуежай бағыты'),
            (3, 3, 3, '2026-05-15T18:30:00', '2026-05-15T19:20:00', 'Жоспарланған', 'Кешкі рейс');
            """;
        seedCommand.ExecuteNonQuery();
    }
}
