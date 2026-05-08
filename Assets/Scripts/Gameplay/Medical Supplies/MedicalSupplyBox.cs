using DG.Tweening;
using NaughtyAttributes;
using System;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class MedicalSupplyBox : Carryable
    {
        [field: SerializeField] public MedicalSupplyData SupplyData { get; private set; }

        [SerializeField] int _startingItems = 5;
        [field: ShowNonSerializedField] public int ItemsLeft { get; private set; }

        void Start()
        {
            ItemsLeft = _startingItems;
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void InitializeRpc(int supplyId)
        {
            Initialize(MedicalSupplyDatabase.Instance.GetData(supplyId));
        }

        public void Initialize(MedicalSupplyData data)
        {
            SupplyData = data;
            // TODO: update texture image
        }

        public override void Interact(GameObject player)
        {
            var plInter = player.GetComponent<Interaction>();
            if (IsCarrying || plInter.CarriedItem != null) return;
            SpawnSupplyObjRpc(NetworkManager.LocalClientId);
        }
        protected override void OnDrop()
        {
            DisableSomeShitRpc();
        }

        [Rpc(SendTo.Everyone)]
        void DisableSomeShitRpc()
        {
            GetComponent<Rigidbody>().isKinematic = true;
            // enable clicks
            Interaction |= InteractionType.Click;
        }

        protected override void OnUpdate()
        {

        }

        [Rpc(SendTo.Server)]
        void SpawnSupplyObjRpc(ulong playerId, RpcParams rpcParams = default)
        {
            var plInter = NetworkManager.Singleton
                            .ConnectedClients[playerId]
                            .PlayerObject
                            .GetComponent<Interaction>();

            var supplyWorldObj = NetworkObjectPool.Singleton.GetNetworkObject(
                PrefabDatabase.Instance.CommonPrefabs[PrefabType.Supply],
                transform.position,
                Quaternion.identity
            );
            if (!supplyWorldObj.IsSpawned)
                supplyWorldObj.Spawn();
            supplyWorldObj.GetComponent<MedicalSupply>().Cl_InitializeRpc(SupplyData.ID);

            plInter.Cl_CarryItemRpc(supplyWorldObj.NetworkObjectId, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));

            ItemsLeft = Mathf.Clamp(ItemsLeft - 1, 0, _startingItems);
            if (ItemsLeft == 0)
            {
                // tell drop zone "you no longer have an item on you"
                Owner.GetComponent<MedicalSupplyTable>().RemoveItem();

                transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    NetworkObjectPool.Singleton.ReturnNetworkObject(
                        GetComponent<NetworkObject>(),
                        PrefabDatabase.Instance.CommonPrefabs[PrefabType.SupplyBox]
                    );
                });
            }

        }

        public override string ToString() => SupplyData.Name;
    }
}
