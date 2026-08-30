using System;
using UnityEngine;

namespace WildWest
{
    [Serializable]
    public class WaveDefinition
    {
        [SerializeField, Min(1)] private int _enemyCount = 4;
        [SerializeField, Min(0f)] private float _spawnInterval = 0.75f;
        [SerializeField] private EnemyStats _regularEnemy = new EnemyStats();
        [SerializeField] private EnemyStats _boss = new EnemyStats();

        public int EnemyCount => _enemyCount;
        public float SpawnInterval => _spawnInterval;
        public EnemyStats RegularEnemy => _regularEnemy;
        public EnemyStats Boss => _boss;
    }
}
