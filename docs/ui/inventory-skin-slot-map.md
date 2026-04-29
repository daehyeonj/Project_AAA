# Inventory Skin Slot Map

These mappings use the current `InventoryUiSkinDefinition` API.

Batch 77.9 serializes only the approved preview assignments listed in the notes column.
The folders below are candidate pools for future human assignment, not permission to fill every slot.

| Slot | Candidate folder | Current curated examples | Notes |
| --- | --- | --- | --- |
| `InventoryBackground` | `Assets/_Game/Content/UI/Sprites/Inventory/Panels` | `UI_TravelBook_BookPageLeft01a`, `UI_TravelBook_BookPageRight01a` | human assignment required |
| `HeaderPanel` | `Assets/_Game/Content/UI/Sprites/Inventory/Panels` | `UI_TravelBook_Frame01a`, `UI_TravelBook_BookPageLeft01a` | human assignment required |
| `FooterPanel` | `Assets/_Game/Content/UI/Sprites/Inventory/Panels` | `UI_TravelBook_Frame01a`, `UI_TravelBook_BookPageRight01a` | human assignment required |
| `ItemDetailPanel` | `Assets/_Game/Content/UI/Sprites/Inventory/Panels` | `UI_TravelBook_Frame01a`, `UI_TravelBook_BookPageRight01a` | human assignment required |
| `MemberRow` | `Assets/_Game/Content/UI/Sprites/Inventory/Rows` | `UI_TravelBook_ButtonValue01a`, `UI_TravelBook_FrameSelect01a` | normal row state in current API |
| `MemberRowSelected` | `Assets/_Game/Content/UI/Sprites/Inventory/Rows` | `UI_TravelBook_ButtonValue01b`, `UI_TravelBook_FrameSelect01a` | selected row state in current API |
| `ItemRow` | `Assets/_Game/Content/UI/Sprites/Inventory/Rows` | `UI_TravelBook_ButtonValue01a`, `UI_TravelBook_FrameSelect01a` | normal row state in current API |
| `ItemRowSelected` | `Assets/_Game/Content/UI/Sprites/Inventory/Rows` | `UI_TravelBook_ButtonValue01b`, `UI_TravelBook_FrameSelect01a` | selected row state in current API |
| `EquipmentSlot` | `Assets/_Game/Content/UI/Sprites/Inventory/Slots` | `UI_TravelBook_Slot01a`, `UI_TravelBook_Slot01b` | generic slot fallback |
| `EquipmentSlotEmpty` | `Assets/_Game/Content/UI/Sprites/Inventory/Slots` | `UI_TravelBook_Slot01a` | Batch 77.9 assigned `UI_TravelBook_Slot01a` |
| `EquipmentSlotEquipped` | `Assets/_Game/Content/UI/Sprites/Inventory/Slots` | `UI_TravelBook_Slot01b`, `UI_TravelBook_Slot01c` | Batch 77.9 assigned `UI_TravelBook_Slot01b` |
| `EquipmentSlotSelected` | `Assets/_Game/Content/UI/Sprites/Inventory/Slots` | `UI_TravelBook_Slot01c`, `UI_TravelBook_Slot01b` | human assignment required |
| `RunSpoilsBadge` | `Assets/_Game/Content/UI/Sprites/Battle/Popups` | `UI_TravelBook_Popup01a` | Batch 77.9 assigned `UI_TravelBook_Popup01a`; this is a preview badge test, not a final icon decision |
| `PendingExtractionBadge` | `Assets/_Game/Content/UI/Sprites/Inventory/Badges` | `UI_TravelBook_IconStar01a`, `UI_TravelBook_IconCoin01a` | human assignment required |

## Notes

- final skin inputs should use `Sprite` references; `Texture` references remain preview-only fallback
- `UI_TravelBook_Slot01a` and `UI_TravelBook_Slot01b` are acceptable preview candidates for empty/equipped slot testing
- unlisted Inventory slots are intentionally empty in the Batch 77.9 default preview skin
- `Inventory/Icons` is staged for future overlay glyph work, but the current skin asset does not expose a dedicated icon slot.
- Curated copies were normalized to `Sprite (2D and UI)` import mode in Batch 77.6.
- Raven Fantasy Icons remains blocked until license confirmation.
