using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Unity.Netcode;
using UnityEngine;

namespace CodeBlue
{
    public enum SelectedCard
    {
        CardA,
        CardB,
        None
    }

    public class CardSelectionScreen : UIScreenBase
    {
        [Header("Cards")]
        [SerializeField, Range(1.5f, 3f)] private float _cardSelectTime;
        [SerializeField] private Loader _cardVoteLoader;
        [SerializeField] private CardSelection _cardA;
        [SerializeField] private CardSelection _cardB;
        [SerializeField] private SelectedCard _selectedCard;
        [SerializeField, SerializedDictionary("Player Id", "Selected Card")] private SerializedDictionary<ulong, SelectedCard> _playerSelectedCards = new();

        void Start()
        {
            CardSpawner.Instance.SetCardDatas(_cardA.gameObject, _cardB.gameObject);
        }

        // -1 for left, 1 for right
        public void Vote(int card)
        {
            if (card == 0) return;
            var leftSelected = card == -1;
            _cardA.OnCardSelect(leftSelected);
            _cardB.OnCardSelect(!leftSelected);
        }

        public void UpdatePlayerSelectedCards(List<ulong> playerIds)
        {
            if (playerIds.SequenceEqual(_playerSelectedCards.Keys)) return;

            foreach (ulong playerId in _playerSelectedCards.Keys.ToList())
            {
                if (playerIds.Contains(playerId)) continue;

                _playerSelectedCards.Remove(playerId);
            }

            foreach (ulong playerId in playerIds)
            {
                if (_playerSelectedCards.Keys.Contains(playerId)) continue;

                _playerSelectedCards.Add(playerId, SelectedCard.None);
            }

            SetPlayerVotes(playerIds);
        }

        private void SetPlayerVotes(List<ulong> playerIds)
        {
            _cardA.UpdatePlayerVotes(playerIds);
            _cardB.UpdatePlayerVotes(playerIds);
        }

        [Rpc(SendTo.Server)]
        public void Sv_SelectCardRpc(ulong playerId, bool isSelected, string cardName)
        {
            SelectedCard selectedCard = cardName switch
            {
                "Card A" => SelectedCard.CardA,
                "Card B" => SelectedCard.CardB,
                _ => SelectedCard.None
            };

            _playerSelectedCards[playerId] = isSelected ? selectedCard : SelectedCard.None;

            Cl_CheckCardVotesRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_CheckCardVotesRpc()
        {
            int cardAVoteCount = _playerSelectedCards.Values.Where(selectedCard => selectedCard == SelectedCard.CardA).ToList().Count;
            int cardBVoteCount = _playerSelectedCards.Values.Where(selectedCard => selectedCard == SelectedCard.CardB).ToList().Count;

            _cardVoteLoader.ResetLoadingTimer();

            int playerCount = UIManager.Instance.LobbyScreen.PlayerCount.GetPlayerCount();

            if (cardAVoteCount + cardBVoteCount != playerCount ||
            cardAVoteCount == cardBVoteCount) return;

            int max = Mathf.Max(cardAVoteCount, cardBVoteCount);

            if (IsServer)
            {
                if (max == cardAVoteCount)
                    _selectedCard = SelectedCard.CardA;
                else if (max == cardBVoteCount)
                    _selectedCard = SelectedCard.CardB;
            }

            _cardVoteLoader.StartLoadingTimer(endValueSeconds: _cardSelectTime);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_UpdateLevelDataRpc()
        {
            DrawnedCardData drawnedCard = _selectedCard switch
            {
                SelectedCard.CardA => _cardA.GetComponent<CardData>().GetCardData(),
                SelectedCard.CardB => _cardB.GetComponent<CardData>().GetCardData(),
                _ => new()
            };

            CardSpawner.Instance.CardSelection(drawnedCard);
            CardSpawner.Instance.ResetSpawnedCards();

            LevelData.Instance.AddCardData(drawnedCard);
        }

        public void OnVoteComplete()
        {
            Cl_UpdateLevelDataRpc();
            ResetPlayerSelectedCards();
            ResetCardSelections();
            GameManager.Instance.Cl_ChangeGameStateRpc(GamePhase.Prep);
        }

        private void ResetPlayerSelectedCards()
        {
            foreach (ulong playerId in _playerSelectedCards.Keys.ToList())
            {
                _playerSelectedCards[playerId] = SelectedCard.None;
            }
        }

        public void ResetCardSelections()
        {
            _cardA.Cl_ResetCardSelectionRpc();
            _cardB.Cl_ResetCardSelectionRpc();
        }
    }
}
