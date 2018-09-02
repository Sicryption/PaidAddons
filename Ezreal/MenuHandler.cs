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

namespace Ezreal
{
    class MenuHandler
    {
        public static Menu mainMenu, Combo, Harass, AutoHarass, Killsteal, LaneClear, JungleClear, LastHit, Flee, Items, Settings;

        public static void Initialize()
        {
            #region CreateMenus
            mainMenu = MainMenu.AddMenu("Ezreal", "Ezreal");
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

            #region Set Menu Values
            AddCheckboxes(ref Combo, "Q", "W", "W Ally", "W Ally under Tower", "W Ally in Fight", "R", "Items", "Ignite_false");
            AddSlider(Combo, "Q Mana %", 0, 0, 100);
            AddSlider(Combo, "W Mana %", 0, 0, 100);
            AddCheckboxes(ref Harass, "Q", "W", "W Ally_false", "W Ally under Tower_false", "W Ally in Fight_false", "R_false", "Items_false", "Ignite_false");
            AddSlider(Harass, "Q Mana %", 50, 0, 100);
            AddSlider(Harass, "W Mana %", 80, 0, 100);
            AddCheckboxes(ref AutoHarass, "Q", "W_false", "W Ally_false", "W Ally under Tower_false", "W Ally in Fight_false");
            AddSlider(AutoHarass, "Q Mana %", 80, 0, 100);
            AddSlider(AutoHarass, "W Mana %", 90, 0, 100);
            AddCheckboxes(ref Killsteal, "Killsteal", "Q", "W", "E_false", "R", "Ignite", "Items");
            AddCheckboxes(ref LaneClear, "Q", "Q only for Last Hit");
            AddSlider(LaneClear, "Q Mana %", 80, 0, 100);
            AddCheckboxes(ref JungleClear, "Q");
            AddSlider(JungleClear, "Q Mana %", 50, 0, 100);
            AddCheckboxes(ref LastHit, "Q");
            AddSlider(LastHit, "Q Mana %", 80, 0, 100);
            AddCheckboxes(ref Flee, "E to Cursor");
            AddCheckboxes(ref Items, "QSS", "Merc Scim", "BOTRK", "Bilgewater Cutlass", "Hextech Gunblade");
            AddCheckboxes(ref Settings, "E on Gapclose", "Tear Stacking", "Skill Leveler", "Draw Q_false", "Draw W_false", "Draw E_false");
            AddSlider(Settings, "Tear Stacking Q Mana %", 60, 0, 100);
            AddSlider(Settings, "Tear Stacking W Mana %", 90, 0, 100);
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
