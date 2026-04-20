using UnityEngine;

public static class BattleUiSkinProvider
{
    private static BattleUiSkinDefinition _currentBattleSkin;
    private static BattleUiSkinDefinition _fallbackBattleSkin;

    public static BattleUiSkinDefinition CurrentBattleSkin
    {
        get
        {
            if (_currentBattleSkin != null)
            {
                return _currentBattleSkin;
            }

            return GetFallbackSkin();
        }
    }

    public static BattleUiSkinDefinition Resolve(BattleUiSkinDefinition assignedSkin)
    {
        if (assignedSkin != null)
        {
            _currentBattleSkin = assignedSkin;
            return assignedSkin;
        }

        return CurrentBattleSkin;
    }

    public static void Register(BattleUiSkinDefinition assignedSkin)
    {
        if (assignedSkin != null)
        {
            _currentBattleSkin = assignedSkin;
        }
    }

    private static BattleUiSkinDefinition GetFallbackSkin()
    {
        if (_fallbackBattleSkin != null)
        {
            return _fallbackBattleSkin;
        }

        _fallbackBattleSkin = ScriptableObject.CreateInstance<BattleUiSkinDefinition>();
        _fallbackBattleSkin.name = "BattleUiSkinFallback";
        _fallbackBattleSkin.hideFlags = HideFlags.HideAndDontSave;
        return _fallbackBattleSkin;
    }
}
