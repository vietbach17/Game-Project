# TODO: Development Status & Remaining Tasks

Cập nhật: **2026-06-27**. Checklist này dựa trên source hiện tại trong `Assets/Scripts`, không dựa trên mục tiêu thiết kế cũ.

---

## 1. Current Status

- [x] C# compile hoàn toàn sạch lỗi (0 errors).
- [x] Weather/GamePhase/Storage/UI/NPC compatibility APIs hoạt động ổn định.
- [x] Khôi phục 100% di chuyển WASD camera-relative, tương tác ruộng đất SoilCell và bếp gas KitchenHearth.
- [ ] Bàn giao và triển khai công việc của 4 thành viên phát triển.

---

## 2. P0 — Completed Tasks

### Player & interaction
- [x] Di chuyển mượt mà của nhân vật Thành trong `Village_Demo.unity` (WASD đồng bộ camera).
- [x] Tương tác [E] / [Space] tự động quét các vật thể gần nhất.
- [x] Tương tác với ruộng đất SoilCell (dọn đá, tưới nước, gieo hạt, thu hoạch) hoạt động ổn định.
- [x] Tương tác Bếp gas KitchenHearth (chế biến khoai khô, luộc khoai).
- [x] Thắp nhang bàn thờ Gia Tiên (AncestralAltar).
- [x] Di chuyển bằng Thuyền thúng (Coracle) và lội vũng bùn (MudPuddle).
- [x] Tự động nạp hạt giống khoai lang `Crop_KhoaiLang.asset` kiểm thử trong Editor.
- [x] HUD gợi ý tương tác và phím tắt được khôi phục.
- [x] Animator di chuyển Idle/Walk/Run hoạt động hoàn hảo.
- [x] Lội nước lũ làm giảm tốc độ di chuyển của Thành (flood movement penalty).
- [x] Đếm ngược chạy lũ 45 giây và logic di chuyển lên nóc nhà khi hoàn thành cứu hộ.

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
