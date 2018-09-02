using EloBuddy;
using EloBuddy.Sandbox;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

internal class Program
{    
	public static Spell.Skillshot Q, W, R;
	public static Spell.Targeted E;
	public static bool ballOnOrianna = false;

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

            if (addonsAuthedFor.Any(a => a.ToLower() == "all" || a.ToLower() == "orianna"))
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

        if (Player.Instance.Hero != Champion.Orianna)
            return;

        Game.OnTick += Game_OnTick;
        Drawing.OnDraw += Drawing_OnDraw;
        MenuManager.Initialize();
        Player.OnLevelUp += Player_OnLevelUp;
        GameObject.OnCreate += GameObject_OnCreate;
        Player_OnLevelUp(Player.Instance, null);
		Q = new Spell.Skillshot(SpellSlot.Q, 825, SkillShotType.Linear, 0, 1200, 160, DamageType.Magical);
		W = new Spell.Skillshot(SpellSlot.W, 255, SkillShotType.Circular, 0, 0, 255, DamageType.Magical);
		E = new Spell.Targeted(SpellSlot.E, 1120, DamageType.Magical);
		R = new Spell.Skillshot(SpellSlot.R, 410, SkillShotType.Circular, 750, 0, 410, DamageType.Magical);
	}

	private static void GameObject_OnCreate(GameObject obj, EventArgs args)
	{
		Menu menu = MenuManager.AutoShield;
		if (menu.GetCheckboxValue("E Self") || menu.GetCheckboxValue("E Allies"))
		{
			MissileClient missileClient = obj as MissileClient;
			if (E.IsReady()
                && missileClient != null 
                && missileClient.Target != null 
                && (
                    (missileClient.Target.IsMe && menu.GetCheckboxValue("E Self")) 
                    || (
                        (missileClient.Target.IsAlly && missileClient.Target.IsInRange(Player.Instance, E.Range) && missileClient.Target.Type == GameObjectType.AIHeroClient && menu.GetCheckboxValue("E Allies"))
                        )))
			{
				AIHeroClient caster = missileClient.SpellCaster as AIHeroClient;
				if (caster != null && !AutoAttacks.IsAutoAttack(missileClient))
					AutoShield(caster.GetSpellDamage(missileClient.Target as AIHeroClient, missileClient.Slot), missileClient);
				else if (caster != null && AutoAttacks.IsAutoAttack(missileClient))
					AutoShield(caster.GetAutoAttackDamage(missileClient.Target as AIHeroClient), missileClient);
				else if (AutoAttacks.IsAutoAttack(missileClient))
					AutoShield(missileClient.SpellCaster.GetAutoAttackDamage(missileClient.Target as AIHeroClient, false), missileClient);
			}
		}
	}

	public static void AutoShield(float damage, MissileClient miss)
	{
        if (miss.Target as AIHeroClient == null)
            return;

		Menu menu = MenuManager.AutoShield;
		float shieldAmount = Calculations.ShieldAmount();
		float num2 = shieldAmount * (float)(menu.GetSliderValue("Incoming Damage > % Shield Amount") / 100);
		float num3 = (miss.Target as AIHeroClient).MaxHealth * (menu.GetSliderValue("Incoming Damage > % Health") / 100);
        float misSpeed = 1500;
        float distanceToTarget = miss.Position.Distance(miss.Target as AIHeroClient, false);
		float timeTilHit = distanceToTarget / misSpeed;
        float ESpeed = 1850f;
		float targDistanceFromBall = (miss.Target as AIHeroClient).Distance(Calculations.BallPosition, false);
		float timeForBallShield = targDistanceFromBall / ESpeed;

        if (timeForBallShield < timeTilHit 
            && 
            (damage > num2 
                || (damage > (miss.Target as AIHeroClient).Health && menu.GetCheckboxValue("E if damage would kill target")) 
                || damage > num3))
            E.Cast(miss.Target as AIHeroClient);
    }
    
	private static void Player_OnLevelUp(Obj_AI_Base obj_AI_Base_0, Obj_AI_BaseLevelUpEventArgs obj_AI_BaseLevelUpEventArgs_0)
	{
        if (MenuManager.Settings.GetCheckboxValue("Skill Leveler"))
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
            var nonLeveledSpells = Player.Instance.Spellbook.Spells.Where(a => a.Level == 0 && slots.Any(b => b == a.Slot)).ToList();
            foreach (SpellDataInst sp in spells)
            {
                if (nonLeveledSpells.Count != 0 && nonLeveledSpells.Any(a => a.Slot == sp.Slot))
                    Player.Instance.Spellbook.LevelSpell(sp.Slot);
                else if (nonLeveledSpells.Count == 0)
                    Player.Instance.Spellbook.LevelSpell(sp.Slot);
            }
        }
    }

	private static void Drawing_OnDraw(EventArgs eventArgs_0)
    {
        Menu menu = MenuManager.Settings;
        if (menu.GetCheckboxValue("Draw Spell Ranges"))
		{
			Q.DrawRange(System.Drawing.Color.Blue);
            E.DrawRange(System.Drawing.Color.Blue);
            if (Calculations.BallPosition != Vector3.Zero)
            {
                Drawing.DrawCircle(Calculations.BallPosition, W.Range, System.Drawing.Color.Blue);
                Drawing.DrawCircle(Calculations.BallPosition, R.Range, System.Drawing.Color.Blue);
            }
		}
		if (menu.GetCheckboxValue("Draw Ball Return Range") || menu.GetCheckboxValue("Draw Ball Return Ally Range"))
		{
            if (Calculations.Ball != null)
			{
				if (Calculations.Ball.Name.Contains("Ghost") && menu.GetCheckboxValue("Draw Ball Return Ally Range"))
				{
					if (menu.GetCheckboxValue("Draw Safe Return Area Instead of Range"))
						Drawing.DrawCircle(Calculations.BallPosition, E.Range + 200, System.Drawing.Color.Green);
					else
                        Drawing.DrawCircle(Player.Instance.Position, E.Range + 200, System.Drawing.Color.Yellow);
				}
				if (Calculations.Ball.Name.Contains("Yomu") && menu.GetCheckboxValue("Draw Ball Return Range"))
				{
					if (menu.GetCheckboxValue("Draw Safe Return Area Instead of Range"))
                        Drawing.DrawCircle(Calculations.BallPosition, Q.Range + 200, System.Drawing.Color.Green);
					else
                        Drawing.DrawCircle(Player.Instance.Position, Q.Range + 200, System.Drawing.Color.Yellow);
				}
			}
		}
	}

	private static void Game_OnTick(EventArgs eventArgs_0)
	{
		ModeManager.hasDoneActionThisTick = false;

        if (MenuManager.Killsteal.GetCheckboxValue("Killsteal"))
            ModeManager.Killsteal();
        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            ModeManager.Combo();
        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            ModeManager.JungleClear();
        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            ModeManager.LastHit();
        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            ModeManager.LaneClear();
        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            ModeManager.Harass();
        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            ModeManager.Flee();
        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.None))
            ModeManager.AutoHarass();
	}
}
