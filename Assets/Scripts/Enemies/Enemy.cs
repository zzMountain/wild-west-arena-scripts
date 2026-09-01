using System;
using UnityEngine;

namespace WildWest
{
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    [RequireComponent(typeof(EnemyMover), typeof(MeleeWeapon))]
    public class Enemy : MonoBehaviour
    {
        private Health _health;
        private EnemyMover _mover;
        private MeleeWeapon _meleeWeapon;
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
            _meleeWeapon = GetComponent<MeleeWeapon>();
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

            if (offset.sqrMagnitude <= _meleeWeapon.Range * _meleeWeapon.Range)
            {
                _mover.FaceTarget(_target);
                _meleeWeapon.TryAttack();
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
            _meleeWeapon.Initialize(
                stats.AttackDamage,
                stats.AttackRange,
                stats.AttackCooldown,
                stats.ImpactDelay);
        }

        private void OnDied()
        {
            enabled = false;
            _mover.enabled = false;
            _meleeWeapon.enabled = false;
            _characterController.enabled = false;
            Died?.Invoke(this);
            Destroy(gameObject, 2.35f);
        }
    }
}
