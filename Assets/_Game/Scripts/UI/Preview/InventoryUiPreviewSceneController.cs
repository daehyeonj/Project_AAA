using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class InventoryUiPreviewSceneController : MonoBehaviour
{
    [SerializeField] private InventoryUiSkinDefinition _skin;
    [SerializeField] private InventoryUiLayoutProfile _layout;
    [SerializeField] private InventoryUiPreviewData _previewData;

    private GUIStyle _titleStyle;
    private GUIStyle _sectionStyle;
    private GUIStyle _bodyStyle;
    private GUIStyle _captionStyle;

    public void SetDependencies(InventoryUiSkinDefinition skin, InventoryUiLayoutProfile layout, InventoryUiPreviewData previewData)
    {
        _skin = skin;
        _layout = layout;
        _previewData = previewData;
    }

    private void OnGUI()
    {
        EnsureStyles();
        InventoryUiPresenterModel model = InventoryUiPreviewAdapter.BuildPreviewModel(_previewData);
        InventoryUiLayoutProfile layout = _layout != null ? _layout : ScriptableObject.CreateInstance<InventoryUiLayoutProfile>();
        Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
        DrawInventorySlot(screenRect, new Color(0.08f, 0.09f, 0.12f, 1f), _skin != null ? _skin.InventoryBackground : null);

        Rect overlayRect = new Rect(
            (Screen.width * 0.5f) - (layout.OverlayWidth * 0.5f),
            (Screen.height * 0.5f) - (layout.OverlayHeight * 0.5f),
            layout.OverlayWidth,
            layout.OverlayHeight);
        DrawInventorySlot(overlayRect, new Color(0.15f, 0.22f, 0.30f, 0.98f), _skin != null ? _skin.InventoryBackground : null);

        Rect innerRect = Inset(overlayRect, layout.PanelPadding);
        Rect headerRect = new Rect(innerRect.x, innerRect.y, innerRect.width, 72f);
        Rect footerRect = new Rect(innerRect.x, innerRect.yMax - layout.FooterHeight, innerRect.width, layout.FooterHeight);
        Rect bodyRect = new Rect(innerRect.x, headerRect.yMax + 12f, innerRect.width, footerRect.y - headerRect.yMax - 24f);

        DrawHeader(headerRect, model);
        DrawBody(bodyRect, layout, model);
        DrawFooter(footerRect, model);
    }

    private void DrawHeader(Rect rect, InventoryUiPresenterModel model)
    {
        DrawInventorySlot(rect, new Color(0.13f, 0.19f, 0.27f, 0.98f), _skin != null ? _skin.HeaderPanel : null);
        GUI.Label(new Rect(rect.x + 18f, rect.y + 10f, rect.width - 36f, 24f), model.HeaderTitle, _titleStyle);
        GUI.Label(new Rect(rect.x + 18f, rect.y + 38f, rect.width - 36f, 18f), model.PartyLabel + " | " + model.SummaryText, _bodyStyle);

        Rect runSpoilsRect = new Rect(rect.xMax - 332f, rect.y + 10f, 154f, 24f);
        Rect pendingRect = new Rect(rect.xMax - 166f, rect.y + 10f, 148f, 24f);
        DrawBadge(runSpoilsRect, model.RunSpoilsBadgeText, _skin != null ? _skin.RunSpoilsBadge : null, new Color(0.35f, 0.24f, 0.10f, 0.98f));
        DrawBadge(pendingRect, model.PendingExtractionBadgeText, _skin != null ? _skin.PendingExtractionBadge : null, new Color(0.18f, 0.28f, 0.16f, 0.98f));
    }

    private void DrawBody(Rect rect, InventoryUiLayoutProfile layout, InventoryUiPresenterModel model)
    {
        float gap = 14f;
        Rect membersRect = new Rect(rect.x, rect.y, layout.MemberColumnWidth, rect.height);
        Rect equipmentRect = new Rect(membersRect.xMax + gap, rect.y, layout.EquipmentColumnWidth, rect.height);
        Rect inventoryRect = new Rect(equipmentRect.xMax + gap, rect.y, layout.InventoryColumnWidth, rect.height);

        DrawMembers(membersRect, layout, model);
        DrawEquipment(equipmentRect, layout, model);
        DrawInventoryList(inventoryRect, layout, model);
    }

    private void DrawMembers(Rect rect, InventoryUiLayoutProfile layout, InventoryUiPresenterModel model)
    {
        DrawInventorySlot(rect, new Color(0.10f, 0.14f, 0.20f, 0.98f), _skin != null ? _skin.HeaderPanel : null);
        GUI.Label(new Rect(rect.x + 12f, rect.y + 10f, rect.width - 24f, 22f), "Party Members", _sectionStyle);
        float y = rect.y + 42f;
        InventoryUiPresenterModel.Member[] members = model.Members ?? System.Array.Empty<InventoryUiPresenterModel.Member>();
        for (int i = 0; i < members.Length; i++)
        {
            InventoryUiPresenterModel.Member member = members[i];
            Rect rowRect = new Rect(rect.x + 12f, y, rect.width - 24f, layout.RowHeight);
            DrawInventorySlot(rowRect, member != null && member.Selected ? new Color(0.22f, 0.34f, 0.46f, 0.98f) : new Color(0.12f, 0.17f, 0.24f, 0.98f), _skin != null ? _skin.GetMemberRowSlot(member != null && member.Selected) : null);
            GUI.Label(new Rect(rowRect.x + 10f, rowRect.y + 10f, rowRect.width - 20f, 20f), member != null ? member.Name : "Member", _bodyStyle);
            GUI.Label(new Rect(rowRect.x + 10f, rowRect.y + 32f, rowRect.width - 20f, 18f), member != null ? member.Role + " | Lv " + member.Level : "Role", _captionStyle);
            y += layout.RowHeight + 8f;
        }
    }

    private void DrawEquipment(Rect rect, InventoryUiLayoutProfile layout, InventoryUiPresenterModel model)
    {
        DrawInventorySlot(rect, new Color(0.10f, 0.14f, 0.20f, 0.98f), _skin != null ? _skin.HeaderPanel : null);
        GUI.Label(new Rect(rect.x + 12f, rect.y + 10f, rect.width - 24f, 22f), "Equipment Slots", _sectionStyle);
        InventoryUiPresenterModel.EquipmentSlot[] slots = model.Slots ?? System.Array.Empty<InventoryUiPresenterModel.EquipmentSlot>();
        float y = rect.y + 48f;
        float x = rect.x + 12f;
        float maxX = rect.xMax - 12f;
        for (int i = 0; i < slots.Length; i++)
        {
            InventoryUiPresenterModel.EquipmentSlot slot = slots[i];
            Rect slotRect = new Rect(x, y, layout.SlotSize, layout.SlotSize);
            bool selected = slot != null && slot.Selected;
            bool equipped = slot != null && slot.HasItem;
            DrawInventorySlot(slotRect, selected ? new Color(0.26f, 0.42f, 0.58f, 0.98f) : equipped ? new Color(0.18f, 0.26f, 0.36f, 0.98f) : new Color(0.08f, 0.11f, 0.16f, 0.98f), _skin != null ? _skin.GetEquipmentSlot(selected, equipped) : null);
            GUI.Label(new Rect(slotRect.x + 6f, slotRect.y + 8f, slotRect.width - 12f, 18f), slot != null ? slot.SlotLabel : "Slot", _captionStyle);
            GUI.Label(new Rect(slotRect.x + 6f, slotRect.y + 30f, slotRect.width - 12f, slotRect.height - 36f), slot != null ? slot.ItemLabel : "No gear", _captionStyle);
            x += layout.SlotSize + layout.SlotGap;
            if (x + layout.SlotSize > maxX)
            {
                x = rect.x + 12f;
                y += layout.SlotSize + layout.SlotGap;
            }
        }
    }

    private void DrawInventoryList(Rect rect, InventoryUiLayoutProfile layout, InventoryUiPresenterModel model)
    {
        DrawInventorySlot(rect, new Color(0.10f, 0.14f, 0.20f, 0.98f), _skin != null ? _skin.HeaderPanel : null);
        GUI.Label(new Rect(rect.x + 12f, rect.y + 10f, rect.width - 24f, 22f), "Inventory Items", _sectionStyle);
        float listHeight = rect.height - layout.DetailPanelHeight - 24f;
        Rect listRect = new Rect(rect.x + 12f, rect.y + 42f, rect.width - 24f, listHeight - 42f);
        Rect detailRect = new Rect(rect.x + 12f, rect.yMax - layout.DetailPanelHeight - 12f, rect.width - 24f, layout.DetailPanelHeight);

        InventoryUiPresenterModel.InventoryItem[] items = model.Items ?? System.Array.Empty<InventoryUiPresenterModel.InventoryItem>();
        float y = listRect.y;
        for (int i = 0; i < items.Length; i++)
        {
            InventoryUiPresenterModel.InventoryItem item = items[i];
            Rect rowRect = new Rect(listRect.x, y, listRect.width, layout.RowHeight);
            DrawInventorySlot(rowRect, item != null && item.Selected ? new Color(0.24f, 0.36f, 0.50f, 0.98f) : new Color(0.12f, 0.17f, 0.24f, 0.98f), _skin != null ? _skin.GetItemRowSlot(item != null && item.Selected) : null);
            GUI.Label(new Rect(rowRect.x + 10f, rowRect.y + 10f, rowRect.width - 20f, rowRect.height - 20f), item != null ? item.ItemLabel : "Item", _bodyStyle);
            y += layout.RowHeight + 8f;
        }

        DrawInventorySlot(detailRect, new Color(0.08f, 0.12f, 0.18f, 0.98f), _skin != null ? _skin.ItemDetailPanel : null);
        GUI.Label(new Rect(detailRect.x + 12f, detailRect.y + 10f, detailRect.width - 24f, 22f), "Selected Item Detail", _sectionStyle);
        GUI.Label(new Rect(detailRect.x + 12f, detailRect.y + 38f, detailRect.width - 24f, detailRect.height - 48f), model.SelectedItemDetailText, _bodyStyle);
    }

    private void DrawFooter(Rect rect, InventoryUiPresenterModel model)
    {
        DrawInventorySlot(rect, new Color(0.10f, 0.14f, 0.20f, 0.98f), _skin != null ? _skin.FooterPanel : null);
        GUI.Label(new Rect(rect.x + 16f, rect.y + 12f, rect.width - 32f, 22f), "Preview-only inventory presenter scaffold", _sectionStyle);
        GUI.Label(new Rect(rect.x + 16f, rect.y + 38f, rect.width - 32f, 22f), "Manual sprite assignment only. No auto-mapping, no runtime inventory mutation.", _captionStyle);
    }

    private void DrawBadge(Rect rect, string label, InventoryUiSkinDefinition.GraphicSlot slot, Color fallbackColor)
    {
        DrawInventorySlot(rect, fallbackColor, slot);
        GUI.Label(rect, label, _captionStyle);
    }

    private void DrawInventorySlot(Rect rect, Color fallbackColor, InventoryUiSkinDefinition.GraphicSlot slot)
    {
        if (_skin == null || slot == null || !InventoryUiSkinRenderer.TryDrawGraphic(rect, slot))
        {
            DrawRect(rect, fallbackColor);
        }
    }

    private static Rect Inset(Rect rect, float padding)
    {
        return new Rect(rect.x + padding, rect.y + padding, rect.width - (padding * 2f), rect.height - (padding * 2f));
    }

    private static void DrawRect(Rect rect, Color color)
    {
        Color previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private void EnsureStyles()
    {
        if (_titleStyle != null)
        {
            return;
        }

        _titleStyle = CreateStyle(20, FontStyle.Bold, TextAnchor.UpperLeft, false);
        _sectionStyle = CreateStyle(14, FontStyle.Bold, TextAnchor.UpperLeft, false);
        _bodyStyle = CreateStyle(12, FontStyle.Normal, TextAnchor.UpperLeft, true);
        _captionStyle = CreateStyle(11, FontStyle.Normal, TextAnchor.MiddleCenter, true);
    }

    private static GUIStyle CreateStyle(int fontSize, FontStyle fontStyle, TextAnchor alignment, bool wordWrap)
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = fontStyle,
            alignment = alignment,
            wordWrap = wordWrap,
            normal = { textColor = Color.white }
        };
    }
}
