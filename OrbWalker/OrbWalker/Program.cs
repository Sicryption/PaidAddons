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
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        static void Loading_OnLoadingComplete(EventArgs args)
        {
            MenuHandler.Initialize();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Events.Drawing_OnDraw;
            Obj_AI_Base.OnBasicAttack += Events.Obj_AI_Base_OnBasicAttack;
            Obj_AI_Base.OnSpellCast += Events.Obj_AI_Base_OnSpellCast;
            Spellbook.OnStopCast += Events.Spellbook_OnStopCast;
            GameObject.OnCreate += Events.Obj_AI_Base_OnCreate;
            GameObject.OnDelete += Events.Obj_AI_Base_OnDelete;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.Instance.IsDead) return;

            Obj_AI_Base target = Player.Instance.GetEnemyMinionsInRange(Player.Instance.GetAARange())
                .OrderBy(a => a.Health).Where(a => Events.GetBestAutoAttackTime(Player.Instance, a) != 0
                && Events.GetBestAutoAttackTime(Player.Instance, a) <= Game.Time).FirstOrDefault();

            if (target != null)
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
        }
    }
}
