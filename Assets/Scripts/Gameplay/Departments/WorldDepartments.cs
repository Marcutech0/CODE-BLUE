using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace CodeBlue
{
    public class WorldDepartments : SingletonBehaviourNonNetworked<WorldDepartments>
    {
        [field: SerializeField, SerializedDictionary("Department Type", "Transform")] public SerializedDictionary<Department, Transform> Departments { get; private set; }

   }

    public static class DepartmentExtensions 
    {
        public static PatientChair GetChairInDepartment(this Transform t) 
        {
            return t.GetComponentInChildren<PatientChair>();
        }
    }
}
