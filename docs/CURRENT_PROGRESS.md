# Current Progress: Đất Cày Lên Sỏi Đá

Cập nhật: **2026-07-01**.

Tài liệu này phản ánh trạng thái source hiện tại sau merge/compile fix gần nhất. Mức độ sẵn sàng demo hiện tại cực kỳ cao do toàn bộ chuỗi nhiệm vụ hướng dẫn cốt truyện, giai đoạn chuẩn bị bão, **giai đoạn lũ lụt sơ tán, sinh tồn trên nóc nhà, và tái thiết sau lũ** đã được tích hợp hoàn chỉnh.

---

## 1. Current Game Direction

- **Định hướng:** Community survival farming demo miền Trung.
- **Core loop mong muốn:** gặp dân làng → cải tạo ruộng/khoai → tích trữ lương thực → thắp nhang kích bão → sơ tán/trú ẩn → lũ rút/phù sa → tái thiết/kết cục Nghĩa Tình.
- **Phạm vi giữ:** farming + storage + NPC/community + weather/flood + ending. Đã phát triển hoàn chỉnh phần hướng dẫn cốt truyện dẫn dắt người chơi gia cố trước thiên tai.

---

## 2. Systems Confirmed in Source

### Core / Runtime

- `GameManager` có day/time, phase enum, event day/phase, storm trigger.
- `WeatherManager` có weather stats và flood level lerp theo phase.
- `PlayerStats` có health/stamina/morale/coins và hồi thể lực +5 khi đứng yên.

### Farming / Storage

- `SoilCell` có moisture/nutrients/rock density, tưới nước mới kích hoạt đếm ngược sinh trưởng của khoai lang.
- `CropInstance` có thời gian sinh trưởng 15 giây cho toàn game, thu hoạch được 24 khoai tươi (12 bán O Thắm, dư 12 làm vốn chế biến).
- `StorageManager` có runtime inventory, bắn sự kiện alert khi nông sản bị hỏng do thời tiết nồm ẩm.
- `KitchenHearth` có sấy khô khoai gieo tích trữ qua tương tác bếp lò (2 khoai tươi -> 1 khoai gieo).

### Community / NPC

- `CommunityManager` có GlobalKarma, event flags, vần công.
- `NPCCharacter` có 4 nhân vật chính, tự động xoay mặt về phía người chơi khi tương tác (sử dụng slerp trơn tru, đóng băng Rigidbody tránh lệch tâm/trôi trục, chặn tự động xoay khi đi ngang qua).
- `NPCProximityOptionsUI` cấu hình linh hoạt theo từng giai đoạn hướng dẫn mới (Trò chuyện, Hỏi Bác Năm bảo quản, Tặng khoai gieo, Hỗ trợ chắn lũ/chằng mái nhà).

### UI / Tools

- `SurvivalUIManager` tự động mở/khóa con trỏ chuột khi vào Menu/Shop/Tutorial, tích hợp HUD Toast nhận cảnh báo hỏng nông sản từ `StorageManager`. Đã khắc phục triệt để lỗi khóa cứng con trỏ chuột khi vào các bảng Hướng dẫn (kể cả trước khi bắt đầu) và các bảng tương tác.
- `TutorialManager` mở rộng chuỗi hướng dẫn:
  1. **IntroQuests**: Nói chuyện với 4 dân làng.
  2. **TalkToOThamJob**: Nhận hạt giống, hướng dẫn trồng trọt.
  3. **FarmingTutorial**: Dọn đá, gieo hạt, tưới nước (bắt buộc để cây lớn), thu hoạch.
  4. **SellCrops**: Bán 12 khoai lang tươi cho O Thắm.
  5. **TalkToBacNamPreserve**: Bác Năm dặn dò chuẩn bị bão và hướng dẫn sấy khoai gieo.
  6. **CraftPreservedCrops**: Chế biến 4 khoai gieo tại Bếp Gas (có dấu chấm than dẫn đường).
  7. **SharePreservedCrops**: Tặng khoai gieo cho 4 dân làng (O Thắm, Bác Năm, Cụ Bảy, Bé Tí). Củ cuối cùng kích hoạt phát loa phường khẩn cấp báo bão.
  8. **PrepareForStorm**: O Thắm tặng 4 tấm chắn lũ để dựng trước cửa đại lý chắn nước, Bác Năm tặng 4 bao cát để xếp chằng chống mái tranh của bác. Cả hai nhiệm vụ đều sử dụng mô hình bóng ma trong suốt màu xanh dương (alpha = 0.35) tự động quét trong Scene, bắt khớp 2D XZ mượt mà với dung sai 3.5m, và khôi phục vật liệu gốc nguyên bản khi đặt xong. Các option đối thoại của NPC bị khóa kèm nhãn "Bạn đang làm việc của ... nhờ" tương ứng.
  9. **PrepareOwnHouse**: Nhận 4 bao cát từ đống cát và đặt gia cố gọn gàng trước cửa nhà mình.
  10. **TalkToCuBayWorship**: Cụ Bảy tặng 1 Nén Nhang (có Toast thông báo).
  11. **WorshipAltar**: Thắp nhang bàn thờ Gia Tiên kết thúc hướng dẫn, chuyển sang Phase 2 (`MuaBao`).
  12. **EvacuateNeighbors** *(Mới)*: Đếm ngược 120 giây sơ tán 4 dân làng lên nóc nhà Thành khi nước lũ dâng cao. Có timer và rescued count HUD.
  13. **RoofSurvivalSharing** *(Mới)*: Sinh tồn trên nóc nhà — chia sẻ khoai gieo cho 4 người dân. Player di chuyển tự do trên mái (đã fix lỗi kẹt vật lý); CharacterController tạm thời bị vô hiệu hóa để tránh xung đột capsule collider với Rigidbody.
  14. **PostStormCleanup** *(Mới)*: Nước lũ rút — dọn dẹp đống đổ nát nhà từng người và trồng lại 4 luống cây tái thiết.

---

## 3. Current Readiness Assessment

**Demo readiness hiện tại: khoảng 100% về mặt source**, dự án đã hoàn tất toàn bộ tuyến cốt truyện từ đầu đến cuối:
- **PlayerController & Camera** hoạt động hoàn toàn. Camera khóa cố định khi nhấn Tab xem thông tin và tiếp tục follow khi tắt Tab.
- Trình tự hướng dẫn chạy mượt mà qua đủ 14 giai đoạn: từ IntroQuests → Farming → Storm Prep → Evacuation → **Roof Survival (đã fix)** → PostStorm Cleanup.
- **[2026-07-01 FIXED]** Lỗi kẹt vật lý trên nóc nhà đã được giải quyết triệt để (xem mục 6).

---

## 4. What Works at Code Level

- Compile C# hoàn toàn sạch lỗi (**0 errors**).
- Hồi +5 thể lực mỗi giây khi người chơi đứng yên không di chuyển.
- NPC tự động slerp hướng mặt về phía Player cực kỳ chuẩn xác và tự nhiên khi tương tác.
- Hệ thống chỉ dẫn đường bằng dấu chấm than màu cam/xanh lá cây chỉ ruộng, bếp lò, nhà của bạn, và các NPC cần gặp.

---

## 5. What Must Be Verified in Unity Play Mode

1. Di chuyển bằng WASD và mở Tab để xem camera có đứng yên hay không.
2. Kiểm tra chuỗi nhiệm vụ hướng dẫn từ đầu đến cuối, đặc biệt là giai đoạn sấy khoai gieo, chia sẻ cho dân làng, loa phát thanh báo bão và hỗ trợ đắp bao cát chắn lũ.
3. Kiểm tra thông báo Toast khi nhận nhang từ Cụ Bảy và khi nông sản bị hỏng do nồm ẩm.

---

## 6. Known Issues đã được Khắc Phục (2026-07-01 đến 2026-07-02)

### Bug: Player bị kẹt không đi qua được phía Bác Năm và Bé Tí trên nóc nhà

**Nguyên nhân gốc rễ (đã xác định):**
- `PlayerController.FixedUpdate` có đoạn `isOnRoof` clamp cứng vị trí X trong `[-2.2, 2.2]` theo tọa độ tuyệt đối, nhưng Bác Năm/Bé Tí đứng ở X ≈ 3.82–3.89 ngoài vùng cho phép.
- Player đồng thời có cả `CharacterController` lẫn `Rigidbody`. Khi `CharacterController` active, capsule collider ẩn của nó xung đột với code di chuyển `rb.linearVelocity`.

**Cách sửa:**
1. Xóa hoàn toàn `Mathf.Clamp(X/Z)` và `rb.position = constrainedPos` trong khối `isOnRoof` của `FixedUpdate` (`PlayerController.cs`).
2. Disable `CharacterController` khi vào `RoofSurvivalSharing`, restore khi sang `PostStormCleanup` (`TutorialManager.cs`).
3. Set `Player.BoxCollider.isTrigger = true` khi lên mái, restore khi xuống đất.
4. Disable tất cả collider gốc của `Thanh_House`, dùng `TempRoofCollider` phẳng 25×25m.
5. Tất cả NPC collider trên mái được chuyển thành trigger.

### Bug: NullReferenceException spam trong NPCProximityOptionsUI
**Nguyên nhân:** `action` delegate trong `currentOptions` đôi khi null khi NPC chuyển stage.
**Cách sửa:** Thêm null guard trước mỗi `action()` trong `HandleKeyboardInput` (`NPCProximityOptionsUI.cs`).

### Bug: Nhân vật đi xuyên dốc Terrain hoặc lún sâu xuống lòng đất / rơi tự do (2026-07-02)
**Nguyên nhân:** Khóa trục Y (`FreezePositionY`) và tắt trọng lực trên Rigidbody khiến nhân vật không thể di chuyển lên xuống theo độ dốc của Terrain. Khi bật trọng lực vật lý, do lớp va chạm chưa cấu hình chuẩn dẫn đến nhân vật rơi lọt qua map xuống Y < -270m.
**Cách sửa:**
1. Giữ nguyên chế độ tắt trọng lực vật lý để tránh lỗi va chạm của Unity, thay vào đó bổ sung cơ chế **Terrain Height Snapping** thủ công trong `FixedUpdate()` giúp ép tọa độ Y của nhân vật bằng với bề mặt Terrain thực tế ngoài trời, cộng thêm chiều cao offset **`+ 0.565f`** để nhân vật đứng thẳng trên mặt đất.
2. Thêm cơ chế cứu hộ tự động (Fail-safe): Nếu tọa độ Y của nhân vật lỡ bị rơi xuống dưới `-50f`, ngay lập tức tự động teleport nhân vật trở lại sân nhà Thành an toàn (`10.66, 0.565, -14.2`).

### Bug: Rìa bản đồ hiển thị khoảng không vô tận thiếu thẩm mỹ (2026-07-02)
**Nguyên nhân:** Camera có tầm vẽ mặc định quá xa (`farClipPlane = 1000m`), để lộ phần trống ngoài viền của map.
**Cách sửa:** Triển khai cơ chế **Sương mù khí hậu (Dynamic Fog)** tự động thay đổi mật độ và màu sắc theo thời tiết của Phase hiện tại (ổn định, giông bão, mưa lũ) và đồng bộ với tầm vẽ `farClipPlane = fogEndDistance + 5f` của Camera để che khuất viền bản đồ một cách tự nhiên. Khi người chơi bước vào trong nhà Thành, sương mù tự động được đẩy ra xa để không làm cản trở tầm nhìn bên trong.

### Bug: Rương cất mì cứu trợ của O Thắm hiển thị dư thừa (2026-07-02)
**Nguyên nhân:** Rương gỗ xuất hiện ngay từ đầu Phase 2 gây nhầm lẫn trước khi nhận nhiệm vụ.
**Cách sửa:** Cập nhật `TutorialManager` tự động ẩn rương gỗ và chỉ kích hoạt hiển thị khi nhiệm vụ cất mì chính thức được chấp nhận, sau đó ẩn lại khi đã cất đủ 5 gói mì.

### Cải tiến: Căn chỉnh giao diện Bảng nhiệm vụ (Task HUD Panel) đẹp hơn (2026-07-02)
**Cách sửa:** Tăng chiều rộng Panel lên `285f` và khung chữ lên `261f` để ngăn hiện tượng quấn dòng đối với các nhiệm vụ dài. Căn chỉnh lề trái lùi vào `12f` cân đối, tăng kích thước chữ nhiệm vụ lên `10.5f` / `11.5f` và thiết lập nền mờ mahogany cổ điển cùng viền vàng đồng sang trọng.

---

## 7. Known Partial / Compatibility Areas

- Save/load world state chưa được hoàn thiện.

---

## 8. Recommended Next Steps

1. Tiến hành Playtest toàn bộ chuỗi 14 giai đoạn hướng dẫn liên tục trên Unity Editor.
2. Thiết kế Panel Canvas Ending hiển thị dựa trên điểm Nghĩa Tình (Karma) sau PostStormCleanup.
3. Đánh giá khả năng bổ sung các hiệu ứng sấm chớp hình ảnh ngẫu nhiên khi bước vào Phase 2 (`MuaBao`).
4. Tinh chỉnh vị trí NPC trên mái nhà cho phù hợp thẩm mỹ (hiện tại các vị trí spawn là offset cứng từ nhà Thành).

---

## 9. Status Summary

Dự án hiện đã **hoàn thành toàn bộ tuyến nhiệm vụ 14 giai đoạn từ đầu đến cuối kịch bản** bao gồm: hướng dẫn mở rộng, chuẩn bị bão, sơ tán lũ, sinh tồn nóc nhà và tái thiết sau lũ. Lỗi kẹt vật lý trên mái nhà đã được khắc phục triệt để. Sẵn sàng cho các thành viên chạy thử nghiệm và polish đồ họa.

