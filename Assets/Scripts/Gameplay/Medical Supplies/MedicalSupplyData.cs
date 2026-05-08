using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public enum MedicalSupplyLocation
    {
        Pharmacy,
        SupplyRoom
    }

    [CreateAssetMenu(fileName = "New Medical Supply", menuName = "Code Blue/Medical Supply")]
    public class MedicalSupplyData : ScriptableObject, INetworkSerializable, IEquatable<MedicalSupplyData>
    {
        public int ID;
        public string Name;
        public string Description;
        public MedicalSupplyLocation Location;
        public float Cost;
        public bool IsShared;
        public GameObject SupplyPrefab;
        public Sprite UITexture;

        void OnValidate()
        {
            if (name != Name)
                name = Name;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ID);
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref Description);
            serializer.SerializeValue(ref Location);
            serializer.SerializeValue(ref Cost);
            serializer.SerializeValue(ref IsShared);
        }
        public bool Equals(MedicalSupplyData other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return base.Equals(other) && ID == other.ID;
        }
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((MedicalSupplyData)obj);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ID);
        }
    }
}