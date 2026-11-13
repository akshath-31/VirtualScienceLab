using UnityEngine;

namespace VirtualScienceLab
{
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider), typeof(LiquidContainer))]
    [DisallowMultipleComponent]
    public sealed class PickupBeaker : MonoBehaviour
    {
        private Rigidbody _body;
        private BoxCollider _box;
        private Collider[] _colliders;
        private bool[] _colliderStates;
        private Transform _originalParent;

        public bool IsHeld { get; private set; }
        public LiquidContainer Liquid { get; private set; }
        public Vector3 DropHalfExtents => Vector3.Scale(_box.size * 0.5f, transform.lossyScale);
        public Vector3 ColliderCentre => _box.center;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _box = GetComponent<BoxCollider>();
            Liquid = GetComponent<LiquidContainer>();
            _colliders = GetComponentsInChildren<Collider>();
            _colliderStates = new bool[_colliders.Length];
            _originalParent = transform.parent;
        }

        public bool PickUp(Transform holdAnchor)
        {
            if (IsHeld || holdAnchor == null) return false;
            if (!_body.isKinematic)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }
            _body.isKinematic = true;
            _body.useGravity = false;
            for (int i = 0; i < _colliders.Length; i++)
            {
                _colliderStates[i] = _colliders[i].enabled;
                _colliders[i].enabled = false;
            }
            IsHeld = true;
            transform.SetParent(holdAnchor, false);
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
            return true;
        }

        public void Drop(Vector3 feetPosition)
        {
            if (!IsHeld) return;
            transform.SetParent(_originalParent, true);
            transform.SetPositionAndRotation(feetPosition, Quaternion.identity);
            _body.position = feetPosition;
            _body.rotation = Quaternion.identity;
            for (int i = 0; i < _colliders.Length; i++) _colliders[i].enabled = _colliderStates[i];
            _body.useGravity = true;
            _body.isKinematic = false;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            IsHeld = false;
        }
    }
}
