using TTT.Helpers;
using UnityEngine;

namespace TTT.Managers
{
    /// <summary>
    /// Singleton class for managing audio related to the game including music and SFX
    /// </summary>
    public class AudioManager : GenericSingleton<AudioManager>
    {
        //Implementation: Songs and SFX are limited so just load all as assets and have playable through methods?

        //TODO 1: Just have a single song imported and playing

        //TODO 2: Have multiple songs imported and shuffle through

        //TODO 3: Expose volume options for UI

        //TODO 3: Have SFX play based on game events like UI clicks and game state
    }
}


