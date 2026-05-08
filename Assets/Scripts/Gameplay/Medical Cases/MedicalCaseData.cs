using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    [CreateAssetMenu(fileName = "New Medical Case Data", menuName = "Code Blue/Medical Case")]
    public class MedicalCaseData : ScriptableObject, INetworkSerializable
    {
        public int ID;
        public string Name;
        public Department RequiredDeparment;
        public TriageLevel TriageLevel;
        public Color TriageLevelColor;
        public float PatienceDuration;
        public bool PatienceReset;
        public int SpawnRate;
        public int WheelChairUsageRate;
        public int FatalityRate;
        public int[] TreatmentPlan;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ID);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref RequiredDeparment);
            serializer.SerializeValue(ref TriageLevel);
            serializer.SerializeValue(ref TriageLevelColor);
            serializer.SerializeValue(ref PatienceDuration);
            serializer.SerializeValue(ref PatienceReset);
            serializer.SerializeValue(ref SpawnRate);
            serializer.SerializeValue(ref WheelChairUsageRate);
            serializer.SerializeValue(ref FatalityRate);
            serializer.SerializeValue(ref TreatmentPlan);
        }
    }
}
