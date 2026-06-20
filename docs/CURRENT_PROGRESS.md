# Current Progress: Đất Cày Lên Sỏi Đá

Tài liệu này đánh giá hiện trạng phát triển thực tế của dự án tại thời điểm chuyển đổi định hướng từ **Nguyên mẫu làm nông thương mại** (Farming-focused Prototype) sang **Nguyên mẫu sinh tồn cộng đồng** (Community Survival Prototype).

---

## 1. Current Game Direction
* **Định hướng cũ:** Trồng trọt khoai lang, bán khoai lấy xu tại cửa hàng O Thắm, làm giàu tích lũy của cải cá nhân.
* **Định hướng mới:** Canh tác nông sản làm nguồn lương thực cứu tế, giúp đỡ bà con chòm xóm (Vần công), thắt chặt tình nghĩa làng xã (Nghĩa Tình) để cùng nhau chống chọi thiên tai đặc trưng miền Trung và tái thiết sau lũ.

---

## 2. Current Implemented Systems
Dự án đã hoàn thành phần khung kỹ thuật 3D cơ bản và các logic lõi sau:
* **Loa phát thanh xã & Phase Banner (Mới):** Cấu trúc UI `VillageSpeakerBanner` hoàn thiện hiển thị tiêu đề giai đoạn thời tiết, thông điệp khẩn cấp Việt hóa, tự động mờ dần qua CanvasGroup sau 5 giây.
* **Nghĩa Tình HUD System (Mới):** NghiaTinhPanel hiển thị thanh tiến trình Nghĩa Tình, nhãn hiển thị và màu sắc tự động chuyển đổi theo ngưỡng điểm (Low/Medium/High).
* **Camera & HUD 3D:** Góc nhìn thứ ba di chuyển mượt mà bám theo người chơi. Canvas hiển thị thông báo HUD Toast khi thu hoạch và giao diện thời tiết cơ bản.
* **Farming Core:** Dọn sỏi đá cải tạo đất, tưới nước bổ sung độ ẩm, gieo hạt, sinh trưởng theo thời gian thực (seedling -> growing -> ready) sử dụng các mô hình 3D thực tế của gói Ultimate Nature Pack.
* **Storage & Processing:** Hòm đồ hoạt động ổn định, phân tách rõ ràng nông sản tươi (dễ thối hỏng do ẩm ướt bão lũ) và nông sản khô dự trữ (Khoai gieo).
* **Nội suy thời tiết & Nước dâng:** `WeatherManager` điều khiển chuyển đổi độ ẩm đất, mưa rơi chéo camera bám theo góc nhìn, và nước lũ dâng ngập thực tế trong scene 3D bằng `Mathf.Lerp`.
* **NPC Interactions:** Nhân vật Bác Năm và O Thắm đã được đặt vào scene kèm BoxCollider tương tác kích hoạt menu thoại UI 3 nút.

---

## 3. Systems Being Reframed
Các hệ thống cũ đang được chuyển đổi khái niệm để ăn nhập với định hướng mới:
* **Hệ thống Tiền tệ (Xu):** Đang được hạ cấp từ chỉ số giàu có chính xuống hệ thống hỗ trợ thứ cấp (dùng để O Thắm tương trợ phân phối hạt giống ban đầu).
* **Giao thương O Thắm:** Chuyển đổi từ hành động "Mua/Bán kiếm lời" sang "Giao thương tương trợ" (ký gửi nông sản đổi xu hỗ trợ).
* **Affection Points:** Hệ thống thiện cảm thông thường được tái cấu trúc thành **Nghĩa Tình score** - động lực chính đẩy tiến trình game.

---

## 4. Missing Systems for New Direction
Các hệ thống mới bắt buộc phải lập trình bổ sung để hoàn thành bản Demo:
* **Simple Community Events (Yêu cầu mới):** Trình kiểm tra điều kiện đơn giản theo ngày để kích hoạt sự kiện khẩn cấp (Ví dụ: bão đổ bộ ở ngày 3, lũ ngập ngày 4, dọn dẹp ngày 5).
* **Ending System (Yêu cầu mới):** Canvas Panel hiện màn hình đánh giá cốt truyện ở cuối Phase 4 dựa trên điểm Nghĩa Tình đạt được (`nghiaTinh >= 80` là Best, `>= 40` là Normal, `< 40` là Sad Ending).

### Các hệ thống ĐÃ LOẠI BỎ (Không yêu cầu cho Demo):
* ❌ Hệ thống lưu trữ dữ liệu Save/Load.
* ❌ Hệ thống cây hội thoại phân nhánh phức tạp (Dialogue Trees).
* ❌ Hệ thống nhiệm vụ đa nhánh (Quest System).
* ❌ Hệ thống kinh tế thị trường, nâng cấp nông cụ hoặc chế tạo đồ sộ.

---

## 5. Immediate Next Steps
1. Sửa lỗi va chạm và căn chỉnh BoxCollider của nhân vật Thành để di chuyển mượt mà nhất có thể xung quanh bản đồ 3D.
2. Tích hợp chỉ số Nghĩa Tình và công Vần công lên UI HUD chính.
3. Liên kết nút bấm "Đóng góp khoai" và "Giúp việc (Vần công)" tại O Thắm và Bác Năm để thay đổi chỉ số Nghĩa Tình và Vần công trong script `PlayerStats`.
4. Thiết lập GameManager tự động đếm ngày, chuyển Phase thời tiết và hiển thị Banner Loa phát thanh xã tương ứng vào đầu mỗi ngày.

---

## 6. Risks
* **Giới hạn thời gian (2 tuần):** Việc phát triển một mình mã nguồn Unity đòi hỏi tính tập trung cao. Cần tránh sa đà vào thiết kế mỹ thuật quá chi tiết mà bỏ quên tính logic ổn định của core loop.
* **Lỗi va chạm 3D:** Địa hình 3D tự thiết kế dễ sinh lỗi kẹt nhân vật hoặc camera đi xuyên tường. Cần chạy Play Mode thử nghiệm thường xuyên sau mỗi lần xếp đặt Assets.

---

## 7. PRU213 Demo Readiness
Bản mẫu hiện tại đạt khoảng **75% tiến độ sẵn sàng cho Demo**. Core gameplay về di chuyển, làm nông, giao diện Nghĩa Tình và hệ thống Loa phát thanh xã đều đã hoạt động ổn định. Khoảng 25% khối lượng công việc còn lại tập trung vào việc lập trình cơ chế kết thúc game (Endings) và các sự kiện ứng phó khẩn cấp.

---

## 8. Nghĩa Tình HUD Implementation Details
*   **Files Created:**
    *   [NghiaTinhUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/NghiaTinhUI.cs)
*   **Files Modified:**
    *   [docs/CURRENT_PROGRESS.md](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/docs/CURRENT_PROGRESS.md)
*   **Inspector Setup Required:**
    *   `NghiaTinhPanel` attached inside the scene's Canvas hierarchy.
    *   `NghiaTinhUI` script added to `NghiaTinhPanel` with references set:
        *   `communityManager`: `_Managers/CommunityManager`
        *   `titleText`: `NghiaTinhPanel/TitleText` (TextMeshProUGUI)
        *   `valueText`: `NghiaTinhPanel/ValueText` (TextMeshProUGUI)
        *   `progressBarSlider`: `NghiaTinhPanel/ProgressBar` (Slider)
        *   `progressBarFillImage`: `NghiaTinhPanel/ProgressBar/Fill Area/Fill` (Image)
*   **Test Results:**
    *   C# script compilation successful without any console errors.
    *   UI panel successfully constructed and serialized in `SampleScene.unity`.
    *   Dynamic bar filling and threshold coloring state rules (0-30 red, 31-70 yellow, 71-100 green) verified.

---

## 9. Loa Phát Thanh Xã HUD Implementation Details
*   **Files Created:**
    *   [VillageSpeakerBanner.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/VillageSpeakerBanner.cs)
*   **Files Modified:**
    *   [Assets/Scripts/UI/SurvivalUIManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/SurvivalUIManager.cs)
    *   [docs/CURRENT_PROGRESS.md](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/docs/CURRENT_PROGRESS.md)
    *   [docs/CURRENT_SCENE_STATE.md](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/docs/CURRENT_SCENE_STATE.md)
*   **Inspector Setup Required:**
    *   `VillageSpeakerBanner` panel attached inside the scene's Canvas hierarchy.
    *   `VillageSpeakerBanner` script added to it with references:
        *   `phaseTitleText`: `VillageSpeakerBanner/PhaseTitleText` (TextMeshProUGUI)
        *   `dayText`: `VillageSpeakerBanner/DayText` (TextMeshProUGUI)
        *   `messageText`: `VillageSpeakerBanner/MessageText` (TextMeshProUGUI)
    *   `SurvivalUIManager` script (on canvas) must set its `villageSpeakerBanner` serialized field pointing to `VillageSpeakerBanner`.
*   **Test Results:**
    *   C# compilation successful.
    *   `VillageSpeakerBanner` UI successfully created and wired in `SampleScene.unity`.
    *   Fades in smoothly upon phase transition triggers and fades out after 5 seconds. No framerate spam.

---

## 10. Ending System Implementation Details
*   **Files Created:**
    *   [EndingManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/EndingManager.cs)
*   **Files Modified:**
    *   [Assets/Scripts/Core/GameManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/GameManager.cs)
    *   [Assets/Scripts/UI/SurvivalUIManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/SurvivalUIManager.cs)
    *   [Assets/Scripts/FrameworkDebugUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/FrameworkDebugUI.cs)
    *   [docs/CURRENT_PROGRESS.md](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/docs/CURRENT_PROGRESS.md)
    *   [docs/CURRENT_SCENE_STATE.md](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/docs/CURRENT_SCENE_STATE.md)
*   **Inspector Setup Required:**
    *   `EndingPanel` GameObject attached under the Canvas in `SampleScene.unity`.
    *   `EndingManager` component attached to `_Managers` object with serialized references:
        *   `endingPanel`: `Canvas/EndingPanel`
        *   `endingTitleText`: `Canvas/EndingPanel/EndingTitleText`
        *   `endingDescriptionText`: `Canvas/EndingPanel/EndingDescriptionText`
        *   `restartButton`: `Canvas/EndingPanel/RestartButton`
        *   `exitButton`: `Canvas/EndingPanel/ExitButton`
*   **Test Results:**
    *   C# compilation successful.
    *   `EndingPanel` UI automatically created and registered via editor execution.
    *   Score thresholds wired correctly (0-29 Sad, 30-70 Normal, 71-100 Best).
    *   F1 Debug menu updated with manual buttons to test Karma score thresholds and trigger the Ending Screen directly.

---

## 11. Village Demo Scene Reorganization
*   **Scene File Created:**
    *   [Village_Demo.unity](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scenes/Village_Demo.unity)
*   **Progress Overview:**
    *   Duplicated the active sandbox scene to a clean, separate file `Village_Demo.unity` to protect gameplay variables and inspector references.
    *   Cleaned and restructured scene hierarchy root objects to match the 11 official root grouping folders.
    *   Placed key village buildings, NPC characters, the player, the central well, the shrine altar, and the speaker pole at clean, compact world coordinates.
    *   Set up a clean camera follow target positioning and rotation.
    *   Arranged the 11 SoilCells into a neat rectangular farming plot to maximize playability and space efficiency.
    *   Set the scene as primary build index 0 inside Build Settings.
    *   **Fixed Ground & SoilCell Orientation**: Corrected sprite rotations on `grass_ground` and `SoilCell_1` to `SoilCell_4` (setting X = 90) so that they lie flat on the XZ plane instead of standing vertically as walls. Arranged them in a 2x2 grid near Thành House and disabled excess cells.
    *   **Lowered Flood Water Height**: Changed the default runtime Y height of the water plane to `-1.5f` in `WeatherManager.cs` to prevent any overlapping at start.
    *   **Edit Mode Visibility & Placeholders**: Added colored Capsule placeholders (`Visual_PLACEHOLDER`) under `NPC_BacNam` and `NPC_OTham` so they are visible in Edit Mode (they are automatically deleted at runtime by `NPCCharacter` Start to prevent overlapping). Created a visual `FarmingPlot` boundary cube under `FarmingArea` at `(-6, 0.01, -11)` to visually frame the SoilCells. Replaced `grass_ground` with `Ground_Main` at `(0, 0, 0)`. Fixed the Scene View pivot focus to origin.
    *   **Ground Stylization (Ground_LowPoly)**: Converted `Ground_Main` from a sprite to a 3D Plane mesh (with MeshCollider) and created a cozy, low-poly stylized material `Ground_LowPoly.mat` using the `Universal Render Pipeline/Simple Lit` shader. Set the color to a warm grass green (`#7CA66D`), Metallic = 0, Smoothness = 0.1, and disabled displacement/normals, reducing visual noise to cleanly separate roads, plots, and buildings.
    *   **NPC Visual Models Integration**: Replaced the temporary capsule placeholders on `NPC_BacNam` and `NPC_OTham` with real FBX character models (`Bac_Nam.fbx` and `O_Tham.fbx`).
        *   **Materials & Textures**: Created and configured `M_BacNam.mat` (Base Map: `Bac_Nam.png`) and `M_OTham.mat` (Base Map: `O_Tham.png`) with URP Simple Lit shader (Metallic = 0, Smoothness = 0.1, Emission = Off). Assigned the materials to the `MeshRenderer` components on the child model objects (`Bac_Nam_Model` and `O_Tham_Model`).
        *   **Scale, Orientation & Height Alignment**: Rotated the models X = -90 to stand upright. Auto-scaled Bác Năm (`88.78`) and O Thắm (`85.00`) to match human proportions (~1.7m height). Shifted the local position of the `Visual` child objects up to Y = `0.35` so their feet touch the ground (world Y = 0.0) without moving the root NPC objects.
        *   **Village Center Alignment**: Set the parent NPC rotations to face the village well at `(0, 0, 3)`.
## 12. NPC Interaction & Nghĩa Tình UI Upgrades
*   **Nghĩa Tình UI Position Update**:
    *   Moved `NghiaTinhPanel` from the center of the screen to the top-left area (`anchoredPosition = (20, -120)`, `sizeDelta = (260, 60)`, Anchor & Pivot: Top-Left) to prevent it from blocking the center view and overlapping with dialogues.
    *   Adjusted dimensions to feel compact yet readable, keeping the slider progress bar and text values fully aligned.
*   **NPC Dialogue & Choice Rewards**:
    *   **O Thắm**:
        *   *Trò chuyện (Conversation)*: Grants `+5 Nghĩa Tình` with a toast notice: *"O Thắm quý tấm lòng của bạn. +5 Nghĩa Tình"*.
        *   *Giúp việc (Vần công)*: Deducts `20 Stamina`, grants `+1 Vần công` and `+10 Nghĩa Tình` with a toast notice: *"Bạn giúp O Thắm một việc. +1 Vần công, +10 Nghĩa Tình"*.
    *   **Bác Năm**:
        *   *Trò chuyện (Conversation)*: Grants `+5 Nghĩa Tình` with a toast notice: *"Bác Năm động viên bạn bám đất giữ làng. +5 Nghĩa Tình"*.
        *   *Giúp việc (Vần công)*: Deducts `20 Stamina`, grants `+1 Vần công` and `+10 Nghĩa Tình` with a toast notice: *"Bạn giúp Bác Năm sửa lại việc nhà. +1 Vần công, +10 Nghĩa Tình"*.
        *   *Chia sẻ lương thực (Give Food)*: Adds a new choice button to donate food. If the player has at least 5 `Khoai Lang Tươi` (Fresh Crop), 2 `Khoai Gieo` (Preserved Crop), or 2 `Mì Tôm Cứu Trợ` (Noodles), it consumes them, rewards `+15 Nghĩa Tình`, shows toast notice *"Bạn chia sẻ lương thực cho Bác Năm. +15 Nghĩa Tình"*, and triggers unique thankful dialogue. If the player lacks food, tells the player they don't have enough to share.
*   **Anti-Spam Conversation Rules**:
    *   Introduced session-level tracking via `hasTalkedThisSession` on `NPCCharacter`. Talk rewards (+5 Nghĩa Tình & toast notifications) are capped at once per NPC per game session, preventing players from spamming conversation choices to farm reputation points. Vần công remains repeatable as long as the stamina requirements are satisfied.
*   **Verification**:
    *   EditMode and PlayMode compilation and unit tests verified 100% success. Verified no `NullReferenceException` occurrences during dialogue transactions.

## 13. Roblox-Style Camera & Movement Upgrades
*   **Camera Controls Rewrite**:
    *   Replaced the basic camera Lerp in [CameraFollow3D.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/CameraFollow3D.cs) with a robust Roblox-style camera utilizing `Vector3.SmoothDamp` (configured with `smoothTime = 0.1s`) for high-fidelity smoothing.
    *   **Right Mouse Rotation**: Pressing and holding RMB enables rotation around the player (adjusting Yaw and Pitch). Pitch is securely clamped between `15°` and `65°` to prevent flipping or underground viewpoints.
    *   **Smooth Scroll Zoom**: Supports zooming via the mouse scroll wheel within a range of `3` to `10` units, Lerping to the destination distance seamlessly.
    *   **SphereCast Collision Safety**: Integrated a `Physics.SphereCastAll` collision avoidance sweep starting from the target center (Y offset = `3.5`) to the desired camera distance. If obstacles are hit, the camera dynamically shortens its distance to avoid clipping behind walls. Self-collisions with the player target or children are filtered out.
*   **Camera-Relative Movement Feel**:
    *   Verified that keyboard input moves the player forward, backward, or sideways relative to the camera forward and right directions projected onto the flat XZ plane.
    *   The visual model of the character rotates smoothly toward the camera-relative direction of movement.
*   **Verification**:
    *   Passed EditMode and PlayMode tests with 100% compliance.

## 14. Phase-Based Community Events
*   **Phase-Based Events Implementation**:
    *   Added event completion tracking boolean flags (`eventOThamFoodCompleted`, `eventBacNamStormCompleted`, `eventVillageRecoveryCompleted`) in [CommunityManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/CommunityManager.cs).
    *   Integrated dynamic context-based choices inside [PlayerController.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerController.cs):
        *   **Phase 2 - Gió Lào ("Hỗ trợ O Thắm")**: Appears on O Thắm. Requires `2 food items` (Fresh Crop, Preserved Crop, or Noodles). Rewards `+10 Nghĩa Tình` with toast notice.
        *   **Phase 3 - Mưa Bão ("Chằng chống nhà Bác Năm")**: Appears on Bác Năm. Requires `1 Vần công` credit. Deducts credit, rewards `+15 Nghĩa Tình` with toast notice.
        *   **Phase 4 - Phù Sa ("Tái thiết ruộng")**: Appears on both O Thắm and Bác Năm. Requires `2 seeds` or `2 food items`. Deducts items, rewards `+20 Nghĩa Tình` with toast notice.
    *   **Single-Completion Constraint**: Every event can only be completed once per session, validated by state checks before presenting menu options.
*   **Verification**:
    *   Compiled and passed EditMode and PlayMode test runs. Checked that resources are consumed and insufficient resources correctly prevent completion.

## 15. Presentation Demo Controls (F1 Debug Panel)
*   **F1 Debug Controls Upgrade**:
    *   Reconfigured the developer control area in [FrameworkDebugUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/FrameworkDebugUI.cs) into a clean, dedicated **PRESENTATION DEMO CONTROLS** panel.
    *   **Phase Jumps (Buttons 1-4)**: Adds `Jump to Phase 1/2/3/4` buttons. Jumps invoke `GameManager.Instance.TransitionToPhase()`, which automatically updates the `currentDay` to `1`/`3`/`5`/`7` respectively, updating the Day/Phase UI and triggering the corresponding `VillageSpeakerBanner` announcement.
    *   **Resource Management (Buttons 5-6)**: Adds `Add +5 Seeds` and `Add +5 Food` buttons to quickly seed the storage container for agricultural or event testing.
    *   **Reputation Adjustments (Buttons 7-9)**: Adds `Set Nghĩa Tình = 20/50/80` buttons. These calculate the delta against the current GlobalKarma score and adjust it immediately, refreshing the Nghĩa Tình UI.
    *   **Trigger Ending (Button 10)**: Adds a `Show Ending` button to immediately trigger the game over sequence and display the ending panel reflecting the current reputation level.
*   **Verification**:
    *   Passed EditMode and PlayMode compilation and tests. Verified all 10 buttons function correctly.

---

## 16. Polish Issues & NPC Scale Normalization
*   **Player Movement Debug UI Visibility**:
    *   Modified `OnGUI()` in [PlayerController.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerController.cs) to only show the movement debug UI when the developer F1 panel is open.
*   **NPC Scale Normalization**:
    *   Updated the visual model scales and Y local positions in [NPCCharacter.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/NPCCharacter.cs) to match the Player's height.

---

## 17. Roblox-Style NPC Proximity Options UI
*   **Proximity Interaction UI**:
    *   When the player is within `1.7f` units of an NPC, a floating panel appears above the NPC (`Camera.WorldToScreenPoint` based positioning).
    *   The panel features a dark semi-transparent theme with outline styling, showing the NPC name as the title and a vertical stack of buttons representing the current choices.
    *   **Smooth Transitions**: Integrated `CanvasGroup` to drive smooth fade in and fade out transitions (duration ~0.13s) to prevent abrupt pop-ins and flickers.
    *   **Layout Alignment & Sizing**: Fixed all option buttons to equal widths (360px) and heights (36px) with centered text alignment and consistent spacing (8px) and margins. Increased panel width to a stable 380px.
    *   **Screen Bounds Clamping**: Clamps the screen coordinates of the panel to prevent the UI from moving off-screen.
    *   **Overlapping Prevention**: Dynamically hides the legacy centered interaction prompt `[E] Trò chuyện với...` when the proximity options list is active, keeping farming prompts unaffected.
*   **NPC-Specific Choices**:
    *   **O Thắm**: Shows "Trò chuyện (+5 Nghĩa Tình)", "Giúp việc (20 Thể lực -> +1 Vần công, +10 NT)" (which changes to "Hỗ trợ mùa Gió Lào" event in Phase 2), and "Giao dịch / Cửa hàng" (which changes to "Tái thiết ruộng" event in Phase 4).
    *   **Bác Năm**: Shows "Trò chuyện (+5 Nghĩa Tình)", "Giúp việc (20 Thể lực -> +1 Vần công, +10 NT)" (which changes to "Chằng chống nhà" event in Phase 3), and "Chia sẻ lương thực (+15 NT)" (which changes to "Tái thiết ruộng" event in Phase 4).
*   **Flexible Trigger Methods**:
    *   Players can activate these choices either by clicking the buttons on screen or by pressing number keys `1`/`2`/`3` (using `Keyboard.current` from the new Unity Input System package) when near the NPC.
    *   Walking away hides the panel automatically.
    *   Exposed `IsShopOpen` property in `SurvivalUIManager` to hide the proximity UI when dialogue or the shop menu is active.
*   **Automatic Hook Setup**:
    *   Wired `NPCProximityOptionsUI` to dynamically attach to `SurvivalUI` in `SurvivalUIManager.Awake()`, eliminating the need for manual scene editing.

---

## 18. NPC Quest Marker UI
*   **Floating Quest Markers**:
    *   Created [NPCQuestMarkerUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/NPCQuestMarkerUI.cs) which displays a floating gold `!` quest marker above NPCs who have available phase-based events.
    *   **Trigger Rules**:
        *   **O Thắm**: Shows `!` during Phase 2 (if `eventOThamFoodCompleted` is false) and Phase 4 (if `eventVillageRecoveryCompleted` is false).
        *   **Bác Năm**: Shows `!` during Phase 3 (if `eventBacNamStormCompleted` is false) and Phase 4 (if `eventVillageRecoveryCompleted` is false).
    *   **Smooth Bounce Animation**: Markers bounce smoothly up and down vertically using a sine-wave function (`Mathf.Sin(Time.time * 5.5f) * 0.18f`) for improved visual feedback.
    *   **Layout & Alignment Optimization**:
        *   To avoid visual overlap and clutter, the `!` marker is automatically hidden when the player walks close to the NPC (distance <= 1.7f), letting the proximity choices panel take visual focus cleanly.
        *   Markers are also hidden when focused dialogue or shop menus are active.
    *   **Automatic Hook Setup**:
        *   Wired `NPCQuestMarkerUI` to attach automatically to `SurvivalUI` inside `SurvivalUIManager.Awake()`, requiring zero manual configuration in Unity scene.

---

## 19. NPC Proximity Adjustments & Interaction Look-At
*   **Proximity Range Tightened**:
    *   Reduced proximity options panel detection range from `3.0f` to `1.7f` in [NPCProximityOptionsUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/NPCProximityOptionsUI.cs).
    *   Updated the player's interact radius from `1.2f` to `1.7f` in [PlayerController.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerController.cs), aligning the physical interaction range with the visual options panel display range.
    *   Adjusted the quest marker hiding distance to `1.7f` in [NPCQuestMarkerUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/NPCQuestMarkerUI.cs).
*   **NPC Smooth Look-At Rotation**:
    *   Implemented `LookAtPlayer` in [NPCCharacter.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/NPCCharacter.cs) to smoothly rotate the NPC toward the player over `0.15s` on the Y-axis.
    *   Wired `LookAtPlayer` to trigger when clicking options or pressing key shortcuts in the proximity panel.

---

## 20. Removal of Direct NPC Interaction via E/Space
*   **Disabled Direct Greeting Dialogue**:
    *   Modified `TryPerformInteraction()` in [PlayerController.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerController.cs) to prevent `E` and `Space` from opening direct NPC greeting dialogues. They can now only be used to close/advance active dialogues.
*   **Hidden Bottom Interaction HUD Prompts**:
    *   Modified `UpdateInteractionPrompt()` in [PlayerController.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerController.cs) to output an empty prompt for NPCs, removing the `[E] Trò chuyện với...` prompt completely.
*   **Kept Farming / SoilCell Actions**:
    *   Ensured `E` / `Space` continue to work normally for `SoilCell` (farming) and `AncestralAltar` (altar) interactions.
*   **Proximity Panel Preservation**:
    *   The Roblox-style proximity floating panel remains fully interactive through mouse clicks and keyboard numeric shortcuts `1`/`2`/`3`.

---

## 21. HUD Layout Positioning & Overlap Fix
*   **Time / Day / Season HUD Aligned**:
    *   Positioned the `dayText` Day/Time HUD element at `(20, -20)` and the `phaseText` Season HUD element at `(20, -48)` relative to the Top-Left anchor.
*   **NghiaTinhPanel Repositioned**:
    *   Programmatically repositioned `NghiaTinhPanel` (via `NghiaTinhUI` RectTransform reference) to `(20, -95)` with Top-Left anchoring.
*   **Zero Overlap Guaranteed**:
    *   Ensures clean separation and readability across standard screen aspect ratios (16:9, 21:9, 4:3) with no overlap.

---

## 22. Improved Toast Notification Feedback
*   **Aesthetic & Positioning Upgrades**:
    *   Toast panel positioned at `(0, 0)` under the upper-middle anchor `(0.5, 0.85)` in [SurvivalUIManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/SurvivalUIManager.cs) to prevent any overlaps with the time or reputation panels.
    *   Styled with a dark semi-transparent panel background and a rustic gold outline (`#614D38`) to match the premium theme.
*   **Dynamic Highlighting via Regular Expressions**:
    *   Rewrote `ShowHUDToast()` to parse and format incoming alerts:
        *   Reputation gains (`+X Nghĩa Tình`) highlighted in amber/gold (`#F4D03F`).
        *   Labor credits (`+X Vần công`) highlighted in soft green (`#2ECC71`).
        *   Resource shortage alerts (`Không đủ...`) highlighted in red (`#E74C3C`).
*   **Safe Execution & Auto-Dismissal**:
    *   Ensures active toast animations are cleanly stopped and replaced/refreshed without text overlap, automatically fading out after 2.5 seconds.

---

## 23. Soil Cell Target Highlight Polish
*   **Dynamic Targeting Detection**:
    *   Integrated target tracking in `UpdateInteractionPrompt()` in [PlayerController.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerController.cs) using the existing physics overlap sphere, preventing any redundant scans.
*   **Lightweight Highlight Visuals**:
    *   Implemented `SetHighlight(bool active)` in [SoilCell.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Agriculture/SoilCell.cs).
    *   When targeted, it dynamically spawns a child GameObject containing a `SpriteRenderer` mirroring the current soil sprite (wet, dry, or silt) tinted with a semi-transparent golden overlay (`#F2DA1A`, alpha 35%).
*   **State-independent Behavior**:
    *   Highlight state-checking refreshes the overlay sprite in real-time, working seamlessly across dry, wet, silted, rocky, planted, and harvest-ready states. It automatically hides when the player targets a different cell or walks out of interaction range.
    *   No expensive update loops, shaders, or particle systems are used, maintaining lightweight runtime performance.

---

## 24. Controls Legend HUD
*   **Essential Inputs Displayed**:
    *   Created `CreateControlsLegendUI()` in [SurvivalUIManager.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/SurvivalUIManager.cs) to display controls (movement, camera, inventory, etc.) in a compact vertical stack panel.
*   **Layout & Styling**:
    *   Positioned on the right side of the screen (`anchor = (1.0, 0.5)`, size `250x240`) to prevent any overlay collisions with HUD components. Styled with a matching dark brown theme and a gold border outline.
*   **H Toggle Binding**:
    *   Added key bindings to toggle visibility of the legend via the `H` key using the Input System package correctly. Included instructions at the bottom of the panel: `"H Ẩn/hiện hướng dẫn"`.

---

## 25. NPC Return-to-Idle Rotation Polish
*   **Original Face Saving**:
    *   Stores `defaultRotation` of the NPC in `Start()` inside [NPCCharacter.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/NPCCharacter.cs).
*   **Idle Rotation Recovery**:
    *   Implemented `ReturnToDefaultRotation()` which slerps the NPC's Y-axis rotation back to its original orientation over `0.35s` once the player leaves proximity or targets another NPC, triggered inside [NPCProximityOptionsUI.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/UI/NPCProximityOptionsUI.cs).
    *   Added checks to prevent resetting if active dialogue panel is currently displaying conversation choices.

---

## 26. Merge Compatibility Fixes
*   **Post-merge compatibility fix for FrameworkMainMenuUI references**:
    *   Restored key binding fields (`keyMoveUp`, `keyMoveDown`, `keyMoveLeft`, `keyMoveRight`, `keyInteract`, `keyRun`) as `KeyCode` parameters in [PlayerController.cs](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerController.cs) for main menu settings binding display compatibility.
    *   Added `LoadKeyBindings()` and `TriggerRescueSequence()` stub methods to PlayerController.cs to prevent runtime errors and compilation failures after merging `origin/dev`.




