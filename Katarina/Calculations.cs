using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Utils;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Collections.Generic;
using SharpDX;

namespace Katarina
{
    class Calculations
    {
        public static AIHeroClient Katarina => Player.Instance;

        public static float Q(Obj_AI_Base target)
        {
            float qdmg = 35 + (40 * Program.Q.Level)
                + (0.3f * Katarina.FlatMagicDamageMod);

            return Katarina.CalculateDamageOnUnit(target, DamageType.Magical, qdmg);
        }
        public static float Dagger(Obj_AI_Base target)
        {
            int[] flat = new int[] { 0, 75, 80, 87, 94, 102, 111, 120, 131, 143, 155, 168, 183, 198, 214, 231, 248, 267, 287 };
            int[] appercent = new int[] { 0, 55, 55, 55, 55, 55, 70, 70, 70, 70, 70, 85, 85, 85, 85, 85, 100, 100, 100 };
            float damage = flat[Katarina.Level]
                + Katarina.BonusAttackDamage()
                + (appercent[Katarina.Level] / 100) * Katarina.FlatMagicDamageMod;

            return Katarina.CalculateDamageOnUnit(target, DamageType.Magical, damage);
        }
        public static float E(Obj_AI_Base target)
        {
            float damage = 15f + (15f * Program.W.Level)
                + (0.25f * Katarina.FlatMagicDamageMod)
                + (0.5f * Katarina.TotalAttackDamage);

            return Katarina.CalculateDamageOnUnit(target, DamageType.Magical, damage);
        }
        public static float RPerDagger(Obj_AI_Base target, bool fullDuration)
        {
            float damage = 12.5f + (12.5f * Program.W.Level)
                + (0.19f * Katarina.FlatMagicDamageMod)
                + (0.22f * Katarina.BonusAttackDamage());

            damage = (fullDuration)?damage * 15:damage;

            return Katarina.CalculateDamageOnUnit(target, DamageType.Magical, damage);
        }
        public static float R(Obj_AI_Base target)
        {
            return RPerDagger(target, true);
        }
        public static float Ignite(Obj_AI_Base target)
        {
            return ((10 + (4 * Katarina.Level)) * 5) - ((target.HPRegenRate / 2) * 5);
        }
        public static bool ShouldStopUlti(List<Obj_AI_Base> targs)
        {
            if (!Katarina.HasBuff("KatarinaR"))
                return true;

            List<AIHeroClient> targets = targs.Where(a=>(a as AIHeroClient) != null).Cast<AIHeroClient>().ToList();

            if (targets.Where(a => a.IsInRange(Katarina, Program.R.Range)).Count() == 0)
                return true;

            foreach (var targ in targets)
            {
                // qDMG = (Program.Q.IsReady()) ? Q(targ) : 0;
                float EDMG = (Program.E.IsReady()) ? E(targ) : 0;
                float daggerDamage = 0;
                var daggers = NearbyDaggers(targ);
                if (daggers.Count > 0 && Program.E.IsReady())
                    daggerDamage = Dagger(targ);

                if ((/*qDMG*/ + EDMG + daggerDamage) / 2 > targ.Health)
                    return true;
            }

            return false;
        }
        public static List<Obj_AI_Base> NearbyDaggers(Obj_AI_Base targ)
        {
            List<Obj_GeneralParticleEmitter> particles = ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(a => a.IsInRange(targ, 340) && a.Name.Contains("Dagger_Ground")).ToList();
            var daggers = ObjectManager.Get<Obj_AI_Base>().Where(a => a.Name == "HiddenMinion" && particles.Any(b => b.IsInRange(a, 5))).ToList();

            return daggers.ToList();
        }

        public static List<Obj_AI_Base> NearbyDaggersFromParticle(Obj_AI_Base particle)
        {
            var daggers = ObjectManager.Get<Obj_AI_Base>().Where(a => a.Name == "HiddenMinion" && particle.IsInRange(a, 5)).ToList();

            return daggers.ToList();
        }
        public static Vector3 ClosestJump(Vector3 pos)
        {
            List<Obj_GeneralParticleEmitter> particles = ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(a => a.Name.Contains("Dagger_Ground")).ToList();
            var jumpPoses = ObjectManager.Get<Obj_AI_Base>().Where(a => a.Name == "HiddenMinion" && particles.Any(b => b.IsInRange(a, 5))).ToList();

            jumpPoses.AddRange(EntityManager.Heroes.AllHeroes.Where(a => !a.IsMe && a.IsInRange(Katarina, Program.E.Range)).Cast<Obj_AI_Base>());
            jumpPoses.AddRange(EntityManager.MinionsAndMonsters.CombinedAttackable.Cast<Obj_AI_Base>());

            var closestDagger = jumpPoses.Where(a => a.Name == "HiddenMinion").OrderBy(a => a.Position.DistanceSquared(pos)).FirstOrDefault();
            var closestUnit = jumpPoses.OrderBy(a => a.Position.DistanceSquared(pos)).FirstOrDefault();

            return (closestUnit == null) ? Vector3.Zero : (closestDagger == null) ? closestUnit.Position : (closestUnit.Distance(pos) > closestDagger.Distance(pos) + 200) ? closestUnit.Position:closestDagger.Position;
        }
        
        public static Vector3 GapcloseWithE(List<Obj_AI_Base> enemies)
        {
            if (enemies.Any(a => a.IsInRange(Katarina, Program.E.Range)) && Katarina.Level >= 6)
                return Vector3.Zero;

            List<Obj_GeneralParticleEmitter> particles = ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(a => a.Name.Contains("Dagger_Ground")).ToList();
            var jumpPoses = ObjectManager.Get<Obj_AI_Base>().Where(a => a.Name == "HiddenMinion" && particles.Any(b => b.IsInRange(a, 5))).ToList();

            if (jumpPoses.Any(a => enemies.Any(b => b.IsInRange(a, Program.E.Range))))
                return jumpPoses.OrderBy(a => enemies.OrderBy(b => b.Distance(a)).FirstOrDefault().Distance(a)).FirstOrDefault().Position;

            return Vector3.Zero;
        }
    }
}