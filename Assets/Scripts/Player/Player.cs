using UnityEngine;

namespace WildWest
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField] private PlayerMover _mover;
        [SerializeField] private PlayerCombat _combat;
        [SerializeField] private Transform _cameraTransform;

        private void OnEnable()
        {
            _inputReader.AttackRequested += OnAttackRequested;
            _inputReader.WeaponToggleRequested += OnWeaponToggleRequested;
        }

        private void OnDisable()
        {
            _inputReader.AttackRequested -= OnAttackRequested;
            _inputReader.WeaponToggleRequested -= OnWeaponToggleRequested;
        }

        private void Update()
        {
            _mover.Move(_inputReader.Movement, _cameraTransform);
        }

        public void EnableControl()
        {
            _inputReader.enabled = true;
        }

        public void DisableControl()
        {
            _inputReader.enabled = false;
        }

        private void OnAttackRequested()
        {
            _combat.Attack();
        }

        private void OnWeaponToggleRequested()
        {
            _combat.SwitchWeapon();
        }
    }
}
