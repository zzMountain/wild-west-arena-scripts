using UnityEngine;
using UnityEngine.UI;

namespace WildWest
{
    public class BossHealthView : MonoBehaviour
    {
        [SerializeField] private EnemySpawner _spawner;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Image _fill;
        [SerializeField] private Text _label;

        private Health _bossHealth;

        private void Awake()
        {
            _panel.SetActive(false);
        }

        private void OnEnable()
        {
            _spawner.BossSpawned += OnBossSpawned;
            _spawner.BossDefeated += OnBossDefeated;

            if (_spawner.CurrentBoss != null)
                OnBossSpawned(_spawner.CurrentBoss);
            else
                _panel.SetActive(false);
        }

        private void OnDisable()
        {
            _spawner.BossSpawned -= OnBossSpawned;
            _spawner.BossDefeated -= OnBossDefeated;
            UnbindHealth();
            _panel.SetActive(false);
        }

        private void OnBossSpawned(Enemy boss)
        {
            UnbindHealth();
            _bossHealth = boss.Health;
            _bossHealth.Changed += OnHealthChanged;
            _panel.SetActive(true);
            Refresh(_bossHealth.CurrentValue, _bossHealth.MaxValue);
        }

        private void OnBossDefeated()
        {
            UnbindHealth();
            _panel.SetActive(false);
        }

        private void OnHealthChanged(int currentValue, int maximumValue)
        {
            Refresh(currentValue, maximumValue);
        }

        private void Refresh(int currentValue, int maximumValue)
        {
            _fill.fillAmount = (float)currentValue / maximumValue;
            _label.text = $"BOSS {currentValue} / {maximumValue}";
        }

        private void UnbindHealth()
        {
            if (_bossHealth == null)
                return;

            _bossHealth.Changed -= OnHealthChanged;
            _bossHealth = null;
        }
    }
}
