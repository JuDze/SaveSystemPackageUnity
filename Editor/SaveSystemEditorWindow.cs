using UnityEditor;
using UnityEngine;
using System.IO;

namespace SaveSystem.Editor
{
    /// <summary>
    /// Editor utility window.
    /// Open via: Tools > Save System > Save Inspector
    /// </summary>
    public sealed class SaveSystemEditorWindow : EditorWindow
    {
        private string   _savesRoot;
        private string[] _files = {};
        private int      _selected = -1;
        private string   _content;
        private Vector2  _scroll;

        [MenuItem("Tools/Save System/Save Inspector")]
        public static void Open() =>
            GetWindow<SaveSystemEditorWindow>("Save Inspector").Show();

        private void OnEnable()
        {
            _savesRoot = Application.persistentDataPath;
            Refresh();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Save System Inspector", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(_savesRoot, EditorStyles.miniLabel);
            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh",     GUILayout.Width(80)))  Refresh();
                if (GUILayout.Button("Open Folder", GUILayout.Width(100))) EditorUtility.RevealInFinder(_savesRoot);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Save Files (.json)", EditorStyles.boldLabel);

            if (_files.Length == 0)
            {
                EditorGUILayout.HelpBox("No .json save files found in persistentDataPath.", MessageType.Info);
            }

            foreach (var file in _files)
            {
                using var row = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox);
                var label = Path.GetFileName(file);
                if (GUILayout.Button(label, EditorStyles.linkLabel))
                {
                    _selected = System.Array.IndexOf(_files, file);
                    _content  = File.ReadAllText(file);
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete Save",
                        $"Delete '{label}'?", "Delete", "Cancel"))
                    {
                        File.Delete(file);
                        Refresh();
                        return;
                    }
                }
            }

            if (_selected >= 0 && !string.IsNullOrEmpty(_content))
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("File Content (encrypted):", EditorStyles.boldLabel);
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(220));
                EditorGUILayout.TextArea(_content, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
                EditorGUILayout.HelpBox(
                    "Data is AES encrypted. Use your game's SaveManager to read actual values.",
                    MessageType.Info);
            }
        }

        private void Refresh()
        {
            _files    = Directory.Exists(_savesRoot)
                ? Directory.GetFiles(_savesRoot, "*.json")
                : new string[0];
            _selected = -1;
            _content  = null;
            Repaint();
        }
    }
}
