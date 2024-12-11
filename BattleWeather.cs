using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public enum WeatherType
{
    Clear,
    Cloudy,
    Rain,
    Storm,
    Fog,
    Snow
}

public class BattleWeather
{
    public WeatherType Type { get; private set; }
    public float Intensity { get; private set; }
    public float TimeOfDay { get; private set; } // 0-24 hours
    private Random _random;
    private List<WeatherParticle> _particles;
    private const int MAX_PARTICLES = 1000;

    public BattleWeather(WeatherType type, float timeOfDay)
    {
        Type = type;
        TimeOfDay = timeOfDay;
        Intensity = 1.0f;
        _random = new Random();
        _particles = new List<WeatherParticle>();
    }

    public WeatherEffects GetEffects()
    {
        return new WeatherEffects(
            GetVisibilityModifier(),
            GetMovementModifier(),
            GetRangedModifier(),
            GetMoraleModifier()
        );
    }

    private float GetVisibilityModifier()
    {
        // Base visibility from time of day
        float timeModifier = TimeOfDay switch
        {
            < 6 => 0.5f,  // Night
            < 8 => 0.7f,  // Dawn
            < 18 => 1.0f, // Day
            < 20 => 0.7f, // Dusk
            _ => 0.5f     // Night
        };

        // Weather effects on visibility
        float weatherModifier = Type switch
        {
            WeatherType.Clear => 1.0f,
            WeatherType.Cloudy => 0.9f,
            WeatherType.Rain => 0.7f,
            WeatherType.Storm => 0.5f,
            WeatherType.Fog => 0.3f,
            WeatherType.Snow => 0.6f,
            _ => 1.0f
        };

        return timeModifier * weatherModifier * Intensity;
    }

    private float GetMovementModifier()
    {
        return Type switch
        {
            WeatherType.Clear => 1.0f,
            WeatherType.Rain => 0.8f,
            WeatherType.Storm => 0.6f,
            WeatherType.Snow => 0.7f,
            _ => 0.9f
        } * Intensity;
    }

    private float GetRangedModifier()
    {
        return Type switch
        {
            WeatherType.Clear => 1.0f,
            WeatherType.Rain => 0.7f,
            WeatherType.Storm => 0.4f,
            WeatherType.Snow => 0.6f,
            WeatherType.Fog => 0.5f,
            _ => 0.8f
        } * Intensity;
    }

    private float GetMoraleModifier()
    {
        return Type switch
        {
            WeatherType.Clear => 1.0f,
            WeatherType.Storm => 0.7f,
            WeatherType.Snow => 0.8f,
            _ => 0.9f
        } * Intensity;
    }

    public void Update(float deltaTime, Vector2 battlefieldSize)
    {
        // Update existing particles
        _particles.RemoveAll(p => !p.Update(deltaTime, battlefieldSize));

        // Generate new particles based on weather type
        if (_particles.Count < MAX_PARTICLES)
        {
            int particlesToAdd = Type switch
            {
                WeatherType.Rain => 20,
                WeatherType.Storm => 30,
                WeatherType.Snow => 15,
                _ => 0
            };

            for (int i = 0; i < particlesToAdd; i++)
            {
                _particles.Add(CreateParticle(battlefieldSize));
            }
        }
    }

    private WeatherParticle CreateParticle(Vector2 battlefieldSize)
    {
        Vector2 position = new(
            _random.Next(0, (int)battlefieldSize.X),
            -10 // Start above screen
        );

        Vector2 velocity = Type switch
        {
            WeatherType.Rain => new Vector2(2f, 15f),
            WeatherType.Storm => new Vector2(4f, 20f),
            WeatherType.Snow => new Vector2(1f, 3f),
            _ => Vector2.Zero
        };

        return new WeatherParticle(position, velocity * Intensity);
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D particleTexture)
    {
        Color particleColor = Type switch
        {
            WeatherType.Rain => new Color(200, 200, 255, 128),
            WeatherType.Storm => new Color(180, 180, 255, 160),
            WeatherType.Snow => new Color(255, 255, 255, 200),
            _ => Color.White
        };

        foreach (var particle in _particles)
        {
            spriteBatch.Draw(
                particleTexture,
                particle.Position,
                null,
                particleColor * 0.5f,
                0f,
                Vector2.Zero,
                Type == WeatherType.Snow ? 2f : 1f,
                SpriteEffects.None,
                0f
            );
        }

        // Draw weather overlay
        if (Type != WeatherType.Clear)
        {
            spriteBatch.Draw(
                particleTexture,
                Vector2.Zero,
                null,
                GetOverlayColor(),
                0f,
                Vector2.Zero,
                Vector2.One * 1000,
                SpriteEffects.None,
                0f
            );
        }
    }

    private Color GetOverlayColor()
    {
        return Type switch
        {
            WeatherType.Fog => new Color(200, 200, 200, 50),
            WeatherType.Storm => new Color(50, 50, 70, 30),
            WeatherType.Rain => new Color(100, 100, 120, 20),
            WeatherType.Snow => new Color(250, 250, 255, 20),
            _ => Color.Transparent
        };
    }
}

public class WeatherEffects
{
    public float VisibilityModifier { get; }
    public float MovementModifier { get; }
    public float RangedModifier { get; }
    public float MoraleModifier { get; }

    public WeatherEffects(float visibility, float movement, float ranged, float morale)
    {
        VisibilityModifier = visibility;
        MovementModifier = movement;
        RangedModifier = ranged;
        MoraleModifier = morale;
    }
}

public class WeatherParticle
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }

    public WeatherParticle(Vector2 position, Vector2 velocity)
    {
        Position = position;
        Velocity = velocity;
    }

    public bool Update(float deltaTime, Vector2 battlefieldSize)
    {
        Position += Velocity * deltaTime;
        return Position.Y < battlefieldSize.Y && Position.X > 0 && Position.X < battlefieldSize.X;
    }
} 