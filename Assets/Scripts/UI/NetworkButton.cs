using TTT.GameEvents;
using UnityEngine;

public class NetworkButton : MonoBehaviour
{
    [SerializeField]
    private GameEvent SystemStateChangeEvent;

    public void OnButtonClickEpico()
    {
        SystemStateChangeEvent.Raise(new StateSystemChangeEventArgs
        {
            NewState = TTT.DataClasses.States.SystemState.PLAYING
        }
        );
    }
}   
