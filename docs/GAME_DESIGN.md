# Game Design Document: Đất Cày Lên Sỏi Đá

Cập nhật: **2026-06-27**. Tài liệu này mô tả **design intent** của demo. Những chỗ chưa có đủ trong source được đánh dấu rõ để tránh nhầm với implementation hiện tại.

---

## 1. High Concept

**Đất Cày Lên Sỏi Đá** là narrative community survival game lấy bối cảnh làng quê miền Trung Việt Nam. Thành trở về quê, cải tạo ruộng sỏi đá, tích trữ lương thực, giúp bà con qua bão lũ và tái thiết làng.

---

## 2. Player Role

Người chơi vào vai **Thành**, một người con xa quê trở về sau biến cố gia đình. Thành không thể sống một mình trên mảnh đất cằn; mục tiêu là bám đất và xây lại tình làng nghĩa xóm.

---

## 3. Intended Core Gameplay Loop

```text
Gặp dân làng
↓
Dọn đá / cải tạo đất / gieo khoai / tưới nước
↓
Thu hoạch khoai tươi và chế biến khoai khô
↓
Tích lũy Nghĩa Tình / Vần công
↓
Thắp nhang tại bàn thờ để kích hoạt khủng hoảng bão lũ
↓
Sơ tán hoặc hỗ trợ dân làng trong mưa bão
↓
Lũ rút, đất Phù Sa cải thiện ruộng
↓
Tái thiết và kết thúc theo điểm Nghĩa Tình
```

---

## 4. Current Implementation Alignment

### Đã align với design

- Farming data model: đất, cây, độ ẩm, dinh dưỡng, sỏi đá, Phù Sa.
- Storage data model: kho, nông sản tươi/khô, decay theo độ ẩm.
- Weather/flood stat model: phase weather, flood level.
- Community model: GlobalKarma, Vần công, NPC affection/dialogue.
- UI model: HUD, dialogue choices, inventory, shop, toast, panels.
- Altar trigger: thắp nhang để chuyển sang bão.

### Chưa align đầy đủ

- Player movement/farming interaction loop chưa thấy trong `PlayerController` hiện tại.
- Countdown sơ tán mới có timer nội bộ và toast, chưa có panel đầy đủ.
- Roof refuge / wall repair / sandbag node gameplay chưa có hệ thống hoàn chỉnh trong source.
- Tutorial slideshow hiện là fallback, chưa phải slideshow ảnh hoàn chỉnh.
- Save/load chưa lưu world state.

---

## 5. Resources & Items

| Resource / Item | Design role | Current source status |
|---|---|---|
| Health | Sức khỏe sinh tồn | Có trong `PlayerStats`. |
| Stamina | Làm nông, giúp việc, tương tác nặng | Có trong `PlayerStats`; caller phải trừ khi action. |
| Morale | Tinh thần trước thiên tai | Có trong `PlayerStats`; altar/cockfighting/cộng đồng có thể chỉnh. |
| Coins | Tài nguyên phụ cho shop | Có trong `PlayerStats` và shop UI. |
| Fresh Crop | Ăn/craft/cứu trợ, dễ thối | Có `ItemData`/storage decay nếu asset đúng. |
| Preserved Crop | Khoai khô dự trữ | Có crafting qua `StorageManager`/`KitchenHearth`. |
| Seed | Trồng khoai | Có item type; cần caller kiểm tra kho khi trồng. |
| Incense | Thắp altar, trigger storm | Có trong `AncestralAltar`. |
| Sandbag/FloodBoard | Chống/sửa lũ | Có item placement/prefab expectation; node gameplay chưa đầy đủ. |
| NonLa | Giảm sốc nhiệt | Có equip flag/prefab instantiate; chưa giảm heat stress. |

---

## 6. NPCs

| NPC | Design role | Current source status |
|---|---|---|
| Bác Năm | Lão nông hướng dẫn, Vần công, chia sẻ lương thực | Có fallback dialogue, affection, Vần công. |
| O Thắm | Shop/nhu yếu phẩm, hỗ trợ cộng đồng | Có fallback dialogue, shop/trade route qua UI. |
| Cụ Bảy | Trưởng thôn/kinh nghiệm dân gian | Có enum/dialogue; scene placement cần Unity verify. |
| Bé Tí | Trẻ nhỏ/checkpoint cảm xúc | Có enum/dialogue; scene placement cần Unity verify. |

---

## 7. Phase Design

### Runtime enum hiện tại

- `LapNghiep`
- `GioLao`
- `ChuanBiBao`
- `MuaBao`
- `PhuSa`
- `EndGame`

### Presentation grouping

- **Before the Storm:** `LapNghiep`, `GioLao`, `ChuanBiBao`
- **Storm / After the Storm:** `MuaBao`, `PhuSa`

### Implementation note

`GameManager` hiện chỉ auto-transition `LapNghiep -> ChuanBiBao` vào ngày 3. `MuaBao` được kích qua `AncestralAltar.TriggerStormCrisis()`. `GioLao` có trong enum/weather/UI nhưng chưa tự lên lịch trong source hiện tại.

---

## 8. Endings

Design vẫn giữ 3 kết cục theo Nghĩa Tình:

- **Best:** làng hồi sinh, Thành được cộng đồng tin tưởng.
- **Normal:** làng qua bão nhưng tình nghĩa chỉ vừa đủ.
- **Sad:** Thành cô độc, cộng đồng suy yếu sau thiên tai.

Source có `EndingManager`, nhưng cần Unity scene validation để đảm bảo panel/references và trigger cuối game hoạt động đúng.

---

## 9. Scope Notes

- Cockfighting minigame không thuộc core design hiện tại; chỉ giữ nếu nhóm quyết định nó là optional cultural side activity.
- Không mở rộng combat, skill tree, nhiều crop, quest graph, economy phức tạp trước khi core farming/weather/community loop chạy chắc.
