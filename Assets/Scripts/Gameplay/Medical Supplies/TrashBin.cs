using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class TrashBin : BaseInteractable
    {
        public override void Interact(GameObject by)
        {
            if (by.TryGetComponent<Interaction>(out Interaction inter))
            {
                if (!inter.CarriedItem) return;

                // TODO: check if pooled or not
                DestroyItemRpc(NetworkManager.Singleton.LocalClientId);
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        void DestroyItemRpc(ulong clientId)
        {
            var player = NetworkManager.Singleton.ConnectedClients[clientId];
            var inter = player.PlayerObject.GetComponent<Interaction>();
            var dropped = inter.DropItem(transform.position);
            NetworkObjectPool.Singleton.ReturnNetworkObject(
                dropped.GetComponent<NetworkObject>(),
                PrefabDatabase.Instance.CommonPrefabs[dropped.GetComponent<Carryable>().PrefabType]
            );
        }
    }
}

