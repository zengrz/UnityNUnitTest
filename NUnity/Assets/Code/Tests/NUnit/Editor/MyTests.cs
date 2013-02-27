using UnityEngine;
using NUnit.Framework;

[TestFixture]
public class MyClassTest
{
    [SetUp]
    public void SetUpMyClassTests()
    {
    }
    
    [TearDown]
    public void TearDownMyClassTests()
    {
    }
    
    [Test]
    public void TestPositive()
    {
        MyClass myc = new MyClass(1, 1);
        myc.Update();

        Assert.AreEqual(2, myc.sum);
        Assert.AreEqual(1, myc.product);
    }

    [Test]
    public void TestSumNegative()
    {
        MyClass myc = new MyClass(-1, -10);
        myc.Update();

        Assert.AreEqual(-11, myc.sum);
        Assert.AreEqual(10, myc.product);
    }
    
    [Test]
    [ExpectedException(typeof(System.NullReferenceException))]
    public void TestException()
    {
        MyClass myc = null;
        myc.Update();
    }
}

[TestFixture]
public class MyComponentTest
{    
    [SetUp]
    public void Init ()
    {
    }

    [TearDown]
    public void Dispose ()
    {
    }
    
    [Category("Positive")]
    [Test]
    public void TestPositive()
    {
        MyComponent myc = (MyComponent)Object.FindObjectOfType(typeof(MyComponent));
        Assert.IsNotNull(myc);
        Debug.Log ("brain.x = " + myc.brain.x);
        Debug.Log ("brain.y = " + myc.brain.y);
        myc.brain.x = 1;
        myc.brain.y = 1;
        myc.Update();

        Assert.AreEqual(2, myc.brain.sum);
        Assert.AreEqual(1, myc.brain.product);
    }
}
