# Input Mapping: Đất Cày Lên Sỏi Đá

Tài liệu này quy chuẩn các thiết lập phím bấm điều khiển và quy tắc ưu tiên xử lý va chạm tương tác trong Unity dành cho bản chơi thử PRU213.

---

## 1. Keyboard & Mouse Controls (Thiết lập phím bấm)

### Movement (Di chuyển)
*   **`W, A, S, D`**: Di chuyển nhân vật Thành trong không gian 3D.
*   **`Left Shift`**: Chạy nhanh (tăng tốc độ di chuyển và tiêu hao Thể lực theo thời gian).

### Camera (Góc nhìn)
*   **`Giữ chuột phải + Di chuột`**: Xoay xoay camera tự do xung quanh nhân vật (chặn góc ngẩng/cúi từ 15° đến 65°).
*   **`Cuộn bánh xe chuột (Mouse Wheel)`**: Phóng to / thu nhỏ camera (Zoom) mượt mà trong khoảng giới hạn từ 3 đến 10 đơn vị.
*   **`Di chuyển camera-relative`**: Phím di chuyển WASD luôn đi theo hướng camera (nhấn W đi thẳng về phía trước theo hướng camera chiếu xuống mặt phẳng XZ).

### Core Interactions & Menus (Tương tác & Giao diện)
*   **`E`**: Phím tương tác ngữ cảnh chính (Cuốc đất, dọn đá, tưới nước, gieo hạt, thu hoạch, thắp nhang bàn thờ, gia cố nhà cửa).
*   **`1, 2, 3`**: Phím số chọn nhanh các hành động trong bảng Proximity popup nổi của NPC khi đứng gần.
*   **`Tab` hoặc `I`**: Mở/tắt giao diện túi đồ (Inventory).
*   **`C`**: Mở/tắt bảng chi tiết điểm Nghĩa Tình và công đổi công Vần công.
*   **`M`**: Mở/tắt bảng thông tin chi tiết thời tiết, stress sinh lý và dự báo lũ lụt.
*   **`H`**: Ẩn/hiện bảng hướng dẫn phím bấm điều khiển (Controls Legend HUD).
*   **`F1`**: Bật/tắt bảng điều khiển Debug nhanh dành cho nhà phát triển (test chỉ số nhanh).

---

## 2. Context-Based Actions (Tương tác theo ngữ cảnh)

Khi người chơi đứng gần các nhóm đối tượng và nhấn phím tương ứng:

*   **Tương tác Đất trồng (`SoilCell`) [Phím E]:**
    1.  *Có đá cằn:* Nhấn E để Dọn sỏi đá (giảm mật độ đá).
    2.  *Đất chưa cuốc:* Nhấn E để Cuốc đất (chuẩn bị gieo trồng).
    3.  *Đất trống:* Nhấn E để Gieo hạt giống (tiêu thụ hạt giống từ hòm đồ).
    4.  *Đất khô mầm:* Nhấn E để Tưới nước (tăng độ ẩm đất).
    5.  *Cây chín:* Nhấn E để Thu hoạch khoai tươi.
*   **Tương tác Dân làng (`NPCCharacter`) [Bảng Proximity - Click hoặc Phím 1/2/3]:**
    *   Tự động xuất hiện bảng Proximity popup khi đứng gần (`<= 1.7m`).
    *   Nhấp chuột hoặc nhấn phím số `1`, `2`, `3` tương ứng để thực hiện lựa chọn: Trò chuyện (Talk), Đóng góp khoai / Sự kiện (Give food / Events), Giúp việc đổi công (Help work).
    *   *Lưu ý:* Phím `E` / `Space` không còn kích hoạt hội thoại NPC trực tiếp hay hiển thị câu chào để tránh trùng lặp, chỉ giữ chức năng đóng hội thoại/tiến tới câu thoại tiếp theo khi hộp hội thoại lớn đã hiển thị.
*   **Tương tác Thiên tai & Ứng phó (`DisasterTarget`):**
    *   *Gần Repair Target (Mái nhà, rào tre):* Nhấn E để chằng chống, gia cố nhà cửa (tiêu hao điểm Vần công).
    *   *Gần Delivery Target (Rương cứu trợ):* Nhấn E để ký gửi lương thực cứu tế (quyên góp khoai).

---

## 3. Interaction Overlap Priority Rules (Quy tắc ưu tiên tương tác)

Trong trường hợp các vùng va chạm (Trigger Colliders) của nhiều đối tượng tương tác nằm đè lên nhau, phím **`[E]`** sẽ ưu tiên xử lý theo thứ tự sau:

```
Nhấn phím [E]
  │
  ├── 1. Disaster Target? ────► [Nếu đang ở Phase 2: After the Storm,
  │                            runtime mốc MuaBao/PhuSa]
  │                            Thực hiện chằng chống/gia cố/quyên góp cứu trợ/tái thiết
  │
  └── 2. SoilCell ruộng vườn? ──► Thực hiện dọn đá / tưới nước / gieo hạt / thu hoạch
```

*   *Lý giải thiết kế:* Với việc chuyển toàn bộ lựa chọn tương tác NPC sang bảng Proximity nổi tự động kích hoạt, phím `E` hoàn toàn được giải phóng khỏi NPC, giảm thiểu triệt để lỗi xung đột điều khiển khi đứng cạnh ruộng vườn của Bác Năm hay O Thắm.
