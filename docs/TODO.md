# TODO: Development Status & Remaining Tasks

Tài liệu này tổng hợp tình trạng phát triển hiện tại và phân rã các nhiệm vụ kỹ thuật còn lại sau khi tái cấu trúc game theo mô hình 2 Phase chiến lược. Cập nhật lần cuối: **2026-06-26**.

---

## 1. Tình trạng tổng thể

**Demo Readiness: ~92%** — Vòng lặp core loop làm nông và tất cả các hệ thống nền tảng (Movement, Camera, Inventory, Proximity UI, Ending) đã hoàn thiện và vận hành ổn định[cite: 26]. ~8% còn lại tập trung vào việc wiring code điều phối sự kiện sơ tán, chèn bao cát và sửa tường sập sau bão[cite: 26].

---

## 2. Priority 0 — Bắt buộc (ĐÃ HOÀN THÀNH ✓)

*   [x] Stable player movement 3D camera-relative + Roblox camera orbit[cite: 26]
*   [x] Basic farming loop: dọn đá → gieo hạt → tưới nước → thu hoạch[cite: 26]
*   [x] Smart Bulk Planting dialog (lựa chọn hàng loạt/từng ô linh hoạt)[cite: 26]
*   [x] Inventory (StorageManager + Inventory UI panel slots 80x80)[cite: 26, 27]
*   [x] Nghĩa Tình HUD (NghiaTinhPanel slider tự động đổi màu theo 3 ngưỡng)[cite: 26, 27]
*   [x] NPC interactions: O Thắm + Bác Năm (Bảng Proximity Panel Roblox-style cách < 1.7m)[cite: 26, 27]
*   [x] NPC interactions: Cụ Bảy + Bé Tí (Đã xếp đặt mô hình 3D vào scene + Cấu hình thoại)[cite: 26, 27]
*   [x] Phase transition UI: Banner Loa phát thanh xã Việt hóa mờ dần sau 5 giây[cite: 26, 27]
*   [x] Ending system: 3 kết cục Sad/Normal/Best dựa trên tổng điểm Nghĩa Tình[cite: 26, 27]
*   [x] Weather system: Mưa Bão dâng nước lũ, Phù Sa auto bồi đắp ruộng đất[cite: 26]
*   [x] Flood Roof Survival: Teleport lên nóc Thanh_House, cấp mì tôm cứu trợ, nghỉ ngơi trên nóc[cite: 26, 27]
*   [x] Tutorial slideshow images (4 PNG assets chi tiết trong Resources)[cite: 26]
*   [x] Ancestral Altar interaction (Thắp nhang tiêu hao vật phẩm +10 Morale)[cite: 26]
*   [x] Kitchen Hearth (Bếp ga dưới mái hiên): sấy khoai khô chống thối + nấu ăn hồi Stamina[cite: 26, 27]
*   [x] Coracle (Thuyền thúng): điều khiển di chuyển linh hoạt trên mặt nước lũ tốc độ 2.5f[cite: 26]
*   [x] MudPuddle: Vũng bùn trơn trượt làm chậm tốc độ di chuyển nhân vật[cite: 26]
*   [x] Item Icons (8 icon PNG mộc mạc) bind chính xác vào ItemData assets[cite: 26]
*   [x] Toast Notifications với cơ chế Regex tự động highlight màu sắc chữ[cite: 26, 27]
*   [x] HUD layout đầy đủ: Time/Nghĩa Tình/Resource/Controls/Quest/Toast/Proximity[cite: 26]
*   [x] F1 Debug panel với 10 nút + Cột debug farming chuyên dụng tại x=540[cite: 26, 27]
*   [x] Shop UI (O Thắm): Menu giao thương tương trợ mua/bán kèm icon vật phẩm rõ ràng[cite: 26]
*   [x] Village_Demo scene: Tọa độ chuẩn hóa root layout,Well, Altar, Cột loa phát thanh[cite: 26, 27]
*   [x] 4x3 SoilCell grid (12 SoilCells standalone) với 3D visuals đồng bộ kích thước footprint[cite: 26, 27]
*   [x] SoilCell Highlight (Spawn khung viền vàng gold mờ khi lọt tầm tương tác)[cite: 26, 27]
*   [x] SweetPotato crop visual 2 stages (Cây non và cây chín sát đất tơi xới)[cite: 26]
*   [x] NPC Quest Marker (Dấu ! vàng bounce sin-wave trên đầu khi có event)[cite: 26, 27]
*   [x] NPC Look-at + Return-to-Idle rotation (Tự xoay hướng mặt về Player và tự trả góc)[cite: 26]

---

## 3. Priority 1 — Implementation Checklist Mới (CÒN LẠI CẦN LÀM LIỀN)

*   [ ] **Tutorial all-4-NPC gate**: Cấu hình `IntroQuests` trong `TutorialManager.cs` bắt buộc phải track đủ sự kiện đóng hội thoại của cả 4 NPC: O Thắm, Bác Năm, Cụ Bảy, Bé Tí mới mở slideshow[cite: 26].
*   [ ] **Altar storm trigger**: Viết hàm kết nối tương tác tại Ancestral Altar, khi thắp nhang xong sẽ gửi lệnh gọi `GameManager.Instance.TransitionToPhase(GamePhase.MuaBao)` để chủ động gọi bão đổ bộ lập tức[cite: 26].
*   [ ] **EvacuationTimerPanel HUD**: Triển khai thiết kế HUD layer Top-Center để hiển thị đồng hồ đếm ngược 45 giây khẩn cấp và tracking checkbox sơ tán[cite: 26, 27].
*   [ ] **45-second evacuation logic**: Lập trình biến đếm `rescuedCount` và countdown trong `PlayerController.cs`. Dắt đủ 4 người $\rightarrow$ Fade Out chuyển sang Phase 2 trên nóc nhà. Hết giờ không dắt đủ $\rightarrow$ Bật màn hình Drown Game Over[cite: 26].
*   [ ] **Roof refuge anchors wiring**: Thiết lập và kéo thả tham chiếu Inspector cho `Roof_Anchor_Nodes` dưới mái nhà `Thanh_House`[cite: 26, 27].
*   [ ] **Phase 2 PhuSa auto-apply**: Viết hàm quét khi lũ rút, tự động ép toàn bộ 12 ô đất `SoilCell_Grid1` → `SoilCell_Grid12` sang trạng thái Phù Sa màu xám ẩm, dọn sạch đá và cài sản lượng thu hoạch nông sản $\times 2$[cite: 26, 27].
*   [ ] **Flood Board functional remap**: Lập trình điểm Neo `Wall_Repair_Node` tại sạp O Thắm và nhà Bác Năm. Khi người chơi nhấn `[E]`, kiểm tra và khấu trừ `item_flood_board` trong kho để dựng lại vách vách tường gỗ mới nguyên vẹn[cite: 26, 27].
*   [ ] **Sandbag functional remap**: Lập trình các điểm Neo chèn mái trên nóc nhà. Khi bão rung lắc, player chạy đến nhấn `[E]` tiêu hao `item_sandbag` để đặt bao cát đè giữ ngói ngập gió, hồi tinh thần nhân vật[cite: 26, 27].
*   [ ] **Tutorial auto-start wiring**: Kết nối `TutorialManager.InitializeTutorial()` từ Main Menu hoặc GameManager khi người chơi nhấn chọn New Game[cite: 26].
*   [ ] **More dialogue lines**: Bổ sung thêm các câu thoại ngắn mang âm hưởng địa phương miền Trung cho 4 NPC ứng với mạch Chạy lũ và Tái thiết hậu lũ[cite: 26].

---

## 4. Priority 2 — Tối ưu hóa & Làm đẹp (TÙY THỜI GIAN)

*   [ ] **Ending Panel ảnh minh họa**: Vẽ hoặc phối cảnh 3 ảnh ngói đa giác mộc mạc tương ứng với 3 kết cục cốt truyện để hiển thị thay vì chỉ dùng text thô[cite: 26].
*   [ ] **Ambient sound polish**: Bổ sung hiệu ứng âm thanh môi trường dồn dập (tiếng gió hú rít ngoài trời bão, tiếng sấm nổ vang, tiếng trống đình làng réo rắt báo động)[cite: 26].
*   [ ] **Menu Settings InputAction**: Cập nhật file `FrameworkMainMenuUI.cs` đọc trực tiếp từ hệ thống InputAction mới thay vì dùng KeyCode legacy[cite: 26].
*   [ ] **BoundaryElement fix**: Thay thế các MeshCollider tảng đá biên giới nhỏ bằng các BoxCollider tàng hình bao quanh rìa map 50m x 50m để chặn player đi lạc[cite: 26].

---

## 5. KHÔNG LÀM (Tuyệt đối không chạm để tránh trễ hạn demo)

*   🚫 Hệ thống Save/Load dữ liệu đa tầng phức tạp giữa các màn chơi[cite: 26].
*   🚫 Cây hội thoại phân nhánh sâu đa luồng (Dialogue Trees/Nodes Graph)[cite: 26].
*   🚫 Kinh tế thị trường đầu cơ, mua đi bán lại làm giàu phức tạp[cite: 26].
*   🚫 Nhiều loại cây trồng nông nghiệp (Chỉ tập trung duy nhất cây Khoai Lang)[cite: 26].
*   🚫 Chế tạo vũ khí, sửa chữa nông cụ, Skill tree nâng cấp nhân vật hoặc hệ thống Chiến đấu (Combat)[cite: 26].
*   🚫 Chế độ chơi mạng Multiplayer[cite: 26].

---

## 6. Deprecated / Không còn làm theo flow cũ (XÓA BỎ)

*   ~~Gia cố ván gỗ trước cửa nhà ở Phase 1 trước khi bão tới~~ $\rightarrow$ Đã chuyển thành Phase 2 wall repair dựng lại tường vách sập sau bão để hợp lý hóa mạch kịch bản[cite: 26].
*   ~~Đặt bao cát chặn nước lũ tràn vào sân vườn ở Phase 1~~ $\rightarrow$ Đã chuyển thành Phase 2 roof defense utility chèn giữ ngói rung chống bão tốc mái trên nóc nhà Thành[cite: 26].
*   ~~Giai đoạn thời tiết Gió Lào độc lập kéo dài ngày game~~ Bars $\rightarrow$ Đã XÓA BỎ hoàn toàn làm phase riêng, chỉ giữ hiệu ứng visual nắng nóng làm mood phụ lồng ghép trong Phase 1[cite: 26].
*   ~~Tự động chuyển Phase thời tiết chỉ theo số ngày đếm lịch tĩnh~~ $\rightarrow$ Đã chuyển sang cơ chế chủ động kích hoạt bão khẩn cấp thông qua hành động thắp nhang cúng của Player tại Altar[cite: 26].