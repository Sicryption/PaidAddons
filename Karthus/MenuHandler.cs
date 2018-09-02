using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace Karthus
{
    class MenuHandler
    {
        public static Menu mainMenu, Combo, Dead, Harass, AutoHarass, Killsteal, LaneClear, JungleClear, LastHit, Flee, Settings, RLogic;

        public static void Initialize()
        {
            #region CreateMenus
            mainMenu = MainMenu.AddMenu("Karthus", "Karthus");
            Combo = AddSubMenu(mainMenu, "Combo");
            Harass = AddSubMenu(mainMenu, "Harass");
            AutoHarass = AddSubMenu(mainMenu, "Auto Harass");
            Killsteal = AddSubMenu(mainMenu, "Killsteal");
            LaneClear = AddSubMenu(mainMenu, "Lane Clear");
            JungleClear = AddSubMenu(mainMenu, "Jungle Clear");
            LastHit = AddSubMenu(mainMenu, "Last Hit");
            Flee = AddSubMenu(mainMenu, "Flee");
            RLogic = AddSubMenu(mainMenu, "R Logic");
            Dead = AddSubMenu(mainMenu, "Dead");
            Settings = AddSubMenu(mainMenu, "Settings");
            #endregion
            
            #region Set Menu Values
            AddCheckboxes(ref Combo, "Q", "W", "E", "R", "Ignite");
            AddSlider(Combo, "Q Mana %", 0, 0, 100);
            AddSlider(Combo, "W Mana %", 0, 0, 100);
            AddSlider(Combo, "E Mana %", 0, 0, 100);
            AddSlider(Combo, "Enemies to E", 1, 1, 5);
            AddCheckboxes(ref Dead, "Attack when Dead", "Attack minions if no enemies", "Q", "W", "R");
            AddCheckboxes(ref Harass, "Q", "W_false", "E_false");
            AddSlider(Harass, "Q Mana %", 25, 0, 100);
            AddSlider(Harass, "W Mana %", 50, 0, 100);
            AddSlider(Harass, "E Mana %", 50, 0, 100);
            AddSlider(Harass, "Enemies to E", 1, 1, 5);
            AddCheckboxes(ref AutoHarass, "Q", "W_false", "E_false");
            AddSlider(AutoHarass, "Q Mana %", 80, 0, 100);
            AddSlider(AutoHarass, "W Mana %", 80, 0, 100);
            AddSlider(AutoHarass, "E Mana %", 80, 0, 100);
            AddSlider(AutoHarass, "Enemies to E", 1, 1, 5);
            AddCheckboxes(ref Killsteal, "Killsteal", "Q", "E", "R", "Ignite");
            AddCheckboxes(ref LaneClear, "Q", "Q only for Last Hit", "E_false");
            AddSlider(LaneClear, "Q Mana %", 50, 0, 100);
            AddSlider(LaneClear, "E Mana %", 50, 0, 100);
            AddSlider(LaneClear, "Minions to hit with Q", 4, 0, 8);
            AddSlider(LaneClear, "Enemies to E", 4, 1, 8);
            AddCheckboxes(ref JungleClear, "Q", "E");
            AddSlider(JungleClear, "Q Mana %", 10, 0, 100);
            AddSlider(JungleClear, "E Mana %", 10, 0, 100);
            AddSlider(JungleClear, "Enemies to E", 4, 1, 8);
            AddCheckboxes(ref LastHit, "Q");
            AddSlider(LastHit, "Q Mana %", 80, 0, 100);
            AddSlider(LastHit, "Minions to hit with Q", 4, 0, 8);
            AddCheckboxes(ref Flee, "W");
            AddCheckboxes(ref Settings, "Auto-Seraphs", "Tear Stacker", "Skill Leveler", "Draw Spell Ranges", "Draw Name if Killable with R", "Perma E_false");
            var slider = AddSlider(Settings, "Q Prediction:", 2, 0, 2);
            slider.OnValueChange += MenuHandler_OnValueChange;
            SetDisplayName(slider, slider.CurrentValue);

            AddCheckboxes(ref RLogic, "Ult only when Dead_false");
            AddSlider(RLogic, "X Enemies Killed to R", 2, 1, 5);
            AddSlider(RLogic, "% Enemeis Killed to R", 50, 1, 100);
            #endregion
        }

        private static void MenuHandler_OnValueChange(ValueBase<int> sender, ValueBase<int>.ValueChangeArgs args)
        {
            HitChance chance = HitChance.Unknown;

            SetDisplayName(sender as Slider, args.NewValue);

            if (args.NewValue == 0)
                chance = HitChance.Low;
            if (args.NewValue == 1)
                chance = HitChance.Medium;
            if (args.NewValue == 2)
                chance = HitChance.High;

            if (chance != HitChance.Unknown)
                Program.Q.MinimumHitChance = chance;
        }

        public static void SetDisplayName(Slider s, int value)
        {
            s.DisplayName = "Q Prediction: ";

            if (value == 0)
                s.DisplayName += "Low";
            else if (value == 1)
                s.DisplayName += "Medium";
            else if (value == 2)
                s.DisplayName += "High";
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
}
