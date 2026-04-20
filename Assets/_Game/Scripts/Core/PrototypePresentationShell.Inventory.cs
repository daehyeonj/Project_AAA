using System.Collections.Generic;
using UnityEngine;

public sealed partial class PrototypePresentationShell
{
    private void DrawInventorySurfaceOverlay(Rect screenRect, List<Rect> blockingRects)
    {
        if (_bootEntry == null || !_bootEntry.IsInventorySurfaceOpen)
        {
            return;
        }

        PrototypeRpgInventorySurfaceData surface = _bootEntry.GetInventorySurfaceData();
        Rect backdropRect = screenRect;
        Rect modalRect = CenterRect(
            Mathf.Min(screenRect.width - 48f, 1500f),
            Mathf.Min(screenRect.height - 64f, 860f));

        DrawRect(backdropRect, new Color(0.02f, 0.03f, 0.06f, 0.82f));
        DrawPanel(modalRect, new Color(0.16f, 0.24f, 0.32f, 0.98f), new Color(0.07f, 0.10f, 0.14f, 0.98f));

        if (blockingRects != null)
        {
            blockingRects.Add(backdropRect);
            blockingRects.Add(modalRect);
        }

        Rect innerRect = Inset(modalRect, 16f);
        Rect headerRect = new Rect(innerRect.x, innerRect.y, innerRect.width, 72f);
        Rect footerRect = new Rect(innerRect.x, innerRect.yMax - 92f, innerRect.width, 92f);
        Rect bodyRect = new Rect(innerRect.x, headerRect.yMax + 12f, innerRect.width, footerRect.y - headerRect.yMax - 24f);

        DrawInventoryHeader(headerRect, surface);
        DrawInventoryBody(bodyRect, surface);
        DrawInventoryFooter(footerRect, surface);
    }

    private void DrawInventoryHeader(Rect rect, PrototypeRpgInventorySurfaceData surface)
    {
        DrawPanel(rect, new Color(0.14f, 0.20f, 0.28f, 0.98f), new Color(0.09f, 0.12f, 0.18f, 0.94f));
        GUI.Label(new Rect(rect.x + 18f, rect.y + 10f, rect.width * 0.42f, 26f), SafeShellText(surface.PanelTitleText), _heroSubtitleStyle);
        GUI.Label(
            new Rect(rect.x + 18f, rect.y + 38f, rect.width * 0.48f, rect.height - 44f),
            BuildDisplayBlock(
                SafeShellText(surface.ContextLabel) + "\n" +
                SafeShellText(surface.InputHintText),
                2,
                124),
            _captionStyle);

        string[] chips =
        {
            BuildDungeonChip("Party", surface.PartyLabel),
            BuildDungeonChip(surface.IsReadOnly ? "Mode" : "Action", surface.IsReadOnly ? "Inspect Only" : "Manual Equip"),
            BuildDungeonChip("Summary", surface.InventorySummaryText),
            BuildDungeonChip("Feedback", surface.FeedbackText)
        };

        float rightX = rect.xMax - 18f;
        for (int i = chips.Length - 1; i >= 0; i--)
        {
            if (!HasMeaningfulValue(chips[i]))
            {
                continue;
            }

            float chipWidth = GetCachedBadgeWidth(chips[i], 20f, 112f, 260f);
            Rect chipRect = new Rect(rightX - chipWidth, rect.y + 12f, chipWidth, 28f);
            DrawPill(chipRect, chips[i], new Color(0.18f, 0.28f, 0.38f, 0.96f), Color.white);
            rightX = chipRect.x - 8f;
        }

        Rect closeRect = new Rect(rect.xMax - 144f, rect.yMax - 36f, 126f, 28f);
        if (DrawActionButton(closeRect, "[Esc] Close", new Color(0.22f, 0.18f, 0.18f, 1f), true, _badgeStyle))
        {
            _bootEntry.CloseInventorySurface();
        }
    }

    private void DrawInventoryBody(Rect rect, PrototypeRpgInventorySurfaceData surface)
    {
        float columnGap = 14f;
        float membersWidth = Mathf.Clamp(rect.width * 0.21f, 244f, 296f);
        float centerWidth = Mathf.Clamp(rect.width * 0.33f, 360f, 460f);
        float itemsWidth = rect.width - membersWidth - centerWidth - (columnGap * 2f);

        Rect membersRect = new Rect(rect.x, rect.y, membersWidth, rect.height);
        Rect centerRect = new Rect(membersRect.xMax + columnGap, rect.y, centerWidth, rect.height);
        Rect itemsRect = new Rect(centerRect.xMax + columnGap, rect.y, itemsWidth, rect.height);

        DrawInventoryMembersPane(membersRect, surface);
        DrawInventoryCenterPane(centerRect, surface);
        DrawInventoryItemsPane(itemsRect, surface);
    }

    private void DrawInventoryMembersPane(Rect rect, PrototypeRpgInventorySurfaceData surface)
    {
        DrawPanel(rect, new Color(0.12f, 0.18f, 0.24f, 0.98f), new Color(0.08f, 0.11f, 0.16f, 0.94f));
        GUI.Label(new Rect(rect.x + 14f, rect.y + 12f, rect.width - 28f, 22f), "Party Members", _panelTitleStyle);

        PrototypeRpgInventoryMemberSurfaceData[] members = surface.Members ?? System.Array.Empty<PrototypeRpgInventoryMemberSurfaceData>();
        float cardY = rect.y + 44f;
        float cardHeight = Mathf.Clamp(rect.height * 0.10f, 58f, 68f);
        float cardGap = 8f;
        float listBottom = cardY;
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgInventoryMemberSurfaceData member = members[i] ?? new PrototypeRpgInventoryMemberSurfaceData();
            Rect cardRect = new Rect(rect.x + 14f, cardY + (i * (cardHeight + cardGap)), rect.width - 28f, cardHeight);
            Color fill = member.IsSelected
                ? new Color(0.28f, 0.46f, 0.60f, 1f)
                : new Color(0.14f, 0.19f, 0.26f, 1f);
            string label = CompactShellText(member.DisplayName, 22) + "\n" +
                           CompactShellText(member.RoleIdentityText + " | Lv" + Mathf.Max(1, member.Level), 34);
            if (DrawActionButton(cardRect, label, fill, true))
            {
                _bootEntry.TrySelectInventoryMember(member.MemberId);
            }

            listBottom = cardRect.yMax;
        }

        float summaryHeight = 92f;
        float summaryTop = Mathf.Max(listBottom + 12f, rect.yMax - summaryHeight - 14f);
        Rect summaryRect = new Rect(rect.x + 14f, summaryTop, rect.width - 28f, rect.yMax - summaryTop - 14f);
        DrawPanel(summaryRect, new Color(0.16f, 0.22f, 0.28f, 0.96f), new Color(0.10f, 0.13f, 0.18f, 0.92f));
        GUI.Label(new Rect(summaryRect.x + 12f, summaryRect.y + 10f, summaryRect.width - 24f, 18f), "Selected", _captionStyle);
        GUI.Label(
            new Rect(summaryRect.x + 12f, summaryRect.y + 30f, summaryRect.width - 24f, summaryRect.height - 42f),
            BuildDisplayBlock(BuildInventoryMemberSummaryText(surface), 3, 34),
            _bodyStyle);
    }

    private void DrawInventoryCenterPane(Rect rect, PrototypeRpgInventorySurfaceData surface)
    {
        DrawPanel(rect, new Color(0.12f, 0.18f, 0.24f, 0.98f), new Color(0.08f, 0.11f, 0.16f, 0.94f));
        GUI.Label(new Rect(rect.x + 14f, rect.y + 12f, rect.width - 28f, 22f), "Equipment Slots", _panelTitleStyle);

        float contentWidth = rect.width - 28f;
        float slotsHeight = GetInventorySlotsPanelHeight(surface);
        Rect slotsRect = new Rect(rect.x + 14f, rect.y + 42f, contentWidth, slotsHeight);
        DrawInventorySlotsGrid(slotsRect, surface);

        string statusText = BuildInventoryCenterStatusText(surface);
        float statusHeight = Mathf.Clamp(_bodyStyle.CalcHeight(new GUIContent(statusText), Mathf.Max(24f, contentWidth - 24f)) + 38f, 82f, 112f);
        Rect statusRect = new Rect(rect.x + 14f, slotsRect.yMax + 12f, contentWidth, statusHeight);
        DrawPanel(statusRect, new Color(0.18f, 0.24f, 0.32f, 0.96f), new Color(0.10f, 0.13f, 0.18f, 0.92f));
        GUI.Label(new Rect(statusRect.x + 12f, statusRect.y + 10f, statusRect.width - 24f, 18f), "Selection Status", _captionStyle);
        GUI.Label(
            new Rect(statusRect.x + 12f, statusRect.y + 30f, statusRect.width - 24f, statusRect.height - 40f),
            statusText,
            _bodyStyle);

        Rect actionsRect = new Rect(rect.x + 14f, statusRect.yMax + 12f, contentWidth, rect.yMax - statusRect.yMax - 26f);
        DrawPanel(actionsRect, new Color(0.14f, 0.20f, 0.28f, 0.96f), new Color(0.09f, 0.12f, 0.16f, 0.92f));
        string actionSummaryText = BuildInventoryActionSummary(surface);
        GUI.Label(
            new Rect(actionsRect.x + 12f, actionsRect.y + 10f, actionsRect.width - 24f, 34f),
            BuildDisplayBlock(actionSummaryText, 2, 52),
            _captionStyle);

        float buttonY = actionsRect.yMax - 48f;
        float buttonGap = 10f;
        float buttonWidth = (actionsRect.width - 24f - (buttonGap * 2f)) / 3f;
        Rect equipRect = new Rect(actionsRect.x + 12f, buttonY, buttonWidth, 34f);
        Rect unequipRect = new Rect(equipRect.xMax + buttonGap, buttonY, buttonWidth, 34f);
        Rect closeRect = new Rect(unequipRect.xMax + buttonGap, buttonY, buttonWidth, 34f);

        if (DrawActionButton(equipRect, "[Enter] Equip", new Color(0.24f, 0.48f, 0.34f, 1f), surface.CanEquipSelectedItem && !surface.IsReadOnly, _badgeStyle))
        {
            _bootEntry.TryConfirmInventoryEquip();
        }

        if (DrawActionButton(unequipRect, "[U] Unequip", new Color(0.34f, 0.28f, 0.18f, 1f), surface.CanUnequipSelectedSlot && !surface.IsReadOnly, _badgeStyle))
        {
            _bootEntry.TryUnequipSelectedInventorySlot();
        }

        if (DrawActionButton(closeRect, "Close", new Color(0.18f, 0.20f, 0.24f, 1f), true, _badgeStyle))
        {
            _bootEntry.CloseInventorySurface();
        }
    }

    private void DrawInventorySlotsGrid(Rect rect, PrototypeRpgInventorySurfaceData surface)
    {
        PrototypeRpgInventorySlotSurfaceData[] slots = surface.Slots ?? System.Array.Empty<PrototypeRpgInventorySlotSurfaceData>();
        float gap = 8f;
        float columns = 2f;
        float cardWidth = (rect.width - gap) / columns;
        float cardHeight = 54f;
        for (int i = 0; i < slots.Length; i++)
        {
            PrototypeRpgInventorySlotSurfaceData slot = slots[i] ?? new PrototypeRpgInventorySlotSurfaceData();
            int row = i / 2;
            int column = i % 2;
            Rect cardRect = new Rect(rect.x + ((cardWidth + gap) * column), rect.y + ((cardHeight + gap) * row), cardWidth, cardHeight);
            Color fill = slot.IsSelected
                ? new Color(0.30f, 0.46f, 0.64f, 1f)
                : new Color(0.14f, 0.19f, 0.24f, 1f);
            string line1 = CompactShellText(slot.SlotLabel + " " + slot.HotkeyLabel, 30);
            string line2 = CompactShellText(slot.HasEquippedItem ? slot.EquippedItemName : "Empty", 26);
            if (DrawActionButton(cardRect, line1 + "\n" + line2, fill, true))
            {
                _bootEntry.TrySelectInventorySlot(slot.SlotKey);
            }
        }
    }

    private void DrawInventoryItemsPane(Rect rect, PrototypeRpgInventorySurfaceData surface)
    {
        DrawPanel(rect, new Color(0.12f, 0.18f, 0.24f, 0.98f), new Color(0.08f, 0.11f, 0.16f, 0.94f));
        GUI.Label(new Rect(rect.x + 14f, rect.y + 12f, rect.width - 28f, 22f), "Party Inventory", _panelTitleStyle);

        float detailsHeight = Mathf.Clamp(rect.height * 0.30f, 180f, 240f);
        Rect listRect = new Rect(rect.x + 14f, rect.y + 42f, rect.width - 28f, rect.height - detailsHeight - 68f);
        Rect detailsRect = new Rect(rect.x + 14f, listRect.yMax + 12f, rect.width - 28f, detailsHeight);
        DrawInventoryItemsList(listRect, surface);

        DrawPanel(detailsRect, new Color(0.18f, 0.24f, 0.32f, 0.96f), new Color(0.10f, 0.13f, 0.18f, 0.92f));
        GUI.Label(new Rect(detailsRect.x + 12f, detailsRect.y + 10f, detailsRect.width - 24f, 22f), SafeShellText(surface.SelectedItemHeaderText), _panelTitleStyle);
        GUI.Label(new Rect(detailsRect.x + 12f, detailsRect.y + 32f, detailsRect.width - 24f, 18f), SafeShellText(surface.SelectedItemMetaText), _captionStyle);
        DrawScrollableTextBlock(
            new Rect(detailsRect.x + 12f, detailsRect.y + 54f, detailsRect.width - 24f, detailsRect.height - 66f),
            "inventory:item_details",
            BuildInventoryItemDetailsText(surface),
            _bodyStyle);
    }

    private void DrawInventoryItemsList(Rect rect, PrototypeRpgInventorySurfaceData surface)
    {
        PrototypeRpgInventoryItemSurfaceData[] items = surface.InventoryItems ?? System.Array.Empty<PrototypeRpgInventoryItemSurfaceData>();
        DrawPanel(rect, new Color(0.16f, 0.22f, 0.28f, 0.96f), new Color(0.09f, 0.12f, 0.16f, 0.92f));

        Rect viewportRect = new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f);
        float cardHeight = 60f;
        float contentHeight = Mathf.Max(viewportRect.height, items.Length * (cardHeight + 8f));
        float contentWidth = Mathf.Max(24f, viewportRect.width - 18f);
        Vector2 scrollPosition = GetShellScrollPosition("inventory:items");
        Vector2 nextScrollPosition = GUI.BeginScrollView(
            viewportRect,
            scrollPosition,
            new Rect(0f, 0f, contentWidth, contentHeight),
            false,
            contentHeight > viewportRect.height + 0.5f);

        for (int i = 0; i < items.Length; i++)
        {
            PrototypeRpgInventoryItemSurfaceData item = items[i] ?? new PrototypeRpgInventoryItemSurfaceData();
            Rect buttonRect = new Rect(0f, i * (cardHeight + 8f), contentWidth - 4f, cardHeight);
            Color fill = item.IsSelected
                ? new Color(0.30f, 0.46f, 0.64f, 1f)
                : item.IsCompatible
                    ? new Color(0.16f, 0.22f, 0.28f, 1f)
                    : new Color(0.14f, 0.16f, 0.18f, 1f);
            string stateTag = item.IsEquipped
                ? "Equipped"
                : item.IsCompatible
                    ? "Stored"
                    : "Mismatch";
            string label = CompactShellText(item.DisplayName, 34) + "\n" +
                           CompactShellText(item.SlotLabel + " | " + item.TierLabel + " | " + stateTag, 54);
            if (DrawActionButton(buttonRect, label, fill, true))
            {
                _bootEntry.TrySelectInventoryItem(item.ItemInstanceId);
            }
        }

        GUI.EndScrollView();
        if (scrollPosition != nextScrollPosition || nextScrollPosition != Vector2.zero)
        {
            _shellScrollByKey["inventory:items"] = nextScrollPosition;
        }
        else
        {
            _shellScrollByKey.Remove("inventory:items");
        }
    }

    private void DrawInventoryFooter(Rect rect, PrototypeRpgInventorySurfaceData surface)
    {
        DrawPanel(rect, new Color(0.12f, 0.18f, 0.24f, 0.98f), new Color(0.08f, 0.11f, 0.16f, 0.94f));
        Rect leftRect = new Rect(rect.x + 12f, rect.y + 10f, rect.width * 0.48f, rect.height - 20f);
        Rect rightRect = new Rect(rect.x + (rect.width * 0.50f), rect.y + 10f, rect.width * 0.48f - 12f, rect.height - 20f);

        DrawScrollableTextBlock(
            leftRect,
            "inventory:footer_left",
            BuildDungeonLines(
                "Feedback: " + SafeShellText(surface.FeedbackText),
                "Manual Action: " + SafeShellText(surface.LatestManualActionSummaryText),
                "Continuity: " + SafeShellText(surface.LatestContinuitySummaryText)),
            _bodyStyle);

        DrawScrollableTextBlock(
            rightRect,
            "inventory:footer_right",
            BuildDungeonLines(
                "Latest Reward: " + SafeShellText(surface.LatestRewardSummaryText),
                "Latest Auto Equip: " + SafeShellText(surface.LatestAutoEquipSummaryText),
                "Footer: " + SafeShellText(surface.FooterSummaryText)),
            _bodyStyle);
    }

    private string BuildInventoryMemberSummaryText(PrototypeRpgInventorySurfaceData surface)
    {
        PrototypeRpgInventoryMemberSurfaceData[] members = surface.Members ?? System.Array.Empty<PrototypeRpgInventoryMemberSurfaceData>();
        for (int i = 0; i < members.Length; i++)
        {
            PrototypeRpgInventoryMemberSurfaceData member = members[i];
            if (member != null && member.IsSelected)
            {
                return BuildDungeonLines(
                    surface.SelectedMemberHeaderText,
                    surface.SelectedMemberProgressText,
                    surface.SelectedMemberRoleText,
                    "Gear Pref: " + SafeShellText(surface.SelectedMemberGearPreferenceText),
                    member.SummaryText);
            }
        }

        return "No member selected.";
    }

    private float GetInventorySlotsPanelHeight(PrototypeRpgInventorySurfaceData surface)
    {
        int slotCount = surface != null && surface.Slots != null ? surface.Slots.Length : 0;
        int rowCount = Mathf.Max(1, Mathf.CeilToInt(slotCount / 2f));
        float cardHeight = 54f;
        float gap = 8f;
        return Mathf.Clamp((rowCount * cardHeight) + ((rowCount - 1) * gap), 54f, 256f);
    }

    private string BuildInventoryCenterStatusText(PrototypeRpgInventorySurfaceData surface)
    {
        List<string> lines = new List<string>();
        lines.Add("Member: " + SafeShellText(surface.SelectedMemberHeaderText));
        lines.Add("Progress: " + SafeShellText(surface.SelectedMemberProgressText));
        lines.Add("Role: " + SafeShellText(surface.SelectedMemberRoleText));
        lines.Add("Gear Pref: " + SafeShellText(surface.SelectedMemberGearPreferenceText));

        bool hasSelectedItem = HasMeaningfulValue(surface.SelectedItemHeaderText) &&
                               !string.Equals(SafeShellText(surface.SelectedItemHeaderText), "Select an item.", System.StringComparison.OrdinalIgnoreCase);
        if (hasSelectedItem)
        {
            lines.Add("Selection: " + SafeShellText(surface.SelectedItemHeaderText) + " | " + SafeShellText(surface.SelectedItemMetaText));
            lines.Add("State: " + SafeShellText(surface.SelectedItemOwnerText));
        }
        else
        {
            lines.Add("Selection: No item selected.");
            lines.Add("State: Choose stored gear to inspect the slot.");
        }

        return BuildDungeonLines(lines.ToArray());
    }

    private string BuildInventoryActionSummary(PrototypeRpgInventorySurfaceData surface)
    {
        if (surface == null)
        {
            return "No action available.";
        }

        if (surface.IsReadOnly)
        {
            return "Battle inspection only. Equipment changes locked.";
        }

        PrototypeRpgInventoryComparisonSurfaceData comparison = surface.Comparison ?? new PrototypeRpgInventoryComparisonSurfaceData();
        bool hasCandidate = HasMeaningfulValue(comparison.CandidateText) &&
                            !string.Equals(SafeShellText(comparison.CandidateText), "Candidate: None", System.StringComparison.OrdinalIgnoreCase);
        if (!hasCandidate)
        {
            return surface.CanUnequipSelectedSlot
                ? "Select an item or use [U] to clear the selected slot."
                : "Select stored gear to inspect and equip it.";
        }

        if (HasMeaningfulValue(comparison.ReasonText) &&
            !string.Equals(SafeShellText(comparison.ReasonText), "Select an item.", System.StringComparison.OrdinalIgnoreCase))
        {
            return comparison.ReasonText;
        }

        return HasMeaningfulValue(comparison.ActionHintText)
            ? comparison.ActionHintText
            : "No action available.";
    }

    private string BuildInventoryItemDetailsText(PrototypeRpgInventorySurfaceData surface)
    {
        return BuildDungeonLines(
            surface.SelectedItemDetailText,
            SafeShellText(surface.SelectedItemFitText),
            "Owner: " + SafeShellText(surface.SelectedItemOwnerText),
            "Action: " + BuildInventoryActionSummary(surface),
            "Hint: " + SafeShellText(surface.InputHintText));
    }
}
