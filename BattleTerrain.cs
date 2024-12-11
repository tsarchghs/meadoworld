using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MeadoworldMono;

namespace MeadoworldMono
{
    public class BattleTerrain
    {
        public TerrainType Type { get; private set; }
        public float[,] HeightMap { get; private set; }
        public bool[,] CoverMap { get; private set; }
        public Vector2 Size { get; private set; }
        private Random _random;

        public BattleTerrain(TerrainType type, Vector2 size)
        {
            Type = type;
            Size = size;
            _random = new Random();
            
            // Initialize maps
            int width = (int)size.X;
            int height = (int)size.Y;
            HeightMap = new float[width, height];
            CoverMap = new bool[width, height];
            
            GenerateTerrain();
        }

        private void GenerateTerrain()
        {
            // Generate height map using Perlin noise
            float scale = Type switch
            {
                TerrainType.Plains => 0.01f,
                TerrainType.Mountains => 0.05f,
                _ => 0.02f
            };

            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    HeightMap[x, y] = Noise.Generate(x * scale, y * scale);
                }
            }

            // Generate cover (trees, rocks, etc.)
            float coverDensity = Type switch
            {
                TerrainType.Forest => 0.3f,
                TerrainType.Mountains => 0.2f,
                TerrainType.Plains => 0.05f,
                _ => 0.1f
            };

            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    CoverMap[x, y] = _random.NextDouble() < coverDensity;
                }
            }
        }

        public TerrainEffects GetEffectsAt(Vector2 position)
        {
            int x = (int)MathHelper.Clamp(position.X, 0, Size.X - 1);
            int y = (int)MathHelper.Clamp(position.Y, 0, Size.Y - 1);

            float height = HeightMap[x, y];
            bool hasCover = CoverMap[x, y];

            return new TerrainEffects(
                GetMovementModifier(height, hasCover),
                GetRangedModifier(height, hasCover),
                GetMeleeModifier(height, hasCover),
                GetVisibilityModifier(height, hasCover)
            );
        }

        private float GetMovementModifier(float height, bool hasCover)
        {
            float modifier = 1f;
            
            // Height effects
            modifier *= 1f - (height * 0.5f);
            
            // Terrain type effects
            modifier *= Type switch
            {
                TerrainType.Swamp => 0.6f,
                TerrainType.Forest => 0.8f,
                TerrainType.Mountains => 0.7f,
                TerrainType.Desert => 0.9f,
                _ => 1f
            };

            // Cover effects
            if (hasCover) modifier *= 0.8f;

            return MathHelper.Clamp(modifier, 0.2f, 1f);
        }

        private float GetRangedModifier(float height, bool hasCover)
        {
            float modifier = 1f;
            
            // Height advantage
            modifier *= 1f + (height * 0.3f);
            
            // Cover penalty
            if (hasCover) modifier *= 0.7f;
            
            // Terrain type effects
            modifier *= Type switch
            {
                TerrainType.Forest => 0.6f,
                TerrainType.Mountains => 1.2f,
                _ => 1f
            };

            return MathHelper.Clamp(modifier, 0.3f, 1.5f);
        }

        private float GetMeleeModifier(float height, bool hasCover)
        {
            float modifier = 1f;
            
            // Height advantage
            modifier *= 1f + (height * 0.2f);
            
            // Terrain type effects
            modifier *= Type switch
            {
                TerrainType.Forest => 0.9f,
                TerrainType.Mountains => 0.8f,
                TerrainType.Swamp => 0.7f,
                _ => 1f
            };

            return MathHelper.Clamp(modifier, 0.6f, 1.3f);
        }

        private float GetVisibilityModifier(float height, bool hasCover)
        {
            float modifier = 1f;
            
            // Height advantage
            modifier *= 1f + (height * 0.4f);
            
            // Cover effects
            if (hasCover) modifier *= 0.6f;
            
            // Terrain type effects
            modifier *= Type switch
            {
                TerrainType.Forest => 0.7f,
                TerrainType.Mountains => 1.3f,
                TerrainType.Desert => 1.2f,
                _ => 1f
            };

            return MathHelper.Clamp(modifier, 0.4f, 1.5f);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D terrainTexture)
        {
            // Draw base terrain
            Color terrainColor = Type switch
            {
                TerrainType.Plains => Color.LightGreen,
                TerrainType.Forest => Color.DarkGreen,
                TerrainType.Hills => Color.SandyBrown,
                TerrainType.Mountains => Color.Gray,
                TerrainType.Swamp => Color.DarkOliveGreen,
                TerrainType.Desert => Color.Wheat,
                _ => Color.White
            };

            spriteBatch.Draw(terrainTexture, Vector2.Zero, terrainColor);

            // Draw height map variations
            for (int x = 0; x < Size.X; x += 10)
            {
                for (int y = 0; y < Size.Y; y += 10)
                {
                    float height = HeightMap[x, y];
                    Color heightColor = terrainColor * (0.8f + height * 0.4f);
                    spriteBatch.Draw(terrainTexture, 
                        new Vector2(x, y), 
                        new Rectangle(0, 0, 10, 10), 
                        heightColor);
                }
            }

            // Draw cover
            Texture2D coverTexture = CreateCoverTexture(spriteBatch.GraphicsDevice);
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    if (CoverMap[x, y])
                    {
                        spriteBatch.Draw(coverTexture, 
                            new Vector2(x * 10, y * 10), 
                            Color.White * 0.7f);
                    }
                }
            }
        }

        private Texture2D CreateCoverTexture(GraphicsDevice graphics)
        {
            Texture2D texture = new Texture2D(graphics, 10, 10);
            Color[] data = new Color[100];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Color.DarkGreen;
            }
            texture.SetData(data);
            return texture;
        }
    }
}

public class TerrainEffects
{
    public float MovementModifier { get; }
    public float RangedModifier { get; }
    public float MeleeModifier { get; }
    public float VisibilityModifier { get; }

    public TerrainEffects(float movement, float ranged, float melee, float visibility)
    {
        MovementModifier = movement;
        RangedModifier = ranged;
        MeleeModifier = melee;
        VisibilityModifier = visibility;
    }
}

// Simple Perlin noise implementation
public static class Noise
{
    public static float Generate(float x, float y)
    {
        const float F2 = 0.366025403f;
        const float G2 = 0.211324865f;

        float n0, n1, n2;

        float s = (x + y) * F2;
        float xs = x + s;
        float ys = y + s;
        int i = FastFloor(xs);
        int j = FastFloor(ys);

        float t = (i + j) * G2;
        float X0 = i - t;
        float Y0 = j - t;
        float x0 = x - X0;
        float y0 = y - Y0;

        int i1, j1;
        if (x0 > y0) { i1 = 1; j1 = 0; }
        else { i1 = 0; j1 = 1; }

        float x1 = x0 - i1 + G2;
        float y1 = y0 - j1 + G2;
        float x2 = x0 - 1.0f + 2.0f * G2;
        float y2 = y0 - 1.0f + 2.0f * G2;

        int ii = i & 255;
        int jj = j & 255;

        float t0 = 0.5f - x0 * x0 - y0 * y0;
        if (t0 < 0.0f) n0 = 0.0f;
        else
        {
            t0 *= t0;
            n0 = t0 * t0 * Grad(ii + jj, x0, y0);
        }

        float t1 = 0.5f - x1 * x1 - y1 * y1;
        if (t1 < 0.0f) n1 = 0.0f;
        else
        {
            t1 *= t1;
            n1 = t1 * t1 * Grad(ii + i1 + jj + j1, x1, y1);
        }

        float t2 = 0.5f - x2 * x2 - y2 * y2;
        if (t2 < 0.0f) n2 = 0.0f;
        else
        {
            t2 *= t2;
            n2 = t2 * t2 * Grad(ii + 1 + jj + 1, x2, y2);
        }

        return 40.0f * (n0 + n1 + n2);
    }

    private static int FastFloor(float x)
    {
        return x > 0 ? (int)x : (int)x - 1;
    }

    private static float Grad(int hash, float x, float y)
    {
        int h = hash & 7;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
    }
} 