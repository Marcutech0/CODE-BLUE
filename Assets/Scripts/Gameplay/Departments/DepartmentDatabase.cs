using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace CodeBlue
{
    public class DepartmentDatabase : SingletonBehaviourNonNetworked<DepartmentDatabase>
    {
        [Header("Preload Data")]
        [SerializeField] private DepartmentData[] _preloadData;
        [field: SerializeField] public DepartmentData ConsultationRoom { get; private set; }

        [Header("Database")]
        [SerializeField, SerializedDictionary("Department", "Card State")]
        private SerializedDictionary<DepartmentData, CardState> _departments = new();

        private void Start()
        {
            LoadPreloadDepartments();
        }

        private void LoadPreloadDepartments()
        {
            foreach (var department in _preloadData)
            {
                var available = department.Department != Department.ConsultationRoom;
                _departments.Add(department, available ? CardState.Available : CardState.Drawn);

                if (department.Department == Department.ConsultationRoom)
                    ConsultationRoom = department;
            }
        }

        public DrawnedCardData GetRandomDataID()
        {
            List<DepartmentData> departments = _departments.Where(pair => pair.Value == CardState.Available && pair.Key.Department == Department.OperatingRoom).Select(pair => pair.Key).ToList();

            if (departments.Count == 0) return new();

            DepartmentData department = departments.SelectRandom();
            _departments[department] = CardState.Drawn;

            return new(CardType.Department, department.ID);
        }

        public DepartmentData GetDepartment(Department department)
        {
            return _departments.FirstOrDefault(p => p.Key.Department == department).Key;
        }

        public int GetDataID(string name)
        {
            return _departments.FirstOrDefault(pair => pair.Key.Name == name).Key.ID;
        }

        public DepartmentData GetData(int id)
        {
            return _departments.FirstOrDefault(pair => pair.Key.ID == id).Key;
        }

        public void DataSelection(int id, CardState cardState)
        {
            DepartmentData department = _departments.FirstOrDefault(pair => pair.Key.ID == id).Key;
            _departments[department] = cardState;
        }

        public void ResetData()
        {
            foreach (var deparment in _departments.Keys)
            {
                _departments[deparment] = CardState.Available;
            }
        }
    }
}