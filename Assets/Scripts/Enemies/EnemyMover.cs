using UnityEngine;

namespace WildWest
{
    [RequireComponent(typeof(CharacterController))]
    public class EnemyMover : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _rotationSpeed = 540f;
        [SerializeField, Min(0f)] private float _gravity = 25f;

        private CharacterController _characterController;
        private float _moveSpeed;
        private float _verticalSpeed;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        public void Initialize(float moveSpeed)
        {
            _moveSpeed = Mathf.Max(0f, moveSpeed);
        }

        public void MoveTowards(Transform target)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > Mathf.Epsilon)
            {
                direction.Normalize();
                FaceDirection(direction);
            }

            if (_characterController.isGrounded && _verticalSpeed < 0f)
                _verticalSpeed = -2f;
            else
                _verticalSpeed -= _gravity * Time.deltaTime;

            Vector3 velocity = direction * _moveSpeed;
            velocity.y = _verticalSpeed;
            _characterController.Move(velocity * Time.deltaTime);
        }

        public void FaceTarget(Transform target)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            FaceDirection(direction);
        }

        private void FaceDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
        }
    }
}
