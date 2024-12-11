using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MeadoworldMono;

public class Battle
{
    public Party PlayerParty { get; private set; }
    public Party EnemyParty { get; private set; }
    public BattlePhase CurrentPhase { get; private set; }
    public List<BattleUnit> PlayerUnits { get; private set; }
    public List<BattleUnit> EnemyUnits { get; private set; }
    public Vector2 BattlefieldSize { get; private set; }
    public float TimeElapsed { get; private set; }
    private Random _random;
    private List<BattleEvent> _eventLog;
    public bool IsComplete { get; private set; }
    public bool PlayerWon { get; private set; }
    public float BattleProgress { get; private set; }
    public BattleRewards Rewards { get; private set; }

    public Battle(Party playerParty, Party enemyParty)
    {
        PlayerParty = playerParty;
        EnemyParty = enemyParty;
        CurrentPhase = BattlePhase.Deployment;
        PlayerUnits = new List<BattleUnit>();
        EnemyUnits = new List<BattleUnit>();
        BattlefieldSize = new Vector2(1000, 600);
        _random = new Random();
        _eventLog = new List<BattleEvent>();
        
        InitializeBattleUnits();
        BattleProgress = 0f;
        PlayerWon = false;
        Rewards = new BattleRewards(
            gold: 0f,
            items: new Dictionary<Item, int>(),
            prisoners: new List<Troop>()
        );
    }

    private void InitializeBattleUnits()
    {
        // Convert party troops to battle units
        foreach (var troopEntry in PlayerParty.Troops)
        {
            for (int i = 0; i < troopEntry.Value; i++)
            {
                PlayerUnits.Add(new BattleUnit(
                    troopEntry.Key,
                    GetRandomDeploymentPosition(true),
                    true));
            }
        }

        foreach (var troopEntry in EnemyParty.Troops)
        {
            for (int i = 0; i < troopEntry.Value; i++)
            {
                EnemyUnits.Add(new BattleUnit(
                    troopEntry.Key,
                    GetRandomDeploymentPosition(false),
                    false));
            }
        }
    }

    private Vector2 GetRandomDeploymentPosition(bool isPlayer)
    {
        float x = isPlayer ? 
            _random.Next(50, 200) : 
            _random.Next((int)BattlefieldSize.X - 200, (int)BattlefieldSize.X - 50);
        float y = _random.Next(50, (int)BattlefieldSize.Y - 50);
        return new Vector2(x, y);
    }

    public void Update(float deltaTime)
    {
        TimeElapsed += deltaTime;
        UpdateBattleProgress();

        switch (CurrentPhase)
        {
            case BattlePhase.Deployment:
                UpdateDeployment();
                break;
            case BattlePhase.Combat:
                UpdateCombat(deltaTime);
                CheckBattleEnd();
                break;
            case BattlePhase.Finished:
                // Battle is over, no updates needed
                break;
        }
    }

    private void UpdateBattleProgress()
    {
        int totalUnits = PlayerUnits.Count + EnemyUnits.Count;
        int remainingUnits = PlayerUnits.Count(u => !u.IsDead) + EnemyUnits.Count(u => !u.IsDead);
        BattleProgress = 1f - (remainingUnits / (float)totalUnits);
    }

    private void CheckBattleEnd()
    {
        int alivePlayers = PlayerUnits.Count(u => !u.IsDead);
        int aliveEnemies = EnemyUnits.Count(u => !u.IsDead);

        if (alivePlayers == 0 || aliveEnemies == 0)
        {
            IsComplete = true;
            PlayerWon = alivePlayers > 0;
            CurrentPhase = BattlePhase.Finished;
            GenerateRewards();
        }
    }

    public void GenerateRewards()
    {
        if (!PlayerWon)
        {
            Rewards = new BattleRewards(
                gold: 0f,
                items: new Dictionary<Item, int>(),
                prisoners: new List<Troop>()
            );
            return;
        }

        // Calculate base reward based on enemy party strength
        float baseReward = EnemyParty.GetTotalTroops() * 10f;
        
        // Add bonus for remaining player troops
        float survivorBonus = PlayerUnits.Count(u => !u.IsDead) / (float)PlayerUnits.Count;
        baseReward *= (1f + survivorBonus);

        // Generate loot
        Dictionary<Item, int> loot = GenerateLoot();

        Rewards = new BattleRewards(
            gold: (int)baseReward,
            items: loot,
            prisoners: new List<Troop>()
        );
    }

    private Dictionary<Item, int> GenerateLoot()
    {
        var loot = new Dictionary<Item, int>();
        // Add some basic loot based on enemy party
        // This is a placeholder implementation
        return loot;
    }

    private void UpdateDeployment()
    {
        // Allow unit repositioning
        // When ready, move to combat phase
        if (Keyboard.GetState().IsKeyDown(Keys.Space))
        {
            CurrentPhase = BattlePhase.Combat;
        }
    }

    private void UpdateCombat(float deltaTime)
    {
        // Update all units
        foreach (var unit in PlayerUnits.ToList())
        {
            UpdateUnit(unit, deltaTime, EnemyUnits);
        }

        foreach (var unit in EnemyUnits.ToList())
        {
            UpdateUnit(unit, deltaTime, PlayerUnits);
        }

        // Check for battle end conditions
        if (PlayerUnits.Count == 0 || EnemyUnits.Count == 0)
        {
            CurrentPhase = BattlePhase.Finished;
        }
    }

    private void UpdateUnit(BattleUnit unit, float deltaTime, List<BattleUnit> enemies)
    {
        if (unit.IsDead) return;

        // Find nearest enemy
        var nearestEnemy = enemies
            .Where(e => !e.IsDead)
            .OrderBy(e => Vector2.Distance(e.Position, unit.Position))
            .FirstOrDefault();

        if (nearestEnemy == null) return;

        // Move towards enemy if not in range
        float attackRange = unit.Troop.AttackRange;
        Vector2 toEnemy = nearestEnemy.Position - unit.Position;
        float distance = toEnemy.Length();

        if (distance > attackRange)
        {
            toEnemy.Normalize();
            unit.Position += toEnemy * unit.Troop.Speed * deltaTime;
        }
        else
        {
            // Attack if in range
            unit.AttackTimer += deltaTime;
            if (unit.AttackTimer >= unit.Troop.AttackSpeed)
            {
                unit.AttackTimer = 0;
                Attack(unit, nearestEnemy);
            }
        }
    }

    private void Attack(BattleUnit attacker, BattleUnit defender)
    {
        float damage = attacker.Troop.Damage;
        
        // Apply random variation
        damage *= 0.8f + (float)_random.NextDouble() * 0.4f;
        
        // Apply armor reduction
        damage *= (100f / (100f + defender.Troop.Armor));

        defender.Health -= damage;
        
        _eventLog.Add(new BattleEvent(
            $"{attacker.Troop.Name} dealt {damage:F1} damage to {defender.Troop.Name}",
            TimeElapsed
        ));

        if (defender.Health <= 0)
        {
            defender.IsDead = true;
            if (defender.IsPlayerUnit)
            {
                PlayerUnits.Remove(defender);
            }
            else
            {
                EnemyUnits.Remove(defender);
            }
            
            _eventLog.Add(new BattleEvent(
                $"{defender.Troop.Name} was defeated!",
                TimeElapsed
            ));
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D unitTexture, SpriteFont font)
    {
        // Draw battlefield background
        spriteBatch.Draw(CreateBattlefieldTexture(spriteBatch.GraphicsDevice), Vector2.Zero, Color.White);

        // Draw units
        foreach (var unit in PlayerUnits.Concat(EnemyUnits))
        {
            Color unitColor = unit.IsPlayerUnit ? Color.Blue : Color.Red;
            if (unit.IsDead) unitColor *= 0.5f;
            
            spriteBatch.Draw(unitTexture, unit.Position, null, unitColor, 0f, 
                new Vector2(unitTexture.Width / 2, unitTexture.Height / 2), 1f, 
                SpriteEffects.None, 0f);

            if (font != null)
            {
                // Draw health bar
                DrawHealthBar(spriteBatch, unit);
                
                // Draw unit type
                Vector2 textPos = unit.Position + new Vector2(0, 20);
                string text = $"{unit.Troop.Name} ({unit.Health:F0})";
                Vector2 textSize = font.MeasureString(text);
                spriteBatch.DrawString(font, text, textPos - textSize / 2, unitColor);
            }
        }

        // Draw battle log
        if (font != null)
        {
            DrawBattleLog(spriteBatch, font);
        }
    }

    private void DrawHealthBar(SpriteBatch spriteBatch, BattleUnit unit)
    {
        Vector2 barPos = unit.Position + new Vector2(-15, -20);
        Rectangle backgroundBar = new Rectangle((int)barPos.X, (int)barPos.Y, 30, 5);
        Rectangle healthBar = new Rectangle((int)barPos.X, (int)barPos.Y, 
            (int)(30 * (unit.Health / unit.Troop.MaxHealth)), 5);

        spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), backgroundBar, Color.DarkGray);
        spriteBatch.Draw(CreatePixelTexture(spriteBatch.GraphicsDevice), healthBar, Color.Green);
    }

    private void DrawBattleLog(SpriteBatch spriteBatch, SpriteFont font)
    {
        Vector2 logPos = new Vector2(10, 10);
        foreach (var evt in _eventLog.TakeLast(5))
        {
            spriteBatch.DrawString(font, evt.Message, logPos, Color.White);
            logPos.Y += 20;
        }
    }

    private Texture2D CreateBattlefieldTexture(GraphicsDevice graphics)
    {
        Texture2D texture = new Texture2D(graphics, (int)BattlefieldSize.X, (int)BattlefieldSize.Y);
        Color[] data = new Color[texture.Width * texture.Height];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Color.ForestGreen;
        }
        texture.SetData(data);
        return texture;
    }

    private Texture2D CreatePixelTexture(GraphicsDevice graphics)
    {
        Texture2D texture = new Texture2D(graphics, 1, 1);
        texture.SetData(new[] { Color.White });
        return texture;
    }

    public bool HasPlayerWon()
    {
        return PlayerWon;
    }

    public BattleRewards GetRewards()
    {
        float gold = 100f; // Example value
        var items = new Dictionary<Item, int>();
        var prisoners = new List<Troop>();
        return new BattleRewards(gold, items, prisoners);
    }
}

public enum BattlePhase
{
    Deployment,
    Combat,
    Finished
}

public class BattleEvent
{
    public string Message { get; set; }
    public float Time { get; set; }

    public BattleEvent(string message, float time)
    {
        Message = message;
        Time = time;
    }
} 