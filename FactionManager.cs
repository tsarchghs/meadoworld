using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MeadoworldMono
{
    public class FactionManager
    {
        private Dictionary<string, Faction> _factions;
        private static FactionManager _instance;

        public static FactionManager Instance
        {
            get
            {
                _instance ??= new FactionManager();
                return _instance;
            }
        }

        public FactionManager()
        {
            _factions = new Dictionary<string, Faction>();
        }

        public void AddFaction(string name)
        {
            if (!_factions.ContainsKey(name))
            {
                _factions[name] = new Faction(name);
            }
        }

        public Faction GetFaction(string name)
        {
            return _factions.GetValueOrDefault(name);
        }

        public void UpdateRelations(string faction1Name, string faction2Name, float change)
        {
            var faction1 = GetFaction(faction1Name);
            var faction2 = GetFaction(faction2Name);

            if (faction1 != null && faction2 != null)
            {
                faction1.UpdateRelation(faction2, change);
                faction2.UpdateRelation(faction1, change);
            }
        }

        public float GetRelation(string faction1Name, string faction2Name)
        {
            var faction1 = GetFaction(faction1Name);
            var faction2 = GetFaction(faction2Name);

            if (faction1 != null && faction2 != null)
            {
                return faction1.GetRelation(faction2);
            }

            return 0f;
        }

        public void Update(float deltaTime)
        {
            foreach (var faction in _factions.Values)
            {
                faction.Update(deltaTime);
            }
        }

        public IEnumerable<Faction> GetAllFactions()
        {
            return _factions.Values;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteFont font)
        {
            // Implementation for drawing factions with font parameter
        }
    }
} 