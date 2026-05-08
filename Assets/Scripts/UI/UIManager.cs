using UnityEngine;
using Unity.Netcode;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using AYellowpaper.SerializedCollections;

namespace CodeBlue
{
    public enum UIScreenType
    {
        MainMenu,
        Lobby,
        CardSelection,
        Preparation,
        Preparation_Lozola,
        Preparation_Aleebabwa,
        Game,
        Summary,
        Lose
    };

    public enum StatusTextType
    {
        Normal,
        Warning,
        Error
    }

    public class UIManager : SingletonBehaviour<UIManager>
    {
        // WE NEED TO FIX THIS AND JUST USE Screens[UIScreenType]
        [Header("Screens")]
        [field: SerializeField, SerializedDictionary("Screen Type", "Screen")]
        public SerializedDictionary<UIScreenType, UIScreenBase> Screens { get; private set; }

        [field: SerializeField] public MainMenuScreen MainMenuScreen { private set; get; }
        [field: SerializeField] public LobbyScreen LobbyScreen { private set; get; }
        [field: SerializeField] public CardSelectionScreen CardSelectionScreen { private set; get; }
        [field: SerializeField] public PreparationScreen PreparationScreen { private set; get; }
        [field: SerializeField] public GameScreen GameScreen { private set; get; }
        [field: SerializeField] public SummaryScreen SummaryScreen { private set; get; }
        [field: SerializeField] public LoseScreen LoseScreen { private set; get; }

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _networkStatusText;
        [SerializeField] private TextMeshProUGUI _buildNumberText;

        [Header("Misc")]
        [SerializeField] private Image _fade;

        public UIScreenType CurrentScreen { get; private set; }

        void Start()
        {
            _buildNumberText.text = "buildver: " + Application.version;

            /*_uiScreens = new();*/
            /*_uiScreens = new() {*/
            /*    { UIScreenType.MainMenu, MainMenuScreen.SetScreenVisibility },*/
            /*    { UIScreenType.Lobby, LobbyScreen.SetScreenVisibility },*/
            /*    { UIScreenType.CardSelection, CardSelectionScreen.SetScreenVisibility },*/
            /*    { UIScreenType.Preparation, PreparationScreen.SetScreenVisibility },*/
            /*    { UIScreenType.Preparation_Lozola, PreparationScreen.LozolaScreen.SetScreenVisibility },*/
            /*    { UIScreenType.Preparation_Aleebabwa, PreparationScreen.AleebabwaScreen.SetScreenVisibility },*/
            /*    { UIScreenType.Game, GameScreen.SetScreenVisibility },*/
            /*    { UIScreenType.Summary, SummaryScreen.SetScreenVisibility },*/
            /*    { UIScreenType.Lose, LoseScreen.SetScreenVisibility },*/
            /*};*/

            LoadScreen(UIScreenType.MainMenu);
        }

        public static T GetScreen<T>() where T : UIScreenBase
        {
            foreach (var pair in Instance.Screens)
            {
                if (pair.Value is T screen)
                    return screen;
            }
            return null;
        }

        public void SetNetworkStatus(string status, StatusTextType type = StatusTextType.Normal)
        {
            (string prefix, Color color) = type switch
            {
                StatusTextType.Error => ("ERR ", Color.red),
                StatusTextType.Warning => ("", Color.yellow),
                StatusTextType.Normal => ("", Color.white),
                _ => ("", Color.white)
            };
            _networkStatusText.color = color;
            _networkStatusText.text = prefix + status;
        }

        public void LoadScreen(UIScreenType screen)
        {
            foreach (UIScreenType uiScreen in Screens.Keys)
            {
                bool isCorrectScreen = (screen == uiScreen) ||
                    (screen == UIScreenType.Preparation_Lozola && (uiScreen == UIScreenType.Preparation_Lozola || uiScreen == UIScreenType.Preparation)) ||
                    (screen == UIScreenType.Preparation_Aleebabwa && (uiScreen == UIScreenType.Preparation_Aleebabwa || uiScreen == UIScreenType.Preparation));

                Screens[uiScreen]?.SetScreenVisibility(isCorrectScreen);
                if (isCorrectScreen)
                    CurrentScreen = screen;
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_LoadScreenRpc(UIScreenType screen)
        {
            LoadScreen(screen);
        }

        public void Fade(bool doFade, Action onFadeComplete)
        {
            var to = doFade ? 1 : 0;
            var from = doFade ? 0 : 1;

            _fade.DOFade(to, 2).From(from).SetEase(Ease.Linear).OnComplete(() => onFadeComplete.Invoke());
        }
    }
}
