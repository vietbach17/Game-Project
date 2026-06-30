# Current Scene State: Đất Cày Lên Sỏi Đá

Cập nhật: **2026-06-27**.

Tài liệu này là checklist trạng thái scene cần xác minh trong Unity. Do phiên hiện tại chỉ đọc source và chưa dùng được Unity MCP, mọi chi tiết scene dưới đây cần được kiểm tra lại trực tiếp trong Editor.

---

## 1. Scene Target

- Scene mục tiêu: `Assets/Scenes/Village_Demo.unity`.
- Nếu Unity đang mở scene khác, chuyển về `Village_Demo.unity` trước khi test demo.
- `SampleScene.unity` chỉ nên dùng làm sandbox.

---

## 2. What Source Expects in Scene

### `_Managers`

Cần có các singleton:

- `GameManager`
- `PlayerStats`
- `WeatherManager`
- `StorageManager`
- `CommunityManager`
- `AudioManager`
- `TutorialManager`
- `EndingManager`

### `_UI` / Canvas

Cần có `SurvivalUIManager` với references quan trọng:

- `CanvasGroup mainUICanvasGroup`
- health/stamina/morale sliders
- day/phase/weather text
- dialogue panel + speaker/content text
- inventory panel + item slot container + item slot prefab
- optional `VillageSpeakerBanner`

### Player

Cần có:

- `PlayerController`
- `Rigidbody`
- collider
- visual/model child

Quan trọng: source `PlayerController` hiện tại thiếu movement loop đầy đủ; nếu Play Mode player đứng yên, đây là P0 cần sửa code chứ không chỉ scene.

### FarmingArea

Cần có:

- các `SoilCell` gameplay cells, lý tưởng là 12 ô 4x3
- collider trigger trên từng ô
- references tới visual prefabs hoặc sprite renderers
- crop data được cấp cho caller trồng cây

### NPCs

Cần có 4 NPC nếu tutorial gate dùng đủ 4 người:

- O Thắm
- Bác Năm
- Cụ Bảy
- Bé Tí

Mỗi NPC cần `NPCCharacter.characterType` đúng và collider để proximity UI tìm được.

### InteractionZones / Props

Cần xác minh:

- Altar/Shrine có `AncestralAltar`
- bếp có `KitchenHearth`
- thuyền có `Coracle`
- vũng bùn có `MudPuddle` nếu dùng phase `PhuSa`
- prefab sandbag/floodboard có `FloodBarrier` nếu dùng chống lũ

### DisasterObjects

Cần xác minh:

- `WeatherManager.disasterContainer` trỏ tới container hợp lệ
- `WeatherManager.waterPlanePrefab` có prefab hợp lệ

---

## 3. Scene Features Claimed by Older Docs That Need Re-Verification

Các docs cũ từng ghi các mục này như đã có, nhưng source hiện tại chưa đủ để khẳng định:

- `EvacuationTimerPanel` thật ở top-center.
- Nón lá giảm heat stress.
- Save/load world state.
- Rain particles / skybox / ambient weather transitions.

Không nên demo các mục này cho đến khi kiểm tra trong Unity hoặc implement lại.

---

## 4. Quick Play Mode Smoke Test

Khi Unity mở được scene:

1. Nhấn Play.
2. Kiểm tra Console có runtime exception không.
3. Di chuyển player bằng WASD/Arrow.
4. Xoay/zoom camera nếu có `CameraFollow3D`.
5. Đến gần NPC, kiểm tra proximity panel.
6. Mở inventory bằng `I` hoặc `Tab`.
7. Đến ruộng, thử target/cải tạo/tưới/trồng một ô.
8. Dùng F1 debug nếu panel hoạt động để advance phase/grow crop.
9. Tương tác altar và xác nhận phase chuyển `MuaBao`.
10. Kiểm tra weather text/flood level/water plane.

---

## 5. Known Source-to-Scene Risks

- `SurvivalUIManager` tạo nhiều object runtime; nếu Start chạy nhiều lần hoặc scene có object cũ, có thể trùng UI.
- `StorageManager.GetItemDataByID()` chỉ tìm item trong kho hoặc `Resources`; nếu item asset không nằm trong Resources và chưa có trong kho, altar/noodles lookup có thể fail.
- `TutorialManager` tracking key dùng `OTham`, `BacNam`, `CuBay`, `BeTi`; `SurvivalUIManager.CloseDialogue()` truyền `speakerNameText.text`, thường là tiếng Việt như `O Thắm`, nên tutorial gate có thể không tick đúng nếu chưa map tên.
- `GameManager.TriggerStormCrisis()` không cho trigger khi đang `GioLao` dù `IsBeforeStorm` xem `GioLao` là trước bão. Nếu phase bị set `GioLao`, altar có thể không chuyển bão.
- `Interaction` và `Interactions` là hai folder/namespace khác nhau; khi gắn script trong scene cần chọn đúng class.

---

## 6. Recommended Scene Cleanup After Verification

- Remove or disable experimental cockfighting objects unless intentionally demoed.
- Ensure exactly one instance of each singleton manager.
- Ensure item data assets are reachable by runtime code.
- Ensure `PlayerController` has complete movement/interaction before polishing UI.
