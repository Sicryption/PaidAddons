using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Collections.Generic;
using System.Linq;

namespace Ezreal
{
    internal class Program
    {
        public static Spell.Skillshot Q, W, E, R;

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

                if (addonsAuthedFor.Any(a => a.ToLower() == "all" || a.ToLower() == "ezreal"))
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

            if (Player.Instance.BaseSkinName != "Ezreal")
                return;

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            MenuHandler.Initialize();

            Q = new Spell.Skillshot(SpellSlot.Q, 1200, EloBuddy.SDK.Enumerations.SkillShotType.Linear, 250, 2000, 80, DamageType.Physical)
            {
                AllowedCollisionCount = 0,
            };
            W = new Spell.Skillshot(SpellSlot.W, 1050, EloBuddy.SDK.Enumerations.SkillShotType.Linear, 250, 1600, 80, DamageType.Magical);
            E = new Spell.Skillshot(SpellSlot.E, 475, EloBuddy.SDK.Enumerations.SkillShotType.Circular, 250, 0, 0, DamageType.Magical);
            R = new Spell.Skillshot(SpellSlot.R, 2000, EloBuddy.SDK.Enumerations.SkillShotType.Linear, 1000, 2000, 160, DamageType.Magical)
            {
                AllowedCollisionCount = int.MaxValue,
            };
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.Instance.IsDead)
                return;

            if (MenuHandler.Settings.GetCheckboxValue("Draw Q"))
                Q.DrawRange(System.Drawing.Color.Blue);
            if (MenuHandler.Settings.GetCheckboxValue("Draw W"))
                W.DrawRange(System.Drawing.Color.Blue);
            if (MenuHandler.Settings.GetCheckboxValue("Draw E"))
                E.DrawRange(System.Drawing.Color.Blue);
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!MenuHandler.Settings.GetCheckboxValue("E on Gapclose") || ModeHandler.hasDoneActionThisTick || !E.IsReady())
                return;

            if (sender.IsEnemy && Player.Instance.IsInRange(e.End, sender.GetAutoAttackRange()))
                ModeHandler.hasDoneActionThisTick = E.Cast(e.End - e.Start + Player.Instance.ServerPosition);
        }

        private static void Game_OnTick(EventArgs args)
        {
            ModeHandler.hasDoneActionThisTick = false;

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
            ModeHandler.AutoHarass();
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                ModeHandler.Flee();
            if (MenuHandler.Killsteal.GetCheckboxValue("Killsteal"))
                ModeHandler.Killsteal();
            if (MenuHandler.Settings.GetCheckboxValue("Tear Stacking"))
                ModeHandler.StackMode();

            if (MenuHandler.Settings.GetCheckboxValue("Skill Leveler"))
            {
                for (int i = Player.Instance.SpellTrainingPoints; i > 0; i--)
                {
                    int levelUpSkill = new int[] { 1, 3, 2, 1, 1, 4, 1, 3, 1, 3, 4, 3, 3, 2, 2, 4, 2, 2 }[Player.Instance.Level - i];

                    if (levelUpSkill == 1)
                        Player.LevelSpell(SpellSlot.Q);
                    if (levelUpSkill == 2)
                        Player.LevelSpell(SpellSlot.W);
                    if (levelUpSkill == 3)
                        Player.LevelSpell(SpellSlot.E);
                    if (levelUpSkill == 4)
                        Player.LevelSpell(SpellSlot.R);
                }
            }
        }
    }
}