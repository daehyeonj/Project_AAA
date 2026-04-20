# Curated UI Sprite Staging

This folder is the project-owned staging area for battle/inventory UI sprite candidates after Batch 77.5.

## What Lives Here

- small, intentional curated copies of UI candidate textures
- battle/inventory/shared candidate folders
- source-map notes that explain where each curated copy came from

## What Does Not Live Here

- raw third-party art packs
- runtime `Resources` content
- auto-assigned final skin decisions

## Source Of Truth

- raw art landing zone remains `Assets/Sprite`
- curated copies here are for manual designer assignment
- final slot choices still require a human pass in Unity Editor

## Important GUID Note

Curated copies in this folder were copied **without** the raw source `.meta` files on purpose.

That keeps the raw pack GUIDs intact and lets Unity generate new GUIDs for curated project-owned copies.

Do not manually duplicate raw `.meta` files into this folder.
