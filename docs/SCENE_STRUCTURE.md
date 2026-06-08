# Scene Structure

## Main Scene

SampleScene

## Important Objects

* **Player**: Main character with Rigidbody, BoxCollider, PlayerController, and PlayerStats.
  * `Player_Base` (Child GameObject): The 3D stylized model and Animator component.
* **NPC_BacNam**: Bác Năm character with conversation logic and trigger BoxCollider.
* **NPC_OTham**: O Thắm character with shop/dialogue logic and trigger BoxCollider.
* **AncestralAltar**: Altar object with thắp nhang logic and trigger BoxCollider.
* **SoilCells**: 4 tilled soil plots (`SoilCell_Grid2`, `SoilCell_Grid3`, `SoilCell_Grid4`, and `SoilCell`).
* **Paths**: Holds `RoadSegment` GameObjects.
  * *Note*: The `MeshCollider` components on `RoadSegment` child elements are disabled to allow smooth player movement.
* **Main Camera**: Handles third-person follow.
* **Global Volume**: Scene lighting and fog.

---

# PROJECT STATUS SNAPSHOT

* **Current Playable Loop**: Player walks/runs, tills SoilCells, plants and waters crops, triggers growth, harvests with HUD feedback, and tracks storage.
* **Current Blockers**: None.
* **Recommended Next Task**: Investigate and fix remaining invisible colliders/blockers near environment landmarks.
