using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEditor;
using UnityEngine;

namespace CodeBlue
{
    public class GameEventDatabase : SingletonBehaviourNonNetworked<GameEventDatabase>
    {
        [Header("Preload Data")]
        [SerializeField] private GameEventData[] _preloadData;

        [Header("Database")]
        [SerializeField, SerializedDictionary("Game Event", "Card State")]
        private SerializedDictionary<GameEventData, CardState> _gameEvents = new();

        private void Start()
        {
            LoadPreloadEvents();
        }

        private void LoadPreloadEvents()
        {
            foreach (var eventData in _preloadData)
            {
                _gameEvents.Add(eventData, CardState.Available);
            }
        }

        public DrawnedCardData GetRandomDataID()
        {
            List<GameEventData> gameEvents = _gameEvents.Where(pair => pair.Value == CardState.Available).Select(pair => pair.Key).ToList();

            if (gameEvents.Count == 0) return new();

            GameEventData gameEvent = gameEvents.SelectRandom();
            _gameEvents[gameEvent] = CardState.Drawn;

            return new(CardType.GameEvent, gameEvent.ID);
        }

        public int GetDataID(string name)
        {
            return _gameEvents.FirstOrDefault(pair => pair.Key.Name == name).Key.ID;
        }

        public GameEventData GetData(int id)
        {
            return _gameEvents.FirstOrDefault(pair => pair.Key.ID == id).Key;
        }

        public void DataSelection(int id, CardState cardState)
        {
            GameEventData gameEvent = _gameEvents.FirstOrDefault(pair => pair.Key.ID == id).Key;
            _gameEvents[gameEvent] = cardState;
        }

        public void ResetData()
        {
            foreach (var eventData in _gameEvents.Keys)
            {
                _gameEvents[eventData] = CardState.Available;
            }
        }
    }
}