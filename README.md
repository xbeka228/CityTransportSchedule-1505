# Қалалық көлікті басқару кестесі

C# тілінде Avalonia UI және SQLite базасымен жасалған desktop ақпараттық жүйе.

## Мүмкіндіктері

- маршруттарды тіркеу;
- көлік бірліктерін тіркеу;
- жүргізушілерді тіркеу;
- рейс кестесін құру;
- маршрут және мәртебе бойынша сүзгілеу;
- рейсті өшіру;
- бастапқы тест деректерін автоматты енгізу.

## Құрылымы

- `Models/` - деректер модельдері;
- `Services/TransportDatabase.cs` - SQLite кестелері, CRUD операциялары және seed деректері;
- `MainWindow.axaml` - Avalonia интерфейсі;
- `MainWindow.axaml.cs` - экран логикасы және оқиғалар.

## Дерекқор

Бағдарлама бірінші іске қосылғанда `city_transport.db` файлын жасайды.

Негізгі кестелер:

- `Routes`
- `Vehicles`
- `Drivers`
- `ScheduleEntries`

## Іске қосу

```bash
dotnet run --project CityTransportSchedule.csproj
```

## Құрастыру

```bash
dotnet build CityTransportSchedule.csproj
```
