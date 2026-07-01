# TODO: Development Status & Remaining Tasks

Cập nhật: **2026-07-01**.

---

## 1. Current Status

- [x] C# compile hoàn toàn sạch lỗi (0 errors).
- [x] Weather/GamePhase/Storage/UI/NPC compatibility APIs hoạt động ổn định.
- [x] Khôi phục 100% di chuyển WASD camera-relative, tương tác ruộng đất SoilCell và bếp gas KitchenHearth.
- [x] Bàn giao và triển khai công việc của 4 thành viên phát triển.
- [x] Phát triển hoàn chỉnh chuỗi nhiệm vụ hướng dẫn chuẩn bị bão (Tutorial Expansion).

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
- [x] Giai đoạn **EvacuateNeighbors**: Đếm ngược 120 giây sơ tán 4 dân làng lên nóc nhà Thành khi nước lũ dâng cao. Có timer và rescued count HUD. Logic đếm ngược chạy lũ 45 giây và di chuyển lên nóc nhà khi hoàn thành cứu hộ.
- [x] Giai đoạn **RoofSurvivalSharing**: Sinh tồn và chia sẻ khoai gieo cho 4 dân làng trên nóc nhà. Player di chuyển tự do (isOnRoof clamp đã được xóa).
- [x] Giai đoạn **PostStormCleanup**: Nước lũ rút, dọn dẹp nhà cửa và trồng lại 4 luống cây tái thiết.
- [x] `TempRoofCollider` phẳng 25×25m trên mái nhà Thành (tài sản collider tạm runtime, tự hủy khi ra khỏi giai đoạn).
- [x] Khóa góc camera đứng yên khi mở Tab xem thông tin hành trang và mở lại khi đóng.
- [x] Tự động hồi phục +5 thể lực mỗi giây khi người chơi đứng yên tại chỗ.
- [x] Sửa lỗi khóa cứng con trỏ chuột khi hiển thị các bảng UI (hướng dẫn, túi đồ, cửa hàng, kết thúc game).
- [x] **[2026-07-01]** Khắc phục triệt để lỗi player bị kẹt không đi qua được phía Bác Năm/Bé Tí trên nóc nhà (xóa isOnRoof clamp cứng X/Z trong `FixedUpdate`).
- [x] **[2026-07-01]** Disable `CharacterController` khi vào giai đoạn nóc nhà, restore khi xuống đất.
- [x] **[2026-07-01]** Fix `NullReferenceException` spam trong `NPCProximityOptionsUI.HandleKeyboardInput()`.

### NPC & Community
- [x] Khắc phục triệt để lỗi NPC tự xoay khi đi ngang qua và xoay lệch tâm (mesh visual được CenterVisualModel chuẩn hóa tại Runtime, đóng băng Rigidbody).
- [x] NPC slerp xoay mặt hướng chính xác về phía Player khi bắt đầu hội thoại.
- [x] Khóa các option tương tác phụ lúc đầu game, chỉ giữ duy nhất option "[1] Trò chuyện" để đi đúng cốt truyện.
- [x] Thiết lập chuỗi hội thoại NPC chi tiết theo từng mốc tiến độ hướng dẫn.

### Farming & Weather
- [x] Chỉnh sửa thời gian sinh trưởng của cây khoai lang thành 15 giây game-wide (áp dụng cho toàn game).
- [x] Cây trồng chỉ bắt đầu sinh trưởng sau khi ruộng đất được tưới nước ẩm (nhiệm vụ trồng trọt yêu cầu tưới nước).
- [x] Bắn thông báo Toast cảnh báo mất nông sản lên màn hình khi thời tiết ẩm nồm gây thối mốc thực phẩm.

### Storm / Tutorial Expansion
- [x] Hướng dẫn sấy khoai gieo khô tại bếp gas (hiển thị dấu chấm than và chữ "Bếp Gas" dẫn đường).
- [x] Gặp và tặng khoai gieo cho 4 dân làng. Hội thoại Bé Tí lo sợ về cơn bão kích hoạt tiếng Loa Phát Thanh Xã khẩn cấp và còi hú cảnh báo thiên tai.
- [x] Tuyến nhiệm vụ gia cố trước bão: O Thắm tặng 4 tấm chắn lũ dựng trước cửa đại lý để chống ngập, Bác Năm tặng 4 bao cát đặt trực tiếp lên mái nhà tranh để chằng chống. Cả hai nhiệm vụ đều sử dụng mô hình bóng ma trong suốt màu xanh dương (alpha = 0.35) tự động quét trong Scene, bắt khớp 2D XZ mượt mà với dung sai 3.5m, khôi phục vật liệu gốc khi hoàn tất và khóa option đối thoại của NPC.
- [x] Nhiệm vụ chuẩn bị bao cát bảo vệ nhà mình: tự động nhận 4 bao cát đặt trước cửa nhà mình (hiển thị dấu chấm than "Nhà Của Bạn").
- [x] Cụ Bảy tặng 1 Nén Nhang cúng tế và hiển thị Toast "Bạn nhận được 1 Nén Nhang từ Cụ Bảy".
- [x] Thắp nhang ban thờ gia tiên để hoàn thành hướng dẫn và kích hoạt chuyển cảnh bão về (`GamePhase.MuaBao`).

---

## 3. P1 — Needed for Storm/Flood Presentation

### Evacuation
- [x] Giai đoạn `EvacuateNeighbors` đã hoàn chỉnh với timer, rescued count HUD và failure flow (Toast + reset).
- [x] `TryEvacuateNPC()` từ proximity options hoạt động đúng các NPC: O Thắm, Bác Năm, Cụ Bảy, Bé Tí.
- [x] `ActivateNPCsOnRoof()` đã hoàn chỉnh: teleport NPC lên mái, kích hoạt visual, chuyển collider thành trigger.
- [x] Failure flow: Toast + reset về vị trí ban đầu nếu hết giờ.

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
