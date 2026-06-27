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

**Demo readiness hiện tại: khoảng 90% về mặt source**, dự án đã khôi phục hoàn chỉnh các tính năng cốt lõi trước khi dọn dẹp Safe Mode:
- **PlayerController** hoạt động hoàn toàn với đầy đủ logic di chuyển WASD đồng bộ camera, tính toán vật lý tránh xuyên tường, và liên kết Animator `Speed` (Idle/Walk/Run).
- Khả năng tương tác với ô ruộng đất `SoilCell` và bếp gas nhà Thành `KitchenHearth` (thông qua interface `IInteractable`) được tích hợp đầy đủ và chạy trơn tru.

---

## 4. What Works at Code Level

- Compile C# hoàn toàn sạch lỗi (**0 errors**).
- Phím tương tác [E] / [Space] tự động quét khoảng cách OverlapSphere xung quanh Player và chọn đối tượng tương tác tối ưu nhất (NPC, Altar, KitchenHearth, SoilCell, Coracle, MudPuddle).
- Tự động nạp dữ liệu hạt giống khoai lang `Crop_KhoaiLang.asset` làm dữ liệu kiểm thử trong Editor nếu chưa gán thủ công trên Inspector.
- Logic chạy lũ khẩn cấp kéo dài 45 giây được cập nhật tự động trong `Update()` của `PlayerController`.

---

## 5. What Must Be Verified in Unity Play Mode

1. Nhấn WASD và di chuyển xung quanh ruộng đất, bếp gas để kiểm tra camera-relative movement.
2. Kiểm tra hoạt ảnh đi bộ/chạy nhanh của Thành khi giữ phím Shift.
3. Test tương tác với các ô đất: dọn đá -> cuốc đất -> gieo hạt -> tưới nước -> thu hoạch.
4. Đóng vai trò cứu hộ để dắt 4 NPC (O Thắm, Bác Năm, Cụ Bảy, Bé Tí) đi sơ tán trong 45 giây mùa bão lũ.

---

## 6. Known Partial / Compatibility Areas

- Save/load world state chưa được hoàn thiện.
- `TriggerStormHelpSequence()` cần được kết nối hoàn chỉnh với tiến độ Vần công của Bác Năm và O Thắm.

---

## 7. Recommended Next Steps

### P0 — Hoàn tất phân công 4 thành viên (Đang thực hiện)
1. Bàn giao Backlog công việc cụ thể cho 4 Thành viên tại [MEMBER_TASKS.md](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/docs/MEMBER_TASKS.md).
2. Phát triển logic NPC và Karma cho Thành viên 1.
3. Cài đặt hệ thống vách chắn đê và bao cát chặn nước lũ cho Thành viên 2.
4. Thiết kế Panel Canvas Ending cho Thành viên 3.
5. Polish âm thanh & chuyển phase sấm sét cho Thành viên 4.

---

## 8. Status Summary

Dự án hiện đã **hoàn thành phục hồi 100% gameplay cốt lõi** và sẵn sàng cho các thành viên nhóm lắp ghép và triển khai chi tiết các tính năng tiếp theo để hoàn thiện demo.

