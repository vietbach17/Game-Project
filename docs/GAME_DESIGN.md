# Game Design Document: Đất Cày Lên Sỏi Đá

---

## 1. High Concept
**Đất Cày Lên Sỏi Đá** là một trò chơi nhập vai sinh tồn cộng đồng theo cốt truyện (Narrative Community Survival Game) lấy bối cảnh tại một ngôi làng nghèo ven biển miền Trung Việt Nam. Trò chơi phản ánh chân thực cuộc sống kiên cường trước thiên tai khắc nghiệt, đề cao tình làng nghĩa xóm, sự sẻ chia tương trợ và văn hóa đổi công (vần công) truyền thống của người Việt[cite: 24].

---

## 2. Player Role
Người chơi vào vai **Thành**, một thanh niên trở về quê hương sau những biến cố lớn của gia đình[cite: 24]. Thành bắt đầu với việc khôi phục mảnh vườn hoang hóa của cha mẹ[cite: 24]. Tuy nhiên, anh sớm nhận ra rằng để tồn tại lâu dài, anh không thể sinh tồn đơn độc mà phải chung tay giúp cả làng chống chọi với mưa bão, lũ lụt và phục hồi đời sống sau thiên tai[cite: 24].

---

## 3. Core Gameplay Loop
Phase 1: Before the Storm
↓
Gặp đủ 4 dân làng (O Thắm, Bác Năm, Cụ Bảy, Bé Tí) để hoàn tất IntroQuests
↓
Khai hoang đất cát pha sỏi đá trên solid 4x3 grid (12 SoilCells độc lập)
↓
Gieo hạt, tưới nước, thu hoạch khoai tươi & chế biến khoai gieo khô
↓
Thắp nhang tại Ancestral Altar [E] để cầu an và kích hoạt bão
↓
Mid-Phase Countdown: 45 giây chạy cảnh báo/sơ tán khẩn cấp 4 dân làng
↓
Phase 2: After the Storm
↓
Trú ẩn trên mái Thanh_House, nhận cứu trợ, giữ Morale/Stamina
↓
Gió lớn làm mái nhà rung lắc → Dùng bao cát (sandbag) chèn ngói chống tốc mái
↓
Nước rút → Toàn bộ 12 SoilCells tự động bồi đắp Phù Sa → yield x2
↓
Tái thiết nhà cửa, dùng tấm ván gỗ (flood board) sửa vách tường sập cho O Thắm & Bác Năm
↓
Phân phối lương thực cứu trợ cho người yếu thế (Cụ Bảy & Bé Tí) → Quyết định Kết cục
---

## 4. Player Objectives
* **Mục tiêu ngắn hạn (Hằng ngày):** Kiểm tra đất trồng, tưới nước bù ẩm, dọn đá dọn ruộng trên lưới 12 ô, thu hoạch khoai tươi, chế biến khoai khô dự phòng ẩm mốc, và lắng nghe chỉ dẫn từ Loa phát thanh xã[cite: 24].
* **Mục tiêu trung hạn (Before the Storm):** Giúp đỡ O Thắm, Bác Năm, Cụ Bảy và Bé Tí để hoàn thành IntroQuests đủ cả 4 người, học vòng lặp canh tác, tích lũy Nghĩa Tình/Vần công và chủ động thắp nhang tại bàn thờ tổ tiên để kích hoạt bão[cite: 24].
* **Mục tiêu khủng hoảng (45 giây):** Trong Mid-Phase Countdown, chạy đua với thời gian để đến vị trí của từng người dân làng, nhấn phím `[E]` dắt họ sơ tán trước khi nước lũ đạt đỉnh[cite: 24].
* **Mục tiêu dài hạn (After the Storm):** Sinh tồn trên mái Thanh_House đứng cùng các cư dân cứu được, sử dụng bao cát chống tốc mái, chờ nước rút, tận dụng đất Phù Sa để trồng vụ mùa bội thu, dùng tấm ván gỗ sửa tường sập và cứu trợ bà con để đưa điểm Nghĩa Tình lên mốc cao nhất[cite: 24].

---

## 5. Main Resources & Items
* **Stamina (Thể lực):** Tiêu hao khi làm nông (cuốc đất, tưới nước, dọn đá) và khi thực hiện công vần công giúp dân làng[cite: 24]. Phục hồi qua giấc ngủ hoặc ăn nông sản[cite: 24].
* **Food (Lương thực):** 
  * *Khoai tươi (Fresh Crop):* Ăn trực tiếp để hồi thể lực, nhưng dễ thối rữa dưới độ ẩm cao của mùa mưa bão ở Phase 2[cite: 24].
  * *Khoai gieo (Preserved Crop):* Khoai tươi cắt lát phơi khô bằng bếp ga dưới mái hiên[cite: 24]. Không bao giờ thối hỏng, dùng làm lương thực cốt lõi để quyên góp cứu trợ sau lũ[cite: 24].
* **Seeds (Hạt giống):** Tài nguyên giới hạn dùng để trồng trọt, nhận được qua trao đổi tương trợ với O Thắm[cite: 24].
* **Nghĩa Tình:** Chỉ số tiến trình (progression) chính[cite: 24]. Đo lường sự tin tưởng và gắn kết của cộng đồng với Thành, quyết định kết cục của game[cite: 24].
* **Xu hỗ trợ (Coins - Secondary):** Đơn vị trao đổi thứ cấp thể hiện nguồn lực hỗ trợ nhỏ từ bên ngoài để Thành mua hạt giống hoặc nhang cúng từ O Thắm[cite: 24].
* **Tấm Ván Gỗ (`item_flood_board`):** Vật liệu chuyên dụng ở Phase 2 dùng để sửa mảng tường sập tại nhà O Thắm/Bác Năm, hoàn toàn loại bỏ khỏi Phase 1 để hợp lý hóa cốt truyện[cite: 24].
* **Bao Cát (`item_sandbag`):** Vật liệu chuyên dụng ở Phase 2 dùng để neo/chèn mái ngói bị rung lắc khi trú bão trên nóc nhà Thành[cite: 24].

---

## 6. NPCs (Nhân vật phụ)
* **Bác Năm:** Lão nông giàu kinh nghiệm nhưng neo đơn, sức khỏe yếu[cite: 24]. Bác hướng dẫn Thành cách dọn đá ruộng cằn và là người cần Thành sửa vách tường nhà sập cũng như chia sẻ lương thực nhất sau bão[cite: 24].
* **O Thắm:** Chủ đại lý nhu yếu phẩm nhỏ trong làng[cite: 24]. O là đầu mối đổi công vần công chính và là nơi Thành cần đến dùng ván gỗ dựng lại sạp hàng bị đổ nát sau lũ[cite: 24].
* **Cụ Bảy:** Trưởng thôn, người điều hành Loa phát thanh xã và trông coi Ancestral Altar[cite: 24]. Cụ là đối tượng yếu thế cần được ưu tiên cứu trợ lương thực chống đói khẩn cấp trong mùa bão lũ[cite: 24].
* **Bé Tí:** Con trai O Thắm, hỗ trợ truyền tin và phát thoại cảnh báo[cite: 24]. Bé Tí là một checkpoint sơ tán độc lập trong countdown chạy lũ, không bị gộp chung vào O Thắm[cite: 24].

**IntroQuests Gate Rule:** Phase 1 bắt buộc người chơi phải hoàn tất đàm thoại với TẤT CẢ 4 NPC (O Thắm, Bác Năm, Cụ Bảy, Bé Tí) mới tính là xong nhiệm vụ thăm hỏi và mở phần farming slides[cite: 24].

**Mid-Phase Rescue Rule:** Số lượng dân làng dắt sơ tán được trong countdown 45 giây sẽ ảnh hưởng trực tiếp đến điểm Nghĩa Tình, lượng mì tôm cứu trợ và số NPC xuất hiện cùng Thành trên nóc nhà ở Phase 2[cite: 24].

---

## 7. Cultural Mechanics
* **Vần công (Community Mutual Aid):** Thành tiêu hao thể lực giúp việc cho hàng xóm để tích lũy tín chỉ Vần công, đổi lấy sự chung sức tương trợ của cộng đồng[cite: 24].
* **Loa phát thanh xã:** Phát thanh mỗi sáng để cung cấp chỉ số thời tiết hằng ngày, cập nhật tin bão lũ khẩn cấp và khích lệ tinh thần người dân[cite: 24].
* **Tích cốc phòng cơ:** Cơ chế phơi sấy chế biến khoai tươi thành khoai khô không thối để tích trữ tài nguyên lâu dài vượt qua mùa thiên tai ngập lụt[cite: 24].
* **Bàn thờ tổ tiên (Ancestral Altar):** Người chơi bấm phím `[E]` thắp nhang cúng để giải tỏa lo âu, hồi phục chỉ số Tinh thần (Morale)[cite: 24]. Đồng thời, nghi thức thắp nhang sau khi hoàn tất tutorial chính là trigger đóng Phase 1 và kích hoạt bão lũ[cite: 24].

---

## 8. Game Phases (Chu Kỳ Thiên Tai 2 Giai Đoạn)
1. **Phase 1: Before the Storm (Trước Bão):** Khai hoang đất trên lưới ruộng 4x3 (12 ô SoilCells), gieo lứa khoai đầu, hoàn thành IntroQuests nói chuyện với đủ 4 NPC, và tiến hành thắp nhang tại bàn thờ để cầu an/kích bão[cite: 24]. Gió Lào bị loại bỏ hoàn toàn làm phase riêng[cite: 24].
2. **Mid-Phase Countdown: Evacuation Crisis (45 giây):** Loa xã báo động khẩn[cite: 24]. Nước lũ dâng từ từ theo trục Y[cite: 24]. Người chơi có 45 giây chạy đua tìm đến 4 NPC bấm phím `[E]` dắt họ sơ tán trước khi floodwater đạt đỉnh[cite: 24].
3. **Phase 2: After the Storm (Sau Bão):** Trú ẩn sinh tồn và chèn bao cát chống tốc mái trên nóc nhà Thành[cite: 24]. Khi nước rút, toàn bộ 12 ô ruộng tự động bồi đắp Phù Sa (yield x2)[cite: 24]. Gameplay tập trung vào dọn bùn, gieo vụ mới, mang tấm ván gỗ đi dựng lại vách tường sập cho O Thắm/Bác Năm, quyên góp khoai khô cho Cụ Bảy/Bé Tí và tổng kết kết cục[cite: 24].

---

## 9. Endings (Hệ thống Kết thúc)
Sau khi kết thúc chu kỳ tái thiết, `EndingManager` kiểm tra tổng điểm Nghĩa Tình để hiển thị kết cục[cite: 24]:
* **🏆 Kết cục Best - Đất Cày Nở Hoa (Nghĩa Tình $\ge 80$):** Làng quê hồi sinh rực rỡ, ruộng khoai bội thu, Thành được cả làng vinh danh dưới loa phát thanh xã[cite: 24].
* **😐 Kết cục Normal - Lá Lành Đùm Lá Rách (Nghĩa Tình 40 - 79):** Ngôi làng sửa sang xong nhưng ai nấy lẳng lặng lo việc nhà nấy, tình cảm xóm giềng dừng ở mức xã giao[cite: 24].
* **💔 Kết cục Sad - Đất Sỏi Đá Cằn (Nghĩa Tình < 40):** Thành ích kỷ giữ tài nguyên, làng quê tan hoang ly tán sau bão, O Thắm bỏ xứ đi nơi khác, Bác Năm quay lưng, Thành cô độc trên sỏi đá[cite: 24].