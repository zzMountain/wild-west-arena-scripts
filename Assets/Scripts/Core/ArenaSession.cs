using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WildWest
{
    public class ArenaSession : MonoBehaviour
    {
        [SerializeField] private Health _playerHealth;
        [SerializeField] private Player _player;
        [SerializeField] private ThirdPersonCamera _thirdPersonCamera;
        [SerializeField] private WaveDirector _waveDirector;
        [SerializeField, Min(0f)] private float _deathRestartDelay = 2.2f;
        [SerializeField, Min(0f)] private float _victoryDelay = 2.2f;

        private Coroutine _reloadRoutine;
        private Coroutine _victoryRoutine;
        private WaitForSecondsRealtime _deathWait;
        private WaitForSecondsRealtime _victoryWait;
        private bool _isEnding;

        public event Action VictoryReached;

        public bool HasWon { get; private set; }

        private void Awake()
        {
            if (_playerHealth == null
                || _player == null
                || _thirdPersonCamera == null
                || _waveDirector == null)
            {
                throw new InvalidOperationException("ArenaSession requires all player and wave dependencies.");
            }

            Time.timeScale = 1f;
            _deathWait = new WaitForSecondsRealtime(_deathRestartDelay);
            _victoryWait = new WaitForSecondsRealtime(_victoryDelay);
        }

        private void OnEnable()
        {
            _playerHealth.Died += OnPlayerDied;
            _waveDirector.AllWavesCompleted += OnAllWavesCompleted;
        }

        private void OnDisable()
        {
            _playerHealth.Died -= OnPlayerDied;
            _waveDirector.AllWavesCompleted -= OnAllWavesCompleted;

            if (_reloadRoutine != null)
            {
                StopCoroutine(_reloadRoutine);
                _reloadRoutine = null;
            }

            if (_victoryRoutine != null)
            {
                StopCoroutine(_victoryRoutine);
                _victoryRoutine = null;
            }

            Time.timeScale = 1f;
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        private void OnPlayerDied()
        {
            if (_isEnding)
                return;

            _isEnding = true;
            _player.DisableControl();
            _waveDirector.Stop();
            _reloadRoutine = StartCoroutine(ReloadAfterDeath());
        }

        private void OnAllWavesCompleted()
        {
            if (_isEnding)
                return;

            _isEnding = true;
            _player.DisableControl();
            _thirdPersonCamera.ReleaseCursor();
            _victoryRoutine = StartCoroutine(ShowVictoryAfterDelay());
        }

        private IEnumerator ReloadAfterDeath()
        {
            yield return _deathWait;
            RestartScene();
        }

        private IEnumerator ShowVictoryAfterDelay()
        {
            yield return _victoryWait;
            HasWon = true;
            Time.timeScale = 0f;
            VictoryReached?.Invoke();
            _victoryRoutine = null;
        }
    }
}
