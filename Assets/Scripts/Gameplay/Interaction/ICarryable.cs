using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public interface ICarryable
    {
        public bool IsCarrying { get; set; }
        public Transform Owner { get; set; }
        public PrefabType PrefabType { get; set; }

        public void Carry(Transform owner);
        public GameObject Drop(Vector3 dropPos, Transform owner = null);
    }

    public abstract class Carryable : BaseInteractable, ICarryable
    {
        public bool IsCarrying { get; set; }
        public Transform Owner { get; set; }
        [field: SerializeField] public PrefabType PrefabType { get; set; }

        public void Carry(Transform owner)
        {
            if (Owner != null && Owner.TryGetComponent(out MedicalSupplyTable table))
                table.RemoveItem();

            if (transform.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            Owner = owner;

            transform.position = owner.transform.position;
            Sv_SetOwnershipRpc(NetworkManager.LocalClientId);
            IsCarrying = true;

            Cl_SetColliderStateRpc(false);
        }

        public GameObject Drop(Vector3 dropPos, Transform owner = null)
        {
            transform.position = dropPos;
            if (transform.TryGetComponent(out Rigidbody rb))
            {
                rb.position = dropPos;
                rb.isKinematic = false;
            }
            transform.rotation = Quaternion.identity;
            IsCarrying = false;
            Owner = owner;

            Cl_SetColliderStateRpc(true);
            Sv_SetOwnershipRpc(NetworkManager.ServerClientId);
            GetComponent<Collider>().enabled = true;

            OnDrop();
            return gameObject;
        }

        public override void InteractHold(GameObject interactedBy)
        {
            Sv_CarryRpc(NetworkManager.LocalClientId);
        }

        [Rpc(SendTo.Server)]
        void Sv_CarryRpc(ulong playerId, RpcParams rpcParams = default)
        {
            var player = NetworkManager
                .ConnectedClients[playerId]
                .PlayerObject
                .GetComponent<Interaction>();

            player.Cl_CarryItemRpc(NetworkObjectId, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        void Sv_SetOwnershipRpc(ulong newOwnerId) =>
            GetComponent<NetworkObject>().ChangeOwnership(newOwnerId);

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        void Cl_SetColliderStateRpc(bool state)
        {
            transform.GetComponent<Collider>().enabled = state;
        }

        protected abstract void OnUpdate();

        protected void UpdatePosition()
        {
            if (!IsCarrying) return;

            transform.position = Owner.position + Owner.forward + Vector3.up;
            transform.forward = Owner.forward;
        }

        void Update()
        {
            UpdatePosition();
        }

        // FIXME: bandaid
        protected virtual void OnDrop() { }
    }
}
