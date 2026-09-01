using System;
using UnityEngine;

namespace WildWest
{
    public class Firearm : MonoBehaviour
    {
        private const int HitBufferSize = 16;

        [SerializeField, Min(1)] private int _damage = 25;
        [SerializeField, Min(0.1f)] private float _range = 60f;
        [SerializeField, Min(0f)] private float _cooldown = 0.3f;
        [SerializeField] private LayerMask _hitMask;
        [SerializeField] private Transform _muzzle;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[HitBufferSize];
        private float _nextAttackTime;

        public event Action<ShotResult> ShotResolved;

        public Transform Muzzle => _muzzle;

        private void Awake()
        {
            if (_hitMask.value == 0)
                throw new InvalidOperationException("Firearm hit mask must be configured.");

            if (_muzzle == null)
                throw new InvalidOperationException("Firearm requires a muzzle.");
        }

        public bool TryAttack(Camera aimCamera)
        {
            if (Time.time < _nextAttackTime)
                return false;

            _nextAttackTime = Time.time + _cooldown;
            Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 origin = _muzzle.position;
            Vector3 aimPoint = ray.GetPoint(_range);

            if (Physics.Raycast(ray, out RaycastHit cameraHit, _range, _hitMask, QueryTriggerInteraction.Ignore))
                aimPoint = cameraHit.point;

            Vector3 shotDirection = aimPoint - origin;
            float shotDistance = Mathf.Min(_range, shotDirection.magnitude);
            shotDirection = shotDistance > Mathf.Epsilon ? shotDirection.normalized : ray.direction;
            Vector3 point = origin + shotDirection * shotDistance;
            Vector3 normal = -shotDirection;
            bool hitSurface = false;
            bool hitDamageable = false;
            bool killedTarget = false;
            int closestHitIndex = FindClosestHit(origin, shotDirection, shotDistance);

            if (closestHitIndex >= 0)
            {
                RaycastHit hit = _hitBuffer[closestHitIndex];
                hitSurface = true;
                point = hit.point;
                normal = hit.normal;
                if (hit.collider.TryGetComponent(out IDamageable damageable))
                {
                    bool wasAlive = damageable.IsAlive;
                    damageable.ApplyDamage(_damage);
                    hitDamageable = true;
                    killedTarget = wasAlive && damageable.IsAlive == false;
                }
            }

            ShotResolved?.Invoke(new ShotResult(
                origin,
                point,
                normal,
                hitSurface,
                hitDamageable,
                killedTarget));
            return true;
        }

        private int FindClosestHit(Vector3 origin, Vector3 direction, float distance)
        {
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                direction,
                _hitBuffer,
                distance,
                _hitMask,
                QueryTriggerInteraction.Ignore);
            int closestIndex = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].distance >= closestDistance)
                    continue;

                closestDistance = _hitBuffer[i].distance;
                closestIndex = i;
            }

            return closestIndex;
        }
    }
}
