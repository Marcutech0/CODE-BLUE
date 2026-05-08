using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace CodeBlue
{
    [Flags]
    public enum GamePhase
    {
        Lobby = 1 << 0,
        Card = 1 << 1,
        Prep = 1 << 2,
        Work = 1 << 3,
        EndOfDay = 1 << 4,
        End = 1 << 5,
        Lose = 1 << 6
    }

    [RequireComponent(typeof(DayNightCycle))]
    public class GameManager : SingletonBehaviour<GameManager>
    {
        public static Dictionary<GamePhase, IState<GameManager>> GamePhasesMap = new() {
            {GamePhase.Lobby, LobbyState.Instance},
            {GamePhase.Card, CardSelectionState.Instance},
            {GamePhase.Prep, PreparationState.Instance},
            {GamePhase.Work, WorkState.Instance},
            {GamePhase.EndOfDay, EndState.Instance},
            {GamePhase.End, EndState.Instance},
            {GamePhase.Lose, LoseState.Instance},
        };

        private StateMachine<GameManager> _states = new();
        public IState<GameManager> CurrentState => _states.CurrentState;
        [field: SerializeField] public GamePhase CurrentPhaseEnum { get; private set; }
        [SerializeField] GameObject _playerPrefab;

        public NetworkVariable<int> PatientsTreated { get; private set; } = new();

        public UnityEvent<GamePhase> OnPhaseChanged { get; private set; } = new();

        void Start()
        {
            CvarRegistry.RegisterCommands(this);
        }

        public override void OnNetworkSpawn()
        {
            PatientsTreated.OnValueChanged += CheckTreatedCount;

            Cl_ChangeGameStateRpc(GamePhase.Lobby);
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_FreezeAllPlayersRpc(bool frozen)
        {
            if (GetLocalPlayer() is NetworkObject localPlayer)
            {
                localPlayer.GetComponent<Movement>().IsFrozen = frozen;
            }
        }

        public void FreezeLocalPlayer(bool frozen)
        {
            if (GetLocalPlayer() is NetworkObject player)
                player.GetComponent<Movement>().IsFrozen = frozen;
        }

        private void CheckTreatedCount(int previousValue, int newValue)
        {
            if (newValue == LevelData.Instance.MaxPatientCount)
            {
                print("[Game]: treated all the patients!");
                Cl_ChangeGameStateRpc(GamePhase.End);
            }
        }

        protected override void OnNetworkPostSpawn()
        {
            ConnectionNotificationManager.Singleton.OnClientConnectionNotification += OnClientConnect;
        }

        private void OnClientConnect(ulong id, ConnectionNotificationManager.ConnectionStatus status)
        {
            if (status == ConnectionNotificationManager.ConnectionStatus.Connected)
            {
                if (CurrentState is not LobbyState) return;
                if (IsServer)
                {
                    SpawnPlayer(id);
                }
            }
        }

        public void SpawnPlayer(ulong owner)
        {
            var availableLocker = LockerRoom.Instance.ClaimAvailableLocker(owner);

            var playerInstance = Instantiate(NetworkManager.GetNetworkPrefabOverride(_playerPrefab), availableLocker.SpawnPoint.position, Quaternion.identity);

            var playerNO = playerInstance.GetComponent<NetworkObject>();
            playerNO.SpawnAsPlayerObject(owner);
        }

        [Rpc(SendTo.Server)]
        public void Sv_PatientTreatedRpc()
        {
            PatientsTreated.Value = PatientsTreated.Value + 1;
        }

        [ConFunc("set_phase", "Set new game phase")]
        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_ChangeGameStateRpc(GamePhase phase)
        {
            var state = GamePhasesMap[phase];
            _states.SetState(state, this);
            CurrentPhaseEnum = phase;
            OnPhaseChanged.Invoke(phase);
        }

        public static NetworkObject GetLocalPlayer()
        {
            return NetworkManager.Singleton.LocalClient.PlayerObject;
        }

        void Update()
        {
            if (!IsServer) return;

            _states.CurrentState?.OnExecute(this);
        }

        public bool IsPhase(GamePhase state) => GamePhasesMap[state] == CurrentState;
    }
}
