# Gameplay Systems Document: Đất Cày Lên Sỏi Đá

Tài liệu này chi tiết hóa các hệ thống gameplay chính phục vụ cho việc lập trình trong Unity cho bản Demo PRU213. Các hệ thống được thiết kế tối giản, dễ cài đặt và tối ưu hóa cho dự án phát triển cá nhân trong 2 tuần.

---

## 1. Player Movement System
* **Mô tả:** Hệ thống điều khiển nhân vật chính Thành di chuyển trong không gian 3D.
* **Cài đặt Unity:**
  * Sử dụng `CharacterController` hoặc `Rigidbody` kết hợp với đầu vào phím `W, A, S, D` để di chuyển.
  * Hỗ trợ phím `Left Shift` để chạy nhanh (tăng tốc độ di chuyển và tiêu hao thể lực theo thời gian).
  * Góc nhìn thứ ba được bổ trợ bởi một camera di chuyển mượt mà bám theo người chơi thông qua script `CameraFollow3D`.
  * Khóa di chuyển (action lock) tạm thời khi thực hiện các hành động nông nghiệp để đảm bảo tính thực tế của hoạt động thể chất.

---

## 2. Interaction System
* **Mô tả:** Cơ chế tương tác với các vật thể trong môi trường bằng phím nóng `[E]`.
* **Cài đặt Unity:**
  * Sử dụng `BoxCollider` 3D (tích chọn `Is Trigger`) trên các đối tượng tương tác như ô đất (`SoilCell`), bàn thờ tổ tiên (`AncestralAltar`), hoặc các NPC (`NPCCharacter`).
  * Khi người chơi bước vào vùng Trigger, hiển thị một nhãn nhắc lệnh (Interaction Prompt) nhỏ trên HUD (ví dụ: *"Nhấn E để Dọn đá"* hoặc *"Nhấn E để nói chuyện"*).
  * Nhấn `[E]` thực thi phương thức hành động tương ứng thông qua Interface `IInteractable` hoặc qua kiểm tra tag đối tượng.

---

## 3. Farming System
* **Mô tả:** Vòng lặp cải tạo ruộng đất cằn cát pha sỏi đá và canh tác giống cây trồng chính (Khoai Lang).
* **Cài đặt Unity:**
  * **Dig (Clear Rocks):** Ô đất (`SoilCell`) khởi đầu với mật độ đá cao (`rockDensity`). Nhấn `[E]` tiêu hao Thể lực để cuốc đất dọn đá dần dần (giảm giá trị `rockDensity` xuống 0).
  * **Water:** Tưới nước tăng độ ẩm đất (`soilMoisture`). Gặp thời tiết nắng nóng Gió Lào, tốc độ bốc hơi ẩm tăng gấp đôi.
  * **Plant:** Gieo hạt giống tiêu thụ hạt giống thực tế (`Item_Seed.asset`) từ hòm đồ.
  * **Growth Stages:** Cây trồng (`CropInstance`) sinh trưởng qua 4 giai đoạn dựa trên thời gian thực tế: `Seedling` (Hạt mầm) -> `Growing` (Phát triển) -> `Ready` (Chín) -> `Withered` (Héo úa nếu thiếu ẩm hoặc ngập úng quá lâu).
  * **Harvest:** Nhấn `[E]` trên cây đã chín để nhận khoai tươi (`Item_FreshCrop.asset`).

---

## 4. Food Resource System
* **Mô tả:** Quản lý kho chứa thực phẩm và cơ chế chế biến sinh tồn.
* **Cài đặt Unity:**
  * Nông sản thu hoạch được chuyển thành **Food** (Khoai tươi).
  * Người chơi có thể đến bếp lò nhà mình để chế biến Khoai tươi thành **Khoai gieo khô** (không bị hỏng/mốc khi độ ẩm không khí tăng cao ở Phase 3).
  * Thực phẩm có hai lựa chọn sử dụng:
    1. *Ăn trực tiếp:* Hồi phục Thể lực cho Thành.
    2. *Đóng góp / Quyên góp:* Gặp các NPC hoặc rương cứu trợ đầu làng để quyên góp khoai khô/tươi giúp bà con chống chịu bão lũ.

---

## 5. Nghĩa Tình System
* **Mô tả:** Hệ thống tính điểm gắn kết cộng đồng và là chỉ số tiến trình chính để quyết định kết cục của trò chơi.
* **Cài đặt Unity:**
  * Điểm Nghĩa Tình (`nghiaTinh`) bắt đầu từ 20. Tăng lên khi người chơi hoàn thành đổi công vần công, thắp nhang đình làng, hoặc đóng góp lương thực cứu đói.
  * Dưới đây là mã nguồn C# tham khảo để tính điểm và quyết định các kết cục (Endings):

```csharp
using UnityEngine;

public class CommunityStats : MonoBehaviour
{
    public int nghiaTinh = 20; // Điểm xuất phát mặc định

    // Thêm điểm Nghĩa Tình
    public void AddNghiaTinh(int amount)
    {
        nghiaTinh = Mathf.Clamp(nghiaTinh + amount, 0, 100);
        Debug.Log($"Nghĩa Tình hiện tại của làng: {nghiaTinh}/100");
    }

    // Đánh giá kết cục game ở cuối Phase 4 dựa trên ngưỡng Nghĩa Tình
    public string EvaluateEnding()
    {
        if (nghiaTinh >= 80)
        {
            return "Best Ending: Đất Cày Nở Hoa (Ngôi làng hồi sinh trù phú, bà con gắn kết bền chặt)";
        }
        else if (nghiaTinh >= 40)
        {
            return "Normal Ending: Lá Lành Đùm Lá Rách (Ngôi làng vượt qua thiên tai nhưng cần thời gian khôi phục)";
        }
        else
        {
            return "Sad Ending: Đất Sỏi Đá Cằn (Mọi người ly hương, Thành cô độc trên ruộng sỏi cát)";
        }
    }
}
```

---

## 6. Community Event System
* **Mô tả:** Trình quản lý sự kiện cốt truyện tối giản kích hoạt tự động theo Phase/Day mà không cần hệ thống Quest phức tạp.
* **Cài đặt Unity:**
  * Hệ thống kiểm tra điều kiện đơn giản (Condition Checks) trong hàm cập nhật ngày của `GameManager`:
    * *Ví dụ:* `if (currentDay == 3 && currentPhase == Phase.Drought) { TriggerLoaPhatThanh("Hôm nay nắng hạn gay gắt, bà con hạn chế ra đồng buổi trưa và chung tay tiết kiệm nước!"); }`
  * Hiển thị thông báo văn bản (Text Toast) trôi qua giữa màn hình và cập nhật nhật ký sự kiện trên HUD.

---

## 7. Weather and Disaster System
* **Mô tả:** Chu kỳ thời tiết khắc nghiệt được nội suy mượt mà để tạo áp lực sinh tồn.
* **Cài đặt Unity:**
  * **Drought / Gió Lào (Phase 2):** Tăng nhiệt độ, tăng tốc độ bốc hơi nước trong đất, tăng nhanh Heat Stress của người chơi khi di chuyển ngoài trời.
  * **Rain (Phase 3 đầu):** Làm ướt đất trồng tự động, tăng nhẹ độ ẩm không khí.
  * **Storm / Flood (Phase 3 giữa):** Mưa lớn kết hợp hệ thống gió thổi chéo hạt mưa bám theo camera. Kích hoạt dâng cao mực nước lũ (`waterLevel`) trong scene. Đất trồng ngập sâu hơn 0.5m sẽ gây úng rễ làm cây trồng thối rữa sau 1 ngày.

---

## 8. NPC Help System
* **Mô tả:** Hệ thống tương trợ hai chiều giữa người chơi và dân làng thông qua các menu thoại đơn giản dạng nút bấm UI.
* **Cài đặt Unity:**
  * Tiếp cận NPC (O Thắm, Bác Năm), nhấn `[E]` để mở bảng tương tác gọn nhẹ gồm 3 lựa chọn:
    1. **Tặng lương thực (Give Food):** Chuyển giao khoai từ hòm đồ sang NPC. Tăng điểm Nghĩa Tình.
    2. **Đổi công (Vần công):** Tiêu hao 20 Thể lực giúp đỡ NPC dọn vườn/làm việc nhà, nhận về 1 điểm vần công tích lũy.
    3. **Nhận hỗ trợ (Share Seeds/Incense):** O Thắm hỗ trợ đổi hạt giống/nhang tổ tiên bằng Xu hỗ trợ. Bác Năm chia sẻ hạt giống khoai phòng thân nếu Thành cạn kiệt nguồn lực.

---

## 9. Phase System
* **Mô tả:** Phân chia game thành 4 giai đoạn thiên tai nối tiếp nhau dựa trên số ngày trôi qua:
* **Phase 1: Tiếng Trống Đình Làng (Ngày 1 - 2):** Thời tiết ôn hòa. Người chơi tập trung cuốc đất dọn đá cải tạo đất cát hoang hóa.
  * **Phase 2: Nắng Cháy Gió Lào (Ngày 3 - 4):** Nắng nóng cực đoan. Người chơi phải căn chỉnh thời gian ra đồng và tưới nước liên tục chống hạn.
  * **Phase 3: Tình Người Trong Mưa Bão (Ngày 5 - 6):** Lũ dâng ngập. Cần di chuyển lương thực lên cao, dùng công Vần công chằng chống nhà cửa tránh sập đổ.
  * **Phase 4: Phù Sa Sau Cơn Lũ (Ngày 7 trở đi):** Nước rút, tận dụng lớp phù sa bồi bồi dưỡng ruộng đất tái thiết đời sống cùng bà con.

---

## 10. Ending System
* **Mô tả:** Khi hết ngày thứ 7 (kết thúc Phase 4), game dừng lại và gọi hàm `EvaluateEnding()` của hệ thống Nghĩa Tình.
* **Cài đặt Unity:**
  * Tắt các hoạt động điều khiển nhân vật.
  * Bật bảng hiển thị kết cục (Ending Canvas Panel) kèm hình ảnh hoặc đoạn mô tả cốt truyện tương ứng với điểm số Nghĩa Tình đạt được, cho phép người chơi quay lại Menu chính hoặc bắt đầu lại hành trình cứu làng.
