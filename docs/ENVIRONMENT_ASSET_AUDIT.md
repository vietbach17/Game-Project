# Environment Asset Audit: Đất Cày Lên Sỏi Đá

Tài liệu này kiểm kê và phân loại toàn bộ tài nguyên môi trường (3D models, textures, prefabs) hiện có hoặc cần bổ sung trong dự án, đảm bảo tối ưu hóa dung lượng và tập trung hoàn toàn vào bối cảnh làng quê miền Trung Việt Nam.

---

## 1. Phân Loại Tài Nguyên Theo Độ Ưu Tiên

---

### Nhóm 1: Thiết Yếu Cho Gameplay (Essential for Gameplay)
Các asset trực tiếp vận hành vòng lặp canh tác sinh tồn và di chuyển:

*   **1. Soil Cell (Ô đất trồng)**
    *   *Trạng thái hiện tại:* Đã cấu hình prefab `SoilCell.prefab` kèm trigger collider và script tương tác.
    *   *Đề xuất sử dụng:* Đặt 4 ô đất làm ruộng canh tác chính của Thành.
    *   *Độ ưu tiên:* Cao nhất (Priority 0).
    *   *Lưu ý kỹ thuật:* Cần đảm bảo trigger BoxCollider căn chỉnh đúng kích thước hiển thị để nhân vật tiếp cận nhấn `[E]` dễ dàng.
*   **2. Ground Textures (Đất khô, đất ướt, cát trắng)**
    *   *Trạng thái hiện tại:* Đã có tệp `dry_soil.png` và `wet_soil.png`.
    *   *Đề xuất sử dụng:* Thay đổi vật liệu đất trồng tương ứng theo độ ẩm thực tế khi tưới tiêu hoặc nắng hạn.
    *   *Độ ưu tiên:* Cao (Priority 0).
*   **3. Flood Water Plane (Mặt nước lũ)**
    *   *Trạng thái hiện tại:* Đã có tệp `water_waves.png` làm texture phủ nước.
    *   *Đề xuất sử dụng:* Tấm phẳng 3D nước lũ nâng hạ độ cao theo biến `waterLevel` ở Phase 3.
    *   *Độ ưu tiên:* Cao (Priority 0).
    *   *Lưu ý kỹ thuật:* Bỏ MeshCollider trên water plane để nhân vật lội nước bình thường mà không bị chặn, chỉ kích hoạt hiệu ứng sinh lý lạnh.
*   **4. Potato Crop 3D Models (Model Khoai Lang)**
    *   *Trạng thái hiện tại:* Đã gán `Plant_1`, `Plant_3`, `Plant_5` từ Nature Pack cho các giai đoạn mầm, phát triển, chín.
    *   *Đề xuất sử dụng:* Sinh tự động tại tọa độ con của SoilCell tương ứng với mức lớn của cây.
    *   *Độ ưu tiên:* Cao (Priority 0).

---

### Nhóm 2: Thiết Yếu Cho Bản Sắc Văn Hóa (Essential for Vietnamese Cultural Identity)
Các asset trực tiếp truyền tải linh hồn làng quê miền Trung Việt Nam:

*   **1. Hoi An House (`HoiAnHouse_M2.fbx`)**
    *   *Trạng thái hiện tại:* Đã import tệp fbx vào `Assets/Prefabs/`.
    *   *Đề xuất sử dụng:* Làm nhà ở chính ba gian của Thành.
    *   *Độ ưu tiên:* Cao (Priority 0).
    *   *Lưu ý kỹ thuật:* Khớp tỷ lệ scale của Mesh và căn BoxCollider bọc ngoài chân tường để nhân vật không đi xuyên tường.
*   **2. Asian House (`asian_house.glb`)**
    *   *Trạng thái hiện tại:* Đã import vào `Assets/Prefabs/`.
    *   *Đề xuất sử dụng:* Làm nhà và cửa hàng đại lý tương trợ của O Thắm.
    *   *Độ ưu tiên:* Cao (Priority 0).
*   **3. Village Well (`Well.fbx`)**
    *   *Trạng thái hiện tại:* Đã có prefab đá cổ.
    *   *Đề xuất sử dụng:* Giếng nước trung tâm làng cạnh nhà Thành để lấy nước tưới.
    *   *Độ ưu tiên:* Cao (Priority 0).
*   **4. Village Speaker Pole (Cột loa phát thanh)**
    *   *Trạng thái hiện tại:* Đã import model cột loa.
    *   *Đề xuất sử dụng:* Đặt ở ngã ba làng, phát âm thanh rè rè thông báo chuyển Phase vào mỗi sáng.
    *   *Độ ưu tiên:* Cao (Priority 0).
*   **5. Ancestral Altar (Bàn thờ/Miếu thờ đầu làng)**
    *   *Trạng thái hiện tại:* Đã gán script `AncestralAltar` hỗ trợ thắp nhang.
    *   *Đề xuất sử dụng:* Đặt dưới gốc cây đa cổ thụ đầu làng làm điểm hồi tinh thần.
    *   *Độ ưu tiên:* Trung bình (Priority 1).

---

### Nhóm 3: Hỗ Trợ Làm Đẹp Môi Trường (Useful for Visual Polish)
Tăng chiều sâu thẩm mỹ sương gió của ngôi làng ven biển:

*   **1. Banana Tree (`BananaTree.obj`)**
    *   *Trạng thái hiện tại:* Đã import kèm tệp base color texture.
    *   *Đề xuất sử dụng:* Đặt sau vườn nhà Thành và dọc các lối đi nhỏ.
    *   *Độ ưu tiên:* Trung bình (Priority 1).
*   **2. Bamboo Stalks (`PUSHILIN_bamboo.obj`)**
    *   *Trạng thái hiện tại:* Đã import.
    *   *Đề xuất sử dụng:* Ghép cụm làm bụi tre làng rợp bóng mát đầu ngõ.
    *   *Độ ưu tiên:* Trung bình (Priority 1).
*   **3. Silt Soil Texture (Đất phù sa bồi đắp)**
    *   *Trạng thái hiện tại:* Có tệp `silt_soil.png`.
    *   *Đề xuất sử dụng:* Tự động đổi vật liệu đất trồng sang màu phù sa xám ẩm khi chuyển sang Phase 4 sau khi rút lũ.
    *   *Độ ưu tiên:* Trung bình (Priority 1).
*   **4. Wood Logs & Rocks (`WoodLog.fbx` / `Rock_1..7.fbx`)**
    *   *Trạng thái hiện tại:* Sẵn có trong Ultimate Nature Pack.
    *   *Đề xuất sử dụng:* Rải rác quanh ruộng để làm chướng ngại vật đá cằn dọn dẹp hoặc củi gỗ chuẩn bị lũ.
    *   *Độ ưu tiên:* Trung bình (Priority 1).

---

### Nhóm 4: Ít Ưu Tiên / Tùy Chọn (Optional / Low Priority)
Có thể thêm hoặc lược bỏ không ảnh hưởng chất lượng Demo:

*   **1. Town Buildings (`House_1.fbx` đến `House_4.fbx`)**
    *   *Trạng thái hiện tại:* Sẵn có trong thư mục `FBX-Village/`.
    *   *Đề xuất sử dụng:* Làm hậu cảnh xa xa cho ngôi làng nếu cần thêm mật độ nhà.
    *   *Độ ưu tiên:* Thấp (Priority 2).
*   **2. Palm & Willow Trees (Cây cọ, cây liễu)**
    *   *Trạng thái hiện tại:* Sẵn có trong Nature Pack.
    *   *Đề xuất sử dụng:* Trang trí ngoài rìa bản đồ.
    *   *Độ ưu tiên:* Thấp (Priority 2).

---

### Nhóm 5: Loại Bỏ Hoặc Bỏ Qua (Remove or Ignore for Now)
Không phù hợp văn hóa Việt Nam hoặc làm loãng phạm vi Demo:

*   **1. Blacksmith, Inn, Sawmill, Stable (`Blacksmith.fbx`...)**
    *   *Trạng thái hiện tại:* Sẵn có trong thư mục `FBX-Village/`.
    *   *Lý do bỏ qua:* Đây là các mô hình mang phong cách thị trấn thời Trung cổ châu Âu (Medieval), hoàn toàn phá vỡ không khí làng quê Việt Nam. Không đưa vào Scene.
*   **2. Cây phủ tuyết (`Rock_Snow`, `Tree_Snow`)**
    *   *Lý do bỏ qua:* Miền Trung Việt Nam chịu hạn hán bão lũ, không có tuyết rơi.
*   **3. Đồ trang trí chợ phương Tây (Market stalls)**
    *   *Lý do bỏ qua:* Tránh làm loãng kinh tế tương trợ bằng các sạp trao đổi kiểu RPG phương Tây.

---

## 2. Thiết Kế Bản Đồ Thu Gọn (Compact Playable Area)
* Không mở rộng địa hình lớn. Thiết kế khu vực chơi bó hẹp trong diện tích khoảng **50m x 50m**:
  * **Trung tâm:** Mảnh vườn nhà Thành (gồm 4 ô đất SoilCell, giếng nước, nhà ngói ba gian).
  * **Phía Tây:** Cửa hàng nhỏ của O Thắm và bụi chuối hàng rào tre.
  * **Phía Đông:** Ngôi nhà tranh đơn sơ của Bác Năm.
  * **Phía Bắc:** Đình làng cổ kính có Cột loa phát thanh xã và Bàn thờ cổ.
  * **Rìa ngoài:** Bờ bụi tre dày đặc và các ngọn đồi sỏi đá làm biên giới chặn di chuyển tự nhiên của nhân vật, giải phóng bộ nhớ và tránh lỗi camera đi ra ngoài phạm vi thiết kế.
