# UI Asset Attribution

## Purpose

This note records the currently audited attribution and license status for curated UI preview assets used by Project AAA.

It is not a final legal approval.
Final release/legal review is still required.

## Curated Copies Location

- `Assets/_Game/Content/UI/Sprites`
- `Assets/_Game/Content/UI/Sprites/_SourceMap`

These are project-owned curated copies for preview and manual assignment workflow.
The raw source packs remain under `Assets/Sprite`.

## TravelBook Lite

- source pack: `Assets/Sprite/Complete_UI_Book_Styles_Pack_Free_v1.0/01_TravelBookLite`
- audited license evidence: `Assets/Sprite/Complete_UI_Book_Styles_Pack_Free_v1.0/License.txt`
- current project status: `allowed for preview/workflow/manual-test runtime use based on visible license text`

Visible license summary from the audited text:

- personal and commercial project use is allowed
- sharing/copying/adapting/building upon the material is allowed
- appropriate credit or a link to the product page is required
- changes should be indicated when applicable
- reselling or publishing the same material, or adapted versions of it, as a standalone asset pack is not allowed
- NFT use is not allowed

Working project rule:

- keep attribution notes with any curated UI usage
- do not treat the raw or curated files as freely redistributable source art
- if the asset moves beyond internal preview/workflow use, perform another legal/release review

Reference links from the audited license text:

- licensor/store page: `https://crusenho.itch.io`
- product page: `https://crusenho.itch.io/complete-ui-book-styles-pack`

## Raven Fantasy Icons

- source pack: `Assets/Sprite/Free - Raven Fantasy Icons`
- audited file found: `Special Note to the Dev.txt`
- license file found in the audited folder: `none`
- current project status: `UNKNOWN license / not approved for assignment`

Working project rule:

- do not assign Raven assets to battle or inventory skin slots
- do not claim commercial-safe or redistribution-safe use
- require explicit license confirmation before any assignment discussion resumes

## Current Usage Boundary

- Codex made no random sprite assignments
- current default skin assets contain only the Batch 77.9 user-provided preview mapping
- Batch 77.10 bridges those same skin assets into `SampleScene` manual-test runtime through `RuntimeUiSkinBridge`
- Battle preview mapping: `PanelBackground = UI_TravelBook_BookCover01a`, `CommandButtonNormal = UI_TravelBook_Button01a_1`, `PopupBackground = UI_TravelBook_Popup01a`, `TopStripBackground = empty`
- Inventory preview mapping: `EquipmentSlotEmpty = UI_TravelBook_Slot01a`, `EquipmentSlotEquipped = UI_TravelBook_Slot01b`, `RunSpoilsBadge = UI_TravelBook_Popup01a`
- those assignments are still preview/test state, not final art approval
- final skin slots use `Sprite` references; `Texture` references remain preview-only fallback
- preview and runtime scenes should still receive a human screenshot pass before final art approval

## Related Docs

- `docs/ui/ui-sprite-curation.md`
- `docs/ui/manual-skin-assignment-checklist.md`
- `docs/ui/ui-skin-preview-mapping-qa.md`
- `Assets/_Game/Content/UI/Sprites/_SourceMap/ui-sprite-candidate-map.md`
