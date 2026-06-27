# Gameplay Systems Document: Đất Cày Lên Sỏi Đá

Tài liệu này mô tả chi tiết các hệ thống gameplay chính thức sau khi tái cấu trúc. Tất cả các hệ thống đã được chỉnh sửa logic để vận hành đồng bộ theo mô hình 2 Phase chiến lược trong scene `Village_Demo.unity`.

---

## 1. Player Movement & Camera System

### Di chuyển (`PlayerController.cs`)
- **Input**: Unity New Input System (WASD / Arrow keys, Left Shift để chạy nhanh).
- **Physics**: `Rigidbody` với `useGravity=false`, `FreezePositionY`, `FreezeRotationXYZ`. Nhân vật di chuyển phẳng, rà sát mặt Terrain.
- **Camera-relative movement**: Vector di chuyển được tính dựa trên `Camera.main.forward` và `Camera.main.right` chiếu lên mặt phẳng XZ, đảm bảo Thành luôn di chuyển theo hướng camera nhìn.
- **Xoay nhân vật**: Mô hình hình ảnh (`characterVisual`) Slerp mượt mà theo hướng di chuyển thực tế (Y-axis only).
- **Action lock**: Cờ `isPerformingAction` khóa hoàn toàn vận tốc Rigidbody velocity trong khi thực hiện các hành động cuốc đất, dọn đá, tưới nước.

### Camera Roblox-style (`CameraFollow3D.cs`)
- **Orbit**: Xoay quanh pivot Player (pivotHeight = 1.35m) bằng công cụ toán học `Vector3.SmoothDamp`.
- **RMB Drag**: Giữ chuột phải để xoay Yaw + Pitch. Góc Pitch được khóa chặt từ 12° đến 55° (mặc định 25°) tránh góc nhìn underground.
- **Scroll Zoom**: Khoảng cách từ 3 đến 9 đơn vị, Lerp mượt mà qua con lăn chuột.
- **SphereCast Collision**: Tự động quét vòng tròn vật cản để thu ngắn khoảng cách camera khi sát tường hoặc mái nhà, bỏ qua layer Player.

---

## 2. Interaction System

### Phím Tương Tác [E] / [Space]
- Lệnh `PlayerController.TryPerformInteraction()` thực hiện quét `Physics.OverlapSphere(radius=2.5f)` xung quanh Thành.
- **Quy tắc giải phóng phím [E] khỏi NPC**: Phím [E] / [Space] được giải phóng 100% khỏi việc kích hoạt hội thoại hay mở bảng chọn của NPC Character, loại bỏ hoàn toàn lỗi xung đột tương tác khi đứng cạnh ruộng vườn. Phím [E] chỉ giữ nhiệm vụ:
  * Thao tác nông nghiệp trên các ô `SoilCell`.
  * Tương tác tâm linh tại bàn thờ `AncestralAltar`.
  * Sấy khoai/Nấu ăn tại bếp ga `KitchenHearth`.
  * Lên/xuống thuyền thúng `Coracle`.
  * Sơ tán dân làng (Trong Mid-Phase Countdown khẩn cấp).
  * Chèn bao cát và sửa tường sập (Trong Phase 2 tái thiết).

### NPC Proximity Panel (`NPCProximityOptionsUI.cs`)
- Tự động bật lên khi khoảng cách giữa Player và NPC `< 1.7m`. Panel nổi bám theo vị trí `Camera.WorldToScreenPoint` trên đầu NPC.
- CanvasGroup kiểm soát hiệu ứng mờ dần (Fade in/out) trong 0.13 giây để tránh hiện tượng nhấp nháy UI.
- Lựa chọn hành động phản hồi nhanh qua chuột click hoặc phím tắt số `[1]` / `[2]` / `[3]` trên bàn phím.
- Gọi hàm `LookAtPlayer()` xoay mượt NPC về hướng Thành khi đàm thoại và `ReturnToDefaultRotation()` đưa NPC về hướng cũ khi Thành đi xa.

### Quest Marker (`NPCQuestMarkerUI.cs`)
- Dấu chấm than `!` màu vàng bổ sung hiệu ứng nảy nhẹ (`Mathf.Sin`) trên đầu NPC có sự kiện cốt truyện/Phase chưa hoàn thành.
- Tự động ẩn đi khi khoảng cách `<= 1.7m` để nhường không gian hiển thị cho bảng nổi Proximity Panel.

---

## 3. Tutorial System (`TutorialManager.cs`)

Hệ thống Tutorial phân rã thành **2 giai đoạn kiểm tra nghiêm ngặt** kết hợp HUD tracker:

### Giai đoạn 1: Khai thông thông tin (`IntroQuests`)
- **Nhiệm vụ bắt buộc**: Người chơi phải di cận và hoàn tất hội thoại (Dialogue Closed) với **CẢ 4 NPC CHÍNH** trong làng (O Thắm, Bác Năm, Cụ Bảy, Bé Tí).
- **Quy tắc Gate**: Hệ thống tracking dựa trên sự kiện đóng hộp thoại lớn `OnDialogueClosed(speakerName)`. Trải nghiệm nhìn thấy hoặc mở Proximity panel không đủ điều kiện để tick hoàn thành.
- Chỉ khi cả 4 NPC đã được đàm thoại xong, hệ thống mới mở khóa chuyển trạng thái sang `ShowingFarmingSlides`.

### Cầu nối Slideshow: `ShowingFarmingSlides`
- Đóng băng thời gian toàn cục (`Time.timeScale = 0f`).
- Hiển thị tuần tự các slide ảnh minh họa thực tế rút từ thư mục Resources (`tutorial_clear_rocks`, `tutorial_plant_water`).
- Hỗ trợ các nút điều hướng "Quay Lại", "Tiếp Theo", "Bỏ Qua". Khi kết thúc, trả lại `Time.timeScale = 1f` và chuyển sang `FarmingTutorial`.

### Giai đoạn 2: Thực hành nông nghiệp (`FarmingTutorial`)
- **Nhiệm vụ trên lưới**: Dọn đá (subTask1) $\rightarrow$ Gieo hạt giống (subTask2) $\rightarrow$ Tưới nước ẩm (subTask3) trên các ô SoilCell Grid.
- Tự động kích hoạt Fallback: Nếu hệ thống quét thấy bất kỳ thành phần `CropInstance` nào có chỉ số `growthDays > 0.01f`, Tutorial tự động nhảy thẳng về trạng thái `Completed` để tránh kẹt mạch chơi.
- Hoàn thành Tutorial gọi lệnh phát Toast và chuyển trạng thái global sang **Phase 1: Before the Storm**.

---

## 4. Farming System

### Lưới ô đất (Farming Grid 4x3)
- Lưới trồng trọt chính thức trong scene `Village_Demo.unity` là một **solid 4x3 grid gồm 12 thực thể `SoilCell_Grid*` độc lập** với khoảng cách spacing 1.5m, định vị tại trung tâm `(-8.0, 0.13, -10.0)`.
- Từng ô đất trong lưới tự quản lý BoxCollider Trigger `(2.0, 0.3, 2.0)`, trạng thái sỏi đá, độ ẩm đất, và highlight viền vàng mục tiêu riêng biệt khi lọt vào tầm tương tác. Object cha `SoilCell` chỉ đóng vai trò liên kết tự động trong `Awake()` phục vụ xử lý hàng loạt (Bulk planting).

### Vòng lặp canh tác cải tạo
1. **Dọn đá** (`ActionClearRocks`): Tiêu hao Thể lực. Đưa RockDensity về 0, chuyển đổi đất từ cằn cỗi (`Soil_Rocky`) sang đất sạch (`Soil_Clean`).
2. **Cuốc đất xới**: Chuyển đổi trạng thái sang đất tơi xới (`Soil_Tilled`).
3. **Gieo hạt**: Mở Smart Bulk Planting Dialog (3 nút bấm: Trồng toàn bộ ô trống lân cận / Chỉ trồng ô này / Hủy). Tiêu hao `item_seed` tương ứng và spawn cây non.
4. **Tưới nước ẩm**: Tăng Moisture đất. Đất chuyển màu sẫm (`Soil_Wet`). Nước tự bốc hơi theo thời gian thực.
5. **Thu hoạch** (`ActionHarvest`): Thu về nông sản `Item_FreshCrop`.

### Lớp đất Phù Sa màu mỡ
- Kích hoạt tự động khi nước lũ rút hoàn toàn ở Phase 2. Toàn bộ 12 ô đất đồng loạt xóa sạch sỏi đá, hồi phục 100% dinh dưỡng. Sản lượng khoai lang thu hoạch tại đất Phù Sa tự động nhân đôi ($\times 2$).

---

## 5. Nghĩa Tình System (Progression Metrics)

### Tích lũy điểm cộng đồng (`CommunityManager.cs`)
Chỉ số **Nghĩa Tình** (`GlobalKarma`) là thước đo tiến trình duy nhất kiểm soát kết cục của trò chơi, tăng lên qua các hoạt động tương trợ:
- Trò chuyện với dân làng: $+5$ điểm Nghĩa Tình (Giới hạn chống spam: 1 lần/NPC/session).
- Đổi công lao động (Vần công): Tiêu hao 20 Thể lực giúp việc cho O Thắm/Bác Năm $\rightarrow$ $+10$ Nghĩa Tình, $+1$ tín chỉ Vần công.
- Hỗ trợ O Thắm quyên góp nhu yếu phẩm (Sự kiện Phase 1): $+10$ Nghĩa Tình.
- Sơ tán khẩn cấp dân làng chạy lũ (Sự kiện cuối Phase 1): Tăng mạnh điểm Nghĩa Tình dựa trên số dân làng dắt được lên vùng cao.
- Tái thiết sửa nhà, vá tường sập sau bão (Sự kiện Phase 2): $+20$ Nghĩa Tình mỗi công trình.

---

## 6. Weather & Disaster System (Cấu trúc 2 Giai đoạn cốt lõi)

Hệ thống thời tiết bão lũ được gom hoàn toàn về **2 Phase thiết kế lớn**. Giai đoạn Gió Lào hạn hán cũ bị XÓA BỎ HOÀN TOÀN khỏi phase timeline, chỉ giữ hiệu ứng nắng nóng làm mood phụ trong Phase 1.

### Phase 1: Before the Storm (Trước Bão)
- **Bối cảnh**: Trời yên bình, mây trôi nhẹ, người chơi thực hiện chuỗi Tutorial, làm nông tích khoai trên lưới 4x3 và gia cố công Vần công.
- **Nghi thức kích hoạt thiên tai**: Khi hoàn thành chuẩn bị, người chơi di chuyển đến cấu trúc `Shrine`, nhấn phím `[E]` thắp nhang tại bàn thờ `AncestralAltar`. Sự kiện này đóng vai trò là trigger chính thức kết thúc Phase 1, gọi lệnh phát thanh khẩn cấp và bật Mid-Phase Countdown.

### Siêu sự kiện: Mid-Phase Evacuation Countdown (Sơ tán chạy lũ)
- Ngay khi thắp nhang xong, Loa phát thanh xã rú lệnh báo động lũ quét, xuất hiện Banner chữ đỏ khẩn cấp. Màn hình đổi `Skybox` mưa bão sấm sét, nước lũ (`3D_Water_Plane`) bắt đầu dâng tịnh tiến đi lên theo trục Y.
- HUD bật màn hình đếm ngược **45 giây khẩn cấp** (`EvacuationTimerPanel`).
- **Gameplay sinh tồn**: Người chơi phải điều khiển Thành chạy đua với mực nước dâng, tìm đến vị trí của 4 NPC để nhấn phím `[E]` dắt họ sơ tán (NPC dắt xong sẽ tạm ẩn `SetActive(false)` để dắt người kế tiếp):
  * *Thành công (Cứu đủ 4 người trong 45s)*: Màn hình Fade Out đen, chuyển thẳng sang Phase 2 an toàn trên nóc nhà.
  * *Thất bại (Hết 45s không cứu kịp)*: Nước dâng ngập đầu nhân vật $\rightarrow$ Hiện màn hình Thua cuộc (Drown Game Over), yêu cầu nhấn nút Load làm lại phân đoạn chạy lũ.

### Phase 2: After the Storm (Sau Bão & Tái Thiết)
- **Lũ lụt trú ẩn**: Thành cùng các dân làng cứu được xuất hiện tập trung trên mái nhà cao của `Thanh_House` (`Roof_Anchor_Nodes`). Thành nhận mì tôm cứu trợ `item_noodles` để ăn hồi sức.
- **Neo mái chống bão**: Gió lớn làm mái nhà ngói rung lắc, người chơi di chuyển đến các điểm Neo ngói rỗng (`Roof_Anchor_SandbagSlots`) trên mái nhà, nhấn `[E]` tiêu hao bao cát `item_sandbag` trong túi đồ để chèn giữ ngói ngập gió $\rightarrow$ Bảo vệ Morale nhân vật và cộng điểm Nghĩa Tình.
- **Tái thiết sau lũ**: Khi bão tan, nước lũ rút hoàn toàn xuống $Y = -1.5\text{m}$. Thành xuống đất, thực hiện dọn bùn phù sa trên ruộng 12 ô gieo vụ mùa bội thu.
- **Sửa tường sập đổ nát**: Người chơi mang Tấm ván gỗ `item_flood_board` đổi từ O Thắm chạy đến các điểm Neo tường sập (`Wall_Repair_Node`) trước sạp hàng O Thắm và nhà Bác Năm, nhấn phím `[E]` để dọn xà bần và dựng lại vách vách tường gỗ mới nguyên vẹn $\rightarrow$ Tăng mạnh Nghĩa Tình.

---

## 7. Storage & Items Framework

### Quản lý hư hỏng nông sản ẩm lụt (`StorageManager.cs`)
- Nông sản khoai tươi (`Item_FreshCrop`) có cơ chế tự động thối rữa, phân hủy (Decay) cực nhanh dưới điều kiện độ ẩm cao của bão lũ Phase 2.
- Người chơi bắt buộc phải sử dụng bếp ga `KitchenHearth` (IInteractable) dưới mái hiên để thực hiện sấy khô khoai tươi thành khoai gieo khô (`Item_PreservedCrop`). Khoai gieo có Decay = 0, lưu trữ an toàn xuyên bão lũ dùng cứu trợ dân làng.

### Định danh danh mục vật phẩm mới
- `item_seed`: Hạt giống gieo trồng trên lưới 4x3.
- `item_fresh_crop`: Khoai lang tươi (ăn hồi sức nhẹ, sấy khô, quyên góp).
- `item_preserved_crop`: Khoai gieo khô (vật phẩm cốt lõi dùng để quyên góp cứu trợ sau lũ).
- `item_incense`: Nhang cúng tương tác tại Altar để kích bão đổ bộ.
- `item_noodles`: Mì tôm cứu trợ khẩn cấp cấp phát tự động khi lên nóc nhà.
- `item_sandbag` (Bao cát): Vật liệu Phase 2 chuyên dụng để tương tác chèn giữ ngói rung trên mái nhà chống bão tốc mái.
- `item_flood_board` (Tấm ván gỗ): Vật liệu Phase 2 chuyên dụng để mang đi vá, dựng lại các mảng vách tường sập đổ nát của sạp O Thắm và nhà Bác Năm sau lũ.

---

## 8. Ending System (Đánh giá kết cục)

Khi chu kỳ ngày tái thiết của Phase 2 khép lại hoàn toàn, `EndingManager` quét chỉ số `GlobalKarma` để mở màn hình kết thúc game:
- **Best Ending: Đất Cày Nở Hoa ($\ge 80$ điểm)**: Cánh đồng khoai 12 ô xanh mướt, mái đình sửa xong. Thành được vinh danh dưới loa phát thanh xã là ân nhân của làng quê hồi sinh từ bùn lũ.
- **Normal Ending: Lá Lành Đùm Lá Rách ($40 \rightarrow 79$ điểm)**: Làng dọn dẹp tạm xong nhưng ai nấy lẳng lặng lo việc nhà nấy, tình làng nghĩa xóm dừng ở mức xã giao, cuộc sống diễn ra bình lặng.
- **Sad Ending: Đất Sỏi Đá Cằn ($< 40$ điểm)**: Thành ích kỷ giữ tài nguyên, làng quê tan hoang hoang hóa, dân làng dọn sạp bỏ xứ đi nơi khác, Thành cô độc trên sỏi đá cũ.