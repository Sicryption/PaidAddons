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

namespace Caitlyn
{
    class MenuHandler
    {
        public static Menu mainMenu, Combo, Harass, AutoHarass, Killsteal, LaneClear, JungleClear, LastHit, Flee, Items, Settings;

        public static void Initialize()
        {
            #region CreateMenus
            mainMenu = MainMenu.AddMenu("Caitlyn", "Caitlyn");
            Combo = AddSubMenu(mainMenu, "Combo");
            Harass = AddSubMenu(mainMenu, "Harass");
            AutoHarass = AddSubMenu(mainMenu, "Auto Harass");
            Killsteal = AddSubMenu(mainMenu, "Killsteal");
            LaneClear = AddSubMenu(mainMenu, "Lane Clear");
            JungleClear = AddSubMenu(mainMenu, "Jungle Clear");
            LastHit = AddSubMenu(mainMenu, "Last Hit");
            Flee = AddSubMenu(mainMenu, "Flee");
            Items = AddSubMenu(mainMenu, "Items");
            Settings = AddSubMenu(mainMenu, "Settings");
            #endregion

            //Use R if no enemies are around me or the enemy, and that unit is outside my aa range
            #region Set Menu Values
            AddCheckboxes(ref Combo, "Q", "W", "Only W CC'd Targets", "E", "R_false", "Items");
            AddSlider(Combo, "Q Mana %", 0, 0, 100);
            AddSlider(Combo, "E Mana %", 0, 0, 100);
            AddSlider(Combo, "Units Hit to use Q", 1, 1, 5);
            AddCheckboxes(ref Harass, "Q", "W", "Only W CC'd Targets", "E", "R_false", "Items_false");
            AddSlider(Harass, "Q Mana %", 70, 0, 100);
            AddSlider(Harass, "E Mana %", 80, 0, 100);
            AddSlider(Harass, "Units Hit to use Q", 1, 1, 5);
            AddCheckboxes(ref AutoHarass, "Q", "AA Trapped Targets");
            AddSlider(AutoHarass, "Q Mana %", 80, 0, 100);
            AddSlider(AutoHarass, "Units Hit to use Q", 1, 1, 5);
            AddCheckboxes(ref Killsteal, "Killsteal", "Q", "E_false", "R", "Ignite", "Items");
            AddCheckboxes(ref LaneClear, "Q");
            AddSlider(LaneClear, "Q Mana %", 80, 0, 100);
            AddSlider(LaneClear, "Units Hit to use Q", 3, 1, 10);
            AddCheckboxes(ref JungleClear, "Q");
            AddSlider(JungleClear, "Q Mana %", 50, 0, 100);
            AddSlider(JungleClear, "Units Hit to use Q", 3, 1, 10);
            AddCheckboxes(ref LastHit, "Q");
            AddSlider(LastHit, "Q Mana %", 80, 0, 100);
            AddSlider(LastHit, "Units Hit to use Q", 3, 1, 10);
            AddCheckboxes(ref Flee, "E to Cursor_false", "E opposite to Cursor");
            AddCheckboxes(ref Items, "QSS", "Merc Scim", "BOTRK", "Bilgewater Cutlass", "Hextech Gunblade");
            AddCheckboxes(ref Settings, "E on Gapclose", "Skill Leveler");
            AddComboBox(Settings, "Q Usage", 0, "Most Units Hit", "Best Hit Chance");
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
