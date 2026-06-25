# Art Pipeline: Đất Cày Lên Sỏi Đá

Tài liệu này định hình phong cách nghệ thuật, yêu cầu mô hình 3D, hiệu ứng thời tiết (weather moods), và quy trình thiết kế môi trường nhằm tôn vinh tối đa bản sắc văn hóa Việt Nam và đảm bảo trải nghiệm trực quan cho bản chơi thử PRU213.

---

## 1. Visual Art Direction
* **Phong cách chủ đạo:** Stylized Low Poly (3D đa giác đơn giản hóa, cách điệu hóa).
* **Đặc tính thẩm mỹ:** Ấm áp nhưng nhuốm màu sương gió (warm but weathered). Mô tả chân thực sự cằn cỗi của miền cát trắng nóng gió và sức tàn phá của bão lũ, nhưng vẫn giữ được sự bình dị, mộc mạc của làng quê miền Trung.
* **Nguyên tắc định hướng:** **Mọi tài nguyên mỹ thuật đưa vào game phải phục vụ trực tiếp cho bản sắc văn hóa Việt Nam hoặc khả năng nhận biết gameplay**. Tránh lạm dụng các asset có sẵn lung linh nhưng không mang hồn cốt nông thôn Việt Nam (ví dụ: nhà kiểu Tây Âu, cối xay gió Trung Cổ).

---

## 2. Required Environment Elements
Các mô hình 3D môi trường bắt buộc phải có trong Scene nhằm phản ánh không gian văn hóa địa phương:
* **Dry rocky field:** Vạt đất cát pha sỏi đá hoang hóa cằn cỗi cần dọn dẹp.
* **Farming Plot:** Lưới trồng trọt 4x3 (tổng cộng 12 SoilCells) mẫu cạnh nhà Thành.
* **Village path:** Đường mòn làng quê nhỏ hẹp, nền đất/cát hoặc bê tông cũ kỹ.
* **Bamboo fence:** Hàng rào tre đơn sơ bao quanh vườn nhà (gồm 12 đoạn rào tre xếp xung quanh ruộng).
* **Old tiled house:** Ngôi nhà ba gian mái ngói rêu phong kiểu truyền thống miền Trung (nhà Thành).
* **Village Well:** Giếng làng xây bằng đá cổ kính, nơi tụ họp sinh hoạt chung.
* **Village speaker pole (Loa phát thanh xã):** Cột gỗ/bê tông treo 2 cụm loa phát thanh quay hai hướng, biểu tượng truyền thông đặc trưng.
* **Ancestral altar or shrine:** Miếu thờ nhỏ đầu làng hoặc bàn thờ gia tiên giản dị trong nhà để thắp nhang cầu an.
* **Floodable low area:** Vùng trũng thấp trên bản đồ để nước lũ dâng ngập tích tụ dìm sâu đất canh tác.
* **Post-flood muddy/silt field:** Texture đất ruộng bồi đắp màu phù sa xám đục ẩm ướt sau khi lũ rút.
* **Simple NPC houses:** Nhà tranh vách đất hoặc nhà cấp bốn đơn sơ của O Thắm, Bác Năm.

---

## 3. Required Mood Phases (Chu kỳ ánh sáng và thời tiết 2 giai đoạn)
Hiệu ứng môi trường, sương mù (fog) và bầu trời (skybox) chuyển đổi tương ứng theo từng giai đoạn cốt truyện:
1. **Phase 1: Before the Storm (Trước Bão) [Ngày 1–4]:**
   - *Khởi đầu*: Ánh sáng ấm dịu, bầu trời trong trẻo, tiếng loa rè nhẹ báo ngày mới bình yên thôn quê.
   - *Chuyển tiếp*: Bầu trời xám xịt dần, sương mù tích tụ báo hiệu giông bão giáp mặt, gió thổi rít nhẹ.
2. **Phase 2: After the Storm (Sau Bão) [Ngày 5+]:**
   - *Bão lũ*: Bầu trời tối sầm, gió thổi chéo rít mạnh kết hợp hệ thống hạt mưa rơi xối xả bám theo camera. Nước lũ đục ngầu dâng ngập ruộng vườn trắng xóa.
   - *Hồi phục*: Nước rút đi hửng sáng trở lại, sương mù ẩm ướt bốc lên từ lòng đất phù sa ẩm xám giàu dinh dưỡng. Màu trời xanh ấm áp báo hiệu ngày hội mừng công tái thiết làng quê.

---

## 4. Asset Priorities (Độ ưu tiên thiết lập Asset)

### Nhóm ưu tiên CAO (High - Bắt buộc phải có và trau chuốt)
* **Ground/Terrain:** Texture đất cát trắng, đất khô cằn, và texture bùn phù sa sau lũ.
* **Player Farmer:** Model nhân vật Thành đậm chất nông dân (nón lá, áo cộc mộc mạc).
* **NPCs:** Model Bác Năm sương gió, O Thắm đôn hậu, Cụ Bảy và Bé Tí.
* **Soil cells & Crops:** 3D model sinh trưởng của cây Khoai Lang trên lưới 4x3.
* **Houses:** Ngôi nhà truyền thống ba gian của Thành và các hộ dân xung quanh.
* **Flood water plane:** Mặt nước lũ đục ngầu dâng lên hạ xuống trơn tru ở Phase 2.
* **Village speaker (Loa phát thanh xã):** Mô hình loa cột phát thanh rõ ràng, đặt ở trung tâm làng.

### Nhóm ưu tiên TRUNG BÌNH (Medium - Giúp tăng không khí làng quê)
* **Tre trúc, rào tre, sỏi đá trang trí:** Tạo biên giới tự nhiên và các điểm nhấn rải rác trên ruộng đồng.
* **Bao cát, ván chắn lũ, dây thừng chằng mái:** Phục vụ trực quan hóa cơ chế chống bão.
* **Bàn thờ/Miếu thờ nhỏ:** Nơi thực hiện hành động thắp nhang phục hồi tinh thần.

### Nhóm ưu tiên THẤP (Low - Chỉ thêm vào nếu dư thời gian)
* **Sạp chợ buôn bán:** Do không tập trung vào kinh tế làm giàu, bỏ qua các sạp bán đồ lớn kiểu phương Tây.
* **Gia súc, gia cầm chạy rông:** (Gà, chó cỏ) chỉ mang tính trang trí bổ trợ.
* **Mở rộng bản đồ rộng lớn:** Giới hạn map nhỏ gọn xung quanh vườn Thành và nhà các NPC để tối ưu hiệu suất và camera.
