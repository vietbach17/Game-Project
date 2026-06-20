# TODO

## High Priority

1. Replace remaining placeholder crop visuals with real crop sprites/models for other crops.
2. Upgrade remaining village environment landmarks and layout structures.
3. Fix outer boundary elements (`BoundaryElement`) microscopic visual scales.

## Medium Priority

1. Weather impact balancing (adjust evaporation speed and stress values).
2. Save / Load system for player stats and farm grid status (Control settings rebinds are already completed & saved).
3. NPC daily routines and pathfinding.
4. Community progression and Altars vần công credits system.

## Low Priority

1. Visual polish (particle effects, lighting post-processing).
2. Advanced farming content (more crop variants and soil quality upgrades).

---

## Recently Completed

* **Audio System**: Implemented `AudioManager` supporting BGMs, weather-reactive ambient loops (rural, Lao wind, storm), and contextual SFX (dig, water, plant, harvest, coin, click, faint, altar).
* **Settings & Controls Customization**: Added interactive Settings Panel in HUD (via bottom-right Gear Button or Esc) and Main Menu (OnGUI) with tabs for key customization, survival guide, and NPC profiles.
* **Dual Input backend wrapper**: Solved legacy and New Input backend crash issues for key queries.
* **NPC O Thắm Shop Setup**: Replaced old placeholder models with low-poly stylized models for O Thắm's shop house and market stall. Created a custom editor script `SetupOThamShop.cs` (`Sown In Stone -> Setup O Tham Shop` menu item) to instantiate models, correct pivots/scales (house 4.5m, stall 1.2m, O Thắm 1.7m), position them left of Thành's house, and generate accurate collision and trigger bounds.
* **Faint & Rescue Loop**: Collapsing from exhaustion or stress triggers rescue at Bác Năm's house with partial resource deductions.

---

# PROJECT STATUS SNAPSHOT

* **Current Playable Loop**: Player can walk/run around the map (customizable keys), inspect SoilCells, till rocks, plant potato seeds (with seeds consumed from storage), water the soil, use the F1 debug command to mature crops quickly for testing, harvest mature crops, receive visual HUD toast feedback, trade with NPC O Thắm at her newly decorated shop (consisting of the house and market stall), view accumulated items/coins in the Inventory (`Tab`/`I` keys), adjust setting options & binds, hear ambient/SFX audio, and pass out to trigger Bac Nam's rescue loop.
* **Current Blockers**: None.
* **Recommended Next Task**: Investigate and fix remaining invisible colliders/blockers near environment landmarks.
