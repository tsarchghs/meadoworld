using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MeadoworldMono
{
    public class Player : Party
    {
        public string Name { get; set; }
        public Location CurrentLocation { get; set; }
        public float Gold { get; set; }
        public new Inventory Inventory { get; private set; }

        public Player() : base()
        {
            Name = string.Empty;
            CurrentLocation = null;
            Gold = 5000f;
            Inventory = new Inventory();
            Inventory.Gold = 5000f;
        }

        public Player(Party party) : base()
        {
            Name = string.Empty;
            CurrentLocation = null;
            Gold = 5000f;
            Inventory = new Inventory();
            Inventory.Gold = 5000f;

            foreach (var troopPair in party.Troops)
            {
                AddTroop(troopPair.Key, troopPair.Value);
            }
        }

        public static Player FromParty(Party party)
        {
            return new Player(party);
        }
    }
} 