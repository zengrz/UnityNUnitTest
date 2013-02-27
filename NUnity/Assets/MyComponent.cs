using UnityEngine;
using System.Collections;

public class MyComponent : MonoBehaviour
{
    public MyClass brain = new MyClass();
    
    public int i = 0;
    [SerializeField]public uint u = 0;
    [SerializeField]public long l = 0;
    public float f = 0.0f;
    
    // Update is called once per frame
    public void Update ()
    {
        brain.Update();
    }
}
