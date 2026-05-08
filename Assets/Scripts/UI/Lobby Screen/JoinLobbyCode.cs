using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class JoinLobbyCode : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI _joinCodeText;

        public void UpdateJoinLobbyCode(string lobbyCode)
        {
            _joinCodeText.text = lobbyCode;
        }
    }
}