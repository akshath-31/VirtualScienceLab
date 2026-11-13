using UnityEngine;

namespace VirtualScienceLab
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class PlayerBeakerController : MonoBehaviour
    {
        [Header("Existing Player and UI")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private PlayerInteraction _inspection;
        [SerializeField] private Transform _holdAnchor;
        [SerializeField] private LineRenderer _pourStream;
        [SerializeField] private GameObject _pickupHint;
        [SerializeField] private GameObject _dropHint;
        [SerializeField] private GameObject _pourHint;

        [Header("Pickup and Drop")]
        [SerializeField, Min(0.1f)] private float _interactionDistance = 3f;
        [SerializeField] private LayerMask _raycastMask = ~0;
        [SerializeField] private Vector3 _holdingOffset = new Vector3(0.32f, -0.28f, 0.65f);
        [SerializeField, Min(0.5f)] private float _dropDistance = 1f;
        [SerializeField, Min(0.01f)] private float _dropClearance = 0.03f;

        [Header("Pour Animation")]
        [SerializeField, Min(1f)] private float _pourMillilitresPerSecond = 100f;
        [SerializeField, Range(40f, 75f)] private float _pourAngle = 65f;
        [SerializeField, Range(10f, 40f)] private float _flowStartAngle = 35f;
        [SerializeField, Min(1f)] private float _tiltDegreesPerSecond = 110f;
        [SerializeField, Min(0f)] private float _streamSag = 0.06f;
        [SerializeField, Min(0f)] private float _pourLift = 0.25f;
        [SerializeField, Min(0.1f)] private float _liftSpeed = 1.1f;
        private float _lift;
        private UnityEngine.InputSystem.InputAction _pickupAction;
        private UnityEngine.InputSystem.InputAction _pourAction;
        private bool _pickupRequested;
        private bool _pourHeld;
        private PickupBeaker _held;
        private PickupBeaker _lookedAt;
        private float _tilt;
        private Vector3 _lastPourDirection = Vector3.forward;

        public PickupBeaker HeldBeaker => _held;
        public PickupBeaker Destination => _lookedAt;
        public bool IsPouring { get; private set; }

        private void Awake()
        {
            if (_playerCamera == null || _inspection == null || _holdAnchor == null || _pourStream == null ||
                _pickupHint == null || _dropHint == null || _pourHint == null)
            {
                Debug.LogError("PlayerBeakerController: assign camera, inspection, hold anchor, stream and all hint references.", this);
                enabled = false;
                return;
            }
            _holdAnchor.localPosition = _holdingOffset;
            _pourStream.useWorldSpace = true;
            _pourStream.positionCount = 12;
            _pourStream.enabled = false;
            SetHints(false, false, false);
            _pickupAction = new UnityEngine.InputSystem.InputAction("Pickup or Drop Beaker", UnityEngine.InputSystem.InputActionType.Button, "<Keyboard>/f");
            _pourAction = new UnityEngine.InputSystem.InputAction("Pour Liquid", UnityEngine.InputSystem.InputActionType.Button, "<Keyboard>/r");
            _pickupAction.performed += OnPickup;
            _pourAction.performed += OnPour;
            _pourAction.canceled += OnPour;
        }

        private void OnEnable()
        {
            _pickupAction?.Enable();
            _pourAction?.Enable();
        }

        private void OnPickup(UnityEngine.InputSystem.InputAction.CallbackContext context) => _pickupRequested = true;
        private void OnPour(UnityEngine.InputSystem.InputAction.CallbackContext context) => _pourHeld = context.ReadValueAsButton();

        private void LateUpdate()
        {
            bool controlsAvailable = !_inspection.IsPanelOpen && Cursor.lockState == CursorLockMode.Locked;
            _lookedAt = controlsAvailable ? RaycastBeaker() : null;
            if (_pickupRequested)
            {
                _pickupRequested = false;
                if (controlsAvailable)
                {
                    if (_held != null) TryDrop();
                    else if (_lookedAt != null && !_lookedAt.IsHeld && _lookedAt.PickUp(_holdAnchor))
                    {
                        _held = _lookedAt;
                        _lookedAt = null;
                        _tilt = 0f;
                        _lift = 0f;
                    }
                }
            }
            bool validDestination = controlsAvailable && _held != null && _lookedAt != null &&
                _lookedAt != _held && !_lookedAt.IsHeld && _held.Liquid.CanTransferTo(_lookedAt.Liquid);
            bool wantsPour = validDestination && _pourHeld;
            _tilt = Mathf.MoveTowards(_tilt, wantsPour ? _pourAngle : 0f, _tiltDegreesPerSecond * Time.deltaTime);
            IsPouring = false;
            _pourStream.enabled = false;
            if (_held != null)
            {
                if (validDestination)
                {
                    Vector3 direction = Vector3.ProjectOnPlane(_lookedAt.Liquid.RimCentre - _held.transform.position, Vector3.up);
                    if (direction.sqrMagnitude > 0.0001f) _lastPourDirection = direction.normalized;
                }
                Vector3 axis = Vector3.Cross(Vector3.up, _lastPourDirection).normalized;
                _held.transform.position = _holdAnchor.position;
                _held.transform.rotation = Quaternion.AngleAxis(_tilt, axis);
                Vector3 lipDirection = Vector3.ProjectOnPlane(_lastPourDirection, _held.transform.up).normalized;
                Vector3 lip = _held.Liquid.RimCentre + lipDirection * _held.Liquid.InnerRadius;
                float desiredLift = wantsPour ? Mathf.Max(_pourLift, _lookedAt.Liquid.RimCentre.y + 0.15f - lip.y) : 0f;
                _lift = Mathf.MoveTowards(_lift, desiredLift, _liftSpeed * Time.deltaTime);
                _held.transform.position += Vector3.up * _lift;
                if (wantsPour && _tilt >= _flowStartAngle && lip.y + _lift > _lookedAt.Liquid.RimCentre.y)
                {
                    float amount = _held.Liquid.TransferTo(_lookedAt.Liquid, _pourMillilitresPerSecond * Time.deltaTime);
                    IsPouring = amount > 0f;
                    if (IsPouring) DrawStream(_lookedAt.Liquid);
                }
            }
            SetHints(controlsAvailable && _held == null && _lookedAt != null,
                controlsAvailable && _held != null, validDestination);
        }

        private PickupBeaker RaycastBeaker()
        {
            Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            ray.origin = _playerCamera.transform.position;
            if (!Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _raycastMask, QueryTriggerInteraction.Ignore)) return null;
            PickupBeaker beaker = hit.collider.GetComponentInParent<PickupBeaker>();
            return beaker != null && beaker.isActiveAndEnabled ? beaker : null;
        }

        private bool TryDrop()
        {
            Vector3 direction = Vector3.ProjectOnPlane(_playerCamera.transform.forward, Vector3.up).normalized;
            if (direction.sqrMagnitude < 0.1f) direction = transform.forward;
            float radius = Mathf.Max(_held.DropHalfExtents.x, _held.DropHalfExtents.z) + _dropClearance;
            float distance = _dropDistance;
            Vector3 origin = _playerCamera.transform.position;
            if (Physics.SphereCast(origin, radius, direction, out RaycastHit obstruction, distance, _raycastMask, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0f, obstruction.distance - _dropClearance);
            if (distance < 0.5f) return false;
            Vector3 position = origin + direction * distance;
            if (Physics.Raycast(position + Vector3.up * 0.25f, Vector3.down, out RaycastHit support, 3f, _raycastMask, QueryTriggerInteraction.Ignore))
                position.y = support.point.y + _dropClearance;
            Vector3 centre = position + _held.ColliderCentre;
            if (Physics.CheckBox(centre, _held.DropHalfExtents * 0.98f, Quaternion.identity, _raycastMask, QueryTriggerInteraction.Ignore)) return false;
            _held.Drop(position);
            _held = null;
            _tilt = 0f;
            _pourStream.enabled = false;
            return true;
        }

        private void DrawStream(LiquidContainer destination)
        {
            Vector3 direction = Vector3.ProjectOnPlane(_lastPourDirection, _held.transform.up).normalized;
            Vector3 start = _held.Liquid.RimCentre + direction * _held.Liquid.InnerRadius;
            Vector3 end = destination.RimCentre - Vector3.up * 0.02f;
            for (int i = 0; i < _pourStream.positionCount; i++)
            {
                float t = i / (float)(_pourStream.positionCount - 1);
                _pourStream.SetPosition(i, Vector3.Lerp(start, end, t) - Vector3.up * (_streamSag * 4f * t * (1f - t)));
            }
            _pourStream.enabled = true;
        }

        private void SetHints(bool pickup, bool drop, bool pour)
        {
            if (_pickupHint.activeSelf != pickup) _pickupHint.SetActive(pickup);
            if (_dropHint.activeSelf != drop) _dropHint.SetActive(drop);
            if (_pourHint.activeSelf != pour) _pourHint.SetActive(pour);
        }

        private void OnDisable()
        {
            _pickupAction?.Disable();
            _pourAction?.Disable();
            _pickupRequested = false;
            _pourHeld = false;
            IsPouring = false;
            if (_pourStream != null) _pourStream.enabled = false;
            if (_pickupHint != null && _dropHint != null && _pourHint != null) SetHints(false, false, false);
        }

        private void OnDestroy()
        {
            _pickupAction?.Dispose();
            _pourAction?.Dispose();
        }
    }
}
