using UnityEngine;

namespace WildWest
{
    public readonly struct ShotResult
    {
        public ShotResult(
            Vector3 origin,
            Vector3 point,
            Vector3 normal,
            bool hitSurface,
            bool hitDamageable,
            bool killedTarget)
        {
            Origin = origin;
            Point = point;
            Normal = normal;
            HitSurface = hitSurface;
            HitDamageable = hitDamageable;
            KilledTarget = killedTarget;
        }

        public Vector3 Origin { get; }
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        public bool HitSurface { get; }
        public bool HitDamageable { get; }
        public bool KilledTarget { get; }
    }
}
