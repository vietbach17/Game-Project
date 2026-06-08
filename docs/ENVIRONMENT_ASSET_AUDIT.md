# Environment Asset Audit

This audit evaluates the current environment assets in the project, catalogs the newly imported stylized 3D prefabs, and provides a replacement plan to transition the game from primitive geometry to detailed Stylized Low Poly models.

---

## 1. Existing Ground Textures & Characters

The following base assets exist in the project:
*   **Player Character Model:** `indonesian_farmer_pak_tani.glb` - A stylized Southeast Asian farmer model. Matches a **Stylized Low Poly** art style.
*   **Ground Textures:** (Under `Assets/Textures/`)
    *   `grass_ground.png` (Grass terrain texture)
    *   `dry_soil.png` (Dry tilled soil texture)
    *   `wet_soil.png` (Wet tilled soil texture)
    *   `silt_soil.png` (Alluvium/silt texture)
    *   `water_waves.png` (Floodwater overlay texture)

---

## 2. Imported Prefab Catalog

The newly imported assets under `Assets/Prefabs/` have been audited and cataloged below:

### A. Village Architecture (`Assets/Prefabs/`)
*   **`HoiAnHouse_M2.fbx` (1.3 MB):** A highly detailed model representing the traditional architectural style of Hoi An in Central Vietnam. Features classic sloped tiling and rustic wooden structures.
*   **`asian_house.glb` (690 KB):** A stylized Oriental/Asian traditional dwelling, ideal for a merchant, residential home, or village shop.
*   **`FBX-Village/FBX/`:** A set of 10 stylized low-poly medieval-style town buildings:
    *   `House_1.fbx` to `House_4.fbx` (Various sizes of village homes)
    *   `Inn.fbx`, `Blacksmith.fbx`, `Bell_Tower.fbx`, `Mill.fbx`, `Sawmill.fbx`, `Stable.fbx`

### B. Vegetation & Farming Props (`Assets/Prefabs/`)
*   **`Banana Tree/BananaTree.obj`:** Dedicated banana plant model with wide, drooping leaves, complete with `BananaTree_BaseColor.png` texture.
*   **`Bamboo/PUSHILIN_bamboo.obj`:** Detailed bamboo stalk model with `PUSHILIN_bamboo.png` texture.
*   **`Fence/Fence.fbx` & `Fence2.fbx`:** Modular low-poly wooden fence segments.
*   **`Well/Well.fbx`:** Dedicated low-poly village stone well model.

### C. Nature Pack (`Assets/Prefabs/FBX-Ultimate-Nature-Pack/FBX/`)
Contains 150 low-poly nature assets, including:
*   **Rocks:** `Rock_1.fbx` to `Rock_7.fbx` (Standard gray rocks), `Rock_Moss_1.fbx` to `Rock_Moss_7.fbx` (Mossy green variants), and `Rock_Snow_1.fbx` to `Rock_Snow_7.fbx` (Snow covered).
*   **Trees:** Birch, Pine, Palm, and Willow trees (including autumn, dead, and snow variants).
*   **Plants & Grass:** Cactus, Flowers, Lilypad, Grass, short grass, Wheat, and Corn crops.
*   **Debris:** `WoodLog.fbx` (Standard, mossy, and snow logs), `TreeStump.fbx`.

---

## 3. Replacement Recommendations

To replace the primitive placeholder geometry currently in `SampleScene.unity` with the newly imported assets, apply the following mappings:

| Target Placeholder | Recommended Imported Model | Rationale |
| :--- | :--- | :--- |
| **Thành's House** | `Assets/Prefabs/HoiAnHouse_M2.fbx` | Matches the Central Vietnam/Hoi An aesthetic perfectly, giving the game a localized, culturally rich appearance. |
| **O Thắm's Shop** | `Assets/Prefabs/asian_house.glb` | An oriental-style house that fits O Thắm's character as a local dealer of seeds and agricultural supplies. |
| **Village Well** | `Assets/Prefabs/Well/Well.fbx` | A dedicated 3D stone well mesh to replace the primitive cylinder well. |
| **Banana Tree** | `Assets/Prefabs/Banana Tree/BananaTree.obj` | Dedicated banana plant model, instantly transforming the tropical village look. |
| **Rock Cluster** | `Assets/Prefabs/FBX-Ultimate-Nature-Pack/FBX/Rock_1.fbx` to `Rock_7.fbx` | Provides natural, irregular low-poly rock shapes to replace primitive gray scale cubes. |

---

## 4. Environment Upgrade Roadmap

*   **Phase 1 (Landmarks & Foliage):** Swap primitive well, banana trees, and rocks with `Well.fbx`, `BananaTree.obj`, and `Rock_1..7.fbx` models. Use `PUSHILIN_bamboo.obj` to create village hedges.
*   **Phase 2 (Buildings):** Swap the primitive player house and O Thắm's shop with `HoiAnHouse_M2.fbx` and `asian_house.glb`. Set up the fence using `Fence.fbx`.
*   **Phase 3 (Polishing & Background):** Replace primitive boundary cubes with modular rocky hills and palm/willow trees from the Ultimate Nature Pack. Add wood logs (`WoodLog.fbx`) as ground details.
