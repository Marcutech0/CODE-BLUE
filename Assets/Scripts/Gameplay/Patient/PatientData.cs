using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class PatientData : NetworkBehaviour
    {
        public NetworkVariable<int> ID = new();
        [field: SerializeField] public MedicalCaseData MedicalCase { private set; get; }
        public NetworkList<int> TreatmentsLeft = new();
        public List<int> UsedTreatments = new();

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_SetMedicalCaseRpc(int medicalCaseID)
        {
            MedicalCase = MedicalCaseDatabase.Instance.GetData(medicalCaseID);
        }

        public void UpdateTreatmentPlan()
        {
            foreach (int t in MedicalCase.TreatmentPlan)
                TreatmentsLeft.Add(t);
        }

        public bool TryTreatment(int supplyId)
        {
            if (TreatmentsLeft.Contains(supplyId))
            {
                Sv_TreatRpc(supplyId);
                return true;
            }
            return false;
        }

        [Rpc(SendTo.Server)]
        void Sv_TreatRpc(int supplyId) 
        {
            UsedTreatments.Add(supplyId);
            TreatmentsLeft.Remove(supplyId);

            // NetworkList does not have Linq support....
            // TODO: write Linq functions for NetworkList
            var treatmentsLeft = new int[TreatmentsLeft.Count];
            for (int i = 0; i < treatmentsLeft.Length; i++)
                treatmentsLeft[i] = TreatmentsLeft[i];

            GetComponent<PatientDiagnosisUI>().Cl_UpdateTreatmentPlanUIRpc(MedicalCase.ID, treatmentsLeft);
            GetComponent<PatientPatience>().Sv_ResetPatienceRpc();
            GetComponent<PatientPatience>().Cl_SetPatienceRpc();
        }
    }
}