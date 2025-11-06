using UnityEngine;
using TTT.GameEvents;
public class CanvasScript : MonoBehaviour
{
    [SerializeField]
    private TextAsset LevelFile;

    [SerializeField]
    private GameEvent newMapEvent;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        newMapEvent.Raise(new NewMapEventArgs() { DataFile = LevelFile });
    }

    // Update is called once per frame
    void Update()
    {
        // NewMapFinishedEventArgs args = eventArgs as NewMapFinishedEventArgs;
    }
}
