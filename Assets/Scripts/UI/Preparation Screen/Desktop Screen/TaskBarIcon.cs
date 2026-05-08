using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CodeBlue
{
    public class TaskBarIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private UIScreenType _uiScreen;
        [SerializeField] private Toggle _taskbarIconToggle;
        [SerializeField] private GameObject _toolTipUI;

        private void Awake()
        {
            _taskbarIconToggle.onValueChanged.AddListener(isOn =>
            {
                if (_uiScreen == UIScreenType.Game)
                    UIManager.Instance.LoadScreen(_uiScreen);
                else
                    UIManager.Instance.LoadScreen(isOn ? _uiScreen : UIScreenType.Preparation);
            });
        }

        private void Start()
        {
            _toolTipUI.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _toolTipUI.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _toolTipUI.SetActive(false);
        }
    }
}
