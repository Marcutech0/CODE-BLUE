using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class CardSelection : UIScreenBase
    {
        [Header("Card UI")]
        [SerializeField] private Toggle _toggle;
        [SerializeField] private GameObject[] _playerVoteMarkers;
        [SerializeField, SerializedDictionary("Player Id", "Player Vote Marker Index")] private SerializedDictionary<ulong, int> _playerVotes = new();

        private void Start()
        {
            ResetCardSelection();
            _toggle.onValueChanged.AddListener(OnCardSelect);
        }

        public void UpdatePlayerVotes(List<ulong> playerIds)
        {
            _playerVotes.Clear();

            for (int i = 0; i < playerIds.Count; i++)
            {
                _playerVotes.Add(playerIds[i], i);
            }
        }

        private void ResetCardSelection()
        {
            _toggle.isOn = false;

            foreach (GameObject playerVoteMarker in _playerVoteMarkers)
            {
                playerVoteMarker.SetActive(false);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void Cl_ResetCardSelectionRpc()
        {
            ResetCardSelection();
        }

        public void OnCardSelect(bool isOn)
        {
            _toggle.interactable = !isOn;

            ulong playerId = NetworkManager.Singleton.LocalClientId;

            Sv_UpdateCardVoterUIRpc(playerId, isOn);
            if (isOn)
            {
                AudioManager.Instance.PlayGameSfx("select");
                UIManager.Instance.CardSelectionScreen.Sv_SelectCardRpc(playerId, isOn, name);
            }
        }

        [Rpc(SendTo.Server)]
        private void Sv_UpdateCardVoterUIRpc(ulong playerId, bool isOn)
        {
            Cl_UpdateCardVoterUIRpc(_playerVotes[playerId], isOn);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_UpdateCardVoterUIRpc(int index, bool isOn)
        {
            _playerVoteMarkers[index].SetActive(isOn);
        }
    }
}
