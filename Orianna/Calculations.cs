using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

internal class Calculations
{
    public static AIHeroClient Orianna => Player.Instance;
    
    private static int lastCheckTick = 0, lastCheckTick2 = 0;
    private static Vector3 ballPosition;
    private static Obj_GeneralParticleEmitter ball;

    public static Vector3 BallPosition
    {
        get
        {
            Vector3 result;
            if (lastCheckTick == Core.GameTickCount)
                result = ballPosition;
            else
            {
                List<string> QStrings = new List<string>
                {
                    "Orianna_Base_Q_Ghost_bind.troy",
                    "Orianna_Base_Q_yomu_ring_green.troy"
                };
                List<Obj_GeneralParticleEmitter> list = 
                    ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(a => QStrings.Any(b => a.Name == b)).ToList();
                if (list.Count > 0 && list.FirstOrDefault() != null)
                    ballPosition = list.FirstOrDefault().Position;
                else if (Orianna.HasBuff("orianaghostself"))
                    ballPosition = Orianna.Position;
                else
                    ballPosition = Vector3.Zero;
                lastCheckTick = Core.GameTickCount;
                result = ballPosition;
            }
            return result;
        }
    }
    public static Obj_GeneralParticleEmitter Ball
    {
        get
        {
            Obj_GeneralParticleEmitter result;
            if (lastCheckTick == Core.GameTickCount)
                result = ball;
            else
            {
                List<string> QStrings = new List<string>
                {
                    "Orianna_Base_Q_Ghost_bind.troy",
                    "Orianna_Base_Q_yomu_ring_green.troy"
                };
                List<Obj_GeneralParticleEmitter> list =
                    ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(a => QStrings.Any(b => a.Name == b)).ToList();
                if (list.Count > 0 && list.FirstOrDefault() != null)
                    ball = list.FirstOrDefault();
                else
                    ball = null;
                lastCheckTick = Core.GameTickCount;
                result = ball;
            }
            return result;
        }
    }
    
    public static float Q(Obj_AI_Base target, int enemiesHit)
    {
        float dmg = (float)(30 + 30 * Program.Q.Level) + 0.5f * Orianna.FlatMagicDamageMod;
        dmg *= 1f - 0.1f * Math.Min(enemiesHit, 4);
        return Orianna.CalculateDamageOnUnit(target, Program.Q.DamageType, dmg);
    }

    public static float W(Obj_AI_Base target)
    {
        float dmg = (float)(25 + 45 * Program.W.Level) + 0.7f * Orianna.FlatMagicDamageMod;
        return Orianna.CalculateDamageOnUnit(target, Program.W.DamageType, dmg);
    }

    public static float E(Obj_AI_Base target)
    {
        float dmg = (float)(30 + 30 * Program.E.Level) + 0.3f * Orianna.FlatMagicDamageMod;
        return Orianna.CalculateDamageOnUnit(target, Program.E.DamageType, dmg);
    }

    public static float ShieldAmount()
    {
        return (float)(40 + 40 * Program.E.Level) + 0.4f * Orianna.FlatMagicDamageMod;
    }

    public static float R(Obj_AI_Base target)
    {
        float dmg = (float)(75 + 75 * Program.R.Level) + 0.7f * Orianna.FlatMagicDamageMod;
        return Orianna.CalculateDamageOnUnit(target, Program.R.DamageType, dmg);
    }

    public static float Ignite(Obj_AI_Base target)
    {
        return ((10 + 4 * Orianna.Level) * 5) - target.HPRegenRate / 2f * 5f;
    }

    public static bool ShouldR(List<Obj_AI_Base> units, bool ks)
    {
        if (ModeManager.hasDoneActionThisTick || BallPosition == Vector3.Zero || !Program.R.IsReady() || !units.All(a => a as AIHeroClient != null))
            return false;
        else
        {
            units = units.Where(a=> a != null && a.IsInRange(ballPosition, Program.R.Range) 
               && Prediction.Position.PredictUnitPosition(a, Program.R.CastDelay).IsInRange(ballPosition, Program.R.Range)).ToList();
            if (ks)
                units = units.Where(a => a.Health - R(a) <= 0).ToList();

            if (units.Count > 0)
            {
                int enemiesToHitWithR = MenuManager.RLogic.GetSliderValue("X Enemies Hit to R");
                int enemiesInRangeToRPercent = MenuManager.RLogic.GetSliderValue("% enemies in range to R");
                List<AIHeroClient> enemyChampions = EntityManager.Heroes.Enemies.Where(a => a != null && a.IsInRange(Orianna, 1320f)).ToList();
                int enemiesInRange = enemyChampions.Count();

                if (units.Count >= enemiesToHitWithR)
                    return true;
                if (units.Count / Math.Max(enemiesInRange, 1) * 100 >= enemiesInRangeToRPercent)
                    return true;
            }
            return false;
        }
    }
}
