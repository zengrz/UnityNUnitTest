using UnityEngine;

using System;
using System.Reflection;

using NUnit.Core;

public class NUnitTestRunner {

    private TestPackage mTestPackage;
    private TestResult mResults;

    public NUnitTestRunner()
    {
        mTestPackage = null;
        mResults = null;
    }

    public void LoadTests()
    {
        if (!CoreExtensions.Host.Initialized)
        {
            CoreExtensions.Host.InitializeService();
        }
        // TODO: Maybe shift the Assembly acquisition from here to "NUnitTestGUI.cs"?
        // NOTE: don't use game assembly -- we want test code to all be in the editor assembly, at least for now.
        Assembly assembly = Assembly.GetExecutingAssembly();
        TestPackage testPackage = new TestPackage(assembly.Location);
        mTestPackage = testPackage;
    }

    public void RunTests()
    {
        SimpleTestRunner runner = new SimpleTestRunner();
        
        if (runner.Load(mTestPackage))
        {
            mResults = runner.Run(new NUnitTestListener());
            runner.Unload();
        }
        else
        {
            Debug.LogError("Failed to load package");
        }
    }

    public TestResult GetTestResult()
    {
        return mResults;
    }

    public void ClearTestResult()
    {
        mResults = null;
    }

    public bool HasResult()
    {
        return mResults != null;
    }

    private class NUnitTestListener : EventListener
    {
        public void RunStarted(string name, int testCount)
        {
            Debug.Log(string.Format("Start {0}: {1} tests", name, testCount));
        }
        
        public void SuiteStarted(TestName testName)
        {
            Debug.Log (string.Format("  start suite {0}", testName.FullName));
        }
        
        public void TestStarted(TestName testName)
        {
            Debug.Log (string.Format("    start test {0}", testName.FullName));
        }
        
        public void TestFinished(TestCaseResult result)
        {
            Debug.Log (string.Format("    finished test {0}", result.Name));
        }
        
        public void SuiteFinished(TestSuiteResult result)
        {
            Debug.Log (string.Format("  finished suite {0}", result.Name));
        }
        
        public void RunFinished(TestResult result)
        {
            Debug.Log(string.Format("Finished {0}: {1}", result.Name, result.ResultState));
        }
        
        public void RunFinished(Exception exception)
        {
            Debug.Log(string.Format("Finished with exception: {0}", exception.ToString()));
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
