using UnityEngine;

[DisallowMultipleComponent]
public sealed class RuntimeUiSkinBridge : MonoBehaviour
{
    [SerializeField] private BattleUiSkinDefinition _battleUiSkin;
    [SerializeField] private InventoryUiSkinDefinition _inventoryUiSkin;

    private void Awake()
    {
        RegisterSkins();
    }

    private void OnEnable()
    {
        RegisterSkins();
    }

    private void RegisterSkins()
    {
        BattleUiSkinProvider.Register(_battleUiSkin);
        InventoryUiSkinProvider.Register(_inventoryUiSkin);
    }
}
