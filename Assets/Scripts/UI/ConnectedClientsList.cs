using System.Linq;
using TTT.GameEvents.Assets.Scripts.GameEvents.Args;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(Dropdown))]
public class ConnectedClientsList : MonoBehaviour
{
    [SerializeField]
    private Dropdown _playersDropdown;

    private ulong[] _clientIds;

    void Start()
    {
        _playersDropdown = GetComponent<Dropdown>();
        SetDropdownContents();
    }

    void SetDropdownContents()
    {
        _playersDropdown.ClearOptions();
        _playersDropdown.AddOptions(_clientIds
            .Select(c => c.ToString())
            .ToList());
    }

    public void OnConnectedClients(Object eventArgs)
    {
        Debug.Log("OMG NEW CLIENTS HAVE CONNECTED");
        ConnectedCilentsEventArgs args = eventArgs as ConnectedCilentsEventArgs;

        _clientIds = args.clientIds;

        if (_playersDropdown is not null)
            SetDropdownContents();
    }
}
