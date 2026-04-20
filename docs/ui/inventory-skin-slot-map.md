# Inventory Skin Slot Map

These mappings use the current `InventoryUiSkinDefinition` API.

Every slot still requires human assignment.
The folders below are candidate pools, not final design decisions.

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
| `EquipmentSlotEmpty` | `Assets/_Game/Content/UI/Sprites/Inventory/Slots` | `UI_TravelBook_Slot01a` | human assignment required |
| `EquipmentSlotEquipped` | `Assets/_Game/Content/UI/Sprites/Inventory/Slots` | `UI_TravelBook_Slot01b`, `UI_TravelBook_Slot01c` | human assignment required |
| `EquipmentSlotSelected` | `Assets/_Game/Content/UI/Sprites/Inventory/Slots` | `UI_TravelBook_Slot01c`, `UI_TravelBook_Slot01b` | human assignment required |
| `RunSpoilsBadge` | `Assets/_Game/Content/UI/Sprites/Inventory/Badges` | `UI_TravelBook_IconCoin01a`, `UI_TravelBook_IconStar01a` | human assignment required |
| `PendingExtractionBadge` | `Assets/_Game/Content/UI/Sprites/Inventory/Badges` | `UI_TravelBook_IconStar01a`, `UI_TravelBook_IconCoin01a` | human assignment required |

## Notes

- `Inventory/Icons` is staged for future overlay glyph work, but the current skin asset does not expose a dedicated icon slot.
- Curated copies were normalized to `Sprite (2D and UI)` import mode in Batch 77.6.
