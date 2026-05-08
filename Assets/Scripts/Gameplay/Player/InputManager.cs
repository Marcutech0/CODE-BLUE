using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static GameplayInput;

namespace CodeBlue
{
    [DisallowMultipleComponent]
    public class InputManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] float _holdDuration = 1;
        [SerializeField] Slider _holdSlider;

        public PlayerActions Player { get; private set; }
        public UIActions UI { get; private set; }
        private float _holdTimer;

        GameplayInput _input;

        public static GameplayInput LocalInput => NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<InputManager>()._input;

        public UnityEvent OnInteract;
        public UnityEvent OnInteractHold;

        bool _isHolding;

        public bool CanHold { get; set; }

        void OnEnable()
        {
            _input = new();
            Player = _input.Player;
            UI = _input.UI;
            Player.InteractMain.performed += Interact;
            Player.SubInteract.performed += InteractHold;
            Player.SubInteract.canceled += InteractHold;

            _input.Enable();
        }

        void OnDisable()
        {
            _input?.Disable();

            Player.InteractMain.performed -= Interact;
            Player.SubInteract.performed -= InteractHold;
            Player.SubInteract.canceled -= InteractHold;
        }

        private void Interact(InputAction.CallbackContext ctx) => OnInteract?.Invoke();

        private void InteractHold(InputAction.CallbackContext context)
        {
            _isHolding = context.performed;

            if (context.canceled)
                CanHold = false;
        }

        private void Update()
        {
            _holdSlider.gameObject.SetActive(_isHolding && CanHold);
            if (_isHolding && CanHold)
            {
                _holdTimer += Time.deltaTime;
                if (_holdTimer >= _holdDuration)
                {
                    _holdTimer = 0;
                    CanHold = false;

                    OnInteractHold?.Invoke();
                }
            }
            else
            {
                _holdTimer = 0;
            }

            _holdSlider.value = _holdTimer / _holdDuration;
        }


    }
}
