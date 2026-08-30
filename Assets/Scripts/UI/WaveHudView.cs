using UnityEngine;
using UnityEngine.UI;

namespace WildWest
{
    public class WaveHudView : MonoBehaviour
    {
        [SerializeField] private WaveDirector _waveDirector;
        [SerializeField] private EnemySpawner _spawner;
        [SerializeField] private Text _waveLabel;
        [SerializeField] private Text _remainingLabel;

        private void OnEnable()
        {
            _waveDirector.WaveChanged += OnWaveChanged;
            _waveDirector.BossPhaseStarted += OnBossPhaseStarted;
            _spawner.EnemiesRemainingChanged += OnEnemiesRemainingChanged;
            RefreshWave(_waveDirector.CurrentWaveNumber, _waveDirector.TotalWaveCount);
            RefreshRemaining(_spawner.ActiveEnemyCount);
        }

        private void OnDisable()
        {
            _waveDirector.WaveChanged -= OnWaveChanged;
            _waveDirector.BossPhaseStarted -= OnBossPhaseStarted;
            _spawner.EnemiesRemainingChanged -= OnEnemiesRemainingChanged;
        }

        private void OnWaveChanged(int currentWave, int totalWaves)
        {
            RefreshWave(currentWave, totalWaves);
            RefreshRemaining(_spawner.ActiveEnemyCount);
        }

        private void OnBossPhaseStarted()
        {
            RefreshWave(_waveDirector.CurrentWaveNumber, _waveDirector.TotalWaveCount);
            RefreshRemaining(_spawner.ActiveEnemyCount);
        }

        private void OnEnemiesRemainingChanged(int count)
        {
            RefreshRemaining(count);
        }

        private void RefreshWave(int currentWave, int totalWaves)
        {
            string suffix = _waveDirector.IsBossPhase ? "  •  BOSS" : string.Empty;
            _waveLabel.text = currentWave > 0
                ? $"WAVE {currentWave} / {totalWaves}{suffix}"
                : $"WAVE - / {totalWaves}";
        }

        private void RefreshRemaining(int count)
        {
            bool showRemaining = _waveDirector.IsBossPhase == false;
            _remainingLabel.gameObject.SetActive(showRemaining);
            _remainingLabel.text = $"{count} LEFT";
        }
    }
}
