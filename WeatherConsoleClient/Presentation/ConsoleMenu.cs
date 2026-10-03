using WeatherConsoleClient.Application.DTOs;
using WeatherConsoleClient.Application.Interfaces;

namespace WeatherConsoleClient.Presentation;

public class ConsoleMenu
{
    private readonly IWeatherService _weatherService;
    private readonly IWeatherFormatter _formatter;

    private TemperatureUnit _unit = TemperatureUnit.Celsius;

    public ConsoleMenu(
        IWeatherService weatherService,
        IWeatherFormatter formatter)
    {
        _weatherService = weatherService;
        _formatter = formatter;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            DisplayMenu();

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ShowCurrentWeatherAsync(cancellationToken);
                    break;
                case "2":
                    await ShowForecastAsync(cancellationToken);
                    break;
                case "3":
                    await ShowDashboardAsync(cancellationToken);
                    break;
                case "4":
                    ChangeTemperatureUnit();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
        }
    }

    private void DisplayMenu()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("       WEATHER CONSOLE CLIENT");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("1. Current Weather");
        Console.WriteLine("2. 5-Day / 3-Hour Forecast");
        Console.WriteLine("3. Weather Dashboard");
        Console.WriteLine($"4. Settings (Unit: {_unit})");
        Console.WriteLine("0. Exit");
        Console.WriteLine();
        Console.Write("Enter your choice: ");
    }

    // Challenge 1: Temperature Unit
    private void ChangeTemperatureUnit()
    {
        Console.WriteLine();
        Console.WriteLine("Temperature Unit:");
        Console.WriteLine("1. Celsius");
        Console.WriteLine("2. Fahrenheit");
        Console.Write("Choose: ");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                _unit = TemperatureUnit.Celsius;
                Console.WriteLine("Unit set to Celsius.");
                break;
            case "2":
                _unit = TemperatureUnit.Fahrenheit;
                Console.WriteLine("Unit set to Fahrenheit.");
                break;
            default:
                Console.WriteLine("Invalid choice. Keeping current unit.");
                break;
        }
    }

    private async Task ShowCurrentWeatherAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.Write("Enter city: ");
        var city = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(city))
        {
            Console.WriteLine("City is required.");
            return;
        }

        var weather = await _weatherService.GetCurrentWeatherAsync(city, cancellationToken);

        if (weather is null)
        {
            Console.WriteLine("City not found.");
            return;
        }

        Console.Write(_formatter.FormatCurrentWeather(weather, _unit));
    }

    // Challenge 2: Forecast Filtering
    private async Task ShowForecastAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.Write("Enter city: ");
        var city = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(city))
        {
            Console.WriteLine("City is required.");
            return;
        }

        var filter = AskForecastFilter();

        var forecast = await _weatherService.GetForecastAsync(city, cancellationToken);

        if (forecast is null)
        {
            Console.WriteLine("City not found.");
            return;
        }

        Console.Write(_formatter.FormatForecast(forecast, _unit, filter));
    }

    private async Task ShowDashboardAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.Write("Enter city: ");
        var city = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(city))
        {
            Console.WriteLine("City is required.");
            return;
        }

        var filter = AskForecastFilter();

        var currentWeatherTask = _weatherService.GetCurrentWeatherAsync(city, cancellationToken);
        var forecastTask = _weatherService.GetForecastAsync(city, cancellationToken);

        await Task.WhenAll(currentWeatherTask, forecastTask);

        var currentWeather = await currentWeatherTask;
        var forecast = await forecastTask;

        if (currentWeather is null || forecast is null)
        {
            Console.WriteLine("Unable to retrieve weather information.");
            return;
        }

        Console.Write(_formatter.FormatDashboard(currentWeather, forecast, _unit, filter));
    }

    private static ForecastFilter AskForecastFilter()
    {
        Console.WriteLine();
        Console.WriteLine("Forecast filter:");
        Console.WriteLine("1. Show all forecast entries");
        Console.WriteLine("2. Show today's forecast");
        Console.WriteLine("3. Show tomorrow's forecast");
        Console.Write("Choose: ");

        var choice = Console.ReadLine();

        return choice switch
        {
            "2" => ForecastFilter.Today,
            "3" => ForecastFilter.Tomorrow,
            _ => ForecastFilter.All
        };
    }
}