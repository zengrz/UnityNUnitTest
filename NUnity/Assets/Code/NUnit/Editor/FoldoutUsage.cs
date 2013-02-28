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
    private bool showFoldout = true;
    //private String status = "Select a GameObject";

    private static Vector2 sScrollPosition = Vector2.zero;

    [MenuItem("Window/Foldout Usage")]
    public static void Init()
    {
        EditorWindow.GetWindow(typeof(FoldoutUsage));
    }

    public void OnGUI()
    {
        GUILayout.Label("Available Test Suites", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
        sScrollPosition = GUILayout.BeginScrollView(sScrollPosition);
        CreateButton("A test", true, 0);

//        EditorGUI.indentLevel += 1;
//        EditorGUILayout.Foldout(showPosition, "Position:");
//        EditorGUI.indentLevel += 1;
//        EditorGUILayout.Foldout(showPosition, "Rotation:");
//        EditorGUI.indentLevel -= 1;
//        EditorGUI.indentLevel -= 1;
        GUILayout.EndScrollView();
    }

    public void CreateButton(string testName, bool isSuite, int xOffset)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(xOffset);

        if (GUILayout.Button("Run", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
        {
        }
        if (isSuite)
        {
            showFoldout = EditorGUILayout.Foldout(showFoldout, GUIContent.none, EditorStyles.foldout);
    
            //GUILayout.BeginVertical();
            if (GUILayout.Button(testName, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
            }

            //GUILayout.EndVertical();
        }
        else
        {
            if (GUILayout.Button(testName, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
            {
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(4);

        if (isSuite && showFoldout)
        {
            for (int i = 0; i < 3; i++)
            {
                CreateButton("Test No.: " + i, false, xOffset + 32);
            }
        }
    }

    public void OnInspectorUpdate()
    {
        Repaint();
    }
}
