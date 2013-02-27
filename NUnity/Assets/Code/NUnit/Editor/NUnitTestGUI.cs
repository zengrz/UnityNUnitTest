using UnityEngine;
using UnityEditor;

using NUnit.Framework;
using NUnit.Core;

using System;
using System.Collections.Generic;
using System.Reflection;

public class NUnitTestGUI : EditorWindow
{
    private const float kIndentMultiplier = 20.0f;

    private static NUnitTestRunner sTestRunner = null;

    [MenuItem("Window/NUnit Test WIndow")]
    static void Init()
    {
        // Create and/or focus the window.
        EditorWindow.GetWindow(typeof(NUnitTestGUI), false, "NUnit Test Window");
    }

    void OnEnable()
    {
        sTestRunner = new NUnitTestRunner();
        sTestRunner.LoadTests();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Test"))
        {
            sTestRunner.RunTests();
        }
        if (GUILayout.Button("Clear"))
        {
            sTestRunner.ClearTestResult();
        }
        GUILayout.EndHorizontal();

        if (sTestRunner.HasResult())
        {
            Vector2 scrollPosition = Vector2.zero;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            DisplayResultGUI(sTestRunner.GetTestResult(), 0);
            GUILayout.EndScrollView();
        }
    }
    //TODO: Shift the recursion part of this method elsewhere, maybe to "NUnitTestRunner.cs", or elsewhere...
    private void DisplayResultGUI(TestResult result, int indent)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indent * kIndentMultiplier);
        GUILayout.Label(string.Format("{0} - {1}", result.Name, result.ResultState));
        GUILayout.EndHorizontal();
        
        TestSuiteResult suiteResult = result as TestSuiteResult;
        if((suiteResult != null) && (suiteResult.Results != null))
        {
            foreach(TestResult child in suiteResult.Results)
            {
                DisplayResultGUI(child, indent + 1);
            }
        }
    }
}
