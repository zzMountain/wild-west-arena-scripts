using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WildWest
{
    public class WeaponHudView : MonoBehaviour
    {
        [SerializeField] private PlayerCombat _combat;
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private Firearm _firearm;
        [SerializeField] private Text _label;
        [SerializeField] private Image _reticle;

        private readonly Color _defaultReticleColor = new Color(0.95f, 0.86f, 0.67f, 0.92f);
        private readonly Color _hitReticleColor = new Color(1f, 0.67f, 0.18f, 1f);
        private readonly Color _killReticleColor = new Color(0.92f, 0.22f, 0.12f, 1f);

        private Coroutine _reticleRoutine;

        private void Awake()
        {
            if (_combat == null
                || _inputReader == null
                || _firearm == null
                || _label == null
                || _reticle == null)
            {
                throw new InvalidOperationException("WeaponHudView requires all HUD dependencies.");
            }

            _reticle.color = _defaultReticleColor;
        }

        private void OnEnable()
        {
            _combat.WeaponChanged += OnWeaponChanged;
            _firearm.ShotResolved += OnShotResolved;
            OnWeaponChanged(_combat.CurrentWeapon);
        }

        private void OnDisable()
        {
            _combat.WeaponChanged -= OnWeaponChanged;
            _firearm.ShotResolved -= OnShotResolved;

            if (_reticleRoutine != null)
            {
                StopCoroutine(_reticleRoutine);
                _reticleRoutine = null;
            }

            _reticle.rectTransform.localScale = Vector3.one;
            _reticle.color = _defaultReticleColor;
        }

        private void OnWeaponChanged(WeaponKind weapon)
        {
            string binding = _inputReader.WeaponToggleBinding.ToUpperInvariant();
            _label.text = weapon == WeaponKind.Firearm
                ? $"REVOLVER     [{binding}] SWITCH"
                : $"KNIFE     [{binding}] SWITCH";
            _reticle.enabled = weapon == WeaponKind.Firearm;
        }

        private void OnShotResolved(ShotResult result)
        {
            if (_reticleRoutine != null)
                StopCoroutine(_reticleRoutine);

            Color feedbackColor = _defaultReticleColor;

            if (result.HitDamageable)
                feedbackColor = result.KilledTarget ? _killReticleColor : _hitReticleColor;

            _reticleRoutine = StartCoroutine(AnimateReticle(feedbackColor));
        }

        private IEnumerator AnimateReticle(Color feedbackColor)
        {
            const float duration = 0.18f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float pulse = Mathf.Sin(progress * Mathf.PI);
                _reticle.rectTransform.localScale = Vector3.one * (1f + pulse * 0.42f);
                _reticle.color = Color.Lerp(feedbackColor, _defaultReticleColor, progress);
                yield return null;
            }

            _reticle.rectTransform.localScale = Vector3.one;
            _reticle.color = _defaultReticleColor;
            _reticleRoutine = null;
        }
    }
}
