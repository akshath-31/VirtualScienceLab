using UnityEngine;
using UnityEngine.InputSystem;

namespace VirtualScienceLab
{
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public sealed class FirstPersonController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Child camera transform, normally 1.6 metres above the player's feet.")]
        [SerializeField] private Transform _cameraTransform;

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float _moveSpeed = 3.5f;
        [SerializeField, Min(0f)] private float _jumpHeight = 1f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField, Min(0f)] private float _groundStickSpeed = 2f;

        [Header("Mouse Look")]
        [Tooltip("Degrees of rotation per pixel of mouse movement.")]
        [SerializeField, Min(0.001f)] private float _mouseSensitivity = 0.12f;
        [SerializeField, Range(1f, 89f)] private float _pitchLimit = 85f;

        private CharacterController _controller;
        private UnityEngine.InputSystem.InputActionMap _actions;
        private UnityEngine.InputSystem.InputAction _moveAction;
        private UnityEngine.InputSystem.InputAction _lookAction;
        private UnityEngine.InputSystem.InputAction _jumpAction;
        private UnityEngine.InputSystem.InputAction _releaseAction;
        private UnityEngine.InputSystem.InputAction _captureAction;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private float _verticalSpeed;
        private float _pitch;
        private bool _jumpRequested;
        private bool _cursorCaptured;

        public bool IsGrounded => _controller != null && _controller.isGrounded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (_cameraTransform == null)
            {
                Camera childCamera = GetComponentInChildren<Camera>();
                if (childCamera != null) _cameraTransform = childCamera.transform;
            }
            if (_cameraTransform == null)
            {
                Debug.LogError("FirstPersonController requires a child camera.", this);
                enabled = false;
                return;
            }
            _pitch = Mathf.DeltaAngle(0f, _cameraTransform.localEulerAngles.x);
            _actions = new UnityEngine.InputSystem.InputActionMap("First Person");
            _moveAction = _actions.AddAction("Move", UnityEngine.InputSystem.InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _lookAction = _actions.AddAction("Look", UnityEngine.InputSystem.InputActionType.Value, "<Mouse>/delta");
            _jumpAction = _actions.AddAction("Jump", UnityEngine.InputSystem.InputActionType.Button, "<Keyboard>/space");
            _releaseAction = _actions.AddAction("Release Cursor", UnityEngine.InputSystem.InputActionType.Button, "<Keyboard>/escape");
            _captureAction = _actions.AddAction("Capture Cursor", UnityEngine.InputSystem.InputActionType.Button, "<Mouse>/leftButton");
            _moveAction.performed += OnMove;
            _moveAction.canceled += OnMove;
            _lookAction.performed += OnLook;
            _lookAction.canceled += OnLook;
            _jumpAction.performed += OnJump;
            _releaseAction.performed += OnRelease;
            _captureAction.performed += OnCapture;
        }

        private void OnEnable()
        {
            if (_actions == null) return;
            _actions.Enable();
            SetCursorCaptured(true);
        }

        private void Update()
        {
            if (_cursorCaptured)
            {
                transform.Rotate(0f, _lookInput.x * _mouseSensitivity, 0f);
                _pitch = Mathf.Clamp(_pitch - _lookInput.y * _mouseSensitivity, -_pitchLimit, _pitchLimit);
                _cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
            _lookInput = Vector2.zero;
            if (_controller.isGrounded && _verticalSpeed < 0f) _verticalSpeed = -_groundStickSpeed;
            if (_cursorCaptured && _jumpRequested && _controller.isGrounded)
                _verticalSpeed = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            _jumpRequested = false;
            float averageVerticalSpeed = _verticalSpeed + 0.5f * _gravity * Time.deltaTime;
            _verticalSpeed += _gravity * Time.deltaTime;
            Vector2 input = _cursorCaptured ? Vector2.ClampMagnitude(_moveInput, 1f) : Vector2.zero;
            Vector3 movement = (transform.right * input.x + transform.forward * input.y) * _moveSpeed;
            movement.y = averageVerticalSpeed;
            CollisionFlags collision = _controller.Move(movement * Time.deltaTime);
            if ((collision & CollisionFlags.Above) != 0 && _verticalSpeed > 0f) _verticalSpeed = 0f;
        }

        private void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context) => _moveInput = context.ReadValue<Vector2>();
        private void OnLook(UnityEngine.InputSystem.InputAction.CallbackContext context) => _lookInput = context.ReadValue<Vector2>();
        private void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext context) { if (_cursorCaptured) _jumpRequested = true; }
        private void OnRelease(UnityEngine.InputSystem.InputAction.CallbackContext context) => SetCursorCaptured(false);
        private void OnCapture(UnityEngine.InputSystem.InputAction.CallbackContext context) => SetCursorCaptured(true);

        private void SetCursorCaptured(bool captured)
        {
            _cursorCaptured = captured;
            _lookInput = Vector2.zero;
            _jumpRequested = false;
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) SetCursorCaptured(false);
        }

        private void OnDisable()
        {
            _actions?.Disable();
            _moveInput = Vector2.zero;
            SetCursorCaptured(false);
        }

        private void OnDestroy() => _actions?.Dispose();

        private void OnValidate()
        {
            _gravity = Mathf.Min(_gravity, -0.1f);
        }
    }
}
