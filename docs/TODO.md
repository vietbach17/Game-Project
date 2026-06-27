# TODO: Development Status & Remaining Tasks

Cập nhật: **2026-06-27**. Checklist này dựa trên source hiện tại trong `Assets/Scripts`, không dựa trên mục tiêu thiết kế cũ.

---

## 1. Current Status

- [x] C# compile đã sạch sau các lỗi merge gần nhất.
- [x] Weather/GamePhase/Storage/UI/NPC compatibility APIs đã tồn tại để Unity không dừng compile.
- [ ] Cần kiểm tra Play Mode trong Unity để xác nhận scene references và gameplay loop.
- [ ] Cần khôi phục/xác nhận player movement + farming interaction vì `PlayerController` hiện không còn loop này trong source được đọc.

---

## 2. P0 — Must Fix Before Demo

### Player & interaction

- [ ] Xác nhận Thành có di chuyển được trong `Village_Demo.unity`.
- [ ] Nếu không di chuyển: khôi phục movement camera-relative vào `PlayerController.Update()` hoặc xác định script controller khác đang phụ trách.
- [ ] Khôi phục/kiểm tra `TryPerformInteraction()` hoặc tương đương để gọi:
  - [ ] `SoilCell.ActionClearRocks()`
  - [ ] `SoilCell.ActionWaterSoil()`
  - [ ] `SoilCell.ActionPlantCrop()`
  - [ ] `CropInstance.ActionHarvest()`
  - [ ] `AncestralAltar.Interact()`
  - [ ] `KitchenHearth.Interact()`
  - [ ] `Coracle.EnterBoat()` / exit flow nếu có API scene gọi
- [ ] Cập nhật `CurrentTargetSoilCell` trong runtime để F1 farming debug hoạt động.

### Unity scene validation

- [ ] Mở Unity và xác nhận Console không còn compile errors.
- [ ] Kiểm tra `_Managers` có đủ `GameManager`, `WeatherManager`, `StorageManager`, `CommunityManager`, `PlayerStats`, `AudioManager`, `TutorialManager`, `EndingManager`.
- [ ] Kiểm tra `SurvivalUIManager` có đủ references: sliders, text, dialogue panel, inventory panel, itemSlotPrefab, banner nếu dùng.
- [ ] Kiểm tra `WeatherManager.waterPlanePrefab` và `disasterContainer`.
- [ ] Kiểm tra `StorageManager` có item data ban đầu hoặc item assets nằm trong `Resources` nếu dùng `GetItemDataByID()` fallback.

### Core gameplay smoke test

- [ ] New Game / Start Journey vào gameplay.
- [ ] Di chuyển quanh nhà và ruộng.
- [ ] Dọn đá một ô đất.
- [ ] Gieo hạt một ô đất.
- [ ] Tưới nước một ô đất.
- [ ] Advance day/debug grow và thu hoạch.
- [ ] Mở inventory và dùng/craft item.
- [ ] Tương tác Altar để chuyển `MuaBao`.

---

## 3. P1 — Needed for Storm/Flood Presentation

### Evacuation

- [ ] Thay `SurvivalUIManager.StartEvacuationCountdown()` toast-only bằng panel/timer thật nếu demo cần phần chạy lũ.
- [ ] Hiển thị rescued count `rescuedCount/4` liên tục.
- [ ] Test `TryEvacuateNPC()` từ proximity options hoặc interaction key.
- [ ] Hoàn thiện `ActivateNPCsOnRoof()` hoặc đổi thành cutscene/toast nếu không đủ thời gian.
- [ ] Quyết định failure flow: toast-only hay Game Over panel thật.

### Community / Vần công

- [ ] Wire `CommunityManager.TriggerStormHelpSequence()` vào thời điểm chuyển `MuaBao` hoặc `ChuanBiBao` nếu Vần công phải có tác dụng.
- [ ] Kiểm tra event flags:
  - [ ] `eventOThamFoodCompleted`
  - [ ] `eventBacNamStormCompleted`
  - [ ] `eventVillageRecoveryCompleted`
- [ ] Đảm bảo NPC quest marker không báo sai phase.

### Flood / recovery props

- [ ] Kiểm tra `FloodBarrier` prefab có component và radius phù hợp.
- [ ] Kiểm tra inventory placement của `item_sandbag` / `item_flood_board` có spawn prefab đúng đường dẫn Resources:
  - [ ] `Resources/Prefabs/Sandbag`
  - [ ] `Resources/Prefabs/FloodBoard`
- [ ] Nếu muốn wall repair/roof sandbag node: cần viết script node riêng; source hiện chưa có hệ thống hoàn chỉnh.

---

## 4. P2 — Polish / Optional

- [ ] Ending illustrations cho Sad/Normal/Best.
- [ ] Thêm thoại địa phương miền Trung cho 4 NPC.
- [ ] Audio ambience: mưa, gió, sấm, loa phát thanh.
- [ ] Cải thiện `WeatherManager`: particles, skybox, ambient transitions.
- [ ] Nón Lá: wire `isWearingNonLa` vào heat stress reduction.
- [ ] Save/load: hoặc mở rộng lưu inventory/soil/crops/NPC/tutorial, hoặc ghi rõ UI là save tạm.
- [ ] Unify folders `Interaction` và `Interactions` để giảm nhầm lẫn.

---

## 5. Experimental / Scope Decision

### Cockfighting

- [ ] Quyết định giữ hay bỏ `CockfightingZone` / `CockfightingMinigame` khỏi demo.
- Nếu giữ:
  - [ ] Reframe thành trò chơi dân gian/hoạt động tinh thần, tránh làm lệch core survival message.
  - [ ] Kiểm tra text/UI không gây phản cảm trong bài demo.
- Nếu bỏ:
  - [ ] Không đặt zone trong scene.
  - [ ] Không nhắc trong gameplay docs chính.

---

## 6. Do Not Overbuild

- [ ] Không thêm nhiều loại cây mới trước khi khoai lang loop chạy chắc.
- [ ] Không làm quest graph phức tạp.
- [ ] Không làm economy phức tạp.
- [ ] Không làm save/load đa tầng nếu chưa cần cho demo.
- [ ] Không mở thêm minigame ngoài farming/community/weather.
