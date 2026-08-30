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
        private Health _ownerHealth;
        private Coroutine _attackRoutine;
        private WaitForSeconds _impactWait;
        private float _nextAttackTime;

        public event Action Swung;

        public Transform AttackPoint => _attackPoint;

        private void Awake()
        {
            _ownerHealth = GetComponentInParent<Health>();

            if (_ownerHealth == null)
                throw new InvalidOperationException("MeleeWeapon requires an owner Health in its parent hierarchy.");

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
            if (Time.time < _nextAttackTime)
                return false;

            _nextAttackTime = Time.time + _cooldown;
            Swung?.Invoke();
            _attackRoutine = StartCoroutine(ApplyDamageAfterWindup());
            return true;
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
                IDamageable damageable = _hitBuffer[i].GetComponentInParent<IDamageable>();

                if (damageable != null
                    && ReferenceEquals(damageable, _ownerHealth) == false
                    && _damagedTargets.Add(damageable))
                {
                    damageable.ApplyDamage(_damage);
                }
            }

            _attackRoutine = null;
        }
    }
}
