using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CodeBlue
{
    public enum CardState
    {
        Available,
        Drawn,
        Selected
    }

    public enum CardType
    {
        MedicalCase,
        Department,
        GameEvent
    }

    [System.Serializable]
    public struct DrawnedCardData : INetworkSerializable
    {
        public CardType CardType;
        public int DataID;

        public DrawnedCardData(CardType cardType, int dataID)
        {
            CardType = cardType;
            DataID = dataID;
        }

        public readonly bool Equals(DrawnedCardData drawnedCardData)
        {
            return CardType == drawnedCardData.CardType && DataID == drawnedCardData.DataID;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref CardType);
            serializer.SerializeValue(ref DataID);
        }
    }

    public class CardSpawner : SingletonBehaviour<CardSpawner>
    {
        [SerializeField] private List<DrawnedCardData> _drawnedCards;

        private bool _hasSpawnedCards;
        private CardData _cardA, _cardB;

        public void SetCardDatas(GameObject cardA, GameObject cardB)
        {
            _cardA = cardA.GetComponent<CardData>();
            _cardB = cardB.GetComponent<CardData>();
        }

        public void ResetSpawnedCards()
        {
            _hasSpawnedCards = false;
        }

        public void SpawnCards()
        {
            if (!IsHost) return;

            if (_hasSpawnedCards) return;
            _hasSpawnedCards = true;

            int dayCount = DayNightCycle.Instance.GetDayCount();

            _drawnedCards.Clear();

            if (dayCount == 1)
            {
                Cl_SetDrawnedCardDataRpc(MedicalCaseDatabase.Instance.GetRandomDataID());
                Cl_SetDrawnedCardDataRpc(MedicalCaseDatabase.Instance.GetRandomDataID());
            }
            else if (dayCount == 2)
            {
                Cl_SetDrawnedCardDataRpc(GameEventDatabase.Instance.GetRandomDataID());
                Cl_SetDrawnedCardDataRpc(WeightedCardSpawn());
            }
            else if (dayCount % 3 == 0 && dayCount <= 12)
            {
                Cl_SetDrawnedCardDataRpc(DepartmentDatabase.Instance.GetRandomDataID());
                Cl_SetDrawnedCardDataRpc(WeightedCardSpawn());
            }
            else if (dayCount >= 4)
            {
                Cl_SetDrawnedCardDataRpc(WeightedCardSpawn());
                Cl_SetDrawnedCardDataRpc(WeightedCardSpawn());
            }

            _cardA.SetCardData(_drawnedCards[0]);
            _cardB.SetCardData(_drawnedCards[1]);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void Cl_SetDrawnedCardDataRpc(DrawnedCardData drawnedCard)
        {
            _drawnedCards.Add(drawnedCard);
        }

        private DrawnedCardData WeightedCardSpawn()
        {
            int roll = Random.Range(0, 100);

            if (roll < 60) // 60% chance for MedicalCase
            {
                return MedicalCaseDatabase.Instance.GetRandomDataID();
            }
            else if (roll < 100) // 40% chance for Event
            {
                return GameEventDatabase.Instance.GetRandomDataID();
            }

            return new();
        }

        public void CardSelection(DrawnedCardData drawnedCard)
        {
            if (_drawnedCards[0].Equals(drawnedCard))
            {
                CardSelection(_drawnedCards[0], CardState.Selected);
                CardSelection(_drawnedCards[1], CardState.Available);
            }
            else if (_drawnedCards[1].Equals(drawnedCard))
            {
                CardSelection(_drawnedCards[0], CardState.Available);
                CardSelection(_drawnedCards[1], CardState.Selected);
            }
        }

        private void CardSelection(DrawnedCardData drawnedCard, CardState cardState)
        {
            switch (drawnedCard.CardType)
            {
                case CardType.MedicalCase:
                    MedicalCaseDatabase.Instance.DataSelection(drawnedCard.DataID, cardState);
                    break;
                case CardType.Department:
                    DepartmentDatabase.Instance.DataSelection(drawnedCard.DataID, cardState);
                    break;
                case CardType.GameEvent:
                    GameEventDatabase.Instance.DataSelection(drawnedCard.DataID, cardState);
                    break;
            }
        }
    }
}
