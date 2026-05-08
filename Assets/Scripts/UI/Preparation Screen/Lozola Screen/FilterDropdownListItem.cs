using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class FilterDropdownListItem : MonoBehaviour
    {
        [SerializeField] private Toggle _filterItemToggle;
        [SerializeField] private TextMeshProUGUI _filterItemText;

        private void Awake()
        {
            _filterItemToggle.onValueChanged.AddListener(SetFilterDropdownText);
        }

        private void SetFilterDropdownText(bool isOn)
        {
            if (!isOn) return;

            UIManager.Instance.PreparationScreen.LozolaScreen.Filter.ChangeFilterDropdownText(_filterItemText.text.ToString());
        }
    }
}