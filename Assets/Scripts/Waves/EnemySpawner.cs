using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WildWest
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Enemy[] _regularPrefabs;
        [SerializeField] private Enemy[] _bossPrefabs;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private Transform _enemyParent;

        private readonly HashSet<Enemy> _activeEnemies = new HashSet<Enemy>();
        private Coroutine _spawnRoutine;
        private bool _isSpawningRegulars;
        private bool _isShutDown;
        private int _nextSpawnPointIndex;

        public event Action<int> EnemiesRemainingChanged;
        public event Action RegularGroupDefeated;
        public event Action<Enemy> BossSpawned;
        public event Action BossDefeated;

        public int ActiveEnemyCount => _activeEnemies.Count;
        public Enemy CurrentBoss { get; private set; }

        private void Awake()
        {
            ValidateConfiguration();
        }

        private void OnDisable()
        {
            Shutdown();
        }

        public void SpawnRegulars(
            int waveIndex,
            WaveDefinition definition,
            Health targetHealth,
            Transform target)
        {
            EnsureOperational();
            StopSpawning();
            _isSpawningRegulars = true;
            _spawnRoutine = StartCoroutine(SpawnRegularsRoutine(waveIndex, definition, targetHealth, target));
        }

        public void SpawnBoss(int waveIndex, EnemyStats stats, Health targetHealth, Transform target)
        {
            EnsureOperational();
            Enemy prefab = _bossPrefabs[waveIndex % _bossPrefabs.Length];
            Enemy boss = Spawn(prefab, stats, targetHealth, target, true);
            CurrentBoss = boss;
            BossSpawned?.Invoke(boss);
        }

        public void StopSpawning()
        {
            if (_spawnRoutine != null)
            {
                StopCoroutine(_spawnRoutine);
                _spawnRoutine = null;
            }

            _isSpawningRegulars = false;
        }

        public void Shutdown()
        {
            if (_isShutDown)
                return;

            _isShutDown = true;
            StopSpawning();

            foreach (Enemy enemy in _activeEnemies)
            {
                if (enemy == null)
                    continue;

                enemy.Died -= OnEnemyDied;
                Destroy(enemy.gameObject);
            }

            _activeEnemies.Clear();
            CurrentBoss = null;
            EnemiesRemainingChanged?.Invoke(0);
        }

        private IEnumerator SpawnRegularsRoutine(
            int waveIndex,
            WaveDefinition definition,
            Health targetHealth,
            Transform target)
        {
            WaitForSeconds spawnWait = new WaitForSeconds(definition.SpawnInterval);

            for (int i = 0; i < definition.EnemyCount; i++)
            {
                Enemy prefab = _regularPrefabs[(waveIndex + i) % _regularPrefabs.Length];
                Spawn(prefab, definition.RegularEnemy, targetHealth, target, false);

                if (i < definition.EnemyCount - 1)
                    yield return spawnWait;
            }

            _spawnRoutine = null;
            _isSpawningRegulars = false;

            if (_activeEnemies.Count == 0)
                RegularGroupDefeated?.Invoke();
        }

        private Enemy Spawn(
            Enemy prefab,
            EnemyStats stats,
            Health targetHealth,
            Transform target,
            bool isBoss)
        {
            Transform spawnPoint = _spawnPoints[_nextSpawnPointIndex % _spawnPoints.Length];
            _nextSpawnPointIndex++;
            Enemy enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, _enemyParent);
            enemy.Initialize(targetHealth, target, stats, isBoss);
            enemy.Died += OnEnemyDied;
            _activeEnemies.Add(enemy);
            EnemiesRemainingChanged?.Invoke(_activeEnemies.Count);
            return enemy;
        }

        private void OnEnemyDied(Enemy enemy)
        {
            if (_activeEnemies.Remove(enemy) == false)
                return;

            enemy.Died -= OnEnemyDied;
            EnemiesRemainingChanged?.Invoke(_activeEnemies.Count);

            if (enemy.IsBoss)
            {
                CurrentBoss = null;
                BossDefeated?.Invoke();
            }
            else if (_isSpawningRegulars == false && _activeEnemies.Count == 0)
            {
                RegularGroupDefeated?.Invoke();
            }
        }

        private void ValidateConfiguration()
        {
            ValidatePrefabs(_regularPrefabs, "regular");
            ValidatePrefabs(_bossPrefabs, "boss");

            if (_spawnPoints == null || _spawnPoints.Length == 0)
                throw new InvalidOperationException("EnemySpawner requires at least one spawn point.");

            if (_enemyParent == null)
                throw new InvalidOperationException("EnemySpawner requires an enemy parent.");

            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                if (_spawnPoints[i] == null)
                    throw new InvalidOperationException($"EnemySpawner spawn point {i} is missing.");
            }
        }

        private void ValidatePrefabs(Enemy[] prefabs, string label)
        {
            if (prefabs == null || prefabs.Length == 0)
                throw new InvalidOperationException($"EnemySpawner requires at least one {label} prefab.");

            for (int i = 0; i < prefabs.Length; i++)
            {
                if (prefabs[i] == null)
                    throw new InvalidOperationException($"EnemySpawner {label} prefab {i} is missing.");
            }
        }

        private void EnsureOperational()
        {
            if (_isShutDown)
                throw new InvalidOperationException("EnemySpawner cannot spawn after shutdown.");
        }
    }
}
