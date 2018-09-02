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

namespace Fiora
{
    class Vital
    {
        public GameObject vital;
        public AIHeroClient enemyAttachedTo;
        public bool isReady = false;
        public Geometry.Polygon.Sector sector;
        public Vector3 direction;
        public float timeOfCreation;
        public float timeTilReady
        {
            get
            {
                return 1.5f - (Game.Time - timeOfCreation);
            }
        }
        public Vector3 center
        {
            get
            {
                return vital.Position.Extend(vital.Position + direction, Program.Q.Range / 4).To3D();
            }
        }

        public Vital(GameObject obj, AIHeroClient hero)
        {
            timeOfCreation = Game.Time;

            vital = obj;
            enemyAttachedTo = hero;

            if (enemyAttachedTo == null)
            {
                Console.WriteLine("error");
                return;
            }

            if (!obj.Name.Contains("Warning"))
                isReady = true;

            List<Vector3> vitalCorners = new List<Vector3>();

            vitalCorners.Add(obj.Position);

            if (vital.Name.Contains("_NE"))
                direction = new Vector3(0, 100, 0);
            else if (vital.Name.Contains("_SE"))
                direction = new Vector3(-100, 0, 0);
            else if (vital.Name.Contains("_NW"))
                direction = new Vector3(100, 0, 0);
            else if (vital.Name.Contains("_SW"))
                direction = new Vector3(0, -100, 0);

            vitalCorners.Add(obj.Position + direction);

            sector = new Geometry.Polygon.Sector(vitalCorners[0], vitalCorners[1], MathUtil.DegreesToRadians(80), Program.Q.Range);
            Console.WriteLine("Create vital: " + vital.Name + "|" + enemyAttachedTo.Name + "|" + direction);
        }
    }
}