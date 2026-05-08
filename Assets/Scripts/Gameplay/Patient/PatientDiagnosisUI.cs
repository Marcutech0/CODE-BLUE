using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class PatientDiagnosisUI : NetworkBehaviour
    {
        [SerializeField] GameObject _diagnosisPanel;
        [SerializeField] TMPro.TextMeshProUGUI _caseText, _treatmentText;

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_UpdateTreatmentPlanUIRpc(int caseId, int[] treatmentsLeft)
        {
            _caseText.text = MedicalCaseDatabase.Instance.GetData(caseId).Name;
            var treatments = "";
            foreach (var idx in treatmentsLeft)
            {
                var treatment = MedicalSupplyDatabase.Instance.GetData(idx);
                treatments += treatment.Name + ", ";
            }
            _treatmentText.text = treatments;
            _diagnosisPanel.SetActive(true);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_CloseRpc() => _diagnosisPanel.SetActive(false);
    }
}