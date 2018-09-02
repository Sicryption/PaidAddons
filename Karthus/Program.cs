using System;
using EloBuddy;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Constants;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace Karthus
{
    internal class Program
    {
        public static Spell.Skillshot Q, W;
        public static Spell.Active R;
        public static Spell.Targeted E;
        public static float LastAutoTime = 0;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static bool isAllowed()
        {
            if (Game.IsCustomGame)
               return true;

            string message = "if you are a programmer decompiling this to gain access, just PM me this and I'll auth you.";
            message += "this is only here to make sure it isnt removed during build";

            System.Net.WebClient wc = new System.Net.WebClient();

            var result = wc.DownloadString("https://raw.githubusercontent.com/Sicryption/PaidAddons/master/AllowedUsers");
            wc.Dispose();

            string username = EloBuddy.Sandbox.SandboxConfig.Username;

            if (result == null || result == "")
            {
                Chat.Print("Failed to reach GitHub.com.");
                return false;
            }

            List<string> listOfLines = result.Split(new string[] { "\n" }, StringSplitOptions.None).ToList();
            //I have this as an list to store multiple lines therefore I can have
            //[Caitlyn]x
            //and
            //[Ezreal]x
            //without conflict

            //allows me to make it free if I wanted too
            List<string> supportedNames = new List<string>() { username, "[Free]" };

            List<string> withUserName = listOfLines.Where(a => supportedNames.Any(b => a.Remove(0, a.IndexOf("]") + 1).Replace("\n", "").ToLower() == b.ToLower())).ToList();

            if (withUserName == null || withUserName.All(a => a == ""))
            {
                Chat.Print("This account has not purchased this addon.");
                return false;
            }

            foreach (string s in withUserName)
            {
                string addons = s.Replace("[", "").Remove(s.IndexOf("]") - 1);
                List<string> addonsAuthedFor = addons.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (addonsAuthedFor.Any(a => a.ToLower() == "all" || a.ToLower() == "karthus"))
                {
                    //Chat.Print("Welcome " + username);
                    return true;
                }
            }

            Chat.Print("This account has not purchased this addon.");
            return false;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (!isAllowed())
                return;

            if (Player.Instance.Hero != Champion.Karthus)
                return;

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            MenuHandler.Initialize();
            Player.OnLevelUp += Player_OnLevelUp;
            Orbwalker.OnAttack += Orbwalker_OnAttack;
            Orbwalker.OnAutoAttackReset += Orbwalker_OnAutoAttackReset;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            MissileClient.OnCreate += MissileClient_OnCreate;

            //when the game starts, without this it won't level the first ability
            Player_OnLevelUp(Player.Instance, null);
            
            Q = new Spell.Skillshot(SpellSlot.Q, 875, SkillShotType.Circular, 600, 0, 320, DamageType.Physical);
            W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Linear, 250, 700, 0, DamageType.True);
            E = new Spell.Targeted(SpellSlot.E, 1120 / 2);
            R = new Spell.Active(SpellSlot.R);//3 sec delay
        }

        private static void MissileClient_OnCreate(GameObject sender, EventArgs args)
        {
            if(!MenuHandler.Settings.GetCheckboxValue("Auto-Seraphs"))
                return;

            InventorySlot seraphs = Player.Instance.GetItem(ItemId.Seraphs_Embrace);

            if (seraphs == null || !seraphs.CanUseItem())
                return;

            MissileClient mis = sender as MissileClient;

            if (mis != null
                && mis.Target != null
                && mis.Target.IsMe
                && E.IsReady())
            {
                AIHeroClient enemy = mis.SpellCaster as AIHeroClient;

                //champion spell
                if (enemy != null && !mis.IsAutoAttack())
                    ShieldOnSpellDamage(enemy.GetSpellDamage(mis.Target as AIHeroClient, mis.Slot, DamageLibrary.SpellStages.Default), mis);
                //champion auto attack
                else if (enemy != null && mis.IsAutoAttack())
                    ShieldOnSpellDamage(enemy.GetAutoAttackDamage(mis.Target as AIHeroClient), mis);
                //other auto attack
                else if (mis.IsAutoAttack())
                    ShieldOnSpellDamage(mis.SpellCaster.GetAutoAttackDamage(mis.Target as AIHeroClient), mis);
            }
        }

        public static void ShieldOnSpellDamage(float damageIn, MissileClient mis)
        {
            InventorySlot seraphs = Player.Instance.GetItem(ItemId.Seraphs_Embrace);

            var shieldAmount = Calculations.SeraphsShield();
            var amountToShield = shieldAmount * 0.5;
            var percentOfUnitMaxHealth = (mis.Target as AIHeroClient).MaxHealth * 0.25;
            
            if (damageIn > amountToShield
                || damageIn > Player.Instance.Health
                || damageIn > percentOfUnitMaxHealth)
                seraphs.Cast();
        }

        public static Obj_AI_Base UnitBeingAutoAttacked;

        private static void Orbwalker_OnAutoAttackReset(EventArgs args)
        {
            UnitBeingAutoAttacked = null;
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            UnitBeingAutoAttacked = null;
        }
        private static void Orbwalker_OnAttack(AttackableUnit target, EventArgs args)
        {
            if(target as Obj_AI_Base != null)
            {
                UnitBeingAutoAttacked = target as Obj_AI_Base;
                //Chat.Print("attack");
            }
        }

        private static void Player_OnLevelUp(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            if (MenuHandler.Settings.GetCheckboxValue("Skill Leveler"))
            {
                List<SpellDataInst> spells = new List<SpellDataInst>()
                {
                    Player.Instance.Spellbook.GetSpell(SpellSlot.R),
                    Player.Instance.Spellbook.GetSpell(SpellSlot.Q),
                    Player.Instance.Spellbook.GetSpell(SpellSlot.E),
                    Player.Instance.Spellbook.GetSpell(SpellSlot.W),
                };

                List<SpellSlot> slots = new List<SpellSlot>()
                {
                    SpellSlot.Q,
                    SpellSlot.W,
                    SpellSlot.E
                };
                var nonLeveledSpells = Player.Instance.Spellbook.Spells.Where(a => a.Level == 0 && slots.Any(b=>b == a.Slot)).ToList();
                foreach (SpellDataInst sp in spells)
                {
                    if (nonLeveledSpells.Count != 0 && nonLeveledSpells.Any(a => a.Slot == sp.Slot))
                        Player.Instance.Spellbook.LevelSpell(sp.Slot);
                    else if (nonLeveledSpells.Count == 0)
                        Player.Instance.Spellbook.LevelSpell(sp.Slot);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Menu menu = MenuHandler.Settings;
            if (menu.GetCheckboxValue("Draw Spell Ranges"))
            {
                Q.DrawRange(Color.Blue);
                W.DrawRange(Color.Blue);
                E.DrawRange(Color.Blue);
            }

            if(menu.GetCheckboxValue("Draw Name if Killable with R"))
            {
                int yOffset = 0;
                foreach(var hero in EntityManager.Heroes.Enemies)
                {
                    if(Calculations.R(hero) > Prediction.Health.GetPrediction(hero, Program.R.CastDelay))
                    {
                        Drawing.DrawText(Drawing.Width - 225, Drawing.Height / 2 + yOffset, (hero.IsHPBarRendered)?System.Drawing.Color.Blue:System.Drawing.Color.Yellow, "[R] " + hero.ChampionName);
                    }
                    else
                        Drawing.DrawText(Drawing.Width - 225, Drawing.Height / 2 + yOffset, System.Drawing.Color.Red, "[R] " + hero.ChampionName);

                    yOffset += 15;
                }
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            ModeHandler.hasDoneActionThisTick = false;
            ModeHandler.useEThisTick = false;

            if (Player.Instance.HasBuff("KarthusDeathDefiedBuff"))
                ModeHandler.Dead();
            else
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    ModeHandler.Combo();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                    ModeHandler.JungleClear();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                    ModeHandler.LastHit();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                    ModeHandler.LaneClear();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                    ModeHandler.Harass();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                    ModeHandler.Flee();
                if (MenuHandler.Killsteal.GetCheckboxValue("Killsteal"))
                    ModeHandler.Killsteal();
                if (MenuHandler.Settings.GetCheckboxValue("Tear Stacker"))
                    ModeHandler.StackTear();
                ModeHandler.AutoHarass();

                ModeHandler.EDisabler();
            }
        }
    }
}