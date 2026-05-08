using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeBlue.Editor
{
    public class CodeBlueTools : EditorWindow
    {
        List<AudioClip> _clips = new();

        [MenuItem("Tools/Code Blue/Audio")]
        static void ShowWindow()
        {
            GetWindow<CodeBlueTools>("CodeBlue Tools");
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("i moved out...", EditorStyles.boldLabel);
            GUILayout.Label("audio tools are now butotns");
        }
    }
}
