using UnityEngine;
using UnityEngine.UI;

namespace CodeBlue
{
    public class LozolaScreen : UIScreenBase
    {
        [field: Header("Functionality")]
        [field: SerializeField] public ExitDesktopApp ExitDesktopApp { private set; get; }
        [field: SerializeField] public Search Search { private set; get; }
        [field: SerializeField] public Filter Filter { private set; get; }
        [field: SerializeField] public LozolaShop LozolaShop { private set; get; }
        [field: SerializeField] public LozolaCart LozolaCart { private set; get; }

        [Header("Components")]
        [SerializeField] private Button _exit;

        private void Awake()
        {
            _exit.onClick.AddListener(() =>
                UIManager.Instance.LoadScreen(UIScreenType.Preparation_Lozola)
            );
        }
    }
}
