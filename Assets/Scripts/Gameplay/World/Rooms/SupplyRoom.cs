using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class SupplyRoom : SingletonBehaviour<SupplyRoom>
    {
        [SerializeField] MedicalSupplyShelf[] _shelves;

        [Rpc(SendTo.Server)]
        public void SpawnSupplyBoxesRpc(int caseId)
        {

        }
    }
}
