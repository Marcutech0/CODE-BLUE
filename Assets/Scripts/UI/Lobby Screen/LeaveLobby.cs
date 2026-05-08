using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class LeaveLobby : NetworkBehaviour
    {
        [SerializeField] private Button _leaveLobbyButton;

        void Start()
        {
            _leaveLobbyButton.onClick.AddListener(RemoveClient);
        }

        public void SetLeaveInteractability(bool isInteractable)
        {
            _leaveLobbyButton.interactable = isInteractable;
        }

        public void RemoveClient()
        {
            if (IsHost)
                Cl_RemoveAllClientRpc();
            else if (IsClient)
                Sv_RemoveClientRpc(NetworkManager.Singleton.LocalClientId);

            UIManager.Instance.LoadScreen(UIScreenType.MainMenu);
        }

        [Rpc(SendTo.Server)]
        private void Sv_RemoveClientRpc(ulong clientID)
        {
            if (clientID == NetworkManager.Singleton.LocalClientId)
                NetworkManager.Singleton.Shutdown();
            else
                NetworkManager.Singleton.DisconnectClient(clientID);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_RemoveAllClientRpc()
        {
            Sv_RemoveClientRpc(NetworkManager.Singleton.LocalClientId);
            UIManager.Instance.LoadScreen(UIScreenType.MainMenu);
        }
    }
}
