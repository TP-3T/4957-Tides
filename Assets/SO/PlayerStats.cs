using System;
using TTT.Features;
using TTT.GameEvents;
using UnityEngine;

//* CB: CLass is basically disabled while we redo the events.
public class PlayerStats : ScriptableObject
{
    public Currencies PlayerCurrencies { get; private set; }

    void OnEnable()
    {
        // GameEvent.BuildEvent += UpdateResources;
    }

    void UpdateResources(object sender, EventArgs e)
    {
        //? CB: Do we want a unique event for building different building types
        //?     Or should we just handle it all in 1 event?
        // BuildEventArgs args = e as BuildEventArgs;
        // Currencies cost = args.Building.currencies;
        // if (PlayerCurrencies.CanSpendResources(cost))
        // {
        //     TTTEvents.BuildCancelEvent.Invoke(
        //         this,
        //         new BuildCancelEventArgs { HexCell = args.HexCell }
        //     );
        // }
        // else
        // {
        //     PlayerCurrencies.SpendResources(cost);
        // }
    }

    void OnDisable()
    {
        // TTTEvents.BuildEvent -= UpdateResources;
    }
}
