using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneFlowService
{
    private readonly Dictionary<GameSceneId, string> _sceneNamesById = new Dictionary<GameSceneId, string>();
    private string _lastTransition = "(none yet)";

    public GameSceneId CurrentSceneId { get; private set; } = GameSceneId.None;
    public string CurrentSceneName { get; private set; } = string.Empty;
    public string LastTransition => _lastTransition;

    public SceneFlowService()
    {
        RegisterDefaultSceneNames();
        SyncWithActiveScene();
    }

    public void RegisterScene(GameSceneId sceneId, string sceneName)
    {
        if (sceneId == GameSceneId.None || string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        _sceneNamesById[sceneId] = sceneName.Trim();
    }

    public bool TryResolveSceneName(GameSceneId sceneId, out string sceneName)
    {
        if (sceneId == GameSceneId.None)
        {
            sceneName = string.Empty;
            return false;
        }

        if (_sceneNamesById.TryGetValue(sceneId, out sceneName) && !string.IsNullOrWhiteSpace(sceneName))
        {
            return true;
        }

        sceneName = GameSceneIds.ToSceneName(sceneId);
        return !string.IsNullOrWhiteSpace(sceneName);
    }

    public void SyncWithActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        string sceneName = activeScene.IsValid() ? activeScene.name : string.Empty;
        RecordScene(GameSceneIds.FromSceneName(sceneName), sceneName);
    }

    public void MarkSceneActive(GameSceneId sceneId)
    {
        string sceneName;
        if (!TryResolveSceneName(sceneId, out sceneName))
        {
            sceneName = string.Empty;
        }

        RecordScene(sceneId, sceneName);
    }

    public bool CanLoad(GameSceneId sceneId)
    {
        string sceneName;
        return TryResolveSceneName(sceneId, out sceneName) &&
               !string.IsNullOrWhiteSpace(sceneName) &&
               Application.CanStreamedLevelBeLoaded(sceneName);
    }

    public bool TryLoadScene(GameSceneId sceneId, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
    {
        string sceneName;
        if (!TryResolveSceneName(sceneId, out sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            return false;
        }

        SceneManager.LoadScene(sceneName, loadSceneMode);
        RecordScene(sceneId, sceneName);
        return true;
    }

    private void RegisterDefaultSceneNames()
    {
        RegisterScene(GameSceneId.LegacySampleScene, GameSceneIds.LegacySampleSceneName);
        RegisterScene(GameSceneId.MainMenuScene, GameSceneIds.MainMenuSceneName);
        RegisterScene(GameSceneId.WorldSimScene, GameSceneIds.WorldSimSceneName);
        RegisterScene(GameSceneId.DungeonRunScene, GameSceneIds.DungeonRunSceneName);
        RegisterScene(GameSceneId.BattleScene, GameSceneIds.BattleSceneName);
        RegisterScene(GameSceneId.ResultScene, GameSceneIds.ResultSceneName);
    }

    private void RecordScene(GameSceneId nextSceneId, string nextSceneName)
    {
        string normalizedName = string.IsNullOrWhiteSpace(nextSceneName) ? string.Empty : nextSceneName.Trim();
        if (CurrentSceneId == nextSceneId && string.Equals(CurrentSceneName, normalizedName))
        {
            return;
        }

        string previousLabel = !string.IsNullOrWhiteSpace(CurrentSceneName)
            ? CurrentSceneName
            : CurrentSceneId.ToString();
        string nextLabel = !string.IsNullOrWhiteSpace(normalizedName)
            ? normalizedName
            : nextSceneId.ToString();

        _lastTransition = previousLabel + " -> " + nextLabel;
        CurrentSceneId = nextSceneId;
        CurrentSceneName = normalizedName;
    }
}
