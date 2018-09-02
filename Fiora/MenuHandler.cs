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

namespace Fiora
{
    class MenuHandler
    {
        public static Menu mainMenu, Combo, Harass, AutoHarass, Killsteal, LaneClear, JungleClear, LastHit, Flee, Items, Settings;

        public static void Initialize()
        {
            #region CreateMenus
            mainMenu = MainMenu.AddMenu("Fiora", "Fiora");
            Combo = AddSubMenu(mainMenu, "Combo");
            Harass = AddSubMenu(mainMenu, "Harass");
            AutoHarass = AddSubMenu(mainMenu, "Auto Harass");
            Killsteal = AddSubMenu(mainMenu, "Killsteal");
            LaneClear = AddSubMenu(mainMenu, "Lane Clear");
            JungleClear = AddSubMenu(mainMenu, "Jungle Clear");
            LastHit = AddSubMenu(mainMenu, "Last Hit");
            Flee = AddSubMenu(mainMenu, "Flee");
            Items = AddSubMenu(mainMenu, "Items");
            //AutoW = AddSubMenu(mainMenu, "Auto-W");
            Settings = AddSubMenu(mainMenu, "Settings");
            #endregion
            
            #region Set Menu Values
            AddCheckboxes(ref Combo, "Q", "W_false", "E", "R", "Items");
            AddSlider(Combo, "Q Mana %", 0, 0, 100);
            AddSlider(Combo, "W Mana %", 0, 0, 100);
            AddSlider(Combo, "E Mana %", 0, 0, 100);
            AddCheckboxes(ref Harass, "Q", "W_false", "E", "Items");
            AddSlider(Harass, "Q Mana %", 50, 0, 100);
            AddSlider(Harass, "W Mana %", 80, 0, 100);
            AddSlider(Harass, "E Mana %", 50, 0, 100);
            AddCheckboxes(ref AutoHarass, "Q", "E", "Items");
            AddSlider(AutoHarass, "Q Mana %", 80, 0, 100);
            AddSlider(AutoHarass, "E Mana %", 80, 0, 100);
            AddCheckboxes(ref Killsteal, "Killsteal", "Q", "W_false", "E", "Items", "Ignite");
            AddCheckboxes(ref LaneClear, "Q", "Q only for Last Hit", "Q with Tiamat", "E", "Items");
            AddSlider(LaneClear, "Q Mana %", 50, 0, 100);
            AddSlider(LaneClear, "E Mana %", 50, 0, 100);
            AddCheckboxes(ref JungleClear, "Q", "E", "Items");
            AddSlider(JungleClear, "Q Mana %", 10, 0, 100);
            AddSlider(JungleClear, "E Mana %", 10, 0, 100);
            AddCheckboxes(ref LastHit, "Q", "E_false", "Items");
            AddSlider(LastHit, "Q Mana %", 80, 0, 100);
            AddSlider(LastHit, "E Mana %", 80, 0, 100);
            AddCheckboxes(ref Flee, "Q to Cursor");
            AddCheckboxes(ref Items, "Use Quick Silver Sash", "Use Mercurials Scimitar", "Use Tiamat", "Use Ravenous Hydra", "Use Titanic Hydra", "Use Youmuus", "Use Bilgewater Cutlass", "Use Hextech Gunblade", "Use Blade of the Ruined King");
            //AddCheckboxes(ref AutoW, "Enable", "Use against CC", "Use against abilities");
            AddCheckboxes(ref Settings, "Only Q to proc Vitals_false", "Skill Leveler", "Draw Vitals_false", "Draw Loading Vitals_false");
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
