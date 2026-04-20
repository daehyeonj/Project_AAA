# Battle Skin Slot Map

These mappings use the current `BattleUiSkinDefinition` API.

Every slot still requires human assignment.
The folders below are candidate pools, not final design decisions.

| Slot | Candidate folder | Current curated examples | Notes |
| --- | --- | --- | --- |
| `PanelBackground` | `Assets/_Game/Content/UI/Sprites/Battle/Panels` | `UI_TravelBook_BookCover01a`, `UI_TravelBook_Frame01a`, `UI_TravelBook_Popup01a` | human assignment required |
| `PanelHeader` | `Assets/_Game/Content/UI/Sprites/Battle/Panels` | `UI_TravelBook_FrameSelect01a`, `UI_TravelBook_Frame01a` | human assignment required |
| `PanelAccent` | `Assets/_Game/Content/UI/Sprites/Shared/Dividers` | `UI_TravelBook_Line01a` | human assignment required |
| `CommandButtonNormal` | `Assets/_Game/Content/UI/Sprites/Battle/Buttons` | `UI_TravelBook_ButtonValue01a`, `UI_TravelBook_Select01a` | human assignment required |
| `CommandButtonHover` | `Assets/_Game/Content/UI/Sprites/Battle/Buttons` | `UI_TravelBook_ButtonValue01b`, `UI_TravelBook_Select01a` | human assignment required |
| `CommandButtonSelected` | `Assets/_Game/Content/UI/Sprites/Battle/Buttons` | `UI_TravelBook_ButtonValue01b`, `UI_TravelBook_Select01a` | human assignment required |
| `CommandButtonDisabled` | `Assets/_Game/Content/UI/Sprites/Battle/Buttons` | `UI_TravelBook_ButtonValue01a` | may need tinting rather than a unique sprite |
| `CurrentUnitCard` | `Assets/_Game/Content/UI/Sprites/Battle/Panels` | `UI_TravelBook_Frame01a`, `UI_TravelBook_FrameSelect01a` | human assignment required |
| `TargetStatusCard` | `Assets/_Game/Content/UI/Sprites/Battle/Panels` | `UI_TravelBook_Frame01a`, `UI_TravelBook_Popup01a` | human assignment required |
| `TimelineChip` | `Assets/_Game/Content/UI/Sprites/Battle/Timeline` | `UI_TravelBook_Point01a`, `UI_TravelBook_Point02a` | human assignment required |
| `TimelineChipCurrent` | `Assets/_Game/Content/UI/Sprites/Battle/Timeline` | `UI_TravelBook_Marker01a`, `UI_TravelBook_Point02a` | human assignment required |
| `TimelineChipEnemy` | `Assets/_Game/Content/UI/Sprites/Battle/Timeline` | `UI_TravelBook_Marker01a`, `UI_TravelBook_Point01a` | human assignment required |
| `HpBarBackground` | `Assets/_Game/Content/UI/Sprites/Battle/Bars` | `UI_TravelBook_Bar01a` | human assignment required |
| `HpBarFill` | `Assets/_Game/Content/UI/Sprites/Battle/Bars` | `UI_TravelBook_Fill01a`, `UI_TravelBook_Fill01b` | human assignment required |
| `PopupBackground` | `Assets/_Game/Content/UI/Sprites/Battle/Popups` | `UI_TravelBook_Popup01a`, `UI_TravelBook_Alert01a` | human assignment required |
| `DropPopupBadge` | `Assets/_Game/Content/UI/Sprites/Battle/Badges` | `UI_TravelBook_IconCoin01a`, `UI_TravelBook_IconStar01a` | human assignment required |
| `BurstWindowBadge` | `Assets/_Game/Content/UI/Sprites/Battle/Badges` | `UI_TravelBook_IconStar01a`, `UI_TravelBook_IconHeart01a` | human assignment required |

## Notes

- `Battle/Icons` is staged for future battle-specific glyph use, but the current skin asset does not expose a dedicated icon slot.
- If a chosen candidate still needs `Sprite (2D and UI)` import mode, convert the curated copy only.
