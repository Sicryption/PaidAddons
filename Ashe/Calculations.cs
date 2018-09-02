using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Collections.Generic;

namespace Ashe
{
    class Calculations
    {
        public static AIHeroClient Ashe => Player.Instance;

        //Q splits dealing less damage. Q does ful damage to units hit by yordle snap trap though
        public static float Q(Obj_AI_Base target)
        {
            float damage = -10 + (40 * Program.Q.Level) + Ashe.TotalAttackDamage * (1.2f + (0.1f * Program.Q.Level));
            
            return Ashe.CalculateDamageOnUnit(target, DamageType.Physical, damage);
        }
        public static float E(Obj_AI_Base target)
        {
            float damage = 30 + (40 * Program.E.Level) +Ashe.TotalMagicalDamage * 0.8f;

            return Ashe.CalculateDamageOnUnit(target, DamageType.Magical, damage);
        }

        public static float R(Obj_AI_Base target)
        {
            float damage = 25 + (225 * Program.R.Level) + Ashe.BonusAttackDamage() * 2f;

            return Ashe.CalculateDamageOnUnit(target, DamageType.Physical, damage);
        }

        public static float Ignite(Obj_AI_Base target)
        {
            return ((10 + (4 * Ashe.Level)) * 5) - ((target.HPRegenRate / 2) * 5);
        }
    }
}