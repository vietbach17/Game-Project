# Current Source Context Report — Đất Cày Lên Sỏi Đá

Tài liệu này cung cấp báo cáo ngữ cảnh mã nguồn toàn diện của dự án Unity tại thời điểm hoàn thành việc định hướng lại lối chơi từ sinh tồn nông nghiệp thương mại sang **Nguyên mẫu sinh tồn cộng đồng (Community Survival Prototype)** và hoàn tất việc di trú, chuẩn hóa các tài nguyên hình ảnh từ `SampleScene` sang `Village_Demo`.

---

## 1. Project Overview

*   **Unity Editor Version**: `6000.4.3f1` (Unity 6)
*   **Active Render Pipeline**: Universal Render Pipeline (URP)
*   **Current Main Demo Scene**: [Village_Demo.unity](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scenes/Village_Demo.unity)
*   **Current Gameplay Direction**: Canh tác nông sản làm nguồn lương thực cứu tế, giúp đỡ bà con chòm xóm (Vần công), và thắt chặt tình nghĩa làng xã (Nghĩa Tình score) để vượt qua các giai đoạn thiên tai khắc nghiệt miền Trung Việt Nam.
*   **Current Target Demo Scope**: Vòng lặp chơi thử nghiệm 4 giai đoạn thời tiết (Gió Lào khô nóng, Mưa bão ngập lụt, Tái thiết sau lũ) được điều phối bởi hệ thống sự kiện cộng đồng, kết thúc bằng một màn hình tổng kết (Sad/Normal/Best Ending) dựa trên chỉ số Nghĩa Tình tích lũy.

---

## 2. Scene Overview

### `Assets/Scenes/Village_Demo.unity`
*   **Purpose**: Cảnh chơi chính thức của bản demo, chứa cấu trúc phân nhóm gọn gàng, tọa độ chuẩn hóa và tất cả các logic gameplay hoạt động thực tế.
*   **Important Root Objects**:
    *   `_Managers`: Chứa toàn bộ các Singletons quản lý logic game.
    *   `_UI`: Hệ thống Canvas chính (`SurvivalCanvas`), EventSystem, và debug panel.
    *   `Player`: Thực thể người chơi chính Thành.
    *   `Main Camera`: Camera đi kèm script theo sát.
    *   `Environment`: Chứa nhà cửa dân làng, giếng nước, hàng rào và mặt đất.
    *   `FarmingArea`: Khung chứa 11 luống đất trồng trọt.
    *   `NPCs`: Nhân vật Bác Năm và O Thắm.
*   **Role**: **Playable Demo Scene** (Build Index 0).
*   **Known Risks**: Cần kiểm soát chặt chẽ các rào cản va chạm xung quanh nhà cửa mới di trú để tránh kẹt người chơi.

### `Assets/Scenes/SampleScene.unity`
*   **Purpose**: Cảnh thử nghiệm (sandbox) của đội ngũ phát triển, chứa các tài nguyên mới, mô hình gốc và các thiết lập nháp trước khi di chuyển sang demo chính.
*   **Important Root Objects**: Chứa cấu trúc hỗn hợp không chuẩn hóa, các mô hình chưa căn chỉnh tỉ lệ và script thử nghiệm cũ.
*   **Role**: **Developer Sandbox / Asset Source Only**. Không được dùng để chạy demo hoặc build sản phẩm.
*   **Known Risks**: Chứa các prefab bị thiếu liên kết (Missing Prefab) và tọa độ chưa được tối ưu hóa.

---

## 3. Core Gameplay Systems

### Player Movement & Roblox-style Camera
*   **Main Scripts**:
    *   [PlayerController.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerController.cs)
    *   [CameraFollow3D.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/CameraFollow3D.cs)
*   **Responsibilities**: Điều khiển di chuyển của nhân vật Thành theo hướng tương quan của camera phẳng (XZ plane) và xoay hướng mượt mà. Camera hỗ trợ xoay RMB, cuộn phóng thu thuôn mượt (Smooth Zoom) bằng `Vector3.SmoothDamp`, và quét va chạm tự động (SphereCast).
*   **Important Behavior**: Camera được thiết lập hạ thấp chiều cao (2.5m), góc nghiêng vừa phải (mặc định 25 độ, phạm vi 12-55 độ) mang lại cảm giác nhập vai đậm chất Roblox Simulator.
*   **Known Dependencies**: New Input System, Rigidbody.

### Farming Loop & SoilCell Target Highlight
*   **Main Scripts**:
    *   [SoilCell.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Agriculture/SoilCell.cs)
    *   [CropData.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Agriculture/CropData.cs)
*   **Responsibilities**: Quản lý vòng đời ô đất canh tác (Dọn đá $\rightarrow$ Cuốc đất $\rightarrow$ Gieo hạt $\rightarrow$ Tưới nước $\rightarrow$ Thu hoạch).
*   **Important Behavior**: Tự động hiển thị một lớp đè màu vàng bán trong suốt (`VisualHighlight`) phản chiếu kết cấu đất tương ứng khi người chơi đứng trong phạm vi tương tác (1.7m) và nhìn về phía ô đất.

### Inventory / Storage
*   **Main Scripts**:
    *   [StorageManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/StorageManager.cs)
*   **Responsibilities**: Quản lý hòm chứa hạt giống, nông sản tươi (dễ thối hỏng do ẩm ướt) và nông sản khô (Khoai gieo).

### Weather / Phase System
*   **Main Scripts**:
    *   [WeatherManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/WeatherManager.cs)
    *   [GameManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/GameManager.cs)
*   **Responsibilities**: Điều khiển ánh sáng môi trường, chu kỳ ngày/đêm, mưa rơi chéo bám góc camera, và dâng lũ ngập thực tế ở Phase 3.

### Nghĩa Tình / CommunityManager
*   **Main Scripts**:
    *   [CommunityManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/CommunityManager.cs)
*   **Responsibilities**: Theo dõi điểm Nghĩa Tình cộng đồng (`GlobalKarma`) và số lượng công Vần công tích lũy của Thành.

### NPC Interaction & Proximity Popup
*   **Main Scripts**:
    *   [NPCCharacter.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/NPCCharacter.cs)
    *   [NPCProximityOptionsUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/NPCProximityOptionsUI.cs)
*   **Responsibilities**: Phát hiện người chơi tiếp cận trong bán kính 1.7m và mở bảng UI lựa chọn nổi trên đầu NPC. Hỗ trợ quay mặt đối diện mượt mà (`Slerp`) và xoay về hướng nghỉ (Default Idle Rotation) khi người chơi rời đi.

### Phase-based Community Events
*   **Main Scripts**:
    *   [CommunityManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Community/CommunityManager.cs)
*   **Responsibilities**: Cung cấp các hành động đặc biệt tùy theo Phase (Hỗ trợ O Thắm ở Phase 2, Chằng chống nhà Bác Năm ở Phase 3, Tái thiết ruộng ở Phase 4) nhằm tiêu hao tài nguyên cứu tế và thưởng lượng lớn Nghĩa Tình. Giới hạn chỉ hoàn thành một lần mỗi sự kiện.

### Village Speaker Banner
*   **Main Scripts**:
    *   [VillageSpeakerBanner.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/VillageSpeakerBanner.cs)
*   **Responsibilities**: Hiển thị thông tin khẩn cấp tiếng Việt bằng CanvasGroup mờ dần sau 5 giây mỗi khi đổi sang Phase thời tiết mới.

### Ending / Game Over
*   **Main Scripts**:
    *   [EndingManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/EndingManager.cs)
*   **Responsibilities**: Kết thúc màn chơi và đưa ra 3 loại đánh giá kết cục dựa trên Nghĩa Tình cuối cùng:
    *   `< 40`: Sad Ending (Bản làng hoang tàn, ly tán).
    *   `>= 40`: Normal Ending (Bà con vượt lũ khó khăn).
    *   `>= 80`: Best Ending (Bà con thắt chặt tình nghĩa vượt lũ thành công).

### F1 Demo / Debug Controls
*   **Main Scripts**:
    *   [FrameworkDebugUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/FrameworkDebugUI.cs)
*   **Responsibilities**: Giao diện điều phối viên, cung cấp 10 nút bấm nhảy nhanh Phase (1-4), thiết lập nhanh tài nguyên (Hạt giống, Thực phẩm), thay đổi Nghĩa Tình (20/50/80), và kích hoạt kết cục ngay lập tức.

---

## 4. Input Mapping

Dự án sử dụng **Unity New Input System**. Cấm sử dụng các phương thức legacy của `UnityEngine.Input` ngoại trừ hiển thị GUI nội bộ.

*   **WASD / D-Pad**: Di chuyển nhân vật (Tương quan góc nhìn Camera).
*   **Right Mouse Button (RMB) (Hold + Drag)**: Xoay hướng góc nhìn Camera.
*   **Mouse Scroll Wheel**: Phóng to / Thu nhỏ khoảng cách Camera (Zoom mượt từ 3 đến 9 đơn vị).
*   **Key [E]**: Tương tác canh tác với ô đất (`SoilCell`) hoặc cúng bái tại bàn thờ (`AncestralAltar`).
*   **Keys [1] / [2] / [3]**: Phím tắt kích hoạt các lựa chọn tương ứng trên bảng hội thoại nổi NPC (Trò chuyện, Giúp việc, Chia sẻ thực phẩm, Sự kiện đặc biệt).
*   **Key [H]**: Ẩn / Hiện bảng Hướng dẫn điều khiển (Controls Legend UI).
*   **Key [F1]**: Mở giao diện Trình kiểm soát Demo (Debug Menu).
*   **Keys [Esc] / [Enter]**: Điều khiển giao diện Menu chính.

---

## 5. UI/HUD Systems

*   **Time/Season HUD** (`dayText`, `phaseText`): Vị trí góc trên bên trái màn hình. Hiển thị ngày thực tế và giai đoạn thiên tai hiện tại.
*   **Nghĩa Tình HUD** (`NghiaTinhPanel`): Anchored Top-Left dưới Time panel tại tọa độ `(20, -100)` được chuyển vào phân cấp `SurvivalUI` tại runtime để đồng bộ không gian tọa độ, kích thước `280x70`. Tự động đổi màu thanh trượt theo ngưỡng điểm Nghĩa Tình hiện tại (Đỏ $\rightarrow$ Vàng $\rightarrow$ Xanh).
*   **Controls Legend UI** (`ControlsLegendPanel`): Anchored Top-Right tại tọa độ `(-20, -120)`, kích thước `220x220`, độ trong suốt tăng lên `0.8` và cỡ chữ giảm xuống `11f` để tránh che khuất tầm nhìn. Hiển thị phím tắt lối chơi cơ bản và có thể ẩn đi bằng phím `H`.
*   **Toast Notifications**: Anchored Upper-Middle `(0.5, 0.85)` tại tọa độ `(0, 0)`. Tự động định dạng nổi bật từ ngữ bằng Regex (Màu vàng cho Nghĩa Tình, màu xanh cho Vần công, màu đỏ cho cảnh báo hết Thể lực/Tài nguyên).
*   **NPC Proximity Options**: Khung hội thoại nổi Screenspace tự động bám trên đầu NPC, giới hạn chiều rộng 380px, căn chỉnh nút đồng nhất 360px và tự động ẩn khi đối thoại/cửa hàng mở rộng.
*   **Main Menu UI**: Màn hình mở đầu thiết kế phong cách bảng gỗ mộc mạc, hiệu ứng rê chuột phóng to 1.08x và chuyển màu vàng ấm `#FAD959` mềm mại trong 0.15s.

---

## 6. Important Assets & Visual Setup

*   **Player Model**: Chuyển đổi thành công sang nhân vật Lão Nông Việt Nam (`indonesian_farmer_pak_tani.glb`) kèm bộ xương chuyển động.
*   **NPC Models**: Sử dụng mô hình FBX thực tế `Bac_Nam.fbx` và `O_Tham.fbx` đi kèm vật liệu URP Simple Lit (`M_BacNam`, `M_OTham`), được chuẩn hóa tỉ lệ cao tương đương người chơi và chân đặt sát mặt đất.
*   **OTham_Shop**: Gian hàng chợ lá mái tre đầm ấm được di trú từ `SampleScene` đặt tại tọa độ `(8.0, 0.0, 10.28)`.
*   **BacNam_House**: Ngôi nhà mái ngói cũ kèm chõng tre di trú đặt tại tọa độ `(-12.0, 0.0, 8.5)`.
*   **Thanh_House**: Nhà chính ngôi nhà cổ kiểu Hội An rêu phong đặt tại tọa độ `(0.0, 0.0, -15.5)` xoay 180 độ. Căn chỉnh khớp hoàn toàn BoxCollider cục bộ `(8.00, 5.57, 9.59)` để loại bỏ tường vô hình chặn spawn.
*   **Village_Well**: Giếng nước đá cổ ở trung tâm làng tại tọa độ `(0.0, 0.63, 3.0)`. Đã thu nhỏ BoxCollider root từ kích thước thế giới khổng lồ `(2.5, 2.5, 2.5)` bị lệch tâm sang BoxCollider gọn gàng ôm sát thành giếng: local center `(0.000043, -0.001367, 0.00375)`, local size `(0.007, 0.007, 0.0075)` tương đương thế giới thực `1.4m x 1.5m x 1.4m` đặt tại world `(0.0085, 1.38, 3.2735)`.
*   **Farming Visuals & Ground**: Ô đất trồng trọt dạng mặt phẳng nằm ngang trục XZ (Rotation X = 90). Nền đất cỏ chính sử dụng Plane 3D cùng vật liệu stylized màu xanh cỏ tự nhiên `#7CA66D` (`Ground_LowPoly.mat`).

---

## 7. Git / Merge Context

*   Dự án vừa hoàn tất giải quyết xung đột mã nguồn sau đợt merge nhánh phát triển chính.
*   **Cảnh nguồn của dự án hiện tại là `Assets/Scenes/Village_Demo.unity`**. Mọi thay đổi về thế giới, nhà cửa, logic cảnh chơi phải được thực hiện trên cảnh này.
*   Không được thay đổi các cơ chế kịch bản hoạt động lõi hoặc ghi đè lung tung cấu trúc của các Singletons trong `_Managers` để tránh mất liên kết Inspector.

---

## 8. Known Issues / Watchlist

*   **Visual Border Scale**: Các khối đá chặn biên giới (`BoundaryElement`) đang có tỉ lệ nhập khẩu quá nhỏ, hoạt động như các bức tường vô hình giữa khoảng không. Cần kiểm tra lại mesh gốc.
*   **Camera Collision Obstacles**: Ở các vị trí góc hẹp sát mái nhà của Thành, camera có thể quét trúng collider nhà và thu ngắn khoảng cách đột ngột. Cần tối ưu góc đứng của người chơi.
*   **Menu Settings Input Bindings**: Phần hiển thị gán phím (key bindings) trong menu cài đặt vẫn còn sử dụng nhãn KeyCode legacy. Cần tiếp tục cập nhật tương thích hoàn toàn với New Input System.

---

## 9. Recommended Next Tasks

1.  **P0: Build Test và Sửa lỗi biên dịch đóng gói**: Chạy thử nghiệm đóng gói dự án để đảm bảo các script Editor (nếu có) được phân tách đúng bằng chỉ thị tiền xử lý `#if UNITY_EDITOR`.
2.  **P1: Chuẩn hóa Gán phím Menu chính**: Cập nhật FrameworkMainMenuUI để đọc trực tiếp các nút bấm từ cấu hình Input System thay vì KeyCode tĩnh.
3.  **P1: Tối ưu hóa va chạm rào chắn bản đồ**: Khắc phục các BoundaryElement bằng cách gán BoxCollider đơn giản bao quanh rìa bản đồ thay vì dùng MeshCollider phức tạp của các tảng đá nhỏ.
4.  **P2: Tăng cường hiệu ứng Flood Water**: Bổ sung bọt nước hoặc đổi màu bầu trời trầm hơn khi lũ bắt đầu dâng ở Phase 3 để tăng không khí kịch tính của game.

---

## 10. Files Most Important to Protect

Nghiêm cấm sửa đổi vô căn cứ hoặc thay thế nội dung các tệp sau để bảo toàn khung logic lõi:

*   [PlayerController.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerController.cs)
*   [SurvivalUIManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/SurvivalUIManager.cs)
*   [NPCProximityOptionsUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/NPCProximityOptionsUI.cs)
*   [NPCCharacter.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/NPCCharacter.cs)
*   [CommunityManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/CommunityManager.cs)
*   [SoilCell.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Agriculture/SoilCell.cs)
*   [GameManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/GameManager.cs)
*   [WeatherManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/WeatherManager.cs)
*   [FrameworkDebugUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/FrameworkDebugUI.cs)
*   [Village_Demo.unity](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scenes/Village_Demo.unity)
*   [docs/CURRENT_PROGRESS.md](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/docs/CURRENT_PROGRESS.md)
*   [docs/CURRENT_SCENE_STATE.md](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/docs/CURRENT_SCENE_STATE.md)
*   [docs/SCENE_STRUCTURE.md](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/docs/SCENE_STRUCTURE.md)
