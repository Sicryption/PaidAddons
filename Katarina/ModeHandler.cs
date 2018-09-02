using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using SharpDX;

namespace Katarina
{
    class ModeHandler
    {
        public static AIHeroClient Katarina => Player.Instance;
        public static bool hasDoneActionThisTick = false;
        
        public static void Combo()
        {
            Menu menu = MenuHandler.Combo;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();
            
            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false);
            
            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, 1);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, false);
            
            if (menu.GetCheckboxValue("R"))
                CastR(enemies, false);

            if (menu.GetCheckboxValue("E to gapclose"))
                CastEToGapClose(enemies);

            if (menu.GetCheckboxValue("Ignite"))
                UseIgnite(enemies, true);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, true);
        }

        public static void Harass()
        {
            Menu menu = MenuHandler.Harass;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, 1);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, false);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, false);
        }

        public static void AutoHarass()
        {
            if (Katarina.IsUnderEnemyturret() || Katarina.IsRecalling())
                return;

            Menu menu = MenuHandler.AutoHarass;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, 1);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, false);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, false);
        }

        public static void JungleClear()
        {
            Menu menu = MenuHandler.JungleClear;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.Monsters.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, 1);

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, false);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, false, true);
        }

        public static void Killsteal()
        {
            Menu menu = MenuHandler.Killsteal;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, true, 1);
            
            if (menu.GetCheckboxValue("E"))
                CastE(enemies, true);

            if (menu.GetCheckboxValue("R"))
                CastR(enemies, true);

            if (menu.GetCheckboxValue("Ignite"))
                UseIgnite(enemies, true);

            if (menu.GetCheckboxValue("Items"))
                UseItems(enemies, true);
        }
        
        public static void Flee()
        {
            Menu menu = MenuHandler.Flee;
            List<Obj_AI_Base> enemies = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();

            if(menu.GetCheckboxValue("W") 
                && !hasDoneActionThisTick
                && Program.W.IsReady())
                hasDoneActionThisTick = Program.W.Cast();
            
            if (menu.GetCheckboxValue("E")
                && !hasDoneActionThisTick
                && Program.E.IsReady())
            {
                Vector3 castPos = Vector3.Zero;

                if (Program.wallJumpSpots.Any(a => a.endPos.IsInRange(Game.CursorPos, 100)))
                {
                    WallJump jump = Program.wallJumpSpots.OrderBy(a => a.endPos.DistanceSquared(Game.CursorPos)).FirstOrDefault();
                    castPos = jump.startPos.To2D().Extend(jump.endPos, 250).To3D();
                }
                else
                    castPos = Calculations.ClosestJump(Game.CursorPos);

                Chat.Print(castPos.Distance(Game.CursorPos) + "/" + Katarina.Position.Distance(Game.CursorPos));
                if (castPos != Vector3.Zero && castPos.DistanceSquared(Game.CursorPos) < Katarina.Position.DistanceSquared(Game.CursorPos))
                    hasDoneActionThisTick = Program.E.Cast(castPos);
            }
        }

        public static void LaneClear()
        {
            Menu menu = MenuHandler.LaneClear;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, false, menu.GetSliderValue("Minions to hit with Q"));

            if (menu.GetCheckboxValue("W"))
                CastW(enemies, false);

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, false, false, true);
        }

        public static void LastHit()
        {
            Menu menu = MenuHandler.LastHit;
            List<Obj_AI_Base> enemies = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();

            if (menu.GetCheckboxValue("Q"))
                CastQ(enemies, true, menu.GetSliderValue("Minions to hit with Q"));

            if (menu.GetCheckboxValue("E"))
                CastE(enemies, true, false, true);
        }
        
        public static void CastQ(List<Obj_AI_Base> enemies, bool ks, int obsToHit)
        {
            if (hasDoneActionThisTick || !Calculations.ShouldStopUlti(enemies) || !Program.Q.IsReady())
                return;
            
            List<Obj_AI_Base> enemiesInQRange = enemies.Where(a => a.IsInRange(Katarina, Program.Q.Range)).ToList();

            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.Q(a) <= 0).ToList();

            if (enemies.Count > 0)
            {
                int bounceRange = 450;//not sure
                List<Obj_AI_Base> allBounceableTargets = ObjectManager.Get<Obj_AI_Base>().Where(a => a.Type == GameObjectType.NeutralMinionCamp || a.Type == GameObjectType.obj_AI_Minion || a.Type == GameObjectType.AIHeroClient).ToList();

                List<Obj_AI_Base> data = new List<Obj_AI_Base>();
                foreach (Obj_AI_Base ob in enemiesInQRange)
                {
                    List<Obj_AI_Base> bounceTargets = new List<Obj_AI_Base>();
                    var firstBounceTarget = allBounceableTargets.Where(a=>a != ob && a.IsInRange(ob, bounceRange)).OrderBy(a => a.Position.To2D().DistanceSquared(ob.Position)).FirstOrDefault();
                    
                    bounceTargets.Add(ob);
                    if (firstBounceTarget != null)
                    {
                        bounceTargets.Add(firstBounceTarget);
                        var secondBounceTarget = allBounceableTargets.Where(a => a != ob && a != firstBounceTarget && a.IsInRange(firstBounceTarget, bounceRange)).OrderBy(a => a.Position.To2D().DistanceSquared(firstBounceTarget.Position)).FirstOrDefault();

                        if (secondBounceTarget != null)
                            bounceTargets.Add(secondBounceTarget);
                    }

                    var enemiesHitByBounce = enemies.Count(a => bounceTargets.Contains(a));
                    if ((data.Count == 0
                        || enemiesHitByBounce > data.Count(a => bounceTargets.Contains(a)))
                        && enemiesHitByBounce > 0)
                        data = bounceTargets;
                    if (data.Count != 0 && enemiesHitByBounce == data.Count(a => bounceTargets.Contains(a)) && enemies.Contains(ob) && ob.HealthPercent < data.FirstOrDefault().HealthPercent)
                        data = bounceTargets;

                }

                if(data.FirstOrDefault() != null && data.Count(a=>enemies.Contains(a)) >= obsToHit)
                    hasDoneActionThisTick = Program.Q.Cast(data.FirstOrDefault());
            }
        }

        public static void CastW(List<Obj_AI_Base> enemies, bool ks)
        {
            if (hasDoneActionThisTick || !Calculations.ShouldStopUlti(enemies) || !Program.W.IsReady())
                return;

            if(enemies.Where(a => a.IsInRange(Katarina, 200)).Count() > 0)
                hasDoneActionThisTick = Program.W.Cast();

            if (enemies.Where(a => a.IsInRange(Katarina, Program.E.Range + 100)).Count() > 0 
                && enemies.Where(a => a.IsInRange(Katarina, Program.E.Range)).Count() == 0)
                hasDoneActionThisTick = Program.W.Cast();
        }
        
        public static void CastE(List<Obj_AI_Base> enemies, bool ks, bool EUnderTower = true, bool isMinionBasedMode = false)
        {
            if (hasDoneActionThisTick || !Calculations.ShouldStopUlti(enemies) || !Program.E.IsReady())
                return;

            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.E(a) <= 0).ToList();

            if (enemies.Count > 0)
            {
                List<Obj_AI_Base> jumpableObjects = new List<Obj_AI_Base>();

                if (!isMinionBasedMode)
                {
                    if (enemies.Any(a => a.Type == GameObjectType.AIHeroClient))
                        jumpableObjects.AddRange(EntityManager.Heroes.AllHeroes.Where(a => !a.IsMe && a.IsInRange(Katarina, Program.E.Range)).Cast<Obj_AI_Base>());

                    if (enemies.Any(a => a.Type == GameObjectType.obj_AI_Minion || a.Type == GameObjectType.NeutralMinionCamp))
                        jumpableObjects.AddRange(EntityManager.MinionsAndMonsters.CombinedAttackable.Cast<Obj_AI_Base>());
                }

                List<Obj_GeneralParticleEmitter> particles = ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(a => a.Name.Contains("Dagger_Ground")).ToList();
                jumpableObjects.AddRange(ObjectManager.Get<Obj_AI_Base>().Where(a => a.Name == "HiddenMinion" && particles.Any(b => b.IsInRange(a, 5))).ToList());

                int enemiesHit = 0;
                Obj_AI_Base bestJump = null;

                foreach (Obj_AI_Base ob in jumpableObjects.Where(a=>EUnderTower || !EntityManager.Turrets.Enemies.Any(e=> e.IsInAutoAttackRange(a))))
                {
                    if (ob.Name == "HiddenMinion")
                    {
                        Geometry.Polygon.Circle c = new Geometry.Polygon.Circle(ob.Position, 340);
                        var tempListEnemies = enemies.Where(a => !c.IsOutside(a.Position.To2D()));

                        if (tempListEnemies.Count() >= enemiesHit)
                        {
                            bestJump = ob;
                            enemiesHit = tempListEnemies.Count();
                        }

                    }
                    //jump to a regular target only if they are killable within an e and an auto attack. Otherwise wait for dagger
                    //don't E under tower unless the mode supports it
                    else if (bestJump == null && Calculations.E(ob) + Katarina.GetAutoAttackDamage(ob) > ob.Health)
                    {
                        bestJump = ob;
                        enemiesHit = Math.Min(enemies.Count(a=>a.IsInRange(bestJump.Position, 170)), 1);
                    }
                }

                if (bestJump != null && enemiesHit > 0)
                {
                    Vector3 castPosition = bestJump.Position;

                    if(bestJump.Name == "HiddenMinion")
                    {
                        var enemiesInRangeOfDagger = enemies.Where(a => a.IsInRange(bestJump, 170 + 169));

                        List<Vector3> jumpPoses = new List<Vector3>();
                        jumpPoses.Add(bestJump.Position);
                        foreach (var ob in enemiesInRangeOfDagger)
                        {
                            bestJump.Position.Extend(ob, 170);
                            bestJump.Position.Extend(ob, 320);
                            bestJump.Position.Extend(ob, 75);
                        }

                        castPosition = jumpPoses.Where(a=> EUnderTower || !EntityManager.Turrets.Enemies.Any(e => e.IsInRange(a, e.GetAutoAttackRange()))).OrderBy(a => enemiesInRangeOfDagger.Count(b => b.IsInRange(a, 340))).FirstOrDefault();
                    }
                    hasDoneActionThisTick = Program.E.Cast(castPosition);
                }
            }
        }

        public static void CastEToGapClose(List<Obj_AI_Base> enemies)
        {
            Vector3 castpos = Calculations.GapcloseWithE(enemies);

            if (!castpos.IsZero && Calculations.ShouldStopUlti(enemies))
                hasDoneActionThisTick = Program.E.Cast(castpos);
        }

        public static void CastR(List<Obj_AI_Base> enemies, bool ks)
        {
            if (hasDoneActionThisTick || !Program.R.IsReady())
                return;

            enemies = enemies.Where(a => a.IsInRange(Katarina, MenuHandler.RLogic.GetSliderValue("Range to Ulti")) 
                && Prediction.Position.PredictUnitPosition(a, 1000).IsInRange(Katarina, Program.R.Range)).ToList();

            if (ks)
                enemies = enemies.Where(a => a.Health - Calculations.R(a) <= 0).ToList();
            
            if (enemies.Count > 0)
            {
                if(MenuHandler.RLogic.GetSliderValue("X Enemies Hit to R") <= enemies.Count)
                    hasDoneActionThisTick = Program.R.Cast();
                if (MenuHandler.RLogic.GetSliderValue("% enemies in range to R") / 100 <= enemies.Count() / Math.Max(Katarina.CountEnemyChampionsInRange(1300), 1))
                    hasDoneActionThisTick = Program.R.Cast();
            }
        }

        public static void UseIgnite(List<Obj_AI_Base> enemies, bool ks)
        {
            Spell.Targeted ignite = EloBuddy.SDK.Spells.SummonerSpells.Ignite;

            if (ignite.Slot == SpellSlot.Unknown || !ignite.IsReady())
                return;

            Obj_AI_Base unit = enemies.Where(a =>
                a.IsInRange(Katarina, ignite.Range)
                && (!ks || Calculations.Ignite(a) >= a.Health)
                && a.MeetsCriteria()).FirstOrDefault();

            if (unit != null)
                hasDoneActionThisTick = ignite.Cast(unit);
        }
        public static void UseItems(List<Obj_AI_Base> enemies, bool ks)
        {
            #region Item Initialization
            InventorySlot HextechGunblade = Katarina.GetItem(ItemId.Hextech_Gunblade);
            #endregion
            
            //all targeted spells that must be used on champions must be called after this
            enemies = enemies.Where(a => a.Type == GameObjectType.AIHeroClient).ToList();
            var target = enemies.OrderBy(a => a.Health).FirstOrDefault();

            #region Hextech Gunblade
            if (!hasDoneActionThisTick &&
                target != null
                && HextechGunblade.MeetsCriteria()
                && enemies.Where(a => a.IsInRange(Katarina, 700)).FirstOrDefault() != null
                && (!ks || enemies.Where(a => a.MeetsCriteria() && a.Health <= DamageLibrary.GetItemDamage(Katarina, a, ItemId.Hextech_Gunblade)).FirstOrDefault() != null))
                hasDoneActionThisTick = HextechGunblade.Cast(target);
            #endregion
        }
        //e to dagger to gapclose
        //jump to dagger if it puts you in range of enemy champion that is low
        //throw dagger in harass to make it get close to enemy
        
        //x jumped to dagger under tower in lane clear
        //x last hit jumpd to minion not a  dagger
    }
}