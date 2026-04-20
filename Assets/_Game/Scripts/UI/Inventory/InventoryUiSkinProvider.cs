using UnityEngine;

public static class InventoryUiSkinProvider
{
    private static InventoryUiSkinDefinition _currentInventorySkin;
    private static InventoryUiSkinDefinition _fallbackInventorySkin;

    public static InventoryUiSkinDefinition CurrentInventorySkin
    {
        get
        {
            if (_currentInventorySkin != null)
            {
                return _currentInventorySkin;
            }

            return GetFallbackSkin();
        }
    }

    public static InventoryUiSkinDefinition Resolve(InventoryUiSkinDefinition assignedSkin)
    {
        if (assignedSkin != null)
        {
            _currentInventorySkin = assignedSkin;
            return assignedSkin;
        }

        return CurrentInventorySkin;
    }

    public static void Register(InventoryUiSkinDefinition assignedSkin)
    {
        if (assignedSkin != null)
        {
            _currentInventorySkin = assignedSkin;
        }
    }

    private static InventoryUiSkinDefinition GetFallbackSkin()
    {
        if (_fallbackInventorySkin != null)
        {
            return _fallbackInventorySkin;
        }

        _fallbackInventorySkin = ScriptableObject.CreateInstance<InventoryUiSkinDefinition>();
        _fallbackInventorySkin.name = "InventoryUiSkinFallback";
        _fallbackInventorySkin.hideFlags = HideFlags.HideAndDontSave;
        return _fallbackInventorySkin;
    }
}
