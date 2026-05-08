using Unity.Netcode;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;

namespace CodeBlue
{
    public class LockerRoom : SingletonBehaviour<LockerRoom>
    {
        [SerializeField] Locker[] _lockers;

        public Locker ClaimAvailableLocker(ulong playerId)
        {
            var available = _lockers.Where(l => l.PlayerOwnerId.Value == 99).FirstOrDefault();
            Assert.IsNotNull(available, "There are no more available lockers! Have reached max lobby capacity");

            available.Sv_SetOwnerRpc(playerId);

            return available;
        }
    }
}
