using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
/// <summary>
/// Handles the functionality of the "Next Turn" button in the game UI.
/// </summary>
public class NextTurn : MonoBehaviour
{
    void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    /// <summary>
    /// Handles the button click event to proceed to the next turn.
    /// </summary>
    public void OnClick()
    {
        Debug.Log("Next Turn Button Clicked");
        // GameManager.Instance.NextTurn();
    }
}
