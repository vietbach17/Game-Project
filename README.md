# Đất Cày Lên Sỏi Đá (Sown in Stone)

### *Hành trình giữ làng sau bão*

Dự án phát triển trò chơi môn học PRU213 - Học kỳ 7 | Unity 6 Project

---

## 1. Project Overview
* **Tên dự án:** Đất Cày Lên Sỏi Đá / Sown in Stone
* **Thể loại:** Narrative Community Survival Game (Mô phỏng sinh tồn cộng đồng theo cốt truyện) kết hợp yếu tố làm nông (farming) và quản lý tài nguyên.
* **Nền tảng:** PC (Unity 6, đồ họa và vật lý 3D, góc nhìn thứ ba).
* **Đối tượng hướng tới:** Người chơi yêu thích trải nghiệm game sinh tồn sâu sắc, mang tính nhân văn và đậm đà bản sắc văn hóa Việt Nam.

## 2. Game Vision
Trò chơi hướng đến việc xây dựng một trải nghiệm sinh tồn khác biệt: **Không làm nông để làm giàu**. 
* Tầm nhìn cốt lõi là kể câu chuyện về sự kiên cường của con người trước thiên tai khắc nghiệt miền Trung Việt Nam.
* Mục tiêu tối thượng của người chơi không phải là tích lũy của cải cá nhân mà là sản xuất lương thực để cứu đói, xây dựng lòng tin, thắt chặt nghĩa tình với bà con chòm xóm để cùng nhau gia cố nhà cửa chống chịu bão lũ, và từng bước tái thiết đời sống sau thiên tai.

## 3. Cultural Theme
Dự án tích hợp sâu sắc đời sống văn hóa, tinh thần và các thử thách thực tế của dải đất duyên hải miền Trung:
* **Nghĩa Tình:** Lối sống tối lửa tắt đèn có nhau, sẵn sàng nhường cơm sẻ áo trong hoạn nạn.
* **Vần công:** Cơ chế đổi công đổi việc truyền thống của nông dân Việt Nam, giúp đỡ lẫn nhau tát nước, gieo hạt hay chằng chống nhà cửa khi mưa bão mà không tính toán bằng tiền bạc.
* **Loa phát thanh xã:** Kênh truyền tin cộng đồng thân thương và quan trọng, thông báo tình hình thời tiết và chỉ đạo từ chính quyền địa phương mỗi sớm mai.
* **Tín ngưỡng tâm linh:** Bàn thờ tổ tiên làng xã, thắp nhang cầu bình an trước những biến động khắc nghiệt của tự nhiên.

## 4. Core Gameplay Loop
```
Thức dậy (Nghe Loa phát thanh xã báo thời tiết)
↓
Canh tác & Cải tạo đất cát sỏi đá (Chuẩn bị lương thực sinh tồn)
↓
Lao động tương trợ / Vần công (Tương tác giúp đỡ dân làng, tăng Nghĩa Tình)
↓
Chống chịu thiên tai (Vượt qua nắng hạn Gió Lào và bão lũ dâng cao)
↓
Tái thiết sau lũ (Tận dụng phù sa màu mỡ để tái sản xuất, khôi phục ngôi làng)
```

## 5. Main Systems

### Farming as Food Production
* Canh tác để sinh tồn chứ không thương mại hóa đại trà.
* Đất trồng (`SoilCell`) bắt đầu với mật độ sỏi đá cao. Người chơi phải cuốc đất dọn đá để cải tạo chất lượng.
* Cây trồng cần được tưới nước và chăm sóc dựa trên độ ẩm đất. Lượng nước bốc hơi phụ thuộc trực tiếp vào thời tiết (ví dụ: bốc hơi cực nhanh dưới Gió Lào).
* Nông sản thu hoạch được ưu tiên đưa vào kho dự trữ cứu đói hoặc chia sẻ cho các hộ gia đình khó khăn.

### Nghĩa Tình Score
* Là chỉ số phát triển chính của toàn bộ trò chơi (thay thế cho hệ thống tiền tệ tích lũy thông thường).
* Điểm Nghĩa Tình tăng lên thông qua việc:
  * Trò chuyện, chia sẻ khó khăn với dân làng (Bác Năm, O Thắm).
  * Chia sẻ lương thực tươi hoặc chế biến khô trong thời kỳ bão lũ đói kém.
  * Thắp nhang tại AncestralAltar cầu an.
* Chỉ số Nghĩa Tình cao mở khóa sự giúp đỡ nhiệt tình hơn từ dân làng và các sự kiện hỗ trợ cộng đồng.

### Community Events
* Được kích hoạt tự động theo diễn biến thời gian và trạng thái thời tiết.
* Loa phát thanh xã phát thông báo khẩn cấp khi chuyển giai đoạn, kêu gọi bà con đoàn kết dọn kênh mương hạn hán hoặc chung tay gia cố mái nhà trước bão.

### Weather and Disaster Phases
* Nội suy thời tiết mượt mà thông qua `WeatherManager` sử dụng hàm `Mathf.Lerp`.
* Hệ thống mưa bão bám theo camera, thay đổi cường độ gió, hạt mưa và trực tiếp dâng cao mực nước lũ thực tế trong không gian 3D.
* Chỉ số sinh lý của nhân vật chịu ảnh hưởng bởi nhiệt độ môi trường: nắng hạn Gió Lào tăng Heat Stress (stress nhiệt), mưa bão ngập lụt tăng Cold Stress (stress lạnh).

### NPC Mutual Aid & Secondary Currency (Xu)
* Tiền xu (Xu) đóng vai trò là nguồn lực trao đổi thứ cấp được cung cấp ban đầu để Thành hỗ trợ giao thương nông cụ hoặc hạt giống cơ bản.
* Người chơi có thể đến gặp O Thắm hoặc Bác Năm thực hiện đổi công (tiêu hao thể lực để nhận công **Vần công**).
* Khi bão lũ đến gần, công vần công tích lũy được dùng để nhờ dân làng giúp chằng chống nhà cửa, tăng khả năng chống chịu bão.

### Post-Flood Silt Soil Recovery
* Các ô đất ruộng bị ngập sâu (>0.5m) trong mùa bão lũ sẽ tự động được bồi đắp một lớp phù sa màu mỡ (Silt Soil) khi nước rút.
* Đất phù sa tự động dọn sạch 30% sỏi đá cũ, hồi phục dinh dưỡng lên 100% và nhân đôi sản lượng thu hoạch cây trồng cho vụ mùa tái thiết (nhận về 5 củ khoai thay vì 2).

---

## 6. Game Phases (Chu Kỳ Thiên Tai)
Mỗi ngày trong game tương ứng với **5 phút ngoài đời thực** (`secondsPerGameHour = 12.5f`). Kịch bản chuyển Phase cốt truyện tự động diễn ra:
* **Phase 1: Tiếng Trống Đình Làng (Ngày 1 - 2):** Thành trở về quê hương, dọn dẹp sỏi đá hoang hóa, cải tạo ruộng đồng và bắt đầu canh tác vụ khoai đầu tiên.
* **Phase 2: Nắng Cháy Gió Lào (Ngày 3 - 4):** Thời tiết nắng nóng cực đoan, Gió Lào thổi mạnh. Thể lực mất rất nhanh dưới ánh nắng, đất đai bốc hơi nước nhanh chóng đòi hỏi tưới tiêu liên tục.
* **Phase 3: Tình Người Trong Mưa Bão (Ngày 5 - 6):** Bão lớn đổ bộ, mưa tầm tã và nước lũ dâng cao ngập ruộng vườn. Người chơi phải chằng chống nhà cửa từ trước, thu hoạch sớm nông sản tươi hoặc chế biến khoai gieo khô dự trữ tránh thối hỏng.
* **Phase 4: Phù Sa Sau Cơn Lũ (Ngày 7 trở đi):** Nước lũ rút mượt mà, phù sa màu mỡ bồi đắp lại những mảnh ruộng cằn cỗi. O Thắm hỗ trợ giảm giá hạt giống để bà con cùng Thành bắt tay tái thiết lại cuộc sống.

---

## 7. Current Unity Setup (Hướng Dẫn Thiết Lập Cảnh 3D)

Do dự án Git chỉ lưu trữ mã nguồn và tài nguyên cấu hình tĩnh, hãy làm theo các bước sau trong Unity Editor để dựng lại Scene 3D hoàn chỉnh:

### Bước 1: Thiết Lập Quản Lý Hệ Thống (Managers)
1. Trong cửa sổ **Hierarchy**, nhấp chuột phải chọn **Create Empty**. Đặt tên GameObject này là `_Managers`.
2. Kéo thả lần lượt 5 script quản lý từ thư mục `Assets/Scripts/` vào cửa sổ Inspector của nó:
   * **`GameManager`** (Cài đặt `secondsPerGameHour = 12.5` để có 1 ngày = 5 phút thực).
   * **`PlayerStats`** (Quản lý Máu, Thể lực, Tinh thần, Xu hỗ trợ ban đầu).
   * **`WeatherManager`** (Nội suy thời tiết, lũ lụt, hệ thống mưa rơi).
   * **`StorageManager`** (Quản lý kho chứa, thối nông sản ẩm, chế biến khoai khô).
   * **`CommunityManager`** (Quản lý điểm nghĩa tình làng xóm và đổi công vần công).

### Bước 2: Tạo Các Vật Phẩm & Cây Trồng (Data Assets)
1. Trong thư mục **Project**, tạo thư mục con tên là `Data` (nếu chưa có) nằm trực tiếp dưới `Assets/`.
2. Trong thư mục `Data`, nhấp chuột phải chọn **Create -> Sown In Stone -> Item Data** để tạo ra 4 vật phẩm:
   * **`Item_FreshCrop`** (Khoai Lang Tươi): *Item ID = item_fresh_crop, Item Name = Khoai Lang Tươi, Type = Nong San Tuoi, Decay Rate In Humidity = 0.8*.
   * **`Item_PreservedCrop`** (Khoai Gieo): *Item ID = item_khoai_gieo, Item Name = Khoai Gieo, Type = Nong San Kho, Decay Rate In Humidity = 0*.
   * **`Item_Incense`** (Nhang): *Item ID = item_incense, Item Name = Nhang, Type = Incense, Morale Restore Value = 10*.
   * **`Item_Seed`** (Hạt giống Khoai Lang): *Item ID = item_potato_seed, Item Name = Hạt giống Khoai Lang, Type = Item_Seed*.
3. Nhấp chuột phải chọn **Create -> Sown In Stone -> Crop Data**:
   * **`Crop_KhoaiLang`**: *Crop Name = Khoai Lang, Days To Mature = 5, Seed Price = 10 (Mức Xu trao đổi cơ bản), Base Sell Value = 25 (Hỗ trợ khi ký gửi), Ideal Moisture = 60, Harvested Item = Item_FreshCrop*.

### Bước 3: Tạo Nhân Vật Chính (Player)
1. Tạo một Empty GameObject trong Hierarchy, đặt tên là **`Player`**.
2. Kéo thả script **`PlayerController`** vào đối tượng này.
3. Thêm các thành phần vật lý 3D:
   * Thêm component **`Rigidbody`**: Trong Inspector, mở phần **Constraints**, tích chọn **tất cả các ô Freeze Rotation** (X, Y, Z) và **Freeze Position Y** (Khóa chiều cao để nhân vật di chuyển phẳng trên trục X/Z, không bị rơi tự do).
   * Thêm component **`Box Collider`** (hoặc `Capsule Collider`).
4. Đưa mô hình nhân vật 3D (ví dụ tệp `indonesian_farmer_pak_tani.glb`) làm con của GameObject Player.
   * Kéo đối tượng mô hình con này vào ô **Character Visual** trên component `PlayerController`.
5. Tạo Animator Controller mới, cấu hình Blend Tree chuyển động và gán vào component **Animator** trên Player.

### Bước 4: Thiết Lập Camera Theo Dõi 3D
1. Chọn đối tượng **Main Camera** trong Hierarchy.
2. Thêm component **`Camera Follow 3D`** vào Main Camera.
3. Kéo đối tượng **Player** ở Hierarchy thả vào trường **Target** của `Camera Follow 3D` để camera tự động bám theo mượt mà.

### Bước 5: Tạo Các Đối Tượng Tương Tác Trên Bản Đồ
1. **Các ô đất trồng (`SoilCell`):**
   * Tạo Empty GameObject tên là `SoilCell`, gắn script **`SoilCell`**.
   * Thêm component **`Box Collider`** (3D) và **tích chọn `Is Trigger`** để nhân vật có thể đi xuyên qua.
2. **Bàn thờ tổ tiên (`AncestralAltar`):**
   * Tạo Empty GameObject tên là `AncestralAltar`, gắn script **`AncestralAltar`** (Kéo tệp `Item_Incense` vào trường *Incense Item*).
   * Thêm **`Box Collider`** (3D) và **tích chọn `Is Trigger`**.
3. **Dân làng (NPCs):**
   * Tạo Empty GameObject tên là `NPC_BacNam`, gắn script **`NPCCharacter`** (chọn Character Type = *Bac Nam*). Thêm **`Box Collider`** (3D), tích **`Is Trigger`**.
   * Tạo Empty GameObject tên là `NPC_OTham`, gắn script **`NPCCharacter`** (chọn Character Type = *O Tham*). Thêm **`Box Collider`** (3D), tích **`Is Trigger`**.

### Bước 6: Tạo Giao Diện & Bộ Kiểm Thử (UI & Tester)
1. Tạo Empty GameObject tên là `_UI_Tester`.
2. Gắn các script sau vào nó:
   * **`FrameworkMainMenuUI`** (Menu gỗ mở đầu)
   * **`FrameworkDebugUI`** (Bảng điều khiển test F1)
   * **`FrameworkTester`** (Bộ chạy kịch bản thử nghiệm)

### Bước 7: Cấu Hinh Inspector Cho UI & Tester (Quan Trọng)
Chọn GameObject `_UI_Tester`, tại Inspector kéo thả liên kết:
1. **Component `Framework Tester`:**
   * Kéo đối tượng `SoilCell` từ Hierarchy vào ô **Test Soil Cell**.
   * Kéo asset `Crop_KhoaiLang` từ Project vào ô **Test Seed Data**.
   * Kéo asset `Item_Incense` vào ô **Test Incense Item**.
   * Kéo đối tượng `AncestralAltar` vào ô **Test Altar**.
2. **Component `Framework Debug UI`:**
   * Kéo asset `Item_FreshCrop` vào ô **Test Fresh Crop**.
   * Kéo asset `Item_PreservedCrop` vào ô **Test Preserved Crop**.
   * Kéo asset `Item_Incense` vào ô **Test Incense**.
   * Kéo asset `Item_Seed` vào ô **Test Seed Item**.
   * Kéo đối tượng `AncestralAltar` vào ô **Test Altar**.
3. **Component `PlayerController` (trên đối tượng `Player`):**
   * Kéo asset `Crop_KhoaiLang` vào ô **Test Seed Data**.
   * Kéo asset `Item_Seed` vào ô **Seed Item**.

---

## 8. Input Controls

* **`W, A, S, D`** : Di chuyển nhân vật 3D trên bản đồ.
* **`Giữ Chuột Phải + Di chuột`** : Xoay Camera tự do góc nhìn thứ ba.
* **`E`** : Tương tác (Dọn đá, Tưới nước, Gieo hạt, Thu hoạch, Trò chuyện với NPC).
* **`I / Tab`** : Mở Hòm đồ (Kiểm tra hạt giống, nông sản khô/tươi, và tài sản).
* **`M`** : Xem chi tiết thời tiết, nhiệt độ, độ ẩm và mực nước lũ dâng.
* **`C`** : Bật bảng Nghĩa Tình & tương trợ làng xóm.
* **`F1`** : Bật/tắt bảng điều khiển Debug nhanh dành cho Nhà phát triển (đốt cháy giai đoạn, thử nghiệm stress nhiệt/lạnh).

---

## 9. Demo Scope for PRU213
Bản thử nghiệm hiện tại hướng đến mục tiêu chứng minh thành công vòng lặp cơ bản (core loop) của trò chơi:
* **Hệ thống thời gian & thời tiết đồng bộ:** Chuyển đổi mượt mà qua các giai đoạn thiên tai trong 4 ngày (chu kỳ 20 phút thực tế).
* **Sinh tồn & Canh tác:** Dọn đá cải tạo đất cằn cỗi, tưới nước bù đắp độ ẩm bị bốc hơi do nắng nóng Gió Lào, và thu hoạch khoai tươi.
* **Ứng phó & Tích trữ:** Chế biến khoai tươi thành khoai khô dự trữ chống thối rữa trước mùa lũ ngập.
* **Mối quan hệ làng xóm:** Gặp gỡ O Thắm để thực hiện đổi công tương trợ (Vần công), mua hạt giống hoặc trao đổi hỗ trợ vật phẩm.
* **Sự bồi đắp sau lũ:** Tái canh tác trên đất phù sa sau khi bão rút và nhận gấp đôi sản lượng nông sản thu hoạch.

## 10. Recommended Development Priorities
Để phát triển từ bản Prototype hiện tại lên phiên bản Demo hoàn chỉnh hơn, nhóm đề xuất các ưu tiên sau:
1. **Tinh chỉnh UI Nghĩa Tình & Vần Công:** Thiết kế lại bảng HUD tương trợ để thể hiện rõ nét hơn việc tích lũy công đổi công và đóng góp lương thực, thay vì hiển thị dạng giao dịch tiền xu thuần túy.
2. **Loa phát thanh xã có âm thanh:** Ghi âm hoặc sinh procedurally giọng đọc phát thanh thông báo đầu ngày bằng tiếng Việt mang chất giọng miền Trung chân thực để tăng tính nhập vai.
3. **Cơ chế gia cố nhà cửa bằng hình ảnh:** Khi người chơi tiêu hao điểm Vần công tương trợ chằng chống nhà cửa, mô hình ngôi nhà của Thành và các NPC sẽ hiển thị thêm lưới thép, bao cát, hoặc dây chằng xung quanh.
4. **Cân bằng sinh lý dưới thời tiết:** Tinh chỉnh tốc độ tăng Heat Stress/Cold Stress để tạo độ thử thách vừa phải, khuyến khích người chơi ở nhà tránh nắng Gió Lào buổi trưa hoặc tránh ra đồng khi mưa lũ dâng cao.
