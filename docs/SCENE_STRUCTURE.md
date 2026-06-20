# Scene Structure: Đất Cày Lên Sỏi Đá

Tài liệu này hướng dẫn cách tổ chức Hierarchy (Cấu trúc phân cấp đối tượng) trong Unity 6 cho bản chơi thử PRU213. Việc giữ cấu trúc sạch sẽ giúp việc quản lý va chạm, camera và các script quản lý hoạt động ổn định nhất.

---

## 1. Hierarchy Root Layout (Thiết kế đối tượng gốc)

Bản đồ được thiết kế gói gọn trong một Scene chính duy nhất tên là `SampleScene.unity`. Hierarchy trong Unity Editor cần được phân nhóm khoa học theo các đối tượng gốc sau:

```
SampleScene
├── _Managers (Chứa các script quản lý hệ thống, Singleton)
├── _UI (Chứa Canvas HUD và các bảng giao diện UI)
├── Player (Nhân vật chính Thành)
├── Main Camera (Camera góc nhìn thứ ba)
├── Environment (Địa hình và vật trang trí)
├── FarmingArea (Khu vực canh tác ruộng khoai)
├── NPCs (Nhân vật dân làng)
├── InteractionZones (Các khu vực kích hoạt sự kiện)
├── DisasterObjects (Các hệ thống hạt thời tiết và nước lũ)
└── Audio (Nguồn âm thanh môi trường và loa phát thanh)
```

---

## 2. Detailed Sub-Hierarchy & Prefab Naming (Chi tiết phân nhóm)

---

### A. _Managers
Nhóm này chứa GameObject rỗng `_Managers` chứa các component Logic chính:
*   **`GameManager`**: Quản lý vòng lặp ngày/đêm và thời gian trôi.
*   **`PhaseManager`**: Quản lý sự kiện cốt truyện tự động chuyển 4 Phase.
*   **`WeatherManager`**: Điều khiển nắng hạn, mưa gió bão cát và nước lũ.
*   **`CommunityManager`**: Lưu trữ điểm Nghĩa Tình và điểm đổi công Vần công.
*   **`InventoryManager`**: Quản lý túi đồ hạt giống và nông sản khô/tươi.
*   **`UIManager`**: Phân phối hiển thị bảng thông báo khẩn cấp và kết cục game.
*   **`AudioManager`**: Quản lý hiệu ứng âm thanh và nhạc nền.

---

### B. Environment
Chứa toàn bộ mô hình tĩnh (Static) làm cảnh quan nền:
*   `Ground`: Plane mặt đất cát trắng miền Trung và đất cát sỏi đá pha cát.
*   `VillagePath`: Đường mòn chạy xuyên làng kết nối các nhà dân.
*   `House_Thanh`: Prefab `HoiAnHouse_M2` làm nhà chính của Thành.
*   `House_OTham`: Nhà cấp bốn của O Thắm.
*   `House_BacNam`: Nhà tranh vách đất của Bác Năm.
*   `Village_Well`: Giếng đá cổ trung tâm làng.
*   `Village_Speaker`: Cột loa phát thanh xã.
*   `Ancestral_Altar`: Miếu thờ nhỏ đầu làng có cây cổ thụ phủ bóng.
*   `Bamboo_Fences`: Chuỗi ghép rào tre bao quanh vườn nhà Thành.
*   `Props`: Thùng gỗ, lu nước, thúng khoai trang trí.

---

### C. FarmingArea
Quản lý các ô đất nông nghiệp và cây trồng động:
*   `SoilCells`: Chứa 4 ô đất `SoilCell_1` đến `SoilCell_4` xếp dạng lưới.
*   `Rocks`: Các tảng đá cằn nằm đè lên SoilCell khi chưa được Thành dọn sạch.
*   `Crops`: Container rỗng để GameManager instantiate prefab cây khoai lang động vào lúc runtime.
*   `SiltSoilVisuals`: Các texture xám sẫm dùng để vẽ đè lên mặt đất khi lũ rút.

---

### D. NPCs
*   `NPC_OTham`: NPC O Thắm tương hỗ (kèm trigger BoxCollider tương tác đổi công/hạt).
*   `NPC_BacNam`: NPC Bác Năm neo đơn (kèm trigger BoxCollider đóng góp khoai).
*   `NPC_CuBay` (Optional): Trưởng thôn đi tuần tra đình làng.
*   `NPC_BeTi` (Optional): Bé Tí chạy loanh quanh sân nhà.

---

### E. DisasterObjects
Các đối tượng tương tác thời tiết và thiên tai:
*   `RainParticleSystem`: Hệ thống hạt mưa bão bám sát theo vị trí Camera.
*   `FloodWaterPlane`: Tấm phẳng 3D phủ nước lũ ngập, nâng hạ trục Y động.
*   `WindEffects`: Lực gió đẩy các hạt mưa chéo góc.
*   `RepairTargets`: Các vị trí chằng sandbag quanh mái nhà được bật lên khi người chơi đổi công Vần công chống bão.
*   `FoodDeliveryTargets`: Rương quyên góp cứu đói đặt ở đầu làng để nhận khoai.

---

### F. _UI
Canvas chứa toàn bộ HUD và UI Popups:
*   `HUD`: Hiển thị thanh máu, thể lực, thanh chỉ số Nghĩa Tình, số lượng Xu hỗ trợ và đồng hồ ngày.
*   `InteractionPrompt`: Nhãn text nhắc lệnh nhỏ giữa màn hình (ví dụ: *"Nhấn E để thắp nhang"*).
*   `InventoryPanel`: Bảng lưới túi đồ (`Tab`/`I` keys).
*   `NghiaTinhPanel`: Giao diện chi tiết điểm Nghĩa Tình và số lượng công đổi công tích lũy.
*   `PhaseBanner`: Hiển thị Banner điện ảnh lướt nhẹ khi đổi ngày mới và cảnh báo thời tiết Loa phát thanh.
*   `EndingScreen`: Màn hình kết game xuất hiện ở cuối Phase 4.
*   `DebugPanel`: Bảng test nhanh F1 dành cho nhà phát triển.

---

## 3. Naming Conventions (Quy chuẩn đặt tên trong Editor)
*   **Root Objects:** Đặt tên dạng danh từ, viết hoa chữ cái đầu (ví dụ: `Player`, `Environment`).
*   **Prefabs:** Đặt tên bắt đầu bằng danh mục viết thường (ví dụ: `item_seed_potato`, `crop_sweet_potato`, `building_hoian_house`).
*   **Scripts:** Đặt tên theo PascalCase (ví dụ: `PlayerController.cs`, `WeatherManager.cs`).
*   **Interactive Targets:** Thêm tiếp vị ngữ để dễ nhận biết (ví dụ: `NPC_OTham`, `Zone_AncestralAltar`).
*   **Static Mesh Colliders:** Đối với nhà cửa và giếng đá tĩnh, đánh dấu **Static** ở Inspector để tối ưu hóa va chạm vật lý và baking ánh sáng.
