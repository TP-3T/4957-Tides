using TTT.GameEvents;
using UnityEngine;

/// <summary>
/// Placeholder while we don't have a way to tell a specific player it's their turn yet.
/// Start the turn whenever it hears that a turn ended.
/// </summary>
public class TurnStarter : MonoBehaviour
{
    [SerializeField]
    private GameEvent startTurnEvent;

    public void StartTurn(Object _)
    {
        startTurnEvent.Raise();
    }
}
