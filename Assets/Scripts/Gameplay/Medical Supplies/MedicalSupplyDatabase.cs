using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeBlue
{
    public class MedicalSupplyDatabase : SingletonBehaviourNonNetworked<MedicalSupplyDatabase>
    {
        [Header("Database")]
        [SerializeField] private List<MedicalSupplyData> _medicalSupplies = new();
        [SerializeField] private List<Sprite> _medicalSupplyIcons = new();

        public int GetDataID(string name)
        {
            return _medicalSupplies.FirstOrDefault(medicalSupply => medicalSupply.Name == name)?.ID ?? throw new($"[SUPPLIES] {name} does not exist");
        }

        public MedicalSupplyData GetData(int id)
        {
            return _medicalSupplies.FirstOrDefault(medicalSupply => medicalSupply.ID == id);
        }

        public MedicalSupplyData GetData(string name)
        {
            return _medicalSupplies.FirstOrDefault(medicalSupply => medicalSupply.Name == name);
        }

        public Sprite GetSprite(string spriteName)
        {
            return _medicalSupplyIcons.FirstOrDefault(medicalSupplyIcon => medicalSupplyIcon.name.Contains(spriteName));
        }
    }
}
