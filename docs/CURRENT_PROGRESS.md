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
  * Fixed a critical `StackOverflowException` (mutual recursion loop) in `SoilCell.cs` when executing actions (clearing rocks, watering, fertilizing, planting) due to child cells delegating back to parent cell.
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

* **O Thắm's Seed Shop & Economy Loop**: Fully implemented shop interface and trade loop, allowing players to spend coins to buy seeds and incense, and sell harvested fresh/preserved crop products with instant inventory updates and HUD feedback.

* **Environment Polish & Collision Alignment**:
  * Visual meshes for landmarks (Village Well, Thanh's House, Bac Nam's House) have been scaled up to match their physical meters size.
  * Decorative rock instances (`Rock_1`, `Rock_2`, `Rock_5`) have been correctly rescaled to natural sizes in the scene.
  * All modular `FenceSegment` objects have been rotated to stand upright (`270` degrees on X) and form a proper enclosure around the farming plot.
  * All BoxColliders have been aligned in the scene to match their visible mesh dimensions exactly, eliminating invisible wall blocking issues around wells, houses, rocks, and fences.
  * Verified player movement in Play Mode to ensure smooth navigation.

* **3D Crop Growth Visuals**:
  * Upgraded the farming system to support optional 3D prefab models for growth stages (`GrowthStagePrefabs` field added to `CropData`).
  * Updated `CropInstance` to dynamically instantiate and display these 3D prefabs during growth stages, falling back to procedural primitives only if prefabs are absent.
  * Replaced the default UI sprite placeholders on `Crop_KhoaiLang` with actual stylized plant models (`Plant_1`, `Plant_3`, `Plant_5` from the Ultimate Nature Pack) representing seedling, growing, and mature stages.

## In Progress

* Polish characters and transition from prototype layout to low-poly stylized models.

## Blocked

* None.

## Known Issues

* Outer boundary elements (`BoundaryElement`) are currently microscopic under `globalScale = 1.0` import settings and act as invisible collision barriers. They will be addressed in a future dedicated boundary pass.
* Real crop art assets (sprites/models) for other crops are not implemented yet.

---

# PROJECT STATUS SNAPSHOT

* **Current Playable Loop**: Player can walk/run around the map, inspect SoilCells, till rocks, plant potato seeds (with seeds consumed from storage), water the soil, use the F1 debug command to mature crops quickly for testing, harvest mature crops, receive visual HUD toast feedback, trade with NPC O Thắm to buy seeds/incense or sell harvested products, and view accumulated items/coins in the Inventory (`Tab`/`I` keys).
* **Current Blockers**: None.
* **Recommended Next Task**: Investigate and fix remaining invisible colliders/blockers near environment landmarks.
