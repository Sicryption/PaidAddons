using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

namespace OrbWalker
{
    class MenuHandler
    {
        private static Menu mainMenu, Drawing;

        private static Dictionary<string, ValueBase> Menu = new Dictionary<string, ValueBase>();
        
        public static void Initialize()
        {
            // Main menu
            mainMenu = MainMenu.AddMenu("OrbWalker", "M");

            // Drawing menu
            CreateSubMenu(ref Drawing, "Drawing");
            CreateGroupLabel(Drawing, "Drawing Features");
            CreateCheckBox(Drawing, "Draw", "Draw All");
            CreateCheckBox(Drawing, "DrawC", "Draw when close");
            CreateGroupLabel(Drawing, "Champion Features");
            CreateCheckBox(Drawing, "Draw_Player_AA", "Draw Player Auto Range");
            CreateCheckBox(Drawing, "Draw_Allies_AA", "Draw Allies Auto Range", false);
            CreateCheckBox(Drawing, "Draw_Enemies_AA", "Draw Enemies Auto Range");
            CreateGroupLabel(Drawing, "Minions/Creatures/Pets Features");
            CreateCheckBox(Drawing, "Draw_MinionsAlly_AA", "Draw Ally Auto Range", false);
            CreateCheckBox(Drawing, "Draw_MinionsEnemy_AA", "Draw Enemy Auto Range", false);
            CreateCheckBox(Drawing, "Draw_Creatures_AA", "Draw Jungle Creature Auto Range");
            CreateGroupLabel(Drawing, "Turret Features");
            CreateCheckBox(Drawing, "Draw_TurretAlly_AA", "Draw Ally Auto Range");
            CreateCheckBox(Drawing, "Draw_TurretEnemy_AA", "Draw Enemy Auto Range");

            foreach(Menu menu in mainMenu.SubMenus)
                foreach (KeyValuePair<string, ValueBase> prompt in menu.LinkedValues)
                    Menu.Add(prompt.Key, prompt.Value);
        }
        
        private static Menu CreateSubMenu(ref Menu menu, string index)
        {
            return menu = mainMenu.AddSubMenu(index);
        }

        private static void CreateGroupLabel(Menu menu, string text)
        {
            menu.Add(menu.DisplayName + text, new GroupLabel(text));
        }
        
        private static void CreateLabel(Menu menu, string text)
        {
            menu.Add(menu.DisplayName + text, new Label(text));
        }

        private static void CreateCheckBox(Menu menu, string index, string text, bool defbool = true)
        {
            menu.Add(index, new CheckBox(text, defbool));
        }

        private static void CreateSlider(Menu menu, string index, string text, int origin, int min = 0, int max = 100)
        {
            menu.Add(index, new Slider(text, origin, min, max));
        }

        private static void CreateComboBox(Menu menu, string index, string text, List<string> data, int defint = 0)
        {
            menu.Add(index, new ComboBox(text, data, defint));
        }

        private static void CreateSeperator(Menu menu, int h = 25)
        {
            menu.AddSeparator(h);
        }
        
        public static bool GetCheckBoxValue(string index)
        {
            ValueBase checkbox = Menu.Where(a => a.Key == index.ToLower()).FirstOrDefault().Value;
            if (checkbox == null)
            {
                Console.WriteLine("No value named: " + index);
                return false;
            }

            return checkbox.Cast<CheckBox>().CurrentValue;
        }

        public static int GetSliderValue(string index)
        {
            ValueBase slider = Menu.Where(a => a.Key == index.ToLower()).FirstOrDefault().Value;
            if (slider == null)
            {
                Console.WriteLine("No value named: " + index);
                return 0;
            }

            return slider.Cast<Slider>().CurrentValue;
        }

        public static string GetComboBoxValue(string index)
        {
            ValueBase combobox = Menu.Where(a => a.Key == index.ToLower()).FirstOrDefault().Value;
            if (combobox == null)
            {
                Console.WriteLine("No value named: " + index);
                return "null";
            }

            return combobox.Cast<ComboBox>().CurrentValue.ToString();
        }
    }
}