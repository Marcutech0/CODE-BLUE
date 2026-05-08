using UnityEngine;

namespace CodeBlue
{
    public class CommonCvar : MonoBehaviour
    {
        bool _showStateUI;

        private void Awake()
        {
            CvarRegistry.RegisterCommands(this);
        }

        [ConFunc("d_togglestate")]
        internal void C_ToggleState() { 
            _showStateUI = !_showStateUI;
        }


#if DEBUG
        void OnGUI()
        {
            if ( !_showStateUI ) return;
            var s = new GUIStyle
            {
                fontSize = 48
            };

            GUI.Label(new Rect(10, 0, 500, 500), $"Current State: {GameManager.Instance.CurrentState}", s);
        }
#endif

    }
}
