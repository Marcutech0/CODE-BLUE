using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    [System.Serializable]
    public struct PatientClipboardInfo : INetworkSerializable
    {
        public string PatientName;
        public string Department;
        public string MedicalCaseName;
        public string PatientTriageLevel;
        public Color PatientTriageLevelColor;
        public int[] TreatmentPlan;
        public int[] UsedTreatments;
            
        public PatientClipboardInfo(string patientName, string department, string medicalCaseName, string patientTriageLevel, Color patientTriageLevelColor, int[] treatmentPlan, int[] usedTreatments)
        {
            PatientName = patientName;
            Department = department;
            MedicalCaseName = medicalCaseName;
            PatientTriageLevel = patientTriageLevel;
            PatientTriageLevelColor = patientTriageLevelColor;
            TreatmentPlan = treatmentPlan;
            UsedTreatments = usedTreatments;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PatientName);
            serializer.SerializeValue(ref Department);
            serializer.SerializeValue(ref MedicalCaseName);
            serializer.SerializeValue(ref PatientTriageLevel);
            serializer.SerializeValue(ref PatientTriageLevelColor);
            serializer.SerializeValue(ref TreatmentPlan);
            serializer.SerializeValue(ref UsedTreatments);
        }
    }
        
    public class PatientClipboard : NetworkBehaviour
    {
        // [Header("Components")]
        // [SerializeField] private TextMeshProUGUI _patientName;
        // [SerializeField] private TextMeshProUGUI _patientMedicalCase;
        // [SerializeField] private TextMeshProUGUI _patientTriageLevel;
        // [SerializeField] private TextMeshProUGUI _patientAdmittedTo;
        // [SerializeField] private TextMeshProUGUI _patientStatus;
        // [SerializeField] private List<Image> _patientTreatmentPlan;
        // [SerializeField] private List<RectTransform> _patientTreatmentPlanCover;
        // [SerializeField] private Button _previousPatientId;
        // [SerializeField] private Button _nextPatientId;

        // [Header("Attributes")]
        // [SerializeField] private float _animationDuration = 0.5f;

        // private int _patientId = 0;
        // private Sequence _sequence;

        // private void Awake()
        // {
        //     _previousPatientId.onClick.AddListener(PreviousPatientId);
        //     _nextPatientId.onClick.AddListener(NextPatientId);
        // }

        // private void PreviousPatientId()
        // {
        //     RpcParams rpcParams = new RpcParams
        //     {
        //         Receive = new RpcReceiveParams { SenderClientId = NetworkManager.Singleton.LocalClientId }
        //     };

        //     Sv_UpdatePatientIdRpc(--_patientId, rpcParams);
        // }

        // private void NextPatientId()
        // {
        //     RpcParams rpcParams = new RpcParams
        //     {
        //         Receive = new RpcReceiveParams { SenderClientId = NetworkManager.Singleton.LocalClientId }
        //     };

        //     Sv_UpdatePatientIdRpc(++_patientId, rpcParams);
        // }

        // [Rpc(SendTo.Server)]
        // private void Sv_UpdatePatientIdRpc(int patientId, RpcParams rpcParams)
        // {
        //     var minMax = GetPatientIdMinMax();
        //     int clampedPatientId = Mathf.Clamp(patientId, minMax.Item1, minMax.Item2);

        //     var patientIdOverflow = CheckPatientIdOverflow(patientId);
        //     return (clampedPatientId, patientIdOverflow.Item1, patientIdOverflow.Item2);
        //     Cl_UpdatePatientIdRpc(results.Item1, results.Item2, results.Item3, rpcParams);
        // }

        // [Rpc(SendTo.SpecifiedInParams)]
        // private void Cl_UpdatePatientIdRpc(int patientid, bool isMin, bool isMax, RpcParams rpcParams)
        // {
        //     _patientId = patientid;
        //     UpdateMinMaxButtonInteractability(isMin, isMax);
        //     UpdatePatientClipboardInfo();
        // }

        // private void UpdateMinMaxButtonInteractability(bool isMin, bool isMax)
        // {
        //     _previousPatientId.interactable = isMin;
        //     _nextPatientId.interactable = isMax;
        // }

        // private (int, int) GetPatientIdMinMax()
        // {
        //     var patients = PatientSpawner.Instance.Patients.ToArray();

        //     if (patients.Length == 0) return (int.MaxValue, int.MinValue);

        //     int minPatientId = int.MaxValue;
        //     int maxPatientId = int.MinValue;

        //     foreach (var patient in patients)
        //     {
        //         int patientId = patient.GetComponent<PatientData>().ID.Value;
        //         minPatientId = Mathf.Min(minPatientId, patientId);
        //         maxPatientId = Mathf.Max(maxPatientId, patientId);
        //     }

        //     return (minPatientId, maxPatientId);
        // }

        // private (bool, bool) CheckPatientIdOverflow(int patientId)
        // {
        //     var minMax = GetPatientIdMinMax();

        //     return (patientId <= minMax.Item1, patientId >= minMax.Item2);
        // }

        // private (int, bool, bool) ClampPatientIdAndCheckPatinietIdOverFlow(int patientId)
        // {
        //     var minMax = GetPatientIdMinMax();
        //     int clampedPatientId = Mathf.Clamp(patientId, minMax.Item1, minMax.Item2);

        //     var patientIdOverflow = CheckPatientIdOverflow(patientId);
        //     return (clampedPatientId, patientIdOverflow.Item1, patientIdOverflow.Item2);
        // }

        // private void Start()
        // {
        //     ResetPatientClipboardInfo();
        // }

        // public void ResetPatientClipboardInfo()
        // {
        //     _patientName.text = "";
        //     _patientMedicalCase.text = "";
        //     _patientTriageLevel.text = "";
        //     _patientTriageLevel.color = Color.white;
        //     _patientAdmittedTo.text = "";
        //     _patientStatus.text = "";
        //     _patientStatus.color = new(0f, 0f, 0f, 0f);
        //     for (int i = 0; i < _patientTreatmentPlan.Count; i++)
        //     {
        //         _patientTreatmentPlan[i].sprite = null;
        //         _patientTreatmentPlanCover[i].localScale = Vector3.zero;
        //     }
        // }

        // [Rpc(SendTo.ClientsAndHost)]
        // public void Cl_ResetAllPatientClipboardInfoRpc()
        // {
        //     ResetPatientClipboardInfo();
        // }

        // [Rpc(SendTo.SpecifiedInParams)]
        // public void Cl_ResetPatientClipboardInfoRpc(RpcParams rpcParams)
        // {
        //     ResetPatientClipboardInfo();
        // }

        // public void UpdatePatientClipboardInfo()
        // {
        //     RpcParams rpcParams = new RpcParams
        //     {
        //         Receive = new RpcReceiveParams { SenderClientId = NetworkManager.Singleton.LocalClientId }
        //     };

        //     Sv_UpdatePatientClipboardInfoRpc(_patientId, rpcParams);
        // }

        // [Rpc(SendTo.Server)]
        // private void Sv_UpdatePatientClipboardInfoRpc(int patientId, RpcParams rpcParams)
        // {
        //     var patients = PatientSpawner.Instance.Patients;

        //     if (patients.Count <= 0)
        //     {
        //         Cl_ResetAllPatientClipboardInfoRpc();
        //         return;
        //     }

        //     var patient = patients.First(patient => patient.GetComponent<PatientData>().ID.Value == patientId);
        //     PatientData patientData = patient.GetComponent<PatientData>();
        //     PatientBehaviour patientBehaviour = patient.GetComponent<PatientBehaviour>();
        //     PatientState patientState = patientBehaviour.State;

        //     Cl_ResetPatientClipboardInfoRpc(RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));

        //     if (patientState == PatientState.Queue || patientState == PatientState.Waiting) return;

        //     MedicalCaseData medicalCaseData = patientData.MedicalCase;
        //     bool isCheckedIn = patientBehaviour.IsCheckedIn;
        //     bool isDiagnosed = patientBehaviour.IsDiagnosed;

        //     PatientClipboardInfo patientClipboardInfo = new (
        //         isCheckedIn || isDiagnosed ? patient.name : "",
        //         isCheckedIn || isDiagnosed ? ExpandWord(medicalCaseData.RequiredDeparment.ToString()) : "",
        //         isCheckedIn && isDiagnosed ? medicalCaseData.name : "",
        //         isCheckedIn && isDiagnosed ? ExpandWord(medicalCaseData.TriageLevel.ToString(), true) : "",
        //         isCheckedIn && isDiagnosed ? medicalCaseData.TriageLevelColor : Color.white,
        //         isCheckedIn && isDiagnosed ? medicalCaseData.TreatmentPlan : new int[] {},
        //         isCheckedIn && isDiagnosed ? patientData.UsedTreatments.ToArray() : new int[] {}
        //     );
            
        //     var results = CheckPatientIdOverflow(patientId);
        //     Cl_UpdatePatientClipboardInfoRpc(patientClipboardInfo, results.Item1, results.Item2, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
        // }

        // [Rpc(SendTo.SpecifiedInParams)]
        // private void Cl_UpdatePatientClipboardInfoRpc(PatientClipboardInfo patientClipboardInfo, bool isMin, bool isMax, RpcParams rpcParams)
        // {
        //     _patientName.text = patientClipboardInfo.PatientName;
        //     _patientMedicalCase.text = patientClipboardInfo.MedicalCaseName;
        //     _patientTriageLevel.text = patientClipboardInfo.PatientTriageLevel;
        //     _patientTriageLevel.color = patientClipboardInfo.PatientTriageLevelColor;
        //     _patientAdmittedTo.text = patientClipboardInfo.Department;
        //     for (int i = 0; i < patientClipboardInfo.TreatmentPlan.Length; i++)
        //     {
        //         int supplyId = patientClipboardInfo.TreatmentPlan[i];
        //         _patientTreatmentPlan[i].sprite = MedicalSupplyDatabase.Instance.GetData(supplyId).UITexture;
                
        //         if (!patientClipboardInfo.UsedTreatments.Contains(supplyId)) return;
        //         _patientTreatmentPlanCover[i].localScale = Vector3.one;
        //     }
        // }

        // private string ExpandWord(string word, bool withHypen = false)
        // {
        //     string spacer = withHypen ? "-" : " ";
        //     return Regex.Replace(word, "(\\B[A-Z])", $"{spacer}$1");
        // }
    }
}