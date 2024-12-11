using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MeadoworldMono
{
    public class BattleUnit
    {
        public Troop Troop { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 TargetPosition { get; set; }
        public bool IsPlayerUnit { get; set; }
        public float Health { get; set; }
        public bool IsDead { get; set; }
        public float AttackTimer { get; set; }
        public float FormationCohesion { get; set; } = 0.8f;

        public BattleUnit(Troop troop, Vector2 position, bool isPlayerUnit)
        {
            Troop = troop;
            Position = position;
            TargetPosition = position;
            IsPlayerUnit = isPlayerUnit;
            Health = troop.MaxHealth;
            IsDead = false;
            AttackTimer = 0;
        }

        public void UpdatePosition(float deltaTime, Vector2 enemyPosition)
        {
            Vector2 formationForce = (TargetPosition - Position) * FormationCohesion;
            Vector2 combatForce = Vector2.Zero;

            float distanceToEnemy = Vector2.Distance(Position, enemyPosition);
            if (distanceToEnemy < Troop.AttackRange * 2)
            {
                Vector2 toEnemy = enemyPosition - Position;
                toEnemy.Normalize();
                combatForce = toEnemy * (1 - FormationCohesion);
            }

            Vector2 totalForce = formationForce + combatForce;
            if (totalForce != Vector2.Zero)
            {
                totalForce.Normalize();
                Position += totalForce * Troop.Speed * deltaTime;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Color color = IsPlayerUnit ? Color.Blue : Color.Red;
            if (IsDead)
                color = Color.Gray;

            spriteBatch.Draw(
                GetUnitTexture(spriteBatch.GraphicsDevice),
                new Rectangle((int)Position.X - 5, (int)Position.Y - 5, 10, 10),
                color
            );
        }

        private Texture2D GetUnitTexture(GraphicsDevice graphics)
        {
            Texture2D texture = new Texture2D(graphics, 1, 1);
            texture.SetData(new[] { Color.White });
            return texture;
        }
    }
} 