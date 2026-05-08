using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class Filter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _filterDropdownText;
        [SerializeField] private Toggle _filterDropdownToggle;
        [SerializeField] private Image _filterDropdownToggleBackground;
        [SerializeField] private Transform _filterDropdownToggleArrow;
        [SerializeField] private GameObject _filterDropdownList;

        private Sequence sequence;

        private void Awake()
        {
           _filterDropdownToggle.onValueChanged.AddListener(ShowFilterDropdownList);
        }

        private void Start()
        {
            ShowFilterDropdownList(false);
        }

        private void ShowFilterDropdownList(bool isOn)
        {
            sequence = DOTween.Sequence();

            _filterDropdownList.SetActive(isOn);

            sequence.Append(_filterDropdownToggleBackground.DOColor(isOn ? Color.black : Color.white, .5f));
            sequence.Join(_filterDropdownToggleArrow.DOLocalRotate(Vector3.forward * (isOn ? 180f : 360f), .5f));
        }

        public void ChangeFilterDropdownText(string value)
        {
            _filterDropdownText.text = value;
            _filterDropdownToggle.isOn = false;
            UIManager.Instance.PreparationScreen.LozolaScreen.LozolaShop.FilterShopItems(_filterDropdownText.text);
        }
    }
}