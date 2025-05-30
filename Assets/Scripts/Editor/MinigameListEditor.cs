#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Duel
{
    public class MinigameListCreator : EditorWindow
    {
        private SceneAsset[] droppedScenes;
        private string saveFileName = "NewMinigameList";
        
        [MenuItem("Tools/Create Minigame List")]
        public static void ShowWindow()
        {
            GetWindow<MinigameListCreator>("Minigame List Creator");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Drag & Drop Scene Files Here:", EditorStyles.boldLabel);
            
            // Drag and drop area
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drop Scene Files Here");
            
            HandleDragAndDrop(dropArea);
            
            // Show dropped scenes
            if (droppedScenes != null && droppedScenes.Length > 0)
            {
                GUILayout.Label($"Scenes to include ({droppedScenes.Length}):");
                foreach (var scene in droppedScenes)
                {
                    EditorGUILayout.ObjectField(scene, typeof(SceneAsset), false);
                }
            }
            
            GUILayout.Space(10);
            saveFileName = EditorGUILayout.TextField("File Name:", saveFileName);
            
            if (GUILayout.Button("Create MinigameList ScriptableObject"))
            {
                CreateMinigameList();
            }
        }
        
        private void HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            
            if (evt.type == EventType.DragUpdated && dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }
            else if (evt.type == EventType.DragPerform && dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.AcceptDrag();
                
                droppedScenes = new SceneAsset[DragAndDrop.objectReferences.Length];
                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    droppedScenes[i] = DragAndDrop.objectReferences[i] as SceneAsset;
                }
                
                Repaint();
            }
        }
        
        private void CreateMinigameList()
        {
            if (droppedScenes == null || droppedScenes.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "No scenes selected!", "OK");
                return;
            }
            
            MinigameList newList = CreateInstance<MinigameList>();
            newList.miniGames = new MinigameList.MinigameEntry[droppedScenes.Length];
            
            for (int i = 0; i < droppedScenes.Length; i++)
            {
                newList.miniGames[i] = new MinigameList.MinigameEntry();
                newList.miniGames[i].sceneName = droppedScenes[i].name;
            }
            
            string path = EditorUtility.SaveFilePanelInProject("Save MinigameList", saveFileName, "asset", "Save the MinigameList");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newList, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newList;
            }
        }
    }
}
#endif
