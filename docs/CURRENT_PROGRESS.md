# Current Progress

## Completed

* **Camera & HUD**: Third-person camera follow, Grid Inventory UI, Community affection progress panel, and Weather details screen.
* **Character System**: 
  * Player uses `Player_Base.fbx` as the animation avatar source.
  * Animator Controller is assigned and functional.
  * Walking and running animations play correctly.
  * Left Shift triggers running smoothly.
  * Player model is properly attached as a child under the `Player` GameObject and follows movement correctly.
* **Farming System**:
  * SoilCells can be cultivated/tilled (cleared of rock density).
  * SoilCells can be watered.
  * SoilCells can be planted with seeds.
  * Crops grow through growth stages (seedling, growing, ready, withered/rotted).
  * Crops can be harvested successfully.
  * Harvest rewards (fresh crops) are calculated and added to the inventory correctly.
* **Crop Visuals**:
  * `CropInstance` creates visible visuals at runtime.
  * Procedural 3D primitive visuals (stems and leaves) fallback setup exists for when `GrowthStageSprites` are invalid or default UI textures.
* **Notifications**:
  * Harvest reward toast notification ("Thu hoạch thành công: +X [Item]") appears directly on the normal gameplay HUD.
  * The developer F1 debug menu is no longer required to see harvest rewards.
* **Environment**:
  * RoadSegment MeshColliders have been disabled to make roads walkable and resolve invisible movement blockers.

## In Progress

* Polish characters and transition from prototype layout to low-poly stylized models.

## Blocked

* None.

## Known Issues

* Some invisible colliders/blockers still exist near parts of the environment.
* Houses and village environment still need polishing.
* O Thắm shop economy loop is not implemented.
* Real crop art assets (sprites/models) are not implemented yet; currently `Crop_KhoaiLang` relies on placeholder UI sprites and triggers the 3D visual fallback.

---

# PROJECT STATUS SNAPSHOT

* **Current Playable Loop**: Player can walk/run around the map, inspect SoilCells, till rocks, plant potato seeds (with seeds consumed from storage), water the soil, use the F1 debug command to mature crops quickly for testing, harvest mature crops, receive visual HUD toast feedback, and view accumulated items in the Inventory (`Tab`/`I` keys).
* **Current Blockers**: None.
* **Recommended Next Task**: Investigate and fix remaining invisible colliders/blockers near environment landmarks, then implement O Thắm's seed shop economy loop.
