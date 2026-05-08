using UnityEngine;
using Unity.Netcode;
using NaughtyAttributes;

namespace CodeBlue
{
    public class OpenSign : BaseInteractable
    {
        [field: ReadOnly] public NetworkVariable<bool> IsOpen { get; set; } = new();

        public override void OnNetworkSpawn()
        {
            GameManager.Instance.OnPhaseChanged.AddListener(ReopenOnNewDay);
        }

        private void ReopenOnNewDay(GamePhase phase)
        {
            if (phase is GamePhase.Lobby || phase is GamePhase.Card)
                Sv_SetOpenRpc(false);
        }

        public override void InteractHold(GameObject interactedBy)
        {
            Sv_SetOpenRpc(!IsOpen.Value);
        }

        [Rpc(SendTo.Server)]
        public void Sv_SetOpenRpc(bool open)
        {
            IsOpen.Value = open;
            IsInteractionDisabled.Value = open;

            if (open)
                GameManager.Instance.Cl_ChangeGameStateRpc(GamePhase.Work);

            Cl_DisableRendererRpc(open);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void Cl_DisableRendererRpc(bool isOpen)
        {
            GetComponent<Collider>().enabled = !isOpen;
            GetComponent<Renderer>().enabled = !isOpen;
        }
    }
}
