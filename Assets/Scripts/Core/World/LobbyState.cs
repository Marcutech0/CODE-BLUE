namespace CodeBlue
{
    public class LobbyState : IState<GameManager>
    {
        private static LobbyState _instance;
        public static LobbyState Instance => _instance ??= new LobbyState();
        LobbyState() { }

        public void OnEnter(GameManager owner)
        {
            UIManager.Instance.Cl_LoadScreenRpc(UIScreenType.Lobby);
            GameManager.Instance.Cl_FreezeAllPlayersRpc(false);
        }

        public void OnExecute(GameManager owner)
        {
        }

        public void OnExit(GameManager owner)
        {
        }
    }
}
