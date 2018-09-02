using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Utils;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Collections.Generic;
using SharpDX;

namespace Fiora
{
    class Calculations
    {
        public static List<Vital> Vitals = new List<Vital>();
        public static AIHeroClient Fiora => Player.Instance;

        public static float Q(Obj_AI_Base target)
        {
            float qdmg = 55 + (10 * Program.Q.Level)
                + ((0.9f + (0.05f * Program.Q.Level)) * Fiora.BonusAttackDamage());

            return Fiora.CalculateDamageOnUnit(target, DamageType.Physical, qdmg);
        }
        public static float W(Obj_AI_Base target)
        {
            float damage = 50 + (40 * Program.W.Level)
                + (Fiora.FlatMagicDamageMod);

            return Fiora.CalculateDamageOnUnit(target, DamageType.Magical, damage);
        }
        public static float Vital(Obj_AI_Base target)
        {
            float damage = 0.02f + (0.045f * (int)(Fiora.BonusAttackDamage() / 100)) * target.MaxHealth;

            return Fiora.CalculateDamageOnUnit(target, DamageType.True, damage);
        }
        public static float Ignite(Obj_AI_Base target)
        {
            return ((10 + (4 * Fiora.Level)) * 5) - ((target.HPRegenRate / 2) * 5);
        }
        public static List<Vital> GetVitals(AIHeroClient enemy)
        {
            return Vitals.Where(a => a.enemyAttachedTo == enemy).ToList();
        }
        public static Vector3 PositionClosestToEnemy(List<Obj_AI_Base> enemy, List<Vector3> positions, bool onlyQForVitals)
        {
            if (onlyQForVitals && enemy.All(a => a.Type == GameObjectType.AIHeroClient))
                return Vector3.Zero;

            float shortestDist = float.MaxValue;
            Vector3 position = Vector3.Zero;
            foreach (Vector3 pos in positions)
            {
                foreach (Obj_AI_Base en in enemy)
                {
                    float dist = pos.DistanceSquared(en.Position);
                    if (dist < shortestDist)
                    {
                        shortestDist = dist;
                        position = pos;
                    }
                }
            }
            return position;
        }
        public static Vector3 QPos(Vector3 source, Vector3 end)
        {
            if (end.DistanceSquared(source) >= 400 * 400)
                return source.Extend(end, 399).To3D((int)source.Z);
            return end;
        }
    }
}