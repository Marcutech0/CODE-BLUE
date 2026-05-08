using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class LozolaShop : NetworkBehaviour
    {
        [SerializeField] private GameObject _lozolaShopItemPrefab;
        [SerializeField] private Transform _lozolaShopContent;
        [SerializeField] private List<LozolaShopItem> _shop = new();
        private string _filter = "", _query = "";

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_AddShopItemRpc(int supplyId)
        {
            MedicalSupplyData medicalSupply = MedicalSupplyDatabase.Instance.GetData(supplyId);
            LozolaShopItem lozolaShopItem = Instantiate(_lozolaShopItemPrefab, _lozolaShopContent).GetComponent<LozolaShopItem>();
            lozolaShopItem.SetLozolaShopItem(medicalSupply.ID, medicalSupply.Location, medicalSupply.UITexture, medicalSupply.Name, medicalSupply.Cost.ToString());

            _shop.Add(lozolaShopItem);
        }

        public void FilterShopItems(string filter)
        {
            _filter = filter;
            FilterAndSearchShopItems();
        }

        public void SearchShopItems(string query)
        {
            _query = query;
            FilterAndSearchShopItems();
        }

        private void FilterAndSearchShopItems()
        {
            foreach (LozolaShopItem lozolaShopItem in _shop)
            {
                bool matchesQuery = lozolaShopItem.GetSupplyName().Contains(_query);
                bool matchesFilter = _filter == "All" || lozolaShopItem.GetSupplyLocation().ToString() == _filter.Replace(" ", "");

                lozolaShopItem.gameObject.SetActive(matchesQuery && matchesFilter);
            }
        }
    }
}