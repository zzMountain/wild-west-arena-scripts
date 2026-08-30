using System;
using UnityEngine;

namespace WildWest
{
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public class CharacterMotionView : MonoBehaviour
    {
        private const int BaseLayer = 0;
        private const int UpperBodyLayer = 1;
        private const float BaseCrossFadeDuration = 0.14f;
        private const float UpperCrossFadeDuration = 0.08f;

        private static readonly int IdleState = Animator.StringToHash("Idle");
        private static readonly int WalkState = Animator.StringToHash("Walk");
        private static readonly int HitState = Animator.StringToHash("Hit");
        private static readonly int DeathState = Animator.StringToHash("Death");
        private static readonly int PistolIdleState = Animator.StringToHash("PistolIdle");
        private static readonly int PistolShootState = Animator.StringToHash("PistolShoot");
        private static readonly int MeleeIdleState = Animator.StringToHash("MeleeIdle");
        private static readonly int MeleeAttackState = Animator.StringToHash("MeleeAttack");
        private static readonly int MoveSpeedMultiplier = Animator.StringToHash("MoveSpeedMultiplier");

        [SerializeField] private Transform _visualRoot;

        private CharacterController _characterController;
        private Animator _animator;
        private PlayerCombat _playerCombat;
        private EnemyAttack _enemyAttack;
        private Health _health;
        private int _currentBaseState;
        private int _currentUpperState;
        private float _upperAttackTime;
        private float _hitTime;
        private bool _isDead;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _health = GetComponent<Health>();
            _playerCombat = GetComponent<PlayerCombat>();
            _enemyAttack = GetComponent<EnemyAttack>();

            if (_visualRoot == null)
                throw new InvalidOperationException("CharacterMotionView requires a visual root.");

            _animator = _visualRoot.GetComponent<Animator>();

            if (_animator == null || _animator.avatar == null || _animator.avatar.isHuman == false)
                throw new InvalidOperationException("CharacterMotionView requires a valid Humanoid Animator.");

            if (_animator.runtimeAnimatorController == null)
                throw new InvalidOperationException("CharacterMotionView requires an Animator Controller.");

            _animator.applyRootMotion = false;
            _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
            _health.Died += OnDied;

            if (_playerCombat != null)
                _playerCombat.AttackStarted += OnPlayerAttackStarted;

            if (_enemyAttack != null)
                _enemyAttack.Attacked += OnEnemyAttacked;
        }

        private void Start()
        {
            PlayBase(IdleState, 0f);
            PlayUpper(GetRestingUpperState(), 0f);
        }

        private void Update()
        {
            if (_isDead)
                return;

            Vector3 horizontalVelocity = _characterController.velocity;
            horizontalVelocity.y = 0f;
            float movementSpeed = horizontalVelocity.magnitude;
            float animationSpeed = movementSpeed > 0.2f
                ? Mathf.Clamp(movementSpeed / 2.5f, 0.75f, 2.4f)
                : 1f;
            _animator.SetFloat(MoveSpeedMultiplier, animationSpeed, 0.08f, Time.deltaTime);
            int locomotionState = horizontalVelocity.sqrMagnitude > 0.04f ? WalkState : IdleState;

            if (_hitTime > 0f)
            {
                _hitTime -= Time.deltaTime;
                PlayBase(HitState, 0.04f);
                _animator.SetLayerWeight(UpperBodyLayer, 0f);
            }
            else
            {
                PlayBase(locomotionState, BaseCrossFadeDuration);
                _animator.SetLayerWeight(UpperBodyLayer, 1f);
            }

            if (_upperAttackTime > 0f)
            {
                _upperAttackTime -= Time.deltaTime;

                if (_upperAttackTime <= 0f)
                    PlayUpper(GetRestingUpperState(), UpperCrossFadeDuration);
            }
            else
            {
                PlayUpper(GetRestingUpperState(), UpperCrossFadeDuration);
            }
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;

            if (_playerCombat != null)
                _playerCombat.AttackStarted -= OnPlayerAttackStarted;

            if (_enemyAttack != null)
                _enemyAttack.Attacked -= OnEnemyAttacked;
        }

        private int GetRestingUpperState()
        {
            if (_playerCombat != null && _playerCombat.CurrentWeapon == WeaponKind.Firearm)
                return PistolIdleState;

            return MeleeIdleState;
        }

        private void PlayBase(int state, float duration)
        {
            if (_currentBaseState == state)
                return;

            _currentBaseState = state;
            _animator.CrossFadeInFixedTime(state, duration, BaseLayer);
        }

        private void PlayUpper(int state, float duration)
        {
            if (_currentUpperState == state)
                return;

            _currentUpperState = state;
            _animator.CrossFadeInFixedTime(state, duration, UpperBodyLayer);
        }

        private void OnPlayerAttackStarted(WeaponKind weapon)
        {
            _upperAttackTime = weapon == WeaponKind.Firearm ? 0.63f : 1.5f;
            int state = weapon == WeaponKind.Firearm ? PistolShootState : MeleeAttackState;
            PlayUpper(state, weapon == WeaponKind.Firearm ? 0.03f : 0.05f);
        }

        private void OnEnemyAttacked()
        {
            _upperAttackTime = 1.5f;
            PlayUpper(MeleeAttackState, 0.05f);
        }

        private void OnDamaged(int damage)
        {
            if (damage <= 0 || _isDead)
                return;

            _hitTime = 0.28f;
            _currentBaseState = 0;
        }

        private void OnDied()
        {
            _isDead = true;
            _animator.SetLayerWeight(UpperBodyLayer, 0f);
            _currentBaseState = 0;
            PlayBase(DeathState, 0.08f);
        }
    }
}
