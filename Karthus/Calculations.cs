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

namespace Karthus
{
    class Calculations
    {
        public static AIHeroClient Karthus => Player.Instance;

        public static float Q(Obj_AI_Base target, bool isolated)
        {
            float qdmg = 20 + (20 * Program.Q.Level)
                + (0.3f * Karthus.FlatMagicDamageMod);

            if (isolated)
                qdmg *= 2;

            return Karthus.CalculateDamageOnUnit(target, DamageType.Magical, qdmg);
        }
        public static float E(Obj_AI_Base target)
        {
            float damage = 10 + (20 * Program.W.Level)
                + (0.2f * Karthus.FlatMagicDamageMod);

            return Karthus.CalculateDamageOnUnit(target, DamageType.Magical, damage);
        }
        public static float R(Obj_AI_Base target)
        {
            float damage = 100 + (150 * Program.W.Level)
                + (0.6f * Karthus.FlatMagicDamageMod);

            return Karthus.CalculateDamageOnUnit(target, DamageType.Magical, damage);
        }
        public static float Ignite(Obj_AI_Base target)
        {
            return ((10 + (4 * Karthus.Level)) * 5) - ((target.HPRegenRate / 2) * 5);
        }

        public static float SeraphsShield()
        {
            return 0.2f * Karthus.Mana + 150;
        }
        private static int lastWWidthTick = 0;
        public static int WallWidth
        {
            get
            {
                if (lastWWidthTick == Core.GameTickCount)
                    return wallWidth;

                wallWidth = 700 + 100 * Program.W.Level;

                lastWWidthTick = Core.GameTickCount;
                return wallWidth;
            }
        }
        private static int wallWidth;

        public static float WallOfPainMaxRangeSqr
        {
            get { return Program.W.RangeSquared + (WallWidth / 2).Pow(); }
        }

        public static float WallOfPainMaxRange
        {
            get { return (float)Math.Sqrt(WallOfPainMaxRangeSqr); }
        }
    }
}