using UnityEngine;

namespace VirtualScienceLab
{
    [DisallowMultipleComponent]
    public sealed class LiquidContainer : MonoBehaviour
    {
        [Header("Liquid")]
        [SerializeField] private string _liquidType = "Water";
        [SerializeField, Min(1f)] private float _capacityMillilitres = 500f;
        [SerializeField, Min(0f)] private float _amountMillilitres = 250f;

        [Header("Interior Dimensions (metres)")]
        [SerializeField, Min(0.001f)] private float _innerRadius = 0.085f;
        [SerializeField, Min(0.001f)] private float _innerHeight = 0.21f;
        [SerializeField] private float _baseHeight = 0.02f;
        [SerializeField] private MeshFilter _liquidMeshFilter;
        [SerializeField] private Renderer _liquidRenderer;
        [SerializeField, Range(12, 48)] private int _segments = 24;

        private Mesh _mesh;
        private Vector3[] _vertices;
        private Vector3 _lastUp;
        private float _lastAmount = -1f;

        public float AmountMillilitres => _amountMillilitres;
        public float CapacityMillilitres => _capacityMillilitres;
        public float FillLevel => _amountMillilitres / _capacityMillilitres;
        public float RemainingCapacity => Mathf.Max(0f, _capacityMillilitres - _amountMillilitres);
        public float InnerRadius => _innerRadius;
        public Vector3 RimCentre => transform.TransformPoint(new Vector3(0f, _baseHeight + _innerHeight, 0f));

        private void Awake()
        {
            _amountMillilitres = Mathf.Clamp(_amountMillilitres, 0f, _capacityMillilitres);
            RefreshVisual();
        }

        private void LateUpdate()
        {
            Vector3 up = transform.InverseTransformDirection(Vector3.up);
            if (Mathf.Abs(_lastAmount - _amountMillilitres) > 0.0001f || (up - _lastUp).sqrMagnitude > 0.00001f)
                RefreshVisual();
        }

        public void SetFillLevel(float fraction)
        {
            _amountMillilitres = Mathf.Clamp01(fraction) * _capacityMillilitres;
            RefreshVisual();
        }

        public bool CanTransferTo(LiquidContainer destination)
        {
            return destination != null && destination != this && destination.isActiveAndEnabled &&
                destination._liquidType == _liquidType && _amountMillilitres > 0.001f && destination.RemainingCapacity > 0.001f;
        }

        public float TransferTo(LiquidContainer destination, float requestedMillilitres)
        {
            if (!CanTransferTo(destination) || requestedMillilitres <= 0f) return 0f;
            float amount = Mathf.Min(requestedMillilitres, _amountMillilitres, destination.RemainingCapacity);
            _amountMillilitres -= amount;
            destination._amountMillilitres += amount;
            RefreshVisual();
            destination.RefreshVisual();
            return amount;
        }

        public void RefreshVisual()
        {
            if (_liquidMeshFilter == null || _liquidRenderer == null) return;
            if (_mesh == null)
            {
                _mesh = new Mesh { name = "Contained Liquid Mesh" };
                _mesh.MarkDynamic();
                _vertices = new Vector3[_segments * 2 + 2];
                int[] triangles = new int[_segments * 12];
                int topCentre = _segments + 1;
                for (int i = 0; i < _segments; i++)
                {
                    int next = (i + 1) % _segments;
                    int b = 1 + i, bn = 1 + next, t = topCentre + 1 + i, tn = topCentre + 1 + next;
                    int n = i * 12;
                    triangles[n] = 0; triangles[n + 1] = bn; triangles[n + 2] = b;
                    triangles[n + 3] = b; triangles[n + 4] = tn; triangles[n + 5] = t;
                    triangles[n + 6] = b; triangles[n + 7] = bn; triangles[n + 8] = tn;
                    triangles[n + 9] = topCentre; triangles[n + 10] = t; triangles[n + 11] = tn;
                }
                _mesh.vertices = _vertices;
                _mesh.triangles = triangles;
                _liquidMeshFilter.sharedMesh = _mesh;
            }
            _lastUp = transform.InverseTransformDirection(Vector3.up);
            _lastAmount = _amountMillilitres;
            float level = _baseHeight + _innerHeight * Mathf.Clamp01(FillLevel);
            _vertices[0] = new Vector3(0f, _baseHeight, 0f);
            _vertices[_segments + 1] = new Vector3(0f, level, 0f);
            float divisor = Mathf.Max(0.2f, _lastUp.y);
            for (int i = 0; i < _segments; i++)
            {
                float angle = i * Mathf.PI * 2f / _segments;
                float x = Mathf.Sin(angle) * _innerRadius, z = Mathf.Cos(angle) * _innerRadius;
                float top = Mathf.Clamp(level - (_lastUp.x * x + _lastUp.z * z) / divisor, _baseHeight, _baseHeight + _innerHeight);
                _vertices[1 + i] = new Vector3(x, _baseHeight, z);
                _vertices[_segments + 2 + i] = new Vector3(x, top, z);
            }
            _mesh.vertices = _vertices;
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            _liquidRenderer.enabled = _amountMillilitres > 0.001f;
        }

        private void OnValidate()
        {
            _capacityMillilitres = Mathf.Max(1f, _capacityMillilitres);
            _amountMillilitres = Mathf.Clamp(_amountMillilitres, 0f, _capacityMillilitres);
        }

        private void OnDestroy()
        {
            if (Application.isPlaying && _mesh != null) Destroy(_mesh);
        }
    }
}
