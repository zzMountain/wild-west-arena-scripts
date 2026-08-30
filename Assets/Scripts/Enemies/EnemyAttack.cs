using System;
using System.Collections;
using UnityEngine;

namespace WildWest
{
    public class EnemyAttack : MonoBehaviour
    {
        private int _damage;
        private float _range;
        private float _cooldown;
        private float _impactDelay;
        private float _nextAttackTime;
        private Coroutine _attackRoutine;
        private WaitForSeconds _impactWait;

        public event Action Attacked;

        public float Range => _range;

        private void OnDisable()
        {
            if (_attackRoutine == null)
                return;

            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        public void Initialize(int damage, float range, float cooldown, float impactDelay)
        {
            _damage = Mathf.Max(1, damage);
            _range = Mathf.Max(0.1f, range);
            _cooldown = Mathf.Max(0f, cooldown);
            _impactDelay = Mathf.Max(0f, impactDelay);
            _impactWait = new WaitForSeconds(_impactDelay);
            _nextAttackTime = 0f;
        }

        public void TryAttack(Health target)
        {
            if (Time.time < _nextAttackTime || target.IsAlive == false)
                return;

            _nextAttackTime = Time.time + _cooldown;
            Attacked?.Invoke();
            _attackRoutine = StartCoroutine(ApplyDamageAfterWindup(target));
        }

        private IEnumerator ApplyDamageAfterWindup(Health target)
        {
            yield return _impactWait;

            if (target != null && target.IsAlive)
            {
                Vector3 offset = target.transform.position - transform.position;
                offset.y = 0f;

                if (offset.sqrMagnitude <= _range * _range)
                    target.ApplyDamage(_damage);
            }

            _attackRoutine = null;
        }
    }
}
