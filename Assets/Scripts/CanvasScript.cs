using UnityEngine;
using TTT.GameEvents;
public class CanvasScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        NewMapFinishedEventArgs args = eventArgs as NewMapFinishedEventArgs;
    }
}
