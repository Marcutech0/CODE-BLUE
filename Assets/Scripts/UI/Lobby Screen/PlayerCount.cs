using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class PlayerCount : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerCountText;

        [SerializeField] private int _playerCount;

        private List<ulong> _playerIds = new();

        private void UpdatePlayerList()
        {
            if (NetworkManager.Singleton == null) return;
            
            List<ulong> playerIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();

            if (playerIds.SequenceEqual(_playerIds)) return;

            _playerIds = playerIds;
            _playerCount = _playerIds.Count;

            UIManager.Instance.LobbyScreen.ReadyCount.UpdatePlayerReadyStates(_playerIds);
            UIManager.Instance.CardSelectionScreen.UpdatePlayerSelectedCards(_playerIds);

            Cl_UpdatePlayerCountRpc(_playerCount);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_UpdatePlayerCountRpc(int playerCount)
        {
            _playerCountText.text = new string('+', playerCount);
        }

        public int GetPlayerCount()
        {
            return _playerCount;
        }

        void LateUpdate()
        {
            if (!IsServer) return;

            UpdatePlayerList();
        }
    }
}