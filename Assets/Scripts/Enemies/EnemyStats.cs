using System;
using UnityEngine;

namespace WildWest
{
    [Serializable]
    public class EnemyStats
    {
        [SerializeField, Min(1)] private int _health = 50;
        [SerializeField, Min(0f)] private float _moveSpeed = 3.5f;
        [SerializeField, Min(1)] private int _attackDamage = 10;
        [SerializeField, Min(0.1f)] private float _attackRange = 1.6f;
        [SerializeField, Min(0f)] private float _attackCooldown = 1.1f;
        [SerializeField, Min(0f)] private float _impactDelay = 0.32f;

        public int Health => _health;
        public float MoveSpeed => _moveSpeed;
        public int AttackDamage => _attackDamage;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public float ImpactDelay => _impactDelay;
    }
}
