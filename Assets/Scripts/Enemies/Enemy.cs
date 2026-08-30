using System;
using UnityEngine;

namespace WildWest
{
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    [RequireComponent(typeof(EnemyMover), typeof(EnemyAttack))]
    public class Enemy : MonoBehaviour
    {
        private Health _health;
        private EnemyMover _mover;
        private EnemyAttack _attack;
        private CharacterController _characterController;
        private Health _targetHealth;
        private Transform _target;

        public event Action<Enemy> Died;

        public Health Health => _health;
        public bool IsBoss { get; private set; }

        private void Awake()
        {
            _health = GetComponent<Health>();
            _mover = GetComponent<EnemyMover>();
            _attack = GetComponent<EnemyAttack>();
            _characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.Died -= OnDied;
        }

        private void Update()
        {
            if (_targetHealth == null || _targetHealth.IsAlive == false)
                return;

            Vector3 offset = _target.position - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude <= _attack.Range * _attack.Range)
            {
                _mover.FaceTarget(_target);
                _attack.TryAttack(_targetHealth);
            }
            else
            {
                _mover.MoveTowards(_target);
            }
        }

        public void Initialize(Health targetHealth, Transform target, EnemyStats stats, bool isBoss)
        {
            _targetHealth = targetHealth;
            _target = target;
            IsBoss = isBoss;
            _health.Configure(stats.Health);
            _mover.Initialize(stats.MoveSpeed);
            float impactDelay = isBoss ? 0.55f : 0.32f;
            _attack.Initialize(stats.AttackDamage, stats.AttackRange, stats.AttackCooldown, impactDelay);
        }

        private void OnDied()
        {
            enabled = false;
            _mover.enabled = false;
            _attack.enabled = false;
            _characterController.enabled = false;
            Died?.Invoke(this);
            Destroy(gameObject, 2.35f);
        }
    }
}
