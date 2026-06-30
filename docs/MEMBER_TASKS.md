# HƯỚNG DẪN CHI TIẾT TỪNG NHIỆM VỤ CHO AI AGENTS (MEMBER BACKLOGS)

> [!NOTE]
> **Cập nhật ngày 2026-06-29:** Toàn bộ khung logic cốt lõi cho các Thành viên dưới đây đã được lập trình và tích hợp thành công vào nhánh chính. Các Thành viên có thể tiến hành chạy thử nghiệm Play Mode trong Unity để kiểm tra trực quan và thực hiện các phần việc tinh chỉnh (polish) chi tiết liên quan đến prefab, asset mỹ thuật, UI Canvas và âm thanh môi trường.

Tài liệu này cung cấp các chỉ dẫn kỹ thuật cực kỳ chi tiết, bao gồm: đường dẫn file, tên lớp (Class), tên phương thức (Method), logic nghiệp vụ và mã nguồn mẫu (Code template) để các AI Agent của từng thành viên có thể đọc hiểu và lập trình hoàn thành nhiệm vụ ngay lập tức.

---

## 👤 THÀNH VIÊN 1: Lập trình Logic NPC & Hệ thống Nghĩa Tình (Gameplay Developer)

### 📌 Mục tiêu:
Tích hợp tính năng quyên góp lương thực/khoai lang (O Thắm) và vần công giúp sửa nhà (Bác Năm) để tích lũy điểm Karma.

### 📂 File cần chỉnh sửa:
1. [NPCProximityOptionsUI.cs](../Assets/Scripts/UI/NPCProximityOptionsUI.cs)
2. [CommunityManager.cs](../Assets/Scripts/Community/CommunityManager.cs)

---

### 🛠 Chỉ dẫn kỹ thuật & Code mẫu:

#### 1. Thêm Tùy chọn Tương tác vào `ConfigureOptionsForNPC` trong `NPCProximityOptionsUI.cs`
AI Agent cần tìm đến phương thức `ConfigureOptionsForNPC(NPCCharacter npc)` và chèn thêm các tùy chọn sau:

* **Đối với O Thắm (`npc.characterType == NPCCharacter.StoryCharacterType.OTham`):**
  ```csharp
  // Thêm nút quyên góp khoai lang vào bảng lựa chọn
  currentOptions.Add(new ProximityOption 
  { 
      label = "[Đóng góp] Quyên góp Khoai Lang tươi (2 củ -> +5 Nghĩa Tình)", 
      action = () => TriggerOThamDonation(npc) 
  });
  ```

* **Đối với Bác Năm (`npc.characterType == NPCCharacter.StoryCharacterType.BacNam`):**
  ```csharp
  // Thêm nút vần công vào bảng lựa chọn
  currentOptions.Add(new ProximityOption 
  { 
      label = "[Vần công] Gia cố nhà cửa (-20 Thể lực -> +10 Nghĩa Tình)", 
      action = () => TriggerBacNamVanCong(npc) 
  });
  ```

#### 2. Định nghĩa các hàm Callback trong `NPCProximityOptionsUI.cs`
Thêm mã nguồn xử lý logic quyên góp và vần công:

```csharp
private void TriggerOThamDonation(NPCCharacter npc)
{
    if (StorageManager.Instance == null || PlayerStats.Instance == null) return;

    // Tìm ItemData Khoai Lang Tươi bằng ID hoặc load từ Database
    ItemData freshCrop = StorageManager.Instance.GetItemDataByID("Item_FreshCrop");
    if (freshCrop == null)
    {
        freshCrop = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_FreshCrop.asset");
    }

    // Kiểm tra số lượng khoai lang tươi trong túi đồ
    var slots = StorageManager.Instance.GetStorageSlots();
    var slot = slots.Find(s => s.item != null && s.item.ItemID == freshCrop.ItemID);
    int quantity = slot != null ? slot.quantity : 0;

    if (quantity >= 2)
    {
        if (StorageManager.Instance.RemoveItem(freshCrop, 2))
        {
            CommunityManager.Instance.AddKarma(5);
            SurvivalUIManager.Instance?.ShowHUDToast("<color=#2ECC71>Đóng góp thành công! Trừ 2 Khoai tươi. Nhận +5 Nghĩa Tình.</color>");
            ConfigureOptionsForNPC(npc); // Cập nhật lại UI
        }
    }
    else
    {
        SurvivalUIManager.Instance?.ShowHUDToast("<color=#E74C3C>Bạn không có đủ 2 củ Khoai Lang Tươi để đóng góp!</color>");
    }
}

private void TriggerBacNamVanCong(NPCCharacter npc)
{
    if (PlayerStats.Instance == null || CommunityManager.Instance == null) return;

    // Kiểm tra thể lực hiện tại của người chơi
    if (PlayerStats.Instance.CurrentStamina >= 20f)
    {
        PlayerStats.Instance.ModifyStamina(-20f);
        CommunityManager.Instance.AddKarma(10);
        
        // Phát tiếng búa gõ đập sửa nhà
        SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_wood_hit"); 
        
        SurvivalUIManager.Instance?.ShowHUDToast("<color=#2ECC71>Đã giúp Bác Năm gia cố vách nhà! Trừ 20 Thể lực, +10 Nghĩa Tình.</color>");
        ConfigureOptionsForNPC(npc); // Cập nhật lại UI
    }
    else
    {
        SurvivalUIManager.Instance?.ShowHUDToast("<color=#E74C3C>Thể lực của bạn quá thấp để làm việc nặng (Cần tối thiểu 20)!</color>");
    }
}
```

---

## 👤 THÀNH VIÊN 2: Hệ thống Dự phòng Thiên tai & Gia cố (Weather & Event Developer)

### 📌 Mục tiêu:
Phát triển tính năng mua bao cát/vách gỗ từ cửa hàng O Thắm, sau đó mang đặt xung quanh vườn để chắn lũ, giảm ngập úng.

### 📂 File cần chỉnh sửa / Tạo mới:
1. [PlayerController.cs](../Assets/Scripts/Core/PlayerController.cs) (Bắt phím tắt đặt vật phẩm chắn lũ).
2. [FloodBarrier.cs](../Assets/Scripts/Interactions/FloodBarrier.cs) (Logic giảm thiểu lượng nước ngập cho ô đất lân cận).

---

### 🛠 Chỉ dẫn kỹ thuật & Code mẫu:

#### 1. Định nghĩa Phím đặt vật phẩm trong `PlayerController.cs`
Người chơi sẽ nhấn phím **[4]** để đặt vách ván chắn nước (`Item_flood_board`) hoặc phím **[5]** để đặt bao cát (`Item_sandbag`).

Thêm mã nguồn vào phương thức `Update()` của `PlayerController.cs`:

```csharp
private void Update()
{
    // ... logic hiện tại của Update ...

    // Đặt ván chắn nước (Phím 4)
    if (Input.GetKeyDown(KeyCode.Alpha4))
    {
        TryPlaceBarrier("Item_flood_board", "Prefabs/FloodBoard");
    }

    // Đặt bao cát (Phím 5)
    if (Input.GetKeyDown(KeyCode.Alpha5))
    {
        TryPlaceBarrier("Item_sandbag", "Prefabs/Sandbag");
    }
}

private void TryPlaceBarrier(string itemID, string prefabResourcePath)
{
    if (StorageManager.Instance == null) return;

    ItemData barrierItem = StorageManager.Instance.GetItemDataByID(itemID);
    if (barrierItem == null)
    {
        barrierItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>($"Assets/Data/{itemID}.asset");
    }

    // Kiểm tra túi đồ xem có vật phẩm này không
    var slots = StorageManager.Instance.GetStorageSlots();
    var slot = slots.Find(s => s.item != null && s.item.ItemID == itemID);
    int qty = slot != null ? slot.quantity : 0;

    if (qty > 0)
    {
        if (StorageManager.Instance.RemoveItem(barrierItem, 1))
        {
            // Tải prefab từ Resources
            GameObject prefab = Resources.Load<GameObject>(prefabResourcePath);
            if (prefab != null)
            {
                // Đặt lệch về phía trước hướng nhân vật đang đứng
                Vector3 spawnPos = transform.position + transform.forward * 1.0f;
                spawnPos.y = 0.5f; // Đảm bảo đặt sát mặt đất

                GameObject barrierInstance = Instantiate(prefab, spawnPos, transform.rotation);
                barrierInstance.name = $"{itemID}_Placed";

                // Đảm bảo instance có component FloodBarrier
                FloodBarrier barrierComponent = barrierInstance.GetComponent<FloodBarrier>();
                if (barrierComponent == null)
                {
                    barrierComponent = barrierInstance.AddComponent<FloodBarrier>();
                }
                
                // Cài đặt bán kính chắn lũ
                barrierComponent.protectionRadius = 3.0f;

                SurvivalUIManager.Instance?.ShowHUDToast($"✓ Đã đặt vách chắn lũ thành công!");
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_place_object");
            }
            else
            {
                Debug.LogError($"[BARRIER] Không tìm thấy Prefab tại Resources/{prefabResourcePath}");
            }
        }
    }
    else
    {
        SurvivalUIManager.Instance?.ShowHUDToast($"<color=#E74C3C>Bạn không có sẵn vật phẩm này trong kho đồ để đặt!</color>");
    }
}
```

#### 2. Đồng bộ logic bảo vệ trong `SoilCell.cs` hoặc `FloodBarrier.cs`
Phương thức cập nhật lượng nước của `SoilCell.cs` trong mùa mưa lũ cần kiểm tra nếu có bất kỳ `FloodBarrier` nào đang ở gần thì giảm tốc độ thấm nước lũ:

```csharp
// Trong SoilCell.cs khi tính toán tích lũy nước lụt:
float floodPenetrationRate = 1.0f;
FloodBarrier[] barriers = FindObjectsByType<FloodBarrier>(FindObjectsSortMode.None);
foreach (var barrier in barriers)
{
    float dist = Vector3.Distance(transform.position, barrier.transform.position);
    if (dist <= barrier.protectionRadius)
    {
        // Vách bảo vệ chặn bớt 80% độ ngập úng
        floodPenetrationRate = 0.2f; 
        break;
    }
}
// Nhân tỷ lệ này vào tốc độ tăng ẩm/ẩm úng của ô ruộng.
```

---

## 👤 THÀNH VIÊN 3: Giao diện Màn hình Kết thúc Game & UI/UX (UI Developer)

### 📌 Mục tiêu:
Dựng panel hiển thị kết thúc game (`EndingPanel`) và cập nhật `EndingManager.cs` để phân tích hiển thị 3 loại kết cục dựa trên tổng điểm Karma.

### 📂 File cần chỉnh sửa / Tạo mới:
1. [EndingManager.cs](../Assets/Scripts/UI/EndingManager.cs)
2. Cấu trúc Hierarchy của UI Canvas trong Unity Editor.

---

### 🛠 Chỉ dẫn kỹ thuật & Code mẫu:

#### 1. Cài đặt Cấu trúc Canvas Hierarchy trong Unity:
Tạo cấu trúc UI sau dưới UI Canvas:
```text
Canvas (UI)
└── EndingPanel (GameObject, Image nền che phủ toàn màn hình, màu đen mờ)
    ├── EndingTitleText (TextMeshPro - Text) -> Tiêu đề (Đại Cát / Bình An / Tiêu Điều)
    ├── EndingDescriptionText (TextMeshPro - Text) -> Đoạn mô tả kết cục chi tiết
    ├── ButtonContainer (Horizontal Layout Group)
    │   ├── RestartButton (Button) -> Nút Chơi Lại
    │   └── ExitButton (Button) -> Nút Thoát Game
```

#### 2. Lập trình mã nguồn cho `EndingManager.cs`
Thay thế hoặc cập nhật phương thức `ShowEnding()` trong `EndingManager.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using SownInStone.Community;

namespace SownInStone.UI
{
    public class EndingManager : MonoBehaviour
    {
        public static EndingManager Instance { get; private set; }

        [Header("--- UI REFERENCES ---")]
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private TextMeshProUGUI endingTitleText;
        [SerializeField] private TextMeshProUGUI endingDescriptionText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (endingPanel != null) endingPanel.SetActive(false);

            // Gán sự kiện cho các nút bấm
            if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
            if (exitButton != null) exitButton.onClick.AddListener(ExitGame);
        }

        public void ShowEnding()
        {
            if (endingPanel == null) return;

            endingPanel.SetActive(true);
            Time.timeScale = 0f; // Dừng game

            int finalKarma = CommunityManager.Instance != null ? CommunityManager.Instance.GlobalKarma : 0;

            if (finalKarma >= 40)
            {
                // Best Ending
                endingTitleText.text = "<color=#2ECC71>KẾT CỤC: ĐẤT NỞ HOA (ĐẠI CÁT)</color>";
                endingDescriptionText.text = $"Với điểm Nghĩa Tình là {finalKarma}, bạn đã đồng lòng cùng cả làng Bác Năm, O Thắm sơ tán an toàn và cứu sống ruộng vườn. Sau lũ, phù sa bồi đắp giúp khoai lang tươi tốt trúng mùa lớn. Làng quê nghèo miền Trung cùng nhau vực dậy tái thiết cuộc sống ấm no.";
            }
            else if (finalKarma >= 15)
            {
                // Normal Ending
                endingTitleText.text = "<color=#F1C40F>KẾT CỤC: LÁ LÀNH ĐÙM LÁ RÁCH (BÌNH AN)</color>";
                endingDescriptionText.text = $"Với điểm Nghĩa Tình là {finalKarma}, tuy ruộng khoai bị thiệt hại nhẹ sau bão và cuộc sống tái thiết còn nhiều chông gai, nhưng nhờ sự đoàn kết giúp đỡ lẫn nhau của bà con, mọi người vẫn vượt qua thiên tai bình an vô sự.";
            }
            else
            {
                // Bad Ending
                endingTitleText.text = "<color=#E74C3C>KẾT CỤC: TIÊU ĐIỀU LY TÁN (TIÊU ĐIỀU)</color>";
                endingDescriptionText.text = $"Với điểm Nghĩa Tình thấp ({finalKarma}), bạn ích kỷ không giúp đỡ cộng đồng và không dự trữ đủ lương thực phòng lũ. Cơn bão quét qua cuốn trôi nhà cửa, bà con phải ly tán lên thành phố kiếm sống trong sự nghèo đói.";
            }
        }

        private void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ExitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
```

---

## 👤 THÀNH VIÊN 4: Âm thanh & Hiệu ứng chuyển Phase (Audio & Polish Developer)

### 📌 Mục tiêu:
Cải thiện không khí bão lũ bằng sấm chớp giật liên hồi (Light Flash) và âm thanh báo động loa phường khi chuyển sang phase mưa bão.

### 📂 File cần chỉnh sửa / Tạo mới:
1. [WeatherManager.cs](../Assets/Scripts/Weather/WeatherManager.cs)
2. [GameManager.cs](../Assets/Scripts/Core/GameManager.cs)

---

### 🛠 Chỉ dẫn kỹ thuật & Code mẫu:

#### 1. Kích hoạt Loa phát thanh phường & Tiếng sấm trong `GameManager.cs`
Khi chuyển đổi phase sang `GamePhase.MuaBao`, phát tín hiệu âm thanh cảnh báo bão khẩn cấp:

```csharp
// Trong GameManager.cs phương thức TransitionToPhase(GamePhase newPhase)
if (newPhase == GamePhase.MuaBao)
{
    // Bật SFX Loa phường cảnh báo thiên tai khẩn cấp
    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_emergency_alarm");
    // Chạy đè nhạc nền mưa bão
    SownInStone.Audio.AudioManager.Instance?.PlayMusic("bgm_heavy_storm");
}
```

#### 2. Lập trình hiệu ứng Chớp sáng sấm sét trong `WeatherManager.cs`
Viết một Coroutine trong `WeatherManager.cs` để nháy sáng Directional Light môi trường ngẫu nhiên trong suốt giai đoạn bão lụt:

```csharp
[Header("--- LIGHTNING POLISH ---")]
[SerializeField] private Light directionalLight; // Đèn chiếu sáng mặt trời trong Scene
private Coroutine lightningCoroutine;

private void Start()
{
    GameManager.OnPhaseChanged += OnPhaseChangedEvent;
}

private void OnPhaseChangedEvent(GamePhase phase)
{
    if (phase == GamePhase.MuaBao)
    {
        if (lightningCoroutine == null)
        {
            lightningCoroutine = StartCoroutine(LightningStrikeRoutine());
        }
    }
    else
    {
        if (lightningCoroutine != null)
        {
            StopCoroutine(lightningCoroutine);
            lightningCoroutine = null;
        }
    }
}

private System.Collections.IEnumerator LightningStrikeRoutine()
{
    if (directionalLight == null)
    {
        directionalLight = RenderSettings.sun;
    }

    float originalIntensity = directionalLight != null ? directionalLight.intensity : 1.0f;

    while (true)
    {
        // Chờ từ 5 đến 12 giây cho mỗi đợt chớp sấm
        yield return new WaitForSeconds(Random.Range(5f, 12f));

        if (directionalLight != null)
        {
            // Phát âm thanh sấm sét nổ đùng đoàng
            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_thunder");

            // Nháy sáng chớp giật 2 lần nhanh
            for (int i = 0; i < 2; i++)
            {
                directionalLight.intensity = originalIntensity * 3.5f; // Chớp sáng cường độ mạnh
                yield return new WaitForSeconds(0.08f);
                directionalLight.intensity = originalIntensity * 0.2f; // Tối sầm lại
                yield return new WaitForSeconds(0.05f);
            }

            // Trả lại độ sáng ban đầu
            directionalLight.intensity = originalIntensity;
        }
    }
}
```
