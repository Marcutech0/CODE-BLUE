using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using System;

namespace CodeBlue
{
    public class CameraFollow : SingletonBehaviour<CameraFollow>
    {
        [SerializeField]
        CinemachineTargetGroup _targetGroup;

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_AddPlayerToCamGroupRpc(ulong playerId)
        {
            _targetGroup.Targets.Clear();

            foreach (var clients in NetworkManager.ConnectedClients.Values)
            {
                _targetGroup.AddMember(clients.PlayerObject.transform, 0.5f, 1);
            }
        }


        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_RemovePlayerToCamGroupRpc(ulong playerId)
        {
            var pl = NetworkManager.ConnectedClients[playerId].PlayerObject;
            _targetGroup.RemoveMember(pl.transform);
        }
    }
}
