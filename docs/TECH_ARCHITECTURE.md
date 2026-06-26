# Technical Architecture: Đất Cày Lên Sỏi Đá

Tài liệu này định hình cấu trúc lập trình C# trong Unity 6 cho dự án PRU213. Kiến trúc được thiết kế tối giản, áp dụng nguyên tắc Single Responsibility (Đơn nhiệm) để một người lập trình chính có thể quản lý và phát triển nhanh chóng trong 2 tuần mà không bị quá tải.

---

## 1. Core Classes & Directory Mappings (Danh sách lớp lập trình)

### A. Core Systems (Hệ thống cốt lõi)
*   **`GameManager`**: Quản lý vòng đời trò chơi, quản lý thời gian ngày/đêm và điều phối các hệ thống con. Ở tầng thiết kế, game có 2 phase chính: **Before the Storm** và **After the Storm**. Ở tầng code hiện tại, `GamePhase` vẫn là các mốc chi tiết `LapNghiep`, `GioLao`, `ChuanBiBao`, `MuaBao`, `PhuSa`.
*   **`PhaseManager`**: Khái niệm thiết kế cho trình tự 2 phase chính; hiện phần lớn điều phối thực tế nằm trong `GameManager` và các manager liên quan.
*   **`UIManager`**: Bắt sự kiện thay đổi dữ liệu để cập nhật HUD (thanh máu, thể lực, thanh Nghĩa Tình, hòm đồ) và hiển thị popups thoại.
*   **`AudioManager`**: Kích hoạt phát nhạc nền (BGM) và hiệu ứng âm thanh môi trường (SFX mưa rít, sấm sét, loa rè).

### B. Player Controller (Điều khiển nhân vật)
*   **`PlayerController`**: Đọc đầu vào bàn phím/chuột để xử lý di chuyển, chạy nhanh và khóa hành động khi đang tương tác.
*   **`PlayerInteraction`**: Dò tìm các đối tượng `IInteractable` trong vùng Trigger và gọi lệnh thực thi khi người chơi nhấn `[E]`.
*   **`PlayerStats`**: Lưu giữ các chỉ số sinh lý tức thời như Máu (Health), Thể lực (Stamina), Tinh thần (Morale), và Trạng thái Stress nhiệt/lạnh.

### C. Farming & Soil (Canh tác & Ruộng vườn)
*   **`SoilCell`**: Đại diện cho 1 ô đất trồng trong lưới 4x3 (12 SoilCells). Lưu trữ ẩm độ đất, mức độ dinh dưỡng, độ sỏi đá và tham chiếu đến `CropInstance` đang gieo.
*   **`CropData` (ScriptableObject)**: Lưu trữ cấu hình tĩnh của cây trồng (Số ngày chín, khả năng chịu úng, giá trị dinh dưỡng, prefab 3D từng giai đoạn).
*   **`CropInstance`**: Đối tượng động quản lý tuổi thọ của cây trồng hiện tại trên ô đất, cập nhật mô hình 3D tương ứng theo giai đoạn sinh trưởng.
*   **`ItemData` (ScriptableObject)**: Định nghĩa vật phẩm tĩnh (Hạt giống, khoai tươi, khoai khô, nhang cúng).

### D. Inventory & Storage (Túi đồ & Kho)
*   **`InventoryManager`**: Quản lý danh sách các ô đồ (`ItemStack`) của người chơi và cung cấp phương thức thêm/bớt hạt giống hoặc thực phẩm.
*   **`ItemStack`**: Cấu trúc dữ liệu lưu trữ một loại `ItemData` kèm số lượng thực tế hiện có.

### E. Community & NPCs (Cộng đồng)
*   **`CommunityManager`**: Quản lý điểm Nghĩa Tình xã hội, tích lũy công Vần công, và cờ trạng thái hoàn thành sự kiện của làng.
*   **`NPCCharacter`**: Chứa thông tin NPC (O Thắm, Bác Năm, Cụ Bảy, Bé Tí) và mở ra danh sách lựa chọn tương tác đơn giản (Vần công, quyên góp, nhận hạt).
*   **`CommunityEvent`**: Khởi chạy các đoạn thông báo khẩn từ Loa phát thanh xã dựa trên ngày hiện tại.

### F. Weather & Disasters (Thiên tai)
*   **`WeatherManager`**: Quản lý nội suy nhiệt độ môi trường, hướng gió, mật độ hạt mưa rơi của ParticleSystem.
*   **`FloodManager`**: Điều phối dâng mực nước lũ trục Y động và gửi thông điệp úng rễ đến các `SoilCell`.

### G. Endings (Kết cục)
*   **`EndingManager`**: Bật canvas kết thúc game, chọn kịch bản kết cục tương ứng dựa trên tổng điểm Nghĩa Tình tích lũy khi kết thúc cốt truyện.

---

## 2. Responsibilities of Key Classes (Vai trò chi tiết)

| Class | Trách nhiệm chính (Single Responsibility) | Tương tác phối hợp |
| :--- | :--- | :--- |
| **`GameManager`** | Kiểm soát trạng thái global của scene (Chơi game, tạm dừng, kết thúc). Chạy đồng hồ đếm giờ. | Gửi sự kiện thời gian sang `PhaseManager`. |
| **`PhaseManager`** | Chuyển đổi trạng thái 2 Phase thiên tai tự động. Bật Banner UI Loa phát thanh xã. | Cập nhật tham số thời tiết mới sang `WeatherManager`. |
| **`CommunityManager`** | Lưu trữ điểm số Nghĩa Tình, số công Vần công tích lũy. | Đọc dữ liệu để cập nhật thanh hiển thị trên `UIManager`. |
| **`InventoryManager`** | Lưu trữ thực phẩm, hạt giống. Khấu trừ hạt giống khi gieo trồng và thêm nông sản khi thu hoạch. | Xác thực số lượng khi gieo hạt trong `PlayerController`. |
| **`SoilCell`** | Xử lý các tương tác nông nghiệp cục bộ trên lưới 4x3: dọn đá cát, tiếp nhận hạt gieo, cập nhật độ ẩm đất và kiểm tra ngập úng. | Gửi yêu cầu trừ hạt giống sang `InventoryManager`. |
| **`NPCCharacter`** | Hiển thị menu UI tương tác 3 nút lựa chọn tương trợ đơn giản tại chỗ. | Kích hoạt phương thức tăng điểm trong `CommunityManager`. |
| **`EndingManager`** | Đọc chỉ số Nghĩa Tình ở ngày cuối của Phase 2 để quyết định kết cục game (Đất sỏi cằn, Lá lành đùm lá rách, hay Đất cày nở hoa). | Tắt di chuyển nhân vật trên `PlayerController`. |

---

## 3. Architecture Anti-Patterns to Avoid (Lưu ý phòng tránh quá tải)

> [!IMPORTANT]
> Để hoàn thành dự án Demo PRU213 trong 2 tuần mà không bị vỡ tiến độ, tuyệt đối **KHÔNG** lập trình các hệ thống cồng kềnh sau:
> *   **Không viết hệ thống Save/Load phức tạp:** Game chỉ kéo dài 20 phút chạy liên tục, chỉ cần lưu dữ liệu tạm thời bằng bộ nhớ RAM (RAM variables) lúc Runtime.
> *   **Không thiết lập Dialogue Graph đa luồng:** Hội thoại NPC chỉ cần là mảng chuỗi string chạy tuần tự (`string[] lines`) hoặc bật tắt UI Popup đơn giản.
> *   **Không xây dựng Quest Framework đa mục tiêu:** Không cần lưu cơ sở dữ liệu nhiệm vụ (Quest Database). Các sự kiện cộng đồng được kích hoạt trực tiếp từ việc kiểm tra điều kiện IF-ELSE đơn giản trong `GameManager` theo ngày hiện tại.
> *   **Tận dụng tối đa ScriptableObjects:** Dùng ScriptableObject để cấu hình thuộc tính cây trồng và vật phẩm, tránh việc tạo class mới cho từng loại nông sản.
