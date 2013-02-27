using UnityEngine;

[System.Serializable]
public class MyClass
{
    public int x = 0;
    public int y = 0;
    
    public int sum = 0;
    public int product = 0;
    
    public MyClass()
    {
    }
    
    public MyClass(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    
    // Update is called once per frame
    public void Update ()
    {
        sum = x + y;
        product = x * y;
    }
}
