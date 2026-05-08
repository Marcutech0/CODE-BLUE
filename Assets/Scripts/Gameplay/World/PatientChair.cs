using Unity.Netcode;

namespace CodeBlue
{
    public class PatientChair : NetworkBehaviour
    {
        public NetworkVariable<ulong> PatientOwnerId = new();

        [Rpc(SendTo.Server)]
        public void Sv_ClaimChairRpc(ulong owner)
        {
            PatientOwnerId.Value = owner;
        }
    }
}
