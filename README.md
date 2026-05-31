# Đất Cày Lên Sỏi Đá — Hướng dẫn Thiết lập Dự án & Trải nghiệm Game

Chào bạn! Dự án này mô phỏng cuộc sống làm nông và sinh tồn khắc nghiệt tại miền Trung Việt Nam qua các giai đoạn thiên tai (Gió Lào, Bão Lũ, Phù Sa). 

Khi bạn mới clone dự án về, bạn mở Scene lên sẽ thấy trống trơn (chỉ có `Main Camera` và `Directional Light`). Điều này hoàn toàn bình thường vì **dự án Git chỉ lưu trữ mã nguồn (scripts) và cài đặt dựng hình (URP settings)** mà không đi kèm với một Scene được dựng sẵn các thực thể game hoặc Prefabs.

Đặc biệt, dự án này sử dụng cơ chế **OnGUI-based UI** vẽ giao diện trực tiếp bằng code C#. Bạn **không cần tạo Canvas phức tạp**, chỉ cần thiết lập các GameObject và kéo thả script theo hướng dẫn dưới đây là có thể chạy thử nghiệm toàn bộ game ngay lập tức!

---

## Hướng dẫn Thiết lập Scene từng bước

Hãy mở dự án trong Unity và thực hiện theo 6 bước đơn giản sau:

### Bước 1: Tạo các GameObjects Quản lý (Managers)
1. Trong cửa sổ **Hierarchy**, nhấp chuột phải và chọn **Create Empty**. Đặt tên GameObject này là `_Managers`.
2. Chọn `_Managers` và kéo thả lần lượt 5 script quản lý cốt lõi từ thư mục `Assets/Scripts/` vào cửa sổ Inspector của nó:
   - [GameManager](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/GameManager.cs) (Quản lý thời gian, ngày/giờ và chuyển giai đoạn game)
   - [PlayerStats](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Core/PlayerStats.cs) (Quản lý các chỉ số sinh tồn: Máu, Thể lực, Tinh thần, Stress)
   - [WeatherManager](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Weather/WeatherManager.cs) (Mô phỏng thời tiết nắng nóng, gió Lào, bão lũ, mực nước lụt)
   - [StorageManager](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Storage/StorageManager.cs) (Quản lý kho chứa đồ, cơ chế thối mốc nông sản tươi, chế biến đồ khô)
   - [CommunityManager](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/CommunityManager.cs) (Quản lý điểm Nghĩa Tình/Karma và cơ chế đổi công hỗ trợ chằng nhà chống bão)

### Bước 2: Tạo các ScriptableObject Tài nguyên (Data Assets)
Do các file asset tĩnh lưu dữ liệu vật phẩm và cây trồng chưa có sẵn trong Git, bạn hãy tự tạo chúng chỉ bằng vài cú click chuột:
1. Trong cửa sổ **Project**, tạo một thư mục mới tên là `Data` (nằm trong thư mục `Assets/`).
2. Vào thư mục `Data`, nhấp chuột phải chọn **Create -> Sown In Stone -> Item Data** để tạo ra 3 vật phẩm:
   - Đặt tên file là `Item_FreshCrop`: Thiết lập trong Inspector:
     - **Item ID:** `item_fresh_crop`
     - **Item Name:** `Khoai Lang Tươi`
     - **Type:** `Nong San Tuoi`
     - **Decay Rate In Humidity:** `0.8` (Thức ăn tươi sẽ thối rữa rất nhanh khi lụt ẩm)
   - Đặt tên file là `Item_PreservedCrop`: Thiết lập trong Inspector:
     - **Item ID:** `item_khoai_gieo`
     - **Item Name:** `Khoai Gieo`
     - **Type:** `Nong San Kho`
     - **Decay Rate In Humidity:** `0` (Khoai gieo phơi khô để lâu không bao giờ mốc)
   - Đặt tên file là `Item_Incense`: Thiết lập trong Inspector:
     - **Item ID:** `item_incense`
     - **Item Name:** `Nhang`
     - **Type:** `Incense`
     - **Morale Restore Value:** `10`
3. Nhấp chuột phải chọn **Create -> Sown In Stone -> Crop Data** để tạo ra hạt giống cây trồng:
   - Đặt tên file là `Crop_KhoaiLang`: Thiết lập trong Inspector:
     - **Crop Name:** `Khoai Lang`
     - **Days To Mature:** `5` (Mất 5 ngày để lớn)
     - **Seed Price:** `10`
     - **Base Sell Value:** `25`
     - **Ideal Moisture:** `60`

### Bước 3: Tạo các GameObject Tương tác trong Game
Hãy tạo các thực thể trên bản đồ để người chơi có thể cải tạo đất, thắp hương cầu an hoặc trò chuyện với láng giềng:
1. Tạo một Empty GameObject tên là `SoilCell` và gắn script [SoilCell](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Agriculture/SoilCell.cs) vào nó.
2. Tạo một Empty GameObject tên là `AncestralAltar` và gắn script [AncestralAltar](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Interactions/AncestralAltar.cs) vào nó.
3. Tạo hai Empty GameObject đại diện cho láng giềng:
   - GameObject thứ nhất đặt tên là `NPC_BacNam`, gắn script [NPCCharacter](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/NPCCharacter.cs), và chỉnh trường **Character Type** thành `Bac Nam`.
   - GameObject thứ hai đặt tên là `NPC_OTham`, gắn script [NPCCharacter](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/Community/NPCCharacter.cs), và chỉnh trường **Character Type** thành `O Tham`.

### Bước 4: Tạo Giao diện & Trình kiểm thử (UI & Tester)
1. Tạo một Empty GameObject mới đặt tên là `_UI_Tester`.
2. Kéo thả lần lượt 3 script sau vào `_UI_Tester`:
   - [FrameworkMainMenuUI](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/FrameworkMainMenuUI.cs) (Menu bắt đầu điện ảnh)
   - [FrameworkDebugUI](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/FrameworkDebugUI.cs) (Bảng kiểm soát sinh tồn)
   - [FrameworkTester](file:///d:/Linh%20tinh/studying/Semester_7/PRU213/in_class/Project/src/clone/Assets/Scripts/FrameworkTester.cs) (Bộ chạy kịch bản thử nghiệm gieo trồng ảo)

### Bước 5: Cấu hình liên kết trên Inspector (Quan trọng)
Chọn GameObject `_UI_Tester`, nhìn vào cửa sổ Inspector và thực hiện kéo thả các liên kết sau:
1. **Trên component `Framework Tester`:**
   - Kéo GameObject `SoilCell` từ Hierarchy vào ô **Test Soil Cell**.
   - Kéo asset `Crop_KhoaiLang` từ thư mục Project vào ô **Test Seed Data**.
   - Kéo asset `Item_Incense` từ thư mục Project vào ô **Test Incense Item**.
   - Kéo GameObject `AncestralAltar` từ Hierarchy vào ô **Test Altar**.
2. **Trên component `Framework Debug UI`:**
   - Kéo asset `Item_FreshCrop` vào ô **Test Fresh Crop**.
   - Kéo asset `Item_PreservedCrop` vào ô **Test Preserved Crop**.
   - Kéo asset `Item_Incense` vào ô **Test Incense**.
   - Kéo GameObject `AncestralAltar` vào ô **Test Altar**.
3. **Trên component `AncestralAltar` (của GameObject `AncestralAltar`):**
   - Kéo asset `Item_Incense` vào ô **Incense Item**.

---

## Trải nghiệm Gameplay sau khi bấm Play

Khi bạn đã hoàn thành thiết lập trên và nhấn nút **Play** ở góc trên cùng của Unity Editor, kịch bản game sẽ diễn ra như sau:

1. **Màn hình Menu Gỗ Nghệ Thuật hiện lên:**
   - Màn hình chuyển sang tông cam đất hoàng hôn miền Trung.
   - Có hiệu ứng các hạt bụi rơm vàng lãng mạn bay ngang qua màn hình.
   - Thời gian trong game tạm dừng để bạn đọc cốt truyện trong tab **Ký Ức Miền Trung** hoặc nhấn **"Về quê bám đất"** để chính thức bắt đầu hành trình.
2. **Bảng Điều Khiển Sinh Tồn Xuất Hiện:**
   - Sau khi thoát Menu, thời gian game sẽ bắt đầu chạy (đồng hồ tăng dần).
   - Ở phía bên trái và phải màn hình, bảng điều khiển gỗ hiển thị đầy đủ các thông tin:
     - **Thời gian & Giai đoạn:** Hiện ngày giờ và 4 mùa cốt truyện.
     - **Thời tiết:** Nhiệt độ biến động ngày đêm, ẩm độ, sức gió và mực nước lũ.
     - **Sức khỏe nhân vật:** Thanh máu (Health), thể lực (Stamina) và tinh thần (Morale).
     - **Tích Cốc Phòng Cơ:** Xem các vật phẩm trong hòm kho. Bạn có thể nhấn nút **Chế biến Khoai Gieo** để dùng 3 củ khoai tươi tạo ra khoai khô tránh ẩm mốc.
     - **Tín ngưỡng & Vần công:** Thắp nhang hồi morale hoặc cử Bác Năm đi đổi công làm ruộng tích lũy nghĩa tình.
     - **Hội thoại cốt truyện:** Có thể trò chuyện, nghe Bác Năm hoặc O Thắm tâm sự ca dao tục ngữ hoặc tặng quà cho họ để tăng điểm thân thiết.
     - **Bàn điều phối thiên tai (Dev Controls):** Trực tiếp kích hoạt giả lập chuyển mùa từ Mùa Lập nghiệp -> Nắng nóng Gió Lào (nhiệt tăng 42°C, làm việc mất máu) -> Bão lũ (nước sông dâng lút nhà, kích hoạt cả làng sang chằng chống nhà hộ bạn nhờ điểm đổi công tích lũy) -> Phù Sa bồi đắp sau lũ.

Chúc bạn có những trải nghiệm thú vị khi kiểm thử dự án game giàu ý nghĩa nhân văn này! Nếu gặp bất kỳ khó khăn nào trong quá trình kéo thả thiết lập, hãy đặt câu hỏi để mình hỗ trợ nhé.
