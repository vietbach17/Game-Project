# Current Scene State: Đất Cày Lên Sỏi Đá

Tài liệu này cung cấp ảnh chụp thực tế (snapshot) trạng thái hiện tại của cảnh 3D Unity (`Village_Demo.unity`) nhằm giúp các AI Agent và lập trình viên nắm bắt chính xác cấu trúc thực tế của dự án, giảm thiểu các phán đoán sai lệch hoặc lập trình trùng lặp tính năng.

> **Scene chính hiện tại:** `Assets/Scenes/Village_Demo.unity` (Build Index 0)  
> `SampleScene.unity` chỉ còn vai trò sandbox phát triển, **không** dùng cho demo/build.

---

## 1. Actual Scene Hierarchy (Cấu trúc phân cấp thực tế)

### Root Objects trong `Village_Demo.unity`:

*   **`_Managers`**: GameObject rỗng chứa các script Singleton điều phối:
    *   `GameManager` – Chu kỳ ngày/đêm, đếm thời gian, phát sự kiện `OnDayChanged` / `OnPhaseChanged` (với 2 Phase chính).
    *   `PlayerStats` – Máu, Thể lực, Tinh thần, Xu, Nhiệt lạnh/nóng stress.
    *   `WeatherManager` – Lerp thời tiết, dâng nước lũ, kiểm soát hạt mưa.
    *   `StorageManager` – Quản lý kho chứa, theo dõi thối nông sản ẩm.
    *   `CommunityManager` – Điểm Nghĩa Tình (`GlobalKarma`), công Vần công, tracking sự kiện Phase.
    *   `EndingManager` – Quản lý màn hình kết cục dựa trên Nghĩa Tình.
    *   `AudioManager` – Âm thanh nền và SFX.
    *   `TutorialManager` – Singleton tutorial 2 giai đoạn (IntroQuests → FarmingTutorial).

*   **`Player`**: Nhân vật chính Thành.
    *   *Components:* `Rigidbody` (useGravity=false, Freeze Rotation X/Y/Z, Freeze Position Y), `BoxCollider`, `PlayerController` script (namespace `SownInStone.Core`).
    *   *Child object `Player_Base`:* Model 3D `indonesian_farmer_pak_tani.glb` + `Animator`. Phụ kiện `NonLa` (Nón lá) có thể equip vào đây.
    *   Spawn position: `(0, 0.5, -6)`.

*   **`Main Camera`**:
    *   *Components:* Camera, `CameraFollow3D` script (Target → `Player`).
    *   Orbit Roblox-style: Distance=6f, defaultPitch=25°, minPitch=12°, maxPitch=55°, zoomRange 3–9. SphereCast collision safety bật.

*   **`Lighting`**:
    *   `Directional Light` – Rotation `(45, 30, 0)`, warm sunlight Intensity 1.1.
    *   `Global Volume` – URP Post-processing.

*   **`Environment`**:
    *   `Ground_Main` (Plane 3D + MeshCollider, material `Ground_LowPoly.mat`, màu `#7CA66D`).
    *   `_Environment/Houses`:
        *   `Thanh_House` – `HoiAnHouse_M2.fbx` tại `(0, 0, -15.5)`, xoay Y=180°. BoxCollider size `(8.00, 5.57, 9.59)`.
        *   `OTham_Shop` – gian hàng lá mái tre tại `(8.0, 0.0, 10.28)`.
        *   `BacNam_House` – nhà mái ngói + chõng tre tại `(-12.0, 0.0, 8.5)`.
        *   `bep_gas` – bếp ga dưới mái hiên Thanh_House tại `(5.89, 0.21, -8.69)`, BoxCollider trigger size `(0.03, 0.03, 0.03)`, scale `(40,40,40)`, đính kèm `KitchenHearth` script (`IInteractable`).
    *   `_Environment/NPCs`:
        *   `NPC_CuBay` – Mixamo `cu_bay@Old Man Idle.fbx` tại `(-14.20, 0.00, 4.95)`, BoxCollider trigger `(1.7, 1.7, 1.7)`.
        *   `NPC_BeTi` – Mixamo `be_ti@Breathing Idle.fbx` tại `(-2.72, 0.00, 5.00)`, BoxCollider trigger `(1.7, 1.7, 1.7)`.
    *   `Village_Well` – tại `(0, 0, 3)`. BoxCollider local center `(0.000043, -0.001367, 0.00375)`, local size `(0.007, 0.007, 0.0075)` với scale 200.
    *   `Village_Speaker` – cột loa tại `(0, 1.75, 6)`.
    *   Fences – 12 đoạn rào `Fence2.fbx` đặt tự động bao quanh ruộng bởi `SetupSolidRuongEditor.cs`.
    *   Bamboo, BananaTree, rocks, props trang trí.

*   **`FarmingArea`**:
    *   `FarmingPlot` – box hình nền nâu tại `(-6, 0.01, -11)` (Edit Mode only).
    *   12 `SoilCell` con (lưới 4x3, spacing 1.5m) trung tâm tại `(-8.0, 0.13, -10.0)`.
        *   Mỗi cell: BoxCollider trigger `(2.0, 0.3, 2.0)`.
        *   `SoilCell` script tự động link cha-con qua `Awake()` theo khoảng cách XZ < 6m.
        *   3D Visuals: `Soil_Rocky`, `Soil_Clean`, `Soil_Tilled`, `Soil_Wet` prefabs từ `Assets/Prefabs/Farming/`.
        *   Target highlight: khung viền vàng gold 4 Cube dẹt (`SetHighlight(bool)`).

*   **`NPCs`** (Root):
    *   `NPC_OTham` – FBX `O_Tham.fbx`, material `M_OTham.mat`. Vị trí `(3.53, 0.5, -9.49)`. Scale visual 1.30, local Y=0.61.
    *   `NPC_BacNam` – FBX `Bac_Nam.fbx`, material `M_BacNam.mat`. Vị trí `(-1.78, 0.5, -10.18)`. Scale visual 1.22, local Y=0.54.
    *   Cả 2 đều có `NPCCharacter` script, BoxCollider trigger, `NPCProximityOptionsUI` phát hiện tự động khi player đến gần 1.7m.

*   **`InteractionZones`**:
    *   `Shrine` – Bàn thờ tại `(7, 0, -6)`, model `AltarModel` + material `Mat_Altar.mat`, BoxCollider trigger, `AncestralAltar` script.

*   **`DisasterObjects`**: Cha rỗng — `WeatherManager` instantiate `3D_Water_Plane` tại runtime (Y=-1.5f khi idle, dâng lên trong Phase 2).

*   **`Audio`**: Các AudioSource môi trường và nhạc nền.

*   **`_UI`** (Canvas `SurvivalCanvas`):
    *   `SurvivalUIManager` – tạo toàn bộ HUD lúc runtime trong `Awake()`.
    *   `TimeSeasonPanel` – Top-Left `(20, -20)`, size `280x70`, nền nâu sẫm + viền vàng, chứa `dayText` và `phaseText`.
    *   `NghiaTinhPanel` – Top-Left `(20, -100)`, size `280x70`, script `NghiaTinhUI`, tự đổi màu thanh (Đỏ/Vàng/Xanh) theo điểm.
    *   `TutorialQuestPanel` – Top-Left `(20, -220)`, size `280x150`, tạo động bởi `TutorialManager`. Hiển thị nhiệm vụ tutorial hiện tại.
    *   `ResourcePanel` – Bottom-Left `(20, 20)`, size `260x120`, thanh Sức khỏe / Thể lực / Tinh thần, tự động ẩn khi dialogue/shop/ending mở.
    *   `ControlsLegendPanel` – Top-Right `(-20, -120)`, size `220x220`, alpha 0.8f, phím `H` toggle.
    *   `VillageSpeakerBanner` – Banner loa phát thanh xã, fade in/out qua CanvasGroup, tự tắt sau 5 giây.
    *   `EndingPanel` – Màn hình kết thúc cốt truyện, liên kết `EndingManager`.
    *   `NPCProximityPanel` – Panel nổi Roblox-style 380px rộng, bám theo NPC, fade in 0.13s.
    *   `inventoryPanel` – Centered `(0,0)`, `400x320`, dark rustic, slot `80x80` icon `60x60`.
    *   `shopPanel` – Centered `(0,0)`, `500x400` dark blue-brown, 5 rows height=60, icon `56x56`.
    *   `ToastPanel` – Anchored upper-middle `(0.5, 0.85)`, tô màu Regex: vàng NT, xanh Vần công, đỏ cảnh báo.
    *   `dialoguePanel` – Bottom full-width, height 185f, `speakerNameText` + `dialogueContentText`.

---

## 2. Existing Prefabs & Asset Registry

### Tòa nhà & Kiến trúc (`Assets/Prefabs/`)
*   `HoiAnHouse_M2.fbx` – Nhà Hội An rêu phong của Thành.
*   `OTham_Shop/` – Gian hàng lá mái tre.
*   `BacNam_House/` – Nhà mái ngói + chõng tre.
*   `Altar/` – Mô hình bàn thờ.
*   `Well/Well.fbx` – Giếng đá cổ.
*   `Fence/Fence.fbx` + `Fence2.fbx` – Rào tre/gỗ.
*   `Loudspeaker/` – Cột loa phát thanh.
*   `Coracle/` – Thuyền thúng (`Coracle.cs`).
*   `MudPuddle/` – Vũng bùn (`MudPuddle.cs`).
*   `Sandbag/` – Bao cát chặn lũ (`FloodBarrier.cs`).
*   `FloodBoard/` – Tấm chắn lũ.
*   `NonLa/` – Nón lá (trang phục Thành).

### Ô đất Farming (`Assets/Prefabs/Farming/`)
*   `Soil_Rocky.prefab` – Đất sỏi đá cằn (state mặc định ban đầu).
*   `Soil_Clean.prefab` – Đất sạch sau dọn đá (localScale `(106.38, 52.38, 100)`, localPos `(0, 0.077, 0)`).
*   `Soil_Tilled.prefab` – Đất cuốc xới.
*   `Soil_Wet.prefab` – Đất ẩm ướt.

### Cây trồng (`Assets/Prefabs/Crops/SweetPotato/`)
*   `SweetPotato_Stage1` – Giai đoạn cây non.
*   `SweetPotato_Stage2` – Giai đoạn cây lớn sắp thu hoạch.
*   Cả 2 stage: localPos `(0, -0.005 ~ -0.0083, -0.004 ~ -0.0068)`, localRot `(-180, 0, 0)`, scale `(1,1,1)`.

### Động vật (`Assets/Prefabs/Chickens/`, `Assets/Prefabs/Dogs/`)
*   Gà và chó đi loanh quanh via `WanderingAnimal.cs`.

### Thực vật & Thiên nhiên (`Assets/Prefabs/FBX-Ultimate-Nature-Pack/`)
*   `Bamboo/`, `Banana Tree/`, `WoodLog.fbx`, `TreeStump.fbx`, `Rock_1` đến `Rock_7`.

---

## 3. Data Assets (ScriptableObjects — `Assets/Data/`)

### ItemData Assets:
| File | ItemID | Tên | Ghi chú |
|------|--------|-----|---------|
| `Item_FreshCrop.asset` | item_fresh_crop | Khoai Lang Tươi | Decay rate 0.8, dễ thối ở Phase 2 |
| `Item_PreservedCrop.asset` | item_preserved_crop | Khoai Gieo Khô | Decay rate 0, an toàn qua lũ |
| `Item_Incense.asset` | item_incense | Nhang Cúng | Hồi Morale +10 khi thắp tại Altar |
| `Item_Seed.asset` | item_seed | Hạt Giống Khoai Lang | 1 hạt = 1 ô đất |
| `Item_Noodles.asset` | item_noodles | Mì Tôm Cứu Trợ | Hồi Stamina, cấp tự động khi lên nóc nhà |
| `Item_sandbag.asset` | item_sandbag | Bao Cát | Chặn nước lũ qua `FloodBarrier` |
| `Item_flood_board.asset` | item_flood_board | Tấm Chắn Lũ | Ván gỗ ép ngăn nước tràn sân |
| `Item_non_la.asset` | item_non_la | Nón Lá | Trang phục bảo vệ thời tiết |

### CropData Assets:
*   `Crop_KhoaiLang.asset` – Chín sau 5 ngày. HarvestedItem = `Item_FreshCrop`. Model Stage1 = `SweetPotato_Stage1`, Stage2 = `SweetPotato_Stage2`.

---

## 4. Known Working Features (Các chức năng hoạt động tốt)

*   **Tutorial 2 giai đoạn:** Giai đoạn 1 (IntroQuests: Gặp cả 4 NPC O Thắm + Bác Năm + Cụ Bảy + Bé Tí) → Slideshow farming → Giai đoạn 2 (FarmingTutorial: Dọn đá, Gieo hạt, Tưới nước). `TutorialQuestPanel` HUD hiển thị checkbox nhiệm vụ real-time.
*   **Vòng lặp canh tác hoàn chỉnh:** Di chuyển → Dọn đá (`ActionClearRocks`) → Gieo hạt bulk/single → Tưới nước → Thu hoạch (×2 nếu đất Phù Sa) → HUD Toast.
*   **Smart Bulk Planting:** Dialog popup tính tự động số ô có thể trồng và hạt giống còn lại, cho chọn trồng hàng loạt hoặc từng ô.
*   **Flood Roof Survival:** Khi lũ > 1.5f trong Phase 2, Thành tự di tản lên nóc nhà `(0, 3.5, -10)`, nhận 5 mì tôm cứu trợ. Nghỉ ngơi trên nóc phục hồi Stamina/Morale/nhiệt.
*   **Bếp Ga (KitchenHearth):** [E] mở dialog: Sấy khô khoai (2 Tươi → 1 Khô), hoặc Nấu ăn (1 Tươi → +15 Stamina).
*   **Bàn thờ (AncestralAltar):** [E] thắp 1 Nhang → +10 Morale.
*   **Thuyền thúng (Coracle):** [E] lên/xuống thuyền, di chuyển tốc độ 2.5f trên nước lũ.
*   **Vũng bùn (MudPuddle):** [E] tương tác, làm giảm tốc độ di chuyển Thành.
*   **Proximity UI NPC (Roblox-style):** Panel nổi tự bám WorldToScreenPoint trên đầu NPC (O Thắm, Bác Năm, Cụ Bảy, Bé Tí) trong phạm vi 1.7m. Fade 0.13s. Nút 1/2/3 + click.
*   **Phase-based Community Events:** O Thắm Phase 1 (hỗ trợ chuẩn bị trước bão), Bác Năm Phase 1 (chằng chống nhà), cả hai Phase 2 (tái thiết). Cụ Bảy Phase 2 (cứu trợ lương thực). Mỗi sự kiện chỉ hoàn thành 1 lần/session.
*   **NPC Quest Marker:** Dấu `!` vàng bounce trên đầu NPC có event chưa hoàn thành, ẩn khi player < 1.7m.
*   **NPC Look-at & Return-to-Idle:** NPC Slerp quay về hướng player khi tương tác, rồi Slerp về `defaultRotation` khi player đi.
*   **Loa phát thanh xã & Phase Banner:** Banner tiếng Việt fade in/out 5s tự động theo Phase/Day.
*   **Hệ thống Nghĩa Tình:** Panel Top-Left, thanh slider đổi màu Đỏ/Vàng/Xanh.
*   **F1 Debug Panel:** Jump Phase 1-2, +5 Hạt giống, +5 Thực phẩm, Set Nghĩa Tình 20/50/80, Show Ending. Cột Farming Debug tại x=540.
*   **Inventory & Shop UI:** Slot `80x80`, icon `60x60`, viền vàng rustic. Shop row `60` height, Buy xanh / Sell cam.
*   **Controls Legend (H):** Panel Top-Right, toggle ẩn/hiện bằng phím `H`.
*   **Kết cục 3 loại:** < 40 Sad, ≥ 40 Normal, ≥ 80 Best.

---

## 5. Known Broken Features & Temporary Hacks

### Lỗi đã biết
*   **BoundaryElement scale:** Các tảng đá biên giới vẫn có scale import nhỏ, hoạt động như tường vô hình.
*   **Menu Settings Input Bindings:** Phần hiển thị gán phím trong FrameworkMainMenuUI vẫn dùng KeyCode legacy, chưa đồng bộ hoàn toàn với New Input System.
*   **Camera Collision góc hẹp:** Camera SphereCast có thể thu ngắn đột ngột khi sát mái Thanh_House.

### Vá tạm thời
*   **Rigidbody Constraint Y + useGravity=false:** Player di chuyển phẳng XZ, không rơi qua địa hình lồi lõm.
*   **Road MeshColliders disabled:** MeshCollider các đoạn đường đi thủ công disable để tránh kẹt.
*   **House_OTham_PLACEHOLDER:** Instance cũ deactivated, đã thay bằng model thực `OTham_Shop`.

---

## 6. Editor Scripts (Assets/Scripts/Editor/)

Các script Editor-only (không build vào game) để setup scene nhanh:
*   `SetupSolidRuongEditor.cs` – Tạo 4x3 SoilCell grid + 12 fence segments tự động.
*   `SetupAltar.cs`, `SetupBacNamHouse.cs`, `SetupOThamShop.cs`, `SetupThanhHouse.cs` – Setup từng khu vực.
*   `SetupAnimals.cs` – Đặt gà + chó.
*   `SetupCoracle.cs`, `SetupLoudspeaker.cs` – Thiết bị đặc biệt.
*   `LowPolyTerrainDeformer.cs`, `TerrainGroundingSweep.cs` – Địa hình.
*   `SetupSoilVisuals.cs`, `SetupSurvivalAssets.cs` – Assets tổng hợp.
*   `TutorialTextureImporter.cs` – Import ảnh tutorial.

---

## 7. Key World Coordinates (Village_Demo.unity)

| Object | Position | Rotation | Ghi chú |
|--------|----------|----------|---------|
| Player | `(0, 0.5, -6)` | default | Spawn point |
| Thanh_House | `(0, 0, -15.5)` | Y=180° | BoxCollider `(8×5.57×9.59)` |
| bep_gas | `(5.89, 0.21, -8.69)` | — | Dưới mái hiên |
| OTham_Shop | `(8.0, 0.0, 10.28)` | — | |
| BacNam_House | `(-12.0, 0.0, 8.5)` | — | |
| Village_Well | `(0, 0.63, 3.0)` | — | BoxCollider gọn `1.4×1.5×1.4m` |
| Village_Speaker | `(0, 1.75, 6)` | — | |
| Shrine/Altar | `(7, 0, -6)` | — | |
| NPC_OTham | `(3.53, 0.5, -9.49)` | — | Visual scale 1.30 |
| NPC_BacNam | `(-1.78, 0.5, -10.18)` | — | Visual scale 1.22 |
| NPC_CuBay | `(-14.20, 0.00, 4.95)` | — | Gần Shrine |
| NPC_BeTi | `(-2.72, 0.00, 5.00)` | — | Gần Well |
| SoilCell Grid 4x3 | Center `(-8.0, 0.13, -10.0)` | Flat XZ | Spacing 1.5m, tổng cộng 12 SoilCells |
| FarmingPlot | `(-6, 0.01, -11)` | — | Visual bounding only |
