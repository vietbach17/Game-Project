# Art Pipeline

## Character Source

Meshy AI (Stylized Low Poly farmer model)

## Rigging

Mixamo Humanoid Rigging

## Export Format

FBX for Unity

## Animation Policy

* **Player_Base.fbx**: Contains Avatar and skeleton structure.
* **Animation Clips**: Walk, Run, Dig, Watering, Plant, Harvest, Jump. (Imported as separate FBX files without skin and retargeted to Player_Base avatar).

## Never Use

AccuRig (Causes skeleton mismatches).

## Crop Visuals Pipeline

* **Ideal sprites**: Growth stage sprites defined in `CropData`.
* **3D Fallback**: If growth stage sprites are missing or contain default UI textures (e.g. `Knob`, `InputFieldBackground`, `UISprite`, `Background`, `Checkmark`), `CropInstance` will automatically instantiate and color 3D primitive geometry (Cylinder stem + Cube leaves) representing seedling, growing, and mature crop shapes.
* Currently `Crop_KhoaiLang.asset` utilizes this fallback pipeline.

---

# PROJECT STATUS SNAPSHOT

* **Current Playable Loop**: Player walks/runs, tills SoilCells, plants and waters crops, triggers growth, harvests with HUD feedback, and tracks storage.
* **Current Blockers**: None.
* **Recommended Next Task**: Investigate and fix remaining invisible colliders/blockers near environment landmarks.
