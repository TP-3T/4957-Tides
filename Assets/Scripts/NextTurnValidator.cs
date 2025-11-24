using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.ComponentModel.Com2Interop;
using TTT.DataClasses.PlayerResources;
using TTT.GameEvents;
using TTT.Managers;
using Unity.VisualScripting;
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
