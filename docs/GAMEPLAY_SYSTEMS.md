# Gameplay Systems Document: Đất Cày Lên Sỏi Đá

Cập nhật: **2026-06-27**. Tài liệu này mô tả các gameplay systems theo source hiện tại. Những phần cũ từng ghi “đã hoàn thiện” nhưng source chưa có đầy đủ nay được đánh dấu rõ là partial/missing.

---

## 1. Phase & Time System

**File:** `Assets/Scripts/Core/GameManager.cs`

### Có hiện tại

- Singleton `GameManager.Instance`.
- `currentDay`, `timeOfDay`, `dayLengthInSeconds`.
- Runtime enum `GamePhase`: `LapNghiep`, `GioLao`, `ChuanBiBao`, `MuaBao`, `PhuSa`, `EndGame`.
- Static events:
  - `OnDayChanged(int)`
  - `OnPhaseChanged(GamePhase)`
- Tự tăng giờ/ngày trong `Update()` nếu `isTimerRunning`.
- Khi ngày 3 và đang `LapNghiep`, tự chuyển sang `ChuanBiBao`.
- `TriggerStormCrisis()` chuyển sang `MuaBao` nếu đang trước bão.
- `MuaBao` dừng timer và gọi `SurvivalUIManager.StartEvacuationCountdown(45f)`.
- `PhuSa` khôi phục timer và `Time.timeScale`.

### Thiếu / cần kiểm tra

- `GioLao` tồn tại trong enum/weather/UI nhưng chưa được auto-schedule trong `GameManager.Update()`.
- `EndGame` enum tồn tại nhưng chưa thấy được dùng trong `GameManager`.
- Ending không được trigger trực tiếp từ timeline trong file hiện tại.

---

## 2. Player Controller & Interaction

**File:** `Assets/Scripts/Core/PlayerController.cs`

### Có hiện tại

- Singleton `PlayerController.Instance`.
- Key binding fields cho menu compatibility: `keyMoveUp`, `keyMoveDown`, `keyMoveLeft`, `keyMoveRight`, `keyInteract`, `keyRun`.
- Flood evacuation state: `isEvacuationActive`, `rescuedCount`, `evacuationTimer`, `evacuatedNPCs`.
- Khi phase `MuaBao`, bật countdown 45 giây nội bộ.
- `TryEvacuateNPC(NPCCharacter npc)`:
  - Chặn NPC đã cứu.
  - Cộng `+15` Nghĩa Tình.
  - Tắt GameObject NPC.
  - Dắt đủ 4 NPC thì chuyển `PhuSa`, teleport lên mái nhà, cấp mì tôm nếu tìm được item.
- Failure hết giờ gọi `SurvivalUIManager.ShowDrownGameOverPanel()`.
- Compatibility wrappers:
  - `LoadKeyBindings()`
  - `SetAnimTrigger()`
  - `LockMovement()`
  - `TriggerRescueSequence()`
  - `EquipNonLa()`
  - `OpenTradeMenu()`

### Thiếu / rủi ro lớn

- Source hiện tại **không có movement loop đầy đủ** trong `PlayerController.Update()` ngoài countdown.
- Không thấy `TryPerformInteraction()`, physics scan farming, hoặc camera-relative movement trong file hiện tại.
- `CurrentTargetSoilCell` property tồn tại nhưng không được cập nhật trong file này.
- Nếu Unity scene không có controller khác bù lại, player có thể không di chuyển/tương tác farming được dù C# compile sạch.

---

## 3. Farming & Crop System

**Files:**

- `Assets/Scripts/Agriculture/SoilCell.cs`
- `Assets/Scripts/Agriculture/CropInstance.cs`
- `Assets/Scripts/Agriculture/CropData.cs`

### Có hiện tại

`SoilCell`:

- Trạng thái đất: `BacMau`, `TrungBinh`, `PhuSa`.
- Chỉ số: `Moisture`, `Nutrients`, `RockDensity`.
- Parent-child linking tự động dựa trên tên `SoilCell`/`Grid` và khoảng cách XZ.
- Bay hơi nước theo `WeatherManager`:
  - Lũ > 0.1: đất ẩm 100%.
  - Mưa > 0.1: tăng ẩm.
  - Gió Lào tăng evaporation speed.
- Phase `PhuSa`: set quality `PhuSa`, nutrients 100, rock 0.
- Actions:
  - `ActionClearRocks(float)`
  - `ActionWaterSoil(float)`
  - `ActionFertilize(float)`
  - `ActionPlantCrop(CropData)`
- Visual switching giữa rocky/clean/tilled/wet hoặc sprite.
- Highlight target bằng sprite overlay hoặc khung cube vàng.
- Debug helpers.

`CropInstance`:

- Tăng trưởng theo `OnDayChanged`.
- Growth phụ thuộc moisture, nutrients, rock density.
- Flood rot nếu `FloodLevel > 0.3f` và không có `FloodBarrier` trong radius.
- Wither nếu nhiệt độ vượt tolerance và đất quá khô.
- `IsReadyToHarvest()`, `ActionHarvest()`, `DebugMature()`, `DebugGrowOneStage()`.
- Yield trên đất `PhuSa` là 5 thay vì base 2.

### Thiếu / cần caller xử lý

- `SoilCell` không tự trừ stamina hoặc item seed; caller phải làm.
- `ActionPlantCrop()` nhận `CropData` trực tiếp, không tự kiểm tra kho hạt giống.
- Vì `PlayerController` hiện thiếu interaction scan, cần kiểm tra scene/runtime có script khác gọi các action này không.

---

## 4. Weather & Disaster System

**File:** `Assets/Scripts/Weather/WeatherManager.cs`

### Có hiện tại

- `WeatherType`: `OnDinh`, `NangNong`, `GioLao`, `MuaGiong`, `BaoLu`.
- Live stats: temperature, humidity, rainIntensity, floodLevel, windSpeed.
- Phase target values:
  - `LapNghiep`: ổn định.
  - `GioLao`: 42°C, humidity thấp, wind cao.
  - `ChuanBiBao`: mưa giông nhẹ.
  - `MuaBao`: bão lũ, flood target 1.85.
  - `PhuSa`: ổn định sau lũ, flood target -1.5.
- Lerp stats mỗi frame.
- Spawn `waterPlanePrefab` dưới `disasterContainer` nếu có.
- Disable `MeshCollider` của water plane.
- Public APIs cho UI/soil/crop: `Temperature`, `Humidity`, `RainIntensity`, `FloodLevel`, `WindSpeed`, `currentVisualWeather`.

### Thiếu / partial

- `UpdateSoilEvaporationParameters()` đang rỗng.
- Không có rain particle/skybox/ambient wind trong source hiện tại.
- Không có vùng lũ theo địa hình; flood là numeric stat + water plane Y.

---

## 5. Storage, Items & Cooking

**Files:**

- `Assets/Scripts/Storage/StorageManager.cs`
- `Assets/Scripts/Storage/ItemData.cs`
- `Assets/Scripts/Interaction/KitchenHearth.cs`

### Có hiện tại

- `StorageManager` quản lý list `InventorySlot` runtime.
- `AddItem`, `RemoveItem`, `GetStorageSlots`, `GetItemDataByID`, `GetItemQuantity`.
- Decay nông sản tươi mỗi ngày khi humidity cao.
- `CraftPreservedItem()` đổi fresh -> preserved và trừ stamina.
- `ItemData`: ID, tên, mô tả, type, icon, stamina/morale restore, decay rate.
- `KitchenHearth` dùng `IInteractable`, có lựa chọn sấy khô hoặc nấu ăn.

### Thiếu / partial

- Inventory không được save/load.
- `GetItemDataByID()` fallback `Resources.LoadAll<ItemData>(string.Empty)` phụ thuộc asset đặt trong Resources; nếu asset không nằm trong Resources thì chỉ tìm được item đã có sẵn trong kho.

---

## 6. NPC, Nghĩa Tình & Vần Công

**Files:**

- `Assets/Scripts/Community/CommunityManager.cs`
- `Assets/Scripts/Community/NPCCharacter.cs`
- `Assets/Scripts/UI/NPCProximityOptionsUI.cs`
- `Assets/Scripts/UI/NPCQuestMarkerUI.cs`

### Có hiện tại

- `CommunityManager` có `GlobalKarma`, event flags, NPC list, `ModifyGlobalKarma()`, `AddKarma()`.
- Vần công tính từ `NPCCharacter.VanCongCredits`.
- `TriggerStormHelpSequence()` thưởng/phạt morale theo tổng credits và tiêu credit.
- `NPCCharacter` hỗ trợ `BacNam`, `OTham`, `CuBay`, `BeTi`, `Custom`.
- Fallback dialogues theo phase và affection.
- Affection, Vần công credits, gift action, look-at-player, return rotation.
- `NPCProximityOptionsUI` có các hành động talk/work/event/trade/share food.
- `NPCQuestMarkerUI` hiển thị marker theo phase/event flags.

### Thiếu / partial

- `TriggerStormHelpSequence()` chưa được gọi trong `GameManager` hiện tại.
- Không có quest data model tổng quát; event progression đang ad hoc qua bool flags.
- NPC rescue flow trong `PlayerController` tắt NPC GameObject nhưng `ActivateNPCsOnRoof()` vẫn là placeholder.

---

## 7. UI / HUD / Menu

**Files:**

- `Assets/Scripts/UI/SurvivalUIManager.cs`
- `Assets/Scripts/UI/TutorialManager.cs`
- `Assets/Scripts/FrameworkMainMenuUI.cs`
- `Assets/Scripts/FrameworkDebugUI.cs`
- `Assets/Scripts/UI/EndingManager.cs`

### Có hiện tại

`SurvivalUIManager`:

- HUD bars health/stamina/morale.
- Day/phase/weather/coins text.
- Dialogue typewriter and 2/3 choice buttons.
- Inventory grid, item click handling, item use, nón lá equip, sandbag/floodboard prefab placement if Resources prefab exists.
- Shop UI for O Thắm items.
- Community and weather details panels.
- Heat/cold/water screen overlays.
- Toast notifications with regex highlight.
- Controls legend.
- Phase announcement and optional `VillageSpeakerBanner`.

`TutorialManager`:

- Tracks 4 NPC talk completion.
- Calls farming tutorial after all talks.
- Updates checklist via `UpdateTutorialQuestPanelText()`.
- Current `ShowTutorial(Action)` immediately invokes callback after refreshing HUD.

`FrameworkMainMenuUI`:

- Main/pause/settings UI, key binding display, save/load partial data.

`FrameworkDebugUI`:

- F1 debug / demo controls and default item seeding.

### Thiếu / partial

- `StartEvacuationCountdown()` is toast-only, not a real timer panel.
- `ShowDrownGameOverPanel()` is toast-only.
- `ShowFarmingTutorialSlideshow()` is toast/callback fallback, not full slideshow.
- Procedural UI is large and brittle; inspector references still need Unity validation.

---

## 8. Interactions & Props

### Có hiện tại

- `AncestralAltar`: checks tutorial, checks pre-storm, consumes incense, restores morale, triggers storm.
- `Coracle`: float/row boat during flood, enter/exit prompt.
- `MudPuddle`: visible in `PhuSa`, consumes stamina, triggers dig animation, locks movement, destroys itself.
- `FloodBarrier`: protection radius marker for crop flood protection.
- `CockfightingZone` / `CockfightingMinigame`: optional experimental OnGUI minigame, pauses time and grants morale on win.

### Scope note

Cockfighting is **experimental** and should not be described as part of the main PRU213 survival demo unless the team explicitly chooses to keep it.

---

## 9. Priority Fix List

1. Restore/verify `PlayerController` movement and farming interactions in Play Mode.
2. Replace toast-only evacuation/game-over/tutorial fallbacks with real UI panels only if needed for demo.
3. Wire `CommunityManager.TriggerStormHelpSequence()` at the intended phase point.
4. Implement or remove docs references to roof sandbag slots and wall repair nodes.
5. Decide save/load scope; either mark partial clearly in UI or persist inventory/world state.
6. Decide cockfighting scope before final demo.
