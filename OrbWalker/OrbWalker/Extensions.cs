using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace OrbWalker
{
    static class Extensions
    {
        public static float TimeAttackWillHit(this Obj_AI_Base sender, Obj_AI_Base target)
        {
            if (sender.IsMelee)
                return sender.AttackCastDelay;

            // Distance * 1000 / speed = time
            float distance = sender.Distance(target);
            float time = distance / sender.BasicAttack.MissileSpeed;


            return sender.AttackCastDelay + time;
        }

        public static bool CanKillWithSpell(this Obj_AI_Base target, float damage)
        {
            return !target.CanKillWithAutoAttack() && target.Health <= damage;
        }

        public static bool CanKillWithAutoAttack(this Obj_AI_Base target)
        {
            return Orbwalker.CanAutoAttack && Player.Instance.IsInRange(target, Player.Instance.GetAutoAttackRange(target))
                && target.Health <= Player.Instance.GetAutoAttackDamage(target);
        }

        #region Stats
        public static float AbilityPower(this Obj_AI_Base target)
        {
            return target.FlatMagicDamageMod;
        }

        public static float AttackDamage(this Obj_AI_Base target)
        {
            return target.FlatPhysicalDamageMod;
        }

        public static float MissingHealth(this Obj_AI_Base target)
        {
            return (target.MaxHealth - target.Health);
        }

        public static float PercentAbilityPower(this Obj_AI_Base target, float percent)
        {
            return (percent / 100) * target.FlatMagicDamageMod;
        }

        public static float PercentAttackDamage(this Obj_AI_Base target, float percent)
        {
            return (percent / 100) * target.FlatPhysicalDamageMod;
        }

        public static float PercentBonusAttackDamage(this Obj_AI_Base target, float percent)
        {
            return (percent / 100) * (target.TotalAttackDamage - target.BaseAttackDamage);
        }

        public static float PercentMaximumHealth(this Obj_AI_Base target, float percent)
        {
            return (percent * 100) * target.MaxHealth;
        }

        public static float PercentMissingHealth(this Obj_AI_Base target, float percent)
        {
            return target.MissingHealth() / target.MaxHealth;
        }
        #endregion

        #region CountUnitsInRange
        // Count Units in range
        public static float CountUnitsInRange(this Obj_AI_Base target, List<Obj_AI_Base> enemies, float range)
        {
            return enemies.Where(a => a.IsInRange(target, range)).Count();
        }

        // Count Champions in range
        public static float CountChampionsInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.Heroes.AllHeroes.ToList().ToObj_AI_BaseList(), range);
        }

        // Count Ally Champions in range
        public static float CountAllyChampionsInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.Heroes.Allies.Where(a => !a.IsMe).ToList().ToObj_AI_BaseList(), range);
        }

        // Count Enemy Champions in range
        public static float CountEnemyChampionsInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList(), range);
        }

        // Count Minions in range
        public static float CountMinionsInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.MinionsAndMonsters.Minions.ToList().ToObj_AI_BaseList(), range);
        }

        // Count Minions in range
        public static float CountAllyMinionsInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.MinionsAndMonsters.Minions.Where(a => a.Team == Player.Instance.Team).ToList().ToObj_AI_BaseList(), range);
        }

        // Count Minions in range
        public static float CountEnemyMinionsInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.MinionsAndMonsters.Minions.Where(a => a.Team != Player.Instance.Team).ToList().ToObj_AI_BaseList(), range);
        }

        // Count Monsters in range
        public static float CountMonstersInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.MinionsAndMonsters.Monsters.ToList().ToObj_AI_BaseList(), range);
        }

        // Count Large Monsters in range
        public static float CountLargeMonstersInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.MinionsAndMonsters.Monsters.Where(a =>
                a.BaseSkinName == "SRU_Red" ||
                a.BaseSkinName == "SRU_Blue" ||
                a.BaseSkinName == "SRU_Dragon_Air" ||
                a.BaseSkinName == "SRU_Dragon_Fire" ||
                a.BaseSkinName == "SRU_Dragon_Water" ||
                a.BaseSkinName == "SRU_Dragon_Earth" ||
                a.BaseSkinName == "SRU_Dragon_Elder" ||
                a.BaseSkinName == "SRU_RiftHerald" ||
                a.BaseSkinName == "SRU_Baron" ||
                a.BaseSkinName == "TT_Spiderboss"
            ).ToList().ToObj_AI_BaseList(), range);
        }

        // Count Creatures in range
        public static float CountCreaturesInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.MinionsAndMonsters.EnemyMinions.Concat(EntityManager.MinionsAndMonsters.Monsters)
                .ToList().ToObj_AI_BaseList(), range);
        }

        // Count Turrets in range
        public static float CountTurretsInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.Turrets.AllTurrets.ToObj_AI_BaseList(), range);
        }

        // Count Ally Turrets in range
        public static float CountAllyTurretsInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.Turrets.Allies.ToObj_AI_BaseList(), range);
        }

        // Count Enemy Turrets in range
        public static float CountEnemyTurretsInRange(this Obj_AI_Base target, float range)
        {
            return CountUnitsInRange(target, EntityManager.Turrets.Enemies.ToObj_AI_BaseList(), range);
        }
        #endregion

        #region CountUnitsInRangeWithPred
        // Count Units in range with Prediction
        public static float CountUnitsInRangeWithPred(this Obj_AI_Base target, List<Obj_AI_Base> enemies, float range, int delay = 250)
        {
            return enemies.Where(a => a.PositionInTime(delay).IsInRange(target, range)).Count();
        }

        // Count Champions in range with Prediction
        public static float CountChampionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.Heroes.AllHeroes.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Count Ally Champions in range with Prediction
        public static float CountAllyChampionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.Heroes.Allies.Where(a => !a.IsMe).ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Count Enemy Champions in range with Prediction
        public static float CountEnemyChampionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList(), range, delay);
        }
        
        // Count Minions in range with Prediction
        public static float CountMinionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.Minions.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Count Ally Minions in range with Prediction
        public static float CountAllyMinionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.AlliedMinions.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Count Enemy Minions in range with Prediction
        public static float CountEnemyMinionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Count Monsters in range with Prediction
        public static float CountMonstersInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.Monsters.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Count Large Monsters in range with Prediction
        public static float CountLargeMonstersInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.Monsters.Where(a =>
                a.BaseSkinName == "SRU_Red" ||
                a.BaseSkinName == "SRU_Blue" ||
                a.BaseSkinName == "SRU_Dragon_Air" ||
                a.BaseSkinName == "SRU_Dragon_Fire" ||
                a.BaseSkinName == "SRU_Dragon_Water" ||
                a.BaseSkinName == "SRU_Dragon_Earth" ||
                a.BaseSkinName == "SRU_Dragon_Elder" ||
                a.BaseSkinName == "SRU_RiftHerald" ||
                a.BaseSkinName == "SRU_Baron" ||
                a.BaseSkinName == "TT_Spiderboss"
            ).ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Count Creatures in range with Prediction
        public static float CountCreaturesInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.EnemyMinions.Concat(EntityManager.MinionsAndMonsters.Monsters)
                .ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Count Turrets in range with Prediction
        public static float CountTurretsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.Turrets.AllTurrets.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Count Ally Turrets in range with Prediction
        public static float CountAllyTurretsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.Turrets.Allies.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Count Enemy Turrets in range with Prediction
        public static float CountEnemyTurretsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.CountUnitsInRangeWithPred(EntityManager.Turrets.Enemies.ToList().ToObj_AI_BaseList(), range, delay);
        }
        #endregion

        #region GetUnitInRange
        // Get Unit in range
        public static Obj_AI_Base GetUnitInRange(this Obj_AI_Base target, List<Obj_AI_Base> enemies, float range)
        {
            return enemies.OrderBy(a => a.Health).FirstOrDefault(a => a.IsInRange(target, range) && a.IsLegitimate());
        }

        // Get Champion in range
        public static Obj_AI_Base GetChampionInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.Heroes.AllHeroes.ToObj_AI_BaseList(), range);
        }

        // Get Ally Champion in range
        public static Obj_AI_Base GetAllyChampionInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.Heroes.Allies.Where(a => !a.IsMe).ToList().ToObj_AI_BaseList(), range);
        }

        // Get Enemy Champion in range
        public static Obj_AI_Base GetEnemyChampionInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.Heroes.Enemies.ToObj_AI_BaseList(), range);
        }

        // Get Minion in range
        public static Obj_AI_Base GetMinionInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.MinionsAndMonsters.Combined.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Ally Minion in range
        public static Obj_AI_Base GetAllyMinionInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.MinionsAndMonsters.AlliedMinions.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Enemy Minion in range
        public static Obj_AI_Base GetEnemyMinionInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Monster in range
        public static Obj_AI_Base GetMonsterInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.MinionsAndMonsters.Monsters.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Large Monster in range
        public static Obj_AI_Base GetLargeMonsterInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.MinionsAndMonsters.Monsters.Where(a =>
                a.BaseSkinName == "SRU_Red" ||
                a.BaseSkinName == "SRU_Blue" ||
                a.BaseSkinName == "SRU_Dragon_Air" ||
                a.BaseSkinName == "SRU_Dragon_Fire" ||
                a.BaseSkinName == "SRU_Dragon_Water" ||
                a.BaseSkinName == "SRU_Dragon_Earth" ||
                a.BaseSkinName == "SRU_Dragon_Elder" ||
                a.BaseSkinName == "SRU_RiftHerald" ||
                a.BaseSkinName == "SRU_Baron" ||
                a.BaseSkinName == "TT_Spiderboss"
            ).ToList().ToObj_AI_BaseList(), range);
        }

        // Get Creature in range
        public static Obj_AI_Base GetCreatureInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.MinionsAndMonsters.EnemyMinions.Concat(EntityManager.MinionsAndMonsters.Monsters)
                .ToList().ToObj_AI_BaseList(), range);
        }

        // Get Turret in range
        public static Obj_AI_Base GetTurretInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.Turrets.AllTurrets.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Ally Turret in range
        public static Obj_AI_Base GetAllyTurretInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.Turrets.Allies.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Enemy Turret in range
        public static Obj_AI_Base GetEnemyTurretInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitInRange(EntityManager.Turrets.Enemies.ToList().ToObj_AI_BaseList(), range);
        }
        #endregion

        #region GetUnitInRangeWithPred
        // Get Unit in range with Prediction
        public static Obj_AI_Base GetUnitInRangeWithPred(this Obj_AI_Base target, List<Obj_AI_Base> enemies, float range, int delay = 250)
        {
            return enemies.OrderBy(a => a.Health).FirstOrDefault(a => a.PositionInTime(delay).IsInRange(target, range) && a.IsLegitimate());
        }

        // Get Champion in range with Prediction
        public static Obj_AI_Base GetChampionInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.Heroes.AllHeroes.ToObj_AI_BaseList(), range, delay);
        }

        // Get Ally Champion in range with Prediction
        public static Obj_AI_Base GetAllyChampionInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.Heroes.Allies.Where(a => !a.IsMe).ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Enemy Champion in range with Prediction
        public static Obj_AI_Base GetEnemyChampionInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.Heroes.Enemies.ToObj_AI_BaseList(), range, delay);
        }

        // Get Minion in range with Prediction
        public static Obj_AI_Base GetMinionInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.MinionsAndMonsters.Minions.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Ally Minion in range with Prediction
        public static Obj_AI_Base GetAllyMinionInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.MinionsAndMonsters.AlliedMinions.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Minion in range with Prediction
        public static Obj_AI_Base GetEnemyMinionInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Monster in range with Prediction
        public static Obj_AI_Base GetMonsterInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.MinionsAndMonsters.Monsters.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Large Monster in range with Prediction
        public static Obj_AI_Base GetLargeMonsterInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.MinionsAndMonsters.Monsters.Where(a =>
                a.BaseSkinName == "SRU_Red" ||
                a.BaseSkinName == "SRU_Blue" ||
                a.BaseSkinName == "SRU_Dragon_Air" ||
                a.BaseSkinName == "SRU_Dragon_Fire" ||
                a.BaseSkinName == "SRU_Dragon_Water" ||
                a.BaseSkinName == "SRU_Dragon_Earth" ||
                a.BaseSkinName == "SRU_Dragon_Elder" ||
                a.BaseSkinName == "SRU_RiftHerald" ||
                a.BaseSkinName == "SRU_Baron" ||
                a.BaseSkinName == "TT_Spiderboss"
            ).ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Creature in range with Prediction
        public static Obj_AI_Base GetCreatureInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.MinionsAndMonsters.EnemyMinions.Concat(EntityManager.MinionsAndMonsters.Monsters)
                .ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Turret in range with Prediction
        public static Obj_AI_Base GetTurretInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.Turrets.AllTurrets.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Ally Turret in range with Prediction
        public static Obj_AI_Base GetAllyTurretInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.Turrets.Allies.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Enemy Turret in range with Prediction
        public static Obj_AI_Base GetEnemyTurretInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitInRangeWithPred(EntityManager.Turrets.Enemies.ToList().ToObj_AI_BaseList(), range, delay);
        }
        #endregion

        #region GetUnitsInRange
        // Get Units in range
        public static List<Obj_AI_Base> GetUnitsInRange(this Obj_AI_Base target, List<Obj_AI_Base> enemies, float range)
        {
            return enemies.OrderBy(a => a.Health).Where(a => a.IsInRange(target, range) && a.IsLegitimate()).ToList();
        }

        // Get Champions in range
        public static List<Obj_AI_Base> GetChampionsInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.Heroes.AllHeroes.ToObj_AI_BaseList(), range);
        }

        // Get Ally Champions in range
        public static List<Obj_AI_Base> GetAllyChampionsInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.Heroes.Allies.Where(a => !a.IsMe).ToList().ToObj_AI_BaseList(), range);
        }

        // Get Ally Champions in range
        public static List<Obj_AI_Base> GetEnemyChampionsInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.Heroes.Enemies.ToObj_AI_BaseList(), range);
        }

        // Get Minions in range
        public static List<Obj_AI_Base> GetMinionsInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.MinionsAndMonsters.Minions.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Ally Minions in range
        public static List<Obj_AI_Base> GetAllyMinionsInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.MinionsAndMonsters.AlliedMinions.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Enemy Minions in range
        public static List<Obj_AI_Base> GetEnemyMinionsInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Monsters in range
        public static List<Obj_AI_Base> GetMonstersInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.MinionsAndMonsters.Monsters.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Large Monsters in range
        public static List<Obj_AI_Base> GetLargeMonstersInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.MinionsAndMonsters.Monsters.Where(a =>
                a.BaseSkinName == "SRU_Red" ||
                a.BaseSkinName == "SRU_Blue" ||
                a.BaseSkinName == "SRU_Dragon_Air" ||
                a.BaseSkinName == "SRU_Dragon_Fire" ||
                a.BaseSkinName == "SRU_Dragon_Water" ||
                a.BaseSkinName == "SRU_Dragon_Earth" ||
                a.BaseSkinName == "SRU_Dragon_Elder" ||
                a.BaseSkinName == "SRU_RiftHerald" ||
                a.BaseSkinName == "SRU_Baron" ||
                a.BaseSkinName == "TT_Spiderboss"
            ).ToList().ToObj_AI_BaseList(), range);
        }

        // Get Creatures in range
        public static List<Obj_AI_Base> GetCreaturesInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.MinionsAndMonsters.EnemyMinions.Concat(EntityManager.MinionsAndMonsters.Monsters)
                .ToList().ToObj_AI_BaseList(), range);
        }

        // Get Turrets in range
        public static List<Obj_AI_Base> GetTurretsInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.Turrets.AllTurrets.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Ally Turrets in range
        public static List<Obj_AI_Base> GetAllyTurretsInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.Turrets.Allies.ToList().ToObj_AI_BaseList(), range);
        }

        // Get Enemy Turrets in range
        public static List<Obj_AI_Base> GetEnemyTurretsInRange(this Obj_AI_Base target, float range)
        {
            return target.GetUnitsInRange(EntityManager.Turrets.Enemies.ToList().ToObj_AI_BaseList(), range);
        }
        #endregion

        #region GetUnitsInRangeWithPred
        // Get Units in range with Prediction
        public static List<Obj_AI_Base> GetUnitsInRangeWithPred(this Obj_AI_Base target, List<Obj_AI_Base> enemies, float range, int delay = 250)
        {
            return enemies.OrderBy(a => a.Health).Where(a => a.PositionInTime(delay).IsInRange(target, range) && a.IsLegitimate()).ToList();
        }

        // Get Champions in range with Prediction
        public static List<Obj_AI_Base> GetChampionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.Heroes.AllHeroes.ToObj_AI_BaseList(), range, delay);
        }

        // Get Ally Champions in range with Prediction
        public static List<Obj_AI_Base> GetAllyChampionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.Heroes.Allies.Where(a => !a.IsMe).ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Enemy Champions in range with Prediction
        public static List<Obj_AI_Base> GetEnemyChampionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.Heroes.Enemies.ToObj_AI_BaseList(), range, delay);
        }

        // Get Minions in range with Prediction
        public static List<Obj_AI_Base> GetMinionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.Minions.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Ally Minions in range with Prediction
        public static List<Obj_AI_Base> GetAllyMinionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.AlliedMinions.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Enemy Minions in range with Prediction
        public static List<Obj_AI_Base> GetEnemyMinionsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Monsters in range with Prediction
        public static List<Obj_AI_Base> GetMonstersInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.Monsters.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Large Monsters in range with Prediction
        public static List<Obj_AI_Base> GetLargeMonstersInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.Monsters.Where(a =>
                a.BaseSkinName == "SRU_Red" ||
                a.BaseSkinName == "SRU_Blue" ||
                a.BaseSkinName == "SRU_Dragon_Air" ||
                a.BaseSkinName == "SRU_Dragon_Fire" ||
                a.BaseSkinName == "SRU_Dragon_Water" ||
                a.BaseSkinName == "SRU_Dragon_Earth" ||
                a.BaseSkinName == "SRU_Dragon_Elder" ||
                a.BaseSkinName == "SRU_RiftHerald" ||
                a.BaseSkinName == "SRU_Baron" ||
                a.BaseSkinName == "TT_Spiderboss"
            ).ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Creatures in range with Prediction
        public static List<Obj_AI_Base> GetCreaturesInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.MinionsAndMonsters.EnemyMinions.Concat(EntityManager.MinionsAndMonsters.Monsters)
                .ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Turrets in range with Prediction
        public static List<Obj_AI_Base> GetTurretsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.Turrets.AllTurrets.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Ally Turrets in range with Prediction
        public static List<Obj_AI_Base> GetAllyTurretsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.Turrets.Allies.ToList().ToObj_AI_BaseList(), range, delay);
        }

        // Get Enemy Turrets in range with Prediction
        public static List<Obj_AI_Base> GetEnemyTurretsInRangeWithPred(this Obj_AI_Base target, float range, int delay = 250)
        {
            return target.GetUnitsInRangeWithPred(EntityManager.Turrets.Enemies.ToList().ToObj_AI_BaseList(), range, delay);
        }
        #endregion
        
        public static float GetAARange(this Obj_AI_Base target)
        {
            float range = 0;

            if (target.BaseSkinName.ToLower().Contains("melee"))
            {
                range = 110;
                if (target.HasBuff("exaltedwithbaronnashorminion") || target.HasBuff("tt_spiderbossminionswap"))
                    range += 75;
            }
            else if (target.BaseSkinName.ToLower().Contains("ranged"))
            {
                range = 550;
                if (target.HasBuff("exaltedwithbaronnashorminion") || target.HasBuff("tt_spiderbossminionswap"))
                    range += 100;
            }
            else if (target.BaseSkinName.ToLower().Contains("siege"))
            {
                range = 300;
                if (target.HasBuff("exaltedwithbaronnashorminion"))
                    range += 600;
                if (target.HasBuff("tt_spiderbossminionswap"))
                    range += 750;
            }
            else if (target.BaseSkinName.ToLower().Contains("super"))
                range = 170;
            else if (target.IsMonster)
                range = target.CharData.AttackRange;
            else
                range = target.GetAutoAttackRange();

            return range;
        }

        public static BuffInstance GetBuff(this Obj_AI_Base target, string name)
        {
            return target.Buffs.FirstOrDefault(a => a.Name == name);
        }

        public static BuffInstance GetBuffOfType(this Obj_AI_Base target, BuffType buff)
        {
            return target.Buffs.FirstOrDefault(a => a.Type == buff);
        }

        public static InventorySlot GetItem(this AIHeroClient target, ItemId id)
        {
            return target.InventoryItems.FirstOrDefault(a => a.Id == id);
        }

        public static bool HasItem(this AIHeroClient target, ItemId id)
        {
            return target.InventoryItems.FirstOrDefault(a => a.Id == id) != null;
        }

        public static bool IsPlayer(this Obj_AI_Base target)
        {
            return target.Name == Player.Instance.Name;
        }

        public static bool IsBuffed(this Obj_AI_Base target)
        {
            return target.Buffs.Any(a => a.DisplayName == "Chrono Shift"
                                          && a.DisplayName == "FioraW"
                                          && a.Type == BuffType.SpellShield);
        }

        public static bool IsLegitimate(this Obj_AI_Base target)
        {
            return (!target.IsNull() && target.IsValid && !target.IsDead
                && target.IsTargetable && target.IsVisible && target.IsHPBarRendered);
        }

        public static bool IsLegitimateWithBuffs(this Obj_AI_Base target)
        {
            return (target.IsLegitimate()
                && !target.IsBuffed() && !target.IsInvulnerable);
        }

        public static bool IsNull(this Obj_AI_Base target)
        {
            return target == null;
        }

        public static Vector3 PositionInTime(this Obj_AI_Base target, int time)
        {
            Vector3 pos = Prediction.Position.PredictUnitPosition(target, time).To3D(0);

            return new Vector3(pos.X, pos.Y, NavMesh.GetHeightForPosition(pos.X, pos.Y));
        }

        #region Convert Object to Obj_AI_Base
        // Convert AIHeroClient to Obj_AI_Base
        public static Obj_AI_Base ToObj_AI_Base(this AIHeroClient target)
        {
            return target;
        }

        // Convert Obj_AI_Minion to Obj_AI_Base
        public static Obj_AI_Base ToObj_AI_Base(this Obj_AI_Minion target)
        {
            return target;
        }

        // Convert Obj_AI_Turret to Obj_AI_Base
        public static Obj_AI_Base ToObj_AI_Base(this Obj_AI_Turret target)
        {
            return target;
        }
        #endregion

        #region Convert Object to List<Obj_AI_Base>
        public static List<Obj_AI_Base> ToObj_AI_BaseList(this Obj_AI_Base target)
        {
            List<Obj_AI_Base> list = new List<Obj_AI_Base>();

            list.Add(target);

            return list;
        }
        #endregion

        #region Convert List to List<Obj_AI_Base>
        // Convert List<AIHeroClient> to List<Obj_AI_Base>
        public static List<Obj_AI_Base> ToObj_AI_BaseList(this List<AIHeroClient> list)
        {
            return list.Cast<Obj_AI_Base>().ToList();
        }

        // Convert List<Obj_AI_Minion> to List<Obj_AI_Base>
        public static List<Obj_AI_Base> ToObj_AI_BaseList(this List<Obj_AI_Minion> list)
        {
            return list.Cast<Obj_AI_Base>().ToList();
        }

        // Convert List<Obj_AI_Turret> to List<Obj_AI_Base>
        public static List<Obj_AI_Base> ToObj_AI_BaseList(this List<Obj_AI_Turret> list)
        {
            return list.Cast<Obj_AI_Base>().ToList();
        }
        #endregion

        #region Convert List to Obj_AI_Base[]
        // Convert List<Obj_AI_Base> to Obj_AI_Base[]
        public static Obj_AI_Base[] ToObj_AI_BaseArray(this List<Obj_AI_Base> list)
        {
            return list.ToArray();
        }

        #endregion

        public static float VisionRange(this Obj_AI_Base target)
        {
            return 1200;
        }

        public static Vector3 BestLinearSkillshotCastPosition(this Spell.Skillshot spell, List<Obj_AI_Base> targets, out int hitnum, bool isCollision = false)
        {
            hitnum = 0;
            Vector2 bestPos = Vector2.Zero;

            List<Vector2> possiblePos = new List<Vector2>();

            if (targets.Count == 1)
            {
                hitnum = 1;

                return targets.FirstOrDefault().PositionInTime(spell.CastDelay);
            }

            for (int o = 0; o < spell.Range; o += spell.Width / 2)
            {
                Vector2 extempPos = Player.Instance.Position.Extend(Player.Instance.Position.To2D() + new Vector2(0, spell.Range), o);

                for (int i = 0; i < 360; i++)
                {
                    Vector2 tempPos = extempPos.RotateAroundPoint(Player.Instance.Position.To2D(), MathUtil.DegreesToRadians(i));

                    possiblePos.Add(tempPos);
                }
            }

            foreach (Vector2 pos in possiblePos)
            {
                int tempHitNum = targets.Count(a => a.PositionInTime(spell.CastDelay).IsInRange(pos, (spell.Width / 2)));

                if (tempHitNum > hitnum)
                {
                    hitnum = tempHitNum;
                    bestPos = pos;
                }
            }

            if (bestPos.IsInRange(Player.Instance, spell.Range))
                return bestPos.To3D();

            return Player.Instance.Position.Extend(bestPos, spell.Range).To3D();
        }

        public static Vector3 BestCircularSkillshotCastPosition(this Spell.Skillshot spell, List<Obj_AI_Base> targets, out int hitnum)
        {
            hitnum = 0;
            Vector2 bestPos = Vector2.Zero;

            List<Vector2> possiblePos = new List<Vector2>();

            if (targets.Count == 1)
            {
                hitnum = 1;

                return targets.FirstOrDefault().PositionInTime(spell.CastDelay);
            }

            for (int o = 0; o < spell.Range; o += spell.Width / 2)
            {
                Vector2 extempPos = Player.Instance.Position.Extend(Player.Instance.Position.To2D() + new Vector2(0, spell.Range), o);

                for (int i = 0; i < 360; i++)
                {
                    Vector2 tempPos = extempPos.RotateAroundPoint(Player.Instance.Position.To2D(), MathUtil.DegreesToRadians(i));

                    possiblePos.Add(tempPos);
                }
            }

            foreach (Vector2 pos in possiblePos)
            {
                int tempHitNum = targets.Count(a => a.PositionInTime(spell.CastDelay).IsInRange(pos, (spell.Width / 2)));

                if (tempHitNum > hitnum)
                {
                    hitnum = tempHitNum;
                    bestPos = pos;
                }
            }

            if (bestPos.IsInRange(Player.Instance, spell.Range))
                return bestPos.To3D();

            return Player.Instance.Position.Extend(bestPos, spell.Range).To3D();
        }
    }
}