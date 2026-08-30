using UnityEngine;

namespace WildWest
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _moveSpeed = 6f;
        [SerializeField, Min(0f)] private float _rotationSpeed = 720f;
        [SerializeField, Min(0f)] private float _gravity = 25f;

        private CharacterController _characterController;
        private float _verticalSpeed;

        public Vector3 Velocity => _characterController.velocity;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        public void Move(Vector2 input, Transform cameraTransform)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 direction = cameraForward * input.y + cameraRight * input.x;

            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            if (_characterController.isGrounded && _verticalSpeed < 0f)
                _verticalSpeed = -2f;
            else
                _verticalSpeed -= _gravity * Time.deltaTime;

            Vector3 displacement = direction * _moveSpeed;
            displacement.y = _verticalSpeed;
            _characterController.Move(displacement * Time.deltaTime);

            FaceDirection(cameraForward);
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
