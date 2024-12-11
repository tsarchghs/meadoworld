using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MeadoworldMono;

public class CaravanManager
{
    private List<Caravan> _caravans;
    private Random _random;
    private List<Location> _locations;
    private const int MAX_CARAVANS = 5;
    private const float CARAVAN_SPAWN_INTERVAL = 24f; // Game hours
    private float _timeSinceLastSpawn;

    public CaravanManager(List<Location> locations)
    {
        if (locations == null || locations.Count < 2)
            throw new ArgumentException("CaravanManager requires at least 2 locations");
        
        _caravans = new List<Caravan>();
        _random = new Random();
        _locations = locations;
        _timeSinceLastSpawn = 0f;
        InitializeCaravans();
    }

    private void InitializeCaravans()
    {
        for (int i = 0; i < MAX_CARAVANS / 2; i++)
        {
            SpawnNewCaravan();
        }
    }

    private void SpawnNewCaravan()
    {
        if (_caravans.Count >= MAX_CARAVANS) return;
        if (_locations == null || _locations.Count < 2) return;

        // Select random start and end locations
        var startLocation = _locations[_random.Next(_locations.Count)];
        var endLocation = _locations
            .Where(l => l != startLocation)
            .OrderBy(_ => _random.Next())
            .First();

        string name = $"Caravan {_caravans.Count + 1}";
        _caravans.Add(new Caravan(name, startLocation, endLocation, _random));
    }

    public void Update(float deltaTime, Weather weather, Season season)
    {
        _timeSinceLastSpawn += deltaTime;

        if (_timeSinceLastSpawn >= CARAVAN_SPAWN_INTERVAL)
        {
            _timeSinceLastSpawn = 0f;
            SpawnNewCaravan();
        }

        foreach (var caravan in _caravans)
        {
            caravan.Update(deltaTime, weather, season);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D caravanTexture)
    {
        foreach (var caravan in _caravans)
        {
            spriteBatch.Draw(caravanTexture,
                caravan.Position - new Vector2(caravanTexture.Width / 2, caravanTexture.Height / 2),
                Color.White);
        }
    }

    public Caravan GetNearestCaravan(Vector2 position)
    {
        return _caravans
            .OrderBy(c => Vector2.Distance(c.Position, position))
            .FirstOrDefault();
    }
} 