using UnityEngine.InputSystem;

public sealed partial class BootEntry
{
    public bool IsInventorySurfaceOpen => _worldView != null && _worldView.IsInventorySurfaceOpen;
    public bool IsBattleResultPopoverVisible => _worldView != null && _worldView.IsBattleResultPopoverVisible;

    public PrototypeRpgInventorySurfaceData GetInventorySurfaceData()
    {
        return _worldView != null ? _worldView.CurrentInventorySurfaceData : new PrototypeRpgInventorySurfaceData();
    }

    public bool TryToggleInventorySurface()
    {
        return _worldView != null && _worldView.TryToggleInventorySurface();
    }

    public void CloseInventorySurface()
    {
        if (_worldView != null)
        {
            _worldView.CloseInventorySurface();
        }
    }

    public bool TryCycleInventoryMember(int direction)
    {
        return _worldView != null && _worldView.TryCycleInventoryMember(direction);
    }

    public bool TrySelectInventoryMember(string memberId)
    {
        return _worldView != null && _worldView.TrySelectInventoryMember(memberId);
    }

    public bool TrySelectInventorySlot(string slotKey)
    {
        return _worldView != null && _worldView.TrySelectInventorySlot(slotKey);
    }

    public bool TrySelectInventoryItem(string itemInstanceId)
    {
        return _worldView != null && _worldView.TrySelectInventoryItem(itemInstanceId);
    }

    public bool TryConfirmInventoryEquip()
    {
        return _worldView != null && _worldView.TryConfirmInventoryEquip();
    }

    public bool TryUnequipSelectedInventorySlot()
    {
        return _worldView != null && _worldView.TryUnequipSelectedInventorySlot();
    }

    private bool TryHandleInventorySurfaceInput(Keyboard keyboard, bool blockKeyboardShortcuts)
    {
        if (keyboard == null)
        {
            return false;
        }

        bool battleResultPopoverVisible = IsBattleResultPopoverVisible;
        bool canToggle = !blockKeyboardShortcuts &&
                         !battleResultPopoverVisible &&
                         (CurrentState == GameStateId.WorldSim || CurrentState == GameStateId.DungeonRun);
        bool inventoryOpen = IsInventorySurfaceOpen;
        if (canToggle && keyboard.iKey.wasPressedThisFrame)
        {
            TryToggleInventorySurface();
            return true;
        }

        if (!inventoryOpen)
        {
            return false;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            CloseInventorySurface();
            return true;
        }

        if (keyboard.qKey.wasPressedThisFrame)
        {
            TryCycleInventoryMember(-1);
            return true;
        }

        if (keyboard.eKey.wasPressedThisFrame)
        {
            TryCycleInventoryMember(1);
            return true;
        }

        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            _worldView.TryMoveInventoryItemSelection(-1);
            return true;
        }

        if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            _worldView.TryMoveInventoryItemSelection(1);
            return true;
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            TryConfirmInventoryEquip();
            return true;
        }

        if (keyboard.uKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)
        {
            TryUnequipSelectedInventorySlot();
            return true;
        }

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            _worldView.TrySelectInventorySlotByIndex(0);
            return true;
        }

        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            _worldView.TrySelectInventorySlotByIndex(1);
            return true;
        }

        if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
        {
            _worldView.TrySelectInventorySlotByIndex(2);
            return true;
        }

        if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
        {
            _worldView.TrySelectInventorySlotByIndex(3);
            return true;
        }

        if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame)
        {
            _worldView.TrySelectInventorySlotByIndex(4);
            return true;
        }

        if (keyboard.digit6Key.wasPressedThisFrame || keyboard.numpad6Key.wasPressedThisFrame)
        {
            _worldView.TrySelectInventorySlotByIndex(5);
            return true;
        }

        if (keyboard.digit7Key.wasPressedThisFrame || keyboard.numpad7Key.wasPressedThisFrame)
        {
            _worldView.TrySelectInventorySlotByIndex(6);
            return true;
        }

        return false;
    }
}
