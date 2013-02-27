using UnityEngine;
using UnityEditor;

using NUnit.Framework;
using NUnit.Core;

using System;
using System.Collections.Generic;
using System.Reflection;

public class NUnityWindow : EditorWindow
{
    private TestResult mResults = null;
    private Vector2 mScrollPosition = Vector2.zero;

    [MenuItem("Window/NUnity")]
    static void Init()
    {
        // Create and/or focus the window.
        EditorWindow.GetWindow(typeof(NUnityWindow), false, "NUnity");
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Test"))
        {
            RunTests();
        }
        if (GUILayout.Button("Clear"))
        {
            mResults = null;
        }
        GUILayout.EndHorizontal();

        if (mResults != null)
        {
            mScrollPosition = GUILayout.BeginScrollView(mScrollPosition);
            DisplayResultGUI(mResults, 0);
            GUILayout.EndScrollView();
        }
    }
    
    void RunTests()
    {
        if (!CoreExtensions.Host.Initialized)
        {
            CoreExtensions.Host.InitializeService();
        }

        // NOTE: don't use game assembly -- we want test code to all be in the editor assembly, at least for now.
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        TestPackage package = new TestPackage(assembly.Location);
        
        SimpleTestRunner runner = new SimpleTestRunner();
        
        if (runner.Load(package))
        {
            mResults = runner.Run(new NUnityListener());
            runner.Unload();
        }
        else
        {
            Debug.LogError("Failed to load package");
        }
    }
    
    private void DisplayResultGUI(TestResult result, int indent)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(indent * 20.0f);
        GUILayout.Label(string.Format("{0} - {1}", result.Name, result.ResultState));
        GUILayout.EndHorizontal();
        
        TestSuiteResult suiteResult = result as TestSuiteResult;
        if ((suiteResult != null) && (suiteResult.Results != null))
        {
            foreach (TestResult child in suiteResult.Results)
            {
                DisplayResultGUI(child, indent+1);
            }
        }
    }

    private class NUnityListener : EventListener
    {
        public void RunStarted(string name, int testCount)
        {
            Debug.Log(string.Format("Start {0}: {1} tests", name, testCount));
        }
        
        public void RunFinished(TestResult result)
        {
            Debug.Log(string.Format("Finished {0}: {1}", result.Name, result.ResultState));
        }
        
        public void RunFinished(Exception exception)
        {
            Debug.Log(string.Format("Finished with exception: {0}", exception.ToString()));
        }
        
        public void TestStarted(TestName testName)
        {
            Debug.Log(string.Format("    start test {0}", testName.FullName));
        }
        
        public void TestFinished(TestCaseResult result)
        {
            Debug.Log(string.Format("    finished test {0}", result.Name));
        }
        
        public void SuiteStarted(TestName testName)
        {
            Debug.Log(string.Format("  start suite {0}", testName.FullName));
        }
        
        public void SuiteFinished(TestSuiteResult result)
        {
            Debug.Log(string.Format("  finished suite {0}", result.Name));
        }
        
        public void UnhandledException(Exception exception)
        {
            Debug.Log(string.Format("Unhandled exception: {0}", exception.ToString()));
        }
        
        public void TestOutput(TestOutput testOutput)
        {
            switch (testOutput.Type)
            {
                case TestOutputType.Error:
                    Debug.LogError(string.Format("    {0}", testOutput.Text));
                    break;
                case TestOutputType.Log:
                case TestOutputType.Out:
                case TestOutputType.Trace:
                    Debug.Log(string.Format("    {0}", testOutput.Text));
                    break;
            }
        }
    }
}
