using TTT.GameEvents;
using UnityEngine;

public class NetworkButton : MonoBehaviour
{
    public GameEvent startEvent;

    public void OnStartServer()
    {
        Debug.Log("I am being clicked");
        startEvent.Raise(new StartNetworkEventArgs() { IsHost = true });
    }

    public void OnStartClient()
    {
        Debug.Log("I am being clicked");
        startEvent.Raise(new StartNetworkEventArgs() { IsHost = false });
    }

    public void OnMapLoad(Object eventArgs)
    {
        gameObject.SetActive(false);
    }
}
