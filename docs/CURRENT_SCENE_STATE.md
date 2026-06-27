# Current Scene State: Đất Cày Lên Sỏi Đá

Tài liệu này cung cấp ảnh chụp thực tế (snapshot) trạng thái hiện tại của cảnh 3D Unity (`Village_Demo.unity`) nhằm giúp các AI Agent và lập trình viên nắm bắt chính xác cấu trúc thực tế của dự án, giảm thiểu các phán đoán sai lệch hoặc lập trình trùng lặp tính năng[cite: 27].

> **Scene chính hiện tại:** `Assets/Scenes/Village_Demo.unity` (Build Index 0)[cite: 27]  
> `SampleScene.unity` chỉ còn vai trò sandbox phát triển, **không** dùng cho demo/build[cite: 27].

---

## 1. Actual Scene Hierarchy (Cấu trúc phân cấp thực tế)

### Root Objects trong `Village_Demo.unity`[cite: 27]:

*   **`_Managers`**: GameObject rỗng chứa các script Singleton điều phối[cite: 27]:
    *   `GameManager` – Chu kỳ ngày/đêm, đếm thời gian, phát sự kiện `OnDayChanged` / `OnPhaseChanged` (với 2 Phase chính)[cite: 27].
    *   `PlayerStats` – Máu, Thể lực, Tinh thần, Xu, Nhiệt lạnh/nóng stress[cite: 27].
    *   `WeatherManager` – Lerp thời tiết, dâng nước lũ, kiểm soát hạt mưa[cite: 27].
    *   `StorageManager` – Quản lý kho chứa, theo dõi thối nông sản tươi dưới độ ẩm cao[cite: 25, 27].
    *   `CommunityManager` – Điểm Nghĩa Tình (`GlobalKarma`), công Vần công, tracking sự kiện khẩn cấp và tái thiết[cite: 24, 27].
    *   `EndingManager` – Quản lý màn hình kết cục dựa trên Nghĩa Tình[cite: 27].
    *   `AudioManager` – Âm thanh nền và SFX[cite: 27].
    *   `TutorialManager` – Singleton tutorial 2 giai đoạn (IntroQuests dắt đủ 4 NPC $\rightarrow$ FarmingTutorial)[cite: 24, 27].

*   **`Player`**: Nhân vật chính Thành[cite: 27].
    *   *Components:* `Rigidbody` (useGravity=false, Freeze Rotation X/Y/Z, Freeze Position Y), `BoxCollider`, `PlayerController` script (namespace `SownInStone.Core`)[cite: 27].
    *   *Child object `Player_Base`:* Model 3D `indonesian_farmer_pak_tani.glb` + `Animator`. Phụ kiện `NonLa` (Nón lá) có thể equip vào đây[cite: 27].
    *   Spawn position: Vị trí mặc định sân nhà Thành `(0, 0.5, -6)`[cite: 25, 27].

*   **`Main Camera`**:
    *   *Components:* Camera, `CameraFollow3D` script (Target → `Player`)[cite: 27].
    *   Orbit Roblox-style: Distance=6f, defaultPitch=25°, minPitch=12°, maxPitch=55°, zoomRange 3–9. SphereCast collision safety bật[cite: 27].

*   **`Lighting`**:
    *   `Directional Light` – Rotation `(45, 30, 0)`, warm sunlight Intensity 1.1[cite: 27].
    *   `Global Volume` – URP Post-processing[cite: 27].

*   **`Environment`**:
    *   `Ground_Main` (Plane 3D + MeshCollider, material `Ground_LowPoly.mat`, màu `#7CA66D`)[cite: 27].
    *   `_Environment/Houses`[cite: 27]:
        *   `Thanh_House` – `HoiAnHouse_M2.fbx` tại `(0, 0, -15.5)`, xoay Y=180°. BoxCollider size `(8.00, 5.57, 9.59)`[cite: 27].
            *   `Roof_Anchor_Nodes` – nhóm Transform marker cho Phase 2 roof refuge[cite: 27]:
                *   `Roof_Anchor_PlayerRefuge` – điểm đứng/trú chính của Thành trên mái nhà[cite: 27].
                *   `Roof_Anchor_AidDrop` – điểm nhận mì tôm/cứu trợ[cite: 27].
                *   `Roof_Anchor_SandbagSlots` – vị trí dùng `item_sandbag` để neo/chèn ngói rung khi trú bão[cite: 27].
        *   `OTham_Shop` – gian hàng lá mái tre tại `(8.0, 0.0, 10.28)`[cite: 27].
            *   `Wall_Repair_Node` – Transform marker Phase 2 cho sửa tường sập bằng `item_flood_board`[cite: 27].
        *   `BacNam_House` – nhà mái ngói + chõng tre tại `(-12.0, 0.0, 8.5)`[cite: 27].
            *   `Wall_Repair_Node` – Transform marker Phase 2 cho sửa tường sập bằng `item_flood_board`[cite: 27].
        *   `bep_gas` – bếp ga dưới mái hiên Thanh_House tại `(5.89, 0.21, -8.69)`, BoxCollider trigger size `(0.03, 0.03, 0.03)`, scale `(40,40,40)`, đính kèm `KitchenHearth` script (`IInteractable`)[cite: 27].
    *   `_Environment/NPCs`[cite: 27]:
        *   `NPC_CuBay` – Mixamo `cu_bay@Old Man Idle.fbx` tại `(-14.20, 0.00, 4.95)`, BoxCollider trigger `(1.7, 1.7, 1.7)`[cite: 27].
        *   `NPC_BeTi` – Mixamo `be_ti@Breathing Idle.fbx` tại `(-2.72, 0.00, 5.00)`, BoxCollider trigger `(1.7, 1.7, 1.7)`[cite: 27].
    *   `Village_Well` – tại `(0, 0, 3)`. BoxCollider local center `(0.000043, -0.001367, 0.00375)`, local size `(0.007, 0.007, 0.0075)` với scale 200[cite: 27].
    *   `Village_Speaker` – cột loa tại `(0, 1.75, 6)`[cite: 27].
    *   Fences – 12 đoạn rào `Fence2.fbx` đặt tự động bao quanh ruộng bởi `SetupSolidRuongEditor.cs`[cite: 27].
    *   Bamboo, BananaTree, rocks, props trang trí[cite: 27].

*   **`FarmingArea`**:
    *   `FarmingPlot` – box hình nền nâu tại `(-6, 0.01, -11)` (Edit Mode only)[cite: 27].
    *   12 `SoilCell` con (solid lưới 4x3, spacing 1.5m) trung tâm tại `(-8.0, 0.13, -10.0)`[cite: 27].
        *   Row 1: `SoilCell_Grid1`, `SoilCell_Grid2`, `SoilCell_Grid3`, `SoilCell_Grid4`[cite: 27].
        *   Row 2: `SoilCell_Grid5`, `SoilCell_Grid6`, `SoilCell_Grid7`, `SoilCell_Grid8`[cite: 27].
        *   Row 3: `SoilCell_Grid9`, `SoilCell_Grid10`, `SoilCell_Grid11`, `SoilCell_Grid12`[cite: 27].
        *   Mỗi cell: BoxCollider trigger `(2.0, 0.3, 2.0)`[cite: 27].
        *   Mỗi `SoilCell_Grid*` là một entity gameplay độc lập; `SoilCell` parent/helper tự động link qua `Awake()` theo khoảng cách XZ < 6m để hỗ trợ bulk actions/debug[cite: 27].
        *   3D Visuals: `Soil_Rocky`, `Soil_Clean`, `Soil_Tilled`, `Soil_Wet` prefabs từ `Assets/Prefabs/Farming/`[cite: 27].
        *   Target highlight: khung viền vàng gold mờ (`SetHighlight(bool)`).

*   **`NPCs`** (Root):
    *   `NPC_OTham` – FBX `O_Tham.fbx`, material `M_OTham.mat`. Vị trí `(3.53, 0.5, -9.49)`. Scale visual 1.30, local Y=0.61[cite: 27].
    *   `NPC_BacNam` – FBX `Bac_Nam.fbx`, material `M_BacNam.mat`. Vị trí `(-1.78, 0.5, -10.18)`. Scale visual 1.22, local Y=0.54[cite: 27].
    *   Cả 2 đều có `NPCCharacter` script, BoxCollider trigger, `NPCProximityOptionsUI` phát hiện tự động khi player đến gần 1.7m[cite: 27].

*   **`InteractionZones`**:
    *   `Shrine` – Bàn thờ tại `(7, 0, -6)`, model `AltarModel` + material `Mat_Altar.mat`, BoxCollider trigger, `AncestralAltar` script[cite: 27].

*   **`DisasterObjects`**: Cha rỗng — `WeatherManager` instantiate `3D_Water_Plane` tại runtime (Y=-1.5f khi idle, dâng lên trong Phase 2)[cite: 27].

*   **`Audio`**: Các AudioSource môi trường và nhạc nền[cite: 27].

*   **`_UI`** (Canvas `SurvivalCanvas`)[cite: 27]:
    *   `SurvivalUIManager` – tạo toàn bộ HUD lúc runtime trong `Awake()`[cite: 27].
    *   `TimeSeasonPanel` – Top-Left `(20, -20)`, size `280x70`, nền nâu sẫm + viền vàng, chứa `dayText` và `phaseText` (Hiển thị Phase 1 / Phase 2)[cite: 25, 27].
    *   `NghiaTinhPanel` – Top-Left `(20, -100)`, size `280x70`, script `NghiaTinhUI`, tự đổi màu thanh (Đỏ/Vàng/Xanh) theo điểm[cite: 27].
    *   `TutorialQuestPanel` – Top-Left `(20, -220)`, size `280x150`, tạo động bởi `TutorialManager`. Hiển thị checklist theo dõi 4 NPC và hoạt động farming[cite: 25, 27].
    *   `ResourcePanel` – Bottom-Left `(20, 20)`, size `260x120`, thanh Sức khỏe / Thể lực / Tinh thần, tự động ẩn khi dialogue/shop/ending mở[cite: 27].
    *   `ControlsLegendPanel` – Top-Right `(-20, -120)`, size `220x220`, alpha 0.8f, phím `H` toggle[cite: 27].
    *   `VillageSpeakerBanner` – Banner loa phát thanh xã, fade in/out qua CanvasGroup, tự tắt sau 5 giây[cite: 27].
    *   `EvacuationTimerPanel` – Runtime HUD layer Top-Center cho Mid-Phase Countdown 45 giây. Hiển thị thời gian còn lại và trạng thái sơ tán 4 dân làng[cite: 25, 27].
    *   `EndingPanel` – Màn hình kết thúc cốt truyện, liên kết `EndingManager`[cite: 27].
    *   `NPCProximityPanel` – Panel nổi Roblox-style 380px rộng, bám theo NPC, fade in 0.13s[cite: 27].
    *   `inventoryPanel` – Centered `(0,0)`, `400x320`, dark rustic, slot `80x80` icon `60x60`[cite: 27].
    *   `shopPanel` – Centered `(0,0)`, `500x400` dark blue-brown, 5 rows height=60, icon `56x56`[cite: 27].
    *   `ToastPanel` – Anchored upper-middle `(0.5, 0.85)`, tô màu Regex: vàng NT, xanh Vần công, đỏ cảnh báo[cite: 27].
    *   `dialoguePanel` – Bottom full-width, height 185f, `speakerNameText` + `dialogueContentText`[cite: 27].

---

## 2. Existing Prefabs & Asset Registry

### Tòa nhà & Kiến trúc (`Assets/Prefabs/`)[cite: 27]
*   `HoiAnHouse_M2.fbx` – Nhà HoiAn ngói cũ của Thành[cite: 27].
*   `OTham_Shop/` – Gian hàng lá mái tre[cite: 27].
*   `BacNam_House/` – Nhà mái ngói + chõng tre[cite: 27].
*   `Altar/` – Mô hình bàn thờ[cite: 27].
*   `Well/Well.fbx` – Giếng đá cổ[cite: 27].
*   `Fence/Fence.fbx` + `Fence2.fbx` – Rào tre/gỗ[cite: 27].
*   `Loudspeaker/` – Cột loa phát thanh[cite: 27].
*   `Coracle/` – Thuyền thúng (`Coracle.cs`)[cite: 27].
*   `MudPuddle/` – Vũng bùn (`MudPuddle.cs`)[cite: 27].
*   `Sandbag/` – Bao cát chặn lũ (`FloodBarrier.cs`)[cite: 27].
*   `FloodBoard/` – Tấm chắn lũ[cite: 27].
*   `NonLa/` – Nón lá (trang phục Thành)[cite: 27].

### Ô đất Farming (`Assets/Prefabs/Farming/`)[cite: 27]
*   `Soil_Rocky.prefab` – Đất sỏi đá cằn (state mặc định ban đầu)[cite: 27].
*   `Soil_Clean.prefab` – Đất sạch sau dọn đá (localScale `(106.38, 52.38, 100)`, localPos `(0, 0.077, 0)`)[cite: 27].
*   `Soil_Tilled.prefab` – Đất cuốc xới[cite: 27].
*   `Soil_Wet.prefab` – Đất ẩm ướt[cite: 27].

### Cây trồng (`Assets/Prefabs/Crops/SweetPotato/`)[cite: 27]
*   `SweetPotato_Stage1` – Giai đoạn cây non[cite: 27].
*   `SweetPotato_Stage2` – Giai đoạn cây lớn sắp thu hoạch[cite: 27].
*   Cả 2 stage: localPos `(0, -0.005 ~ -0.0083, -0.004 ~ -0.0068)`, localRot `(-180, 0, 0)`, scale `(1,1,1)`[cite: 27].

### Động vật (`Assets/Prefabs/Chickens/`, `Assets/Prefabs/Dogs/`)[cite: 27]
*   Gà và chó đi loanh quanh via `WanderingAnimal.cs`[cite: 27].

### Thực vật & Thiên nhiên (`Assets/Prefabs/FBX-Ultimate-Nature-Pack/`)[cite: 27]
*   `Bamboo/`, `Banana Tree/`, `WoodLog.fbx`, `TreeStump.fbx`, `Rock_1` đến `Rock_7`[cite: 27].

---

## 3. Data Assets (ScriptableObjects — `Assets/Data/`)[cite: 27]

### ItemData Assets[cite: 27]:
| File | ItemID | Tên | Ghi chú |
|------|--------|-----|---------|
| `Item_FreshCrop.asset` | item_fresh_crop | Khoai Lang Tươi | Decay rate 0.8, dễ thối ở Phase 2[cite: 27] |
| `Item_PreservedCrop.asset` | item_preserved_crop | Khoai Gieo Khô | Decay rate 0, an toàn qua lũ[cite: 27] |
| `Item_Incense.asset` | item_incense | Nhang Cúng | Hồi Morale +10 khi thắp tại Altar[cite: 27] |
| `Item_Seed.asset` | item_seed | Hạt Giống Khoai Lang | 1 hạt = 1 ô đất[cite: 27] |
| `Item_Noodles.asset` | item_noodles | Mì Tôm Cứu Trợ | Hồi Stamina, cấp tự động khi lên nóc nhà[cite: 27] |
| `Item_sandbag.asset` | item_sandbag | Bao Cát | Phase 2 roof defense: neo/chèn ngói rung tại `Roof_Anchor_SandbagSlots`[cite: 27] |
| `Item_flood_board.asset` | item_flood_board | Tấm Chắn Lũ | Phase 2 structural repair: sửa tường sập tại `Wall_Repair_Node` của OTham_Shop/BacNam_House[cite: 27] |
| `Item_non_la.asset` | item_non_la | Nón Lá | Trang phục bảo vệ thời tiết[cite: 27] |

### CropData Assets[cite: 27]:
*   `Crop_KhoaiLang.asset` – Chín sau 5 ngày. HarvestedItem = `Item_FreshCrop`. Model Stage1 = `SweetPotato_Stage1`, Stage2 = `SweetPotato_Stage2`[cite: 27].

---

## 4. Key World Coordinates (Village_Demo.unity)[cite: 27]

| Object | Position | Rotation | Ghi chú |
|--------|----------|----------|---------|
| Player | `(0, 0.5, -6)` | default | Spawn point[cite: 27] |
| Thanh_House | `(0, 0, -15.5)` | Y=180° | BoxCollider `(8×5.57×9.59)`[cite: 27] |
| bep_gas | `(5.89, 0.21, -8.69)` | — | Dưới mái hiên[cite: 27] |
| OTham_Shop | `(8.0, 0.0, 10.28)` | — | |[cite: 27] |
| BacNam_House | `(-12.0, 0.0, 8.5)` | — | |[cite: 27] |
| Village_Well | `(0, 0.63, 3.0)` | — | BoxCollider gọn `1.4×1.5×1.4m`[cite: 27] |
| Village_Speaker | `(0, 1.75, 6)` | — | |[cite: 27] |
| Shrine/Altar | `(7, 0, -6)` | — | |[cite: 27] |
| NPC_OTham | `(3.53, 0.5, -9.49)` | — | Visual scale 1.30[cite: 27] |
| NPC_BacNam | `(-1.78, 0.5, -10.18)` | — | Visual scale 1.22[cite: 27] |
| NPC_CuBay | `(-14.20, 0.00, 4.95)` | — | Gần Shrine[cite: 27] |
| NPC_BeTi | `(-2.72, 0.00, 5.00)` | — | Gần Well[cite: 27] |
| SoilCell Grid 4x3 | Center `(-8.0, 0.13, -10.0)` | Flat XZ | Spacing 1.5m, tổng cộng 12 standalone SoilCells (`SoilCell_Grid1` → `SoilCell_Grid12`)[cite: 27] |
| Roof_Anchor_Nodes | Child of `Thanh_House` | — | Roof refuge / aid drop / sandbag slot markers for Phase 2[cite: 27] |
| Wall_Repair_Node | Child of `OTham_Shop`, `BacNam_House` | — | Post-flood wall repair targets using `item_flood_board`[cite: 27] |
| FarmingPlot | `(-6, 0.01, -11)` | — | Visual bounding only[cite: 27] |