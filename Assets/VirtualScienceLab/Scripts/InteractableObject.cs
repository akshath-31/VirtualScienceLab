using UnityEngine;

namespace VirtualScienceLab
{
    [DisallowMultipleComponent]
    public sealed class InteractableObject : MonoBehaviour
    {
        [Header("Equipment Information")]
        [SerializeField] private string _equipmentName = "Laboratory Equipment";
        [SerializeField, TextArea(4, 12)] private string _description = "Describe this equipment and its safe use.";

        public string EquipmentName => _equipmentName;
        public string Description => _description;
    }
}
