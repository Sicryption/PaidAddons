using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using SharpDX;

namespace Ezreal
{
    class ModeHandler
    {
        public static AIHeroClient Ezreal => Player.Instance;
        public static bool hasDoneActionThisTick = false;

        //E Gapclose
        public static void Combo()
        {
            Menu menu = MenuHandler.Combo;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, false, menu);

            if (menu.GetCheckboxValue("W Ally"))
            {
                if (menu.GetCheckboxValue("W Ally under Tower") && Ezreal.IsUnderEnemyturret())
                    CastW(EntityManager.Heroes.Allies.ToObj_AI_BaseList(), false, menu);

                if (menu.GetCheckboxValue("W Ally in Fight") && Ezreal.CountEnemyHeroesInRangeWithPrediction(1200, 250) >= 2 && Ezreal.CountEnemyAlliesInRangeWithPrediction((int)Program.W.Range, 250) >= 1)
                    CastW(EntityManager.Heroes.Allies.ToObj_AI_BaseList(), false, menu);
            }

            if (menu.GetCheckboxValue("R"))
                CastR(enemies, false);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, false);

            if (menu.GetCheckboxValue("Ignite"))
                UseIgnite(enemies, true);
        }

        public static void Harass()
        {
            Menu menu = MenuHandler.Harass;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, false, menu);

            if (menu.GetCheckboxValue("W Ally"))
            {
                if (menu.GetCheckboxValue("W Ally under Tower") && Ezreal.IsUnderEnemyturret())
                    CastW(EntityManager.Heroes.Allies.ToObj_AI_BaseList(), false, menu);

                if (menu.GetCheckboxValue("W Ally in Fight") && Ezreal.CountEnemyHeroesInRangeWithPrediction(1200, 250) >= 2 && Ezreal.CountEnemyAlliesInRangeWithPrediction((int)Program.W.Range, 250) >= 1)
                    CastW(EntityManager.Heroes.Allies.ToObj_AI_BaseList(), false, menu);
            }
            if (menu.GetCheckboxValue("R"))
                CastR(enemies, false);
        }

        public static void AutoHarass()
        {
            if (Ezreal.IsUnderEnemyturret())
                return;

            Menu menu = MenuHandler.AutoHarass;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, false, menu);

            if (menu.GetCheckboxValue("W Ally"))
            {
                if (menu.GetCheckboxValue("W Ally under Tower") && Ezreal.IsUnderEnemyturret())
                    CastW(EntityManager.Heroes.Allies.ToObj_AI_BaseList(), false, menu);

                if (menu.GetCheckboxValue("W Ally in Fight") && Ezreal.CountEnemyHeroesInRangeWithPrediction(1200, 250) >= 2 && Ezreal.CountEnemyAlliesInRangeWithPrediction((int)Program.W.Range, 250) >= 1)
                    CastW(EntityManager.Heroes.Allies.ToObj_AI_BaseList(), false, menu);
            }
        }

        public static void JungleClear()
        {
            Menu menu = MenuHandler.JungleClear;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.Monsters.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu);
        }

        public static void Killsteal()
        {
            Menu menu = MenuHandler.Killsteal;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, true, null);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, true, null);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, true);

            if (menu.GetCheckboxValue("R"))
                CastR(enemies, true);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, true);

            if (menu.GetCheckboxValue("Ignite"))
                UseIgnite(enemies, true);
        }
        
        public static void Flee()
        {
            Menu menu = MenuHandler.Flee;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("E to Cursor"))
                CastE(Game.CursorPos);
        }

        public static void LaneClear()
        {
            Menu menu = MenuHandler.LaneClear;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, menu.GetCheckboxValue("Q only for Last Hit"), menu);
        }

        public static void LastHit()
        {
            Menu menu = MenuHandler.LastHit;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, true, menu);
        }

        public static void StackMode()
        {
            if ((!Program.Q.IsReady() && !Program.W.IsReady()) || hasDoneActionThisTick || Ezreal.IsRecalling() || Ezreal.CountEnemyChampionsInRange(2000) != 0 || Ezreal.CountEnemyMinionsInRange(2000) != 0)
                return;

            if (Ezreal.HasItem(ItemId.Tear_of_the_Goddess, ItemId.Manamune, ItemId.Archangels_Staff))
            {
                if(Program.Q.IsReady() && (Ezreal.IsInShopRange() || Ezreal.ManaPercent >= MenuHandler.Settings.GetSliderValue("Tear Stacking Q Mana %")))
                    hasDoneActionThisTick = Program.Q.Cast(Ezreal.Position + Ezreal.Direction);
                if (!hasDoneActionThisTick && Program.W.IsReady() && (Ezreal.IsInShopRange() || Ezreal.ManaPercent >= MenuHandler.Settings.GetSliderValue("Tear Stacking W Mana %")))
                    hasDoneActionThisTick = Program.W.Cast(Ezreal.Position + Ezreal.Direction);
            }
        }

        public static void UseItems(List<Obj_AI_Base> enemies, bool ks)
        {
            Menu menu = MenuHandler.Items;

            #region Item Initialization
            InventorySlot QSS = menu.GetCheckboxValue("QSS") ? Ezreal.GetItem(ItemId.Quicksilver_Sash) : null,
                MercurialsScimitar = menu.GetCheckboxValue("Merc Scim") ? Ezreal.GetItem(ItemId.Mercurial_Scimitar) : null,
                BOTRK = menu.GetCheckboxValue("BOTRK") ? Ezreal.GetItem(ItemId.Blade_of_the_Ruined_King) : null,
                BilgewaterCutlass = menu.GetCheckboxValue("Bilgewater Cutlass") ? Ezreal.GetItem(ItemId.Bilgewater_Cutlass) : null,
                HextechGunblade = menu.GetCheckboxValue("Hextech Gunblade")? Ezreal.GetItem(ItemId.Hextech_Gunblade) : null;
            #endregion

            #region QSS
            if (!hasDoneActionThisTick &&
                QSS.MeetsCriteria() &&
                Ezreal.CanCancleCC())
                hasDoneActionThisTick = QSS.Cast();
            #endregion

            #region Mercurials Scimitar
            if (!hasDoneActionThisTick &&
                MercurialsScimitar.MeetsCriteria() &&
                Ezreal.CanCancleCC())
                hasDoneActionThisTick = MercurialsScimitar.Cast();
            #endregion
            
            //all targeted spells that must be used on champions must be called after this
            enemies = enemies.Where(a => a.Type == GameObjectType.AIHeroClient).ToList();
            var target = enemies.OrderBy(a => a.Health).FirstOrDefault();

            #region Hextech Gunblade
            if (!hasDoneActionThisTick &&
                target != null
                && HextechGunblade.MeetsCriteria()
                && enemies.Where(a => a.IsInRange(Ezreal, 700)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Ezreal, a, ItemId.Hextech_Gunblade)).FirstOrDefault() != null))
                hasDoneActionThisTick = HextechGunblade.Cast(target);
            #endregion

            #region BOTRK
            if (!hasDoneActionThisTick &&
                target != null
                && BOTRK.MeetsCriteria()
                && enemies.Where(a => a.IsInRange(Ezreal, 550)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Ezreal, a, ItemId.Blade_of_the_Ruined_King)).FirstOrDefault() != null))
                hasDoneActionThisTick = BOTRK.Cast(target);
            #endregion

            #region Bilgewater Cutlass
            if (!hasDoneActionThisTick &&
                target != null
                && BilgewaterCutlass.MeetsCriteria()
                && enemies.Where(a => a.IsInRange(Ezreal, 550)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Ezreal, a, ItemId.Bilgewater_Cutlass)).FirstOrDefault() != null))
                hasDoneActionThisTick = BilgewaterCutlass.Cast(target);
            #endregion
        }

        public static void UseIgnite(List<Obj_AI_Base> enemies, bool ks)
        {
            Spell.Targeted ignite = EloBuddy.SDK.Spells.SummonerSpells.Ignite;

            if (ignite.Slot == SpellSlot.Unknown || !ignite.IsReady())
                return;

            Obj_AI_Base unit = enemies.Where(a =>
                a.IsInRange(Ezreal, ignite.Range)
                && (!ks || Calculations.Ignite(a) >= a.Health)
                && a.MeetsCriteria()).FirstOrDefault();

            if (unit != null)
                hasDoneActionThisTick = ignite.Cast(unit);
        }

        public static void CastQ(List<Obj_AI_Base> enemies, bool ks, Menu menu)
        {
            if (!Program.Q.IsReady() || Orbwalker.IsAutoAttacking || hasDoneActionThisTick || (menu != null && menu.GetSliderValue("Q Mana %") > (int)Ezreal.ManaPercent))
                return;
            
            enemies = enemies.Where(a => Program.Q.GetPrediction(a).HitChance >= EloBuddy.SDK.Enumerations.HitChance.High).ToList();

            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.Q(a) <= 0 && Program.Q.GetPrediction(a).HitChance >= EloBuddy.SDK.Enumerations.HitChance.High).ToList();

            if (enemies.Count() >= 1)
            {
                PredictionResult bestPR = (Orbwalker.LastTarget != null && (Obj_AI_Base)Orbwalker.LastTarget != null && enemies.Contains(Orbwalker.LastTarget))
                    ?Prediction.Position.PredictLinearMissile(
                        (Obj_AI_Base)Orbwalker.LastTarget, Program.Q.Range, Program.Q.Radius, Program.Q.CastDelay, Program.Q.Speed, Program.Q.AllowedCollisionCount, Ezreal.Position)
                        :null;

                if (bestPR != null && bestPR.HitChance > EloBuddy.SDK.Enumerations.HitChance.High)
                {
                    hasDoneActionThisTick = Program.Q.Cast(bestPR.CastPosition);
                    return;
                }

                foreach (Obj_AI_Base obj in enemies.OrderBy(a=>a.Distance(Ezreal)))
                {
                    var pr = Prediction.Position.PredictLinearMissile(obj, Program.Q.Range, Program.Q.Radius, Program.Q.CastDelay, Program.Q.Speed, Program.Q.AllowedCollisionCount, Ezreal.Position);
                    if (bestPR == null || pr.HitChancePercent > bestPR.HitChancePercent)
                        bestPR = pr;
                }

                if (bestPR != null && !bestPR.CastPosition.IsZero)
                    hasDoneActionThisTick = Program.Q.Cast(bestPR.CastPosition);
            }
        }
        public static void CastW(List<Obj_AI_Base> units, bool ks, Menu menu)
        {
            if (!Program.W.IsReady() || hasDoneActionThisTick || (menu != null && menu.GetSliderValue("W Mana %") > (int)Ezreal.ManaPercent))
                return;

            units = units.Where(a => Program.W.GetPrediction(a).HitChance >= EloBuddy.SDK.Enumerations.HitChance.High).ToList();
            
            if (ks)
                units = units.Where(a => a.Health - Calculations.W(a) <= 0).ToList();

            if (units.Count() >= 1)
            {
                PredictionResult bestPR = null;
                foreach (Obj_AI_Base obj in units.OrderBy(a => a.Distance(Ezreal)))
                {
                    var pr = Prediction.Position.PredictLinearMissile(obj, Program.W.Range, Program.W.Radius, Program.W.CastDelay, Program.W.Speed, Program.W.AllowedCollisionCount, Ezreal.Position);
                    if (bestPR == null || pr.HitChancePercent > bestPR.HitChancePercent)
                        bestPR = pr;
                }

                if (bestPR != null)
                    hasDoneActionThisTick = Program.W.Cast(bestPR.CastPosition);
            }
        }
        public static void CastE(List<Obj_AI_Base> enemies, bool ks)
        {
            if (!Program.E.IsReady() || hasDoneActionThisTick)
                return;

            //750 is bolt radius
            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.E(a) <= 0 && a.IsInRange(Ezreal, 750 + Program.E.Range)).ToList();

            if (enemies.Count >= 1)
            {
                Vector3 pos = enemies.OrderBy(a => a.Distance(Ezreal)).FirstOrDefault().Position;

                if (pos != Vector3.Zero)
                    hasDoneActionThisTick = CastE(pos);
            }
        }
        public static bool CastE(Vector3 pos)
        {
            if (!Program.E.IsReady() || hasDoneActionThisTick)
                return false;
            
            hasDoneActionThisTick = Program.E.Cast(pos.IsInRange(Ezreal, Program.E.Range) ? pos : Ezreal.Position.Extend(pos, Program.E.Range - 1).To3D());
            return true;
        }
        public static void CastR(List<Obj_AI_Base> enemies, bool ks)
        {
            if (!Program.R.IsReady() || hasDoneActionThisTick || enemies.Any(a=>a.IsInRange(Ezreal, Ezreal.GetAutoAttackRange())))
                return;

            enemies = enemies.Where(a => Program.R.GetPrediction(a).HitChance >= EloBuddy.SDK.Enumerations.HitChance.Medium).ToList();

            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.R(a) <= 0).ToList();

            if (enemies.Count() >= 1)
            {
                PredictionResult bestPR = null;
                foreach (Obj_AI_Base obj in enemies.OrderBy(a => a.Distance(Ezreal)))
                {
                    var pr = Prediction.Position.PredictLinearMissile(obj, Program.R.Range, Program.R.Radius, Program.R.CastDelay, Program.R.Speed, Program.R.AllowedCollisionCount, Ezreal.Position);
                    if (bestPR == null || pr.HitChancePercent > bestPR.HitChancePercent)
                        bestPR = pr;
                }

                if (bestPR != null && !bestPR.CastPosition.IsZero)
                    hasDoneActionThisTick = Program.R.Cast(bestPR.CastPosition);
            }
        }
    }
}