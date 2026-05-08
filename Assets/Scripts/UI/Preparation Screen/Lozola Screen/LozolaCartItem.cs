using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class LozolaCartItem : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI _supplyName;
        [SerializeField] private TextMeshProUGUI _supplyPrice;
        [SerializeField] private TMP_InputField _supplyQuantity;
        [SerializeField] private Button _minus;
        [SerializeField] private Button _add;
        [SerializeField] private int _quantity = 0;
        private int _supplyId;

        private void Awake()
        {
            _minus.onClick.AddListener(() => Sv_RemoveQuantityRpc());
            _add.onClick.AddListener(() => Sv_AddQuantityRpc());
        }

        [Rpc(SendTo.Server)]
        private void Sv_AddQuantityRpc()
        {
            Cl_UpdateCartItemQuantityRpc(++_quantity);
            UIManager.Instance.PreparationScreen.LozolaScreen.LozolaCart.UpdateTotalPrice();
        }

        [Rpc(SendTo.Server)]
        private void Sv_RemoveQuantityRpc()
        {
            Cl_UpdateCartItemQuantityRpc(--_quantity);
            UIManager.Instance.PreparationScreen.LozolaScreen.LozolaCart.UpdateTotalPrice();

            if (_quantity > 0) return;

            UIManager.Instance.PreparationScreen.LozolaScreen.LozolaCart.RemoveCartItem(GetSupplyId());
            Destroy(gameObject);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_UpdateCartItemQuantityRpc(int quantity)
        {
            _supplyQuantity.text = quantity.ToString();
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_SetLozolaCartItemRpc(int supplyId, string supplyName, string supplyPrice)
        {
            Transform lozolaCartContent = UIManager.Instance.PreparationScreen.LozolaScreen.LozolaCart.LozolaCartContent;
            transform.SetParent(lozolaCartContent, false);

            _supplyId = supplyId;
            _supplyName.text = supplyName;
            _supplyPrice.text = supplyPrice;
            name = supplyName;

            if (!IsServer) return;
            Sv_AddQuantityRpc();
        }

        public float GetCartItemPrice()
        {
            if (!float.TryParse(_supplyPrice.text.Replace("$", ""), out float supplyPrice)) return 0f;
            return supplyPrice;
        }

        public int GetCartItemQuantity()
        {
            if (!int.TryParse(_supplyQuantity.text, out int supplyQuantity)) return 0;
            return supplyQuantity;
        }

        public int GetSupplyId()
        {
            return _supplyId;
        }
    }
}