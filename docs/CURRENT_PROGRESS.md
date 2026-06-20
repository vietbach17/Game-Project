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
* **O Thắm's Shop Visuals & Scene Setup**:
  * Integrated low-poly 3D models for O Thắm's shop house (`Meshy_AI_Low_poly_stylized_3D__0620062116_texture.fbx`) and her market stall (`Meshy_AI_Low_poly_stylized_3D__0620065520_texture.fbx`).
  * Removed the old placeholder asset `asian_house.glb`.
  * Created a custom Editor script `SetupOThamShop.cs` (`Sown In Stone -> Setup O Tham Shop` menu item) to automate the entire scene configuration.
  * The script automatically generates materials, applies textures, instantiates the prefabs under a unified `OTham_Shop` GameObject, and corrects coordinate offsets.
  * Corrected pivot offsets horizontally and vertically to place the bottoms of both the house and the stall flat on the ground (Y = 0).
  * Auto-scaled the shop house to a height of `4.5` meters and the market stall to `1.2` meters.
  * Positioned the shop in the empty field to the left of Thành's house (`X: 4.5`, `Z: -10.0`) and aligned NPC O Thắm to stand right behind the stall facing the player (`X: 4.5`, `Y: 0.5`, `Z: -11.2`) with her height scaled to `1.7` meters.
  * Automatically calculated and aligned a BoxCollider to cover the shop structure to prevent player clipping.
* **Bác Năm's House & Daybed Setup**:
  * Integrated custom low-poly 3D models for Bác Năm's traditional house (`BacNam_House_Model.fbx`) and his bamboo daybed / chõng tre (`BacNam_Daybed_Model.fbx`).
  * Created a custom Editor script `SetupBacNamHouse.cs` (`Sown In Stone -> Setup Bac Nam House` menu item) to automate scene setup.
  * The script generates materials and assigns textures (`BacNam_House_Texture.png` and `BacNam_Daybed_Texture.png`) automatically.
  * Corrects pivot offsets to place the bottom of both models flat on the ground (`Y = 0`).
  * Auto-scales the house to a height of `4.5` meters and the bamboo daybed to `1.2` meters.
  * Positions the house at world `(8.0, 0.0, 12.0)` rotated 180 degrees to face south, and the daybed next to Bác Năm at `(8.5, 0.0, 7.5)`.
  * Repositions NPC Bác Năm next to the daybed at `(7.0, 0.5, 7.5)` facing the player, auto-scaling his visual to `1.7` meters.
  * Configures a physical BoxCollider covering the house structure to prevent player clipping and extends Bác Năm's interaction trigger to cover the daybed area.
* **Player's House (Thành's House) Setup**:
  * Integrated custom low-poly 3D model for Thành's house (`Meshy_AI_Stylized_low_poly_3D__0620084846_texture.fbx`).
  * Created a custom Editor script `SetupThanhHouse.cs` (`Sown In Stone -> Setup Thanh House` menu item) to automate scene setup.
  * The script generates materials and assigns textures (`Meshy_AI_Stylized_low_poly_3D__0620084846_texture.png`) automatically.
  * Corrects pivot offsets to place the bottom of the house model flat on the ground (`Y = 0`).
  * Auto-scales the house to a height of `4.5` meters.
  * Positions the house at world `(10.66, 0.0, -10.0)` rotated 180 degrees to face the road/player (south).
  * Configures a physical BoxCollider covering the house structure to prevent player clipping.
* **Ancestral Altar Setup**:
  * Integrated custom low-poly stone altar model (`Meshy_AI_tôi_muốn_làm_mộ_0613091059_texture.fbx`).
  * Created a custom Editor script `SetupAltar.cs` (`Sown In Stone -> Setup Altar` menu item) to automate scene setup.
  * The script generates materials and assigns textures (`Meshy_AI_tôi_muốn_làm_mộ_0613091059_texture.png`) automatically.
  * Corrects pivot offsets to place the bottom of the altar model flat on the ground (`Y = 0`).
  * Auto-scales the altar to a height of `1.8` meters.
  * Positions the altar at world `(7.5, 0.0, -13.0)` (bottom corner of Thành's house).
  * Configures a trigger BoxCollider covering the altar interaction zone for easy incense offering.
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

* **Current Playable Loop**: Player can walk/run around the map (customizable keys), inspect SoilCells, till rocks, plant potato seeds (with seeds consumed from storage), water the soil, use the F1 debug command to mature crops quickly for testing, harvest mature crops, receive visual HUD toast feedback, trade with NPC O Thắm at her newly decorated shop (consisting of the house and market stall), adjust setting options & binds, hear ambient/SFX audio, and pass out to trigger Bác Năm's rescue loop.
* **Current Blockers**: None.
* **Recommended Next Task**: Investigate and fix remaining invisible colliders/blockers near environment landmarks.
