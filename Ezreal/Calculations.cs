using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Collections.Generic;

namespace Ezreal
{
    class Calculations
    {
        public static AIHeroClient Ezreal => Player.Instance;

        public static float Q(Obj_AI_Base target)
        {
            float damage = 15 + (20 * Program.Q.Level) + Ezreal.TotalAttackDamage * 1.1f + Ezreal.TotalMagicalDamage * 0.4f;

            bool hasSheenBuff = Ezreal.HasBuff("sheen");
            bool canUseSheenItem = Ezreal.InventoryItems.Any(a => a.CanUseItem() && (a.Id == ItemId.Trinity_Force || a.Id == ItemId.Lich_Bane || a.Id == ItemId.Iceborn_Gauntlet || a.Id == ItemId.Sheen));

            if (canUseSheenItem || hasSheenBuff)
            {
                if (Ezreal.HasItem(ItemId.Lich_Bane))
                    damage += (0.75f * Ezreal.BaseAttackDamage) + (0.5f * Ezreal.FlatMagicDamageMod);
                else if (Ezreal.HasItem(ItemId.Trinity_Force))
                    damage += (2f * Ezreal.BaseAttackDamage);
                else if (Ezreal.HasItem(ItemId.Iceborn_Gauntlet))
                    damage += Ezreal.BaseAttackDamage;
                else if (Ezreal.HasItem(ItemId.Sheen))
                    damage += Ezreal.BaseAttackDamage;
            }

            return Ezreal.CalculateDamageOnUnit(target, DamageType.Physical, damage);
        }
        public static float W(Obj_AI_Base target)
        {
            float damage = 25 + (45 * Program.W.Level) + Ezreal.TotalMagicalDamage * 0.8f;

            return Ezreal.CalculateDamageOnUnit(target, DamageType.Magical, damage);
        }
        public static float E(Obj_AI_Base target)
        {
            float damage = 25 + (50 * Program.E.Level) + Ezreal.BonusAttackDamage() * 0.5f + Ezreal.TotalMagicalDamage * 0.75f;

            return Ezreal.CalculateDamageOnUnit(target, DamageType.Magical, damage);
        }
        //include damage reduction as units hit4
        //Each enemy hit reduces the projectile's damage by 10%, down to a minimum 30% damage.
        public static float R(Obj_AI_Base target)
        {
            float damage = 100 + (150 * Program.R.Level) + Ezreal.BonusAttackDamage() + Ezreal.TotalMagicalDamage * 0.9f;

            return Ezreal.CalculateDamageOnUnit(target, DamageType.Magical, damage);
        }
        public static float Ignite(Obj_AI_Base target)
        {
            return ((10 + (4 * Ezreal.Level)) * 5) - ((target.HPRegenRate / 2) * 5);
        }
    }
}