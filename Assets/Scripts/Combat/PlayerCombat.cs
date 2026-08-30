using System;
using UnityEngine;

namespace WildWest
{
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private Camera _aimCamera;
        [SerializeField] private Firearm _firearm;
        [SerializeField] private MeleeWeapon _meleeWeapon;

        public event Action<WeaponKind> WeaponChanged;
        public event Action<WeaponKind> AttackStarted;

        public WeaponKind CurrentWeapon { get; private set; }

        private void Awake()
        {
            if (_aimCamera == null || _firearm == null || _meleeWeapon == null)
                throw new InvalidOperationException("PlayerCombat requires camera and weapon dependencies.");

            SelectWeapon(WeaponKind.Firearm);
        }

        public void Attack()
        {
            bool attackStarted = CurrentWeapon == WeaponKind.Firearm
                ? _firearm.TryAttack(_aimCamera)
                : _meleeWeapon.TryAttack();

            if (attackStarted)
                AttackStarted?.Invoke(CurrentWeapon);
        }

        public void SwitchWeapon()
        {
            WeaponKind nextWeapon = CurrentWeapon == WeaponKind.Firearm
                ? WeaponKind.Melee
                : WeaponKind.Firearm;
            SelectWeapon(nextWeapon);
        }

        private void SelectWeapon(WeaponKind weapon)
        {
            CurrentWeapon = weapon;
            WeaponChanged?.Invoke(CurrentWeapon);
        }
    }
}
