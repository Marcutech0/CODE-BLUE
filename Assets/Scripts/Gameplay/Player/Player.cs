using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] Color[] _indicatorColors;

        [SerializeField] Renderer _idIndicatorRenderer;

        void Awake()
        {
            CvarRegistry.RegisterCommands(this);
        }

        [Tooltip("Disable when not owner")]
        [SerializeField] Behaviour[] _disableWhenNotOwner;
        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                foreach (var bev in _disableWhenNotOwner)
                    bev.enabled = false;
            }
        }

        protected override void OnNetworkPostSpawn() 
        { 
            _idIndicatorRenderer.material.color = _indicatorColors[OwnerClientId % 4];
            GroupCamera.Instance.AddTargetToList(transform);
        }

        public override void OnNetworkDespawn()
        {
            GroupCamera.Instance.RefreshTargets();
            base.OnNetworkDespawn();
        }

        #region cvars
        [ConFunc("give_item", "Gives item")]
        [Rpc(SendTo.Server)]
        void _GiveItemCvarRpc(ulong playerId, string itemName)
        {
            var supp = MedicalSupplyDatabase.Instance.GetData(itemName);
            var pref = PrefabDatabase.Instance.CommonPrefabs[PrefabType.Supply];

            var supplyInstance = Instantiate(NetworkManager.GetNetworkPrefabOverride(pref), transform.position, Quaternion.identity);
            var suppNetObj = supplyInstance.GetComponent<NetworkObject>();
            suppNetObj.Spawn();
            suppNetObj.GetComponent<MedicalSupply>().Cl_InitializeRpc(supp.ID);

            var player = NetworkManager.ConnectedClients[playerId].PlayerObject;

            player.GetComponent<Interaction>().CarryItem(suppNetObj.gameObject);
        }

        [ConFunc("give_box", "Gives supply box")]
        [Rpc(SendTo.Server)]
        [UsedImplicitly]
        private void CFUNC_GiveItemRpc(ulong playerId, string itemName)
        {
            var supp = MedicalSupplyDatabase.Instance.GetData(itemName);
            var pref = PrefabDatabase.Instance.CommonPrefabs[PrefabType.SupplyBox];

            var supplyInstance = Instantiate(NetworkManager.GetNetworkPrefabOverride(pref), transform.position, Quaternion.identity);
            var suppNetObj = supplyInstance.GetComponent<NetworkObject>();
            suppNetObj.GetComponent<MedicalSupplyBox>().Initialize(supp);
            suppNetObj.Spawn();

            var player = NetworkManager.ConnectedClients[playerId].PlayerObject;

            player.GetComponent<Interaction>().CarryItem(suppNetObj.gameObject);
        }
        #endregion
    }
}

