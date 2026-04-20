using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameSessionRoot : MonoBehaviour
{
    [SerializeField] private RuntimeGameState _runtimeState = new RuntimeGameState();

    private SceneFlowService _sceneFlowService;

    public RuntimeGameState RuntimeState => _runtimeState;
    public SceneFlowService SceneFlowService => _sceneFlowService;

    private void Awake()
    {
        EnsureServices();
        RefreshSceneBinding();
    }

    public void RefreshSceneBinding()
    {
        EnsureServices();
        _sceneFlowService.SyncWithActiveScene();
        _runtimeState.SetActiveScene(_sceneFlowService.CurrentSceneId, _sceneFlowService.CurrentSceneName);
    }

    public void RecordAppFlowStage(AppFlowStage stage, string lastTransition = null)
    {
        EnsureServices();
        _runtimeState.SetCurrentStage(stage, lastTransition);
    }

    public void RecordLegacyGameState(GameStateId stateId, string lastTransition = null)
    {
        EnsureServices();
        _runtimeState.SetLegacyState(stateId, lastTransition);
    }

    public void RecordLanguage(PrototypeLanguage language)
    {
        EnsureServices();
        _runtimeState.SetLanguage(language);
    }

    private void EnsureServices()
    {
        if (_runtimeState == null)
        {
            _runtimeState = new RuntimeGameState();
        }

        if (_sceneFlowService == null)
        {
            _sceneFlowService = new SceneFlowService();
        }
    }
}
