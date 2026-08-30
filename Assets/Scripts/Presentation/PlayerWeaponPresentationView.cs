using System;
using UnityEngine;

namespace WildWest
{
    [RequireComponent(typeof(PlayerCombat))]
    public class PlayerWeaponPresentationView : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Camera _aimCamera;
        [SerializeField] private Firearm _firearm;
        [SerializeField] private MeleeWeapon _meleeWeapon;
        [SerializeField] private GameObject _firearmView;
        [SerializeField] private GameObject _meleeView;
        [SerializeField] private Vector3 _firearmGripPosition = Vector3.zero;
        [SerializeField] private Vector3 _firearmGripEuler = Vector3.zero;
        [SerializeField] private Vector3 _meleeGripPosition = Vector3.zero;
        [SerializeField] private Vector3 _meleeGripEuler = Vector3.zero;
        [SerializeField] private Vector3 _meleeHitPoint = new Vector3(0f, 0.38f, 0f);

        private PlayerCombat _combat;
        private Quaternion _firearmGripRotation;
        private Quaternion _meleeGripRotation;
        private float _weaponKick;

        private void Awake()
        {
            _combat = GetComponent<PlayerCombat>();

            if (_animator == null
                || _aimCamera == null
                || _firearm == null
                || _meleeWeapon == null
                || _firearmView == null
                || _meleeView == null)
            {
                throw new InvalidOperationException("PlayerWeaponPresentationView requires all weapon presentation dependencies.");
            }

            _firearmGripRotation = Quaternion.Euler(_firearmGripEuler);
            _meleeGripRotation = Quaternion.Euler(_meleeGripEuler);
        }

        private void OnEnable()
        {
            _combat.WeaponChanged += OnWeaponChanged;
            _firearm.Fired += OnFirearmFired;
        }

        private void Start()
        {
            AttachWeapons();
            ShowWeapon(_combat.CurrentWeapon);
        }

        private void OnDisable()
        {
            _combat.WeaponChanged -= OnWeaponChanged;
            _firearm.Fired -= OnFirearmFired;
        }

        private void LateUpdate()
        {
            Transform firearmTransform = _firearmView.transform;
            firearmTransform.SetLocalPositionAndRotation(_firearmGripPosition, _firearmGripRotation);
            AlignFirearmWithAim(firearmTransform);
            firearmTransform.position -= _aimCamera.transform.forward * (_weaponKick * 0.085f);
            _meleeView.transform.SetLocalPositionAndRotation(_meleeGripPosition, _meleeGripRotation);
            _weaponKick = Mathf.MoveTowards(_weaponKick, 0f, Time.deltaTime * 8f);
        }

        private void OnWeaponChanged(WeaponKind weapon)
        {
            ShowWeapon(weapon);
        }

        private void OnFirearmFired()
        {
            _weaponKick = 1f;
        }

        private void ShowWeapon(WeaponKind weapon)
        {
            _firearmView.SetActive(weapon == WeaponKind.Firearm);
            _meleeView.SetActive(weapon == WeaponKind.Melee);
        }

        private void AttachWeapons()
        {
            Transform rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);

            if (rightHand == null)
                throw new InvalidOperationException("Player Humanoid rig has no right hand bone.");

            Vector3 firearmWorldScale = _firearmView.transform.lossyScale;
            Vector3 meleeWorldScale = _meleeView.transform.lossyScale;
            _firearmView.transform.SetParent(rightHand, false);
            _firearmView.transform.SetLocalPositionAndRotation(_firearmGripPosition, _firearmGripRotation);
            RestoreWorldScale(_firearmView.transform, firearmWorldScale);
            _meleeView.transform.SetParent(rightHand, false);
            _meleeView.transform.SetLocalPositionAndRotation(_meleeGripPosition, _meleeGripRotation);
            RestoreWorldScale(_meleeView.transform, meleeWorldScale);

            Transform attackPoint = _meleeWeapon.AttackPoint;
            attackPoint.SetParent(_meleeView.transform, false);
            attackPoint.SetLocalPositionAndRotation(_meleeHitPoint, Quaternion.identity);
        }

        private void AlignFirearmWithAim(Transform firearmTransform)
        {
            Transform muzzle = _firearm.Muzzle;

            if (muzzle == null)
                return;

            Quaternion muzzleRelativeRotation = Quaternion.Inverse(firearmTransform.rotation) * muzzle.rotation;
            Vector3 aimDirection = _aimCamera.transform.forward;
            aimDirection.y = Mathf.Clamp(aimDirection.y, -0.08f, 0.55f);
            aimDirection.Normalize();
            Quaternion desiredMuzzleRotation = Quaternion.LookRotation(aimDirection, transform.up);
            firearmTransform.rotation = desiredMuzzleRotation * Quaternion.Inverse(muzzleRelativeRotation);
        }

        private void RestoreWorldScale(Transform target, Vector3 worldScale)
        {
            Vector3 parentScale = target.parent.lossyScale;
            target.localScale = new Vector3(
                DivideScale(worldScale.x, parentScale.x),
                DivideScale(worldScale.y, parentScale.y),
                DivideScale(worldScale.z, parentScale.z));
        }

        private float DivideScale(float value, float divisor)
        {
            return Mathf.Abs(divisor) > Mathf.Epsilon ? value / divisor : value;
        }
    }
}
