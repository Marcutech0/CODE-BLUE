using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class CardData : NetworkBehaviour
    {
        [Header("Card Data")]
        [SerializeField] private DrawnedCardData _cardData;

        [Header("Card UI")]
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private TextMeshProUGUI _description;

        [Header("Medical Card")]
        [SerializeField] private GameObject _medicalCaseGroup;
        [SerializeField] private Transform _treatmentPlanGroup;
        [SerializeField] private TextMeshProUGUI _severityLevel;
        [SerializeField] private Image _departmentImage;

        [Header("Department Card")]
        [SerializeField] private GameObject _deptCardGroup; // dc for department card
        [SerializeField] private Image _deptCardDepartmentIcon;

        [Header("Game Event")]
        [SerializeField] private GameObject _gameEventGroup;
        [SerializeField] private TextMeshProUGUI _gameEventDurationText;

        [SerializeField] private Image _image;

        public void SetCardData(DrawnedCardData drawnedCardData)
        {
            switch (drawnedCardData.CardType)
            {
                case CardType.MedicalCase:
                    MedicalCaseData medicalCase = MedicalCaseDatabase.Instance.GetData(drawnedCardData.DataID);
                    SetCardData(medicalCase, drawnedCardData.DataID);
                    break;
                case CardType.Department:
                    DepartmentData department = DepartmentDatabase.Instance.GetData(drawnedCardData.DataID);
                    SetCardData(department, drawnedCardData.DataID);
                    break;
                case CardType.GameEvent:
                    GameEventData gameEvent = GameEventDatabase.Instance.GetData(drawnedCardData.DataID);
                    SetCardData(gameEvent, drawnedCardData.DataID);
                    break;
            }
        }

        public void SetCardData(MedicalCaseData medicalCase, int dataID)
        {
            if (medicalCase == null) return;

            var durationText = $"Must be treated within {medicalCase.PatienceDuration} seconds";
            if (medicalCase.PatienceReset)
                durationText += ", but patience resets every treatment step";

            Cl_SetCardData_MedicalCaseRpc(
                medicalCase.Name,
                triageLevel: medicalCase.TriageLevel,
                description: durationText,
                dataID: dataID,
                department: medicalCase.RequiredDeparment
            );
        }

        public void SetCardData(DepartmentData department, int dataID)
        {
            if (department == null) return;
            Cl_SetCardData_DepartmentRpc(department.Name, department.Description, dataID, department.Department);
        }

        public void SetCardData(GameEventData gameEvent, int dataID)
        {
            if (gameEvent == null) return;
            Cl_SetCardData_GameEventRpc(gameEvent.Name, gameEvent.Description, gameEvent.DurationText, dataID);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_SetCardData_MedicalCaseRpc(string title, string description, int dataID, TriageLevel triageLevel, Department department)
        {
            SetCardBaseDetails(dataID, title, description, CardType.MedicalCase);
            _severityLevel.text = triageLevel.GetDescription();
            _severityLevel.color = triageLevel switch
            {
                TriageLevel.NonUrgent => new Color32(94, 172, 0, 255), // Color32 for 0-255 range
                TriageLevel.SemiUrgent => new Color32(255, 200, 37, 255),
                TriageLevel.Critical => Color.red,
                _ => Color.black
            };

            _departmentImage.sprite = DepartmentDatabase.Instance.GetDepartment(department).Icon;

            var treatmentsIdx = MedicalCaseDatabase.Instance.GetData(dataID).TreatmentPlan;
            for (int i = 0; i < treatmentsIdx.Length; i++)
            {
                var idx = treatmentsIdx[i];
                var treatmentData = MedicalSupplyDatabase.Instance.GetData(idx);

                var treatmentPlanUI = _treatmentPlanGroup.transform.GetChild(i);
                treatmentPlanUI.gameObject.SetActive(true);
                treatmentPlanUI.GetComponent<TreatmentPlanElementUI>().Init(treatmentData);
            }

        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_SetCardData_DepartmentRpc(string title, string description, int dataId, Department department)
        {
            SetCardBaseDetails(dataId, title, description, CardType.Department);
            _departmentImage.sprite = DepartmentDatabase.Instance.GetDepartment(department).Icon;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_SetCardData_GameEventRpc(string title, string description, string durationText, int dataId)
        {
            SetCardBaseDetails(dataId, title, description, CardType.GameEvent);
            _gameEventDurationText.text = durationText;
        }

        void SetCardBaseDetails(int dataID, string title, string description, CardType type)
        {
            _medicalCaseGroup.SetActive(type == CardType.MedicalCase);
            _deptCardGroup.SetActive(type == CardType.Department);
            _gameEventGroup.SetActive(type == CardType.GameEvent);

            _title.text = ExpandWord(title);
            _description.text = description;
            _cardData = new DrawnedCardData(type, dataID);
        }

        private string ExpandWord(string word)
        {
            return Regex.Replace(word, "(\\B[A-Z])", "-$1");
        }

        public DrawnedCardData GetCardData()
        {
            return _cardData;
        }
    }
}
