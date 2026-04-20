using UnityEngine;

[System.Serializable]
public sealed class RuntimeGameState
{
    [SerializeField] private AppFlowStage _currentStage = AppFlowStage.Boot;
    [SerializeField] private GameSceneId _activeSceneId = GameSceneId.None;
    [SerializeField] private string _activeSceneName = string.Empty;
    [SerializeField] private PrototypeLanguage _currentLanguage = PrototypeLanguage.English;
    [SerializeField] private GameStateId _legacyGameStateId = GameStateId.Boot;
    [SerializeField] private string _lastTransition = "(none yet)";

    public AppFlowStage CurrentStage => _currentStage;
    public GameSceneId ActiveSceneId => _activeSceneId;
    public string ActiveSceneName => _activeSceneName;
    public PrototypeLanguage CurrentLanguage => _currentLanguage;
    public GameStateId LegacyGameStateId => _legacyGameStateId;
    public string LastTransition => _lastTransition;

    public void SetCurrentStage(AppFlowStage stage, string lastTransition = null)
    {
        _currentStage = stage;
        if (!string.IsNullOrWhiteSpace(lastTransition))
        {
            _lastTransition = lastTransition.Trim();
        }
    }

    public void SetActiveScene(GameSceneId sceneId, string sceneName)
    {
        _activeSceneId = sceneId;
        _activeSceneName = string.IsNullOrWhiteSpace(sceneName) ? string.Empty : sceneName.Trim();
    }

    public void SetLanguage(PrototypeLanguage language)
    {
        _currentLanguage = language;
    }

    public void SetLegacyState(GameStateId stateId, string lastTransition = null)
    {
        _legacyGameStateId = stateId;
        if (!string.IsNullOrWhiteSpace(lastTransition))
        {
            _lastTransition = lastTransition.Trim();
        }
    }
}
