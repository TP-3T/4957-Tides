using TMPro;
using UnityEngine;

/// <summary>
/// Script for displaying active lobbies.
/// </summary>
public class LobbyMenu : MonoBehaviour
{
    [SerializeField]
    public GameObject LobbyEntry;

    [SerializeField]
    public GameObject Body;

    [SerializeField]
    public GameObject IpAddress;
    
    [SerializeField]
    public GameObject Port;


    /// <summary>
    /// Gets the IP address from the TextMeshPro component.
    /// </summary>
    public string GetIpAddress()
    {
        if (IpAddress != null)
        {
            var tmp = IpAddress.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                return tmp.text;
            }
        }
        return "127.0.0.1";
    }

    /// <summary>
    /// Gets the port from the TextMeshPro component, parsing it as a ushort.
    /// Filters out non-numeric characters before parsing.
    /// </summary>
    public ushort GetPort()
    {
        if (Port != null)
        {
            var tmp = Port.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                // Extract only numeric characters
                string numericOnly = "";
                foreach (char c in tmp.text)
                {
                    if (char.IsDigit(c))
                    {
                        numericOnly += c;
                    }
                }
                
                if (ushort.TryParse(numericOnly, out ushort port))
                {
                    return port;
                }
            }
        }
        return 7777; // Default port
    }
}
