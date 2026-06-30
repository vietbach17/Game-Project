# Current Source Context Report — Đất Cày Lên Sỏi Đá

Cập nhật: **2026-06-27**. Đây là snapshot đọc từ `Assets/Scripts` hiện tại, không phải lịch sử mong muốn trong các docs cũ.

---

## 1. Project Overview

- **Unity Editor:** Unity 6 (`6000.4.3f1` theo project context).
- **Primary namespaces:** `SownInStone.Core`, `SownInStone.Agriculture`, `SownInStone.Community`, `SownInStone.Storage`, `SownInStone.UI`, `SownInStone.Weather`, `SownInStone.Audio`, `SownInStone.Interactions`, `SownInStone.Interaction`.
- **Main scene target:** `Assets/Scenes/Village_Demo.unity` nếu scene tồn tại và build settings đúng. Một số docs cũ vẫn nhắc `SampleScene`; cần kiểm tra Unity scene trực tiếp trước khi chỉnh scene.
- **Compile status:** sau lần sửa gần nhất, `dotnet build Assembly-CSharp.csproj` không còn `error CS...` trong `Temp/compile-check.log`.

---

## 2. Active Script Map

### Core

| Script | Current role | Notes |
|---|---|---|
| `Core/GameManager.cs` | Singleton ngày/giờ, runtime phases, events, storm trigger. | Tự chuyển ngày 3 từ `LapNghiep` sang `ChuanBiBao`; `GioLao` chưa có auto schedule. |
| `Core/PlayerController.cs` | Sơ tán NPC, roof refuge transition, keybinding compatibility, wrappers. | Hiện **không còn movement/farming scan đầy đủ** trong file; cần khôi phục nếu gameplay movement mất trong Unity. |
| `Core/PlayerStats.cs` | Health, stamina, morale, coins, heat/cold stress, item use. | Có rescue callback tối giản. |
| `Core/CameraFollow3D.cs` | Camera follow/orbit. | Có nhiều camera-mode APIs được các script khác gọi. |
| `Core/WanderingAnimal.cs` | Animal wandering behavior. | Phụ trợ scene. |

### Agriculture

| Script | Current role | Notes |
|---|---|---|
| `Agriculture/SoilCell.cs` | Soil quality/moisture/nutrients/rock density, parent-child field linking, clear/water/fertilize/plant, visuals/highlight. | Không tự tiêu hao stamina/item; caller phải xử lý nếu muốn gameplay hoàn chỉnh. |
| `Agriculture/CropInstance.cs` | Crop growth by day, flood/heat damage, harvest yield, 2D/3D fallback visuals. | Flood protection tìm `FloodBarrier` theo radius. |
| `Agriculture/CropData.cs` | Crop ScriptableObject config. | Dữ liệu phụ thuộc asset. |

### Weather

| Script | Current role | Notes |
|---|---|---|
| `Weather/WeatherManager.cs` | Weather enum, live stats, phase target values, flood water plane lerp. | Bản hiện tại tối giản; `UpdateSoilEvaporationParameters()` rỗng; chưa có particles/ambient đầy đủ. |

### Storage

| Script | Current role | Notes |
|---|---|---|
| `Storage/StorageManager.cs` | Runtime inventory, add/remove, item lookup, item quantity, humidity decay, preserved item crafting. | Chưa có save/load inventory. |
| `Storage/ItemData.cs` | Item ScriptableObject. | `ItemID`, `ItemName`, type, icon, restore values, decay rate. |

### Community

| Script | Current role | Notes |
|---|---|---|
| `Community/CommunityManager.cs` | GlobalKarma, event flags, Vần công credits calculation, storm-help sequence. | `TriggerStormHelpSequence()` chưa thấy được gọi từ `GameManager`. |
| `Community/NPCCharacter.cs` | NPC identity, fallback dialogues by phase, affection, Vần công credits, gifts, look-at. | Supports `BacNam`, `OTham`, `CuBay`, `BeTi`, `Custom`. |
| `Community/NPCData.cs` | ScriptableObject dialogue data. | Optional path for authored NPC data. |

### UI

| Script | Current role | Notes |
|---|---|---|
| `UI/SurvivalUIManager.cs` | Large central HUD/dialogue/inventory/shop/toast/weather/community/overlay manager. | Many UI elements are created procedurally at runtime. |
| `UI/TutorialManager.cs` | Intro 4-NPC gate, farming checklist callbacks. | `ShowTutorial(Action)` currently refreshes HUD then immediately calls callback; slideshow is not a full image flow in current source. |
| `UI/NPCProximityOptionsUI.cs` | Floating NPC options panel. | Handles talk/work/events/trade. |
| `UI/NPCQuestMarkerUI.cs` | Floating quest marker. | Based on phase and community event flags. |
| `UI/NghiaTinhUI.cs` | Karma slider display. | Needs inspector references. |
| `UI/VillageSpeakerBanner.cs` | Phase announcement banner. | Optional serialized reference in `SurvivalUIManager`. |
| `UI/EndingManager.cs` | Ending panel by karma thresholds. | Relies on scene UI references. |
| `UI/Billboard.cs` | Face camera helper. | Simple utility. |

### Interactions

| Script | Current role | Notes |
|---|---|---|
| `Interactions/AncestralAltar.cs` | Consumes incense, restores morale, triggers storm crisis. | Has `IsIncenseBurning` and `ActionBurnIncense()` compatibility API. |
| `Interaction/KitchenHearth.cs` | `IInteractable`: dry crops/cook fresh crop via dialogue. | Folder is singular `Interaction`, while other interactables are plural `Interactions`. |
| `Interactions/Coracle.cs` | Boat enter/exit/rowing and flood Y alignment. | Uses `PlayerController.keyInteract`. |
| `Interactions/FloodBarrier.cs` | Flood protection radius marker. | Placement is driven elsewhere; object itself is simple. |
| `Interactions/MudPuddle.cs` | Appears in `PhuSa`, consumes stamina, locks player, destroys itself. | Uses `PlayerController` wrappers. |
| `Interactions/CockfightingZone.cs`, `CockfightingMinigame.cs` | Experimental OnGUI minigame. | Not core survival loop; decide keep/remove before demo. |

### Framework / Tooling

| Script | Current role | Notes |
|---|---|---|
| `FrameworkMainMenuUI.cs` | OnGUI main/pause/settings/save/load/debug-ish menu. | Save/load is partial. Uses legacy `KeyCode` display. |
| `FrameworkDebugUI.cs` | F1/demo controls, resource injection, phase jumps, farming debug. | Editor/demo helper, not production UX. |
| `FrameworkTester.cs` | Simple runtime validation/test harness. | Useful for smoke checks. |
| `Editor/*.cs` | Scene/setup/import helper scripts. | Editor-only automation. |

---

## 3. What Is Implemented vs Partial

### Implemented enough to build on

- Runtime phase events and weather stat propagation.
- Storage add/remove/decay/crafting at runtime.
- Soil/crop simulation APIs.
- Dialogue/choice/toast/inventory/shop UI foundation.
- NPC fallback dialogue and community karma primitives.
- Altar storm trigger.
- Basic flood transition values and water plane movement.

### Partial / prototype / compatibility-only

- `PlayerController` movement and farming interaction loop: docs old claim a full controller, but current file is reduced to evacuation + wrappers.
- Tutorial slideshow: current implementation is a toast/callback fallback, not a multi-slide UI.
- Evacuation UI: `StartEvacuationCountdown()` shows toast only; no persistent countdown panel in current source.
- Roof refuge anchors and wall repair nodes: referenced by docs/design, not implemented as complete source systems.
- Flood barriers: object exists and crop protection checks radius, but placement/persistence is minimal.
- Nón Lá: equip flag and optional prefab instantiate exist, but heat-stress reduction is not wired in `PlayerStats`/weather logic.
- Save/load: partial PlayerPrefs only; no world state.
- Cockfighting: experimental, unrelated to core loop.

---

## 4. Current Critical Gaps

1. **Restore/verify player movement and interaction loop** in `PlayerController` or another controller script.
2. **Wire farming actions to inventory/stamina** if not already done elsewhere in scene.
3. **Turn evacuation into a visible HUD** instead of a toast-only notification.
4. **Persist or intentionally drop save/load scope**; current partial save can mislead players.
5. **Decide cockfighting scope**: remove from demo checklist or reframe as non-core optional test.
6. **Unify `Interaction` vs `Interactions` folder/namespace convention** to avoid future misplaced files.
7. **Run actual Unity Play Mode smoke test** after opening scene, because compile success does not prove scene references are valid.

---

## 5. Files Most Important to Inspect Before Changes

- `Assets/Scripts/Core/PlayerController.cs`
- `Assets/Scripts/Core/GameManager.cs`
- `Assets/Scripts/Weather/WeatherManager.cs`
- `Assets/Scripts/UI/SurvivalUIManager.cs`
- `Assets/Scripts/UI/TutorialManager.cs`
- `Assets/Scripts/UI/NPCProximityOptionsUI.cs`
- `Assets/Scripts/Agriculture/SoilCell.cs`
- `Assets/Scripts/Agriculture/CropInstance.cs`
- `Assets/Scripts/Storage/StorageManager.cs`
- `Assets/Scripts/Community/NPCCharacter.cs`
- `Assets/Scripts/Community/CommunityManager.cs`
