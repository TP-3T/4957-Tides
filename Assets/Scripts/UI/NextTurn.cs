using TTT.GameEvents;
using TTT.Managers;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
/// <summary>
/// Handles the functionality of the "Next Turn" button in the game UI.
/// </summary>
public class NextTurn : MonoBehaviour
{
    [SerializeField]
    private GameEvent endingTurnEvent;

    /// <summary>
    /// UI Button component.
    /// </summary>
    private Button nextTurnButton;

    void Start()
    {
        // ! do we really need this? -Rodrigo
        // GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;

        nextTurnButton = GetComponent<Button>();
    }

    public void Enable(Object _)
    {
        nextTurnButton.interactable = true;
    }

    public void EndTurn()
    {
        nextTurnButton.interactable = false;

        endingTurnEvent.Raise();
    }

    public void OnClick()
    {
        bool canEndTurn = GameManager.Instance.CanEndTurn();

        if (!canEndTurn)
        {
            Debug.Log(
                "Player tried to end the turn, but not all requirements were met."
            );
            return;
        }

        EndTurn();
    }
}
