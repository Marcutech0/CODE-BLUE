using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class Search : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _searchField;
        [SerializeField] private Button _searchButton;

        private void Start()
        {
            LozolaShop lozolaShop = UIManager.Instance.PreparationScreen.LozolaScreen.LozolaShop;
            _searchField.onValueChanged.AddListener(lozolaShop.SearchShopItems);
            _searchButton.onClick.AddListener(() => lozolaShop.SearchShopItems(_searchField.text));
        }
    }
}