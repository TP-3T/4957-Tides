using System;
using System.Collections.Generic;
using TTT.DataClasses.States;
using TTT.GameEvents;
using TTT.Helpers;
using UnityEngine;

namespace TTT.Managers
{
    public class ResourcesController : GenericSingleton<ResourcesController>
    {
        /// <summary>
        /// TurnEnding event specifically for resource producers
        /// </summary>
        public event Action TurnEnding;

        /// <summary>
        /// SeasonEnding event specifically for resource producers
        /// </summary>
        public event Action SeasonEnding;

        /// <summary>
        /// YearEnding event specifically for resource producers
        /// </summary>
        public event Action YearEnding;

        private Dictionary<GameState, Action> stateActionMap;

        public override void Awake()
        {
            base.Awake();
            stateActionMap = new Dictionary<GameState, Action>
            {
                { GameState.TURN_END, TurnEnding },
                { GameState.SEASON_END, SeasonEnding },
                { GameState.YEAR_END, YearEnding },
            };
        }

        /// <summary>
        /// Handles game state changes.
        /// </summary>
        /// <param name="eventArgs"></param>
        public void OnGameStateChanging(UnityEngine.Object eventArgs)
        {
            if (eventArgs is not StateGameStateChangedEventArgs gameState)
            {
                return;
            }

            stateActionMap[gameState.NewState].Invoke();
        }
    }
}
