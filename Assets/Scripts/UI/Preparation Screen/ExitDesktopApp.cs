using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class ExitDesktopApp : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private Button _exit;

        private void Start()
        {
            _exit.onClick.AddListener(() =>
                UIManager.Instance.LoadScreen(UIScreenType.Preparation)
            );
        }
    }
}
