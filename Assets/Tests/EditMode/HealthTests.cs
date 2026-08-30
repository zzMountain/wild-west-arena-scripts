using NUnit.Framework;
using UnityEngine;

namespace WildWest.Tests
{
    public class HealthTests
    {
        private GameObject _gameObject;

        [TearDown]
        public void TearDown()
        {
            if (_gameObject != null)
                Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void ApplyDamage_WhenDamageExceedsHealth_ClampsAtZeroAndDiesOnce()
        {
            _gameObject = new GameObject("Health Test");
            Health health = _gameObject.AddComponent<Health>();
            int deathCount = 0;
            health.Died += () => deathCount++;
            health.Configure(50);

            health.ApplyDamage(100);
            health.ApplyDamage(1);

            Assert.That(health.CurrentValue, Is.EqualTo(0));
            Assert.That(health.IsAlive, Is.False);
            Assert.That(deathCount, Is.EqualTo(1));
        }

        [Test]
        public void Configure_WhenMaximumIsInvalid_UsesOneAsMinimum()
        {
            _gameObject = new GameObject("Health Test");
            Health health = _gameObject.AddComponent<Health>();

            health.Configure(0);

            Assert.That(health.MaxValue, Is.EqualTo(1));
            Assert.That(health.CurrentValue, Is.EqualTo(1));
        }
    }
}
