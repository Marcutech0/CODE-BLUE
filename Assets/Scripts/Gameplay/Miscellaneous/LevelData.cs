using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public class LevelData : SingletonBehaviour<LevelData>
    {
        [Header("Patient")]
        [field: SerializeField] public int MaxPatientCount { get; set; }

        // 0.8x lower bound, 1.2x upper bound
        // ex: spawn interval 15s (20 customers / 5 minutes)
        // min spawn time 15 * 0.8 = 12 seconds
        // max spawn time 15 * 1.2 = 18 seconds
        [field: SerializeField] public Vector2 SpawnIntervalVariance { get; set; } = new Vector2(0.8f, 1.2f);

        [Header("Cards")]
        [field: SerializeField] public List<DrawnedCardData> MedicalCases { get; private set; }
        [field: SerializeField] public List<DrawnedCardData> Departments { get; private set; }
        [field: SerializeField] public List<DrawnedCardData> GameEvents { get; private set; }
        [field: SerializeField] public List<int> MedicalSupplies { get; private set; }

        public DrawnedCardData LatestCard { get; private set; }

        private void Start()
        {
            MedicalCases = new();
            Departments = new();
            GameEvents = new();
            MedicalSupplies = new();

            Departments.Add(new(CardType.Department, DepartmentDatabase.Instance.ConsultationRoom.ID));
        }

        public void AddCardData(DrawnedCardData drawnedCard)
        {
            switch (drawnedCard.CardType)
            {
                case CardType.MedicalCase:
                    MedicalCases.Add(drawnedCard);
                    Sv_AddMedicalSuppliesRpc(drawnedCard);
                    break;
                case CardType.Department:
                    Departments.Add(drawnedCard);
                    break;
                case CardType.GameEvent:
                    GameEvents.Add(drawnedCard);
                    break;
            }
            LatestCard = drawnedCard;
        }

        [Rpc(SendTo.Server)]
        private void Sv_AddMedicalSuppliesRpc(DrawnedCardData drawnedCard)
        {
            MedicalCaseData medicalCase = MedicalCaseDatabase.Instance.GetData(drawnedCard.DataID);
            List<int> supplyIds = medicalCase.TreatmentPlan.ToList();

            foreach (int supplyId in supplyIds)
            {
                if (MedicalSupplies.Contains(supplyId)) return;

                MedicalSupplies.Add(supplyId);
                UIManager.GetScreen<LozolaScreen>().LozolaShop.Cl_AddShopItemRpc(supplyId);
                /* OLD::: UIManager.Instance.PreparationScreen.LozolaScreen.LozolaShop.Cl_AddShopItemRpc(supplyId);*/
            }
        }
    }
}
