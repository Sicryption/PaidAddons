using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System;

internal class MenuManager
{
    public static Menu main, Combo, Harass, AutoHarass, Killsteal, LaneClear, JungleClear, LastHit, Flee, RLogic, AutoShield, Settings;

	public static void Initialize()
	{
		main = MainMenu.AddMenu("Orianna", "Orianna");
		Combo = AddSubMenu(main, "Combo");
		Harass = AddSubMenu(main, "Harass");
        AutoHarass = AddSubMenu(main, "Auto Harass");
		Killsteal = AddSubMenu(main, "Killsteal");
		LaneClear = AddSubMenu(main, "Lane Clear");
		JungleClear = AddSubMenu(main, "Jungle Clear");
		LastHit = AddSubMenu(main, "Last Hit");
		Flee = AddSubMenu(main, "Flee");
		RLogic = AddSubMenu(main, "R Logic");
		AutoShield = AddSubMenu(main, "AutoShield");
		Settings = AddSubMenu(main, "Settings");
		AddCheckboxes(ref Combo, new string[]
		{
			"Q",
			"W",
			"W for Speed-Up_false",
			"E Self",
			"E Allies",
			"R",
			"Ignite"
		});
		AddSlider(Combo, "Use Q to position for x enemies to hit with R    (0 is disable)", 3, 0, 5);
		AddSlider(Combo, "Q Mana %", 0, 0, 100);
		AddSlider(Combo, "W Mana %", 0, 0, 100);
		AddSlider(Combo, "E Mana %", 0, 0, 100);
		AddCheckboxes(ref Harass, new string[]
		{
			"Q",
			"W",
			"E Self",
			"E Allies"
		});
		AddSlider(Harass, "Q Mana %", 25, 0, 100);
		AddSlider(Harass, "W Mana %", 50, 0, 100);
		AddSlider(Harass, "E Mana %", 50, 0, 100);
		AddCheckboxes(ref AutoHarass, new string[]
		{
			"Q",
			"W",
			"E Self",
			"E Allies_false"
		});
		AddSlider(AutoHarass, "Q Mana %", 80, 0, 100);
		AddSlider(AutoHarass, "W Mana %", 80, 0, 100);
		AddSlider(AutoHarass, "E Mana %", 80, 0, 100);
		AddCheckboxes(ref Killsteal, new string[]
		{
			"Killsteal",
			"Q",
			"W",
			"E Self",
			"E Allies",
			"R",
			"Ignite"
		});
		AddCheckboxes(ref LaneClear, new string[]
		{
			"Q",
			"W",
			"E Self",
			"E Allies"
		});
		AddSlider(LaneClear, "Q Mana %", 50, 0, 100);
		AddSlider(LaneClear, "W Mana %", 50, 0, 100);
		AddSlider(LaneClear, "E Mana %", 50, 0, 100);
		AddSlider(LaneClear, "Minions to hit with Q", 4, 0, 8);
		AddSlider(LaneClear, "Minions to hit with W", 4, 0, 8);
		AddSlider(LaneClear, "Minions to hit with E", 4, 0, 8);
		AddCheckboxes(ref JungleClear, new string[]
		{
			"Q",
			"W",
			"E Self",
			"E Allies"
		});
		AddSlider(JungleClear, "Q Mana %", 10, 0, 100);
		AddSlider(JungleClear, "W Mana %", 10, 0, 100);
		AddSlider(JungleClear, "E Mana %", 10, 0, 100);
		AddCheckboxes(ref LastHit, new string[]
		{
			"Q",
			"W",
			"E Self",
			"E Allies"
		});
		AddSlider(LastHit, "Q Mana %", 80, 0, 100);
		AddSlider(LastHit, "W Mana %", 80, 0, 100);
		AddSlider(LastHit, "E Mana %", 80, 0, 100);
		AddSlider(LastHit, "Minions to hit with Q", 4, 0, 8);
		AddSlider(LastHit, "Minions to hit with W", 4, 0, 8);
		AddSlider(LastHit, "Minions to hit with E", 4, 0, 8);
		AddCheckboxes(ref Flee, new string[]
		{
			"W",
			"E Self"
		});
		AddCheckboxes(ref AutoShield, new string[]
		{
			"E Self",
			"E Allies_false",
			"E if damage would kill target"
		});
		AddSlider(AutoShield, "Incoming Damage > % Health", 10, 1, 100);
		AddSlider(AutoShield, "Incoming Damage > % Shield Amount", 50, 1, 100);
		AddCheckboxes(ref Settings, new string[]
		{
			"Skill Leveler",
			"Draw Spell Ranges",
			"Draw Ball Return Range",
			"Draw Ball Return Ally Range",
			"Draw Safe Return Area Instead of Range"
		});
		AddSlider(RLogic, "X Enemies Hit to R", 2, 1, 5);
		AddSlider(RLogic, "% enemies in range to R", 50, 1, 100);
	}

    public static void AddCheckboxes(ref Menu menu, params string[] checkBoxValues)
    {
        foreach (string s in checkBoxValues)
        {
            if (s.Length > "_false".Length && s.Contains("_false"))
                AddCheckbox(ref menu, s.Remove(s.IndexOf("_false"), 6), false);
            else
                AddCheckbox(ref menu, s, true);
        }
    }

    public static Menu AddSubMenu(Menu startingMenu, string text)
    {
        Menu menu = startingMenu.AddSubMenu(text, text + ".");
        menu.AddGroupLabel(text + " Settings");
        return menu;
    }

    public static CheckBox AddCheckbox(ref Menu menu, string text, bool defaultValue = true)
    {
        return menu.Add(menu.UniqueMenuId + text, new CheckBox(text, defaultValue));
    }

    public static CheckBox GetCheckbox(Menu menu, string text)
    {
        return menu.Get<CheckBox>(menu.UniqueMenuId + text);
    }

    public static bool GetCheckboxValue(Menu menu, string text)
        {
            CheckBox checkbox = GetCheckbox(menu, text);

            if (checkbox == null)
                Console.WriteLine("Checkbox (" + text + ") not found under menu (" + menu.DisplayName + "). Unique ID (" + menu.UniqueMenuId + text + ")");

            return checkbox.CurrentValue;
        }
    public static ComboBox AddComboBox(Menu menu, string text, int defaultValue = 0, params string[] values)
    {
        return menu.Add(menu.UniqueMenuId + text, new ComboBox(text, defaultValue, values));
    }
    public static ComboBox GetComboBox(Menu menu, string text)
    {
        return menu.Get<ComboBox>(menu.UniqueMenuId + text);
    }
    public static string GetComboBoxText(Menu menu, string text)
    {
        return menu.Get<ComboBox>(menu.UniqueMenuId + text).SelectedText;
    }
    public static Slider GetSlider(Menu menu, string text)
    {
        return menu.Get<Slider>(menu.UniqueMenuId + text);
    }
    public static int GetSliderValue(Menu menu, string text)
    {
        return menu.Get<Slider>(menu.UniqueMenuId + text).CurrentValue;
    }
    public static Slider AddSlider(Menu menu, string text, int defaultValue, int minimumValue, int maximumValue)
    {
        return menu.Add(menu.UniqueMenuId + text, new Slider(text, defaultValue, minimumValue, maximumValue));
    }
}
