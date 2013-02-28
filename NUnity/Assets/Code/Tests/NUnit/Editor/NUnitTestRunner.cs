using UnityEngine;

using System;
using System.Reflection;

using NUnit.Core;

public class NUnitTestRunner {

    private TestPackage mTestPackage;
    private TestResult mResults; // Need this to check if the tests are all done. Should be better ways to achieve the same thing (ie check for status)
    private SimpleTestRunner mTestRunner; // So that we can cancel/stop the test at any point in time.

    private bool isLoaded;

    public NUnitTestRunner()
    {
        mTestPackage = null;
        mResults = null;
        mTestRunner = null;
        isLoaded = false;
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
        mTestPackage = new TestPackage(assembly.Location);
        mTestRunner = new SimpleTestRunner();

        if (mTestRunner.Load(mTestPackage))
        {
            isLoaded = true;
        }
        else
        {
            Debug.LogError("Failed to load package");
        }

        Debug.Log("TestName - " + mTestPackage.TestName);
        Debug.Log("Name - " + mTestPackage.Name);
        Debug.Log("FullName - " + mTestPackage.FullName);
    }

    public void RunTests()
    {
        if (isLoaded)
        {
            //mResults = mTestRunner.Run(new NUnitTestListener());
            mResults = mTestRunner.Run(new NUnitTestListener(), new SingleTestFilter(mTestPackage.FullName));
            //mResults = mTestRunner.Run(new NUnitTestListener(), new SingleTestFilter("TestPositive"));
            mTestRunner.Unload();
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

    public class SingleTestFilter : TestFilter
    {
        private string testName;
    
        public SingleTestFilter(string TestName)
        {
            testName = TestName;
        }
    
    
        public override bool Match(ITest test)
        {
            return test.TestName.Name.Equals(testName);
        }
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
