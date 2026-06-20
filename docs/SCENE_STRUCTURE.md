# Scene Structure

## Main Scene

SampleScene

## Important Objects

* **Player**: Main character with Rigidbody, BoxCollider, PlayerController, and PlayerStats.
  * `Player_Base` (Child GameObject): The 3D stylized model and Animator component.
* **NPC_BacNam**: Bác Năm character with conversation logic and trigger BoxCollider.
* **OTham_Shop**: Unified shop parent GameObject grouping the shop house (holding a standard physical BoxCollider) and the market stall visual elements.
* **NPC_OTham**: O Thắm character positioned right behind the counter of the market stall facing the player, containing dialogue/shop logic and a trigger BoxCollider that extends forward over the counter for easy interaction.
* **AncestralAltar**: Altar object with thắp nhang logic and trigger BoxCollider.
* **SoilCells**: 4 tilled soil plots (`SoilCell_Grid2`, `SoilCell_Grid3`, `SoilCell_Grid4`, and `SoilCell`).
* **Paths**: Holds `RoadSegment` GameObjects.
  * *Note*: The `MeshCollider` components on `RoadSegment` child elements are disabled to allow smooth player movement.
* **Main Camera**: Handles third-person follow.
* **Global Volume**: Scene lighting and fog.

---

# PROJECT STATUS SNAPSHOT

* **Current Playable Loop**: Player walks/runs, tills SoilCells, plants and waters crops, triggers growth, harvests with HUD feedback, trades items at O Thắm's newly styled shop, and tracks storage.
* **Current Blockers**: None.
* **Recommended Next Task**: Investigate and fix remaining invisible colliders/blockers near environment landmarks.
