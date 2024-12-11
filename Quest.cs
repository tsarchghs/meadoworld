using System;
using System.Collections.Generic;

namespace MeadoworldMono
{
    public class Quest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public Action<Player> RewardAction { get; set; }
        public Location TargetLocation { get; set; }
        public int ReputationChange { get; set; }

        public Quest(string name, string description, Action<Player> rewardAction, Location targetLocation, int reputationChange)
        {
            Name = name;
            Description = description;
            RewardAction = rewardAction;
            TargetLocation = targetLocation;
            ReputationChange = reputationChange;
            IsCompleted = false;
        }

        public void Complete(Player player, Dictionary<Location, int> reputation)
        {
            if (!IsCompleted)
            {
                RewardAction(player);
                reputation[TargetLocation] += ReputationChange;
                IsCompleted = true;
            }
        }
    }
} 