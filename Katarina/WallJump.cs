using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using SharpDX;

namespace Katarina
{
    class WallJump
    {
        public Vector3 startPos,
            endPos;
        public Obj_AI_Base
            particle;

        public WallJump(Vector3 start, Vector3 end, Obj_AI_Base pParticle)
        {
            startPos = start;
            endPos = end;
            particle = pParticle;
        }
    }
}
