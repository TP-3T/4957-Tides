using TTT.GameEvents;
using UnityEngine;

public class NetworkButton : MonoBehaviour
{
    [SerializeField]
    public GameEvent startEvent;

    public void OnStartServer()
    {
        Debug.Log("me host start button click :))");
        startEvent.Raise(new StartNetworkEventArgs() { IsHost = true });
    }

    public void OnStartClient()
    {
        startEvent.Raise(new StartNetworkEventArgs() { IsHost = false });
    }

    public void OnMapLoad(Object eventArgs)
    {
        gameObject.SetActive(false);
    }
}
