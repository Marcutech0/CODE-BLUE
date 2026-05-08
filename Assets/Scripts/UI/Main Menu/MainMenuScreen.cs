using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class MainMenuScreen : UIScreenBase
    {
        [Header("Components")]
        [SerializeField] private Button _playAsHost;
        [SerializeField] private TMP_InputField _joinCodeInput;

        [SerializeField] private GameObject _menuGroup;
        [SerializeField] private GameObject _loadingGroup;

        [SerializeField] Slider _masterVolSlider;
        [SerializeField] Slider _uiVolSlider;
        [SerializeField] Slider _gameVolSlider;
        [SerializeField] Slider _musicVolSlider;

        void Start()
        {
            _masterVolSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
            _uiVolSlider.onValueChanged.AddListener(AudioManager.Instance.SetUIVolume);
            _gameVolSlider.onValueChanged.AddListener(AudioManager.Instance.SetGameVolume);
            _musicVolSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);

            AudioManager.Instance.PlayRandomBGM();
        }

        public void StartHost()
        {
            StartCoroutine(LobbyManager.Instance.StartServer(onServerStart: () =>
            {
                UIManager.Instance.LobbyScreen.JoinLobbyCode.UpdateJoinLobbyCode(LobbyManager.Instance.JoinCode);
                UIManager.Instance.LoadScreen(UIScreenType.Lobby);
            }));
        }

        public void StartClient()
        {
            StartCoroutine(LobbyManager.Instance.JoinClient(_joinCodeInput.text, () =>
                UIManager.Instance.LoadScreen(UIScreenType.Lobby)
            ));
        }

        public override void SetScreenVisibility(bool isVisible)
        {
            base.SetScreenVisibility(isVisible);

            _menuGroup.SetActive(true);
            _loadingGroup.SetActive(false);
        }

        public void Quit() => Application.Quit();

        public void ButtonClick()
        {
            AudioManager.Instance.PlayUISfx("blip");
        }

    }
}
