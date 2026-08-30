using UnityEngine;
using UnityEngine.UI;

namespace WildWest
{
    public class HealthBarView : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private Image _fill;
        [SerializeField] private Text _label;

        private void OnEnable()
        {
            _health.Changed += OnHealthChanged;
            Refresh(_health.CurrentValue, _health.MaxValue);
        }

        private void OnDisable()
        {
            _health.Changed -= OnHealthChanged;
        }

        private void OnHealthChanged(int currentValue, int maximumValue)
        {
            Refresh(currentValue, maximumValue);
        }

        private void Refresh(int currentValue, int maximumValue)
        {
            _fill.fillAmount = (float)currentValue / maximumValue;
            _label.text = $"HP {currentValue} / {maximumValue}";
        }
    }
}
