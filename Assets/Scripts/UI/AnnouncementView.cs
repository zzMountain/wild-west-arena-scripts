using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WildWest
{
    public class AnnouncementView : MonoBehaviour
    {
        [SerializeField] private WaveDirector _waveDirector;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _label;
        [SerializeField, Min(0f)] private float _duration = 1.5f;

        private Coroutine _displayRoutine;
        private WaitForSecondsRealtime _displayWait;

        private void Awake()
        {
            _displayWait = new WaitForSecondsRealtime(_duration);
            _panel.SetActive(false);
        }

        private void OnEnable()
        {
            _waveDirector.WaveChanged += OnWaveChanged;
            _waveDirector.BossPhaseStarted += OnBossPhaseStarted;
            _waveDirector.AllWavesCompleted += OnAllWavesCompleted;
        }

        private void OnDisable()
        {
            _waveDirector.WaveChanged -= OnWaveChanged;
            _waveDirector.BossPhaseStarted -= OnBossPhaseStarted;
            _waveDirector.AllWavesCompleted -= OnAllWavesCompleted;

            if (_displayRoutine != null)
            {
                StopCoroutine(_displayRoutine);
                _displayRoutine = null;
            }

            _panel.SetActive(false);
        }

        private void OnWaveChanged(int currentWave, int totalWaves)
        {
            Show($"WAVE {currentWave} / {totalWaves}");
        }

        private void OnBossPhaseStarted()
        {
            Show("BOSS INCOMING");
        }

        private void OnAllWavesCompleted()
        {
            if (_displayRoutine != null)
            {
                StopCoroutine(_displayRoutine);
                _displayRoutine = null;
            }

            _panel.SetActive(false);
        }

        private void Show(string text)
        {
            if (_displayRoutine != null)
                StopCoroutine(_displayRoutine);

            _label.text = text;
            _panel.SetActive(true);
            _displayRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return _displayWait;
            _panel.SetActive(false);
            _displayRoutine = null;
        }
    }
}
