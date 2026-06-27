# Current Progress: Đất Cày Lên Sỏi Đá

Cập nhật: **2026-06-27**.

Tài liệu này phản ánh trạng thái source hiện tại sau merge/compile fix gần nhất. Mức độ sẵn sàng demo hiện nên đánh giá thận trọng vì một số hệ thống cốt lõi đang ở dạng compatibility/prototype.

---

## 1. Current Game Direction

- **Định hướng:** Community survival farming demo miền Trung.
- **Core loop mong muốn:** gặp dân làng → cải tạo ruộng/khoai → tích trữ lương thực → thắp nhang kích bão → sơ tán/trú ẩn → lũ rút/phù sa → tái thiết/kết cục Nghĩa Tình.
- **Phạm vi nên giữ:** farming + storage + NPC/community + weather/flood + ending. Không mở rộng minigame phụ nếu chưa cần.

---

## 2. Systems Confirmed in Source

### Core / Runtime

- `GameManager` có day/time, phase enum, event day/phase, storm trigger.
- `WeatherManager` có weather stats và flood level lerp theo phase.
- `PlayerStats` có health/stamina/morale/coins và heat/cold stress.

### Farming / Storage

- `SoilCell` có clear/water/fertilize/plant APIs, moisture/nutrients/rock density, PhuSa conversion.
- `CropInstance` có growth by day, harvest, wither/rot checks.
- `StorageManager` có runtime inventory, decay, item lookup, preserved crafting.
- `KitchenHearth` có sấy khô/nấu ăn qua dialogue choices.

### Community / NPC

- `CommunityManager` có GlobalKarma, event flags, Vần công credit aggregation.
- `NPCCharacter` có 4 nhân vật chính, fallback dialogue theo phase, affection/Vần công.
- `NPCProximityOptionsUI` và `NPCQuestMarkerUI` tồn tại.

### UI / Tools

- `SurvivalUIManager` có HUD, inventory, shop, dialogue, choices, toast, overlays, community/weather panels.
- `TutorialManager` có IntroQuests + FarmingTutorial checklist callbacks.
- `EndingManager`, `FrameworkMainMenuUI`, `FrameworkDebugUI` tồn tại.
- `CockfightingZone` / `CockfightingMinigame` tồn tại nhưng đang là experimental.

---

## 3. Current Readiness Assessment

**Demo readiness hiện tại: khoảng 60–70% về mặt source**, không nên ghi 90%+ cho đến khi kiểm tra Play Mode trong Unity.

Lý do:

- C# compile đã sạch sau fix gần nhất.
- Nhiều hệ thống gameplay/data/UI đã có nền tảng.
- Nhưng `PlayerController` hiện tại không còn loop di chuyển/farming interaction đầy đủ trong source được đọc.
- Một số docs cũ mô tả EvacuationTimerPanel, roof nodes, wall repair, slideshow, flood/rain effects như đã hoàn thiện; source hiện tại mới có wrapper/toast hoặc chưa có wiring đầy đủ.

---

## 4. What Works at Code Level

- Compile C# không còn lỗi `CS...` trong log gần nhất.
- Phase transition có thể phát event cho weather/UI.
- Altar có thể consume incense, restore morale, trigger storm crisis.
- Storage có thể thêm/xóa item, decay fresh crops, craft preserved crops.
- Soil/crop APIs có thể được gọi để tạo loop farming nếu caller đúng.
- UI có thể mở inventory/shop/dialogue/toast và tạo nhiều panel runtime.
- NPC dialogue/community options có logic nền.

---

## 5. What Must Be Verified in Unity Play Mode

1. Player có thật sự di chuyển được không.
2. Phím tương tác farming có gọi `SoilCell` actions không.
3. Inventory prefab/slot references có được gán trong scene không.
4. `SurvivalUIManager` có đủ serialized references để không bị null khi tạo layout không.
5. `WeatherManager.waterPlanePrefab` và `disasterContainer` có được gán không.
6. Altar có tìm được `item_incense` không, đặc biệt nếu item data không nằm trong Resources.
7. NPC proximity panel có xuất hiện và gọi đúng actions không.
8. Tutorial 4-NPC gate có match tên speaker thực tế không (`OTham`, `BacNam`, `CuBay`, `BeTi` vs tên tiếng Việt hiển thị).
9. Ending panel references có hợp lệ không.

---

## 6. Known Partial / Compatibility Areas

- `PlayerController`: nhiều method chỉ là wrapper để compile; cần khôi phục gameplay controller nếu thiếu.
- `TutorialManager.ShowTutorial(Action)`: hiện gọi callback ngay, không phải slideshow thật.
- `SurvivalUIManager.StartEvacuationCountdown()`: hiện toast-only.
- `SurvivalUIManager.ShowDrownGameOverPanel()`: hiện toast-only.
- `ActivateNPCsOnRoof()`: placeholder.
- `WeatherManager.UpdateSoilEvaporationParameters()`: rỗng.
- Nón Lá equip chưa giảm heat stress trong `PlayerStats`.
- Save/load chưa lưu inventory/world state.
- `TriggerStormHelpSequence()` chưa được wired từ phase transition.

---

## 7. Recommended Next Steps

### P0 — restore playable demo

1. Mở Unity và kiểm tra Console sau compile.
2. Test player movement. Nếu không di chuyển, khôi phục movement code vào `PlayerController` hoặc xác nhận script khác đang điều khiển player.
3. Test interaction với SoilCell: clear/water/plant/harvest.
4. Test inventory item data và `StorageManager.GetItemDataByID()`.
5. Test altar → `MuaBao` → weather/flood/UI.

### P1 — make storm sequence presentable

1. Làm countdown UI thật thay vì toast-only.
2. Hoàn thiện NPC evacuation hoặc cắt scope khỏi demo.
3. Hoàn thiện `ActivateNPCsOnRoof()` hoặc chuyển thành narrative toast đơn giản.
4. Wire `CommunityManager.TriggerStormHelpSequence()` nếu Vần công cần ảnh hưởng bão.

### P2 — polish

1. Ending illustrations.
2. More Central Vietnam dialogue.
3. Audio ambience.
4. Decide/remove cockfighting experimental scope.

---

## 8. Status Summary

Dự án hiện **compile được** và có nhiều module gameplay quan trọng, nhưng tài liệu/scene cần được kiểm chứng lại trong Unity vì source đang thể hiện một trạng thái sau merge: nhiều tính năng được giữ bằng compatibility wrappers, chưa chắc đã hoạt động như docs cũ mô tả.
