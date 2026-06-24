# Project Overview: Đất Cày Lên Sỏi Đá

Dự án này là tài liệu tổng quan giới thiệu cấu trúc, định hướng thiết kế và giới hạn phạm vi phát triển của trò chơi dành cho Giảng viên hướng dẫn, thành viên trong nhóm, hoặc các AI Agent tiếp quản lập trình trong tương lai.

---

## 1. Academic Context & Project Goal
* **Bối cảnh học thuật:** Dự án được phát triển dưới dạng bản Demo thử nghiệm môn học **PRU213** (Lập trình game với Unity).
* **Mục tiêu dự án:** Phát triển một bản chơi thử (playable demo) chạy mượt mà trong vòng 2 tuần, chứng minh được vòng lặp gameplay cốt lõi (core loop) và truyền tải được thông điệp nghệ thuật - nhân văn sâu sắc.
* **Điểm số tối ưu:** **Điểm nhấn chấm điểm mạnh nhất của trò chơi không nằm ở sự phức tạp của hệ thống kỹ thuật (system complexity), mà nằm ở bản sắc văn hóa Việt Nam rõ nét được diễn tả trực tiếp qua gameplay** (Nghĩa Tình, Vần công, Loa phát thanh xã, biến động bão lũ miền Trung).

---

## 2. High Concept & Genre
* **Tên trò chơi:** Đất Cày Lên Sỏi Đá / Sown in Stone
* **Thể loại:** Narrative Community Survival Game with farming elements (Mô phỏng sinh tồn cộng đồng theo cốt truyện kết hợp yếu tố làm nông đơn giản).
* **Tóm tắt cốt truyện:** Trò chơi kể về Thành, một người con xa xứ quay trở về quê hương miền Trung sau biến cố gia đình. Thay vì làm nông để làm giàu, người chơi phải cải tạo ruộng vườn cằn cỗi để thu hoạch lương thực cứu đói, giúp đỡ bà con chòm xóm vượt qua các chu kỳ thiên tai khắc nghiệt (hạn hán, Gió Lào, mưa bão, lũ lụt), và chung tay tái thiết làng quê sau cơn lũ dữ.

---

## 3. Core Cultural Values
Trò chơi lấy các giá trị văn hóa Việt Nam làm trung tâm điều phối cơ chế gameplay:
* **Tình làng nghĩa xóm (Nghĩa Tình):** Đo lường sự tin tưởng của dân làng, tăng lên khi người chơi chia sẻ lương thực hoặc giúp việc tương trợ.
* **Vần công:** Phương thức đổi công lao động truyền thống. Dùng công sức hỗ trợ hàng xóm lúc bình thường để đổi lấy sự chung tay giúp đỡ của cả cộng đồng khi hoạn nạn đổ bộ.
* **Loa phát thanh xã:** Âm thanh cầu nối tin tức quen thuộc ở các vùng nông thôn, điều hướng nhịp sống và cảnh báo rủi ro thiên tai hằng ngày cho người chơi.

---

## 4. Current Gameplay Direction
Vòng lặp chơi thử tập trung kiểm nghiệm 4 giai đoạn thời tiết (Phases) tương ứng với 4 ngày trải nghiệm (mỗi ngày 5 phút thực):
1. **Dọn ruộng & canh tác vụ khoai đầu tiên** (cải tạo đất cát pha sỏi đá).
2. **Đối phó hạn hán Gió Lào** (quản lý thể lực dưới nắng nóng và giữ ẩm ruộng).
3. **Chống chịu mưa bão & lũ dâng ngập** (thu hoạch chạy lũ, chằng chống nhà cửa bằng công vần công tích lũy, bảo vệ lương thực khỏi ẩm mốc).
4. **Tái thiết trên đất phù sa** (thu hoạch vụ mùa bội thu từ lớp đất phù sa mới bồi đắp sau lũ).

---

## 5. Main Systems (Implemented or Planned)
* **Player Controller 3D (Đã triển khai):** Di chuyển nhân vật, chạy nhanh tiêu hao thể lực, action locks khi thực hiện cuốc đất/tưới nước.
* **Farming System (Đã triển khai):** Cuốc đất dọn đá cằn, gieo hạt tiêu hao tài nguyên hòm đồ, tưới nước ẩm đất và thu hoạch khoai tươi chín theo thời gian thực.
* **Weather & Water Level Lerping (Đã triển khai):** Thay đổi thời tiết mượt mà, hạt mưa bão bám camera và mực nước lũ dâng ngập thực tế trong scene 3D.
* **Nghĩa Tình & Vần Công System (Đã triển khai mẫu):** Hệ thống tích lũy công đổi công và điểm cộng đồng quyết định kết cục trò chơi.
* **Storage & Decay (Đã triển khai):** Kho chứa thực phẩm hỗ trợ cơ chế thối nông sản ẩm (fresh crop decay) và chế biến khoai gieo khô dự trữ.
* **NPC Dialogue & Interactive HUD (Đã triển khai):** Tương tác với O Thắm/Bác Năm mở menu thoại UI, thông báo HUD Toast khi thu hoạch hoặc có biến động thời tiết.

---

## 6. Scope Guidelines (What NOT to Overbuild)
Để đảm bảo tiến độ hoàn thành đúng hạn trong phạm vi dự án PRU213, **tuyệt đối không mở rộng hoặc xây dựng quá phức tạp** các tính năng sau:
* **Không làm Dialogue Trees phức tạp:** Chỉ sử dụng các đoạn hội thoại tuyến tính đơn giản hoặc bảng chọn nút bấm UI gọn nhẹ (Trò chuyện / Giúp đỡ / Trao đổi).
* **Không làm hệ thống Save/Load đồ sộ:** Bản Demo chạy liên tục trong khoảng 20 phút chơi, không yêu cầu lưu trữ dữ liệu đa tầng giữa các lần chơi.
* **Không thiết kế quá nhiều loại cây trồng:** Tập trung duy nhất vào cây Khoai Lang (Khoai tươi và Khoai gieo khô) để phục vụ cốt truyện cứu trợ.
* **Không làm cơ chế Crafting lớn:** Không cần hệ thống ghép đồ phức tạp kiểu chế tạo vũ khí/nông cụ. Các tương tác chế biến nông sản (phơi sấy khoai khô) được thực hiện qua các nút bấm tương tác nhanh tại Bếp lò.
* **Không xây dựng kinh tế phức tạp:** Tiền xu (Xu) chỉ giữ vai trò thứ cấp dùng để đổi công hoặc mua hạt giống ban đầu, loại bỏ các cơ chế đầu cơ, mua đi bán lại làm giàu.
