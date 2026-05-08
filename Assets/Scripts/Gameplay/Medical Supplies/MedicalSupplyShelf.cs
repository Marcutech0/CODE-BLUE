using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    [RequireComponent(typeof(AudioSource))]
    public class MedicalSupplyShelf : BaseInteractable
    {
        [SerializeField] Dictionary<Transform, bool> _shelves = new();
        [SerializeField] MedicalSupplyData _supplyData;
        [SerializeField] Transform[] _slots;

        [Header("UI")]
        [SerializeField] AudioClip _insufficientBalanceSfx;

        AudioSource _audio;
        Interaction _player;

        void Awake()
        {
            _audio = GetComponent<AudioSource>();
        }

        public void InitializeShelf(MedicalSupplyData data)
        {
            _supplyData = data;
            // spawn dat shit
        }

        public override void Interact(GameObject by)
        {
            _player = by.GetComponent<Interaction>();
            if (_player.CarriedItem) return;

            if (SharedEconomy.Instance.CurrentSalary.Value < _supplyData.Cost)
            {
                // run audio client side
                SharedEconomy.Instance.Cl_InsufficientBalanceRpc();
                if (_insufficientBalanceSfx)
                    _audio.PlayOneShot(_insufficientBalanceSfx);
                return;
            }
            SpawnSupplyBoxRpc(NetworkManager.Singleton.LocalClientId);

            SharedEconomy.Instance.Sv_AddSalaryRpc(-_supplyData.Cost);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        void SpawnSupplyBoxRpc(ulong player, RpcParams rpcParams = default)
        {
            var box = NetworkObjectPool.Singleton.GetNetworkObject(PrefabDatabase.Instance.CommonPrefabs[PrefabType.SupplyBox], transform.position, Quaternion.identity);
            box.Spawn();

            box.GetComponent<MedicalSupplyBox>().Initialize(_supplyData);

            var pl = NetworkManager.Singleton.ConnectedClients[player];
            pl.PlayerObject.GetComponent<Interaction>().Cl_CarryItemRpc(box.NetworkObjectId, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        }
    }
}
