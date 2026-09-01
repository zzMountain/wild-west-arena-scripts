using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WildWest
{
    public class MeleeWeapon : MonoBehaviour
    {
        private const int HitBufferSize = 16;

        [SerializeField] private Transform _attackPoint;
        [SerializeField, Min(1)] private int _damage = 40;
        [SerializeField, Min(0.1f)] private float _radius = 1.35f;
        [SerializeField, Min(0f)] private float _cooldown = 0.65f;
        [SerializeField, Min(0f)] private float _impactDelay = 0.22f;
        [SerializeField] private LayerMask _targetMask;

        private readonly Collider[] _hitBuffer = new Collider[HitBufferSize];
        private readonly HashSet<IDamageable> _damagedTargets = new HashSet<IDamageable>();
        private Coroutine _attackRoutine;
        private WaitForSeconds _impactWait;
        private float _nextAttackTime;

        public event Action AttackStarted;

        public Transform AttackPoint => _attackPoint;
        public float Range => _radius;

        private void Awake()
        {
            if (_attackPoint == null)
                throw new InvalidOperationException("MeleeWeapon requires an attack point.");

            if (_targetMask.value == 0)
                throw new InvalidOperationException("MeleeWeapon target mask must be configured.");

            _impactWait = new WaitForSeconds(_impactDelay);
        }

        private void OnDisable()
        {
            if (_attackRoutine == null)
                return;

            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        private void OnDrawGizmosSelected()
        {
            if (_attackPoint == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_attackPoint.position, _radius);
        }

        public bool TryAttack()
        {
            if (_attackRoutine != null || Time.time < _nextAttackTime)
                return false;

            _nextAttackTime = Time.time + _cooldown;
            AttackStarted?.Invoke();
            _attackRoutine = StartCoroutine(ApplyDamageAfterWindup());
            return true;
        }

        public void Initialize(int damage, float range, float cooldown, float impactDelay)
        {
            _damage = Mathf.Max(1, damage);
            _radius = Mathf.Max(0.1f, range);
            _cooldown = Mathf.Max(0f, cooldown);
            _impactDelay = Mathf.Max(0f, impactDelay);
            _impactWait = new WaitForSeconds(_impactDelay);
            _nextAttackTime = 0f;
        }

        private IEnumerator ApplyDamageAfterWindup()
        {
            yield return _impactWait;
            _damagedTargets.Clear();
            int hitCount = Physics.OverlapSphereNonAlloc(
                _attackPoint.position,
                _radius,
                _hitBuffer,
                _targetMask,
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].TryGetComponent(out IDamageable damageable)
                    && _damagedTargets.Add(damageable))
                    damageable.ApplyDamage(_damage);
            }

            _attackRoutine = null;
        }
    }
}
