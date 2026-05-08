using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public enum Department
    {
        LockerRoom,
        LobbyArea,
        Pharmacy,
        SupplyRoom,
        Morgue,
        ConsultationRoom,
        OperatingRoom,
        MaternityWard,
        PediatricsUnit,
        AmbulanceLane
    }

    [CreateAssetMenu(fileName = "New Department Data", menuName = "Code Blue/Department")]
    public class DepartmentData : ScriptableObject, INetworkSerializable
    {
        public int ID;
        public string Name;
        public string Description;
        public Department Department;
        public GameObject DepartmentPrefab;
        public Sprite Icon;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ID);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref Description);
            serializer.SerializeValue(ref Department);
        }
    }
}