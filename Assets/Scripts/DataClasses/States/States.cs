namespace TTT.DataClasses.States
{
    /// <summary>
    /// The over-all state of the game.
    /// Should represent a high-level, system-wide view.
    /// </summary>
    public enum SystemState
    {
        MAIN_MENU,
        SAVING,
        LOADING,

        // For multiplayer
        LOBBY,
        PLAYING,
        EXITING,
    }

    /// <summary>
    /// Should be changed when the user selects the game mode in the main menu.
    /// Allows for changing logic based on how this flag is set.
    /// </summary>
    public enum GameType
    {
        SIMULATION,
        SCENARIO,
        SOCIAL,
    }

    /// <summary>
    /// The states of the game.
    /// </summary>
    public enum GameState
    {
        PLAYING,

        // Turn end can be used in Social games to check
        // if all players have played.
        TURN_END,
        SEASON_END,
        YEAR_END,
        FLOODING,
    }
}
