using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using SharpDX;

namespace Karthus
{
    class ModeHandler
    {
        public static AIHeroClient Karthus => Player.Instance;
        public static bool hasDoneActionThisTick = false;
        public static bool useEThisTick = false;
        
        public static void Combo()
        {
            Menu menu = MenuHandler.Combo;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu, QTargetStyle.Single);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, menu);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);

            if (menu.GetCheckboxValue("R"))
                CastR(enemies);

            if (menu.GetCheckboxValue("Ignite"))
                UseIgnite(enemies, true);
        }

        public static void Harass()
        {
            Menu menu = MenuHandler.Harass;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu, QTargetStyle.Single);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, menu);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);
        }

        public static void AutoHarass()
        {
            if (Karthus.IsUnderEnemyturret() || Karthus.IsRecalling())
                return;

            Menu menu = MenuHandler.AutoHarass;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu, QTargetStyle.Single);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, menu);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);
        }

        public static void JungleClear()
        {
            Menu menu = MenuHandler.JungleClear;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.Monsters.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu, QTargetStyle.AOE);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);
        }

        public static void Killsteal()
        {
            Menu menu = MenuHandler.Killsteal;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, true, menu, QTargetStyle.Single);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, true, menu);

            if (menu.GetCheckboxValue("R"))
                CastR(enemies);

            if (menu.GetCheckboxValue("Ignite"))
                UseIgnite(enemies, true);
        }
        
        public static void Flee()
        {
            Menu menu = MenuHandler.Flee;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, menu);
        }

        public static void LaneClear()
        {
            Menu menu = MenuHandler.LaneClear;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, menu.GetCheckboxValue("Q only for Last Hit"), menu, QTargetStyle.AOE);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);
        }

        public static void LastHit()
        {
            Menu menu = MenuHandler.LastHit;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, true, menu, QTargetStyle.AOE, true);
        }

        public static void Dead()
        {
            Menu menu = MenuHandler.Dead;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("R"))
                CastR(enemies, true);

            if (menu.GetCheckboxValue("Attack minions if no enemies")
                && enemies.Where(a => a.IsInRange(Karthus, Program.Q.Range)).Count() == 0)
                enemies = EntityManager.MinionsAndMonsters.CombinedAttackable.ToList().ToObj_AI_BaseList();

            if (!menu.GetCheckboxValue("Attack when Dead"))
                return;

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu, QTargetStyle.AOE);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, menu);

        }
        public static void EDisabler()
        {
            if (!useEThisTick && Program.E.IsReady() && Program.E.IsActive() && !MenuHandler.Settings.GetCheckboxValue("Perma E"))
                hasDoneActionThisTick = Program.E.Cast(Karthus);
        }

        public static void StackTear()
        {
            if (!useEThisTick && Karthus.IsInShopRange() && Karthus.HasItem(ItemId.Tear_of_the_Goddess, ItemId.Manamune, ItemId.Archangels_Staff))
            {
                if(!Program.E.IsActive() && Program.E.IsReady())
                    hasDoneActionThisTick = Program.E.Cast(Karthus);
                useEThisTick = true;
            }
        }

        public enum QTargetStyle
        {
            Single,
            AOE
        };

        public static void CastQ(List<Obj_AI_Base> enemies, bool ks, Menu menu, QTargetStyle style, bool careAboutLastHit = false)
        {
            if (hasDoneActionThisTick || !Program.Q.IsReady() || menu == null || (MenuHandler.GetSlider(menu, ("Q Mana %")) != null && menu.GetSliderValue("Q Mana %") > (int)Karthus.ManaPercent))
                return;
            
            enemies = enemies.Where(a => a.IsInRange(Karthus, Program.Q.Range + Program.Q.Radius)).ToList();

            if (enemies.Count > 0)
            {
                var enemiesToHit = (menu == null && MenuHandler.GetSlider(menu, ("Minions to hit with Q")) != null) ? menu.GetSliderValue("Minions to hit with Q") : 1;
                var allUnits = ObjectManager.Get<Obj_AI_Base>().Where(a => a.MeetsCriteria() && a.IsInRange(Karthus, 1300) && (a.Type == GameObjectType.AIHeroClient || a.Type == GameObjectType.NeutralMinionCamp || a.Type == GameObjectType.obj_AI_Minion));

                int enemiesHit = 0;
                Vector3 bestPos = Vector3.Zero;
                foreach(var enemy in enemies.OrderBy(a=>a.Health))
                {
                    if (style == QTargetStyle.Single && enemiesHit != 0)
                        break;

                    //if an auto attack is flying to kill this thing, don't attack
                    if (careAboutLastHit
                        && Orbwalker.IsAutoAttacking
                        && Orbwalker.LastTarget == enemy)
                        //&& ObjectManager.Get<MissileClient>().Where(a => a.SpellCaster == Karthus && a.Target == enemy).FirstOrDefault() != null)
                        continue;
                        

                    var prediction = Program.Q.GetPrediction(enemy);
                    var nearbyEnemies = enemies.Where(a => a.IsInRange(prediction.CastPosition, Program.Q.Radius));
                    var nearbyEnemiesCount = nearbyEnemies.Count();
                    bool isolated = !allUnits.Where(a => a != enemy).Any(a => a.MeetsCriteria() && a.IsEnemy && a.IsInRange(prediction.CastPosition, Program.Q.Radius));

                    if (prediction.HitChance < Program.Q.MinimumHitChance)
                        continue;

                    //the sign is greater than because they don't die
                    if (ks && enemy.Health - Calculations.Q(enemy, isolated) > 0)
                        continue;

                    if(style == QTargetStyle.AOE)
                    {
                        var tempList = nearbyEnemies;
                        if (ks)
                            tempList = nearbyEnemies.Where(a => a.Health - Calculations.Q(a, isolated) <= 0);
                        
                        if (tempList.Count() > enemiesHit && tempList.Count() >= enemiesToHit)
                        {
                            enemiesHit = tempList.Count();
                            bestPos = prediction.CastPosition;
                        }
                    }
                    else if(style == QTargetStyle.Single)
                    {
                        if (nearbyEnemiesCount == 1)
                        {
                            enemiesHit = nearbyEnemiesCount;
                            bestPos = prediction.CastPosition;
                        }
                    }
                }
                hasDoneActionThisTick = Program.Q.Cast(bestPos);
            }
        }

        public static void CastW(List<Obj_AI_Base> enemies, Menu menu)
        {
            if (hasDoneActionThisTick || !Program.W.IsReady() || menu == null || (MenuHandler.GetSlider(menu, ("W Mana %")) != null && menu.GetSliderValue("W Mana %") > (int)Karthus.ManaPercent))
                return;

            enemies = enemies.Where(a => a.IsInRange(Karthus, Calculations.WallOfPainMaxRange)).ToList();

            if (enemies.Count > 0)
            {
                foreach (var enemy in enemies)
                {
                    if (hasDoneActionThisTick)
                        break;

                    // Get predicted position after the delay
                    var targetPosition = enemy.Position(Program.W.CastDelay);

                    if (targetPosition.IsInRange(Karthus, Program.W.Range))
                        hasDoneActionThisTick = Program.W.Cast(targetPosition);

                    // Check if target is in range
                    if (targetPosition.IsInRange(Karthus, Calculations.WallOfPainMaxRange))
                    {
                        // Extended range
                        var x = Program.W.Range;
                        var y = (float)Math.Sqrt(Player.Instance.Distance(targetPosition, true) - Program.W.RangeSquared);
                        var z = Player.Instance.Distance(targetPosition);
                        var angle = (float)Math.Acos((y.Pow() + z.Pow() - x.Pow()) / (2 * y * z));
                        var direction = (Player.Instance.ServerPosition.To2D() - targetPosition.To2D()).Normalized().Rotated(-angle);
                        var castPosition = (targetPosition.To2D() + y * direction).To3DWorld();

                        // Final check if the cast position is in range (should always be true)
                        if (Program.W.IsInRange(castPosition) && Program.W.Cast(castPosition))
                            hasDoneActionThisTick = Program.W.Cast(castPosition);
                    }
                }
            }
        }
        
        public static void CastE(List<Obj_AI_Base> enemies, bool ks, Menu menu)
        {
            if (useEThisTick || menu == null || (MenuHandler.GetSlider(menu, ("E Mana %")) != null && menu.GetSliderValue("E Mana %") > (int)Karthus.ManaPercent))
                return;

            enemies = enemies.Where(a => a.IsInRange(Karthus, Program.E.Range)).ToList();

            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.E(a) <= 0).ToList();

            if (enemies.Count > 0)
            {
                var enemiesToHit = (menu == null && MenuHandler.GetSlider(menu, ("Enemies to E")) != null) ? menu.GetSliderValue("Enemies to E") : 1;
                
                if (enemiesToHit <= enemies.Count)
                {
                    //is ready check down here so the disabler can still come through
                    if(!Program.E.IsActive() && Program.E.IsReady() && !hasDoneActionThisTick)
                    {
                        hasDoneActionThisTick = Program.E.Cast(Karthus);
                    }
                    if(!useEThisTick)
                        useEThisTick = true;
                }
            }
        }

        public static void CastR(List<Obj_AI_Base> enemies, bool isDead = false)
        {
            if (hasDoneActionThisTick || !Program.R.IsReady())
                return;

            Menu menu = MenuHandler.RLogic;
               
            //if (ks)
            enemies = enemies.Where(a => a.Health - Calculations.R(a) <= 0).ToList();

            if (enemies.Count() > 0)
            {
                if (isDead || Karthus.CountEnemyChampionsInRange(1200) == 0)
                {
                    List<bool> checks = new List<bool>();

                    //only dead
                    bool deadCheck = (!menu.GetCheckboxValue("Ult only when Dead") || isDead) ? true : false;

                    checks.Add(enemies.Count >= menu.GetSliderValue("X Enemies Killed to R") ? true : false);
                    checks.Add((EntityManager.Heroes.Enemies.Where(a => a.MeetsCriteria() && a.IsInRange(Karthus, 1300)).Count() != 0 && enemies.Count / EntityManager.Heroes.Enemies.Where(a=>a.MeetsCriteria() && a.IsInRange(Karthus, 1300)).Count() >= menu.GetSliderValue("% Enemies Killed to R")) ? true : false);

                    if(deadCheck && checks.Any(a=>a== true))
                        hasDoneActionThisTick = Program.R.Cast();
                }
            }
        }

        public static void UseIgnite(List<Obj_AI_Base> enemies, bool ks)
        {
            Spell.Targeted ignite = EloBuddy.SDK.Spells.SummonerSpells.Ignite;

            if (ignite.Slot == SpellSlot.Unknown || !ignite.IsReady())
                return;

            Obj_AI_Base unit = enemies.Where(a =>
                a.IsInRange(Karthus, ignite.Range)
                && (!ks || Calculations.Ignite(a) >= a.Health)
                && a.MeetsCriteria()).FirstOrDefault();

            if (unit != null)
                hasDoneActionThisTick = ignite.Cast(unit);
        }
    }
}