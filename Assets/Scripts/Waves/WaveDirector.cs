using System;
using System.Collections;
using UnityEngine;

namespace WildWest
{
    public class WaveDirector : MonoBehaviour
    {
        private const int RequiredWaveCount = 3;

        [SerializeField] private EnemySpawner _spawner;
        [SerializeField] private Health _playerHealth;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private WaveDefinition[] _waves;
        [SerializeField, Min(0f)] private float _waveDelay = 1.5f;

        private Coroutine _nextWaveRoutine;
        private WaitForSeconds _waveWait;
        private int _currentWaveIndex = -1;
        private bool _isStopped;

        public event Action<int, int> WaveChanged;
        public event Action BossPhaseStarted;
        public event Action AllWavesCompleted;

        public int CurrentWaveNumber => _currentWaveIndex + 1;
        public int TotalWaveCount => _waves.Length;
        public bool IsBossPhase { get; private set; }
        public bool HasCompleted { get; private set; }

        private void Awake()
        {
            ValidateConfiguration();
            _waveWait = new WaitForSeconds(_waveDelay);
        }

        private void OnEnable()
        {
            if (_isStopped)
                return;

            _spawner.RegularGroupDefeated += OnRegularGroupDefeated;
            _spawner.BossDefeated += OnBossDefeated;
        }

        private void Start()
        {
            if (_isStopped == false)
                _nextWaveRoutine = StartCoroutine(BeginNextWave());
        }

        private void OnDisable()
        {
            _spawner.RegularGroupDefeated -= OnRegularGroupDefeated;
            _spawner.BossDefeated -= OnBossDefeated;

            Stop();
            _spawner.Shutdown();
        }

        public void Stop()
        {
            _isStopped = true;
            _spawner.StopSpawning();

            if (_nextWaveRoutine != null)
            {
                StopCoroutine(_nextWaveRoutine);
                _nextWaveRoutine = null;
            }
        }

        private IEnumerator BeginNextWave()
        {
            yield return _waveWait;

            if (_isStopped)
                yield break;

            _currentWaveIndex++;
            WaveDefinition definition = _waves[_currentWaveIndex];
            IsBossPhase = false;
            WaveChanged?.Invoke(CurrentWaveNumber, TotalWaveCount);
            _spawner.SpawnRegulars(_currentWaveIndex, definition, _playerHealth, _playerTransform);
            _nextWaveRoutine = null;
        }

        private void OnRegularGroupDefeated()
        {
            if (_isStopped)
                return;

            IsBossPhase = true;
            BossPhaseStarted?.Invoke();
            WaveDefinition definition = _waves[_currentWaveIndex];
            _spawner.SpawnBoss(_currentWaveIndex, definition.Boss, _playerHealth, _playerTransform);
        }

        private void OnBossDefeated()
        {
            if (_isStopped)
                return;

            if (_currentWaveIndex >= _waves.Length - 1)
            {
                HasCompleted = true;
                AllWavesCompleted?.Invoke();
            }
            else
            {
                _nextWaveRoutine = StartCoroutine(BeginNextWave());
            }
        }

        private void ValidateConfiguration()
        {
            if (_spawner == null)
                throw new InvalidOperationException("WaveDirector requires an EnemySpawner.");

            if (_playerHealth == null || _playerTransform == null)
                throw new InvalidOperationException("WaveDirector requires the player Health and Transform.");

            if (_waves == null || _waves.Length != RequiredWaveCount)
                throw new InvalidOperationException($"WaveDirector requires exactly {RequiredWaveCount} waves.");

            for (int i = 0; i < _waves.Length; i++)
            {
                if (_waves[i] == null)
                    throw new InvalidOperationException($"WaveDirector wave {i} is missing.");

                if (_waves[i].RegularEnemy == null || _waves[i].Boss == null)
                    throw new InvalidOperationException($"WaveDirector wave {i} stats are missing.");
            }
        }
    }
}
