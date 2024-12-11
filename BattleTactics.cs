using System;
using Microsoft.Xna.Framework;

namespace MeadoworldMono
{
    public enum TacticType
    {
        Charge,
        HoldPosition,
        Skirmish,
        Flank,
        Retreat
    }

    public class BattleTactics
    {
        public TacticType CurrentTactic { get; private set; }
        public Formation Formation { get; private set; }
        public float MoraleModifier { get; private set; }
        public float SpeedModifier { get; private set; }
        public float DamageModifier { get; private set; }

        public BattleTactics(Formation formation)
        {
            Formation = formation;
            CurrentTactic = TacticType.HoldPosition;
            UpdateModifiers();
        }

        public void SetTactic(TacticType tactic)
        {
            CurrentTactic = tactic;
            UpdateModifiers();
        }

        private void UpdateModifiers()
        {
            switch (CurrentTactic)
            {
                case TacticType.Charge:
                    MoraleModifier = 1.2f;
                    SpeedModifier = 1.5f;
                    DamageModifier = 1.3f;
                    break;
                case TacticType.HoldPosition:
                    MoraleModifier = 1.1f;
                    SpeedModifier = 0.5f;
                    DamageModifier = 1.1f;
                    break;
            case TacticType.Skirmish:
                    MoraleModifier = 0.9f;
                    SpeedModifier = 1.2f;
                    DamageModifier = 0.8f;
                    break;
                case TacticType.Flank:
                    MoraleModifier = 1.1f;
                    SpeedModifier = 1.3f;
                    DamageModifier = 1.2f;
                    break;
                case TacticType.Retreat:
                    MoraleModifier = 0.5f;
                    SpeedModifier = 1.4f;
                    DamageModifier = 0.6f;
                    break;
            }
        }

        public void UpdateUnitBehavior(BattleUnit unit, Vector2 enemyCenter)
        {
            switch (CurrentTactic)
            {
                case TacticType.Charge:
                    unit.FormationCohesion = 0.3f;
                    unit.TargetPosition = enemyCenter;
                    break;
                case TacticType.HoldPosition:
                    unit.FormationCohesion = 0.9f;
                    break;
                case TacticType.Skirmish:
                    UpdateSkirmishBehavior(unit, enemyCenter);
                    break;
                case TacticType.Flank:
                    UpdateFlankBehavior(unit, enemyCenter);
                    break;
                case TacticType.Retreat:
                    unit.FormationCohesion = 0.4f;
                    unit.TargetPosition = GetRetreatPosition(unit);
                    break;
            }
        }

        private void UpdateSkirmishBehavior(BattleUnit unit, Vector2 enemyCenter)
        {
            float distanceToEnemy = Vector2.Distance(unit.Position, enemyCenter);
            if (distanceToEnemy < unit.Troop.AttackRange * 1.5f)
            {
                // Move away from enemy
                Vector2 awayFromEnemy = unit.Position - enemyCenter;
                awayFromEnemy.Normalize();
                unit.TargetPosition = unit.Position + awayFromEnemy * 100f;
            }
            unit.FormationCohesion = 0.6f;
        }

        private void UpdateFlankBehavior(BattleUnit unit, Vector2 enemyCenter)
        {
            // Calculate flanking position
            Vector2 toEnemy = enemyCenter - unit.Position;
            Vector2 perpendicular = new Vector2(-toEnemy.Y, toEnemy.X);
            perpendicular.Normalize();
            
            // Alternate between left and right flank based on unit's position
            if (unit.Position.X > enemyCenter.X)
                perpendicular *= -1;
            
            unit.TargetPosition = enemyCenter + perpendicular * 150f;
            unit.FormationCohesion = 0.7f;
        }

        private Vector2 GetRetreatPosition(BattleUnit unit)
        {
            // Retreat away from enemy center
            Vector2 retreatDir = unit.Position - Formation.Center;
            retreatDir.Normalize();
            return unit.Position + retreatDir * 200f;
        }
    }
} 