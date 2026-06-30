# AI Agent Rules: Đất Cày Lên Sỏi Đá

Tài liệu này định hình các quy tắc lập trình cốt lõi và giới hạn thiết kế mà các AI Coding Agent tiếp quản dự án bắt buộc phải tuân thủ nghiêm ngặt khi thực hiện code hoặc chỉnh sửa trong Unity.

---

## 1. Current Target (Mục tiêu hiện tại)
Đưa dự án trở thành một bản **chơi thử (Demo) dài khoảng 10-15 phút được đánh bóng tỉ mỉ dành cho môn học PRU213**, xoay quanh câu chuyện giúp đỡ một ngôi làng nghèo ở miền Trung Việt Nam chống chọi và hồi sinh sau thiên tai bão lũ miền Trung.

---

## 2. Core AI Coding Rules (10 quy tắc lập trình cốt lõi)

1.  **Không làm Stardew Valley Clone:** Tuyệt đối không mở rộng trò chơi thành game làm nông thế giới mở tự do quy mô lớn. 
2.  **Hạ thấp độ phức tạp hệ thống:** Không ưu tiên phát triển cơ chế kinh tế làm giàu, sạp giao thương thị trường, đa dạng chủng loại hạt giống cây trồng, hệ thống chế tạo phức tạp, tính năng lưu trữ dữ liệu Save/Load đa luồng, hoặc cây hội thoại đa nhánh.
3.  **Nghĩa Tình là thước đo chính:** Đảm bảo chỉ số thắt chặt lòng tin cộng đồng (**Nghĩa Tình score**) luôn là động lực chính dẫn dắt tiến trình gameplay và quyết định kết cục của trò chơi.
4.  **Làm nông để sinh tồn:** Cấu trúc hoạt động gieo trồng khoai lang tập trung phục vụ sản xuất thực phẩm tự cung tự cấp để Thành hồi phục thể lực và đóng góp lương thực cứu đói cho chòm xóm.
5.  **Bản sắc văn hóa Việt Nam trong gameplay:** Đảm bảo yếu tố văn hóa Việt Nam xuất hiện trực tiếp trong hành vi chơi game (đổi công Vần công chống bão, nghe Loa phát thanh xã lập kế hoạch ngày, quyên góp lương thực, thắp nhang đình làng cầu an), chứ không chỉ biểu hiện qua hình ảnh trang trí tĩnh.
6.  **Ưu tiên thay đổi nhỏ, dễ kiểm thử:** Thực hiện các bước sửa đổi code Unity nhỏ lẻ, rõ ràng, chạy Play Mode stress test thường xuyên để tránh xung đột vật lý hoặc camera.
7.  **Bảo tồn các hệ thống đang hoạt động ổn định:** Tuyệt đối không viết lại hoặc thay thế các cơ chế đang vận hành tốt (như PlayerController 3D, WeatherManager, hoặc Grid Inventory gốc) trừ khi chúng bị lỗi nghiêm trọng.
8.  **Sử dụng Simple Managers:** Tổ chức logic thông qua các lớp quản lý gọn nhẹ (GameManagers, PhaseManagers) cấu hình liên kết trực tiếp bằng tham chiếu Inspector trong Unity Editor thay vì code cứng.
9.  **Giới hạn tính năng mới:** Mọi dòng code hoặc tính năng mới bổ sung vào game bắt buộc phải hỗ trợ trực tiếp một trong các yếu tố:
    *   Vòng lặp làm nông cứu đói (Farming loop).
    *   Hoạt động tương trợ cộng đồng (Community help / Vần công).
    *   Sinh tồn thời tiết thiên tai (Disaster survival).
    *   Bản sắc văn hóa Việt Nam (Cultural identity).
    *   Đánh bóng chất lượng Demo cho môn học PRU213.
10. **Tránh Overengineering:** Sử dụng các biến trạng thái đơn giản, ScriptableObjects gọn nhẹ và hàm `Mathf.Lerp` thay vì cố gắng thiết kế các Framework lập trình đa tầng phức tạp hoặc Dialogue Graph cồng kềnh.

> **Ghi chú scope sau merge remote:** Project hiện có script experimental `CockfightingZone` / `CockfightingMinigame`. Agent không được mở rộng mini-game này thành hệ thống phụ lớn, không biến nó thành combat/betting/economy, và không đưa vào demo chính nếu chưa có quyết định thiết kế rõ ràng. Nếu giữ, chỉ frame như hoạt động tinh thần/văn hóa rất nhỏ, thưởng Morale nhẹ, không ảnh hưởng core progression Nghĩa Tình.
