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

namespace Katarina
{
    internal class Program
    {
        public static List<WallJump> wallJumpSpots = new List<WallJump>();

        public static Spell.Targeted Q;
        public static Spell.Skillshot E;
        public static Spell.Active W, R;

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

            List<string> withUserName = listOfLines.Where(a => supportedNames.Any(b=>a.Remove(0, a.IndexOf("]") + 1).Replace("\n", "").ToLower() == b.ToLower())).ToList();

            if (withUserName == null || withUserName.All(a => a == ""))
            {
                Chat.Print("This account has not purchased this addon.");
                return false;
            }

            foreach (string s in withUserName)
            {
                string addons = s.Replace("[", "").Remove(s.IndexOf("]") - 1);
                List<string> addonsAuthedFor = addons.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (addonsAuthedFor.Any(a => a.ToLower() == "all" || a.ToLower() == "katarina"))
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

            if (Player.Instance.Hero != Champion.Katarina)
                return;

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            MenuHandler.Initialize();
            Player.OnLevelUp += Player_OnLevelUp;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;

            //when the game starts, without this it won't level the first ability
            Player_OnLevelUp(Player.Instance, null);

            Q = new Spell.Targeted(SpellSlot.Q, 625, DamageType.Magical);
            W = new Spell.Active(SpellSlot.W, 0);
            E = new Spell.Skillshot(SpellSlot.E, 725, SkillShotType.Circular, 200, 0, 170, DamageType.Magical);
            R = new Spell.Active(SpellSlot.R, 550);
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name.Contains("Dagger_Ground"))
            {
                //Chat.Print("Particle: " + sender.Name + " removed from: " + sender.Position);
                wallJumpSpots.RemoveAll(a => a.particle == sender as Obj_AI_Base);
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if(sender.Name.Contains("Dagger_Ground"))
            {
                List<WallJump> wallPositions = new List<WallJump>();

                Vector2 tempExtendPos = (sender.Position + new Vector3(0, 100, 0)).To2D();
                List<Vector3> checkPositions = new List<Vector3>();

                for (int i = 0; i < 24; i++)
                {
                    bool wallBetween = false;
                    Vector3 lastEmptySpot = sender.Position;
                    for (int divisor = 1; divisor <= 8; divisor++)
                    {
                        Vector3 endPosition = sender.Position.Extend(tempExtendPos.RotateAroundPoint(sender.Position.To2D(), (float)((i * 15) * Math.PI / 180)), ((Program.E.Radius + 220 - 1) / 8) * divisor).To3D((int)sender.Position.Z);

                        if (wallBetween && !endPosition.IsWall() && !endPosition.IsBuilding())
                        {
                            wallBetween = false;
                            wallPositions.Add(new WallJump(lastEmptySpot, endPosition, sender as Obj_AI_Base));
                        }

                        if (endPosition.IsWall() || endPosition.IsBuilding())
                            wallBetween = true;
                        else
                            lastEmptySpot = endPosition;

                    }
                }

                wallJumpSpots.AddRange(wallPositions);
                //Chat.Print("Wall Spots Added: " + wallJumpSpots.Count());
                //Chat.Print("Particle: " + sender.Name + " added from: " + sender.Position);
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
                E.DrawRange(Color.Blue);
                R.DrawRange(Color.Blue);
            }
            
            if (menu.GetCheckboxValue("Draw Jump Spots"))
                foreach(WallJump j in wallJumpSpots)
                    Drawing.DrawLine(j.startPos.WorldToScreen(), j.endPos.WorldToScreen(), 2, System.Drawing.Color.Red);
        }

        private static void Game_OnTick(EventArgs args)
        {
            ModeHandler.hasDoneActionThisTick = false;

            if (MenuHandler.Killsteal.GetCheckboxValue("Killsteal"))
                ModeHandler.Killsteal();

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
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.None))
                ModeHandler.AutoHarass();
        }
    }
}