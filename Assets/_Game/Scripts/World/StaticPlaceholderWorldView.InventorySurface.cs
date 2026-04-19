public sealed partial class StaticPlaceholderWorldView
{
    private bool _isInventorySurfaceOpen;
    private string _inventorySelectedMemberId = string.Empty;
    private string _inventorySelectedSlotKey = PrototypeRpgEquipmentSlotKeys.Head;
    private string _inventorySelectedItemInstanceId = string.Empty;
    private string _inventoryFeedbackText = "None";
    private PrototypeRpgInventorySurfaceData _cachedInventorySurfaceData = new PrototypeRpgInventorySurfaceData();
    private string _cachedInventoryPartyId = string.Empty;
    private string _cachedInventoryMemberId = string.Empty;
    private string _cachedInventorySlotKey = string.Empty;
    private string _cachedInventoryItemInstanceId = string.Empty;
    private string _cachedInventoryFeedbackText = string.Empty;
    private string _cachedInventoryRunSpoilsKey = string.Empty;
    private int _cachedInventoryRevision = -1;
    private bool _cachedInventoryReadOnly;

    public bool IsInventorySurfaceOpen => _isInventorySurfaceOpen;
    public PrototypeRpgInventorySurfaceData CurrentInventorySurfaceData => BuildInventorySurfaceData();

    public bool TryToggleInventorySurface()
    {
        if (_isInventorySurfaceOpen)
        {
            CloseInventorySurface();
            return true;
        }

        return TryOpenInventorySurface();
    }

    public bool TryOpenInventorySurface()
    {
        string partyId = ResolveCurrentInventoryPartyId();
        if (string.IsNullOrEmpty(partyId))
        {
            _inventoryFeedbackText = "No party inventory is available in the current context.";
            InvalidateInventorySurfaceCache();
            return false;
        }

        _isInventorySurfaceOpen = true;
        _inventoryFeedbackText = IsBattleViewActive
            ? "Battle inspection only. Equipment changes are locked."
            : "Inspect gear, compare candidates, and adjust slots.";
        if (_cachedInventoryPartyId != partyId)
        {
            _inventorySelectedMemberId = string.Empty;
            _inventorySelectedSlotKey = PrototypeRpgEquipmentSlotKeys.Head;
            _inventorySelectedItemInstanceId = string.Empty;
        }

        InvalidateInventorySurfaceCache();
        return true;
    }

    public void CloseInventorySurface()
    {
        _isInventorySurfaceOpen = false;
        _inventoryFeedbackText = "None";
        InvalidateInventorySurfaceCache();
    }

    public bool TryCycleInventoryMember(int direction)
    {
        PrototypeRpgInventorySurfaceData surface = BuildInventorySurfaceData();
        if (!_isInventorySurfaceOpen || surface.Members == null || surface.Members.Length <= 0)
        {
            return false;
        }

        int currentIndex = FindSelectedInventoryMemberIndex(surface);
        int nextIndex = WrapInventoryIndex(currentIndex + direction, surface.Members.Length);
        _inventorySelectedMemberId = surface.Members[nextIndex].MemberId;
        _inventorySelectedItemInstanceId = string.Empty;
        _inventoryFeedbackText = surface.Members[nextIndex].DisplayName + " selected.";
        InvalidateInventorySurfaceCache();
        return true;
    }

    public bool TrySelectInventoryMember(string memberId)
    {
        if (!_isInventorySurfaceOpen || string.IsNullOrEmpty(memberId))
        {
            return false;
        }

        _inventorySelectedMemberId = memberId;
        _inventorySelectedItemInstanceId = string.Empty;
        _inventoryFeedbackText = "Member updated.";
        InvalidateInventorySurfaceCache();
        return true;
    }

    public bool TrySelectInventorySlotByIndex(int slotIndex)
    {
        string[] slotKeys = PrototypeRpgEquipmentSlotKeys.OrderedKeys;
        if (slotIndex < 0 || slotIndex >= slotKeys.Length)
        {
            return false;
        }

        return TrySelectInventorySlot(slotKeys[slotIndex]);
    }

    public bool TrySelectInventorySlot(string slotKey)
    {
        if (!_isInventorySurfaceOpen)
        {
            return false;
        }

        _inventorySelectedSlotKey = string.IsNullOrEmpty(slotKey) ? PrototypeRpgEquipmentSlotKeys.Head : slotKey;
        _inventorySelectedItemInstanceId = string.Empty;
        _inventoryFeedbackText = PrototypeRpgEquipmentSlotKeys.ToDisplayLabel(_inventorySelectedSlotKey) + " selected.";
        InvalidateInventorySurfaceCache();
        return true;
    }

    public bool TryMoveInventoryItemSelection(int direction)
    {
        PrototypeRpgInventorySurfaceData surface = BuildInventorySurfaceData();
        if (!_isInventorySurfaceOpen || surface.InventoryItems == null || surface.InventoryItems.Length <= 0)
        {
            return false;
        }

        int currentIndex = FindSelectedInventoryItemIndex(surface);
        int nextIndex = WrapInventoryIndex(currentIndex + direction, surface.InventoryItems.Length);
        _inventorySelectedItemInstanceId = surface.InventoryItems[nextIndex].ItemInstanceId;
        _inventoryFeedbackText = surface.InventoryItems[nextIndex].DisplayName + " selected.";
        InvalidateInventorySurfaceCache();
        return true;
    }

    public bool TrySelectInventoryItem(string itemInstanceId)
    {
        if (!_isInventorySurfaceOpen || string.IsNullOrEmpty(itemInstanceId))
        {
            return false;
        }

        _inventorySelectedItemInstanceId = itemInstanceId;
        InvalidateInventorySurfaceCache();
        return true;
    }

    public bool TryConfirmInventoryEquip()
    {
        PrototypeRpgInventorySurfaceData surface = BuildInventorySurfaceData();
        if (!_isInventorySurfaceOpen)
        {
            return false;
        }

        if (surface.IsReadOnly)
        {
            _inventoryFeedbackText = "Battle inspection only. Equipment changes are locked.";
            InvalidateInventorySurfaceCache();
            return false;
        }

        if (_runtimeEconomyState == null)
        {
            _inventoryFeedbackText = "Party inventory runtime is unavailable.";
            InvalidateInventorySurfaceCache();
            return false;
        }

        bool equipped = _runtimeEconomyState.TryManualEquipPartyInventoryItem(
            surface.PartyId,
            surface.SelectedMemberId,
            surface.SelectedSlotKey,
            surface.SelectedItemInstanceId,
            out string feedbackText);
        _inventoryFeedbackText = string.IsNullOrEmpty(feedbackText) ? (equipped ? "Equipment updated." : "Equip failed.") : feedbackText;
        InvalidateInventorySurfaceCache();
        return equipped;
    }

    public bool TryUnequipSelectedInventorySlot()
    {
        PrototypeRpgInventorySurfaceData surface = BuildInventorySurfaceData();
        if (!_isInventorySurfaceOpen)
        {
            return false;
        }

        if (surface.IsReadOnly)
        {
            _inventoryFeedbackText = "Battle inspection only. Equipment changes are locked.";
            InvalidateInventorySurfaceCache();
            return false;
        }

        if (_runtimeEconomyState == null)
        {
            _inventoryFeedbackText = "Party inventory runtime is unavailable.";
            InvalidateInventorySurfaceCache();
            return false;
        }

        bool unequipped = _runtimeEconomyState.TryManualUnequipPartyMemberSlot(
            surface.PartyId,
            surface.SelectedMemberId,
            surface.SelectedSlotKey,
            out string feedbackText);
        _inventoryFeedbackText = string.IsNullOrEmpty(feedbackText) ? (unequipped ? "Equipment updated." : "Unequip failed.") : feedbackText;
        InvalidateInventorySurfaceCache();
        return unequipped;
    }

    private PrototypeRpgInventorySurfaceData BuildInventorySurfaceData()
    {
        if (!_isInventorySurfaceOpen || _runtimeEconomyState == null)
        {
            return new PrototypeRpgInventorySurfaceData();
        }

        string partyId = ResolveCurrentInventoryPartyId();
        if (string.IsNullOrEmpty(partyId))
        {
            CloseInventorySurface();
            return new PrototypeRpgInventorySurfaceData();
        }

        bool isReadOnly = IsBattleViewActive;
        int revision = _runtimeEconomyState.GetPartyInventoryRevision(partyId);
        string runSpoilsKey = IsDungeonRunActive
            ? BuildLootAmountText(_carriedLootAmount) + "|" + _runResultState
            : string.Empty;

        // The presentation shell reads this surface every OnGUI frame, so only rebuild it
        // when the party revision or the player's UI selection changes.
        if (_cachedInventorySurfaceData != null &&
            _cachedInventoryPartyId == partyId &&
            _cachedInventoryMemberId == _inventorySelectedMemberId &&
            _cachedInventorySlotKey == _inventorySelectedSlotKey &&
            _cachedInventoryItemInstanceId == _inventorySelectedItemInstanceId &&
            _cachedInventoryFeedbackText == _inventoryFeedbackText &&
            _cachedInventoryRunSpoilsKey == runSpoilsKey &&
            _cachedInventoryRevision == revision &&
            _cachedInventoryReadOnly == isReadOnly)
        {
            return _cachedInventorySurfaceData;
        }

        PrototypeRpgInventorySurfaceData surface = _runtimeEconomyState.BuildPartyInventorySurface(
            partyId,
            _inventorySelectedMemberId,
            _inventorySelectedSlotKey,
            _inventorySelectedItemInstanceId,
            isReadOnly,
            _inventoryFeedbackText);
        ApplyDungeonRunPendingSpoilsSurface(surface);
        _inventorySelectedMemberId = surface.SelectedMemberId;
        _inventorySelectedSlotKey = string.IsNullOrEmpty(surface.SelectedSlotKey) ? PrototypeRpgEquipmentSlotKeys.Head : surface.SelectedSlotKey;
        _inventorySelectedItemInstanceId = surface.SelectedItemInstanceId;
        _cachedInventorySurfaceData = surface;
        _cachedInventoryPartyId = partyId;
        _cachedInventoryMemberId = _inventorySelectedMemberId;
        _cachedInventorySlotKey = _inventorySelectedSlotKey;
        _cachedInventoryItemInstanceId = _inventorySelectedItemInstanceId;
        _cachedInventoryFeedbackText = _inventoryFeedbackText;
        _cachedInventoryRunSpoilsKey = runSpoilsKey;
        _cachedInventoryRevision = revision;
        _cachedInventoryReadOnly = isReadOnly;
        return surface;
    }

    private void InvalidateInventorySurfaceCache()
    {
        _cachedInventorySurfaceData = null;
        _cachedInventoryPartyId = string.Empty;
        _cachedInventoryMemberId = string.Empty;
        _cachedInventorySlotKey = string.Empty;
        _cachedInventoryItemInstanceId = string.Empty;
        _cachedInventoryFeedbackText = string.Empty;
        _cachedInventoryRunSpoilsKey = string.Empty;
        _cachedInventoryRevision = -1;
        _cachedInventoryReadOnly = false;
    }

    private void ApplyDungeonRunPendingSpoilsSurface(PrototypeRpgInventorySurfaceData surface)
    {
        if (surface == null || !IsDungeonRunActive)
        {
            return;
        }

        string runSpoilsText = _carriedLootAmount > 0
            ? "Run Spoils " + BuildLootAmountText(_carriedLootAmount) + " pending extraction"
            : "No pending run spoils.";
        string latestRewardText = string.IsNullOrEmpty(surface.LatestRewardSummaryText) || surface.LatestRewardSummaryText == "None"
            ? runSpoilsText
            : surface.LatestRewardSummaryText + " | " + runSpoilsText;
        surface.LatestRewardSummaryText = latestRewardText;

        string footerText = string.IsNullOrEmpty(surface.FooterSummaryText) || surface.FooterSummaryText == "None"
            ? runSpoilsText
            : surface.FooterSummaryText + " | " + runSpoilsText;
        surface.FooterSummaryText = footerText;

        if (string.IsNullOrEmpty(surface.FeedbackText) || surface.FeedbackText == "None")
        {
            surface.FeedbackText = runSpoilsText;
        }
    }

    private string ResolveCurrentInventoryPartyId()
    {
        if (_runtimeEconomyState == null)
        {
            return string.Empty;
        }

        if (_activeDungeonParty != null && !string.IsNullOrEmpty(_activeDungeonParty.PartyId))
        {
            return _activeDungeonParty.PartyId;
        }

        string cityId = ResolveDispatchBriefingCityId();
        if (string.IsNullOrEmpty(cityId))
        {
            cityId = _currentHomeCityId;
        }

        if (string.IsNullOrEmpty(cityId))
        {
            return string.Empty;
        }

        string idlePartyId = _runtimeEconomyState.GetIdlePartyIdInCity(cityId);
        if (!string.IsNullOrEmpty(idlePartyId))
        {
            return idlePartyId;
        }

        return _runtimeEconomyState.GetActivePartyIdForCity(cityId);
    }

    private static int FindSelectedInventoryMemberIndex(PrototypeRpgInventorySurfaceData surface)
    {
        if (surface == null || surface.Members == null || surface.Members.Length <= 0)
        {
            return 0;
        }

        for (int i = 0; i < surface.Members.Length; i++)
        {
            if (surface.Members[i] != null && surface.Members[i].IsSelected)
            {
                return i;
            }
        }

        return 0;
    }

    private static int FindSelectedInventoryItemIndex(PrototypeRpgInventorySurfaceData surface)
    {
        if (surface == null || surface.InventoryItems == null || surface.InventoryItems.Length <= 0)
        {
            return 0;
        }

        for (int i = 0; i < surface.InventoryItems.Length; i++)
        {
            if (surface.InventoryItems[i] != null && surface.InventoryItems[i].IsSelected)
            {
                return i;
            }
        }

        return 0;
    }

    private static int WrapInventoryIndex(int value, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        int wrapped = value % count;
        return wrapped < 0 ? wrapped + count : wrapped;
    }
}
