using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class LozolaShopItem : MonoBehaviour
    {
        [SerializeField] private Image _supplyIcon;
        [SerializeField] private TextMeshProUGUI _supplyName;
        [SerializeField] private TextMeshProUGUI _supplyPrice;
        [SerializeField] private Button _addToCart;
        private int _supplyId;
        private MedicalSupplyLocation _supplyLocation;

        private void Awake()
        {
            _addToCart.onClick.AddListener(() =>
                UIManager.Instance.PreparationScreen.LozolaScreen.LozolaCart.Sv_AddCartItemRpc(_supplyId, _supplyName.text, _supplyPrice.text)
            );
        }

        public void SetLozolaShopItem(int supplyId, MedicalSupplyLocation supplyLocation, Sprite supplyIcon, string supplyName, string supplyPrice)
        {
            _supplyId = supplyId;
            _supplyLocation = supplyLocation;
            _supplyIcon.sprite= supplyIcon;
            _supplyName.text = supplyName;
            _supplyPrice.text = $"${supplyPrice}";
            name = supplyName;
        }

        public string GetSupplyName()
        {
            return _supplyName.text;
        }

        public MedicalSupplyLocation GetSupplyLocation()
        {
            return _supplyLocation;
        }
    }
}