using System;
using Microsoft.Xna.Framework;

namespace MeadoworldMono;

public class Weather
{
    public WeatherType CurrentWeather { get; private set; }
    public float Intensity { get; private set; } // 0.0 to 1.0
    private Random _random = new();
    private float _weatherChangeTimer;
    private const float WEATHER_CHANGE_INTERVAL = 4f; // Hours
    private Season _season;
    private float _seasonalStormChance;

    public Weather(Season season)
    {
        _season = season;
        CurrentWeather = WeatherType.Clear;
        Intensity = 0f;
    }

    private void UpdateSeasonalValues()
    {
        _seasonalStormChance = Season.CurrentSeason switch
        {
            SeasonType.Spring => 0.3f,  // Spring showers
            SeasonType.Summer => 0.2f,  // Occasional storms
            SeasonType.Autumn => 0.4f,  // Rainy season
            SeasonType.Winter => 0.5f,  // Winter storms
            _ => 0.3f
        };
    }

    public void Update(GameTime gameTime)
    {
        UpdateSeasonalValues();
        switch (Season.CurrentSeason)
        {
            case SeasonType.Spring:
                // Spring weather logic
                break;
            case SeasonType.Summer:
                // Summer weather logic
                break;
            case SeasonType.Autumn:
                // Autumn weather logic
                break;
            case SeasonType.Winter:
                // Winter weather logic
                break;
        }
    }

    private void UpdateWeather()
    {
        if (_random.NextDouble() < 0.3f)
        {
            CurrentWeather = _random.NextDouble() switch
            {
                var n when n < (0.6 - _seasonalStormChance) => WeatherType.Clear,
                var n when n < 0.8 => WeatherType.Rain,
                _ => WeatherType.Storm
            };
            
            Intensity = (float)_random.NextDouble();
        }
    }

    public float GetGatheringModifier()
    {
        return CurrentWeather switch
        {
            WeatherType.Clear => 1.0f,
            WeatherType.Rain => 0.7f - (Intensity * 0.2f),
            WeatherType.Storm => 0.4f - (Intensity * 0.3f),
            _ => 1.0f
        };
    }

    public string GetWeatherDescription()
    {
        string intensity = Intensity switch
        {
            < 0.3f => "Light",
            < 0.7f => "Moderate",
            _ => "Heavy"
        };

        return CurrentWeather switch
        {
            WeatherType.Clear => "Clear Weather",
            WeatherType.Rain => $"{intensity} Rain",
            WeatherType.Storm => $"{intensity} Storm",
            _ => "Unknown Weather"
        };
    }

    public float GetMovementSpeedModifier()
    {
        return CurrentWeather switch
        {
            WeatherType.Clear => 1.0f,
            WeatherType.Rain => 0.8f - (Intensity * 0.2f),
            WeatherType.Storm => 0.5f - (Intensity * 0.3f),
            _ => 1.0f
        };
    }

    public float GetTradePriceMultiplier()
    {
        return 1.0f;
    }

    public float GetEncounterChanceModifier()
    {
        return 1.0f;
    }

    public float GetTravelSpeedModifier()
    {
        return CurrentWeather switch
        {
            WeatherType.Clear => 1.0f,
            WeatherType.Rain => 0.8f - (Intensity * 0.2f),
            WeatherType.Storm => 0.5f - (Intensity * 0.3f),
            _ => 1.0f
        };
    }
}

public enum WeatherType
{
    Clear,
    Cloudy,
    Rain,
    Storm,
    Fog,
    Snow
}