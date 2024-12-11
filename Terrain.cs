using System;
using Microsoft.Xna.Framework;

namespace MeadoworldMono;

public class Terrain
{
    public void Update()
    {
        var currentSeason = Season.CurrentSeason;
    }

    public float GetResourceMultiplier()
    {
        return Season.CurrentSeason switch
        {
            SeasonType.Spring => 1.2f,
            SeasonType.Summer => 1.0f,
            SeasonType.Autumn => 0.8f,
            SeasonType.Winter => 0.6f,
            _ => 1.0f
        };
    }

    public static float GetTravelSpeedModifier(string terrainType, Weather weather, Season season)
    {
        float baseModifier = terrainType switch
        {
            "Plains" => 1.0f,
            "Forest" => 0.8f,
            "Mountains" => 0.6f,
            "Swamp" => 0.5f,
            _ => 1.0f
        };

        // Weather affects different terrain types differently
        float weatherImpact = weather.CurrentWeather switch
        {
            WeatherType.Rain when terrainType == "Swamp" => 0.5f,
            WeatherType.Rain => 0.8f,
            WeatherType.Storm when terrainType == "Mountains" => 0.4f,
            WeatherType.Storm => 0.6f,
            _ => 1.0f
        };

        // Seasonal effects on terrain
        float seasonImpact = (Season.CurrentSeason, terrainType) switch
        {
            (SeasonType.Winter, "Mountains") => 0.3f,
            (SeasonType.Winter, _) => 0.7f,
            (SeasonType.Autumn, "Swamp") => 0.7f,
            _ => 1.0f
        };

        return baseModifier * weatherImpact * seasonImpact;
    }

    public static (string message, float severity) CheckForHazards(
        string terrainType, 
        Weather weather, 
        Season season,
        Random random)
    {
        float hazardChance = (terrainType, weather.CurrentWeather) switch
        {
            ("Mountains", WeatherType.Storm) => 0.2f,
            ("Mountains", _) => 0.1f,
            ("Forest", WeatherType.Storm) => 0.15f,
            ("Swamp", _) => 0.1f,
            _ => 0.05f
        };

        // Season modifies hazard chance
        hazardChance *= Season.CurrentSeason switch
        {
            SeasonType.Winter => 1.5f,
            SeasonType.Autumn => 1.2f,
            _ => 1.0f
        };

        if (random.NextDouble() < hazardChance)
        {
            return (terrainType, weather.CurrentWeather) switch
            {
                ("Mountains", WeatherType.Storm) => ("Avalanche warning!", 0.8f),
                ("Mountains", _) => ("Treacherous path ahead", 0.5f),
                ("Forest", WeatherType.Storm) => ("Falling trees!", 0.7f),
                ("Forest", _) => ("Dense undergrowth slows progress", 0.4f),
                ("Swamp", _) => ("Dangerous quicksand nearby", 0.6f),
                _ => ("Difficult terrain ahead", 0.3f)
            };
        }

        return (null, 0f);
    }
} 