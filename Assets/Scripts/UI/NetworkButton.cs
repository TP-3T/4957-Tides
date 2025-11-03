using TTT.GameEvents;
using Unity.Netcode;
using UnityEngine;

public class NetworkButton : MonoBehaviour
{
    public GameEvent startEvent;

    public void OnStartHost()
    {
        Debug.Log("I am being clicked HOST");
        startEvent.Raise(new StartNetworkEventArgs() { IsHost = true });
    }

    public void OnStartClient()
    {
        Debug.Log("I am being clicked CLIENT");
        startEvent.Raise(new StartNetworkEventArgs() { IsHost = false });
    }

    public void OnMapLoad(Object eventArgs)
    {
        gameObject.SetActive(false);
    }
}
