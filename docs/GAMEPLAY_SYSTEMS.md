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

## Sound & Ambiance (Audio)

* **Atmospheric Ambiance**: Automatic fading between ambient soundscapes (calm wind for normal weather, howling dry wind for Gió Lào, thunderstorms for Storm/Flood phases).
* **Interactive SFX**: Plays action-specific sounds like digging (`sfx_dig`), planting (`sfx_plant`), watering (`sfx_water`), harvesting (`sfx_harvest`), altar offerings (`sfx_altar`), coin transactions (`sfx_coin`), button clicks (`sfx_click`), warning alerts (`sfx_warning`), and fainting (`sfx_faint`).

## Survival & Faint-Rescue Loop

* **Physiological Stresses**: Stresses (Heat Stress, Cold Stress) rise according to weather and wind speed. Exhaustion or peak stress levels deplete player health.
* **Faint & Recovery**: Dropping to zero health or reaching 100% stress triggers a fainting sequence. The player is rescued by Bác Năm, waking up in Bác Năm's house the next morning with restored stats but losing a portion of coins/inventory items.

---

# PROJECT STATUS SNAPSHOT

* **Current Playable Loop**: Player walks/runs (custom keybindings), tills SoilCells, plants and waters crops, triggers growth, harvests with HUD feedback, trades items, interacts with the dynamic settings UI, hears ambient loops & action SFX, and triggers the Faint & Rescue loop.
* **Current Blockers**: None.
* **Recommended Next Task**: Investigate and fix remaining invisible colliders/blockers near environment landmarks.
