namespace CodeBlue
{
    public class WorkState : IState<GameManager>
    {
        private static WorkState _instance;
        public static WorkState Instance => _instance ??= new WorkState();

        WorkState() { }

        public void OnEnter(GameManager owner)
        {
            AudioManager.Instance.PlayRandomBGM();
            UIManager.Instance.Cl_LoadScreenRpc(UIScreenType.Game);
            DayNightCycle.Instance.Sv_StartDayRpc();
            PatientQueue.Instance.CreatePatientQueue();
            PatientSpawner.Instance.Sv_StartSpawningRpc();
        }

        public void OnExecute(GameManager owner)
        {
        }

        public void OnExit(GameManager owner)
        {
        }
    }
}
