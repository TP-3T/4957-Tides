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
    private GameManager GameManager;
    [SerializeField] private int maxC02 = 500;
    [SerializeField] private int maxTemperature = 50;

    [SerializeField] private GameEvent _PlayerLoseEvent;

    public GameEvent NextTurnClickedEvent;

    void Start()
    {
        GameManager = FindAnyObjectByType<GameManager>();
    }

    public void OnValidatingNextTurn(Object _)
    {
        if (!HasEnoughResources())
        {
            return;
        }

        if (GameManager.GetCO2() > maxC02)
        {
            _PlayerLoseEvent.Raise();
            return;
        }

        if (GameManager.GetTemperature() > maxTemperature)
        {
            _PlayerLoseEvent.Raise();
            return;
        }

        NextTurnClickedEvent.Raise();
    }

    public bool HasEnoughResources()
    {
        return AllResources.All((res) => res.AmountOwned >= 0);
    }
}
