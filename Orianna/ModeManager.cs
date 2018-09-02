using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Spells;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

internal class ModeManager
{
    public static bool hasDoneActionThisTick = false;
    public static AIHeroClient Orianna => Player.Instance;

	public static void Combo()
	{
		Menu menu = MenuManager.Combo;
		List<Obj_AI_Base> list = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();
		if (menu.GetCheckboxValue("Q"))
			CastQ(list, false, menu, false);

		if (menu.GetSliderValue("Use Q to position for x enemies to hit with R    (0 is disable)") != 0)
			CastQ(list, false, menu, true);

		if (menu.GetCheckboxValue("R"))
			CastR(list, false);

		if (menu.GetCheckboxValue("W"))
			CastW(list, false, menu);

		if (menu.GetCheckboxValue("W for Speed-Up") && Calculations.BallPosition.IsInRange(Orianna, Program.W.Range) && Program.W.IsReady())
			if (list.Any(a=> a.IsFacing(Orianna)) && !hasDoneActionThisTick)
                hasDoneActionThisTick = Program.W.Cast(Orianna.Position);

		if (menu.GetCheckboxValue("E Allies") || menu.GetCheckboxValue("E Self"))
			CastE(list, false, menu);

		if (menu.GetCheckboxValue("Ignite"))
			CastIgnite(list, true);
	}

	public static void Harass()
	{
		Menu menu = MenuManager.Harass;
		List<Obj_AI_Base> list = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();
		if (menu.GetCheckboxValue("Q"))
			CastQ(list, false, menu, false);

		if (menu.GetCheckboxValue("W"))
			CastW(list, false, menu);

		if (menu.GetCheckboxValue("E Allies") || menu.GetCheckboxValue("E Self"))
			CastE(list, false, menu);
	}

	public static void AutoHarass()
	{
		if (!Orianna.IsUnderEnemyturret() && !Orianna.IsRecalling())
		{
			Menu menu = MenuManager.AutoHarass;
			List<Obj_AI_Base> list = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();
			if (menu.GetCheckboxValue("Q"))
				CastQ(list, false, menu, false);

			if (menu.GetCheckboxValue("W"))
				CastW(list, false, menu);

			if (menu.GetCheckboxValue("E Allies") || menu.GetCheckboxValue("E Self"))
				CastE(list, false, menu);
		}
	}

	public static void JungleClear()
	{
		Menu menu = MenuManager.JungleClear;
		List<Obj_AI_Base> list = EntityManager.MinionsAndMonsters.Monsters.ToList().ToObj_AI_BaseList();
		if (menu.GetCheckboxValue("Q"))
			CastQ(list, false, menu, false);

		if (menu.GetCheckboxValue("W"))
			CastW(list, false, menu);

		if (menu.GetCheckboxValue("E Allies") || menu.GetCheckboxValue("E Self"))
			CastE(list, false, menu);
	}

	public static void Killsteal()
	{
		Menu menu = MenuManager.Killsteal;
		List<Obj_AI_Base> list = EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();
		if (menu.GetCheckboxValue("Q"))
			CastQ(list, true, menu, false);

		if (menu.GetCheckboxValue("W"))
			CastW(list, true, menu);

		if (menu.GetCheckboxValue("E Allies") || menu.GetCheckboxValue("E Self"))
			CastE(list, true, menu);

		if (menu.GetCheckboxValue("R"))
			CastR(list, true);

		if (menu.GetCheckboxValue("Ignite"))
			CastIgnite(list, true);
	}

	public static void Flee()
	{
		Menu menu = MenuManager.Flee;
        EntityManager.Heroes.Enemies.ToList().ToObj_AI_BaseList();
		if (menu.GetCheckboxValue("W") && Calculations.BallPosition.IsInRange(Orianna, Program.W.Range) && !hasDoneActionThisTick && Program.W.IsReady())
			hasDoneActionThisTick = Program.W.Cast(Orianna.Position);

		if (menu.GetCheckboxValue("E Self") && !hasDoneActionThisTick && Calculations.BallPosition != Vector3.Zero && Calculations.BallPosition != Orianna.Position && !Program.ballOnOrianna && Program.E.IsReady())
			hasDoneActionThisTick = Program.E.Cast(Orianna);
	}

	public static void LaneClear()
	{
		Menu menu = MenuManager.LaneClear;
		List<Obj_AI_Base> list = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();
		if (menu.GetCheckboxValue("Q"))
			CastQ(list, false, menu, false);

		if (menu.GetCheckboxValue("W"))
			CastW(list, false, menu);

		if (menu.GetCheckboxValue("E Allies") || menu.GetCheckboxValue("E Self"))
			CastE(list, false, menu);
	}

	public static void LastHit()
	{
		Menu menu = MenuManager.LastHit;
		List<Obj_AI_Base> list = EntityManager.MinionsAndMonsters.EnemyMinions.ToList().ToObj_AI_BaseList();
		if (menu.GetCheckboxValue("Q"))
			CastQ(list, true, menu, false);

		if (menu.GetCheckboxValue("W"))
			CastW(list, true, menu);

		if (menu.GetCheckboxValue("E Allies") || menu.GetCheckboxValue("E Self"))
			CastE(list, true, menu);
	}

	public static void CastQ(List<Obj_AI_Base> targets, bool ks, Menu menu, bool qIntoPosition = false)
	{
		if (!hasDoneActionThisTick && Program.Q.IsReady() && menu != null && (MenuManager.GetSlider(menu, "Q Mana %") == null || menu.GetSliderValue("Q Mana %") <= (int)Orianna.ManaPercent))
        {
            targets = targets.Where(a => a.IsInRange(Orianna, Program.Q.Range)).ToList();
			if (ks)
                targets = targets.Where(a => a.Health <= Calculations.Q(a, 4)).ToList();

			if (targets.Count > 0)
			{
				int minionsToHitQWith = (menu == null || MenuManager.GetSlider(menu, "Minions to hit with Q") == null) ? 1 : menu.GetSliderValue("Minions to hit with Q");
				List<Obj_AI_Base> enemiesHit = new List<Obj_AI_Base>();
				Obj_AI_Base bestTarget = null;
                foreach(Obj_AI_Base ob in targets)
                {
                    float qIntoRRange = ((Program.R.IsReady() && qIntoPosition) ? Program.R.Range : 175);
                    Geometry.Polygon.Rectangle path = new Geometry.Polygon.Rectangle(Calculations.BallPosition, ob.Position, (float)Program.Q.Width);

                    List<Obj_AI_Base> tempEnemiesHit = targets.Where(a=>
                        !path.IsOutside(a.Position.To2D())
                        || a.Position == Calculations.BallPosition
                        || Program.Q.GetPrediction(ob).UnitPosition.IsInRange(a, qIntoRRange)).ToList();

                    if (tempEnemiesHit.Count > enemiesHit.Count)
                    {
                        enemiesHit = tempEnemiesHit;
                        bestTarget = ob;
                    }
                    if (bestTarget != null && tempEnemiesHit.Count == enemiesHit.Count && ob.HealthPercent < bestTarget.HealthPercent)
                    {
                        enemiesHit = tempEnemiesHit;
                        bestTarget = ob;
                    }
                }

				if (enemiesHit.Count >= minionsToHitQWith && bestTarget != null && (!qIntoPosition || enemiesHit.Count >= menu.GetSliderValue("Use Q to position for x enemies to hit with R    (0 is disable)")))
				{
					hasDoneActionThisTick = Program.Q.Cast(Program.Q.GetPrediction(bestTarget).CastPosition);
				}
			}
		}
	}

	public static void CastW(List<Obj_AI_Base> targets, bool ks, Menu menu)
    {
        if (!hasDoneActionThisTick 
            //should always W then R since W has no cast time
            //&& !Calculations.ShouldR(targets, ks) 
            && Calculations.BallPosition != Vector3.Zero
            && Program.W.IsReady() 
            && menu != null 
            && (MenuManager.GetSlider(menu, "W Mana %") == null || menu.GetSliderValue("W Mana %") <= (int)Orianna.ManaPercent))
        {
            targets = targets.Where(a => a.IsInRange(Calculations.BallPosition, Program.W.Range)).ToList();
            if (ks)
                targets = targets.Where(a => Calculations.W(a) >= a.Health).ToList();

			if (targets.Count > 0)
            {
                int enemiesToHit = (menu == null || MenuManager.GetSlider(menu, "Minions to hit with W") == null) ? 1 : menu.GetSliderValue("Minions to hit with W");
				if (targets.Count >= enemiesToHit)
					hasDoneActionThisTick = Program.W.Cast(Orianna.Position);
			}
		}
	}

	public static void CastE(List<Obj_AI_Base> targets, bool ks, Menu menu)
	{
		if (!hasDoneActionThisTick && Program.E.IsReady() && !Calculations.ShouldR(targets, ks) && menu != null && (MenuManager.GetSlider(menu, "E Mana %") == null || menu.GetSliderValue("E Mana %") <= (int)Orianna.ManaPercent))
        {
            if (ks)
                targets = targets.Where(a => Calculations.E(a) >= a.Health).ToList();

            if (targets.Count > 0)
			{
				int enemiesToHit = (menu == null || MenuManager.GetSlider(menu, "Minions to hit with E") == null) ? 1 : menu.GetSliderValue("Minions to hit with E");
				List<AIHeroClient> list = new List<AIHeroClient>();
                
                if (MenuManager.GetCheckbox(menu, "E Allies") != null && menu.GetCheckboxValue("E Allies"))
                    list = EntityManager.Heroes.Allies.Where(a => a.IsInRange(Orianna, Program.Q.Range) && !a.IsMe).ToList();

                if (MenuManager.GetCheckbox(menu, "E Self") != null && menu.GetCheckboxValue("E Self"))
                    list.Add(Orianna);

                List<Obj_AI_Base> enemiesHit = new List<Obj_AI_Base>();
				AIHeroClient targetUsingEOn = null;
				foreach (AIHeroClient current in list)
                {
                    Geometry.Polygon.Rectangle path = new Geometry.Polygon.Rectangle(Calculations.BallPosition, current.Position, 160f);
					List<Obj_AI_Base> targsHit = targets.Where(a=> !path.IsOutside(a.Position.To2D()) || a.Position == Calculations.BallPosition).ToList<Obj_AI_Base>();
					if (targsHit.Count > enemiesHit.Count)
					{
						enemiesHit = targsHit;
						targetUsingEOn = current;
					}
					if (targsHit.Count == enemiesHit.Count && enemiesHit.Count != 0 && current.IsMe)
					{
						enemiesHit = targsHit;
						targetUsingEOn = current;
					}
				}
				if (enemiesHit.Count >= enemiesToHit && targetUsingEOn != null)
                {
                    hasDoneActionThisTick = Program.E.Cast(targetUsingEOn);
                }
			}
		}
	}

	public static void CastR(List<Obj_AI_Base> targets, bool ks)
	{
		if (Calculations.ShouldR(targets, ks))
			hasDoneActionThisTick = Program.R.Cast(Orianna.Position);
	}

	public static void CastIgnite(List<Obj_AI_Base> targets, bool ks)
	{
        Spell.Targeted ignite = SummonerSpells.Ignite;
        
		if (ignite.Slot != SpellSlot.Unknown && ignite.IsReady())
		{
			Obj_AI_Base target = targets.Where(a=>a.IsInRange(Orianna, ignite.Range) && (!ks || Calculations.Ignite(a) >= a.Health) && a.MeetsCriteria()).FirstOrDefault();
			if (target != null)
				hasDoneActionThisTick = ignite.Cast(target);
		}
	}
}
