# Gameplay Systems

## Farming

* **Digging (Clear Rocks)**: Soil cells start with high rock density. Digging decreases rock density to improve soil quality (from BacMau to TrungBinh).
* **Planting**: Player plants seeds from inventory. Crop type matches seed type.
* **Watering**: Wet soil satisfies moisture needs. Evaporation rate depends on weather and wind speed.
* **Crops Growth**: Crops grow over days. Growth speed is modified by soil quality, moisture satisfaction, nutrients, and rock density obstacle factors.
* **Harvesting**: Harvesting yields items depending on soil quality (e.g., base yield 2, or 5 if Phù Sa soil).
* **Harvest Rewards & Feedback**: Crops are removed, rewards are added to the storage slot, and a toast message ("Thu hoạch thành công: +X [Item]") is displayed on the HUD screen.

## Community

* **Reputation / Affection**: Increased by gifting items and talking to villagers (Bác Năm, O Thắm).
* **Village Support (Vần Công)**: Exchanging labor credits to chằng chống nhà cửa during storms.

## Weather

* **Normal**: Standard temperature and moisture decay.
* **Gió Lào**: High temperature, very low humidity, accelerated soil water evaporation, increases Heat Stress.
* **Mưa Bão (Storm/Flood)**: Rains heavily, water level rises. Flood-intolerant crops rot if flooded. Cold Stress increases.
* **Phù Sa**: Rains recede, leaving nutrient-rich silt quality soil.

## Inventory & Notifications

* **Storage**: Slots display item name, icon, and count. Fresh crops decay in high humidity unless processed into dry items (e.g., Khoai Gieo).
* **On-Screen HUD Toast**: Displays active alerts and harvest notifications at the upper-middle HUD.

## Settings & Key Customization

* **Dynamic Rebinds**: Players can rebind their movement (W, A, S, D), Interaction (E), and Running (LeftShift) keys. Saved to `PlayerPrefs` and automatically updates dynamic tooltips (e.g. "Nhấn [E] để tương tác" updates dynamically if re-bound).
* **Survival Guides**: Contextual pages inside settings explaining gameplay loops (Farming, Weather, Altar, Rescue).
* **NPC Bios & Affinity**: Real-time display of community NPC stats (Bac Nam, O Tham) and affection level.
* **Auto-Pause**: Activating settings panels scales the game's time scale to zero.

## Economy & Shop Setup

* **Trade system**: NPC O Thắm sells seeds and incense and buys harvested fresh crops.
* **Shop Environment Layout**: O Thắm's shop consists of a shop house (scaled to 4.5m) and a market stall (scaled to 1.2m) located in the empty field to the left of Thành's house (X: 4.5, Z: -10.0).
* **Interactive Alignment**: NPC O Thắm stands behind the counter of the market stall facing the player (Z: -11.2, rotated 180 degrees), with a BoxCollider trigger covering the front of the counter for smooth interactions. A physical BoxCollider is attached to the shop house to prevent clipping.
* **Automated Setup Tool**: A custom Editor script `SetupOThamShop.cs` is accessible via the Unity Editor menu (`Sown In Stone -> Setup O Tham Shop`) to rebuild/re-align the shop models and NPC setup automatically.

## Bác Năm's House & Interaction Setup

* **Role**: NPC Bác Năm guides players in tilling, farming, and gives weather warnings.
* **House Environment Layout**: Bác Năm's home consists of a stylized traditional house (scaled to 4.5m) and a bamboo daybed (chõng tre) (scaled to 1.2m) located at world `(8.0, 0.0, 12.0)` rotated 180 degrees to face south.
* **Interactive Alignment**: NPC Bác Năm is positioned at world `(7.0, 0.5, 7.5)` facing the player, right next to the daybed at `(8.5, 0.0, 7.5)`, with a BoxCollider trigger covering the daybed area for easy interaction. A physical BoxCollider is attached to the house to prevent clipping.
* **Automated Setup Tool**: A custom Editor script `SetupBacNamHouse.cs` is accessible via the Unity Editor menu (`Sown In Stone -> Setup Bac Nam House`) to automatically recreate materials, align pivots, scale heights, and place the house, daybed, and NPC in the scene.

## Player's House (Thành's House) Setup

* **Role**: The main character's home base and initial spawn area.
* **House Environment Layout**: Thành's house consists of a stylized yellow plaster farmhouse (scaled to 4.5m) located at world `(10.66, 0.0, -10.0)` rotated 180 degrees to face the road/player (south).
* **Interactive Alignment**: Serves as the starting point and visual representation of the player's home. A physical BoxCollider is configured around the house walls to prevent clipping.
* **Automated Setup Tool**: A custom Editor script `SetupThanhHouse.cs` is accessible via the Unity Editor menu (`Sown In Stone -> Setup Thanh House`) to automatically recreate materials from textures, align the pivot flat to the ground (Y=0), scale the height to 4.5m, rotate Y by 180, and add the BoxCollider.

## Ancestral Altar Setup

* **Role**: The place where the player can offer incense to restore Morale.
* **Altar Environment Layout**: The altar consists of a custom low-poly stone altar model (scaled to 1.8m) located at world `(7.5, 0.0, -13.0)` (bottom corner of Thành's house).
* **Interactive Alignment**: Configured with a BoxCollider trigger covering the altar area (`center: (0f, 0.9f, 0f)`, `size: (2.5f, 1.8f, 2.5f)`) so players can easily approach and press `[E]` to interact.
* **Automated Setup Tool**: A custom Editor script `SetupAltar.cs` is accessible via the Unity Editor menu (`Sown In Stone -> Setup Altar`) to recreate materials, instantiate under `AncestralAltar`, scale to 1.8m, place flat on the ground (Y=0), and configure the trigger.

## Sound & Ambiance (Audio)

* **Atmospheric Ambiance**: Automatic fading between ambient soundscapes (calm wind for normal weather, howling dry wind for Gió Lào, thunderstorms for Storm/Flood phases).
* **Interactive SFX**: Plays action-specific sounds like digging (`sfx_dig`), planting (`sfx_plant`), watering (`sfx_water`), harvesting (`sfx_harvest`), altar offerings (`sfx_altar`), coin transactions (`sfx_coin`), button clicks (`sfx_click`), warning alerts (`sfx_warning`), and fainting (`sfx_faint`).

## Survival & Faint-Rescue Loop

* **Physiological Stresses**: Stresses (Heat Stress, Cold Stress) rise according to weather and wind speed. Exhaustion or peak stress levels deplete player health.
* **Faint & Recovery**: Dropping to zero health or reaching 100% stress triggers a fainting sequence. The player is rescued by Bác Năm, waking up in Bác Năm's house the next morning with restored stats but losing a portion of coins/inventory items.

---

# PROJECT STATUS SNAPSHOT

* **Current Playable Loop**: Player walks/runs (custom keybindings), tills SoilCells, plants and waters crops, triggers growth, harvests with HUD feedback, trades items at O Thắm's newly styled shop, interacts with NPC Bác Năm at his newly styled house and chõng tre, interacts with the dynamic settings UI, hears ambient loops & action SFX, and triggers the Faint & Rescue loop.
* **Current Blockers**: None.
* **Recommended Next Task**: Investigate and fix remaining invisible colliders/blockers near environment landmarks.
