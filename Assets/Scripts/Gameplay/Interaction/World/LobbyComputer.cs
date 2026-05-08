using UnityEngine;

namespace CodeBlue
{
    public class LobbyComputer : BaseInteractable
    {
        public override void InteractHold(GameObject interactedBy)
        {
            PatientQueue.Instance.Sv_CheckInPatientRpc();
        }
    }
}


