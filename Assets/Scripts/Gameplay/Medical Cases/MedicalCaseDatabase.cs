using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace CodeBlue
{
    // use descriptions for user-friendly string names
    public enum TriageLevel
    {
        [Description("Non-Urgent")]
        NonUrgent = 0,
        [Description("Semi-Urgent")]
        SemiUrgent = 1,
        [Description("Critical")]
        Critical = 2
    }

    public class MedicalCaseDatabase : SingletonBehaviourNonNetworked<MedicalCaseDatabase>
    {
        [Header("Preload Data")]
        [SerializeField] private MedicalCaseData[] _preloadData;
        [field: SerializeField, SerializedDictionary("Triage Level", "Color")]
        public SerializedDictionary<TriageLevel, Color> TriageLevelColors = new()
        {
            { TriageLevel.NonUrgent, Color.green},
            { TriageLevel.SemiUrgent, Color.yellow},
            { TriageLevel.Critical, Color.red},
        };

        [Header("Database")]
        [SerializeField, SerializedDictionary("Medical Case", "Card State")]
        private SerializedDictionary<MedicalCaseData, CardState> _medicalCases = new();

        private void Start()
        {
            LoadPreloadMedicalCases();
        }

        private void LoadPreloadMedicalCases()
        {
            foreach (var medicalCaseData in _preloadData)
            {
                _medicalCases.Add(medicalCaseData, CardState.Available);
            }
        }

        public DrawnedCardData GetRandomDataID()
        {
            int[] departmentIDs = LevelData.Instance.Departments.Select(department => department.DataID).ToArray();

            Department[] departments = departmentIDs.Select(deparmentID =>
            {
                if (deparmentID == 1)
                    return DepartmentDatabase.Instance.ConsultationRoom.Department;
                else
                    return DepartmentDatabase.Instance.GetData(deparmentID).Department;
            }).ToArray();

            List<MedicalCaseData> medicalCases = _medicalCases.Where(pair => departments.Contains(pair.Key.RequiredDeparment) && pair.Value == CardState.Available).Select(pair => pair.Key).ToList();

            if (medicalCases.Count == 0) return new();

            MedicalCaseData medicalCase = medicalCases.SelectRandom();
            _medicalCases[medicalCase] = CardState.Drawn;

            return new(CardType.MedicalCase, medicalCase.ID);
        }

        public int GetDataID(string name)
        {
            return _medicalCases.FirstOrDefault(pair => pair.Key.Name == name).Key.ID;
        }

        public MedicalCaseData GetData(int id)
        {
            return _medicalCases.FirstOrDefault(pair => pair.Key.ID == id).Key;
        }

        public void DataSelection(int id, CardState cardState)
        {
            MedicalCaseData medicalCase = _medicalCases.FirstOrDefault(pair => pair.Key.ID == id).Key;
            _medicalCases[medicalCase] = cardState;
        }

        public void ResetData()
        {
            foreach (var medicalCase in _medicalCases.Keys)
            {
                _medicalCases[medicalCase] = CardState.Available;
            }
        }
    }
}
