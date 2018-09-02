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

namespace Fiora
{
    internal class Program
    {
        public static Spell.Skillshot Q, W;
        public static Spell.Active E;
        public static Spell.Targeted R;
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

                if (addonsAuthedFor.Any(a => a.ToLower() == "all" || a.ToLower() == "fiora"))
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

            if (Player.Instance.BaseSkinName != "Fiora")
                return;

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += DrawVitals;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            MenuHandler.Initialize();
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;

            Q = new Spell.Skillshot(SpellSlot.Q, 400, SkillShotType.Circular, 250, null, null, DamageType.Physical);
            W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Linear, 250, 3200, 70, DamageType.Magical); 
            E = new Spell.Active(SpellSlot.E, 0, DamageType.Physical);
            R = new Spell.Targeted(SpellSlot.R, 500, DamageType.True);
        }

        public static float startTime = 0;
        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Speed")
                && !sender.Name.Contains("Heal")
                && !sender.Name.Contains("hit_tar")
                && (sender.Name.Contains("Fiora_Base_Passive") || sender.Name.Contains("Fiora_Base_R_Mark") || (!sender.Name.Contains("Passive") && sender.Name.Contains("_Timeout"))))
            {
                Calculations.Vitals.Remove(Calculations.Vitals.Where(a => a.vital == sender).FirstOrDefault());
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Speed")
                && !sender.Name.Contains("Heal")
                && !sender.Name.Contains("hit_tar")
                && (sender.Name.Contains("Fiora_Base_Passive") || sender.Name.Contains("Fiora_Base_R_Mark") || (!sender.Name.Contains("Passive") && sender.Name.Contains("_Timeout"))))
            {
                Vital v = new Vital(sender, EntityManager.Heroes.Enemies.OrderBy(a=>a.Position.DistanceSquared(sender.Position)).FirstOrDefault());
                
                Calculations.Vitals.Add(v);
            }
            if (sender.Type == GameObjectType.obj_GeneralParticleEmitter)
            {
                if(sender.Name.Contains("Fiora") && !sender.Name.Contains("Speed") && !sender.Name.Contains("Heal"))
                Console.WriteLine(sender.Name);
            }
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
             LastAutoTime = Game.Time;
        }

        private static void DrawVitals(EventArgs args)
        {
            if (MenuHandler.Settings.GetCheckboxValue("Draw Vitals"))
                foreach (AIHeroClient enemy in EntityManager.Heroes.Enemies.Where(a => a.HasBuff("fiorapassivemanager") || a.HasBuff("fiorarmark")))
                {
                    var vitals = Calculations.GetVitals(enemy);
                    if (vitals.Count > 0)
                        foreach (Vital vital in vitals)
                        {
                            if (vital.isReady)
                                vital.sector.Draw(System.Drawing.Color.Blue, 1);
                            else if (MenuHandler.Settings.GetCheckboxValue("Draw Loading Vitals"))
                                vital.sector.Draw(System.Drawing.Color.Orange, 1);
                        }
                }
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