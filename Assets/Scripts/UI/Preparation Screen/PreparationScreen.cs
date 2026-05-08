using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameplayInput;

namespace CodeBlue
{
    public class PreparationScreen : UIScreenBase
    {
        [field: Header("Functionality")]
        [field: SerializeField] public LozolaScreen LozolaScreen { private set; get; }
        [field: SerializeField] public AleebabwaScreen AleebabwaScreen { private set; get; }

        GameplayInput _input;

        void OnEnable()
        {
            _input = new GameplayInput();
            _input.Enable();
            _input.UI.Leave.performed += LeavePressed;
        }

        void OnDisable()
        {
            _input.Disable();
            _input.UI.Leave.performed -= LeavePressed;
        }

        private void LeavePressed(InputAction.CallbackContext ctx)
        {
            if (!GameManager.Instance.IsPhase(GamePhase.Work) && !GameManager.Instance.IsPhase(GamePhase.Prep)) return;

            var targScreen = (UIManager.Instance.CurrentScreen == UIScreenType.Preparation_Lozola || UIManager.Instance.CurrentScreen == UIScreenType.Preparation_Aleebabwa) ? UIScreenType.Preparation : UIScreenType.Game;

            UIManager.Instance.LoadScreen(targScreen);
        }

        public override void SetScreenVisibility(bool isVisible)
        {
            base.SetScreenVisibility(isVisible);
            GameManager.Instance.FreezeLocalPlayer(isVisible);
        }
    }
}
