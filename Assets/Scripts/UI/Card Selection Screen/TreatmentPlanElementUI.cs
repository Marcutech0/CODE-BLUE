using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class TreatmentPlanElementUI : MonoBehaviour
    {
        [SerializeField] Image _supplyIcon;
        [SerializeField] TextMeshProUGUI _supplyName;

        public void Init(MedicalSupplyData data)
        {
            _supplyIcon.sprite = data.UITexture;
            _supplyName.text = data.Name;
        }
    }
}
