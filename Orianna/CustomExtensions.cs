using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using SharpDX;

static class CustomExtensions
{
    public static bool MeetsCriteria(this Obj_AI_Base target)
    {
        if (!target.IsDead && target.IsVisible && !target.IsInvulnerable && target.IsTargetable && target.IsHPBarRendered && target.IsInRange(Player.Instance, 2000))
            return true;
        return false;
    }
    public static bool GetCheckboxValue(this Menu self, string text)
    {
        return MenuManager.GetCheckboxValue(self, text);
    }
    public static int GetSliderValue(this Menu self, string text)
    {
        return MenuManager.GetSliderValue(self, text);
    }
    public static string GetComboBoxText(this Menu self, string text)
    {
        return MenuManager.GetComboBoxText(self, text);
    }
    public static List<Obj_AI_Base> ToObj_AI_BaseList(this List<AIHeroClient> list)
    {
        List<Obj_AI_Base> returnList = new List<Obj_AI_Base>();
        foreach (AIHeroClient unit in list.Where(a => a.MeetsCriteria()))
            returnList.Add(unit as Obj_AI_Base);
        return returnList;
    }
    public static List<Obj_AI_Base> ToObj_AI_BaseList(this List<Obj_AI_Minion> list)
    {
        List<Obj_AI_Base> returnList = new List<Obj_AI_Base>();
        foreach (Obj_AI_Minion unit in list.Where(a => a.MeetsCriteria()))
            returnList.Add(unit as Obj_AI_Base);
        return returnList;
    }
    public static float MissingHealth(this AIHeroClient self)
    {
        return self.MaxHealth - self.Health;
    }
    public static float BonusAttackDamage(this AIHeroClient self)
    {
        return self.TotalAttackDamage - self.BaseAttackDamage;
    }
    public static InventorySlot GetItem(this AIHeroClient self, ItemId item)
    {
        return self.InventoryItems.Where(a => a.Id == item).FirstOrDefault();
    }
    public static bool CanCancleCC(this AIHeroClient self)
    {
        return (self.HasBuffOfType(BuffType.Blind)
            || self.HasBuffOfType(BuffType.Charm)
            || self.HasBuffOfType(BuffType.Fear)
            || self.HasBuffOfType(BuffType.Knockback)
            || self.HasBuffOfType(BuffType.Silence)
            || self.HasBuffOfType(BuffType.Snare)
            || self.HasBuffOfType(BuffType.Stun)
            || self.HasBuffOfType(BuffType.Taunt))
            //not being knocked back by dragon
            && !self.HasBuff("moveawaycollision")
            //not standing on raka silence
            && !self.HasBuff("sorakaepacify")
            && !self.HasBuff("plantsatchelknockback");
    }
    public static bool MeetsCriteria(this InventorySlot item)
    {
        if (item != null && item.CanUseItem() && !ModeManager.hasDoneActionThisTick)
            return true;
        return false;
    }
    public static bool IsAutoCanceling(this AIHeroClient self, List<Obj_AI_Base> enemies)
    {
        return Orbwalker.IsAutoAttacking || enemies.Where(a => a.IsInRange(self, self.GetAutoAttackRange())).FirstOrDefault() == null;
    }
    public static bool IsIsolated(this Obj_AI_Base enemy)
    {
        return ObjectManager.Get<Obj_GeneralParticleEmitter>().Any(a => a.Name.Contains("Khazix_Base_Q_Single") && a.IsInRange(enemy, 1));
    }
    public static bool IsWithin(this float startTime, float duration)
    {
        if (Game.Time - startTime < duration)
            return true;
        return false;
    }
    public static bool ContainsAny(this string s, bool CaseSensitive, params string[] text)
    {
        List<string> temp = text.ToList();
        if (!CaseSensitive)
        {
            List<string> NonCaseSensitiveList = new List<string>();
            foreach (string str in temp)
                NonCaseSensitiveList.Add(str.ToLower());
            temp = NonCaseSensitiveList;
        }

        foreach (string str in temp)
            if (s.ToLower().Contains(str.ToLower()))
                return true;
        return false;
    }
}