using System;
using System.Collections;
using UnityEngine;

namespace WildWest
{
    public class GunFeedbackView : MonoBehaviour
    {
        [SerializeField] private Firearm _firearm;
        [SerializeField] private ThirdPersonCamera _camera;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Light _muzzleLight;
        [SerializeField] private LineRenderer _tracer;
        [SerializeField] private AudioSource _shotAudioSource;
        [SerializeField] private ParticleSystem _impactEffect;
        [SerializeField, Min(0.01f)] private float _flashDuration = 0.055f;
        [SerializeField, Min(0.01f)] private float _tracerDuration = 0.085f;

        private Coroutine _feedbackRoutine;
        private AudioClip _shotClip;

        private void Awake()
        {
            if (_firearm == null
                || _camera == null
                || _muzzle == null
                || _muzzleLight == null
                || _tracer == null
                || _shotAudioSource == null
                || _impactEffect == null)
            {
                throw new InvalidOperationException("GunFeedbackView requires all feedback dependencies.");
            }

            _muzzleLight.enabled = false;
            _tracer.enabled = false;
            _shotClip = CreateShotClip();
        }

        private void OnEnable()
        {
            _firearm.ShotResolved += OnShotResolved;
        }

        private void OnDisable()
        {
            _firearm.ShotResolved -= OnShotResolved;

            if (_feedbackRoutine != null)
            {
                StopCoroutine(_feedbackRoutine);
                _feedbackRoutine = null;
            }

            _muzzleLight.enabled = false;
            _tracer.enabled = false;
        }

        private void OnDestroy()
        {
            if (_shotClip != null)
                Destroy(_shotClip);
        }

        private void OnShotResolved(ShotResult result)
        {
            _camera.AddShotImpulse();
            _shotAudioSource.pitch = UnityEngine.Random.Range(0.96f, 1.04f);
            _shotAudioSource.PlayOneShot(_shotClip, 0.78f);
            _muzzleLight.enabled = true;
            _tracer.SetPosition(0, _muzzle.position);
            _tracer.SetPosition(1, result.Point);
            _tracer.enabled = true;

            if (_feedbackRoutine != null)
                StopCoroutine(_feedbackRoutine);

            _feedbackRoutine = StartCoroutine(HideTransientFeedback());

            if (result.HitSurface)
            {
                _impactEffect.transform.SetPositionAndRotation(
                    result.Point + result.Normal * 0.015f,
                    Quaternion.LookRotation(result.Normal));
                _impactEffect.Play(true);
            }
        }

        private IEnumerator HideTransientFeedback()
        {
            float elapsed = 0f;
            float duration = Mathf.Max(_flashDuration, _tracerDuration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                if (elapsed >= _flashDuration)
                    _muzzleLight.enabled = false;

                if (elapsed >= _tracerDuration)
                    _tracer.enabled = false;

                yield return null;
            }

            _muzzleLight.enabled = false;
            _tracer.enabled = false;
            _feedbackRoutine = null;
        }

        private AudioClip CreateShotClip()
        {
            const int sampleRate = 44100;
            const float duration = 0.22f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            System.Random random = new System.Random(4137);

            for (int i = 0; i < sampleCount; i++)
            {
                float time = (float)i / sampleRate;
                float progress = time / duration;
                float crackEnvelope = Mathf.Exp(-time * 42f);
                float boomEnvelope = Mathf.Exp(-time * 15f);
                float noise = (float)(random.NextDouble() * 2d - 1d);
                float crack = noise * crackEnvelope * 0.72f;
                float boom = Mathf.Sin(time * Mathf.PI * 2f * 92f) * boomEnvelope * 0.55f;
                float snap = Mathf.Sin(time * Mathf.PI * 2f * 1650f) * crackEnvelope * 0.18f;
                samples[i] = Mathf.Clamp((crack + boom + snap) * (1f - progress), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Runtime Revolver Shot", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
