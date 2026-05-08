using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    [CreateAssetMenu(fileName = "New Department Data", menuName = "Code Blue/Department")]
    public class GameEventData : ScriptableObject, INetworkSerializable
    {
        public int ID;
        public string Name;
        public string Description;
        public string DurationText;
        public int DurationDay;
        public float SpawnRate;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ID);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref Description);
            serializer.SerializeValue(ref DurationText);
            serializer.SerializeValue(ref DurationDay);
            serializer.SerializeValue(ref SpawnRate);
        }
    }
}