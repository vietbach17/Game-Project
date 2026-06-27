# Scene Structure: Đất Cày Lên Sỏi Đá

Tài liệu này mô tả cấu trúc Hierarchy chính thức của bản demo `Village_Demo.unity`[cite: 25]. **Đây là scene demo duy nhất** — mọi thay đổi liên quan đến gameplay, prefab placement và inspector reference đều phải thực hiện trên scene này[cite: 25].

---

## 1. Hierarchy Root Layout
Village_Demo.unity
├── _Managers          ← Các Singleton Manager Script
├── _UI                ← Canvas HUD và UI popup

├── Player             ← Nhân vật chính Thành
├── Main Camera        ← Camera orbit Roblox-style
├── Lighting           ← Directional Light + Global Volume
├── Environment        ← Địa hình, nhà cửa, props trang trí
├── FarmingArea        ← Khu ruộng trồng khoai lang (Lưới 4x3)
├── NPCs               ← NPC O Thắm, Bác Năm (chính)
├── InteractionZones   ← Bàn thờ tổ tiên (Shrine/Altar) và các điểm Neo Tái Thiết
├── DisasterObjects    ← Container runtime flood water plane
└── Audio              ← AudioSource môi trường và nhạc nền
---

## 2. Detailed Sub-Hierarchy

### A. `_Managers`
GameObject rỗng chứa tất cả Singleton managers[cite: 25]:

| Component Script | Namespace | Chức năng |
|-----------------|-----------|-----------|
| `GameManager` | `SownInStone.Core` | Chu kỳ ngày/đêm, chuyển các mốc runtime `LapNghiep`, `ChuanBiBao`, `MuaBao`, `PhuSa`, OnDayChanged, OnPhaseChanged[cite: 25]. Thiết kế gom thành 2 phase chính: Before the Storm / After the Storm[cite: 25]. Gió Lào được loại bỏ hoàn toàn[cite: 24]. |
| `PlayerStats` | `SownInStone.Core` | Health, Stamina, Morale, Coins, HeatStress, ColdStress[cite: 25]. |
| `WeatherManager` | `SownInStone.Weather` | Lerp thời tiết theo các mốc runtime trong 2 phase chính, FloodLevel, hạt mưa[cite: 25]. |
| `StorageManager` | `SownInStone.Storage` | Kho đồ: AddItem/RemoveItem, decay nông sản tươi dưới độ ẩm cao[cite: 25]. |
| `CommunityManager` | `SownInStone.Community` | GlobalKarma (Nghĩa Tình), VanCong, event completion tracking (cứu hộ khẩn cấp và tái thiết)[cite: 24, 25]. |
| `EndingManager` | `SownInStone.UI` | IsEndingShown, 3 kết cục dựa trên điểm Nghĩa Tình[cite: 25]. |
| `AudioManager` | `SownInStone.Audio` | PlaySFX(key), phát nhạc nền và hiệu ứng mưa rít, sấm sét, loa rè[cite: 25]. |
| `TutorialManager` | `SownInStone` | Tutorial 2 giai đoạn (IntroQuests dắt đủ 4 NPC $\rightarrow$ FarmingTutorial), DontDestroyOnLoad[cite: 24, 25]. |

---

### B. `_UI`
Canvas `SurvivalCanvas` với `SurvivalUIManager` tạo HUD tại runtime trong `Awake()`[cite: 25].

**HUD Panels (runtime-generated):**
SurvivalCanvas (SurvivalUIManager)
├── TimeSeasonPanel         ← Top-Left (20, -20) 280×70 | dayText + phaseText (Hiển thị Phase 1 / Phase 2)
├── NghiaTinhPanel          ← Top-Left (20, -100) 280×70 | NghiaTinhUI, slider đổi màu 3 ngưỡng
├── TutorialQuestPanel      ← Top-Left (20, -220) 280×150 | Checklist theo dõi 4 NPC và hoạt động farming
├── ResourcePanel           ← Bottom-Left (20, 20) 260×120 | HP/Stamina/Morale bars, tự động ẩn khi thoại mở
├── ControlsLegendPanel     ← Top-Right (-20, -120) 220×220 | H toggle, alpha 0.8
├── ToastPanel              ← Upper-Middle (0.5, 0.85) | Regex-colored notifications (+NT vàng, +VC xanh, đỏ lỗi)
├── NPCProximityPanel       ← WorldToScreen bám NPC | Rộng 380px, nút 360px, phím số 1/2/3 chọn lựa tương trợ
├── VillageSpeakerBanner    ← Centered | CanvasGroup tự động fade 5s thông báo tin thời tiết Việt hóa
├── EvacuationTimerPanel    ← Top-Center | NEW HUD layer: Đồng hồ đếm ngược 45s và trạng thái sơ tán dân làng
├── inventoryPanel          ← Center (0,0) 400×320 | I/Tab toggle mở hòm đồ chứa bao cát, ván gỗ
├── shopPanel               ← Center (0,0) 500×400 | Sạp giao thương tương trợ của O Thắm
├── dialoguePanel           ← Bottom full-width h=185 | Hộp thoại cốt truyện lớn và nút chọn
└── EndingPanel             ← Fullscreen | EndingManager kết cục màn hình
---

### C. `Player`
Player (PlayerController, Rigidbody, BoxCollider)
└── Player_Base
└── [model] indonesian_farmer_pak_tani.glb (Animator)
- Rigidbody: `useGravity=false`, `FreezePositionY`, `FreezeRotationXYZ` (Thành di chuyển phẳng bám sát Terrain)[cite: 25].
- Spawn: Vị trí mặc định sân nhà Thành `(0, 0.5, -6)`[cite: 25].
- interactRadius: Phạm vi quét tương tác `2.5f`[cite: 25].

---

### D. `Environment`
Environment
├── Ground_Main              ← Plane 3D MeshCollider, gán vật liệu Ground_LowPoly.mat (#7CA66D) mộc mạc
├── _Environment
│   ├── Houses
│   │   ├── Thanh_House      ← Nhà ngói cũ của Thành tại (0,0,-15.5) Y=180°. BoxCollider bọc chân tường
│   │   │   ├── bep_gas      ← KitchenHearth IInteractable (Sấy khoai khô / Nấu ăn) tại (5.89, 0.21, -8.69)
│   │   │   └── Roof_Anchor_Nodes ← NEW: 3-4 điểm Trigger cố định trên mái để chèn bao cát chống tốc mái ở Phase 2
│   │   ├── OTham_Shop       ← Sạp hàng lá tre tại (8.0, 0.0, 10.28)
│   │   │   └── Wall_Repair_Node ← NEW: Điểm Neo tương tác sửa vách tường sập bằng tấm ván gỗ ở Phase 2 sau lũ
│   │   └── BacNam_House     ← Nhà ngói của Bác Năm tại (-12.0, 0.0, 8.5)
│   │       └── Wall_Repair_Node ← NEW: Điểm Neo tương tác sửa vách tường sập bằng tấm ván gỗ ở Phase 2 sau lũ
│   └── NPCs (Chứa các NPC phụ)
│       ├── NPC_CuBay        ← Cụ Bảy tại (-14.20, 0.00, 4.95) | Điểm quyên góp khoai khô ở Phase 2
│       └── NPC_BeTi         ← Bé Tí tại (-2.72, 0.00, 5.00) | Checkpoint sơ tán khẩn cấp riêng biệt
├── Village_Well             ← Giếng làng đá tại (0, 0.63, 3.0), BoxCollider thu gọn bảo vệ lối đi mượt mà
├── Village_Speaker          ← Cột loa phát thanh xã tại (0, 1.75, 6)
├── Fences                   ← 12 đoạn Fence2.fbx đặt tự động bao quanh bọc ruộng
└── [Bamboo, BananaTree, Rocks, Props...]
---

### E. `FarmingArea`
FarmingArea
├── FarmingPlot              ← Visual bounding box (0.01m cao) - Edit Mode only
├── SoilCell                 ← Parent/helper field (auto-detect liên kết trong Awake)
├── Row 1
│   ├── SoilCell_Grid1       ← Standalone 3D SoilCell
│   ├── SoilCell_Grid2       ← Standalone 3D SoilCell
│   ├── SoilCell_Grid3       ← Standalone 3D SoilCell
│   └── SoilCell_Grid4       ← Standalone 3D SoilCell
├── Row 2
│   ├── SoilCell_Grid5       ← Standalone 3D SoilCell
│   ├── SoilCell_Grid6       ← Standalone 3D SoilCell
│   ├── SoilCell_Grid7       ← Standalone 3D SoilCell
│   └── SoilCell_Grid8       ← Standalone 3D SoilCell
└── Row 3
├── SoilCell_Grid9       ← Standalone 3D SoilCell
├── SoilCell_Grid10      ← Standalone 3D SoilCell
├── SoilCell_Grid11      ← Standalone 3D SoilCell
└── SoilCell_Grid12      ← Standalone 3D SoilCell (Tổng cộng 12 SoilCells, lưới rắn khít 4x3)
- Mỗi SoilCell ô con: BoxCollider trigger `(2.0, 0.3, 2.0)`, xoay flat XZ[cite: 25].
- Vòng phát triển: Hoán đổi trực tiếp qua 4 prefabs `Soil_Rocky` / `Soil_Clean` / `Soil_Tilled` / `Soil_Wet`[cite: 25].
- Cơ chế Phù Sa: Tự động chuyển đổi cả 12 ô đất con sang trạng thái `PhuSa` màu xám bồi đắp ở Phase 2 khi lũ rút[cite: 24].

---

### F. `NPCs` (Root - Các nhân vật lõi)
NPCs
├── NPC_OTham   (NPCCharacter, BoxCollider trigger) ← (3.53, 0.5, -9.49) | Visual O_Tham.fbx (M_OTham.mat)
└── NPC_BacNam  (NPCCharacter, BoxCollider trigger) ← (-1.78, 0.5, -10.18) | Visual Bac_Nam.fbx (M_BacNam.mat)
- Proximity UI detection range: `1.7m`[cite: 25].
- Trong countdown chạy lũ, dắt thành công sẽ thực hiện dọn dẹp hoặc tạm ẩn (`SetActive(false)`) để chuẩn bị hiển thị tái tụ họp cùng Thành trên mái nhà `Thanh_House` ở Phase 2[cite: 24].

---

### G. `InteractionZones`
InteractionZones
└── Shrine                   ← Miếu thờ đầu làng (7, 0, -6)
├── AltarModel           ← Chiếc bàn thờ trang nghiêm Mat_Altar.mat
└── [BoxCollider trigger, AncestralAltar script] ← Nhấn [E] thắp nhang trừ nhang kích hoạt bão lũ khẩn cấp
---

### H. `DisasterObjects`
- `WeatherManager` runtime-instantiate `3D_Water_Plane` (Mặt nước lũ đục ngầu) tại Runtime[cite: 25].
- Cao độ mặc định khi yên bình: $Y = -1.5\text{m}$ (Ẩn dưới đất)[cite: 25]. Khi bão lũ kích hoạt, nước dâng tịnh tiến trục Y vượt mốc $+1.5\text{m}$ gây lụt úng ruộng vườn[cite: 24, 25].