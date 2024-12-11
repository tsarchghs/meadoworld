using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MeadoworldMono
{
    public enum FormationType
    {
        Line,
        Square,
        Wedge,
        Circle,
        Scattered
    }

    public class Formation
    {
        public FormationType Type { get; private set; }
        public Vector2 Center { get; private set; }
        public float Spacing { get; private set; }
        public float Rotation { get; set; } = 0f;
        public List<BattleUnit> Units { get; private set; }

        public Formation(FormationType type, Vector2 center, float spacing = 20f)
        {
            Type = type;
            Center = center;
            Spacing = spacing;
            Units = new List<BattleUnit>();
        }

        public void UpdateUnitPositions()
        {
            if (Units.Count == 0) return;

            switch (Type)
            {
                case FormationType.Line:
                    ArrangeInLine();
                    break;
                case FormationType.Square:
                    ArrangeInSquare();
                    break;
                case FormationType.Wedge:
                    ArrangeInWedge();
                    break;
                case FormationType.Circle:
                    ArrangeInCircle();
                    break;
                case FormationType.Scattered:
                    ArrangeScattered();
                    break;
            }
        }

        private void ArrangeInLine()
        {
            int unitCount = Units.Count;
            float totalWidth = (unitCount - 1) * Spacing;
            float startX = Center.X - totalWidth / 2;

            for (int i = 0; i < unitCount; i++)
            {
                float x = startX + i * Spacing;
                Vector2 rotatedPos = RotatePoint(new Vector2(x, Center.Y), Center, Rotation);
                Units[i].TargetPosition = rotatedPos;
            }
        }

        private void ArrangeInSquare()
        {
            int sideLength = (int)Math.Ceiling(Math.Sqrt(Units.Count));
            int currentUnit = 0;

            for (int row = 0; row < sideLength && currentUnit < Units.Count; row++)
            {
                for (int col = 0; col < sideLength && currentUnit < Units.Count; col++)
                {
                    float x = Center.X + (col - sideLength / 2f) * Spacing;
                    float y = Center.Y + (row - sideLength / 2f) * Spacing;
                    Vector2 rotatedPos = RotatePoint(new Vector2(x, y), Center, Rotation);
                    Units[currentUnit].TargetPosition = rotatedPos;
                    currentUnit++;
                }
            }
        }

        private void ArrangeInWedge()
        {
            int rows = (int)Math.Ceiling(Math.Sqrt(Units.Count * 2));
            int currentUnit = 0;

            for (int row = 0; row < rows && currentUnit < Units.Count; row++)
            {
                int unitsInRow = row + 1;
                float rowWidth = (unitsInRow - 1) * Spacing;
                float startX = Center.X - rowWidth / 2;

                for (int col = 0; col < unitsInRow && currentUnit < Units.Count; col++)
                {
                    float x = startX + col * Spacing;
                    float y = Center.Y + row * Spacing;
                    Vector2 rotatedPos = RotatePoint(new Vector2(x, y), Center, Rotation);
                    Units[currentUnit].TargetPosition = rotatedPos;
                    currentUnit++;
                }
            }
        }

        private void ArrangeInCircle()
        {
            int unitCount = Units.Count;
            float radius = unitCount * Spacing / (2 * MathF.PI);
            float angleStep = 2 * MathF.PI / unitCount;

            for (int i = 0; i < unitCount; i++)
            {
                float angle = i * angleStep + Rotation;
                float x = Center.X + radius * MathF.Cos(angle);
                float y = Center.Y + radius * MathF.Sin(angle);
                Units[i].TargetPosition = new Vector2(x, y);
            }
        }

        private void ArrangeScattered()
        {
            Random random = new Random();
            float scatterRadius = Spacing * MathF.Sqrt(Units.Count);

            foreach (var unit in Units)
            {
                float angle = (float)(random.NextDouble() * 2 * Math.PI);
                float distance = (float)(random.NextDouble() * scatterRadius);
                float x = Center.X + distance * MathF.Cos(angle);
                float y = Center.Y + distance * MathF.Sin(angle);
                unit.TargetPosition = new Vector2(x, y);
            }
        }

        private Vector2 RotatePoint(Vector2 point, Vector2 center, float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            Vector2 translated = point - center;
            return new Vector2(
                translated.X * cos - translated.Y * sin,
                translated.X * sin + translated.Y * cos
            ) + center;
        }
    }
} 