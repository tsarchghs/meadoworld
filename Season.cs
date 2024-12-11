namespace MeadoworldMono;

public enum SeasonType
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public class Season
{
    public static SeasonType CurrentSeason { get; private set; } = SeasonType.Spring;
    public SeasonType Type { get; private set; }

    public Season()
    {
        Type = SeasonType.Spring;
    }

    public Season(SeasonType type)
    {
        Type = type;
    }

    public static void SetCurrentSeason(SeasonType type)
    {
        CurrentSeason = type;
    }

    public float GetTravelSpeedModifier()
    {
        return Type switch
        {
            SeasonType.Spring => 1.0f,
            SeasonType.Summer => 1.2f,
            SeasonType.Autumn => 0.9f,
            SeasonType.Winter => 0.6f,
            _ => 1.0f
        };
    }

    public string GetSeasonDescription()
    {
        return Type switch
        {
            SeasonType.Spring => "The weather is mild and roads are clearing.",
            SeasonType.Summer => "Perfect weather for traveling.",
            SeasonType.Autumn => "Rain makes travel slower.",
            SeasonType.Winter => "Snow and ice make travel difficult.",
            _ => "Unknown season"
        };
    }

    public void AdvanceDay()
    {
        // Implementation for advancing the season if enough days have passed
    }

    public float GetFoodPriceMultiplier()
    {
        return Type switch
        {
            SeasonType.Spring => 1.0f,
            SeasonType.Summer => 0.8f,
            SeasonType.Autumn => 1.2f,
            SeasonType.Winter => 1.5f,
            _ => 1.0f
        };
    }

    public float GetResourcePriceMultiplier()
    {
        return Type switch
        {
            SeasonType.Spring => 1.2f,
            SeasonType.Summer => 1.0f,
            SeasonType.Autumn => 0.8f,
            SeasonType.Winter => 1.3f,
            _ => 1.0f
        };
    }

    public float GetEncounterChanceModifier()
    {
        return Type switch
        {
            SeasonType.Spring => 1.0f,
            SeasonType.Summer => 1.2f,
            SeasonType.Autumn => 1.0f,
            SeasonType.Winter => 0.7f,
            _ => 1.0f
        };
    }
} 