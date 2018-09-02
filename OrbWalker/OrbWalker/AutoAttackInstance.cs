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
    class AutoAttackInstance
    {
        public Obj_AI_Base _attacker, _target;
        public float _starttime, _delay, _missileSpeed, _damage;
        public MissileClient _missile;
        public bool finishedProcessing = false;
        
        public AutoAttackInstance(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            _attacker = sender;
            _target = args.Target as Obj_AI_Base;
            _starttime = args.Time;
            _delay = sender.AttackCastDelay;
            _missileSpeed = args.SData.MissileSpeed;
            _damage = _attacker.GetAutoAttackDamage(_target);
        }

        public void AssignMissile(MissileClient missile)
        {
            _missile = missile;
        }

        public float TimeAttackWillHit
        {
            get {
                if (_attacker.IsMelee)
                    return _starttime + _delay;

                // Distance / speed = time
                float distance = _missile != null ? + _missile.Distance(_target) : _attacker.Distance(_target);
                float time = distance / (_missile != null ? _missile.SData.MissileSpeed : _missileSpeed);

                return _starttime + _delay + time;
            }
        }
    }
}