using Unity.Netcode;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeBlue
{
    public class MedicalSupply : Carryable
    {
        [field: SerializeField] public MedicalSupplyData Data { get; set; }

        void OnValidate()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_InitializeRpc(int supplyId)
        {
            Data = MedicalSupplyDatabase.Instance.GetData(supplyId);
            // TODO: update texture image
        }

        protected override void OnUpdate()
        {
        }

        public override string ToString() => Data.Name;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.up, Data.Name);
        }
#endif
    }
}
