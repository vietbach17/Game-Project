# Đất Cày Lên Sỏi Đá (Sown in Stone) — Hướng Dẫn Thiết Lập & Tài Liệu Dự Án

Dự án này là một game mô phỏng sinh tồn và làm nông thực tế tại miền Trung Việt Nam qua các giai đoạn thiên tai khắc nghiệt (Lập Nghiệp, Nắng Cháy Gió Lào, Tình Người Trong Lũ, và Phù Sa Sau Lũ).

Hiện tại, dự án đã được chuyển đổi hoàn toàn sang **môi trường đồ họa & vật lý 3D** với hệ thống điều phối tự động, thương mại thực tế và bồi đắp đất đai sau thiên tai.

---

## 🌟 Tổng Quan Các Tính Năng Hiện Có

### 1. ⏱️ Hệ Thống Thời Gian Thực Tế & Tự Động Hóa Cốt Truyện
* **Tỷ lệ thời gian:** `1 ngày trong game = 5 phút ngoài đời thực` (24 giờ game trôi qua trong 300 giây thực tế; tức `secondsPerGameHour = 12.5f`).
* **Tự động chuyển Phase cốt truyện chính:**
  * **Ngày 1 đến Ngày 2:** GĐ 1 — Tiếng Trống Đình Làng (Lập nghiệp, dọn đá cải tạo đất).
  * **Ngày 3 đến Ngày 4:** GĐ 2 — Nắng Cháy Gió Lào (Nhiệt độ tăng cao, mất thể lực nhanh khi ra nắng).
  * **Ngày 5 đến Ngày 6:** GĐ 3 — Tình Người Trong Bão Lũ (Mưa lớn đổ bộ, nước lũ dâng ngập ruộng vườn).
  * **Ngày 7 trở đi:** GĐ 4 — Phù Sa Sau Cơn Lũ (Nước rút mượt mà, phù sa bồi đắp ruộng đất).
* **Banner thông báo Điện ảnh:** Tự động tạo procedurally hiệu ứng banner trôi nổi giữa màn hình, Fade In/Fade Out trong 6 giây báo hiệu đổi Phase mà không cần thiết lập thủ công.

### 🌤️ 2. Nội Suy Thời Tiết & Nước Lũ Mượt Mà (Lerps)
* Nhiệt độ, Độ ẩm, Sức gió, Mưa rơi và Mực nước lũ thay đổi từ từ bằng hàm `Mathf.Lerp` thay vì nhảy số tức thời.
* Tích hợp hệ thống hạt mưa bão (`ParticleSystem`) tự động bám theo Camera, tăng/giảm mật độ hạt và thổi chéo theo chiều gió dựa trên cường độ thời tiết.

### 💰 3. Hệ Thống Tiền Xu & Thương Mại Thực Tế (No Fake Debug)
* **Tiền tệ:** Nhân vật Thành khởi đầu với **50 Xu** (Hiển thị ở đáy bảng Hòm đồ `Tab` và trên Debug UI).
* **Khấu trừ hạt giống:** Gieo hạt giống sẽ tiêu hao đúng **1 Hạt giống thực tế** (`Item_Seed.asset`) trong kho đồ. Nếu hết hạt giống, hệ thống sẽ từ chối gieo và hướng dẫn người chơi đi mua.
* **Giao dịch 3 lựa chọn với đại lý O Thắm:** Khi tiếp cận O Thắm nhấn `[E]`, menu thoại hỗ trợ 3 nút phân nhánh:
  1. **Trò chuyện:** Giao lưu tăng tinh thần và tình cảm.
  2. **Giúp việc (Vần công):** Tiêu hao 20 Thể lực giúp đỡ O Thắm để nhận 1 công vần công.
  3. **Giao dịch (Mua/Bán):**
     * **Mua hạt giống:** Giá gốc 10 xu/hạt. Riêng ở **Phase 4 (Phù Sa)**, O Thắm giảm giá đặc biệt còn **6 xu/hạt** hỗ trợ bà con tái thiết.
     * **Bán khoai tươi:** Bán toàn bộ khoai tươi thu hoạch được lấy **25 xu/củ** để làm giàu tài sản.

### 🌱 4. Cơ Chế Đất Phù Sa Màu Mỡ
* Ghi nhận các ô đất bị ngập nước lũ sâu (>0.5m) trong mùa bão lũ.
* Khi bão tan bước sang Phase 4, các ô đất ngập nước này tự động chuyển đổi thành **Đất Phù Sa (Silt Soil)**:
  * Tự động dọn sạch 30% sỏi đá cũ.
  * Hồi phục dinh dưỡng lên mức tối đa 100%.
  * Nhân đôi sản lượng thu hoạch cây trồng khi chín (nhận về **5 củ khoai tươi** thay vì 2).

### 🛠️ 5. Bảng Điều Khiển Lập Trình Nhanh (F1 Debug UI)
* Bấm **`F1`** khi chơi để bật/tắt bảng Debug:
  * Hiển thị trực quan chỉ số, thời tiết và tài sản.
  * 3 nút kiểm thử stress sinh lý: **`Thêm +35% Stress Nhiệt`**, **`Thêm +35% Stress Lạnh`**, và **`Reset Toàn Bộ Stress`** về 0%.
  * Nút tặng nhanh: +50 Xu, -50 Xu, +5 Khoai Tươi, +5 Hạt Giống để test nhanh.

---

## 🛠️ Hướng Dẫn Thiết Lập Scene Từ Đầu

Do dự án Git chỉ lưu trữ mã nguồn và tài nguyên cấu hình tĩnh, hãy làm theo các bước sau trong Unity Editor để dựng lại Scene 3D hoàn chỉnh:

### Bước 1: Thiết Lập Quản Lý Hệ Thống (Managers)
1. Trong cửa sổ **Hierarchy**, nhấp chuột phải chọn **Create Empty**. Đặt tên GameObject này là `_Managers`.
2. Kéo thả lần lượt 5 script quản lý từ thư mục `Assets/Scripts/` vào cửa sổ Inspector của nó:
   * **`GameManager`** (Cài đặt `secondsPerGameHour = 12.5` để có 1 ngày = 5 phút thực).
   * **`PlayerStats`** (Quản lý Máu, Thể lực, Tinh thần, Xu).
   * **`WeatherManager`** (Nội suy thời tiết, lũ lụt, hệ thống mưa rơi).
   * **`StorageManager`** (Quản lý kho chứa, thối nông sản ẩm, chế biến khoai khô).
   * **`CommunityManager`** (Quản lý nghĩa tình làng xóm và đổi công chống bão).

### Bước 2: Tạo Các Vật Phẩm & Cây Trồng (Data Assets)
1. Trong thư mục **Project**, tạo thư mục con tên là `Data` (nếu chưa có) nằm trực tiếp dưới `Assets/`.
2. Trong thư mục `Data`, nhấp chuột phải chọn **Create -> Sown In Stone -> Item Data** để tạo ra 4 vật phẩm:
   * **`Item_FreshCrop`** (Khoai Lang Tươi): *Item ID = item_fresh_crop, Item Name = Khoai Lang Tươi, Type = Nong San Tuoi, Decay Rate In Humidity = 0.8*.
   * **`Item_PreservedCrop`** (Khoai Gieo): *Item ID = item_khoai_gieo, Item Name = Khoai Gieo, Type = Nong San Kho, Decay Rate In Humidity = 0*.
   * **`Item_Incense`** (Nhang): *Item ID = item_incense, Item Name = Nhang, Type = Incense, Morale Restore Value = 10*.
   * **`Item_Seed`** (Hạt giống Khoai Lang): *Item ID = item_potato_seed, Item Name = Hạt giống Khoai Lang, Type = Item_Seed*.
3. Nhấp chuột phải chọn **Create -> Sown In Stone -> Crop Data**:
   * **`Crop_KhoaiLang`**: *Crop Name = Khoai Lang, Days To Mature = 5, Seed Price = 10, Base Sell Value = 25, Ideal Moisture = 60, Harvested Item = Item_FreshCrop*.

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

### Bước 7: Cấu Hình Inspector Cho UI & Tester (Quan Trọng)
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

## 🎮 Cách Bắt Đầu Trải Nghiệm Game

1. Nhấn nút **Play** trong Unity Editor.
2. Màn hình **Menu Gỗ Ký Ức** sẽ hiện lên, nhấn nút **"Về quê bám đất"** để bắt đầu.
3. Sử dụng các phím **WASD** để di chuyển nhân vật trong không gian 3D.
4. Lại gần các luống đất hoặc NPC, nhấn **`[E]`** để thực hiện tương tác:
   * Nhặt đá dọn ruộng -> Tưới nước -> Gieo hạt giống thực tế -> Thu hoạch nông sản khi chín.
   * Nói chuyện với O Thắm, chọn giao dịch mua hạt giống hoặc bán khoai tích xu.
5. Nhấn phím **`Tab`** hoặc **`I`** để mở hòm đồ theo dõi tài sản Xu và hạt giống thực tế.
6. Nhấn phím **`F1`** để mở bảng debug kiểm thử nhanh chỉ số stress sinh lý hoặc tặng/trừ tài nguyên.
