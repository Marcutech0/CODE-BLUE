using UnityEngine;
using UnityEditor;
using System.IO;
using System.Drawing.Printing;

namespace CodeBlue.Editor
{
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : UnityEditor.Editor
    {
        const string AUDIO_FOLDER_PATH = "Assets/Audio/SFX";

        public override void OnInspectorGUI()
        {
            AudioManager manager = (AudioManager)target;
            DrawDefaultInspector();

            if (GUILayout.Button("Load sfx files"))
            {
                LoadAudioFiles(manager);
            }
        }

        void LoadAudioFiles(AudioManager manager)
        {
            manager.SFXClips.Clear();
            
            if (Directory.Exists(AUDIO_FOLDER_PATH))
            {
                var paths = Directory.GetFiles(AUDIO_FOLDER_PATH, "*.wav");

                foreach (var path in paths)
                {
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

                    if (clip != null) manager.SFXClips.Add(clip);
                    else Debug.LogError("[Audio] failed to load clip at " + path);
                }
            }
        }
    }
}
