#if UNITY_EDITOR
using Duel.DebugScripts;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(UnityServicesDebugInfo))]
    public class UnityServicesDebugInfoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            UnityServicesDebugInfo debugInfo = (UnityServicesDebugInfo)target;
        
            // Custom header
            EditorGUILayout.Space(10);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 16;
            headerStyle.normal.textColor = Color.cyan;
            EditorGUILayout.LabelField("Unity Services Debug Inspector", headerStyle);
        
            EditorGUILayout.Space(10);
        
            // Refresh button
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.fontSize = 12;
            buttonStyle.fixedHeight = 30;
        
            if (GUILayout.Button("🔄 Refresh Unity Services Info", buttonStyle))
            {
                debugInfo.ManualRefresh();
                EditorUtility.SetDirty(debugInfo);
            }
        
            EditorGUILayout.Space(5);
        
            // Auto-refresh toggle
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Toggle Auto-Refresh", GUILayout.Height(25)))
            {
                debugInfo.ToggleAutoRefresh();
                EditorUtility.SetDirty(debugInfo);
            }
            EditorGUILayout.EndHorizontal();
        
            EditorGUILayout.Space(5);
        
            // Log all values button
            if (GUILayout.Button("📋 Log All Values to Console", GUILayout.Height(25)))
            {
                debugInfo.LogAllValues();
            }
        
            EditorGUILayout.Space(10);
        
            // Draw default inspector
            DrawDefaultInspector();
        }
    }
}
#endif