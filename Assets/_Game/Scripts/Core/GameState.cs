public enum GameStateId
{
    Boot,
    MainMenu,
    WorldSim,
    DungeonRun
}

public sealed class GameState
{
    public GameStateId CurrentState { get; private set; }

    public string LastTransition { get; private set; }

    public GameState(GameStateId initialState)
    {
        CurrentState = initialState;
        LastTransition = "(none yet)";
    }

    public bool ChangeState(GameStateId nextState)
    {
        if (CurrentState == nextState)
        {
            return false;
        }

        LastTransition = CurrentState + " -> " + nextState;
        CurrentState = nextState;
        return true;
    }
}

