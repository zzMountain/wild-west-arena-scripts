using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WildWest
{
    public class VictoryView : MonoBehaviour
    {
        [SerializeField] private ArenaSession _session;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private CanvasGroup _panelCanvasGroup;
        [SerializeField] private RectTransform _cardTransform;
        [SerializeField] private GameObject[] _gameplayHudObjects;
        [SerializeField, Min(0.01f)] private float _revealDuration = 0.32f;

        private Coroutine _revealRoutine;

        private void Awake()
        {
            if (_session == null
                || _panel == null
                || _restartButton == null
                || _panelCanvasGroup == null
                || _cardTransform == null)
            {
                throw new InvalidOperationException("VictoryView requires all presentation dependencies.");
            }

            _panel.SetActive(false);
        }

        private void OnEnable()
        {
            _session.VictoryReached += OnVictoryReached;
            _restartButton.onClick.AddListener(OnRestartClicked);

            if (_session.HasWon)
                ShowVictory();
        }

        private void OnDisable()
        {
            _session.VictoryReached -= OnVictoryReached;
            _restartButton.onClick.RemoveListener(OnRestartClicked);

            if (_revealRoutine != null)
            {
                StopCoroutine(_revealRoutine);
                _revealRoutine = null;
            }

            _panel.SetActive(false);
        }

        private void OnVictoryReached()
        {
            ShowVictory();
        }

        private void OnRestartClicked()
        {
            _session.RestartScene();
        }

        private void ShowVictory()
        {
            SetGameplayHudVisible(false);
            _panelCanvasGroup.alpha = 0f;
            _cardTransform.localScale = Vector3.one * 0.86f;
            _panel.SetActive(true);

            if (_revealRoutine != null)
                StopCoroutine(_revealRoutine);

            _revealRoutine = StartCoroutine(RevealPanel());
        }

        private IEnumerator RevealPanel()
        {
            float elapsed = 0f;

            while (elapsed < _revealDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / _revealDuration);
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
                _panelCanvasGroup.alpha = easedProgress;
                _cardTransform.localScale = Vector3.one * Mathf.Lerp(0.86f, 1f, easedProgress);
                yield return null;
            }

            _panelCanvasGroup.alpha = 1f;
            _cardTransform.localScale = Vector3.one;
            _revealRoutine = null;
        }

        private void SetGameplayHudVisible(bool isVisible)
        {
            foreach (GameObject hudObject in _gameplayHudObjects)
            {
                if (hudObject != null)
                    hudObject.SetActive(isVisible);
            }
        }
    }
}
