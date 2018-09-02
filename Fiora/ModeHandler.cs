using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using SharpDX;

namespace Fiora
{
    class ModeHandler
    {
        public static AIHeroClient Fiora => Player.Instance;
        public static bool hasDoneActionThisTick = false;
        
        public static void Combo()
        {
            Menu menu = MenuHandler.Combo;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, false, menu);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);

            if (menu.GetCheckboxValue("R"))
                CastR(enemies, false);

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
                CastW(enemies, false, menu);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, false);
        }

        public static void AutoHarass()
        {
            if (Fiora.IsUnderEnemyturret() || Fiora.IsRecalling())
                return;

            Menu menu = MenuHandler.AutoHarass;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, false);
        }

        public static void JungleClear()
        {
            Menu menu = MenuHandler.JungleClear;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.Monsters.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, false);
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
                CastE(enemies, true, null);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, true);

            if (menu.GetCheckboxValue("Ignite"))
                UseIgnite(enemies, true);
        }
        
        public static void Flee()
        {
            Menu menu = MenuHandler.Flee;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();
            
            if(menu.GetCheckboxValue("Q to Cursor") && Program.Q.IsReady() && !hasDoneActionThisTick)
                hasDoneActionThisTick=  Program.Q.Cast(Game.CursorPos.IsInRange(Fiora, Program.Q.Range) ? Game.CursorPos : Fiora.Position.Extend(Game.CursorPos, Program.Q.Range).To3D());
        }

        public static void LaneClear()
        {
            Menu menu = MenuHandler.LaneClear;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q") && (!menu.GetCheckboxValue("Q with Tiamat") || Fiora.HasItem(ItemId.Tiamat, ItemId.Ravenous_Hydra, ItemId.Ravenous_Hydra_Melee_Only)))
                CastQ(enemies, menu.GetCheckboxValue("Q only for Last Hit"), menu);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, menu);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, false);
        }

        public static void LastHit()
        {
            Menu menu = MenuHandler.LastHit;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, true, menu);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, true, menu);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, true);
        }

        public static float lastQPositionCount = 0;
        public static void CastQ(List<Obj_AI_Base> enemies, bool ks, Menu menu)
        {
            //focuses: vitals/ksable/nearest champion/nearest minion
            if (hasDoneActionThisTick || !Program.Q.IsReady()  || (menu != null && menu.GetSliderValue("Q Mana %") > (int)Fiora.ManaPercent))
                return;

            //400 is Q stab range radius
            enemies = enemies.Where(a => a.IsInRange(Fiora, Program.Q.Range + 400 + a.BoundingRadius)).ToList();

            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.Q(a) <= 0 &&  Prediction.Health.GetPrediction(a, Program.Q.CastDelay) > 0).ToList();

            if (enemies.Any(a => Fiora.IsInRange(a, Program.Q.Range * 2 + a.BoundingRadius)))
            {
                if (enemies.Count > 0 && Game.Time - lastQPositionCount > 0.3f)
                {
                    Vector2 tempExtendPos = (Player.Instance.Position + new Vector3(0, 100, 0)).To2D();
                    List<Vector3> QPositions = new List<Vector3>();
                    QPositions.Add(Player.Instance.Position);

                    for (int i = 0; i < 24; i++)
                    {
                        for (int divisor = 1; divisor <= 8; divisor++)
                        {
                            Vector3 endPosition = Player.Instance.Position.Extend(tempExtendPos.RotateAroundPoint(Player.Instance.Position.To2D(), (float)((i * 15) * Math.PI / 180)), (400 / 8) * divisor).To3D((int)Player.Instance.Position.Z);

                            if(!endPosition.IsWall() && !endPosition.IsBuilding())
                                QPositions.Add(endPosition);
                        }
                    }
                    lastQPositionCount = Game.Time;

                    //vital check
                    float distance = float.MaxValue;
                    Vector3 bestPos = Vector3.Zero;
                    foreach (AIHeroClient hero in enemies.Where(a => a.Type == GameObjectType.AIHeroClient).Cast<AIHeroClient>())
                    {
                        if (hero == null)
                            continue;

                        var vitals = Calculations.GetVitals(hero).ToList();
                        if (vitals.Count > 0)
                        {
                            if (QPositions.Count() == 0)
                                continue;

                            var goodVitals = vitals.Where(v => QPositions.Any(a=> !v.sector.IsOutside(a.To2D()) && !v.enemyAttachedTo.IsInRange(a, v.enemyAttachedTo.BoundingRadius) && (v.isReady || a.Distance(Fiora.Position) / Fiora.MoveSpeed >= v.timeTilReady + 0.75f)));

                            if (goodVitals.Count() == 0)
                                continue;

                            foreach(Vector3 pos in QPositions)
                            {
                                var vital = goodVitals.Where(v=>!v.sector.IsOutside(pos.To2D()) && !v.enemyAttachedTo.IsInRange(pos, v.enemyAttachedTo.BoundingRadius)).OrderBy(a=>a.enemyAttachedTo.IsInRange(pos, Fiora.GetAutoAttackRange())).ThenBy(v => v.center.DistanceSquared(Player.Instance.Position)).FirstOrDefault();
                                if (vital != null)
                                {
                                    var dist = vital.center.DistanceSquared(Player.Instance.Position);
                                    if (dist < distance)
                                    {
                                        distance = dist;
                                        bestPos = pos;
                                    }
                                }
                            }
                            if(bestPos != null && !bestPos.IsZero && enemies.Any(a=>a.IsInRange(bestPos, Program.Q.Range)))
                            {
                                hasDoneActionThisTick = Program.Q.Cast(Calculations.QPos(Player.Instance.Position, bestPos));
                                break;
                            }
                        }
                    }

                    //ksable enemy
                    if (!hasDoneActionThisTick)
                    {
                        var ksableEnemy = enemies.Where(a => a.Health - Calculations.Q(a) <= 0 && QPositions.Any(p => a.IsInRange(p, Program.Q.Range))).OrderByDescending(a => a.FlatGoldRewardMod).FirstOrDefault();
                        if (ksableEnemy != null)
                        {
                            var possibleQPositions = QPositions.Where(a => ksableEnemy.IsInRange(a, Program.Q.Range)).OrderBy(a => a.DistanceSquared(ksableEnemy.Position));

                            if (possibleQPositions.Count() > 0 && possibleQPositions.FirstOrDefault() != null)
                                hasDoneActionThisTick = Program.Q.Cast(Calculations.QPos(Player.Instance.Position, possibleQPositions.FirstOrDefault()));
                        }
                    }

                    //champions and minions
                    if (!hasDoneActionThisTick)
                    {
                        bestPos = Calculations.PositionClosestToEnemy(enemies, QPositions, MenuHandler.Settings.GetCheckboxValue("Only Q to proc Vitals"));

                        if (bestPos != Vector3.Zero && bestPos != null)
                            hasDoneActionThisTick = Program.Q.Cast(Calculations.QPos(Player.Instance.Position, bestPos));
                    }
                }
            }
        }
        
        public static void CastW(List<Obj_AI_Base> enemies, bool ks, Menu menu)
        {
            if (hasDoneActionThisTick || !Program.W.IsReady() || (menu != null && menu.GetSliderValue("W Mana %") > (int)Fiora.ManaPercent))
                return;

            enemies = enemies.Where(a => a.IsInRange(Fiora, Program.W.Range)).ToList();

            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.W(a) <= 0).ToList();

            if(enemies.Count > 0)
            {
                PredictionResult bestPrediction = null;
                foreach(Obj_AI_Base ob in enemies)
                {
                    var pred = Program.W.GetPrediction(ob);
                    if (bestPrediction == null || pred.HitChance > bestPrediction.HitChance)
                        bestPrediction = pred;
                }

                if (bestPrediction != null && bestPrediction.HitChance >= EloBuddy.SDK.Enumerations.HitChance.High)
                    hasDoneActionThisTick = Program.W.Cast(bestPrediction.CastPosition);
            }
        }
        
        public static float lastSetAuto = 0;
        public static void CastE(List<Obj_AI_Base> enemies, bool ks, Menu menu)
        {
            if (hasDoneActionThisTick || !Program.E.IsReady() || Game.Time - Program.LastAutoTime > 0.25f || (menu != null && menu.GetSliderValue("E Mana %") > (int)Fiora.ManaPercent))
                return;

            //fiora's aa range becomes 175 after using the spell  
            enemies = enemies.Where(a => a.IsInRange(Fiora, Fiora.BoundingRadius + 175)).ToList();

            if (ks)
                enemies = enemies.Where(a => a.Health - Fiora.GetAutoAttackDamage(a) <= 0).ToList();

            if (enemies.Count > 0)
            {
                hasDoneActionThisTick = Program.E.Cast();
                if (hasDoneActionThisTick && Game.Time - lastSetAuto > 0.5f)
                {
                    Orbwalker.ResetAutoAttack();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, enemies.OrderBy(a => a.Health).FirstOrDefault());
                    lastSetAuto = Game.Time;
                }
            }
        }

        public static void CastR(List<Obj_AI_Base> enemies, bool ks)
        {
            if (hasDoneActionThisTick || !Program.R.IsReady() || !enemies.Any(a => Fiora.IsInAutoAttackRange(a)))
                return;

            enemies = enemies.Where(a => Fiora.IsInAutoAttackRange(a)).ToList();

            if (enemies.Count > 0)
            {
                var rEnemy = enemies.Where(a => Prediction.Health.GetPrediction(a, 250) > 100).OrderBy(a => a.Health).FirstOrDefault();
                if (rEnemy != null)
                    hasDoneActionThisTick = Program.R.Cast(rEnemy);
            }
        }

        public static void UseItems(List<Obj_AI_Base> enemies, bool ks)
        {
            Menu menu = MenuHandler.Items;

            #region Item Initialization
            InventorySlot QSS = (MenuHandler.GetCheckboxValue(MenuHandler.Items, "Use Quick Silver Sash")) ? Fiora.GetItem(ItemId.Quicksilver_Sash) : null,
                MercurialsScimitar = (MenuHandler.GetCheckboxValue(MenuHandler.Items, "Use Mercurials Scimitar")) ? Fiora.GetItem(ItemId.Mercurial_Scimitar) : null,
                RavenousHydra = (MenuHandler.GetCheckboxValue(MenuHandler.Items, "Use Ravenous Hydra")) ? Fiora.GetItem(ItemId.Ravenous_Hydra) : null,
                TitanicHydra = (MenuHandler.GetCheckboxValue(MenuHandler.Items, "Use Titanic Hydra")) ? Fiora.GetItem(ItemId.Titanic_Hydra) : null,
                Tiamat = (MenuHandler.GetCheckboxValue(MenuHandler.Items, "Use Tiamat")) ? Fiora.GetItem(ItemId.Tiamat) : null,
                Youmuus = (MenuHandler.GetCheckboxValue(MenuHandler.Items, "Use Youmuus")) ? Fiora.GetItem(ItemId.Youmuus_Ghostblade) : null,
                BOTRK = (MenuHandler.GetCheckboxValue(MenuHandler.Items, "Use Blade of the Ruined King")) ? Fiora.GetItem(ItemId.Blade_of_the_Ruined_King) : null,
                BilgewaterCutlass = (MenuHandler.GetCheckboxValue(MenuHandler.Items, "Use Bilgewater Cutlass")) ? Fiora.GetItem(ItemId.Bilgewater_Cutlass) : null,
                HextechGunblade = (MenuHandler.GetCheckboxValue(MenuHandler.Items, "Use Hextech Gunblade")) ? Fiora.GetItem(ItemId.Hextech_Gunblade) : null;
            #endregion

            #region QSS
            if (!hasDoneActionThisTick &&
                QSS.MeetsCriteria() &&
                Fiora.CanCancleCC())
                hasDoneActionThisTick = QSS.Cast();
            #endregion

            #region Mercurials Scimitar
            if (!hasDoneActionThisTick &&
                MercurialsScimitar.MeetsCriteria() &&
                Fiora.CanCancleCC())
                hasDoneActionThisTick = MercurialsScimitar.Cast();
            #endregion

            #region Ravenous Hydra
            if (!hasDoneActionThisTick &&
                RavenousHydra.MeetsCriteria()
                && !Orbwalker.IsAutoAttacking
                && enemies.Where(a => a.IsInRange(Fiora, 400)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Fiora, a, ItemId.Ravenous_Hydra)).FirstOrDefault() != null))
                hasDoneActionThisTick = RavenousHydra.Cast();
            #endregion

            #region Titanic Hydra
            if (!hasDoneActionThisTick &&
                TitanicHydra.MeetsCriteria()
                && enemies.Where(a => a.IsInRange(Fiora, Fiora.GetAutoAttackRange())).FirstOrDefault() != null
                && !Orbwalker.IsAutoAttacking
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Fiora, a, ItemId.Titanic_Hydra)).FirstOrDefault() != null))
                hasDoneActionThisTick = TitanicHydra.Cast();
            #endregion

            #region Tiamat
            if (!hasDoneActionThisTick &&
                Tiamat.MeetsCriteria()
                && !Orbwalker.IsAutoAttacking
                && enemies.Where(a => a.IsInRange(Fiora, 400)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Fiora, a, ItemId.Tiamat)).FirstOrDefault() != null))
                hasDoneActionThisTick = Tiamat.Cast();
            #endregion

            #region Youmuus
            if (!hasDoneActionThisTick &&
                Youmuus.MeetsCriteria()
                && Fiora.CountEnemyHeroesInRangeWithPrediction((int)Fiora.GetAutoAttackRange(), 0) >= 1)
                hasDoneActionThisTick = Youmuus.Cast();
            #endregion

            //all targeted spells that must be used on champions must be called after this
            enemies = enemies.Where(a => a.Type == GameObjectType.AIHeroClient).ToList();
            var target = enemies.OrderBy(a => a.Health).FirstOrDefault();

            #region Hextech Gunblade
            if (!hasDoneActionThisTick &&
                target != null
                && HextechGunblade.MeetsCriteria()
                && enemies.Where(a => a.IsInRange(Fiora, 700)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Fiora, a, ItemId.Hextech_Gunblade)).FirstOrDefault() != null))
                hasDoneActionThisTick = HextechGunblade.Cast(target);
            #endregion

            #region BOTRK
            if (!hasDoneActionThisTick &&
                target != null
                && BOTRK.MeetsCriteria()
                && enemies.Where(a => a.IsInRange(Fiora, 550)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Fiora, a, ItemId.Blade_of_the_Ruined_King)).FirstOrDefault() != null))
                hasDoneActionThisTick = BOTRK.Cast(target);
            #endregion

            #region Bilgewater Cutlass
            if (!hasDoneActionThisTick &&
                target != null
                && BilgewaterCutlass.MeetsCriteria()
                && enemies.Where(a => a.IsInRange(Fiora, 550)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Fiora, a, ItemId.Bilgewater_Cutlass)).FirstOrDefault() != null))
                hasDoneActionThisTick = BilgewaterCutlass.Cast(target);
            #endregion
        }

        public static void UseIgnite(List<Obj_AI_Base> enemies, bool ks)
        {
            Spell.Targeted ignite = EloBuddy.SDK.Spells.SummonerSpells.Ignite;

            if (ignite.Slot == SpellSlot.Unknown || !ignite.IsReady())
                return;

            Obj_AI_Base unit = enemies.Where(a =>
                a.IsInRange(Fiora, ignite.Range)
                && (!ks || Calculations.Ignite(a) >= a.Health)
                && a.MeetsCriteria()).FirstOrDefault();

            if (unit != null)
                hasDoneActionThisTick = ignite.Cast(unit);
        }
    }
}