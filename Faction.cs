using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MeadoworldMono
{
    public class Faction
    {
        public string Name { get; set; }
        public List<Location> ControlledLocations { get; private set; }
        public float Gold { get; private set; }
        public Dictionary<Faction, float> Relations { get; private set; }
        public Color BannerColor { get; set; }
        public List<Troop> AvailableTroops { get; private set; }

        public Faction(string name)
        {
            
            Name = name;
            ControlledLocations = new List<Location>();
            Gold = 1000f;
            Relations = new Dictionary<Faction, float>();
            AvailableTroops = new List<Troop>();
        }

        public void AddLocation(Location location)
        {
            if (!ControlledLocations.Contains(location))
            {
                ControlledLocations.Add(location);
                location.ControllingFaction = this;
            }
        }

        public void RemoveLocation(Location location)
        {
            if (ControlledLocations.Contains(location))
            {
                ControlledLocations.Remove(location);
                if (location.ControllingFaction == this)
                    location.ControllingFaction = null;
            }
        }

        public void UpdateRelation(Faction otherFaction, float change)
        {
            if (!Relations.ContainsKey(otherFaction))
                Relations[otherFaction] = 0f;
            
            Relations[otherFaction] = Math.Clamp(Relations[otherFaction] + change, -100f, 100f);
        }

        public float GetRelation(Faction otherFaction)
        {
            return Relations.GetValueOrDefault(otherFaction, 0f);
        }

        public void AddTroop(Troop troop)
        {
            if (!AvailableTroops.Contains(troop))
                AvailableTroops.Add(troop);
        }

        public void Update(float deltaTime)
        {
            // Update faction economy
            foreach (var location in ControlledLocations)
            {
                Gold += location.Prosperity * 0.1f * deltaTime;
            }

            // Update diplomatic relations
            foreach (var faction in Relations.Keys.ToList())
            {
                if (GetRelation(faction) < 0)
                    UpdateRelation(faction, 0.1f * deltaTime); // Relations naturally improve over time
            }
        }
    }
} 