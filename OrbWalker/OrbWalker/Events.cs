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
    class Events
    {
        public static List<AutoAttackInstance> activeautoattacks = new List<AutoAttackInstance>();

        public static void Obj_AI_Base_OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            //Chat.Print("Basic Attack Created");
            activeautoattacks.Add(new AutoAttackInstance(sender, args));
        }

        public static void Obj_AI_Base_OnCreate(GameObject sender, EventArgs args)
        {
            MissileClient missile = sender as MissileClient;

            if (missile != null)
            {
                AutoAttackInstance autoattack = activeautoattacks.Where(a => a._attacker == missile.SpellCaster && a._missile == null)
                    .OrderByDescending(a => a._starttime).FirstOrDefault();

                if (autoattack != null)
                {
                    //Chat.Print("Missile Created");
                    autoattack._damage = autoattack._attacker.GetAutoAttackDamage(autoattack._target);
                    autoattack.AssignMissile(missile);
                }
            }
        }

        public static void Obj_AI_Base_OnDelete(GameObject sender, EventArgs args)
        {
            MissileClient missile = sender as MissileClient;

            if (missile != null)
            {
                AutoAttackInstance autoattack = activeautoattacks
                    .Where(a => a._attacker == missile.SpellCaster && a._missile == missile).FirstOrDefault();

                if (autoattack != null)
                {
                    //Chat.Print("Missile Destroyed");
                    activeautoattacks.Remove(autoattack);
                }
            }
        }

        public static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!args.IsAutoAttack()) return;

            //0.10f is extra delay for missiles to be created
            AutoAttackInstance autoattack = activeautoattacks.Where(a => Game.Time > a._starttime + a._delay + 0.10f).FirstOrDefault();

            if (autoattack != null)
            {
                autoattack.finishedProcessing = true;

                if(autoattack._missile == null)
                    //Chat.Print("Basic Attack Completes");
                    activeautoattacks.Remove(autoattack);
            }
        }

        public static void Spellbook_OnStopCast(Obj_AI_Base sender, SpellbookStopCastEventArgs args)
        {
            AutoAttackInstance autoattack = activeautoattacks.Where(a => a._attacker == sender)
                .OrderBy(a => a._starttime).FirstOrDefault();

            if (autoattack != null)
            {
                //Chat.Print("Basic Attack Cancelled");
                activeautoattacks.Remove(autoattack);
            }
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.Instance.IsDead || Game.Time < 10
                || !MenuHandler.GetCheckBoxValue("Draw")) return;

            if (MenuHandler.GetCheckBoxValue("Draw_Player_AA"))
                Drawing.DrawCircle(Player.Instance.Position, Player.Instance.GetAARange(),
                    System.Drawing.Color.FromArgb(125, 125, 125));

            if (MenuHandler.GetCheckBoxValue("Draw_Allies_AA")
                && (!MenuHandler.GetCheckBoxValue("DrawC") || Player.Instance.CountAllyChampionsInRange(Player.Instance.VisionRange()) > 0))
            {
                foreach (Obj_AI_Base ally in EntityManager.Heroes.Allies)
                    if (ally.IsLegitimate() && !ally.IsPlayer())
                        Drawing.DrawCircle(ally.Position, ally.GetAARange(),
                            System.Drawing.Color.FromArgb(0, 0, 125));
            }

            if (MenuHandler.GetCheckBoxValue("Draw_Enemies_AA")
                && (!MenuHandler.GetCheckBoxValue("DrawC") || Player.Instance.CountEnemyChampionsInRange(Player.Instance.VisionRange()) > 0))
            {
                foreach (Obj_AI_Base enemy in EntityManager.Heroes.Enemies)
                {
                    if (enemy.IsLegitimate())
                    {
                        if (Player.Instance.IsInRange(enemy, enemy.GetAARange()))
                            Drawing.DrawCircle(enemy.Position, enemy.GetAARange(),
                                System.Drawing.Color.FromArgb(225, 255, 0));
                        else
                            Drawing.DrawCircle(enemy.Position, enemy.GetAARange(),
                                System.Drawing.Color.FromArgb(125, 0, 0));
                    }
                }
            }

            if (MenuHandler.GetCheckBoxValue("Draw_MinionsAlly_AA")
                && (!MenuHandler.GetCheckBoxValue("DrawC") || Player.Instance.CountAllyMinionsInRange(Player.Instance.VisionRange()) > 0))
            {
                foreach (Obj_AI_Base ally in EntityManager.MinionsAndMonsters.AlliedMinions)
                    if (ally.IsLegitimate())
                        Drawing.DrawCircle(ally.Position, ally.GetAARange(),
                            System.Drawing.Color.FromArgb(0, 0, 125));
            }

            if (MenuHandler.GetCheckBoxValue("Draw_MinionsEnemy_AA")
                && (!MenuHandler.GetCheckBoxValue("DrawC") || Player.Instance.CountEnemyMinionsInRange(Player.Instance.VisionRange()) > 0))
            {
                foreach (Obj_AI_Base enemy in EntityManager.MinionsAndMonsters.EnemyMinions)
                    if (enemy.IsLegitimate())
                        Drawing.DrawCircle(enemy.Position, enemy.GetAARange(),
                            System.Drawing.Color.FromArgb(125, 0, 0));
            }

            if (MenuHandler.GetCheckBoxValue("Draw_Creatures_AA")
                && (!MenuHandler.GetCheckBoxValue("DrawC") || Player.Instance.CountCreaturesInRange(Player.Instance.VisionRange()) > 0))
            {
                foreach (Obj_AI_Base creature in EntityManager.MinionsAndMonsters.Monsters)
                    if (creature.IsLegitimate())
                        Drawing.DrawCircle(creature.Position, creature.GetAARange(),
                            System.Drawing.Color.FromArgb(25, 125, 25));
            }
            /*
            if (MenuHandler.GetCheckBoxValue("Draw_TurretAlly_AA")
                && (!MenuHandler.GetCheckBoxValue("DrawC") || Player.Instance.CountAllyTurretsInRange(Player.Instance.ViewRange()) > 0))
            {
                foreach (Obj_AI_Base turret in EntityManager.Turrets.Allies)
                    if (turret.IsLegitimate())
                        Drawing.DrawCircle(turret.Position, turret.GetAARange(),
                            System.Drawing.Color.FromArgb(0, 0, 125));
            }*/

            if (MenuHandler.GetCheckBoxValue("Draw_TurretEnemy_AA")
                && (!MenuHandler.GetCheckBoxValue("DrawC") || Player.Instance.CountEnemyTurretsInRange(Player.Instance.VisionRange()) > 0))
            {
                foreach (Obj_AI_Base turret in EntityManager.Turrets.Enemies)
                {
                    if (turret.IsLegitimate())
                    {
                        if (Player.Instance.IsInRange(turret, turret.GetAARange()))
                            Drawing.DrawCircle(turret.Position, turret.GetAARange(),
                                System.Drawing.Color.FromArgb(225, 225, 0));
                        else
                            Drawing.DrawCircle(turret.Position, turret.GetAARange(),
                                System.Drawing.Color.FromArgb(125, 0, 0));
                    }
                }
            }

            foreach(Obj_AI_Minion min in Player.Instance.GetMinionsInRange(Player.Instance.GetAutoAttackRange()))
            {
                float time = GetBestAutoAttackTime(Player.Instance, min);
                if (time != 0)
                {
                    if (time <= Game.Time)
                        Drawing.DrawCircle(min.Position, min.BoundingRadius, System.Drawing.Color.White);
                    else if (time <= Game.Time + 2f)
                        Drawing.DrawCircle(min.Position, min.BoundingRadius, System.Drawing.Color.Orange);
                }
            }
        }

        public static float GetBestAutoAttackTime(Obj_AI_Base sender, Obj_AI_Base target)
        {
            float senderdmg = sender.GetAutoAttackDamage(target);
            float addeddmg = 0;
            float targethp = target.Health;

            if (targethp - senderdmg <= 0 && GetTargetHealthInTime(target, Game.Time + sender.TimeAttackWillHit(target)) > 0)
                return Game.Time;

            /*foreach (AutoAttackInstance inst in activeautoattacks.Where(a => a._target == target))
            {
                addeddmg += inst._damage;

                if (targethp - senderdmg - addeddmg <= 0)
                    return inst.TimeAttackWillHit - sender.TimeAttackWillHit(target);
            }*/

            return 0;
        }

        public static float GetTargetHealthInTime(Obj_AI_Base target, float time)
        {
            float addeddmg = 0;
            float targethp = target.Health;

            foreach (AutoAttackInstance inst in activeautoattacks.Where(a => a._target == target && a.TimeAttackWillHit <= time))
                addeddmg += inst._damage;

            return targethp - addeddmg;
        }
    }
}
