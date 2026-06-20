# Scene Structure

## Main Scene

SampleScene

## Important Objects

* **Player**: Main character with Rigidbody, BoxCollider, PlayerController, and PlayerStats.
  * `Player_Base` (Child GameObject): The 3D stylized model and Animator component.
* **Thanh_House**: The main character's house, holding a custom low-poly yellow farmhouse model and a physical BoxCollider.
* **BacNam_House**: Unified house parent GameObject grouping Bác Năm's traditional house (holding a physical BoxCollider) and his bamboo daybed (chõng tre) visual elements.
* **NPC_BacNam**: Bác Năm character positioned next to his bamboo daybed facing the player, containing dialogue/tutorial logic and a trigger BoxCollider that covers the daybed area for easy interaction.
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

* **Current Playable Loop**: Player walks/runs, tills SoilCells, plants and waters crops, triggers growth, harvests with HUD feedback, trades items at O Thắm's newly styled shop, interacts with NPC Bác Năm at his newly styled house and chõng tre, and tracks storage.
* **Current Blockers**: None.
* **Recommended Next Task**: Investigate and fix remaining invisible colliders/blockers near environment landmarks.
