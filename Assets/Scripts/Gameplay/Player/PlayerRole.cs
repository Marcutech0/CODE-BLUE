using UnityEngine;
using Unity.Netcode;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeBlue
{
    [System.Flags]
    public enum PlayerRoles
    {
        Nurse = 1 << 1,
        Doctor = 1 << 2,
    }

    public class PlayerRole : NetworkBehaviour
    {
        [field: SerializeField] public NetworkVariable<PlayerRoles> CurrentRole { get; private set; } = new(PlayerRoles.Nurse);

        public override void OnNetworkSpawn()
        {
            Sv_SwitchRoleRpc(PlayerRoles.Nurse);
        }

        [Rpc(SendTo.Server)]
        public void Sv_SwitchRoleRpc(PlayerRoles role)
        {
            CurrentRole.Value = role;
            // TODO change appearance/costume
            Cl_UpdateMatColorRpc(role);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void Cl_UpdateMatColorRpc(PlayerRoles role)
        {
            var col = role switch
            {
                PlayerRoles.Nurse => Color.green,
                PlayerRoles.Doctor => Color.blue,
                _ => Color.red
            };
            GetComponent<Renderer>().material.color = col;
        }


#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Handles.Label(transform.position + Vector3.up, CurrentRole.Value.ToString());
        }
#endif
    }

}
