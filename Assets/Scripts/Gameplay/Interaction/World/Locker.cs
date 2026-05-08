using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class Locker : BaseInteractable
    {
        public NetworkVariable<ulong> PlayerOwnerId = new(99); // 0 is server

        [field: SerializeField] public Transform SpawnPoint { get; set; }

        public override void InteractHold(GameObject by)
        {
            if (!by.TryGetComponent(out Player player))
                return;

            var role = by.GetComponent<PlayerRole>().CurrentRole.Value;
            // toggle between roles
            role ^= (PlayerRoles.Nurse | PlayerRoles.Doctor);

            AudioManager.Instance.PlayGameSfx("switch-roles");
            Sv_SwitchRoleRpc(by.GetComponent<NetworkObject>().OwnerClientId, role);
        }

        [Rpc(SendTo.Server)]
        public void Sv_SetOwnerRpc(ulong playerId) => PlayerOwnerId.Value = playerId;

        [Rpc(SendTo.Server)]
        void Sv_SwitchRoleRpc(ulong playerId, PlayerRoles next)
        {
            var player = NetworkManager.ConnectedClients[playerId];
            if (PlayerOwnerId.Value != playerId) return;

            player.PlayerObject.GetComponent<PlayerRole>().Sv_SwitchRoleRpc(next);
        }

    }
}
