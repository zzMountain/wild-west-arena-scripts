using System;
using UnityEngine;

namespace WildWest
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int _maxValue = 100;

        public event Action<int, int> Changed;
        public event Action<int> Damaged;
        public event Action Died;

        public int CurrentValue { get; private set; }
        public int MaxValue => _maxValue;
        public bool IsAlive => CurrentValue > 0;

        private void Awake()
        {
            ResetState();
        }

        private void OnValidate()
        {
            _maxValue = Mathf.Max(1, _maxValue);
        }

        public void ApplyDamage(int damage)
        {
            if (damage <= 0 || IsAlive == false)
                return;

            int previousValue = CurrentValue;
            CurrentValue = Mathf.Max(0, CurrentValue - damage);
            Damaged?.Invoke(previousValue - CurrentValue);
            Changed?.Invoke(CurrentValue, _maxValue);

            if (CurrentValue == 0)
                Died?.Invoke();
        }

        public void Configure(int maxValue)
        {
            _maxValue = Mathf.Max(1, maxValue);
            ResetState();
        }

        public void ResetState()
        {
            CurrentValue = _maxValue;
            Changed?.Invoke(CurrentValue, _maxValue);
        }
    }
}
