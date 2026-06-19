# Current Progress

## Completed

* **Audio & Sound System (AudioManager)**:
  * Centralized `AudioManager.cs` loaded dynamically as a persistent singleton.
  * Dynamically loads background tracks, ambient soundscapes, and action SFX from `Assets/Resources/Audio/` without hardcoded path dependencies.
  * Supports automatic ambient fading based on weather phases (rural day/night music, dry Southwest Gió Lào wind loop, storm and rain ambience).
  * Plays contextual SFX for actions: tilling/digging (`sfx_dig`), watering (`sfx_water`), planting (`sfx_plant`), harvesting (`sfx_harvest`), altar offerings (`sfx_altar`), coin transactions (`sfx_coin`), and button clicks (`sfx_click`).
* **Settings Menu & Custom Keybindings**:
  * Unified Settings UI accessible via a gear button in the bottom-right corner of the HUD or the **`Esc`** key.
  * Dynamic GUI settings available on the main menu via OnGUI layout.
  * **Key Customization Tab**: Supports fully rebindable controls for Move Up, Move Down, Move Left, Move Right, Interact, and Run. Mapped to `PlayerPrefs` for automatic saving and loading.
  * **Survival Guide Tab**: Displays guides explaining farming methods, weather hazards, altar offerings, and rescue mechanics.
  * **NPC Profiles Tab**: Shows biographies and live community affection stats for village NPCs (Bác Năm, O Thắm).
  * Settings menu automatically pauses the game state (`Time.timeScale = 0`) when opened.
* **Dual Input System Compatibility**:
  * Seamlessly handles both Unity's Legacy Input Manager and New Input System backends at runtime.
  * Implemented safe key query abstractions (e.g. converting `UnityEngine.InputSystem.Key` to `KeyCode` for rebinding captures) to prevent `InvalidOperationException` crashes when the New Input backend is active.
* **Faint & Rescue Gameplay Loop**:
  * Integrated survival stress-collapse mechanics. If the player's health drops to 0 or temperature stress reaches 100% (heatstroke / freezing), a faint sequence plays (faint animation + `sfx_faint`).
  * The player is rescued by Bác Năm, waking up at Bác Năm's house the next morning with restored stats and a tax/toll deducted from their coins and inventory.
* **Camera & HUD**: Third-person camera follow, Grid Inventory UI, Community affection progress panel, and Weather details screen.
* **Character System**: 
  * Player uses `Player_Base.fbx` as the animation avatar source.
  * Animator Controller is assigned and functional.
  * Walking and running animations play correctly.
  * Left Shift (or customized run key) triggers running smoothly.
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
  * Mesh scaling for Village Well, Thanh's House, and Bác Năm's House matching physical sizes.
  * Rock and modular FenceSegment visual scaling, positioning, and upright rotations fixed.
  * Aligned BoxColliders eliminating invisible collision walls.
* **3D Crop Growth Visuals**:
  * Upgraded crop growth stages supporting 3D models (`Plant_1`, `Plant_3`, `Plant_5` from the Ultimate Nature Pack) on `CropData` dynamically spawned at runtime.

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
