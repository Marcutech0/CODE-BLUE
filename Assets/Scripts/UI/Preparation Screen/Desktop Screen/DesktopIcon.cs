using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class DesktopIcon : MonoBehaviour
    {
        [SerializeField] private UIScreenType _uiScreen;
        [SerializeField] private Button _desktopIconButton;

        private void Awake()
        {
            _desktopIconButton.onClick.AddListener(() => UIManager.Instance.LoadScreen(_uiScreen));
        }
    }
}
