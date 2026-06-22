# Current Scene State: Đất Cày Lên Sỏi Đá

Tài liệu này cung cấp ảnh chụp thực tế (snapshot) trạng thái hiện tại của cảnh 3D Unity (`SampleScene.unity`) nhằm giúp các AI Agent và lập trình viên nắm bắt chính xác cấu trúc thực tế của dự án, giảm thiểu các phán đoán sai lệch hoặc lập trình trùng lặp tính năng.

---

## 1. Actual Scene Hierarchy (Cấu trúc phân cấp thực tế)
Trong cảnh chơi `SampleScene.unity`, các đối tượng đang được xếp đặt như sau:
*   **`_Managers`**: GameObject rỗng chứa các script điều phối:
    *   `GameManager` (Điều khiển chu kỳ ngày đêm, đếm thời gian).
    *   `PlayerStats` (Máu, Thể lực, Tinh thần, Xu).
    *   `WeatherManager` (Lerp thời tiết, dâng nước lũ, kiểm soát hạt mưa).
    *   `StorageManager` (Quản lý kho chứa, thối nông sản ẩm).
    *   `CommunityManager` (Lưu giữ điểm Nghĩa Tình và đổi công Vần công).
    *   `EndingManager` (Quản lý màn hình kết cục game dựa trên điểm Nghĩa Tình).
*   **`Player`**: Nhân vật chính.
    *   *Components:* `Rigidbody` (Freeze Rotation X/Y/Z, Freeze Position Y), `BoxCollider` (hoặc CapsuleCollider), `PlayerController` script.
    *   *Child object `Player_Base`:* Chứa mô hình 3D `indonesian_farmer_pak_tani.glb` và component `Animator`.
*   **`Main Camera`**:
    *   *Components:* Camera, `Camera Follow 3D` script (Target liên kết đến `Player`).
*   **`SoilCells`**: Nhóm chứa 4 luống đất độc lập (`SoilCell_Grid2`, `SoilCell_Grid3`, `SoilCell_Grid4`, và `SoilCell`).
    *   *Components:* `BoxCollider` (Is Trigger = True), `SoilCell` script.
*   **`AncestralAltar`**: Bàn thờ tổ tiên làng xã.
    *   *Components:* `BoxCollider` (Is Trigger = True), `AncestralAltar` script.
*   **`NPC_BacNam`** & **`NPC_OTham`**: Các nhân vật dân làng tĩnh.
    *   *Components:* `BoxCollider` (Is Trigger = True), `NPCCharacter` script.
*   **`Paths`**: Chứa các đối tượng `RoadSegment` tạo lối đi.
*   **`_UI_Tester`**: GameObject chứa các script UI thử nghiệm:
    *   `FrameworkMainMenuUI` (Màn hình mở đầu bằng gỗ).
    *   `FrameworkDebugUI` (Bảng debug F1).
    *   `FrameworkTester` (Chạy kịch bản test nhanh).
*   **`Canvas`** (Main UI Canvas):
    *   `NghiaTinhPanel` (HUD displaying Community Reputation, positioned top-left below Time panel at x=20, y=-100 inside the `SurvivalUI` hierarchy, size 280x70, with `NghiaTinhUI` script).
    *   `ResourcePanel` (HUD panel containing Health, Stamina, and Morale sliders, positioned bottom-left at x=20, y=20. Auto-hides when NPC Dialogue, Shop UI, or Ending screen is active to prevent overlapping).
    *   `VillageSpeakerBanner` (Banner Loa phát thanh xã phát thông điệp khẩn cấp, kèm script `VillageSpeakerBanner`).
    *   `EndingPanel` (Màn hình kết thúc cốt truyện, kèm script `EndingManager` liên kết ngoài).
    *   `NPCProximityPanel` (Dynamic dark semi-transparent options popup panel showing up when player is near NPCs, managed by `NPCProximityOptionsUI` attached to `SurvivalUI`).
    *   `inventoryPanel` (Centered at `(0, 0)`, size `400x320` with a dark rustic background. Item slots size `80x80` with dark rustic background, gold outlines, centered `60x60` icons, and bold warm yellow quantity counts).
    *   `shopPanel` (Centered at `(0, 0)`, size `500x400` with dark blue-brown background. List container size `460x320` containing 5 item rows. Each row is `60` height with `56x56` icons, warm gold item titles, gray descriptions, and custom Green Buy / Orange Sell transaction buttons).

---

## 2. Existing Prefabs & Asset Registry

### Tòa nhà & Kiến trúc (Assets/Prefabs/)
*   `HoiAnHouse_M2.fbx`: Nhà chính của Thành kiểu cổ kính rêu phong.
*   `asian_house.glb`: Nhà/đại lý tương trợ của O Thắm.
*   `Well/Well.fbx`: Giếng đá cổ để múc nước tưới.
*   `Fence/Fence.fbx` & `Fence2.fbx`: Các đoạn rào tre/gỗ đơn sơ.

### Thực vật & Thiên nhiên (Assets/Prefabs/FBX-Ultimate-Nature-Pack/)
*   `Banana Tree/BananaTree.obj`: Cây chuối trang trí vườn nhà.
*   `Bamboo/PUSHILIN_bamboo.obj`: Các thân tre ghép bụi tre làng.
*   `WoodLog.fbx` / `TreeStump.fbx`: Củi gỗ trang trí mặt ruộng.
*   `Rock_1.fbx` đến `Rock_7.fbx`: Đá tảng tự nhiên rải rác xung quanh ruộng.
*   `Plant_1`, `Plant_3`, `Plant_5`: Model cây dùng cho các giai đoạn sinh trưởng của Khoai Lang.

---

## 3. Data Assets (ScriptableObjects)
Đã cấu hình các tệp dữ liệu tĩnh trong thư mục `Assets/Data/`:

*   **Item Data Assets (`ItemData`):**
    *   `Item_FreshCrop.asset` (Khoai Lang Tươi): Loại: Nông sản tươi. Decay rate: 0.8.
    *   `Item_PreservedCrop.asset` (Khoai Gieo): Loại: Nông sản khô. Decay rate: 0.
    *   `Item_Incense.asset` (Nhang cúng): Hồi Morale: 10.
    *   `Item_Seed.asset` (Hạt giống Khoai Lang).
*   **Crop Data Assets (`CropData`):**
    *   `Crop_KhoaiLang.asset`: Cấu hình chín sau 5 ngày, hạt giống giá trị 10 xu, ký gửi thu hoạch 25 xu. Đã liên kết mô hình 3D `Plant_1`, `Plant_3`, `Plant_5` cho các giai đoạn lớn.

---

## 4. Known Working Features (Các chức năng hoạt động tốt)
*   **Vòng lặp canh tác hoàn chỉnh:** Thành di chuyển 3D $\rightarrow$ Cuốc đất dọn đá cằn $\rightarrow$ Gieo hạt tiêu hao đúng 1 Seed từ hòm đồ $\rightarrow$ Tưới nước ẩm đất $\rightarrow$ Thu hoạch $\rightarrow$ Nhận khoai tươi cộng vào kho đồ $\rightarrow$ HUD hiển thị Toast báo nhận quà.
*   **Nội suy thời tiết & Nước dâng:** Thời tiết Lerp mượt mà. Hệ thống hạt mưa tự động thổi chéo bám sát camera. Nước lũ nâng hạ Y thực tế dìm ngập ruộng đồng.
*   **O Thắm Transaction Menu:** Tiếp tiếp cận O Thắm bấm E mở UI 3 lựa chọn (Trò chuyện tăng tình nghĩa, Vần công tiêu hao 20 thể lực lấy công vần công, Trao đổi tương trợ hạt giống/nhang).
*   **Hệ thống Loa phát thanh xã & Phase Banner:** Tự động hiển thị tiêu đề giai đoạn và nội dung tin phát thanh khẩn cấp bằng tiếng Việt khi bắt đầu ngày mới hoặc đổi Phase, tự động ẩn dần sau 5 giây.
*   **Debug Menu (F1):** Tăng giảm nhanh hạt giống, xu hỗ trợ, khoai tươi, và thử nghiệm nhanh stress nhiệt/lạnh.

---

## 5. Known Broken Features & Temporary Hacks

### Lỗi đã biết (Known Broken Features)
*   **Lỗi BoundaryElement scale:** Các tảng đá chặn biên giới rìa bản đồ (`BoundaryElement`) đang bị lỗi scale import quá nhỏ (`globalScale = 1.0`), đóng vai trò như các bức tường vô hình giữa khoảng không. Cần căn chỉnh lại scale Mesh.

### Cách vá tạm thời (Temporary Hacks)
*   **Disabled Road MeshColliders:** Các MeshCollider trên các mảnh đường đi `RoadSegment` đã bị disable thủ công để giải phóng di chuyển của nhân vật chính Thành, ngăn kẹt collider vô hình trên lối đi.
*   **Rigidbody Constraint Y:** Rigidbody của Player được tích chọn khóa chiều cao (Freeze Position Y) và khóa xoay (Freeze Rotation X/Y/Z) để nhân vật di chuyển phẳng trên trục X/Z, tránh rơi tự do khi gặp lỗi địa hình lồi lõm trước khi có bản vá terrain hoàn chỉnh.

---

## 6. New Clean Scene: Village_Demo.unity
*   **Scene File:** [Village_Demo.unity](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scenes/Village_Demo.unity)
*   **Hierarchy Structure:**
    *   `_Managers`: Contains all Singleton Manager scripts (`GameManager`, `PlayerStats`, `WeatherManager`, `StorageManager`, `CommunityManager`, `EndingManager`).
    *   `_UI`: UI elements (`SurvivalCanvas`, `EventSystem`, `TestController`).
    *   `Player`: Player controller and Rigidbody/Collider.
    *   `Main Camera`: Camera follow script.
    *   `Lighting`: Contains lighting objects (`Directional Light`, `Global Volume`).
    *   `Environment`: Land, fences, paths, and houses (`Houses` group contains the migrated 3D visual models for `Thanh_House`, `OTham_Shop`, and `BacNam_House`. `House_OTham_PLACEHOLDER` is deactivated).
    *   `FarmingArea`: Rectangular plot containing the `SoilCell` GameObjects.
    *   `NPCs`: `NPC_BacNam` and `NPC_OTham`.
    *   `InteractionZones`: `Shrine` (includes the migrated 3D `AltarModel`).
    *   `DisasterObjects`: Empty parent (for runtime instantiated flood water plane).
    *   `Audio`: Ambient and music sources.
 
*   **Key World Coordinates:**
    *   `Player`: `(0, 0.5, -6)`
    *   `Thanh_House`: `(0.0, 0.0, -15.5)` (Rotated Y = 180°, BoxCollider size = 8.0 x 5.57 x 9.59)
    *   `OTham_Shop`: `(8.0, 0.0, 10.28)`
    *   `BacNam_House`: `(-12.0, 0.0, 8.5)`
    *   `Village_Well`: `(0, 0, 3)`
    *   `Village_Speaker`: `(0, 1.75, 6)` (Custom Cylinder pole acts as stand-in)
    *   `Shrine`: `(7, 0, -6)`
    *   `NPC_OTham`: `(3.53, 0.5, -9.49)` (Visual child scale = `1.30`, local Y position = `0.61`, height = `2.21m`)
    *   `NPC_BacNam`: `(-1.78, 0.5, -10.18)` (Visual child scale = `1.22`, local Y position = `0.54`, height = `2.074m`)
    *   `SoilCells`: Arranged as a clean 2x2 grid near Thành House (SoilCell_1: `(-8, 0.02, -10)`, SoilCell_2: `(-6, 0.02, -10)`, SoilCell_3: `(-8, 0.02, -12)`, SoilCell_4: `(-6, 0.02, -12)`). Excess SoilCells are disabled. All active SoilCells are rotated X = 90 to lie flat on XZ plane.
    *   `FarmingPlot`: A visual brown soil bounding box under `FarmingArea` at `(-6, 0.01, -11)` to visually frame the SoilCells in Edit Mode.
    *   `Main Camera`: Controlled by `CameraFollow3D` script at runtime with Roblox-style third-person orbit settings: Default Distance = 4.5, pivotHeight = 1.35, Default Pitch = 10° (pitch range: 10°-55°), Zoom Range = 1.3-8.0, smoothTime = 0.02s, and SphereCast-based collision safety (ignoring the Player layer) enabled. Player visual remains perfectly centered horizontally and lower-center vertically in LateUpdate.
    *   `Directional Light`: Rotation `(45, 30, 0)`, Color set to warm sunlight (Color: Warm Yellow, Intensity: 1.1).
 
*   **Scene Setup Limitations:**
    *   `3D_Water_Plane` is dynamically instantiated by `WeatherManager` at runtime and positioned at `Y = -1.5f` (under ground) until storm phase, preventing issues with early flooded visuals.
    *   Ground (`Ground_Main`, previously `grass_ground`) and SoilCell sprite orientations have been corrected to lie flat on the XZ plane by setting Rotation X = 90.
    *   NPCs use real FBX models inside the scene (placed in `Visual` child), scaled and positioned so they stand upright at the same height as the Player (~2.20m) and their feet touch the ground.
