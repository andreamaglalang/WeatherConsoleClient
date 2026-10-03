using System.Globalization;
using System.Text;
using WeatherConsoleClient.Application.DTOs;
using WeatherConsoleClient.Application.Interfaces;

namespace WeatherConsoleClient.Presentation;

public class ConsoleWeatherFormatter : IWeatherFormatter
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public string FormatCurrentWeather(CurrentWeatherDto weather, TemperatureUnit unit)
    {
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine("========================================");
        sb.AppendLine("CURRENT WEATHER");
        sb.AppendLine("========================================");
        sb.AppendLine();
        sb.AppendLine($"City        : {weather.Name}");
        sb.AppendLine($"Temperature : {FormatTemp(weather.Main.Temperature, unit)}");
        sb.AppendLine($"Feels Like  : {FormatTemp(weather.Main.FeelsLike, unit)}");
        sb.AppendLine($"Humidity    : {weather.Main.Humidity} %");
        sb.AppendLine($"Pressure    : {weather.Main.Pressure} hPa");

        if (weather.Weather.Count > 0)
            sb.AppendLine($"Condition   : {weather.Weather[0].Description}");

        sb.AppendLine($"Wind Speed  : {weather.Wind.Speed:F2} m/s");

        if (weather.Main.Temperature > 35m)
        {
            sb.AppendLine();
            sb.AppendLine("WEATHER ALERT:");
            sb.AppendLine("High temperature detected.");
        }

        return sb.ToString();
    }

    public string FormatForecast(ForecastDto forecast, TemperatureUnit unit, ForecastFilter filter)
    {
        var sb = new StringBuilder();

        var items = FilterForecast(forecast.Items, filter);

        sb.AppendLine();
        sb.AppendLine("========================================");
        sb.AppendLine("5-DAY / 3-HOUR FORECAST");
        sb.AppendLine("========================================");
        sb.AppendLine();
        sb.AppendLine($"City: {forecast.City.Name}");
        sb.AppendLine($"Filter: {filter}");
        sb.AppendLine();
        sb.AppendLine($"{"Date & Time",-15}{"Temp",10}{"Condition",-20}{"Rain",8}");
        sb.AppendLine(new string('-', 53));

        bool rainAlerted = false;

        foreach (var item in items)
        {
            var condition = item.Weather.Count > 0 ? item.Weather[0].Description : "Unknown";
            var precipitation = item.ProbabilityOfPrecipitation * 100;
            var dateText = FormatDateTime(item.DateTimeText);

            sb.AppendLine(
                $"{dateText,-15}" +
                $"{FormatTemp(item.Main.Temperature, unit),9}  " +
                $"{condition,-18}" +
                $"{precipitation,5:F0}%");

            if (item.ProbabilityOfPrecipitation >= 0.6m && !rainAlerted)
            {
                sb.AppendLine();
                sb.AppendLine("RAIN ALERT: High probability of precipitation.");
                rainAlerted = true;
            }
        }

        if (items.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("FORECAST SUMMARY");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Highest Temperature : {FormatTemp(items.Max(i => i.Main.Temperature), unit)}");
            sb.AppendLine($"Lowest Temperature  : {FormatTemp(items.Min(i => i.Main.Temperature), unit)}");
            sb.AppendLine($"Average Temperature : {FormatTemp(items.Average(i => i.Main.Temperature), unit)}");
            sb.AppendLine($"Highest Rain Chance : {items.Max(i => i.ProbabilityOfPrecipitation) * 100:F0} %");
        }
        else
        {
            sb.AppendLine();
            sb.AppendLine("No forecast entries for the selected filter.");
        }

        return sb.ToString();
    }

    public string FormatDashboard(CurrentWeatherDto current, ForecastDto forecast, TemperatureUnit unit, ForecastFilter filter)
    {
        var sb = new StringBuilder();

        sb.AppendLine();
        sb.AppendLine("========================================");
        sb.AppendLine("WEATHER DASHBOARD");
        sb.AppendLine("========================================");
        sb.AppendLine();
        sb.AppendLine($"City: {current.Name}");
        sb.AppendLine();
        sb.AppendLine("CURRENT WEATHER");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"Temperature : {FormatTemp(current.Main.Temperature, unit)}");
        sb.AppendLine($"Feels Like  : {FormatTemp(current.Main.FeelsLike, unit)}");
        sb.AppendLine($"Humidity    : {current.Main.Humidity}%");

        if (current.Weather.Count > 0)
            sb.AppendLine($"Condition   : {current.Weather[0].Description}");

        if (current.Main.Temperature > 35m)
        {
            sb.AppendLine();
            sb.AppendLine("WEATHER ALERT:");
            sb.AppendLine("High temperature detected.");
        }

        sb.AppendLine();
        sb.AppendLine("FORECAST");
        sb.AppendLine("----------------------------------------");

        var items = FilterForecast(forecast.Items, filter);
        bool rainAlerted = false;

        foreach (var item in items)
        {
            var condition = item.Weather.Count > 0 ? item.Weather[0].Description : "Unknown";
            var precipitation = item.ProbabilityOfPrecipitation * 100;
            var dateText = FormatDateTime(item.DateTimeText);

            sb.AppendLine(
                $"{dateText,-15}" +
                $"{FormatTemp(item.Main.Temperature, unit),9}  " +
                $"{condition,-18}" +
                $"{precipitation,5:F0}%");

            if (item.ProbabilityOfPrecipitation >= 0.6m && !rainAlerted)
            {
                sb.AppendLine();
                sb.AppendLine("RAIN ALERT: High probability of precipitation.");
                rainAlerted = true;
            }
        }

        if (items.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("FORECAST SUMMARY");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Highest Temperature : {FormatTemp(items.Max(i => i.Main.Temperature), unit)}");
            sb.AppendLine($"Lowest Temperature  : {FormatTemp(items.Min(i => i.Main.Temperature), unit)}");
            sb.AppendLine($"Average Temperature : {FormatTemp(items.Average(i => i.Main.Temperature), unit)}");
            sb.AppendLine($"Highest Rain Chance : {items.Max(i => i.ProbabilityOfPrecipitation) * 100:F0} %");
        }

        return sb.ToString();
    }

    private static string FormatTemp(decimal celsius, TemperatureUnit unit)
    {
        if (unit == TemperatureUnit.Fahrenheit)
        {
            var f = celsius * 9m / 5m + 32m;
            return $"{f:F1} °F";
        }
        return $"{celsius:F2} °C";
    }

    private static string FormatDateTime(string apiDateTime)
    {
        if (DateTime.TryParse(apiDateTime, Culture, DateTimeStyles.None, out var dt))
            return dt.ToString("MMM dd HH:mm", Culture);

        return apiDateTime;
    }

    private static List<ForecastItemDto> FilterForecast(List<ForecastItemDto> items, ForecastFilter filter)
    {
        if (filter == ForecastFilter.All)
            return items;

        var target = filter == ForecastFilter.Today
            ? DateTime.Today
            : DateTime.Today.AddDays(1);

        return items
            .Where(i =>
                DateTime.TryParse(i.DateTimeText, out var dt) &&
                dt.Date == target)
            .ToList();
    }
}