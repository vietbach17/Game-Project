# AI Agent Task Execution: Đất Cày Lên Sỏi Đá

Tài liệu này định hình quy trình làm việc từng bước (workflow) và các tiêu chuẩn kỹ thuật bắt buộc khi một AI Coding Agent nhận nhiệm vụ lập trình tính năng mới trong dự án Unity.

---

## 1. Core Principles (Nguyên tắc cốt lõi)

1.  **Đọc ngữ cảnh trước khi viết code (Read context first):** Luôn đọc tài liệu `GAME_BIBLE.md` và `CURRENT_SCENE_STATE.md` để hiểu kiến trúc và các đối tượng đang có sẵn, tránh phán đoán sai lệch.
2.  **Chỉ làm một tính năng lớn tại một thời điểm (Single major feature):** Không lập trình song song nhiều hệ thống phức tạp cùng lúc. Chia tách nhiệm vụ thành các bước nhỏ dễ kiểm soát.
3.  **Thay đổi gia tăng, có kiểm soát (Incremental changes):** Thực hiện viết code từng phần nhỏ, mở Play Mode trong Unity để test hoạt động trước khi chuyển sang phần tiếp theo.
4.  **Cập nhật tiến độ sau khi hoàn thành (Update CURRENT_PROGRESS.md):** Sau khi hoàn thành bất kỳ nhiệm vụ nào, bắt buộc phải cập nhật tệp `docs/CURRENT_PROGRESS.md` để đồng bộ thông tin cho các agent tiếp theo.
5.  **Tài liệu hóa liên kết Inspector (Document Inspector assignments):** Nếu script mới yêu cầu liên kết kéo thả trong Inspector (ví dụ: gán GameObject, Prefab, hay Text UI), bắt buộc ghi rõ hướng dẫn thiết lập này vào tệp `README.md` hoặc tài liệu cập nhật.
6.  **Bảo tồn hệ thống đang chạy (Preserve working systems):** Kính trọng code cũ. Tránh thay thế các cơ chế chuyển động hoặc tính toán thời tiết sẵn có trừ khi bị lỗi logic nặng.
7.  **Tối giản hóa thiết kế (Avoid overengineering):** Chỉ sử dụng cấu trúc lập trình đơn giản nhất để hoàn thành mục tiêu (ưu tiên biến cục bộ, ScriptableObjects và if-else).

---

## 2. Step-by-Step AI Execution Workflow (Quy trình thực thi 5 bước)

```
Bước 1: Nghiên cứu & Đọc Scene State
   │
   ▼
Bước 2: Lập Kế Hoạch Chỉnh Sửa (task.md)
   │
   ▼
Bước 3: Viết Code & Gán Inspector
   │
   ▼
Bước 4: Chạy Thử (Play Mode Test) & Sửa Bug
   │
   ▼
Bước 5: Cập Nhật Tài Liệu Tiến Độ (CURRENT_PROGRESS.md)
```

### Bước 1: Nghiên cứu & Đọc Scene State
*   Đọc `docs/CURRENT_SCENE_STATE.md` để xác định script nào đang chịu trách nhiệm cho mảng tương ứng.
*   Grep tìm kiếm lớp/hàm liên quan trong thư mục `Assets/Scripts/` để hiểu cách dữ liệu được truyền đi (ví dụ: `PlayerStats` nắm giữ Xu và Thể lực).

### Bước 2: Lập Kế Hoạch Chỉnh Sửa
*   Xác định rõ các tệp `.cs` cần sửa hoặc viết mới.
*   Liệt kê các biến/hàm cần thêm và dự kiến cách tích hợp chúng lên UI Canvas.

### Bước 3: Viết Code & Gán Inspector
*   Lập trình logic lớp học.
*   Sử dụng thuộc tính `[SerializeField]` cho các tham chiếu ngoài thay vì dùng hàm `GameObject.Find` hay `GetComponent` ở thời gian chạy (Runtime) để tối ưu hóa hiệu suất và tránh lỗi tham chiếu null (`NullReferenceException`).

### Bước 4: Chạy Thử & Sửa Bug
*   Mở Unity Editor chạy thử.
*   Kiểm tra Console log để chắc chắn không sinh ra lỗi Exception.
*   Sử dụng phím Debug `F1` để đẩy nhanh quá trình kiểm tra (ví dụ: chuyển nhanh Phase để test lũ lụt).

### Bước 5: Cập Nhật Tài Liệu
*   Chuyển các đầu việc đã xong trong `docs/TODO.md` sang trạng thái `[x]`.
*   Cập nhật `docs/CURRENT_PROGRESS.md` (mô tả hệ thống mới đã hoạt động, các file đã thay đổi).
*   Ghi nhận các liên kết Inspector mới vào hướng dẫn cài đặt nếu có.
