# TODO: Development Status & Remaining Tasks

Tài liệu này tổng hợp tình trạng phát triển hiện tại và các nhiệm vụ còn lại. Cập nhật lần cuối: **2026-06-26**.

---

## 1. Tình trạng tổng thể

**Demo Readiness: ~92%** — Gameplay core loop và tất cả systems chính đã hoàn thiện và ổn định. Phần còn lại là polish và nội dung visual.

---

## 2. Priority 0 — Bắt buộc (ĐÃ HOÀN THÀNH ✓)

*   [x] Stable player movement 3D camera-relative + Roblox camera orbit
*   [x] Basic farming loop: dọn đá → gieo hạt → tưới nước → thu hoạch
*   [x] Smart Bulk Planting dialog (hàng loạt/từng ô)
*   [x] Inventory (StorageManager + Inventory UI panel)
*   [x] Nghĩa Tình HUD (NghiaTinhPanel slider đổi màu)
*   [x] NPC interactions: O Thắm + Bác Năm (Proximity Panel Roblox-style)
*   [x] NPC interactions: Cụ Bảy + Bé Tí (đã đặt scene + Proximity Panel)
*   [x] Community events: Phase 1 chuẩn bị trước bão, Phase 2 ngập lũ và tái thiết sau lũ
*   [x] Phase transition UI: Banner Loa phát thanh xã Việt hóa
*   [x] Ending system: 3 kết cục Sad/Normal/Best dựa trên Nghĩa Tình
*   [x] Weather system: Mưa Bão flood, Phù Sa soil upgrade
*   [x] Flood Roof Survival: Teleport lên nóc nhà, mì tôm cứu trợ, nghỉ ngơi trên nóc
*   [x] Tutorial system 2 giai đoạn: IntroQuests (gặp cả 4 NPC) + FarmingTutorial với HUD quest panel
*   [x] Tutorial slideshow images (4 PNG assets)
*   [x] Ancestral Altar interaction (thắp nhang +10 Morale)
*   [x] Kitchen Hearth (bếp ga): sấy khoai + nấu ăn
*   [x] Coracle (thuyền thúng): di chuyển trên nước lũ
*   [x] MudPuddle: làm chậm di chuyển
*   [x] Item Icons (8 icon PNG) bind vào ItemData assets
*   [x] Toast Notifications với Regex highlight màu
*   [x] HUD layout đầy đủ: Time/Nghĩa Tình/Resource/Controls/Quest/Toast/Proximity
*   [x] F1 Debug panel với 10 nút + Farming debug column
*   [x] Shop UI (O Thắm): Buy/Sell với icon
*   [x] Village_Demo scene: tọa độ chuẩn hóa, 4 NPC, 3 nhà, well, shrine, speaker
*   [x] 4x3 SoilCell grid (12 SoilCells) với 3D visuals (Rocky/Clean/Tilled/Wet)
*   [x] SoilCell Highlight (viền vàng gold)
*   [x] SweetPotato crop visual 2 stages
*   [x] Silt Soil mechanic (Phù Sa) Phase 2 (harvest ×2)
*   [x] NPC Quest Marker (dấu ! bounce)
*   [x] NPC Look-at + Return-to-Idle rotation

---

## 3. Priority 1 — Nên hoàn thành (CÒN LẠI)

*   [ ] **Tutorial auto-start**: Kết nối `TutorialManager.InitializeTutorial()` từ GameManager khi game bắt đầu (hiện phải trigger thủ công).
*   [ ] **Multiple ending illustrations**: Tạo 3 ảnh minh họa cho 3 kết cục (Đất sỏi đá cằn, Lá lành đùm lá rách, Đất cày nở hoa) — hiện chỉ có text.
*   [ ] **More dialogue lines**: Thêm thoại địa phương miền Trung cho O Thắm, Bác Năm, Cụ Bảy, Bé Tí theo từng Phase (hiện còn ít thoại).
*   [ ] **Flood Board / Sandbag mechanic**: `item_flood_board` và `item_sandbag` đã có data asset nhưng `FloodBarrier.cs` chưa được kết nối gameplay đầy đủ.
*   [ ] **Nón Lá mechanic**: `item_non_la` đã có data + icon nhưng chưa có hiệu ứng gameplay (giảm stress thời tiết khi trang bị ngoài trời).

---

## 4. Priority 2 — Nice to have (TÙY THỜI GIAN)

*   [ ] **Ending Panel ảnh minh họa**: 3 kết cục có ảnh đẹp.
*   [ ] **Ambient sound polish**: Tiếng mưa bão, tiếng gió rít, tiếng trống đình làng réo rắt.
*   [ ] **Camera angle preset**: Nút bấm reset về góc nhìn mặc định khi bị xoay lạc.
*   [ ] **Menu Settings InputAction**: Cập nhật FrameworkMainMenuUI đọc từ InputAction thay vì KeyCode legacy.
*   [ ] **BoundaryElement fix**: Thay MeshCollider nhỏ bằng BoxCollider bao quanh rìa map.
*   [ ] **Village decorations**: Thêm lu nước, thúng khoai, các prop truyền thống Việt Nam.

---

## 5. KHÔNG LÀM (Tuyệt đối không để tránh trễ hạn)

*   🚫 Hệ thống Save/Load phức tạp
*   🚫 Cây hội thoại phân nhánh sâu (Dialogue Trees)
*   🚫 Kinh tế thị trường / làm giàu phức tạp
*   🚫 Nhiều loại cây trồng (chỉ Khoai Lang)
*   🚫 Chế tạo nông cụ / Skill tree / Combat
*   🚫 Multiplayer

---

## 6. Known Issues cần theo dõi

| Issue | Mức độ | Giải pháp hiện tại |
|-------|--------|-------------------|
| BoundaryElement scale nhỏ | Low | Hoạt động như tường vô hình, chấp nhận được cho demo |
| Camera SphereCast giật mái nhà | Low | Cài smoothTime đủ chậm để ít thấy |
| Menu KeyCode legacy | Low | Chờ P1 fix sau |
| TutorialManager auto-start | Medium | Cần wiring vào GameManager.Start() |
| FloodBarrier chưa gameplay | Low | Item data đã sẵn sàng, chờ implement |
| Cockfighting minigame mới merge | Medium | Cần quyết định giữ/bỏ. Hiện coi là experimental, không mở rộng nếu chưa chứng minh phục vụ trực tiếp Morale/cultural identity mà không làm lệch scope demo |

---

## 7. Files cần bảo vệ (KHÔNG SỬA BỪA BÃI)

| File | Lý do |
|------|-------|
| `PlayerController.cs` | Core movement + farming interactions |
| `SurvivalUIManager.cs` | Toàn bộ HUD runtime generation |
| `TutorialManager.cs` | Tutorial flow delicate |
| `NPCProximityOptionsUI.cs` | NPC interaction UI |
| `NPCCharacter.cs` | NPC logic + event tracking |
| `CommunityManager.cs` | Nghĩa Tình scoring |
| `SoilCell.cs` | Farming state machine |
| `GameManager.cs` | Phase/day cycle |
| `WeatherManager.cs` | Weather + flood |
| `Village_Demo.unity` | Scene chính, inspector references |
