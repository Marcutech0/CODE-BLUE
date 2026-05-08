using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public enum ReadyState
    {
        Idle,
        Ready,
    }

    public class ReadyCount : NetworkBehaviour
    {
        [Header("Toggle")]
        [SerializeField] private Toggle _readyToggle;

        [Header("Color")]
        [SerializeField] private Color _readyToggleColor = new(0f, 0.2078f, 1f); //#0035FF

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI _readyCountText;

        [Header("Load")]
        [SerializeField] private RectTransform _readyLoadingBar;
        [SerializeField, Range(1.5f, 3f)] private float _readyLoadDuration;

        [Header("Miscellaneous")]
        [SerializeField] private NetworkVariable<float> _readyLoadingBarSize = new();

        [SerializeField, SerializedDictionary("Player Id", "Ready State")] private SerializedDictionary<ulong, ReadyState> _playerReadyStates = new();

        private float _readyLoadTimer;

        private bool _isReadyLoading;

        private float _readyBarSizeX;

        void Start()
        {
            _readyBarSizeX = _readyLoadingBar.sizeDelta.x;
            _readyToggle.onValueChanged.AddListener(UpdateReady);
        }

        public void UpdatePlayerReadyStates(List<ulong> playerIds)
        {
            if (playerIds.SequenceEqual(_playerReadyStates.Keys)) return;
            
            Cl_SetReadyToggleIsOnRpc(false);

            foreach (ulong playerId in _playerReadyStates.Keys.ToList())
            {
                if (playerIds.Contains(playerId)) continue;

                _playerReadyStates.Remove(playerId);
            }

            foreach (ulong playerId in playerIds)
            {
                if (_playerReadyStates.Keys.Contains(playerId)) continue;

                _playerReadyStates.Add(playerId, ReadyState.Idle);
            }
        }

        private void ResetPlayerReadyState()
        {
            foreach (ulong playerId in _playerReadyStates.Keys.ToList())
            {
                _playerReadyStates[playerId] = ReadyState.Idle;
            }
        }

        public void SetVisibility(bool isVisible)
        {
            var col = new Color(71 / 255, 91/255, 1, isVisible ? 1 : 0);
            _readyLoadingBar.GetComponent<Image>().color = col;
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_SetReadyToggleIsOnRpc(bool isOn)
        {
            _readyToggle.isOn = isOn;
        }

        public void UpdateReady(bool isOn)
        {
            UIManager.Instance.LobbyScreen.LeaveLobby.SetLeaveInteractability(!isOn);
            _readyToggle.image.color = isOn ? _readyToggleColor : Color.white;
            Sv_UpdatePlayerReadyStateRpc(NetworkManager.Singleton.LocalClientId, isOn ? ReadyState.Ready : ReadyState.Idle);
            SetVisibility(isOn);
        }

        [Rpc(SendTo.Server)]
        private void Sv_UpdatePlayerReadyStateRpc(ulong playerId, ReadyState readyState)
        {
            _playerReadyStates[playerId] = readyState;
            int readyCount = _playerReadyStates.Where(pair => pair.Value == ReadyState.Ready).ToList().Count;
            int playerCount = UIManager.Instance.LobbyScreen.PlayerCount.GetPlayerCount();
            _isReadyLoading = readyCount == playerCount;
            Cl_UpdateReadyCountRpc(readyCount);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_UpdateReadyCountRpc(int readyCount)
        {
            _readyCountText.text = new string('+', readyCount);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_UpdateReadyLoadingBarRpc()
        {
            _readyLoadingBar.sizeDelta = new Vector2(_readyLoadingBarSize.Value  * _readyBarSizeX, 30);
        }

        private void Update()
        {
            if (!IsServer) return;

            if (_isReadyLoading)
                _readyLoadTimer += Time.deltaTime;
            else
                _readyLoadTimer = 0f;

            _readyLoadingBarSize.Value = _readyLoadTimer / _readyLoadDuration;
            Cl_UpdateReadyLoadingBarRpc();

            if (_readyLoadTimer < _readyLoadDuration) return;

            _readyLoadTimer = 0f;
            _isReadyLoading = false;
            Cl_SetReadyToggleIsOnRpc(false);
            ResetPlayerReadyState();

            GameManager.Instance.Cl_ChangeGameStateRpc(GamePhase.Card);
        }
    }
}