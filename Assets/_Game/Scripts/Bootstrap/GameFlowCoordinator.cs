public sealed class GameFlowCoordinator
{
    private const float DefaultBootToMainMenuDelaySeconds = 0.85f;

    private readonly GameState _gameState;
    private readonly System.Action<string> _log;
    private readonly float _bootToMainMenuDelaySeconds;
    private readonly UnityEngine.Color _bootColor;
    private readonly UnityEngine.Color _mainMenuColor;
    private readonly UnityEngine.Color _worldSimColor;
    private readonly UnityEngine.Color _dungeonRunColor;

    public GameStateId CurrentState => _gameState != null ? _gameState.CurrentState : GameStateId.Boot;
    public string LastTransition => _gameState != null ? _gameState.LastTransition : "(missing)";

    public GameFlowCoordinator(
        GameState gameState,
        UnityEngine.Color bootColor,
        UnityEngine.Color mainMenuColor,
        UnityEngine.Color worldSimColor,
        UnityEngine.Color dungeonRunColor,
        float bootToMainMenuDelaySeconds = DefaultBootToMainMenuDelaySeconds,
        System.Action<string> log = null)
    {
        _gameState = gameState;
        _log = log;
        _bootToMainMenuDelaySeconds = bootToMainMenuDelaySeconds;
        _bootColor = bootColor;
        _mainMenuColor = mainMenuColor;
        _worldSimColor = worldSimColor;
        _dungeonRunColor = dungeonRunColor;
    }

    public bool TryAdvanceFromBoot(float unscaledTime)
    {
        return CurrentState == GameStateId.Boot &&
               unscaledTime >= _bootToMainMenuDelaySeconds &&
               EnterMainMenu();
    }

    public bool EnterMainMenu()
    {
        return CurrentState == GameStateId.Boot && ChangeState(GameStateId.MainMenu);
    }

    public bool EnterWorldSim()
    {
        return CurrentState == GameStateId.MainMenu && ChangeState(GameStateId.WorldSim);
    }

    public bool ReturnToMainMenu()
    {
        return CurrentState == GameStateId.WorldSim && ChangeState(GameStateId.MainMenu);
    }

    public bool EnterDungeonRun()
    {
        return CurrentState == GameStateId.WorldSim && ChangeState(GameStateId.DungeonRun);
    }

    public bool ExitDungeonRunToWorldSim()
    {
        return CurrentState == GameStateId.DungeonRun && ChangeState(GameStateId.WorldSim);
    }

    public GameFlowPresentationState GetCurrentPresentationState()
    {
        return BuildPresentationState(CurrentState);
    }

    private bool ChangeState(GameStateId nextState)
    {
        if (_gameState == null || !_gameState.ChangeState(nextState))
        {
            return false;
        }

        _log?.Invoke("[GameState] " + _gameState.LastTransition);
        return true;
    }

    private GameFlowPresentationState BuildPresentationState(GameStateId state)
    {
        GameFlowPresentationState presentationState = new GameFlowPresentationState();
        presentationState.CurrentStateId = state;
        presentationState.ShowWorldSim = state == GameStateId.WorldSim;
        presentationState.ShowDungeonRun = state == GameStateId.DungeonRun;
        presentationState.EnableWorldCamera = state == GameStateId.WorldSim;
        presentationState.RequiresDungeonCameraConfigure = state == GameStateId.DungeonRun;
        presentationState.BackgroundColor = state == GameStateId.WorldSim
            ? _worldSimColor
            : state == GameStateId.DungeonRun
                ? _dungeonRunColor
                : state == GameStateId.MainMenu
                    ? _mainMenuColor
                    : _bootColor;
        return presentationState;
    }
}
