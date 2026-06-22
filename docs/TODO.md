# TODO: 2-Week Solo Development Plan

Tài liệu này vạch ra kế hoạch hành động chi tiết trong 2 tuần cuối của dự án. Mặc dù nhóm có 5 thành viên, việc triển khai mã nguồn Unity thực tế được xử lý bởi một người chính (Solo Developer). Kế hoạch này tập trung tối đa vào yếu tố thẩm mỹ, tính hấp dẫn và bản sắc văn hóa Việt Nam để ghi điểm cao nhất với giảng viên hướng dẫn.

---

## 1. Development Priorities

### Priority 0 - Must Finish (Bắt buộc phải hoàn thành)
*   [x] **Update README and docs:** Cập nhật toàn bộ tài liệu hướng dẫn và mô tả game theo hướng đi mới.
*   [x] **Stable player movement:** Tinh chỉnh chuyển động nhân vật 3D mượt mà, sửa lỗi va chạm hoặc kẹt địa hình.
*   [x] **Stable camera:** Camera theo sau nhân vật mượt mà từ góc nhìn thứ ba, xoay chuyển không bị giật lag.
*   [x] **Basic farming loop:** Hoàn thiện dọn sỏi đá, cuốc đất, gieo hạt khoai lang, tưới nước và thu hoạch.
*   [x] **Basic inventory count:** Bảng đếm số lượng hạt giống, khoai tươi và khoai gieo khô đơn giản trong túi đồ.
*   [x] **Nghĩa Tình UI:** Hiển thị điểm Nghĩa Tình trên HUD chính của người chơi.
*   [x] **At least 2 NPC interactions:** Tương tác thoại đơn giản bằng nút bấm UI với O Thắm (mua hạt giống/đổi công Vần công) và Bác Năm (đóng góp khoai cứu đói).
*   [x] **At least 3 community events:** Kích hoạt cảnh báo từ Loa phát thanh xã, đóng góp lương thực khi bão, và dọn dẹp sau bão.
*   [x] **Phase transition UI:** Hiển thị Banner cinematic thông báo chuyển ngày/Phase thời tiết kèm thông tin thông báo khẩn từ Loa phát thanh xã.
*   [x] **One ending screen:** Màn hình kết cục game hiển thị text/ảnh đánh giá dựa trên tổng điểm Nghĩa Tình đạt được ở cuối ngày thứ 4 (Phase 4).

### Priority 1 - Should Finish (Nên hoàn thành)
*   [x] **Weather visual polish:** Làm đẹp hệ thống hạt mưa chéo bám camera, hiệu ứng mờ sương (fog) và tông màu nắng nóng Gió Lào.
*   [x] **Flood visual polish:** Nước lũ dâng ngập mượt mà, phản chiếu ánh sáng và tạo cảm giác ngập lụt chân thực.
*   [x] **Silt soil mechanic:** Đất trồng ngập nước ở Phase 3 tự động chuyển thành đất phù sa màu mỡ (Silt Soil) ở Phase 4, nhân đôi sản lượng thu hoạch.
*   [x] **More Vietnamese environment props:** Đưa thêm các mô hình như lũy tre làng, cây chuối, bờ rào gỗ truyền thống vào bản đồ.
*   [x] **Sound effects:** Tiếng mưa rơi, gió rít Gió Lào, tiếng trống đình làng réo rắt báo bão, và tiếng loa phát thanh xã rè rè đặc trưng.

### Priority 2 - Nice To Have (Có thể làm thêm nếu thừa thời gian)
*   [x] **Cụ Bảy / Bé Tí:** Thêm mô hình 3D NPC Trưởng thôn (Cụ Bảy) đứng gần đình làng và Bé Tí chạy việc vặt.
*   [x] **Ancestral Altar interaction:** Tương tác thắp nhang tại bàn thờ tổ tiên để hồi phục Tinh thần (giảm Stress).
*   [ ] **More dialogue lines:** Thêm một vài dòng thoại đậm chất địa phương (Central Vietnam dialect) cho O Thắm, Bác Năm.
*   [ ] **Multiple ending illustrations:** Vẽ/tạo 3 hình ảnh minh họa tương ứng cho 3 kết cục Nghĩa Tình (Đất sỏi đá cằn, Lá lành đùm lá rách, Đất cày nở hoa).
*   [x] **Simple main menu polish:** Làm đẹp Menu chính ("Về quê bám đất") bằng nhạc nền nhẹ nhàng và hình ảnh làng quê.

### Do Not Prioritize (Tuyệt đối không làm để tránh trễ hạn)
*   🚫 Hệ thống Save/Load phức tạp.
*   🚫 Cây hội thoại phân nhánh sâu (Dialogue Trees).
*   🚫 Kinh tế thị trường/Giao dịch làm giàu phức tạp.
*   🚫 Quá nhiều loại cây trồng khác nhau (chỉ tập trung vào Khoai Lang).
*   🚫 Chế tạo trang bị/Nông cụ phức tạp.
*   🚫 Hệ thống cây kỹ năng (Skill tree) hoặc Chiến đấu (Combat).

---

## 2. Daily 14-Day Execution Plan

### Tuần 1: Củng cố core loop và UI cốt lõi

*   **Ngày 1 (Hôm nay):** Hoàn thành rà soát và cập nhật toàn bộ tài liệu dự án (`README`, `PROJECT_OVERVIEW`, `GAME_DESIGN`, `GAMEPLAY_SYSTEMS`).
*   **Ngày 2:** Kiểm tra và tinh chỉnh `PlayerController` 3D cùng `Camera Follow`. Loại bỏ triệt để các collider vô hình chặn đường đi trên bản đồ.
*   **Ngày 3:** Hoàn thiện cơ chế Farming gốc trên `SoilCell`: đảm bảo gieo hạt trừ đúng 1 hạt giống từ kho, tưới nước cập nhật ẩm mượt mà.
*   **Ngày 4:** Thiết kế giao diện HUD tối giản: Tích hợp thanh điểm Nghĩa Tình, số lượng Khoai tươi/Khoai gieo khô trực quan trên màn hình.
*   **Ngày 5:** Triển khai logic hội thoại đơn giản cho NPC O Thắm và Bác Năm (UI Popup có 3 nút bấm phân nhánh đơn giản).
*   **Ngày 6:** Viết code logic tích lũy điểm Nghĩa Tình và đổi công Vần công, lưu trữ tạm thời trong class `PlayerStats`/`CommunityManager`.
*   **Ngày 7:** Xây dựng màn hình kết thúc game (Ending Panel) tự động hiển thị đánh giá kết cục dựa trên điểm Nghĩa Tình khi hết Phase 4.

### Tuần 2: Thiên tai, Âm thanh và Đánh giá Thẩm mỹ

*   **Ngày 8:** Liên kết hệ thống chuyển Phase thời gian (`GameManager`) với `WeatherManager` để nội suy mượt mà qua các ngày 1, 2, 3, 4.
*   **Ngày 9:** Hoàn thiện cơ chế bão lũ dâng mực nước lũ (`waterLevel`) và kiểm tra ngập úng làm chết khoai lang tươi.
*   **Ngày 10:** Lập trình cơ chế đất phù sa (Silt Soil) ở Phase 4: tự động đổi texture đất, dọn sỏi đá và nhân đôi sản lượng thu hoạch.
*   **Ngày 11:** Trang trí thêm Assets môi trường (lũy tre, cây chuối, lu nước truyền thống Việt Nam) để tăng tính thẩm mỹ và bản sắc văn hóa.
*   **Ngày 12:** Tích hợp âm thanh môi trường (tiếng mưa bão rầm rì, tiếng loa phát thanh xã mở đầu ngày, tiếng trống dồn dập).
*   **Ngày 13:** Kiểm tra tổng thể (Stress test), cân bằng tốc độ mất thể lực dưới nắng Gió Lào và bão lũ để game có độ khó hấp dẫn.
*   **Ngày 14:** Xuất bản đóng gói Demo (Build Game), ghi hình video Gameplay Walkthrough và chuẩn bị tài liệu thuyết trình báo cáo trước giảng viên.
