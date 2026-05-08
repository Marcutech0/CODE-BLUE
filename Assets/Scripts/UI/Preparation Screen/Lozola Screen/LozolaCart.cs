using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class LozolaCart : NetworkBehaviour
    {
        [SerializeField] private GameObject _lozolaCartItemPrefab;
        [field: SerializeField] public Transform LozolaCartContent { private set; get; }
        [SerializeField] private TextMeshProUGUI _lozolaCartTotalPrice;
        [SerializeField] private Button _purchase;
        [SerializeField] private List<LozolaCartItem> _cart = new();
        [SerializeField] private float _totalPrice;
        private List<int> _supplyIds = new();

        private void Awake()
        {
            _purchase.onClick.AddListener(() => Sv_PurchaseRpc());
        }

        [Rpc(SendTo.Server)]
        private void Sv_PurchaseRpc()
        {
            int[] supplyIds = _cart.Select(cartItem => cartItem.GetSupplyId()).ToArray();

            MedicalSupplyBoxSpawner.Instance.Sv_SpawnBoxesRpc(supplyIds);

            SharedEconomy.Instance.Sv_AddSalaryRpc(-_totalPrice);

            foreach (LozolaCartItem cartItem in _cart.ToList())
            {
                RemoveCartItem(cartItem.GetSupplyId());
                Destroy(cartItem.gameObject);
            }
        }

        public void RemoveCartItem(int supplyid)
        {
            _supplyIds.Clear();
            _supplyIds = _cart.Select(cartitem => cartitem.GetSupplyId()).ToList();

            if (!_supplyIds.Contains(supplyid)) return;

            _supplyIds.Remove(supplyid);
            LozolaCartItem lozolaCartItem = _cart.First(cartItem => cartItem.GetSupplyId() == supplyid);
            _cart.Remove(lozolaCartItem);
            lozolaCartItem.GetComponent<NetworkObject>().Despawn();

            UpdateTotalPrice();
        }

        [Rpc(SendTo.Server)]
        public void Sv_AddCartItemRpc(int supplyId, string supplyName, string supplyCost)
        {
            _supplyIds.Clear();
            _supplyIds = _cart.Select(cartItem => cartItem.GetSupplyId()).ToList();

            if (_supplyIds.Contains(supplyId)) return;

            LozolaCartItem lozolaCartItem = Instantiate(_lozolaCartItemPrefab).GetComponent<LozolaCartItem>();
            lozolaCartItem.GetComponent<NetworkObject>().Spawn();
            lozolaCartItem.Cl_SetLozolaCartItemRpc(supplyId, supplyName, supplyCost);

            _cart.Add(lozolaCartItem);
            UpdateTotalPrice();
        }

        public void UpdateTotalPrice()
        {
            _totalPrice = 0;

            foreach (LozolaCartItem lozolaCartItem in _cart)
            {
                _totalPrice += lozolaCartItem.GetCartItemPrice() * lozolaCartItem.GetCartItemQuantity();
            }

            Cl_SetTotalPriceRpc(_totalPrice);
            SetPurchaseInteractability();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_SetTotalPriceRpc(float totalPrice)
        {
            _lozolaCartTotalPrice.text = $"${totalPrice}";
        }

        private void SetPurchaseInteractability()
        {
            Cl_SetPurchaseInteractabilityeRpc(
                _totalPrice <= SharedEconomy.Instance.CurrentSalary.Value &&
                _cart.Count >= 1
            );
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_SetPurchaseInteractabilityeRpc(bool isInteractable)
        {
            _purchase.interactable = isInteractable;
        }
    }
}
