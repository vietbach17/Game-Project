# Scene Structure: Đất Cày Lên Sỏi Đá

Tài liệu này mô tả cấu trúc Hierarchy chính thức của bản demo `Village_Demo.unity`. **Đây là scene demo duy nhất** — mọi thay đổi liên quan đến gameplay, prefab placement và inspector reference đều phải thực hiện trên scene này.

---

## 1. Hierarchy Root Layout

```
Village_Demo.unity
├── _Managers          ← Các Singleton Manager Script
├── _UI                ← Canvas HUD và UI popup  
├── Player             ← Nhân vật chính Thành
├── Main Camera        ← Camera orbit Roblox-style
├── Lighting           ← Directional Light + Global Volume
├── Environment        ← Địa hình, nhà cửa, props trang trí
├── FarmingArea        ← Khu ruộng trồng khoai lang
├── NPCs               ← NPC O Thắm và Bác Năm (chính)
├── InteractionZones   ← Bàn thờ tổ tiên (Shrine/Altar)
├── DisasterObjects    ← Container runtime flood water plane
└── Audio              ← AudioSource môi trường và nhạc nền
```

---

## 2. Detailed Sub-Hierarchy

### A. `_Managers`
GameObject rỗng chứa tất cả Singleton managers:

| Component Script | Namespace | Chức năng |
|-----------------|-----------|-----------|
| `GameManager` | `SownInStone.Core` | Chu kỳ ngày/đêm, chuyển các mốc runtime `LapNghiep`, `GioLao`, `ChuanBiBao`, `MuaBao`, `PhuSa`, OnDayChanged, OnPhaseChanged. Design gom thành 2 phase chính: Before the Storm / After the Storm |
| `PlayerStats` | `SownInStone.Core` | Health, Stamina, Morale, Coins, HeatStress, ColdStress |
| `WeatherManager` | `SownInStone.Weather` | Lerp thời tiết theo các mốc runtime trong 2 phase chính, FloodLevel, hạt mưa |
| `StorageManager` | `SownInStone.Storage` | Kho đồ: AddItem/RemoveItem, decay nông sản tươi |
| `CommunityManager` | `SownInStone.Community` | GlobalKarma (Nghĩa Tình), VanCong, event completion tracking |
| `EndingManager` | `SownInStone.UI` | IsEndingShown, 3 kết cục dựa trên Nghĩa Tình |
| `AudioManager` | `SownInStone.Audio` | PlaySFX(key), missingClips cache |
| `TutorialManager` | `SownInStone` | Tutorial 2 giai đoạn, DontDestroyOnLoad |

---

### B. `_UI`
Canvas `SurvivalCanvas` với `SurvivalUIManager` tạo HUD tại runtime trong `Awake()`.

**HUD Panels (runtime-generated):**
```
SurvivalCanvas (SurvivalUIManager)
├── TimeSeasonPanel         ← Top-Left (20, -20) 280×70 | dayText + phaseText
├── NghiaTinhPanel          ← Top-Left (20, -100) 280×70 | NghiaTinhUI, slider đổi màu
├── TutorialQuestPanel      ← Top-Left (20, -220) 280×150 | TutorialManager tạo động
├── ResourcePanel           ← Bottom-Left (20, 20) 260×120 | HP/Stamina/Morale bars
├── ControlsLegendPanel     ← Top-Right (-20, -120) 220×220 | H toggle, alpha 0.8
├── ToastPanel              ← Upper-Middle (0.5, 0.85) | Regex-colored notifications
├── NPCProximityPanel       ← WorldToScreen bám NPC | 380px, fade 0.13s
├── VillageSpeakerBanner    ← Centered | CanvasGroup fade 5s auto-hide
├── inventoryPanel          ← Center (0,0) 400×320 | I/Tab toggle
├── shopPanel               ← Center (0,0) 500×400 | O Thắm shop
├── dialoguePanel           ← Bottom full-width h=185 | speaker + content + buttons
61: └── EndingPanel             ← Fullscreen | EndingManager link
```

**Lưu ý quan trọng:**
- `SurvivalUIManager.Awake()` tự động attach `NPCProximityOptionsUI` và `NPCQuestMarkerUI` vào chính nó — **không cần thêm thủ công vào scene**.
- `NghiaTinhPanel` được reparent vào `SurvivalUI` lúc runtime để đồng bộ coordinate space.

---

### C. `Player`
```
Player (PlayerController, Rigidbody, BoxCollider)
└── Player_Base
    └── [model] indonesian_farmer_pak_tani.glb (Animator)
```
- Rigidbody: `useGravity=false`, `FreezePositionY`, `FreezeRotationXYZ`.
- Spawn: `(0, 0.5, -6)`.
- interactRadius: `2.5f`.

---

### D. `Environment`
```
Environment
├── Ground_Main              ← Plane 3D, MeshCollider, Ground_LowPoly.mat (#7CA66D)
├── _Environment
│   ├── Houses
│   │   ├── Thanh_House      ← HoiAnHouse_M2.fbx tại (0,0,-15.5) Y=180°
│   │   │   └── bep_gas      ← KitchenHearth IInteractable tại (5.89, 0.21, -8.69)
│   │   ├── OTham_Shop       ← tại (8.0, 0.0, 10.28)
│   │   └── BacNam_House     ← tại (-12.0, 0.0, 8.5)
│   └── NPCs
│       ├── NPC_CuBay        ← cu_bay@Old Man Idle.fbx tại (-14.20, 0.00, 4.95)
│       └── NPC_BeTi         ← be_ti@Breathing Idle.fbx tại (-2.72, 0.00, 5.00)
├── Village_Well             ← (0, 0.63, 3.0), BoxCollider gọn 1.4×1.5×1.4m
├── Village_Speaker          ← (0, 1.75, 6)
├── Fences                   ← 12 đoạn Fence2.fbx tự động bởi SetupSolidRuongEditor
└── [Bamboo, BananaTree, Rocks, Props...]
```

---

### E. `FarmingArea`
```
FarmingArea
├── FarmingPlot              ← Visual bounding box (0.01m cao) - Edit Mode only
├── SoilCell                 ← Parent field (auto-detect bởi SoilCell.Awake)
├── SoilCell_Grid1           ← 3D cell
├── SoilCell_Grid2           ← 3D cell  
├── SoilCell_Grid3           ← ...
├── SoilCell_Grid4           ← ...
├── SoilCell_Grid5           ← ...
├── SoilCell_Grid6           ← ...
├── SoilCell_Grid7           ← ...
├── SoilCell_Grid8           ← ...
├── SoilCell_Grid9           ← ...
├── SoilCell_Grid10          ← ...
├── SoilCell_Grid11          ← ...
└── SoilCell_Grid12          ← (12 cells total, lưới 4x3, spacing 1.5m)
```
- Mỗi SoilCell Grid: BoxCollider trigger `(2.0, 0.3, 2.0)`, xoay flat XZ.
- 3D Visuals: `Soil_Rocky` / `Soil_Clean` / `Soil_Tilled` / `Soil_Wet` prefabs.
- SoilCell cha tự động link với các con trong `Awake()` qua tên+khoảng cách XZ.

---

### F. `NPCs` (Root)
```
NPCs
├── NPC_OTham   (NPCCharacter, BoxCollider trigger) ← (3.53, 0.5, -9.49)
│   └── Visual (O_Tham.fbx, M_OTham.mat, scale 1.30)
└── NPC_BacNam  (NPCCharacter, BoxCollider trigger) ← (-1.78, 0.5, -10.18)
    └── Visual (Bac_Nam.fbx, M_BacNam.mat, scale 1.22)
```
- Cả 2 NPC có `StoryCharacterType` enum: `OTham` / `BacNam`.
- Proximity detection: 1.7m.
- NPC phụ (`CuBay`, `BeTi`) nằm dưới `Environment/_Environment/NPCs/`.

---

### G. `InteractionZones`
```
InteractionZones
└── Shrine                   ← (7, 0, -6)
    ├── AltarModel           ← Mat_Altar.mat
    └── [BoxCollider trigger, AncestralAltar script]
```

---

### H. `DisasterObjects`
- Cha rỗng (empty parent).
- `WeatherManager` runtime-instantiate `3D_Water_Plane` vào đây tại Y=-1.5f khi khởi động, dâng lên trong Phase 2 (Sau Bão).
- Coracle, MudPuddle, FloodBarrier có thể được đặt thêm đây hoặc trong Environment.

---

## 3. Naming Conventions

| Loại | Quy chuẩn | Ví dụ |
|------|-----------|-------|
| Root Objects | PascalCase noun | `Player`, `Environment`, `FarmingArea` |
| NPC GameObjects | Prefix `NPC_` | `NPC_OTham`, `NPC_BacNam`, `NPC_CuBay` |
| Prefabs | PascalCase | `Soil_Rocky`, `SweetPotato_Stage1` |
| Data Assets | `Item_` / `Crop_` prefix | `Item_Seed.asset`, `Crop_KhoaiLang.asset` |
| Materials | `M_` prefix | `M_BacNam.mat`, `Ground_LowPoly.mat` |
| Scripts | PascalCase | `PlayerController.cs`, `SoilCell.cs` |
| Interactive Zones | Descriptive nouns | `Shrine`, `bep_gas`, `Village_Well` |
| Static Mesh Colliders | Mark **Static** trong Inspector | Thanh_House, Village_Well |

---

## 4. Important Architecture Rules

1. **Scene chính là `Village_Demo.unity`** — Mọi thay đổi gameplay thực hiện ở đây.
2. **`SampleScene.unity` là sandbox** — Chỉ dùng để thử nghiệm asset mới.
3. **Không thêm SurvivalUIManager vào scene thủ công** — Script tự tạo HUD trong `Awake()`.
4. **Không thêm NPCProximityOptionsUI / NPCQuestMarkerUI vào scene** — Tự wire trong SurvivalUIManager.
5. **TutorialManager** có `DontDestroyOnLoad` — Đặt trong `_Managers`, không đặt ở nơi khác.
6. **Tất cả Managers là Singleton** — Không được tạo duplicate instance trong scene.
