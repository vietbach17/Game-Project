# SampleScene to Village_Demo Asset Migration Audit

## Executive Summary
This audit report outlines the visual, environmental, and structural assets present in the developer sandbox scene `SampleScene.unity` and evaluates their suitability for migration into the community survival demo scene `Village_Demo.unity`. 

Our inspection reveals that `SampleScene.unity` contains critical 3D model setups and environment props (such as the actual 3D meshes for **O Thắm's Shop**, **Bác Năm's House**, **Thành's House**, and the **Ancestral Altar**) that are currently missing in `Village_Demo.unity` or represented only by simple placeholder primitives. Migrating these high-quality visual assets will significantly improve the visual fidelity of the PRU213 demo, elevating it from a blockout to a professional-looking low-poly indie game, without risking the logic or community survival systems of `Village_Demo`.

---

## Safe to Migrate
The following assets are visual-only or contain simple collision components. They are safe to migrate directly:

| Object/Asset Name | Current Location in SampleScene | Type | Why Useful | Suggested Village_Demo Location | Migration Risk |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **OTham_Shop** | `_Environment/Houses/OTham_Shop` | GameObject (MeshFilter/Renderer) | Contains real 3D models for O Thắm's house and two market stall displays. | `Environment/_Environment/Houses/` (Replaces `House_OTham_PLACEHOLDER`) | Low |
| **BacNam_House** | `_Environment/Houses/BacNam_House` | GameObject (MeshFilter/Renderer) | Contains real 3D models for Bác Năm's house and a rustic wooden daybed. | `Environment/_Environment/Houses/` | Low |
| **Thanh_House** | `_Environment/Houses/Thanh_House` | GameObject (MeshFilter/Renderer) | Contains the 3D model for Thành's home. | `Environment/_Environment/Houses/` | Low |
| **AncestralAltar/AltarModel** | `AncestralAltar/AltarModel` (child only) | GameObject (MeshFilter/Renderer) | Replaces the placeholder visual model of the altar with the actual 3D model assets. | `InteractionZones/AncestralAltar/` (Attach as child, keep existing script on parent) | Low |

---

## Migrate With Caution
These objects carry scripts or gameplay logic. Care must be taken to migrate only their visual meshes, or to ensure their script properties match the updated survival/community systems in `Village_Demo.unity`:

| Object/Asset Name | Current Location | Reason for Caution | Required Checks before Migration | Risk |
| :--- | :--- | :--- | :--- | :--- |
| **AncestralAltar** (Parent) | Root | Contains the `AncestralAltar` script. Migrating the parent could overwrite customized inspector references or trigger logic. | Keep the existing `AncestralAltar` parent object in `Village_Demo` and only copy the 3D submeshes (`AltarModel`) into it. | Medium |
| **NPC_BacNam** / **NPC_OTham** | Root | These contain active `NPCCharacter` scripts. `Village_Demo` has highly tailored proximity popup options and look-at scripts. | **Do NOT migrate parent NPC objects.** Keep the current NPCs in `Village_Demo` and only swap out visual models if needed. | High |

---

## Do Not Migrate
The following objects should not be migrated as they are redundant, obsolete sandbox tools, or present extreme regression risks to gameplay:

| Object/Asset Name | Reason |
| :--- | :--- |
| **_Managers** | Contains core gameplay managers (GameManager, PlayerStats, WeatherManager, etc.). Overwriting these will break the newly implemented community survival phases, Nghĩa Tình calculations, and weather heights. |
| **Player** | Contains the active `PlayerController` and camera follow setups. Replaces the customized Roblox-style camera controls, collision sweeps, and movement feel. |
| **TestController** | Redundant. The test controller and menu logic are already clean and well-structured under `_UI/TestController` in `Village_Demo`. |
| **SoilCell / SoilCell_Grid1-9** | Redundant. The 11 flat SoilCells in `Village_Demo` are already properly configured in a clean farming grid with target highlighting. |
| **CropInstance_Test** | These are runtime testing crops that should not be baked into the scene file. |

---

## Missing or Broken References
*   **Resolved Reference (Village_Demo)**: The previously missing prefab `House_OTham` (guid: `11405ec53ed7de04a9255246da9bf2d2`) was successfully cleared and replaced with a flat 3D placeholder layout in `Village_Demo.unity`.
*   **SampleScene Sandbox Inconsistencies**: Several `SoilCell` instances in `SampleScene` have hardcoded test values and raw coordinates that do not align with the flat Y=0 coordinates of the `Village_Demo` URP terrain.

---

## Recommended Migration Order
To guarantee a clean integration that doesn't break active gameplay:
1.  **Extract Visual Meshes**: Copy only the child visual models (`HouseModel`, `StallModel`, etc.) from `SampleScene`'s houses.
2.  **Replace Placeholders in Village_Demo**: Parent the copied house visual models under the corresponding `Environment/_Environment/Houses` hierarchy in `Village_Demo`.
3.  **Upgrade Altar Visuals**: Paste the `AltarModel` visual child under `Village_Demo`'s `AncestralAltar` root, preserving the parent's collider and active script.
4.  **Scene Dressings**: Copy safe decorative vegetation or paths from `SampleScene` to make the village roads look complete.

---

## Suggested Placement in Village_Demo
*   **OTham_Shop**: Place at `(8.0, 0.0, 10.28)` under `Environment/_Environment/Houses/`, replacing the Cube primitive.
*   **BacNam_House**: Place at `(-12.0, 0.0, 8.5)` near Bác Năm's NPC area to anchor his chat interactions.
*   **Thanh_House**: Place at `(0.0, 0.0, -6.0)` right behind the player spawn point (home position).
*   **AltarModel**: Center under the existing `AncestralAltar` collider object at `(0.0, 0.0, 15.0)`.

---

## Final Recommendation
1.  **First Priority (Migrate Immediately)**: The 3D model children for `OTham_Shop` and `BacNam_House`.
2.  **Ignore Completely**: `_Managers`, `Player`, `TestController`, and all test crops (`CropInstance_Test`).
3.  **Code Changes**: **No code changes are needed.** The scripts in `Village_Demo` are fully robust and reference parent coordinate transforms, meaning visual mesh swaps will work automatically.
