# Project Overview: Đất Cày Lên Sỏi Đá

Cập nhật: **2026-06-27**. Tài liệu này mô tả hướng đi và phạm vi hiện tại của source Unity sau các lần merge gần đây.

---

## 1. Academic Context & Project Goal

- **Bối cảnh:** Demo môn PRU213, phát triển bằng Unity 6.
- **Mục tiêu:** Tạo playable demo ngắn về sinh tồn cộng đồng miền Trung: làm nông, tích trữ lương thực, giúp dân làng vượt thiên tai, tái thiết sau lũ.
- **Điểm nhấn:** Bản sắc văn hóa Việt Nam trong gameplay: Nghĩa Tình, Vần công, loa phát thanh xã, bão lũ, bàn thờ tổ tiên, khoai gieo khô.

---

## 2. High Concept & Genre

- **Tên game:** Đất Cày Lên Sỏi Đá / Sown in Stone
- **Thể loại:** Narrative community survival game with farming elements.
- **Vai trò người chơi:** Thành trở về quê miền Trung, cải tạo ruộng vườn và giúp bà con chống bão lũ.
- **Core fantasy:** Không phải làm nông để làm giàu, mà làm nông để tích cốc phòng cơ và giữ làng qua thiên tai.

---

## 3. Current Source Reality

Source hiện tại đã có nhiều hệ thống gameplay và UI, nhưng một số phần đang ở trạng thái **prototype/compatibility** sau merge:

### Đã có trong source

- `GameManager`: ngày/giờ, enum phase runtime, event `OnDayChanged` / `OnPhaseChanged`, trigger bão qua `TriggerStormCrisis()`.
- `WeatherManager`: `WeatherType`, nhiệt độ/độ ẩm/mưa/gió/mực nước lũ, lerp theo phase, spawn water plane nếu có prefab.
- `SoilCell` + `CropInstance`: đất, độ ẩm, dinh dưỡng, sỏi đá, trồng cây, tăng trưởng theo ngày, úng/héo, thu hoạch.
- `StorageManager` + `ItemData`: kho đồ runtime, add/remove item, decay nông sản tươi theo độ ẩm, chế biến đồ khô.
- `CommunityManager` + `NPCCharacter`: Nghĩa Tình, Vần công, NPC theo loại nhân vật, thoại fallback theo phase, affection.
- `SurvivalUIManager`: HUD, inventory, dialogue, choice buttons, toast, shop, weather/community panels, overlays.
- `TutorialManager`: gate nói chuyện 4 NPC, checklist tutorial, callbacks dọn đá/gieo hạt/tưới nước.
- Interactions: `AncestralAltar`, `KitchenHearth`, `Coracle`, `MudPuddle`, `FloodBarrier`.
- Experimental: `CockfightingZone` / `CockfightingMinigame` đã tồn tại nhưng chưa thuộc core scope.

### Đang thiếu hoặc chưa hoàn chỉnh

- `PlayerController` hiện tại chủ yếu là flow sơ tán và compatibility wrappers; phần di chuyển/farming scan đầy đủ không còn nằm trong file này ở trạng thái hiện tại.
- `WeatherManager` là bản tối giản: chưa có rain particle/ambient đầy đủ, chưa gắn world damage zones.
- Save/load chỉ lưu một phần nhỏ qua menu; chưa lưu kho đồ, đất/cây, NPC, tutorial, flood barriers.
- Các node roof refuge / wall repair / sandbag placement được docs cũ nhắc nhiều, nhưng source hiện tại chưa có hệ thống tương tác node hoàn chỉnh.
- `CommunityManager.TriggerStormHelpSequence()` có logic nhưng chưa thấy wiring trực tiếp từ `GameManager` phase transition.
- Tutorial slideshow trong `SurvivalUIManager` hiện là fallback toast/callback, chưa phải slideshow ảnh đầy đủ.

---

## 4. Runtime Phase Model

Code hiện dùng enum chi tiết:

- `LapNghiep`
- `GioLao`
- `ChuanBiBao`
- `MuaBao`
- `PhuSa`
- `EndGame`

Thiết kế có thể gom lại thành 2 phase lớn để thuyết trình:

- **Before the Storm:** `LapNghiep`, `GioLao`, `ChuanBiBao`
- **Storm / After the Storm:** `MuaBao`, `PhuSa`

Lưu ý: `GameManager.Update()` hiện chỉ tự chuyển `LapNghiep -> ChuanBiBao` vào ngày 3. `GioLao` tồn tại trong enum và UI/weather, nhưng chưa có lịch tự động trong `GameManager` hiện tại.

---

## 5. Scope Guidelines

Nên tập trung sửa cho demo chạy ổn thay vì mở rộng:

- Ưu tiên compile sạch và scene mở được trong Unity.
- Ưu tiên khôi phục `PlayerController` movement/farming interaction nếu đang mất do merge.
- Giữ farming chỉ quanh khoai lang/hạt giống/khoai tươi/khoai khô.
- Không mở rộng combat/minigame ngoài scope; cockfighting đang là experimental, không đưa vào core demo nếu chưa có quyết định.
- Không xây save/load phức tạp trước khi core loop chạy lại đầy đủ.

---

## 6. Recommended Demo Narrative

1. Thành về quê, gặp dân làng.
2. Dọn ruộng, gieo khoai, tưới nước, thu hoạch.
3. Chế biến/tích trữ lương thực.
4. Thắp nhang cầu an, bão/lũ kích hoạt.
5. Sơ tán dân làng hoặc trú ẩn.
6. Lũ rút, đất phù sa cải thiện ruộng.
7. Kết cục dựa trên Nghĩa Tình.
