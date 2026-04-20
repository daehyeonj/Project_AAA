using UnityEngine;

[CreateAssetMenu(menuName = "Project AAA/UI/Inventory UI Layout Profile", fileName = "InventoryUiLayout")]
public sealed class InventoryUiLayoutProfile : ScriptableObject
{
    [Header("Overlay")]
    public float OverlayWidth = 1500f;
    public float OverlayHeight = 860f;
    public float PanelPadding = 16f;
    public float FooterHeight = 92f;

    [Header("Columns")]
    public float MemberColumnWidth = 280f;
    public float EquipmentColumnWidth = 420f;
    public float InventoryColumnWidth = 520f;

    [Header("Rows / Slots")]
    public float SlotSize = 84f;
    public float SlotGap = 10f;
    public float RowHeight = 64f;
    public float DetailPanelHeight = 180f;
}
