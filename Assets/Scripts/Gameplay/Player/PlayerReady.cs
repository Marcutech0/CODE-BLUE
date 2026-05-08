using System;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBlue
{
    public class PlayerReady : MonoBehaviour
    {
        [SerializeField] InputManager _input;

        private void Start()
        {
            _input.Player.Ready.performed  += OnReady;
            _input.Player.Vote.performed  += OnVote;
        }

        private void OnDisable()
        {
            _input.Player.Ready.performed  -= OnReady;
            _input.Player.Vote.performed  -= OnVote;
        }


        bool _isReady;
        private void OnReady(InputAction.CallbackContext context)
        {
            if (GameManager.Instance.CurrentPhaseEnum != GamePhase.Lobby) return;
            _isReady = !_isReady;
            UIManager.Instance.LobbyScreen.ReadyCount.UpdateReady(_isReady);
        }

        private void OnVote(InputAction.CallbackContext context)
        {
            if (GameManager.Instance.CurrentPhaseEnum != GamePhase.Card) return;

            var val = context.ReadValue<float>();
            UIManager.Instance.CardSelectionScreen.Vote((int)val);
        }
    }
}
