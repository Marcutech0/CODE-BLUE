using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace CodeBlue
{
    public class CvarManager : NetworkBehaviour
    {
        [SerializeField] GameObject _consoleUI;
        [SerializeField] TMP_InputField _consoleInput;

        protected override void OnNetworkPostSpawn()
        {
            base.OnNetworkPostSpawn();
        }

#if DEBUG
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                _consoleInput.text = "";
                var active = !_consoleUI.activeSelf;
                _consoleUI.SetActive(active);
                if (active)
                    EventSystem.current.SetSelectedGameObject(_consoleInput.gameObject);
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                CvarRegistry.ExecuteCommand(_consoleInput.text);
                _consoleUI.SetActive(false);
                _consoleInput.text = "";
            }
        }
#endif
    }
}
