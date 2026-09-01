using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WildWest
{
    public class GunFeedbackView : MonoBehaviour
    {
        [SerializeField] private Firearm _firearm;
        [SerializeField] private ThirdPersonCamera _camera;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private GameObject _muzzleLightEffect;
        [SerializeField] private ParticleSystem _muzzleEffect;
        [SerializeField] private GameObject _tracerEffect;
        [SerializeField] private LineRenderer _tracer;
        [SerializeField] private AudioSource _shotAudioSource;
        [SerializeField] private AudioClip _shotClip;
        [SerializeField] private ParticleSystem _impactEffect;
        [SerializeField, Min(0.01f)] private float _flashDuration = 0.055f;
        [SerializeField, Min(0.01f)] private float _tracerDuration = 0.085f;

        private readonly Dictionary<GameObject, Coroutine> _hideRoutines = new Dictionary<GameObject, Coroutine>();

        private void Awake()
        {
            if (_firearm == null
                || _camera == null
                || _muzzle == null
                || _muzzleLightEffect == null
                || _muzzleEffect == null
                || _tracerEffect == null
                || _tracer == null
                || _shotAudioSource == null
                || _shotClip == null
                || _impactEffect == null)
            {
                throw new InvalidOperationException("GunFeedbackView requires all feedback dependencies.");
            }

            _muzzleLightEffect.SetActive(false);
            _tracerEffect.SetActive(false);
        }

        private void OnEnable()
        {
            _firearm.ShotResolved += OnShotResolved;
        }

        private void OnDisable()
        {
            _firearm.ShotResolved -= OnShotResolved;

            foreach (Coroutine routine in _hideRoutines.Values)
                StopCoroutine(routine);

            _hideRoutines.Clear();
            _muzzleLightEffect.SetActive(false);
            _tracerEffect.SetActive(false);
        }

        private void OnShotResolved(ShotResult result)
        {
            _camera.AddShotImpulse();
            _shotAudioSource.pitch = UnityEngine.Random.Range(0.96f, 1.04f);
            _shotAudioSource.PlayOneShot(_shotClip, 0.78f);
            _muzzleEffect.Play(true);
            _tracer.SetPosition(0, _muzzle.position);
            _tracer.SetPosition(1, result.Point);
            ShowTransientEffect(_muzzleLightEffect, _flashDuration);
            ShowTransientEffect(_tracerEffect, _tracerDuration);

            if (result.HitSurface)
            {
                _impactEffect.transform.SetPositionAndRotation(
                    result.Point + result.Normal * 0.015f,
                    Quaternion.LookRotation(result.Normal));
                _impactEffect.Play(true);
            }
        }

        private void ShowTransientEffect(GameObject effect, float duration)
        {
            if (_hideRoutines.TryGetValue(effect, out Coroutine routine))
                StopCoroutine(routine);

            effect.SetActive(true);
            _hideRoutines[effect] = StartCoroutine(DisableAfterDelay(effect, duration));
        }

        private IEnumerator DisableAfterDelay(GameObject effect, float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            effect.SetActive(false);
            _hideRoutines.Remove(effect);
        }
    }
}
