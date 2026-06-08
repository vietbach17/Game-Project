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

---

# PROJECT STATUS SNAPSHOT

* **Current Playable Loop**: Player walks/runs, tills SoilCells, plants and waters crops, triggers growth, harvests with HUD feedback, and tracks storage.
* **Current Blockers**: None.
* **Recommended Next Task**: Investigate and fix remaining invisible colliders/blockers near environment landmarks.
