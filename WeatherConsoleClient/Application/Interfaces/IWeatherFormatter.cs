using WeatherConsoleClient.Application.DTOs;

namespace WeatherConsoleClient.Application.Interfaces;

public interface IWeatherFormatter
{
    string FormatCurrentWeather(CurrentWeatherDto weather, TemperatureUnit unit);
    string FormatForecast(ForecastDto forecast, TemperatureUnit unit, ForecastFilter filter);
    string FormatDashboard(CurrentWeatherDto current, ForecastDto forecast, TemperatureUnit unit, ForecastFilter filter);
}

public enum TemperatureUnit
{
    Celsius,
    Fahrenheit
}

public enum ForecastFilter
{
    All,
    Today,
    Tomorrow
}