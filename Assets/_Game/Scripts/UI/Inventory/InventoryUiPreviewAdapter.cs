using System;

public static class InventoryUiPreviewAdapter
{
    public static InventoryUiPresenterModel BuildPreviewModel(InventoryUiPreviewData previewData)
    {
        InventoryUiPresenterModel model = new InventoryUiPresenterModel();
        if (previewData == null)
        {
            return model;
        }

        model.HeaderTitle = SafeText(previewData.HeaderTitle, "Character Equipment");
        model.PartyLabel = SafeText(previewData.PartyLabel, "Party");
        model.SummaryText = SafeText(previewData.SummaryText, "Preview");
        model.RunSpoilsBadgeText = SafeText(previewData.RunSpoilsBadgeText, "Run Spoils pending extraction");
        model.PendingExtractionBadgeText = SafeText(previewData.PendingExtractionBadgeText, "Pending Extraction");
        model.SelectedItemDetailText = SafeText(previewData.SelectedItemDetailText, "Item detail pending.");
        model.Members = BuildMembers(previewData.Members);
        model.Slots = BuildSlots(previewData.Slots);
        model.Items = BuildItems(previewData.Items);
        return model;
    }

    private static InventoryUiPresenterModel.Member[] BuildMembers(InventoryUiPreviewData.Member[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<InventoryUiPresenterModel.Member>();
        }

        InventoryUiPresenterModel.Member[] result = new InventoryUiPresenterModel.Member[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            InventoryUiPreviewData.Member member = source[i] ?? new InventoryUiPreviewData.Member();
            result[i] = new InventoryUiPresenterModel.Member
            {
                Name = SafeText(member.Name, "Member"),
                Role = SafeText(member.Role, "Role"),
                Level = member.Level,
                Selected = member.Selected
            };
        }

        return result;
    }

    private static InventoryUiPresenterModel.EquipmentSlot[] BuildSlots(InventoryUiPreviewData.EquipmentSlot[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<InventoryUiPresenterModel.EquipmentSlot>();
        }

        InventoryUiPresenterModel.EquipmentSlot[] result = new InventoryUiPresenterModel.EquipmentSlot[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            InventoryUiPreviewData.EquipmentSlot slot = source[i] ?? new InventoryUiPreviewData.EquipmentSlot();
            result[i] = new InventoryUiPresenterModel.EquipmentSlot
            {
                SlotLabel = SafeText(slot.SlotLabel, "Slot"),
                ItemLabel = SafeText(slot.ItemLabel, "No gear"),
                HasItem = slot.HasItem,
                Selected = slot.Selected
            };
        }

        return result;
    }

    private static InventoryUiPresenterModel.InventoryItem[] BuildItems(InventoryUiPreviewData.InventoryItem[] source)
    {
        if (source == null || source.Length <= 0)
        {
            return Array.Empty<InventoryUiPresenterModel.InventoryItem>();
        }

        InventoryUiPresenterModel.InventoryItem[] result = new InventoryUiPresenterModel.InventoryItem[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            InventoryUiPreviewData.InventoryItem item = source[i] ?? new InventoryUiPreviewData.InventoryItem();
            result[i] = new InventoryUiPresenterModel.InventoryItem
            {
                ItemLabel = SafeText(item.ItemLabel, "Item"),
                DetailText = SafeText(item.DetailText, "No detail"),
                Selected = item.Selected
            };
        }

        return result;
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
