using UnityEngine;

namespace CodeBlue
{
    public class LobbyScreen : UIScreenBase
    {
        [field: Header("Functionality")]
        [field: SerializeField] public LeaveLobby LeaveLobby { private set; get; }
        [field: SerializeField] public JoinLobbyCode JoinLobbyCode { private set; get; }
        [field: SerializeField] public PlayerCount PlayerCount { private set; get; }
        [field: SerializeField] public ReadyCount ReadyCount { private set; get; }

        public override void SetScreenVisibility(bool isVisible)
        {
            base.SetScreenVisibility(isVisible);
            ReadyCount.SetVisibility(false);
        }
    }
}
