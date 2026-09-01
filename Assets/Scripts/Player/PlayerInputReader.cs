using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WildWest
{
    public class PlayerInputReader : MonoBehaviour
    {
        private const string PlayerMapName = "Player";
        private const string MoveActionName = "Move";
        private const string LookActionName = "Look";
        private const string AttackActionName = "Attack";
        private const string ToggleWeaponActionName = "ToggleWeapon";

        [SerializeField] private InputActionAsset _actions;

        private InputActionMap _playerMap;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _attackAction;
        private InputAction _toggleWeaponAction;

        public event Action AttackRequested;
        public event Action WeaponToggleRequested;

        public Vector2 Movement { get; private set; }
        public Vector2 Look { get; private set; }
        public string WeaponToggleBinding
        {
            get
            {
                InputAction toggleAction = _toggleWeaponAction
                    ?? _actions.FindActionMap(PlayerMapName, true).FindAction(ToggleWeaponActionName, true);
                string bindingPath = toggleAction.bindings[0].effectivePath;

                return InputControlPath.ToHumanReadableString(
                    bindingPath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
            }
        }

        private void Awake()
        {
            _playerMap = _actions.FindActionMap(PlayerMapName, true);
            _moveAction = _playerMap.FindAction(MoveActionName, true);
            _lookAction = _playerMap.FindAction(LookActionName, true);
            _attackAction = _playerMap.FindAction(AttackActionName, true);
            _toggleWeaponAction = _playerMap.FindAction(ToggleWeaponActionName, true);
        }

        private void OnEnable()
        {
            _moveAction.performed += OnMovementPerformed;
            _moveAction.canceled += OnMovementCanceled;
            _lookAction.performed += OnLookPerformed;
            _lookAction.canceled += OnLookCanceled;
            _attackAction.performed += OnAttackPerformed;
            _toggleWeaponAction.performed += OnWeaponTogglePerformed;
            _playerMap.Enable();
        }

        private void OnDisable()
        {
            _playerMap.Disable();
            _moveAction.performed -= OnMovementPerformed;
            _moveAction.canceled -= OnMovementCanceled;
            _lookAction.performed -= OnLookPerformed;
            _lookAction.canceled -= OnLookCanceled;
            _attackAction.performed -= OnAttackPerformed;
            _toggleWeaponAction.performed -= OnWeaponTogglePerformed;
            Movement = Vector2.zero;
            Look = Vector2.zero;
        }

        private void OnMovementPerformed(InputAction.CallbackContext context)
        {
            Movement = context.ReadValue<Vector2>();
        }

        private void OnMovementCanceled(InputAction.CallbackContext context)
        {
            Movement = Vector2.zero;
        }

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            Look = context.ReadValue<Vector2>();
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            Look = Vector2.zero;
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            AttackRequested?.Invoke();
        }

        private void OnWeaponTogglePerformed(InputAction.CallbackContext context)
        {
            WeaponToggleRequested?.Invoke();
        }
    }
}
