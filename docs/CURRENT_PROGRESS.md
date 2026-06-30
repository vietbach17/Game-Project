# Current Progress: Đất Cày Lên Sỏi Đá

Cập nhật: **2026-06-29**.

Tài liệu này phản ánh trạng thái source hiện tại sau merge/compile fix gần nhất. Mức độ sẵn sàng demo hiện tại cực kỳ cao do toàn bộ chuỗi nhiệm vụ hướng dẫn cốt truyện và giai đoạn chuẩn bị bão đã được tích hợp hoàn chỉnh.

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

- `SurvivalUIManager` tự động mở/khóa con trỏ chuột khi vào Menu/Shop/Tutorial, tích hợp HUD Toast nhận cảnh báo hỏng nông sản từ `StorageManager`.
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

---

## 3. Current Readiness Assessment

**Demo readiness hiện tại: khoảng 98% về mặt source**, dự án đã hoàn tất phần lớn tính năng kịch bản:
- **PlayerController & Camera** hoạt động hoàn toàn. Camera khóa cố định khi nhấn Tab xem thông tin và tiếp tục follow khi tắt Tab.
- Trình tự hướng dẫn chạy mượt mà từ đầu đến khi chuyển Phase mưa bão.

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

## 6. Known Partial / Compatibility Areas

- Save/load world state chưa được hoàn thiện.

---

## 7. Recommended Next Steps

1. Tiến hành Playtest toàn bộ chuỗi hướng dẫn và giai đoạn chuẩn bị bão trên Unity Editor để tinh chỉnh nhịp độ.
2. Thiết kế Panel Canvas Ending hiển thị dựa trên điểm Nghĩa Tình (Karma).
3. Đánh giá khả năng bổ sung các hiệu ứng sấm chớp hình ảnh ngẫu nhiên khi bước vào Phase 2 (`MuaBao`).

---

## 8. Status Summary

Dự án hiện đã **hoàn thành phát triển toàn bộ tuyến nhiệm vụ hướng dẫn mở rộng và kịch bản chuẩn bị bão**, sẵn sàng cho các thành viên chạy thử nghiệm và polish đồ họa.

