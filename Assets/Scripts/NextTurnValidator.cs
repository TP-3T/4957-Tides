using System.Collections.Generic;
using System.Linq;
using TTT.DataClasses.PlayerResources;
using TTT.GameEvents;
using UnityEngine;

public class NextTurnValidator : MonoBehaviour
{
    [field: SerializeField]
    public List<PlayerResource> AllResources { get; set; }

    public GameEvent NextTurnClickedEvent;

    public void OnValidatingNextTurn(Object _)
    {
        if (!HasEnoughResources())
        {
            return;
        }

        NextTurnClickedEvent.Raise();
    }

    public bool HasEnoughResources()
    {
        return AllResources.All((res) => res.AmountOwned >= 0);
    }
}
