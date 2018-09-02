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

namespace Katarina
{
    class MenuHandler
    {
        public static Menu mainMenu, Combo, Harass, AutoHarass, Killsteal, LaneClear, JungleClear, LastHit, Flee, Settings, RLogic;

        public static void Initialize()
        {
            #region CreateMenus
            mainMenu = MainMenu.AddMenu("Katarina", "Katarina");
            Combo = AddSubMenu(mainMenu, "Combo");
            Harass = AddSubMenu(mainMenu, "Harass");
            AutoHarass = AddSubMenu(mainMenu, "Auto Harass");
            Killsteal = AddSubMenu(mainMenu, "Killsteal");
            LaneClear = AddSubMenu(mainMenu, "Lane Clear");
            JungleClear = AddSubMenu(mainMenu, "Jungle Clear");
            LastHit = AddSubMenu(mainMenu, "Last Hit");
            Flee = AddSubMenu(mainMenu, "Flee");
            RLogic = AddSubMenu(mainMenu, "R Logic");
            Settings = AddSubMenu(mainMenu, "Settings");
            #endregion

            #region Set Menu Values
            AddCheckboxes(ref Combo, "Q", "W", "E", "E to gapclose", "R", "Ignite", "Items");
            AddCheckboxes(ref Harass, "Q", "W", "E");
            AddCheckboxes(ref AutoHarass, "Q", "W_false", "E_false");
            AddCheckboxes(ref Killsteal, "Killsteal", "Q", "E", "R", "Ignite", "Items");
            AddCheckboxes(ref LaneClear, "Q", "W", "E");
            AddSlider(LaneClear, "Minions to hit with Q", 3, 1, 3);
            AddCheckboxes(ref JungleClear, "Q", "W", "E");
            AddCheckboxes(ref LastHit, "Q", "E");
            AddSlider(LastHit, "Minions to hit with Q", 3, 1, 3);
            AddCheckboxes(ref Flee, "W", "E");
            AddCheckboxes(ref Settings, "Skill Leveler", "Draw Spell Ranges", "Draw Jump Spots");
            AddSlider(RLogic, "X Enemies Hit to R", 1, 1, 5);
            AddSlider(RLogic, "% enemies in range to R", 50, 1, 100);
            AddSlider(RLogic, "Range to Ulti", 400, 100, 550);
            #endregion
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
