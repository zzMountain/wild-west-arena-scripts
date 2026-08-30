using UnityEngine;

namespace WildWest
{
    [RequireComponent(typeof(Camera))]
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private PlayerInputReader _inputReader;
        [SerializeField, Min(0.1f)] private float _distance = 5.5f;
        [SerializeField, Min(0f)] private float _height = 1.7f;
        [SerializeField] private float _shoulderOffset = 0.65f;
        [SerializeField, Min(0f)] private float _sensitivity = 0.12f;
        [SerializeField] private float _minimumPitch = -35f;
        [SerializeField] private float _maximumPitch = 65f;
        [SerializeField, Min(0f)] private float _collisionRadius = 0.25f;
        [SerializeField] private LayerMask _collisionMask = -1;
        [SerializeField, Min(0f)] private float _recoilReturnSpeed = 15f;
        [SerializeField, Min(0f)] private float _fovReturnSpeed = 18f;

        private Camera _camera;
        private float _yaw;
        private float _pitch = 12f;
        private float _recoilPitch;
        private float _recoilYaw;
        private float _recoilRoll;
        private float _fovKick;
        private float _baseFieldOfView;
        private float _shake;
        private float _shakeSeed;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _baseFieldOfView = _camera.fieldOfView;
            _yaw = _target.eulerAngles.y;
        }

        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            ReleaseCursor();
        }

        private void LateUpdate()
        {
            float deltaTime = Time.unscaledDeltaTime;
            Vector2 look = _inputReader.Look;
            _yaw += look.x * _sensitivity;
            _pitch = Mathf.Clamp(_pitch - look.y * _sensitivity, _minimumPitch, _maximumPitch);
            _recoilPitch = Mathf.MoveTowards(_recoilPitch, 0f, _recoilReturnSpeed * deltaTime);
            _recoilYaw = Mathf.MoveTowards(_recoilYaw, 0f, _recoilReturnSpeed * deltaTime);
            _recoilRoll = Mathf.MoveTowards(_recoilRoll, 0f, _recoilReturnSpeed * deltaTime);
            _fovKick = Mathf.MoveTowards(_fovKick, 0f, _fovReturnSpeed * deltaTime);
            _shake = Mathf.MoveTowards(_shake, 0f, deltaTime * 0.8f);
            _camera.fieldOfView = _baseFieldOfView + _fovKick;

            Quaternion orbitRotation = Quaternion.Euler(
                _pitch + _recoilPitch,
                _yaw + _recoilYaw,
                _recoilRoll);
            Vector3 pivot = _target.position + Vector3.up * _height;
            Vector3 backward = orbitRotation * Vector3.back;
            Vector3 shoulder = orbitRotation * Vector3.right * _shoulderOffset;
            Vector3 desiredPosition = pivot + backward * _distance + shoulder;
            Vector3 castDirection = desiredPosition - pivot;
            float castDistance = castDirection.magnitude;

            if (Physics.SphereCast(
                    pivot,
                    _collisionRadius,
                    castDirection.normalized,
                    out RaycastHit hit,
                    castDistance,
                    _collisionMask,
                    QueryTriggerInteraction.Ignore))
            {
                desiredPosition = pivot + castDirection.normalized * Mathf.Max(0.1f, hit.distance - _collisionRadius);
            }

            if (_shake > 0f)
            {
                float horizontalShake = (Mathf.PerlinNoise(_shakeSeed, Time.time * 35f) - 0.5f) * _shake;
                float verticalShake = (Mathf.PerlinNoise(_shakeSeed + 10f, Time.time * 35f) - 0.5f) * _shake;
                desiredPosition += orbitRotation * new Vector3(horizontalShake, verticalShake, 0f);
            }

            transform.SetPositionAndRotation(desiredPosition, Quaternion.LookRotation(pivot - desiredPosition));
        }

        public void AddShotImpulse()
        {
            _recoilPitch -= 1.8f;
            _recoilYaw += UnityEngine.Random.Range(-0.35f, 0.35f);
            _recoilRoll += UnityEngine.Random.Range(-0.25f, 0.25f);
            _fovKick = Mathf.Max(_fovKick, 1.4f);
            _shake = Mathf.Max(_shake, 0.12f);
            _shakeSeed = UnityEngine.Random.value * 100f;
        }

        public void ReleaseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
