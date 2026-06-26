# Current Source Context Report — Đất Cày Lên Sỏi Đá

Tài liệu này cung cấp báo cáo ngữ cảnh mã nguồn toàn diện của dự án Unity tại trạng thái hiện tại. Đây là nguồn tham chiếu tin cậy nhất cho AI Agent và lập trình viên khi cần hiểu kiến trúc hệ thống trước khi viết hoặc chỉnh sửa code.

---

## 1. Project Overview

*   **Unity Editor Version**: `6000.4.3f1` (Unity 6)
*   **Active Render Pipeline**: Universal Render Pipeline (URP)
*   **Namespace chính**: `SownInStone`, `SownInStone.Core`, `SownInStone.Agriculture`, `SownInStone.Community`, `SownInStone.UI`, `SownInStone.Storage`, `SownInStone.Weather`, `SownInStone.Audio`, `SownInStone.Interactions`
*   **Current Main Demo Scene**: [Village_Demo.unity](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scenes/Village_Demo.unity) (Build Index 0)
*   **Input System**: Unity New Input System (`ENABLE_INPUT_SYSTEM`). **Cấm dùng `UnityEngine.Input` legacy** ngoại trừ trong GUI nội bộ.
*   **Current Gameplay Direction**: Canh tác nông sản làm lương thực cứu tế, giúp đỡ bà con chòm xóm (Vần công), thắt chặt tình nghĩa làng xã (Nghĩa Tình score) để vượt qua **2 phase chính ở tầng thiết kế**: Phase 1 `Before the Storm` và Phase 2 `After the Storm`.
*   **Runtime phase enum note**: Code hiện tại vẫn dùng các mốc enum chi tiết `LapNghiep`, `GioLao`, `ChuanBiBao`, `MuaBao`, `PhuSa`. Khi viết docs/design, hãy gom chúng vào 2 phase chính: `LapNghiep` + `GioLao` + `ChuanBiBao` = **Phase 1: Before the Storm**; `MuaBao` + `PhuSa` = **Phase 2: After the Storm**.

---

## 2. Scene Overview

### `Assets/Scenes/Village_Demo.unity` ← MAIN DEMO SCENE
*   **Purpose**: Cảnh chơi chính thức, đã chuẩn hóa tọa độ, logic gameplay ổn định.
*   **Root Objects**: `_Managers`, `_UI`, `Player`, `Main Camera`, `Lighting`, `Environment`, `FarmingArea`, `NPCs`, `InteractionZones`, `DisasterObjects`, `Audio`.
*   **Role**: **Playable Demo Scene** (Build Index 0). Mọi thay đổi gameplay phải thực hiện trên scene này.

### `Assets/Scenes/SampleScene.unity` ← SANDBOX ONLY
*   **Purpose**: Cảnh thử nghiệm dev, chứa cấu trúc hỗn hợp chưa chuẩn hóa.
*   **Role**: **Không dùng cho demo/build.** Chỉ dùng để thử nghiệm asset mới trước khi migrate sang `Village_Demo`.

---

## 3. Script Architecture — All Active Scripts

### 3.1 `Assets/Scripts/Core/`
| Script | Mô tả |
|--------|-------|
| [PlayerController.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerController.cs) | Singleton. Di chuyển camera-relative, xoay nhân vật mượt, tương tác `[E]`, farming, flood roof survival, bulk planting dialog. interactRadius=2.5f. |
| [CameraFollow3D.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/CameraFollow3D.cs) | Roblox-style orbit: RMB xoay, scroll zoom (3–9), SphereCast collision, SmoothDamp. Pitch 12°–55°, default 25°. |
| [GameManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/GameManager.cs) | Singleton. Chu kỳ ngày/đêm, chuyển các mốc runtime `LapNghiep`, `GioLao`, `ChuanBiBao`, `MuaBao`, `PhuSa`, sự kiện `OnDayChanged` / `OnPhaseChanged`. Các mốc này được gom thành 2 phase chính trong design. |
| [PlayerStats.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerStats.cs) | Singleton. Health, Stamina, Morale, Coins, HeatStress, ColdStress. |
| [WanderingAnimal.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/WanderingAnimal.cs) | Hành vi đi loanh quanh ngẫu nhiên cho gà/chó. |

### 3.2 `Assets/Scripts/Agriculture/`
| Script | Mô tả |
|--------|-------|
| [SoilCell.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Agriculture/SoilCell.cs) | Quản lý ô đất. Lưới 4x3 (12 SoilCells). 3 quality: BacMau / TrungBinh / PhuSa. 4 state visuals. Auto link cha-con trong Awake(). Bay hơi nước thực tế. Highlight target viền vàng. Debug helpers. |
| [CropInstance.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Agriculture/CropInstance.cs) | Vòng đời cây trồng (sinh trưởng qua thời gian, IsReadyToHarvest, ActionHarvest, DebugMature). |
| [CropData.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Agriculture/CropData.cs) | ScriptableObject chứa config cây trồng (DaysToMature, HarvestedItem, stages). |

### 3.3 `Assets/Scripts/Community/`
| Script | Mô tả |
|--------|-------|
| [CommunityManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/CommunityManager.cs) | Singleton. GlobalKarma (Nghĩa Tình), VanCong count. Tracking bools event: `eventOThamFoodCompleted`, `eventBacNamStormCompleted`, `eventVillageRecoveryCompleted`. |
| [NPCCharacter.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/NPCCharacter.cs) | NPC logic. Enum `StoryCharacterType`: OTham, BacNam, CuBay, BeTi. LookAtPlayer Slerp, ReturnToDefaultRotation, hasTalkedThisSession. |
| [NPCData.cs](file:///d:/Linh%20tinh/studying/Semester_7\PRU213\in_class\Project\src\clone\Assets\Scripts\Community\NPCData.cs) | ScriptableObject dữ liệu NPC tĩnh. |

### 3.4 `Assets/Scripts/UI/`
| Script | Mô tả |
|--------|-------|
| [SurvivalUIManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/SurvivalUIManager.cs) | Singleton. Tạo toàn bộ HUD lúc runtime trong Awake(). ShowDialogue, ShowDialogueWithChoices (2 và 3 nút), ShowHUDToast (Regex highlight), RefreshInventoryUI, OpenShop, ReorganizeUILayout. |
| [TutorialManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/TutorialManager.cs) | Singleton. Tutorial 2 giai đoạn: IntroQuests (gặp cả 4 NPC: O Thắm, Bác Năm, Cụ Bảy, Bé Tí) → Slideshow farming → FarmingTutorial (dọn đá, gieo hạt, tưới nước). TutorialQuestPanel HUD checkbox. DontDestroyOnLoad. |
| [NPCProximityOptionsUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/NPCProximityOptionsUI.cs) | Panel nổi bám NPC trong phạm vi 1.7m. CanvasGroup fade 0.13s. Phím số 1/2/3 + click. Auto-wire trong SurvivalUIManager.Awake(). |
| [NPCQuestMarkerUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/NPCQuestMarkerUI.cs) | Dấu `!` vàng bounce sin-wave trên NPC có event chưa hoàn thành. Ẩn khi player < 1.7m. Auto-wire trong SurvivalUIManager.Awake(). |
| [NghiaTinhUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/NghiaTinhUI.cs) | Hiển thị thanh Nghĩa Tình + label, tự đổi màu Fill theo ngưỡng (0–30 đỏ, 31–70 vàng, 71–100 xanh). |
| [VillageSpeakerBanner.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/VillageSpeakerBanner.cs) | Banner loa xã: PhaseTitleText + DayText + MessageText. CanvasGroup fade in/out, tự ẩn sau 5 giây. |
| [EndingManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/EndingManager.cs) | Singleton. `IsEndingShown` property. Đánh giá Nghĩa Tình → 3 kết cục (< 40 Sad, ≥ 40 Normal, ≥ 80 Best). |
| [Billboard.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/Billboard.cs) | Giữ GameObject luôn quay mặt camera. |

### 3.5 `Assets/Scripts/Interactions/`
| Script | Mô tả |
|--------|-------|
| [AncestralAltar.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Interactions/AncestralAltar.cs) | Thắp nhang: tiêu 1 Nhang → +10 Morale (clamp 100). |
| [Coracle.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Interactions/Coracle.cs) | Thuyền thúng: lên/xuống thuyền, di chuyển 2.5f, constraint biên lũ. |
| [MudPuddle.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Interactions/MudPuddle.cs) | Vũng bùn: làm chậm tốc độ di chuyển. |
| [FloodBarrier.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Interactions/FloodBarrier.cs) | Bao cát/tấm chắn lũ. |

### 3.6 `Assets/Scripts/Interaction/` (Interface-based)
| Script | Mô tả |
|--------|-------|
| [IInteractable.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Interaction/IInteractable.cs) | Interface `void Interact()`. |
| [KitchenHearth.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Interaction/KitchenHearth.cs) | Implements IInteractable. Bếp ga: Sấy khô (2 Tươi → 1 Khô) hoặc Nấu ăn (1 Tươi → +15 Stamina). |

### 3.7 `Assets/Scripts/Storage/`
| Script | Mô tả |
|--------|-------|
| [StorageManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Storage/StorageManager.cs) | Singleton. Quản lý kho đồ, AddItem/RemoveItem/GetStorageSlots. Tracking thối nông sản tươi theo ẩm. |
| [ItemData.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Storage/ItemData.cs) | ScriptableObject: ItemID, ItemName, Description, type(enum), Icon(Sprite), StaminaRestoreValue, MoraleRestoreValue, DecayRateInHumidity. |

### 3.8 `Assets/Scripts/Weather/`
| Script | Mô tả |
|--------|-------|
| [WeatherManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Weather/WeatherManager.cs) | Singleton. Lerp Weather theo các mốc runtime trong 2 phase chính, Temperature, Humidity, RainIntensity, FloodLevel, hạt mưa bám camera. 3D_Water_Plane instantiate runtime tại Y=-1.5f. |

### 3.9 `Assets/Scripts/Audio/`
| Script | Mô tả |
|--------|-------|
| [AudioManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Audio/AudioManager.cs) | Singleton. PlaySFX(string key), cache `missingClips` HashSet tránh spam lỗi. |

### 3.10 Scripts gốc (không namespace)
| Script | Mô tả |
|--------|-------|
| [FrameworkDebugUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/FrameworkDebugUI.cs) | F1 Debug: Nút Phase jump (1-2) + resource + Nghĩa Tình + Show Ending. Cột Farming debug tại x=540. |
| [FrameworkMainMenuUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/FrameworkMainMenuUI.cs) | Main menu phong cách bảng gỗ mộc mạc, hover scale 1.08x, GameStats sub-tab (Live Health/Stamina/Weather stats). |
| [FrameworkTester.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/FrameworkTester.cs) | Chạy kịch bản test nhanh trong Editor. |

---

## 4. Input Mapping

Dự án sử dụng **Unity New Input System** (`ENABLE_INPUT_SYSTEM`).

| Phím | Hành động |
|------|-----------|
| WASD / Arrow | Di chuyển camera-relative trên XZ |
| Left Shift | Chạy nhanh |
| RMB (hold + drag) | Xoay camera (Yaw + Pitch) |
| Mouse Scroll | Zoom in/out (3–9 đơn vị) |
| `[E]` / `[Space]` | Tương tác farming / bàn thờ / bếp ga / thuyền thúng / nóc nhà |
| `[1]` / `[2]` / `[3]` | Phím tắt lựa chọn NPC proximity panel |
| `[H]` | Ẩn/hiện Controls Legend |
| `[F1]` | Mở/đóng F1 Debug Panel |
| `[I]` / `[Tab]` | Mở Inventory |
| `[Esc]` / `[Enter]` | Menu chính |

---

## 5. UI/HUD Systems

| Element | Vị trí | Ghi chú |
|---------|--------|---------|
| TimeSeasonPanel | Top-Left `(20, -20)` size `280x70` | Day + Phase text |
| NghiaTinhPanel | Top-Left `(20, -100)` size `280x70` | Slider đổi màu + NghiaTinhUI script |
| TutorialQuestPanel | Top-Left `(20, -220)` size `280x150` | Tạo động bởi TutorialManager |
| ResourcePanel | Bottom-Left `(20, 20)` size `260x120` | HP/Stamina/Morale bars, ẩn khi dialogue/shop/ending |
| ControlsLegendPanel | Top-Right `(-20, -120)` size `220x220` | Alpha 0.8, H toggle |
| ToastPanel | Upper-Middle `(0.5, 0.85)` | Regex highlight vàng NT / xanh VC / đỏ cảnh báo |
| NPCProximityPanel | WorldToScreen trên đầu NPC | Width 380px, nút 360px, fade 0.13s |
| VillageSpeakerBanner | Centered | Fade in/out 5s |
| inventoryPanel | Center `(0,0)` size `400x320` | Slot `80x80`, icon `60x60` |
| shopPanel | Center `(0,0)` size `500x400` | Row height 60, icon `56x56` |
| dialoguePanel | Bottom full-width height `185f` | speakerName + content + buttons |
| EndingPanel | Fullscreen | Sad/Normal/Best ending |

---

## 6. Important Assets & Visual Setup

*   **Player Model**: `indonesian_farmer_pak_tani.glb` (Lão Nông Việt Nam) + Animator.
*   **NPC Models**: `Bac_Nam.fbx` (M_BacNam.mat), `O_Tham.fbx` (M_OTham.mat). URP Simple Lit, scale đứng vừa bằng Player (~2.2m).
*   **NPCs phụ**: `cu_bay@Old Man Idle.fbx`, `be_ti@Breathing Idle.fbx` (Mixamo-rigged).
*   **Ground**: Plane 3D + MeshCollider, `Ground_LowPoly.mat` màu `#7CA66D`.
*   **Soil Visuals**: `Soil_Rocky`, `Soil_Clean`, `Soil_Tilled`, `Soil_Wet` prefabs. Materials tại `Assets/Art/Farming_Plot_Status/Materials/`.
*   **Crop Visuals**: `SweetPotato_Stage1`, `SweetPotato_Stage2` (2 stages FBX).
*   **Tutorial Images**: `Assets/Resources/Textures/Tutorial/` — `tutorial_clear_rocks.png`, `tutorial_plant_water.png`, `tutorial_npc_interact.png`, `tutorial_flood_survival.png`. Intro frames: `Textures/Tutorial/tutorial_intro/{0..4}`.
*   **Item Icons** (`Assets/Art/UI/Icons/`): `hat_giong_khoai_lang.png`, `khoai_lang.png`, `khoai_gieo_kho.png`, `3_cay_nhang.png`, `mi_tom.png`, `bao_cat.png`, `tam_chan_lu.png`, `non_la.png`.

---

## 7. Data Assets (`Assets/Data/`)

| Asset | ItemID | Loại |
|-------|--------|------|
| `Item_Seed.asset` | item_seed | Seed |
| `Item_FreshCrop.asset` | item_fresh_crop | Nông sản tươi (dễ thối ở Phase 2) |
| `Item_PreservedCrop.asset` | item_preserved_crop | Nông sản khô |
| `Item_Incense.asset` | item_incense | Vật phẩm tâm linh |
| `Item_Noodles.asset` | item_noodles | Thực phẩm cứu trợ |
| `Item_sandbag.asset` | item_sandbag | Thiết bị chống lũ |
| `Item_flood_board.asset` | item_flood_board | Tấm Chắn Lũ (gỗ ép) |
| `Item_non_la.asset` | item_non_la | Nón Lá (trang phục bảo vệ thời tiết) |
| `Crop_KhoaiLang.asset` | — | CropData: chín 5 ngày |

---

## 8. Known Issues / Watchlist

*   **BoundaryElement scale**: Tảng đá biên giới quá nhỏ, hoạt động như tường vô hình.
*   **Camera góc hẹp**: SphereCast có thể thu ngắn camera đột ngột sát mái Thanh_House.
*   **Menu Settings legacy KeyCode**: FrameworkMainMenuUI hiển thị gán phím dạng KeyCode thay vì InputAction.
*   **Dialogue E/Space conflict**: [E] hiện chỉ đóng dialogue NPC, không mở. Tương tác NPC chỉ qua Proximity Panel.

---

## 9. Recommended Next Tasks

1. **P0: Tutorial completion**: Kết nối `TutorialManager.InitializeTutorial()` từ Main Menu hoặc GameManager để tự động khởi động tutorial khi bắt đầu game mới.
2. **P1: Multiple ending illustrations**: Tạo/vẽ 3 ảnh minh họa cho 3 kết cục Nghĩa Tình hiển thị trong EndingPanel.
3. **P1: Menu Settings InputAction**: Cập nhật FrameworkMainMenuUI đọc trực tiếp từ InputAction thay vì KeyCode tĩnh.
4. **P2: BoundaryElement fix**: Thay MeshCollider nhỏ bằng BoxCollider đơn giản bao quanh rìa map.
5. **P2: More dialogue lines**: Bổ sung thoại địa phương (Central Vietnam dialect) cho O Thắm, Bác Năm, Cụ Bảy, Bé Tí.

---

## 10. Files Most Important to Protect

Nghiêm cấm sửa đổi vô căn cứ hoặc thay thế nội dung các tệp sau:

*   [PlayerController.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerController.cs)
*   [SurvivalUIManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/SurvivalUIManager.cs)
*   [TutorialManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/TutorialManager.cs)
*   [NPCProximityOptionsUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/NPCProximityOptionsUI.cs)
*   [NPCCharacter.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/NPCCharacter.cs)
*   [CommunityManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/CommunityManager.cs)
*   [SoilCell.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Agriculture/SoilCell.cs)
*   [GameManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/GameManager.cs)
*   [WeatherManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Weather/WeatherManager.cs)
*   [FrameworkDebugUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/FrameworkDebugUI.cs)
*   [Village_Demo.unity](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scenes/Village_Demo.unity)
