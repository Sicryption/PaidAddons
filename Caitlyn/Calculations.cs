using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Collections.Generic;

namespace Caitlyn
{
    class Calculations
    {
        public static AIHeroClient Caitlyn => Player.Instance;

        //Q splits dealing less damage. Q does ful damage to units hit by yordle snap trap though
        public static float Q(Obj_AI_Base target)
        {
            float damage = -10 + (40 * Program.Q.Level) + Caitlyn.TotalAttackDamage * (1.2f + (0.1f * Program.Q.Level));
            
            return Caitlyn.CalculateDamageOnUnit(target, DamageType.Physical, damage);
        }
        public static float E(Obj_AI_Base target)
        {
            float damage = 30 + (40 * Program.E.Level) +Caitlyn.TotalMagicalDamage * 0.8f;

            return Caitlyn.CalculateDamageOnUnit(target, DamageType.Magical, damage);
        }

        public static float R(Obj_AI_Base target)
        {
            float damage = 25 + (225 * Program.R.Level) + Caitlyn.BonusAttackDamage() * 2f;

            return Caitlyn.CalculateDamageOnUnit(target, DamageType.Physical, damage);
        }

        public static float Ignite(Obj_AI_Base target)
        {
            return ((10 + (4 * Caitlyn.Level)) * 5) - ((target.HPRegenRate / 2) * 5);
        }
    }
}