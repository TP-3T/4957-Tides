using System.Collections;
using System.Collections.Generic;
using Hex;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
/// <summary>
/// Handles the functionality of the "Next Turn" button in the game UI.
/// </summary>
public class NextTurn : MonoBehaviour
{
    private HexGrid hg;
    private Sea s;

    void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;

        s = FindFirstObjectByType<Sea>();
    }

    /// <summary>
    /// Handles the button click event to proceed to the next turn.
    /// </summary>
    public void OnClick()
    {
        s.HandleNextTurnClickedServerRpc();
    }
}
