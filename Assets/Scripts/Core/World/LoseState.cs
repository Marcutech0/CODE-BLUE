namespace CodeBlue
{
    public class LoseState : IState<GameManager>
    {
        private static LoseState _instance;
        public static LoseState Instance => _instance ??= new LoseState();

        LoseState() { }

        public void OnEnter(GameManager owner)
        {
            if (!owner.IsServer) return;

            PatientSpawner.Instance.Sv_StopSpawningRpc();
            PatientSpawner.Instance.Sv_AllPatientsLeaveRpc();
            PatientQueue.Instance.ClearChairs(); // this runs on the server only
            DayNightCycle.Instance.Sv_EndDayRpc();
            GameManager.Instance.Cl_FreezeAllPlayersRpc(true);
            UIManager.Instance.Cl_LoadScreenRpc(UIScreenType.Lose);
            UIManager.GetScreen<LoseScreen>().Cl_ShowLoseRpc();
        }

        public void OnExecute(GameManager owner)
        {
        }

        public void OnExit(GameManager owner)
        {
        }
    }
}
