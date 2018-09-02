using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using SharpDX;

namespace Caitlyn
{
    class ModeHandler
    {
        public static AIHeroClient Caitlyn => Player.Instance;
        public static bool hasDoneActionThisTick = false;

        //E Gapclose
        public static void Combo()
        {
            Menu menu = MenuHandler.Combo;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, menu.GetCheckboxValue("Only W CC'd Targets"));

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);

            if (menu.GetCheckboxValue("R"))
                CastR(enemies, true);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, false);
        }

        public static void Harass()
        {
            Menu menu = MenuHandler.Harass;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, menu.GetCheckboxValue("Only W CC'd Targets"));

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);

            if (menu.GetCheckboxValue("R"))
                CastR(enemies, true);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, false);
        }

        public static void AutoHarass()
        {
            Menu menu = MenuHandler.AutoHarass;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu);

            if (menu.GetCheckboxValue("AA Trapped Targets"))
            {
                List<Obj_AI_Base> en = enemies.Where(a => a.HasBuff("caitlynyordletrapinternal") && a.IsInRange(Caitlyn, Caitlyn.GetAutoAttackRange())).ToList();

                if (en.Count >= 1)
                    Player.IssueOrder(GameObjectOrder.AttackUnit, en.FirstOrDefault());
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

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, true, null);

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

            if (menu.GetCheckboxValue("E opposite to Cursor"))
                CastE(Game.CursorPos.Extend(Player.Instance.Position.To2D(), Game.CursorPos.Distance(Player.Instance) * 2).To3D());
        }

        public static void LaneClear()
        {
            Menu menu = MenuHandler.LaneClear;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu);
        }

        public static void LastHit()
        {
            Menu menu = MenuHandler.LastHit;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, true, menu);
        }

        public static void UseItems(List<Obj_AI_Base> enemies, bool ks)
        {
            Menu menu = MenuHandler.Items;

            #region Item Initialization
            InventorySlot QSS = menu.GetCheckboxValue("QSS") ? Caitlyn.GetItem(ItemId.Quicksilver_Sash) : null,
                MercurialsScimitar = menu.GetCheckboxValue("Merc Scim") ? Caitlyn.GetItem(ItemId.Mercurial_Scimitar) : null,
                BOTRK = menu.GetCheckboxValue("BOTRK") ? Caitlyn.GetItem(ItemId.Blade_of_the_Ruined_King) : null,
                BilgewaterCutlass = menu.GetCheckboxValue("Bilgewater Cutlass") ? Caitlyn.GetItem(ItemId.Bilgewater_Cutlass) : null,
                HextechGunblade = menu.GetCheckboxValue("Hextech Gunblade") ? Caitlyn.GetItem(ItemId.Hextech_Gunblade) : null;
            #endregion

            #region QSS
            if (!hasDoneActionThisTick &&
                QSS.MeetsCriteria() &&
                Caitlyn.CanCancleCC())
                hasDoneActionThisTick = QSS.Cast();
            #endregion

            #region Mercurials Scimitar
            if (!hasDoneActionThisTick &&
                MercurialsScimitar.MeetsCriteria() &&
                Caitlyn.CanCancleCC())
                hasDoneActionThisTick = MercurialsScimitar.Cast();
            #endregion

            //all targeted spells that must be used on champions must be called after this
            enemies = enemies.Where(a => a.Type == GameObjectType.AIHeroClient).ToList();
            var target = enemies.OrderBy(a => a.Health).FirstOrDefault();

            #region Hextech Gunblade
            if (!hasDoneActionThisTick &&
                target != null
                && HextechGunblade.MeetsCriteria()
                && enemies.Where(a => a.IsInRange(Caitlyn, 700)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Caitlyn, a, ItemId.Hextech_Gunblade)).FirstOrDefault() != null))
                hasDoneActionThisTick = HextechGunblade.Cast(target);
            #endregion

            #region BOTRK
            if (!hasDoneActionThisTick &&
                target != null
                && BOTRK.MeetsCriteria()
                && enemies.Where(a => a.IsInRange(Caitlyn, 550)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Caitlyn, a, ItemId.Blade_of_the_Ruined_King)).FirstOrDefault() != null))
                hasDoneActionThisTick = BOTRK.Cast(target);
            #endregion

            #region Bilgewater Cutlass
            if (!hasDoneActionThisTick &&
                target != null
                && BilgewaterCutlass.MeetsCriteria()
                && enemies.Where(a => a.IsInRange(Caitlyn, 550)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Caitlyn, a, ItemId.Bilgewater_Cutlass)).FirstOrDefault() != null))
                hasDoneActionThisTick = BilgewaterCutlass.Cast(target);
            #endregion
        }

        public static void UseIgnite(List<Obj_AI_Base> enemies, bool ks)
        {
            Spell.Targeted ignite = EloBuddy.SDK.Spells.SummonerSpells.Ignite;

            if (ignite.Slot == SpellSlot.Unknown || !ignite.IsReady())
                return;

            Obj_AI_Base unit = enemies.Where(a =>
                a.IsInRange(Caitlyn, ignite.Range)
                && (!ks || Calculations.Ignite(a) >= a.Health)
                && a.MeetsCriteria()).FirstOrDefault();

            if (unit != null)
                hasDoneActionThisTick = ignite.Cast(unit);
        }

        public static void CastQ(List<Obj_AI_Base> enemies, bool ks, Menu menu)
        {
            if (!Program.Q.IsReady() || Orbwalker.IsAutoAttacking || enemies.Count <= 0 || hasDoneActionThisTick || (menu != null && menu.GetSliderValue("Q Mana %") > (int)Caitlyn.ManaPercent))
                return;

            enemies = enemies.Where(a => Prediction.Health.GetPrediction(a, Program.Q.CastDelay) > 0 &&
            Program.Q.GetPrediction(a).HitChance >= EloBuddy.SDK.Enumerations.HitChance.High).ToList();

            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.Q(a) <= 0 && Program.Q.GetPrediction(a).HitChance >= EloBuddy.SDK.Enumerations.HitChance.High).ToList();

            if (enemies.Count() >= 1 && (menu == null || enemies.Count() >= menu.GetSliderValue("Units Hit to use Q")))
            {
                PredictionResult bestPR = null;
                int enemiesHit = 0;
                foreach (Obj_AI_Base obj in enemies)
                {
                    var pr = Prediction.Position.PredictLinearMissile(obj, Program.Q.Range, Program.Q.Radius, Program.Q.CastDelay, Program.Q.Speed, Program.Q.AllowedCollisionCount, Caitlyn.Position);
                    var hitCount = 0;
                    if (pr != null)
                        //adding one so that it includes the unit hit
                        hitCount = pr.CollisionObjects.Count(a => enemies.Contains(a)) + 1;
                    if(MenuHandler.Settings.GetComboBoxText("Q Usage") == "Most Units Hit")
                        if (pr != null && (bestPR == null || enemiesHit == 0 || hitCount > enemiesHit))
                        {
                            bestPR = pr;
                            enemiesHit = hitCount;
                        }

                    if (MenuHandler.Settings.GetComboBoxText("Q Usage") == "Best Hit Chance")
                        if (pr != null && (bestPR == null || enemiesHit == 0 || pr.HitChancePercent > bestPR.HitChancePercent))
                        {
                            bestPR = pr;
                            enemiesHit = hitCount;
                        }
                }

                if (bestPR != null && (menu == null || enemiesHit >= menu.GetSliderValue("Units Hit to use Q")))
                    hasDoneActionThisTick = Program.Q.Cast(bestPR.CastPosition);
            }
        }
        public static void CastW(List<Obj_AI_Base> units, bool onlyCC)
        {
            if (!Program.W.IsReady() || hasDoneActionThisTick)
                return;
            

            List<Obj_AI_Base> ccdunits = units.Where(a => a.Buffs.Where(b => b.EndTime - Game.Time >= (float)Program.W.CastDelay / 1000 && !b.Name.ToLower().Contains("caitlyn") && (b.IsKnockup || b.IsKnockback || b.IsRoot || b.IsStunOrSuppressed || b.IsSlow)).FirstOrDefault() != null).ToList();
            
            if(ccdunits.Count >= 1) 
                hasDoneActionThisTick = Program.W.Cast(ccdunits.FirstOrDefault().Position);

            if (!onlyCC)
            {
                units = units.Where(a => Program.W.GetPrediction(a).HitChance >= EloBuddy.SDK.Enumerations.HitChance.High).ToList();
                if (!hasDoneActionThisTick && units.Count() >= 1)
                    hasDoneActionThisTick = Program.W.Cast(Program.W.GetPrediction(units.FirstOrDefault()).CastPosition);
            }
        }
        public static void CastE(List<Obj_AI_Base> enemies, bool ks, Menu menu)
        {
            if (!Program.E.IsReady() || !enemies.Any(a=>a.IsInAutoAttackRange(Caitlyn)) || hasDoneActionThisTick || (menu != null && menu.GetSliderValue("E Mana %") > (int)Caitlyn.ManaPercent))
                return;

            enemies = enemies.Where(a => Program.E.GetPrediction(a).HitChance >= EloBuddy.SDK.Enumerations.HitChance.High).ToList();

            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.E(a) <= 0 && Program.E.GetPrediction(a).HitChance >= EloBuddy.SDK.Enumerations.HitChance.High).ToList();

            if (enemies.Count() >= 1)
            {
                PredictionResult bestPR = null;
                foreach (Obj_AI_Base obj in enemies)
                {
                    var pr = Prediction.Position.PredictLinearMissile(obj, Program.E.Range, Program.E.Radius, Program.E.CastDelay, Program.E.Speed, Program.E.AllowedCollisionCount, Caitlyn.Position);
                    if (pr != null && (bestPR == null || pr.HitChancePercent > bestPR.HitChancePercent))
                        bestPR = pr;
                }

                if (bestPR != null)
                    hasDoneActionThisTick = Program.E.Cast(bestPR.CastPosition);
            }
        }
        public static void CastE(Vector3 pos)
        {
            if (!Program.E.IsReady() || hasDoneActionThisTick)
                return;

            hasDoneActionThisTick = Program.E.Cast(pos.IsInRange(Caitlyn, Program.E.Range) ? pos : Caitlyn.Position.Extend(pos, Program.E.Range - 1).To3D());
        }
        public static void CastR(List<Obj_AI_Base> enemies, bool ks)
        {
            if (!Program.R.IsReady() || hasDoneActionThisTick || enemies.Count <= 0)
                return;

            enemies = enemies.Where(a => a.CountEnemiesInRange(1250) == 1 && Caitlyn.CountEnemiesInRange(Caitlyn.GetAutoAttackRange()) == 0).ToList();

            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.R(a) <= 0).ToList();

            if(enemies.Count >= 1)
                hasDoneActionThisTick = Program.R.Cast(enemies.FirstOrDefault());
        }
    }
}