# Đất Cày Lên Sỏi Đá (Sown in Stone) — GAME BIBLE

*Tài liệu này là Nguồn Sự Thật Duy Nhất (Single Source of Truth) cho toàn bộ dự án Đất Cày Lên Sỏi Đá. Tất cả các AI Agent và lập trình viên bắt buộc phải đọc tài liệu này đầu tiên trước khi thực hiện bất kỳ thay đổi nào để tránh hiện tượng loãng tính năng (feature drift) và đảm bảo tính nhất quán.*

---

## 1. Game Identity (Định danh dự án)
*   **Tên game:** Đất Cày Lên Sỏi Đá / Sown in Stone
*   **Thể loại:** Narrative Community Survival Game with farming elements (Mô phỏng sinh tồn cộng đồng theo cốt truyện kết hợp làm nông đơn giản).
*   **Đồ họa:** 3D Stylized Low Poly (Mộc mạc, rêu phong, phản ánh làng quê miền Trung Việt Nam).
*   **Thời lượng trải nghiệm:** 10 - 15 phút chơi thử (Demo PRU213).
*   **Mục tiêu học thuật:** Đạt điểm tối đa môn PRU213 bằng cách tập trung làm nổi bật bản sắc văn hóa Việt Nam thông qua cơ chế gameplay trực quan, tính thẩm mỹ cao và vòng lặp chơi ổn định, không lỗi.

---

## 2. Vision & Core Message (Tầm nhìn & Thông điệp cốt lõi)
*   **Tầm nhìn:** Phản ánh chân thực tinh thần kiên cường vượt qua nghịch cảnh thiên tai khắc nghiệt (mưa bão ngập lụt miền Trung) của con người miền Trung Việt Nam.
*   **Thông điệp cốt lõi:** **"Không làm nông để làm giàu, mà làm để sẻ chia và cùng nhau vượt qua giông bão."** Sự sinh tồn của cá nhân nhân vật chính luôn gắn liền chặt chẽ với sự sinh tồn của cả cộng đồng làng xã.

---

## 3. Core Gameplay Loop (Vòng lặp gameplay lõi)
```
Dọn sỏi đá & Canh tác khoai lang (Sản xuất lương thực trên lưới 4x3)
    │
    ▼
Lao động tương trợ / Vần công giúp đỡ dân làng
    │
    ▼
Tích lũy điểm Nghĩa Tình & Tích lũy công đổi công
    │
    ▼
Ứng phó thiên tai (Phase 1: Trước Bão - Gia cố nhà cửa bằng công vần công)
    │
    ▼
Sinh tồn ngập lũ & Tái thiết sau lũ (Phase 2: Sau Bão - Trồng vụ mùa bội thu trên đất Phù Sa mới)
```

---

## 4. Main Progression & Victory Condition
*   **Chỉ số thăng tiến chính:** **Nghĩa Tình score** (0 - 100). Đây là tài nguyên cốt lõi quyết định kết thúc của game.
*   **Điều kiện chiến thắng:** Đạt điểm Nghĩa Tình cao nhất ở cuối Phase 2 (Sau Bão) để kích hoạt **Best Ending: Đất Cày Nở Hoa** (Làng quê khôi phục trù phú, bà con đoàn kết vượt thiên tai).

---

## 5. Phase Progression (Chu kỳ thiên tai 2 giai đoạn)
*Tỷ lệ thời gian: 1 ngày game = 5 phút thực tế.*

1.  **Phase 1: Before the Storm (Trước Bão) [Ngày 1 - 4]:** Trải nghiệm ôn hòa của những ngày đầu lập nghiệp. Người chơi cuốc đất dọn đá trên lưới 4x3, gieo hạt, tưới nước bù ẩm, trò chuyện với cả 4 NPC (O Thắm, Bác Năm, Cụ Bảy, Bé Tí) để hoàn thành các mục tiêu hướng dẫn ban đầu, tích lũy công Vần công, gia cố nhà cửa và chuẩn bị lương thực dự trữ chống bão.
2.  **Phase 2: After the Storm (Sau Bão) [Ngày 5+]:** Mưa bão cuồng phong và lũ lụt dâng cao ngập ruộng vườn. Người chơi phải leo lên nóc nhà để sinh tồn lánh nạn, ăn mì cứu trợ. Sau khi lũ rút, lớp đất Phù Sa màu mỡ tự động bồi đắp bồi dưỡng ruộng vườn (xóa đá, nâng cao dinh dưỡng) để người chơi cùng dân làng tái thiết và gieo trồng bội thu vụ mới (yield x2), đồng thời quyết định kết cục của câu chuyện.

---

## 6. Cultural Pillars (Trụ cột văn hóa trong gameplay)
*   **Nghĩa Tình:** Lòng tin cộng đồng, tăng khi Thành quyên góp khoai cứu trợ hoặc giúp việc cho bà con.
*   **Vần công:** Đổi công tương trợ lẫn nhau. Giúp đỡ dân làng lúc bình thường để nhận lại sự chung sức gia cố nhà cửa trước khi bão lũ ập tới.
*   **Loa phát thanh xã:** Rè rè phát tin đầu ngày báo thời tiết và kêu gọi đoàn kết từ Trưởng thôn.
*   **Tích cốc phòng cơ:** Chế biến khoai tươi thành khoai gieo phơi khô tại bếp lò để trữ lâu dài không lo thối úng dưới độ ẩm cao của bão lũ.
*   **Bàn thờ tổ tiên (Ancestral Altar):** Thắp nhang cầu nguyện để giải tỏa Stress, hồi phục Tinh thần (Morale).

---

## 7. Main NPCs
*   **O Thắm:** Đầu mối tương trợ. Phân phối hạt giống/nhang ban đầu bằng Xu hỗ trợ. Sẵn sàng đổi công vần công.
*   **Bác Năm:** Lão nông neo đơn cần Thành quyên góp lương thực giúp đỡ khi bão lũ cô lập ngôi làng.
*   **Cụ Bảy:** Trưởng thôn giữ gìn tinh thần cộng đồng và trông coi Ancestral Altar.
*   **Bé Tí:** Người truyền tin và thông báo sự thay đổi thời tiết khẩn cấp cho Thành.

---

## 8. Main Systems & Resources
*   **Stamina:** Thể lực tiêu hao khi làm việc đồng áng hoặc giúp việc vần công. Hồi phục bằng cách ngủ hoặc ăn khoai.
*   **Food:** Khoai lang tươi (ăn để hồi stamina, dễ thối do ẩm) và Khoai gieo khô (dùng cứu trợ lâu dài).
*   **Farming:** Lưới trồng trọt 4x3 (12 SoilCells). Cuốc dọn đá $\rightarrow$ Gieo hạt $\rightarrow$ Tưới ẩm $\rightarrow$ Thu hoạch.
*   **Weather/Flood:** Lerp mượt mà ánh sáng mặt trời, cường độ sương mù, mưa rơi chéo và mực nước lũ nâng hạ thực tế.

---

## 9. Rules of Design: Must Exist vs. Must NOT Exist

### Things that MUST exist (Bắt buộc phải có)
*   [x] Thể hiện rõ nét các hoạt động mang hồn cốt nông thôn Việt Nam (nón lá, loa rè đầu cột, cúng đình làng, đổi công vần công).
*   [x] Hệ thống dâng nước lũ thực tế trong không gian 3D ngập úng ruộng vườn.
*   [x] Chỉ số Nghĩa Tình là progression duy nhất để đánh giá kết thúc game ở màn hình Ending Screen.
*   [x] Banner Cinematic thông báo ngày mới và dự báo khẩn cấp từ Loa phát thanh xã.
*   [x] Khả năng sấy/chế biến nông sản tránh hư hại trong bão lũ.

### Things that MUST NOT exist (Cấm phát triển - Tránh feature drift)
*   🚫 **Không xây dựng hệ thống Save/Load:** Demo chơi liên tục 20 phút, lưu RAM tạm thời là đủ.
*   🚫 **Không làm cây hội thoại phân nhánh phức tạp:** Dùng popups 3 nút bấm UI đơn giản tại chỗ.
*   🚫 **Không làm hệ thống nâng cấp trang bị/crafting lớn:** Không cần ghép vũ khí, rèn cuốc, hay mở rộng kho đồ.
*   🚫 **Không xây dựng kinh tế thị trường đầu cơ:** Xu chỉ là đơn vị hỗ trợ giao dịch hạt giống cơ bản từ O Thắm. Không làm giàu từ bán khoai lấy xu.
*   🚫 **Không làm nhiều loại cây trồng:** Chỉ sử dụng duy nhất cây Khoai Lang.
*   🚫 **Không làm cơ chế chiến đấu (Combat) hay kẻ thù:** Đây là game sinh tồn chống lại thiên nhiên và gìn giữ cộng đồng.
