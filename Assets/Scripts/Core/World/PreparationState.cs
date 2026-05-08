using UnityEngine;

namespace CodeBlue
{
    public class PreparationState : IState<GameManager>
    {
        private static PreparationState _instance;

        public static PreparationState Instance => _instance ??= new PreparationState();

        public void OnEnter(GameManager owner)
        {
            UIManager.Instance.Cl_LoadScreenRpc(UIScreenType.Game);
            UIManager.Instance.GameScreen.Clock.Cl_StartClockFillRpc();

            if (owner.IsServer)
            {
                var latestCard = LevelData.Instance.LatestCard;
                if (latestCard.CardType != CardType.MedicalCase) return;

                var medCase = MedicalCaseDatabase.Instance.GetData(latestCard.DataID);
                MedicalSupplyBoxSpawner.Instance.Sv_SpawnBoxesRpc(medCase.TreatmentPlan);

                UIManager.Instance.LoadScreen(UIScreenType.Game);
            }
        }

        public void OnExecute(GameManager owner)
        {
        }

        public void OnExit(GameManager owner)
        {
        }
    }
}
