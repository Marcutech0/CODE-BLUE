namespace CodeBlue
{
    public class EndOfDayState : IState<GameManager>
    {
        public static EndOfDayState _instance;
        public static EndOfDayState Instance => _instance ??= new EndOfDayState();

        PatientQueue queue;
        public void OnEnter(GameManager owner)
        {
            queue = PatientQueue.Instance;
        }

        public void OnExecute(GameManager owner)
        {
            if (PatientQueue.Instance.IsAnyChairOccupied()) owner.Cl_ChangeGameStateRpc(GamePhase.End);
        }

        public void OnExit(GameManager owner)
        {
        }
    }

    public class EndState : IState<GameManager>
    {
        private static EndState _instance;
        public static EndState Instance => _instance ??= new EndState();

        EndState() { }

        public void OnEnter(GameManager owner)
        {
            if (!owner.IsServer) return;

            GameManager.Instance.Cl_FreezeAllPlayersRpc(true);
            PatientQueue.Instance.Sv_AllPatientsInQueueLeaveRpc();
            PatientSpawner.Instance.Sv_StopSpawningRpc();
            UIManager.Instance.Cl_LoadScreenRpc(UIScreenType.Summary);
            UIManager.GetScreen<SummaryScreen>().Cl_ShowSummaryRpc();
        }

        public void OnExecute(GameManager owner)
        {
        }

        public void OnExit(GameManager owner)
        {
        }
    }
}
