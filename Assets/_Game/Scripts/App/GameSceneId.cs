using System;

public enum GameSceneId
{
    None,
    LegacySampleScene,
    MainMenuScene,
    WorldSimScene,
    DungeonRunScene,
    BattleScene,
    ResultScene
}

public static class GameSceneIds
{
    public const string LegacySampleSceneName = "SampleScene";
    public const string MainMenuSceneName = "MainMenuScene";
    public const string WorldSimSceneName = "WorldSimScene";
    public const string DungeonRunSceneName = "DungeonRunScene";
    public const string BattleSceneName = "BattleScene";
    public const string ResultSceneName = "ResultScene";

    public static string ToSceneName(GameSceneId sceneId)
    {
        return sceneId == GameSceneId.LegacySampleScene
            ? LegacySampleSceneName
            : sceneId == GameSceneId.MainMenuScene
                ? MainMenuSceneName
                : sceneId == GameSceneId.WorldSimScene
                    ? WorldSimSceneName
                    : sceneId == GameSceneId.DungeonRunScene
                        ? DungeonRunSceneName
                        : sceneId == GameSceneId.BattleScene
                            ? BattleSceneName
                            : sceneId == GameSceneId.ResultScene
                                ? ResultSceneName
                                : string.Empty;
    }

    public static GameSceneId FromSceneName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return GameSceneId.None;
        }

        string normalized = sceneName.Trim();
        return string.Equals(normalized, LegacySampleSceneName, StringComparison.Ordinal)
            ? GameSceneId.LegacySampleScene
            : string.Equals(normalized, MainMenuSceneName, StringComparison.Ordinal)
                ? GameSceneId.MainMenuScene
                : string.Equals(normalized, WorldSimSceneName, StringComparison.Ordinal)
                    ? GameSceneId.WorldSimScene
                    : string.Equals(normalized, DungeonRunSceneName, StringComparison.Ordinal)
                        ? GameSceneId.DungeonRunScene
                        : string.Equals(normalized, BattleSceneName, StringComparison.Ordinal)
                            ? GameSceneId.BattleScene
                            : string.Equals(normalized, ResultSceneName, StringComparison.Ordinal)
                                ? GameSceneId.ResultScene
                                : GameSceneId.None;
    }
}
