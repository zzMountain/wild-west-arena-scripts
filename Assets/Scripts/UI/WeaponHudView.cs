using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WildWest
{
    public class WeaponHudView : MonoBehaviour
    {
        [SerializeField] private PlayerCombat _combat;
        [SerializeField] private Firearm _firearm;
        [SerializeField] private Text _label;
        [SerializeField] private Image _reticle;

        private readonly Color _defaultReticleColor = new Color(0.95f, 0.86f, 0.67f, 0.92f);
        private readonly Color _hitReticleColor = new Color(1f, 0.67f, 0.18f, 1f);
        private readonly Color _killReticleColor = new Color(0.92f, 0.22f, 0.12f, 1f);

        private Coroutine _reticleRoutine;

        private void Awake()
        {
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
            _label.text = weapon == WeaponKind.Firearm
                ? "REVOLVER     [Q] SWITCH"
                : "KNIFE     [Q] SWITCH";
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
