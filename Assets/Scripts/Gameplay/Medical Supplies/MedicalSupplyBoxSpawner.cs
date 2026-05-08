using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class MedicalSupplyBoxSpawner : SingletonBehaviour<MedicalSupplyBoxSpawner>
    {
        [Rpc(SendTo.Server)]
        public void Sv_SpawnBoxesRpc(int[] supplyIds)
        {
            if (!IsServer) return;
            foreach (var id in supplyIds)
            {
                var halfExtents = transform.localScale / 2;
                float x = Random.Range(transform.position.x - halfExtents.x, transform.position.x + halfExtents.x);
                float z = Random.Range(transform.position.z - halfExtents.z, transform.position.z + halfExtents.z);
                var pos = new Vector3(x, transform.position.y, z);

                var box = NetworkObjectPool.Singleton.GetNetworkObject(PrefabDatabase.Instance.CommonPrefabs[PrefabType.SupplyBox], pos, Quaternion.identity);
                box.Spawn();
                box.GetComponent<MedicalSupplyBox>().InitializeRpc(id);
            }

            if (supplyIds.Length > 0)
                Cl_ShowNotifsRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        void Cl_ShowNotifsRpc()
        {
            PagerMessage.Instance.ShowMessage("Your packages have been delivered at the loading bay");
            PagerMessage.Instance.ShowNotificationFlag(GameObject.Find("Delivery Bay").transform);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
