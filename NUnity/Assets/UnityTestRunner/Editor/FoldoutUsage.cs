using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;

// Create a foldable menu that hides/shows the selected transform
// position.
// if no Transform is selected, the Foldout item will be folded until
// a transform is selected.

public class FoldoutUsage : EditorWindow
{
    private bool showPosition = true;
    //private String status = "Select a GameObject";

    private static Vector2 sScrollPosition = Vector2.zero;

    [MenuItem("Window/Foldout Usage")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(FoldoutUsage));
    }

    void OnGUI()
    {
#if UNITY_EDITOR
        GUILayout.Label("Available Test Suites", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
#endif
        sScrollPosition = GUILayout.BeginScrollView(sScrollPosition);
//        EditorGUI.indentLevel += 1;
//        EditorGUILayout.Foldout(showPosition, "Position:");
//        EditorGUI.indentLevel += 1;
//        EditorGUILayout.Foldout(showPosition, "Rotation:");
//        EditorGUI.indentLevel -= 1;
//        EditorGUI.indentLevel -= 1;
        GUILayout.EndScrollView();
    }

    void OnInspectorUpdate()
    {
        this.Repaint();
    }
}
