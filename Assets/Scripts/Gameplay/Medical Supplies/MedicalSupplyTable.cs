using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class MedicalSupplyTable : BaseInteractable
    {
        public NetworkVariable<bool> HasObject { get; private set; } = new();
        private Collider _collider;

        [SerializeField] Image _supplyIconUI;

        void Start()
        {
            _collider = GetComponent<Collider>();
        }

        public override void Interact(GameObject by)
        {
            /*if (HasObject.Value) return;*/

            var player = by.GetComponent<Interaction>();
            if (player.CarriedItem)
            {
                var suppId = player.CarriedItem.TryGetComponent(out MedicalSupplyBox supp) ? supp.SupplyData.ID : player.CarriedItem.GetComponent<MedicalSupply>().Data.ID; 
                Cl_UpdateSupplyIconRpc(suppId);

                Sv_DropItemRpc(NetworkManager.Singleton.LocalClientId);
                Sv_SetInteractionStatusRpc(true);
                Cl_HideDatShitRpc(true);
            }
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        void Sv_DropItemRpc(ulong playerId, RpcParams rpcParams = default)
        {
            var player = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<Interaction>();
            player.Cl_DropItemAtTransformRpc(NetworkObjectId, offset: Vector3.zero, rpcParams: RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.Server)]
        void Sv_SetInteractionStatusRpc(bool status)
        {
            HasObject.Value = status;
            IsInteractionDisabled.Value = status;
        }

        public void RemoveItem()
        {
            Cl_UpdateSupplyIconRpc();
            Sv_SetInteractionStatusRpc(false);
            Cl_HideDatShitRpc(false); 
        }

        [Rpc(SendTo.ClientsAndHost)]
        void Cl_HideDatShitRpc(bool hidden) => _collider.enabled = !hidden;  

        [Rpc(SendTo.ClientsAndHost)]
        void Cl_UpdateSupplyIconRpc(int supplyId = -1)
        {
            _supplyIconUI.gameObject.SetActive(supplyId > -1);

            if (supplyId == -1) 
                return; 

            var supplyImage = MedicalSupplyDatabase.Instance.GetData(supplyId);
            _supplyIconUI.sprite = supplyImage.UITexture;
        }
    }
}
