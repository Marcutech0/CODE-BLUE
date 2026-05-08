namespace CodeBlue
{
    public class CardSelectionState : IState<GameManager>
    {
        private static CardSelectionState _instance;

        public static CardSelectionState Instance => _instance ??= new CardSelectionState();

        CardSelectionState() { }

        public void OnEnter(GameManager owner)
        {
            AudioManager.Instance.PlayGameSfx("ready");
            UIManager.Instance.Cl_LoadScreenRpc(UIScreenType.CardSelection);
            GameManager.Instance.Cl_FreezeAllPlayersRpc(true);
            CardSpawner.Instance.SpawnCards();
        }

        public void OnExecute(GameManager owner)
        {
        }

        public void OnExit(GameManager owner)
        {
            GameManager.Instance.Cl_FreezeAllPlayersRpc(false);
        }
    }
}
