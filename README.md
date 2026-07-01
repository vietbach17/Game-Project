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

### ⚙️ 6. Bảng Cài Đặt (Settings Menu) & Tùy Biến Phím Bấm Linh Hoạt
* **Nút bánh răng HUD**: Một nút hình bánh răng nằm ở góc dưới cùng bên phải màn hình khi chơi game để mở bảng Settings nhanh, hoặc có thể nhấn phím **`Esc`** để bật/tắt Settings.
* **Tích hợp Menu chính**: Cho phép tùy biến phím bấm trực tiếp từ Menu gỗ ban đầu thông qua giao diện OnGUI.
* **3 Tab chức năng chính**:
  1. **Tùy Biến Phím (Key Customization)**: Thiết lập các phím di chuyển (Lên, Xuống, Trái, Phải), Tương Tác (`Interact`), và Chạy nhanh (`Run`). Lưu trữ tự động vào `PlayerPrefs`. Hệ thống tương thích hoàn hảo cả với Legacy Input và New Input System của Unity, tự động cập nhật văn bản hiển thị trên các gợi ý tương tác của người chơi dựa trên phím bấm thực tế đã cài đặt.
  2. **Cẩm Nang Sinh Tồn (Survival Guide)**: Hướng dẫn chi tiết bằng văn bản về cơ chế Trồng trọt, Ứng phó thời tiết (Lũ lụt, Gió Lào), Cúng bàn thờ tổ tiên và cơ chế Ngất xỉu & Cứu hộ.
  3. **Hồ Sơ Nhân Vật (NPC Profiles)**: Xem thông tin lý lịch của các NPC trong làng (Bác Năm, O Thắm) cùng với chỉ số mức độ tình cảm (Affection Level) hiện tại của họ đối với bạn.
* **Tự động Pause**: Game sẽ tự động tạm dừng (`Time.timeScale = 0`) khi mở Settings Menu để đảm bảo người chơi không bị ảnh hưởng bởi thời tiết hoặc mất thể lực khi đang điều chỉnh.

### 🎵 7. Hệ Thống Âm Thanh Động (Audio System & AudioManager)
* Quản lý nhạc nền (BGM) và hiệu ứng âm thanh (SFX) tập trung thông qua `AudioManager.cs`.
* **Nhạc nền & Ambient**:
  * `bgm_menu` tại Main Menu và `bgm_main` khi vào game.
  * Ambient động thay đổi theo thời tiết và giai đoạn: gió xào xạc vùng quê thanh bình (`ambient_rural`), gió hú hanh khô (`ambient_wind_lao`), mưa bão ầm ầm sấm chớp (`ambient_storm`).
* **Hiệu ứng SFX tương tác**:
  * SFX phát ra theo từng hành động thực tế: Nhặt đá/cuốc đất (`sfx_dig`/`sfx_clear_rocks`), gieo hạt (`sfx_plant`), tưới nước (`sfx_water`), thu hoạch (`sfx_harvest`), thắp hương cúng bái (`sfx_altar`), giao dịch tiền xu (`sfx_coin`), bấm nút GUI (`sfx_click`), kiệt sức ngất xỉu (`sfx_faint`), hoặc âm thanh cảnh báo stress cực độ (`sfx_warning`).

### 💤 8. Cơ Chế Sinh Tồn & Cứu Hộ (Faint & Rescue)
* Người chơi cần quản lý 3 chỉ số sinh lý quan trọng: Máu (Health), Thể Lực (Stamina) và Tinh Thần (Morale) cùng với chỉ số Stress Nhiệt (Heat Stress) và Stress Lạnh (Cold Stress).
* Nếu Máu giảm về 0, hoặc Stress đạt đỉnh 100% khi hoạt động quá sức dưới trời nắng gắt (Gió Lào) hoặc bão lũ lạnh giá, nhân vật Thành sẽ ngất xỉu ngay tại chỗ (phát hoạt ảnh ngất và âm thanh `sfx_faint`).
* Sau đó, Thành sẽ được Bác Năm cứu hộ đưa về nhà. Người chơi sẽ tỉnh dậy vào sáng hôm sau tại nhà Bác Năm với các chỉ số được hồi phục một phần nhưng bị khấu trừ một lượng nhỏ tiền xu hoặc nông sản coi như chi phí thuốc men, cứu trợ.

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
   * Dự án hỗ trợ một công cụ Editor tự động hóa việc thiết lập này.
   * Trên thanh menu của Unity Editor, chọn **`Sown In Stone -> Setup Altar`**.
   * Script sẽ tự động:
     * Tải mô hình 3D từ FBX bàn thờ mới của bạn (`Altar_Model.fbx`).
     * Tự gán Material tương ứng (`Mat_Altar`).
     * Căn chỉnh chân bàn thờ sát mặt đất phẳng (`Y = 0`), scale chiều cao hợp lý (cao 1.8m).
     * Đặt bàn thờ ở vị trí chuẩn (`X: 7.5, Z: -13.0`) tại góc dưới (phía trước bên trái) nhà của Thành.
     * Cấu hình trigger BoxCollider kích thước `(2.5, 1.8, 2.5)` bao quanh bàn thờ thuận tiện cho việc thắp nhang tăng Morale.
3. **Thiết lập Sạp Hàng O Thắm & NPC O Thắm (`OTham_Shop`):**
   * Dự án hỗ trợ một công cụ Editor tự động hóa việc thiết lập này.
   * Trên thanh menu của Unity Editor, chọn **`Sown In Stone -> Setup O Tham Shop`**.
   * Script sẽ tự động:
     * Tải mô hình 3D từ các FBX mới của nhà (`Meshy_AI_Low_poly_stylized_3D__0620062116_texture.fbx`) và sạp hàng (`Meshy_AI_Low_poly_stylized_3D__0620065520_texture.fbx`).
     * Tự sinh và gán Material tương ứng (`Mat_OTham_House` và `Mat_OTham_Stall`).
     * Căn chỉnh pivot của cả nhà và sạp hàng sát mặt đất phẳng (`Y = 0`), tự động tỉ lệ hóa chiều cao (nhà cao 4.5m, sạp hàng cao 1.2m).
     * Đặt sạp hàng tại vùng trống bên trái nhà Thành (`X: 4.5, Z: -10.0`).
     * Đặt NPC O Thắm (`NPC_OTham`) đứng sau sạp hàng đối mặt với người chơi (`X: 4.5, Y: 0.5, Z: -11.2`), tự động scale chiều cao NPC đạt 1.7m.
     * Cấu hình BoxCollider bao bọc nhà để ngăn người chơi đi xuyên tường, và cấu hình vùng Trigger của O Thắm bao phủ mặt trước sạp hàng thuận tiện cho việc nhấn phím `[E]` tương tác mua bán.
4. **Thiết lập Nhà Bác Năm & Chõng tre (`BacNam_House`):**
   * Dự án hỗ trợ một công cụ Editor tự động hóa việc thiết lập này.
   * Trên thanh menu của Unity Editor, chọn **`Sown In Stone -> Setup Bac Nam House`**.
   * Script sẽ tự động:
     * Tải mô hình 3D từ các FBX mới của nhà (`BacNam_House_Model.fbx`) và chõng tre (`BacNam_Daybed_Model.fbx`).
     * Tự gán các Material tương ứng (`Mat_BacNam_House` và `Mat_BacNam_Daybed`).
     * Căn chỉnh chân nhà và chõng tre sát mặt đất phẳng (`Y = 0`), scale chiều cao hợp lý (nhà cao 4.5m, chõng tre cao 1.2m).
     * Đặt nhà ở vị trí chuẩn (`X: 8.0, Z: 12.0`) xoay 180 độ đón người chơi, đặt chõng tre ở trước hiên (`X: 8.5, Z: 7.5`).
     * Định vị lại NPC Bác Năm đứng cạnh chõng tre (`X: 7.0, Y: 0.5, Z: 7.5`), tự động scale chiều cao NPC đạt 1.7m.
     * Cấu hình BoxCollider bảo vệ cho nhà để tránh đi xuyên tường, và cấu hình vùng Trigger của Bác Năm bao trùm chõng tre thuận tiện cho việc đối thoại.
5. **Thiết lập Nhà Nhân Vật Thành (`Thanh_House`):**
   * Dự án hỗ trợ một công cụ Editor tự động hóa việc thiết lập này.
   * Trên thanh menu của Unity Editor, chọn **`Sown In Stone -> Setup Thanh House`**.
   * Script sẽ tự động:
     * Tải mô hình 3D của nhà Thành (`Meshy_AI_Stylized_low_poly_3D__0620084846_texture.fbx`).
     * Tự gán Material tương ứng (`Mat_Thanh_House`).
     * Căn chỉnh chân nhà sát mặt đất phẳng (`Y = 0`), scale chiều cao hợp lý (nhà cao 4.5m).
     * Đặt nhà ở vị trí chuẩn (`X: 10.66, Z: -10.0`) xoay 180 độ đón người chơi (quay mặt về phía Nam giống sạp O Thắm).
     * Cấu hình BoxCollider bao bọc nhà để ngăn người chơi đi xuyên tường.
6. **Dân làng (NPCs):**
   * Đối với Bác Năm: Nếu không chạy Script tự động ở trên, bạn tự tạo Empty GameObject tên là `NPC_BacNam`, gắn script **`NPCCharacter`** (chọn Character Type = *Bac Nam*). Thêm **`Box Collider`** (3D), tích **`Is Trigger`**.
   * Đối với O Thắm: Nếu không chạy Script tự động ở trên, bạn tự tạo Empty GameObject tên là `NPC_OTham`, gắn script **`NPCCharacter`** (chọn Character Type = *O Tham*). Thêm **`Box Collider`** (3D), tích **`Is Trigger`**.

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
* **Sơ tán & Sinh tồn lũ (Đã hoàn chỉnh):** Chạy lũ sơ tán 4 dân làng lên nóc nhà, chia sẻ khoai gieo dự trữ để cùng nhắm bỡ qua cơn lũ.
* **Sự bồi đắp sau lũ:** Tái canh tác trên đất phù sa sau khi bão rút và nhận gấp đôi sản lượng nông sản thu hoạch.

## 10. Recommended Development Priorities
Để phát triển từ bản Prototype hiện tại lên phiên bản Demo hoàn chỉnh hơn, nhóm đề xuất các ưu tiên sau:
1. **Tinh chỉnh UI Nghĩa Tình & Vần Công:** Thiết kế lại bảng HUD tương trợ để thể hiện rõ nét hơn việc tích lũy công đổi công và đóng góp lương thực, thay vì hiển thị dạng giao dịch tiền xu thuần túy.
2. **Loa phát thanh xã có âm thanh:** Ghi âm hoặc sinh procedurally giọng đọc phát thanh thông báo đầu ngày bằng tiếng Việt mang chất giọng miền Trung chân thực để tăng tính nhập vai.
3. **Cơ chế gia cố nhà cửa bằng hình ảnh:** Khi người chơi tiêu hao điểm Vần công tương trợ chằng chống nhà cửa, mô hình ngôi nhà của Thành và các NPC sẽ hiển thị thêm lưới thép, bao cát, hoặc dây chằng xung quanh.
4. **Cân bằng sinh lý dưới thời tiết:** Tinh chỉnh tốc độ tăng Heat Stress/Cold Stress để tạo độ thử thách vừa phải, khuyến khích người chơi ở nhà tránh nắng Gió Lào buổi trưa hoặc tránh ra đồng khi mưa lũ dâng cao.

---

## 11. CHANGELOG

### [feature/fix-roof-survival-movement] — 2026-07-01

#### Bug Fixes
- **[CRITICAL] Khắc phục player bị kẹt trên nóc nhà (`PlayerController.cs`)**
  - Xóa toàn bộ `Mathf.Clamp(X/Z)` và `rb.position = constrainedPos` trong khối `isOnRoof` của `FixedUpdate`.
  - Trước đó, player bị clamp cứng X trong `[-2.2, 2.2]` theo tọa độ tuyệt đối; Bác Năm/Bé Tí ở X ≈ 3.82–3.89 vượt ra ngoài giới hạn nên player chặn hoàn toàn mỗi FixedUpdate frame.
- **Disable `CharacterController` khi lên mái nhà (`TutorialManager.cs`)**
  - Player có cả `CharacterController` lẫn `Rigidbody`. Khi cả hai cùng active, capsule collider ẩn của `CharacterController` xung đột với `rb.linearVelocity` gây kẹt vật lý.
  - `CharacterController` bị disable khi bắt đầu `RoofSurvivalSharing`, restore khi sang `PostStormCleanup`.
- **Fix `NullReferenceException` trong `NPCProximityOptionsUI.cs`**
  - Thêm null guard `if (action != null)` trước mỗi gọi `action()` trong `HandleKeyboardInput()`.
  - Loại bỏ hoàn toàn chuỗi exception spam mỗi frame trong Unity Console.

#### Improvements
- `TempRoofCollider` mở rộng lên 25×25m để phủ toàn bộ diện tích nóc nhà.
- Toàn bộ collider gốc của `Thanh_House` bị tạm thời disable khi trên mái, tránh va chạm với dốc mái/chi tiết 3D.
- NPC collider trên mái được chuyển thành trigger khi lên mái, restore khi về nhà.
