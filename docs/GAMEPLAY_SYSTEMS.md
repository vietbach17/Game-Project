# Gameplay Systems Document: Đất Cày Lên Sỏi Đá

Tài liệu này mô tả chi tiết các hệ thống gameplay hiện đang được triển khai trong dự án. Tất cả các hệ thống đã được implement và hoạt động trong `Village_Demo.unity` tại thời điểm cập nhật tài liệu này.

---

## 1. Player Movement & Camera System

### Di chuyển (`PlayerController.cs`)
- **Input**: Unity New Input System (WASD / Arrow keys, Left Shift để chạy).
- **Physics**: `Rigidbody` với `useGravity=false`, `FreezePositionY`, `FreezeRotationXYZ`.
- **Camera-relative movement**: Vector di chuyển được tính dựa trên `Camera.main.forward` và `Camera.main.right` chiếu lên mặt phẳng XZ, đảm bảo Thành luôn di chuyển theo hướng camera nhìn.
- **Xoay nhân vật**: `characterVisual` Slerp mượt mà theo hướng di chuyển (Y-axis only).
- **Flood speed penalty**: Giảm 40% tốc độ khi FloodLevel > 0.5f (khi ngập lụt ở Phase 2).
- **Action lock**: `isPerformingAction` khóa Rigidbody velocity trong khi thực hiện hành động farming.

### Camera Roblox-style (`CameraFollow3D.cs`)
- **Orbit**: Xoay quanh pivot Player (pivotHeight = 1.35m) bằng `Vector3.SmoothDamp`.
- **RMB Drag**: Giữ chuột phải để xoay Yaw + Pitch.
- **Pitch clamp**: 12° – 55°, mặc định 25°.
- **Scroll Zoom**: Distance 3–9 đơn vị, Lerp mượt mà.
- **SphereCast Collision**: Tự động thu ngắn khoảng cách khi có vật cản, bỏ qua layer Player.

---

## 2. Interaction System

### Phím [E] / [Space]
- `PlayerController.TryPerformInteraction()` quét `Physics.OverlapSphere(radius=2.5f)`.
- **Ưu tiên xử lý**: NPCCharacter (chỉ đóng dialogue, không mở) → AncestralAltar → IInteractable (bếp ga) → Coracle → MudPuddle → SoilCell.
- **NPC interaction**: Chỉ qua Proximity Panel (không qua [E] trực tiếp).

### NPC Proximity Panel (`NPCProximityOptionsUI.cs`)
- Phát hiện player < 1.7m, mở panel nổi bám WorldToScreenPoint.
- Screen bounds clamping để tránh panel thoát khỏi màn hình.
- CanvasGroup fade in/out 0.13s.
- Phím tắt `[1]` / `[2]` / `[3]` + mouse click.
- NPCCharacter.LookAtPlayer() khi tương tác, ReturnToDefaultRotation() khi đi xa.

### Quest Marker (`NPCQuestMarkerUI.cs`)
- Dấu `!` vàng bounce (sin-wave) trên NPC có event chưa hoàn thành.
- Ẩn khi player < 1.7m (để nhường chỗ Proximity Panel).
- Ẩn khi dialogue/shop đang mở.

---

## 3. Tutorial System (`TutorialManager.cs`)

Hệ thống tutorial **2 giai đoạn** với HUD quest tracker:

### Giai đoạn 1: `IntroQuests`
- **Nhiệm vụ**: Gặp và trò chuyện với TẤT CẢ 4 NPC: O Thắm (taskA), Bác Năm (taskB), Cụ Bảy (taskC), và Bé Tí (taskD).
- `TutorialManager.RegisterTalkStart(speakerName)` → `OnDialogueClosed(speakerName)` tracking.
- Khi cả 4 hoàn thành → chuyển sang `ShowingFarmingSlides`.

### Slideshow Bridge: `ShowingFarmingSlides`
- Dừng thời gian (`Time.timeScale = 0f`).
- Hiển thị các slides hướng dẫn dọn đá, gieo hạt, tưới nước với ảnh PNG từ Resources.
- Nút "Quay Lại" / "Tiếp Theo" / "Bỏ Qua".
- Xong → `Time.timeScale = 1f`, chuyển sang `FarmingTutorial`.

### Giai đoạn 2: `FarmingTutorial`
- **Nhiệm vụ**: Dọn đá (subTask1) + Gieo hạt (subTask2) + Tưới nước (subTask3).
- Callbacks: `SoilCell.ActionClearRocks()` → `OnRockCleared()`, `SoilCell.ActionPlantCrop()` → `OnCropPlanted()`, `SoilCell.ActionWaterSoil()` → `OnSoilWatered()`.
- Fallback: Scan `CropInstance[]` mỗi frame, nếu có cây đang lớn (growthDays > 0.01f) → hoàn thành tutorial.
- Hoàn thành → `CompleteTutorial()` → Toast + `GameManager.TransitionToPhase(Phase1_BeforeTheStorm)`.

### `TutorialQuestPanel` HUD
- Tạo động bởi TutorialManager, Top-Left `(20, -220)`, size `280x150`.
- Checkbox `☐` / `✓` theo real-time cho cả 4 NPC ở Giai đoạn 1 và các hoạt động farming ở Giai đoạn 2.
- Ẩn khi tutorial hoàn thành.

---

## 4. Farming System

### Lưới ô đất (Farming Grid)
- Lưới trồng trọt được cấu hình theo **lưới 4x3 (tổng cộng 12 SoilCell)**.
- Mỗi ô đất đại diện cho một thực thể `SoilCell` độc lập.

### SoilCell State Machine
| State | Điều kiện | Visual |
|-------|-----------|--------|
| Rocky | RockDensity > 0 | `Soil_Rocky` prefab |
| Wet | RockDensity=0, Moisture ≥ 35% hoặc PhuSa | `Soil_Wet` prefab |
| Tilled | RockDensity=0, Moisture < 35%, có cây | `Soil_Tilled` prefab |
| Clean | RockDensity=0, Moisture < 35%, không cây | `Soil_Clean` prefab |

### Vòng lặp canh tác
1. **Dọn đá** (`ActionClearRocks`): Tốn Stamina (8f × số ô). Giảm RockDensity về 0. Quality → TrungBinh.
2. **Tưới nước** (`ActionWaterSoil`): Moisture += amount. Tự bốc hơi theo thời tiết.
3. **Gieo hạt** (Bulk/Single dialog): Tốn 1 `Item_Seed` mỗi ô. Tạo `CropInstance` con.
4. **Sinh trưởng**: `CropInstance` tích lũy `currentGrowthDays` mỗi ngày. Đổi visual Stage 1 → Stage 2.
5. **Thu hoạch** (`ActionHarvest`): Nhận `Item_FreshCrop`. Sản lượng ×2 nếu đất có chất lượng Phù Sa (PhuSa) hoặc trong giai đoạn tái thiết.

### Smart Bulk Planting
- Dialog 3 nút: "Trồng hàng loạt (N ô)" / "Chỉ trồng ô này" / "Hủy".
- N = min(hạt còn lại, số ô có thể trồng gần player).

### SoilCell Parent-Child Architecture
- 1 parent cell (tên không chứa "Grid") tự động link với các child cells (tên chứa "Grid") trong `Awake()` qua khoảng cách XZ < 6m.
- `IsParentField = true` khi childCells.Count > 0.
- Hành động farming trên parent cell tự động lan sang tất cả child cells.

### Soil Highlight
- `SetHighlight(bool)`: Spawn khung viền vàng gold 4 Cube dẹt (3D mode) hoặc SpriteRenderer overlay (2D mode).
- Tự cập nhật theo state đất.

### Phù Sa (Phase 2 - Sau lũ)
- Đất Phù Sa tự động kích hoạt trong **Phase 2 (After the Storm)** làm trạng thái phục hồi sau lũ: quality = PhuSa, Nutrients = 100, RockDensity = 0.
- Thu hoạch trên đất Phù Sa → yield ×2.

---

## 5. Nghĩa Tình System

### Tích điểm (CommunityManager)
| Hành động | Điểm |
|-----------|------|
| Trò chuyện với O Thắm / Bác Năm / Cụ Bảy / Bé Tí | +5 (1 lần/session) |
| Giúp việc Vần công | +10 (tiêu 20 Stamina) |
| Chia sẻ lương thực cho Bác Năm | +15 |
| Event Phase 1 – Hỗ trợ O Thắm chuẩn bị trước bão | +10 |
| Event Phase 2 – Cứu trợ khẩn cấp dân làng | +20 |
| Event Phase 2 – Tái thiết ruộng vườn sau lũ | +20 |

### Kết cục
| Nghĩa Tình | Kết cục |
|-----------|---------|
| ≥ 80 | **Best**: Đất Cày Nở Hoa |
| ≥ 40 | **Normal**: Lá Lành Đùm Lá Rách |
| < 40 | **Sad**: Đất Sỏi Đá Cằn |

### HUD Display (NghiaTinhUI.cs)
- Slider fill đổi màu: 0–30 đỏ, 31–70 vàng, 71–100 xanh.
- Panel Top-Left `(20, -100)` trong SurvivalUI.

---

## 6. Weather & Disaster System

Hệ thống thời tiết và thiên tai được chia làm **2 giai đoạn chính**:

### Phase 1: Before the Storm (Trước bão)
- **Thời tiết**: Ôn hòa, sau đó chuyển dần sang nhiều mây, mưa nhẹ rải rác báo hiệu bão về.
- **Mục tiêu**: Chuẩn bị lương thực, dọn dẹp ruộng, tích lũy công Vần công, gia cố nhà cửa.

### Phase 2: After the Storm (Sau bão)
- **Thời tiết**: Bão lũ ập tới với mưa lớn cuồng phong (`RainIntensity` cực đại), sấm sét, mực nước lũ (`FloodLevel`) dâng cao gây ngập úng ruộng vườn.
- **Flood Roof Survival**: Khi nước ngập sâu, player phải leo lên nóc nhà lá tránh lũ, nhận mì tôm cứu trợ, nghỉ ngơi phục hồi sức lực.
- **Sự phục hồi**: Nước rút dần, để lại lớp đất Phù Sa màu mỡ giúp dọn sạch đá, nâng cao dưỡng chất đất và nhân đôi sản lượng thu hoạch nông sản vụ sau.

---

## 7. NPC Interaction System

### O Thắm (OTham)
| Lựa chọn | Điều kiện | Phần thưởng |
|----------|-----------|-------------|
| Trò chuyện | 1 lần/session | +5 Nghĩa Tình |
| Giúp việc Vần công | ≥ 20 Stamina | -20 Stamina, +1 VanCong, +10 Nghĩa Tình |
| Chuẩn bị trước bão (Phase 1) | Giao nộp nhu yếu phẩm | +10 Nghĩa Tình |
| Giao dịch (Shop) | Bình thường | Mở ShopUI |
| Tái thiết ruộng sau lũ (Phase 2) | Chia sẻ hạt giống/khoai | +20 Nghĩa Tình |

### Bác Năm (BacNam)
| Lựa chọn | Điều kiện | Phần thưởng |
|----------|-----------|-------------|
| Trò chuyện | 1 lần/session | +5 Nghĩa Tình |
| Giúp việc Vần công | ≥ 20 Stamina | -20 Stamina, +1 VanCong, +10 Nghĩa Tình |
| Chia sẻ lương thực | Khoai tươi, Khoai gieo khô, hoặc Mì tôm | +15 Nghĩa Tình |
| Gia cố nhà chống bão (Phase 1) | 1 VanCong | -1 VanCong, +15 Nghĩa Tình |
| Tái thiết sau lũ (Phase 2) | Chia sẻ nông sản | +20 Nghĩa Tình |

### Cụ Bảy (CuBay)
| Lựa chọn | Điều kiện | Phần thưởng |
|----------|-----------|-------------|
| Trò chuyện | 1 lần/session | Thoại khuyên nhủ, giữ tinh thần làng xã |
| Cứu trợ lũ lụt (Phase 2) | Quyên góp lương thực chống đói | +20 Nghĩa Tình |

### Bé Tí (BeTi)
- Hỗ trợ truyền tin, cảnh báo thời tiết và các đoạn thoại ngắn thay đổi theo tình trạng bão lũ.

---

## 8. Special Interactions

### Bàn thờ tổ tiên (`AncestralAltar.cs`)
- [E] khi đứng gần: Kiểm tra có `Item_Incense` (Nhang) trong kho.
- Nếu có: Tiêu 1 Nhang → Morale += 10 (clamp 100) + toast báo hiệu lòng thành kính.
- Nếu không: Thông báo "Không có nhang cúng".

### Bếp ga (`KitchenHearth.cs` — IInteractable)
- [E] khi đứng gần: Mở dialog 2 lựa chọn:
  - **Sấy khô khoai**: 2 `Item_FreshCrop` → 1 `Item_PreservedCrop` (Khoai gieo khô - không thối rữa dưới độ ẩm cao).
  - **Nấu ăn**: 1 `Item_FreshCrop` → +15 Stamina.
- Nếu không đủ nguyên liệu: Cảnh báo đỏ.

### Thuyền thúng (`Coracle.cs`)
- [E] để lên/xuống thuyền. Giúp di chuyển linh hoạt trên mặt nước lũ khi ngập lụt ở Phase 2 (tốc độ 2.5f).

### Vũng bùn (`MudPuddle.cs`)
- Tiếp xúc làm giảm tốc độ di chuyển của nhân vật do sình lầy trơn trượt.

---

## 9. Storage & Items

### StorageManager
- `AddItem(item, quantity)` / `RemoveItem(item, quantity)` / `GetStorageSlots()`.
- **Hư hỏng nông sản**: `Item_FreshCrop` (Khoai tươi) có tỷ lệ thối rữa tự động dưới độ ẩm cao trong bão lũ (Phase 2). Cần sấy khô thành `Item_PreservedCrop` để dự trữ an toàn.

### Item Types
| ItemID | Tên | Dùng để |
|--------|-----|---------|
| item_seed | Hạt Giống | Gieo trồng trên SoilCell (1 seed/ô) |
| item_fresh_crop | Khoai Lang Tươi | Ăn hồi thể lực, quyên góp, sấy khô |
| item_preserved_crop | Khoai Gieo Khô | Dự trữ lâu dài chống ngập úng, quyên góp |
| item_incense | Nhang Cúng | Thắp bàn thờ tổ tiên hồi phục Morale |
| item_noodles | Mì Tôm Cứu Trợ | Lương thực khẩn cấp phát trên nóc nhà |
| item_sandbag | Bao Cát | Gia cố đê, chắn lũ (`FloodBarrier`) |
| item_flood_board | Tấm Chắn Lũ | Ngăn chặn nước tràn vào khu sinh hoạt |
| item_non_la | Nón Lá | Giảm stress thời tiết khi làm việc ngoài trời |

---

## 10. UI/HUD System Overview

### Toast Notifications
- `ShowHUDToast(message)` hiển thị nhanh góc màn hình với màu sắc đặc trưng:
  - `+X Nghĩa Tình` → vàng `#F4D03F`
  - `+X Vần công` → xanh lá `#2ECC71`
  - `Không đủ...` → đỏ `#E74C3C`

### Loa Phát Thanh Xã (VillageSpeakerBanner)
- Tự động kích hoạt khi bước sang ngày mới hoặc chuyển giai đoạn (Phase change).
- Nội dung tiếng Việt phát tin thời tiết khẩn cấp, cập nhật tình hình bão lũ và khích lệ bà con xã viên.

### F1 Debug Panel (FrameworkDebugUI)
- Hỗ trợ nhảy nhanh Phase 1/Phase 2, thêm hạt giống, khoai tươi, thay đổi điểm Nghĩa Tình và các công cụ debug farming nhanh chóng.

---

## 11. Phase System

| Phase | Enum | Ngày | Sự kiện chính |
|-------|------|------|--------------|
| Trước Bão | `BeforeTheStorm` | 1–4 | Trải nghiệm yên bình, làm quen dân làng, trồng trọt chuẩn bị đối phó bão. |
| Sau Bão | `After the Storm` | 5+ | Lũ lụt đổ bộ, sinh tồn trên nóc nhà, nước rút bồi đắp phù sa, tái thiết và tổng kết kết cục game. |

---

## 12. Ending System

- `GameManager` gọi `EndingManager.ShowEnding()` khi kết thúc chu kỳ cốt truyện ở Phase 2.
- Đánh giá kết cục dựa trên điểm Nghĩa Tình tích lũy (`GlobalKarma`):
  - **Best Ending (≥ 80)**: Đất Cày Nở Hoa
  - **Normal Ending (≥ 40)**: Lá Lành Đùm Lá Rách
  - **Sad Ending (< 40)**: Đất Sỏi Đá Cằn
- Resource Panel tự động ẩn khi Ending hiển thị. Người chơi có thể chọn "Chơi lại" hoặc "Thoát".
