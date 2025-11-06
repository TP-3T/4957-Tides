using TTT.GameEvents;
using UnityEngine;

public class NetworkButton : MonoBehaviour
{
    public GameEvent startEvent;

    public void OnStartServer()
    {
        startEvent.Raise(new StartNetworkEventArgs() { IsHost = true });
    }

    public void OnStartClient()
    {
        startEvent.Raise(new StartNetworkEventArgs() { IsHost = false });
    }

    public void OnMapLoad(Object eventArgs)
    {
        NewMapFinishedEventArgs args = eventArgs as NewMapFinishedEventArgs;
        if (args.WasSuccessful)
        {
            gameObject.SetActive(false);
        }
    }
}
