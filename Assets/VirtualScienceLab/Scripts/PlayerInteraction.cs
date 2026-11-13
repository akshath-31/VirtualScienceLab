using UnityEngine;

namespace VirtualScienceLab
{
    [DisallowMultipleComponent]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private FirstPersonController _movementController;
        [SerializeField, Min(0.1f)] private float _interactionDistance = 3f;
        [SerializeField] private LayerMask _interactionMask = ~0;

        [Header("Inspection UI")]
        [SerializeField] private GameObject _crosshair;
        [SerializeField] private GameObject _prompt;
        [SerializeField] private GameObject _informationPanel;
        [SerializeField] private TMPro.TMP_Text _equipmentNameText;
        [SerializeField] private TMPro.TMP_Text _descriptionText;
        [SerializeField] private UnityEngine.UI.Button _closeButton;

        private UnityEngine.InputSystem.InputAction _inspectAction;
        private InteractableObject _currentTarget;
        private bool _interactionRequested;
        private bool _panelOpen;
        private bool _restoreMovement;
        private CursorLockMode _previousCursorLock;
        private bool _previousCursorVisible;

        public InteractableObject CurrentTarget => _currentTarget;
        public bool IsPanelOpen => _panelOpen;

        private void Awake()
        {
            if (_playerCamera == null || _movementController == null || _crosshair == null ||
                _prompt == null || _informationPanel == null || _equipmentNameText == null ||
                _descriptionText == null || _closeButton == null)
            {
                Debug.LogError("PlayerInteraction: assign all player and inspection UI references.", this);
                enabled = false;
                return;
            }
            _informationPanel.SetActive(false);
            _prompt.SetActive(false);
            _crosshair.SetActive(true);
            _closeButton.onClick.AddListener(ClosePanel);
            _inspectAction = new UnityEngine.InputSystem.InputAction("Inspect", UnityEngine.InputSystem.InputActionType.Button, "<Keyboard>/e");
            _inspectAction.performed += OnInspect;
        }

        private void OnEnable() => _inspectAction?.Enable();

        private void OnInspect(UnityEngine.InputSystem.InputAction.CallbackContext context) => _interactionRequested = true;

        private void LateUpdate()
        {
            if (!_panelOpen) UpdateTarget();
            if (!_interactionRequested) return;
            _interactionRequested = false;
            if (_panelOpen) ClosePanel();
            else if (_currentTarget != null) OpenPanel(_currentTarget);
        }

        private void UpdateTarget()
        {
            InteractableObject target = null;
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                // Measure the three-metre range from the camera, not its near plane.
                ray.origin = _playerCamera.transform.position;
                if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance, _interactionMask, QueryTriggerInteraction.Ignore))
                {
                    target = hit.collider.GetComponentInParent<InteractableObject>();
                    if (target != null && !target.isActiveAndEnabled) target = null;
                }
            }
            _currentTarget = target;
            bool showPrompt = target != null;
            if (_prompt.activeSelf != showPrompt) _prompt.SetActive(showPrompt);
        }

        private void OpenPanel(InteractableObject equipment)
        {
            _equipmentNameText.text = equipment.EquipmentName;
            _descriptionText.text = equipment.Description;
            _previousCursorLock = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            _restoreMovement = _movementController.enabled;
            _panelOpen = true;
            _prompt.SetActive(false);
            _crosshair.SetActive(false);
            _informationPanel.SetActive(true);
            // Pause movement and mouse look without altering the existing controller script.
            if (_restoreMovement) _movementController.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ClosePanel()
        {
            if (!_panelOpen) return;
            _panelOpen = false;
            _informationPanel.SetActive(false);
            _crosshair.SetActive(true);
            if (_restoreMovement && _movementController != null) _movementController.enabled = true;
            _restoreMovement = false;
            Cursor.lockState = _previousCursorLock;
            Cursor.visible = _previousCursorVisible;
            _currentTarget = null;
            _prompt.SetActive(false);
        }

        private void OnDisable()
        {
            _inspectAction?.Disable();
            _interactionRequested = false;
            ClosePanel();
            if (_prompt != null) _prompt.SetActive(false);
            _currentTarget = null;
        }

        private void OnDestroy()
        {
            _inspectAction?.Dispose();
            if (_closeButton != null) _closeButton.onClick.RemoveListener(ClosePanel);
        }
    }
}
