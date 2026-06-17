using UnityEngine;
using SownInStone.Core;
using SownInStone.Weather;
using SownInStone.Community;
using SownInStone.Storage;
using SownInStone.Interactions;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SownInStone
{
    /// <summary>
    /// Bảng điều khiển giao diện lập trình nhanh (OnGUI Debug UI) để test game trực quan ngay lập tức.
    /// Không cần vẽ Canvas phức tạp, chỉ cần nhấn Play là bảng điều khiển này tự hiện lên cực kỳ đẹp mắt!
    /// </summary>
    public class FrameworkDebugUI : MonoBehaviour
    {
        [Header("--- TÀI NGUYÊN KIỂM THỬ ---")]
        [SerializeField] private ItemData testFreshCrop;
        [SerializeField] private ItemData testPreservedCrop;
        [SerializeField] private ItemData testIncense;
        [SerializeField] private ItemData testSeedItem;
        [SerializeField] private AncestralAltar testAltar;

        private string alertMessage = "Hệ thống hoạt động bình thường.";
        private float alertTimer = 0f;

        private string activeDialogue = "\"Về quê bám đất thờ cúng tổ tiên là o mừng lắm con, ráng lên Thành nghe con!\"";
        private string activeSpeakerName = "O Thắm";

        [Header("--- THIẾT LẬP HIỂN THỊ ---")]
        [Tooltip("Bảng điều khiển có hiển thị hay không (sẽ được kích hoạt sau khi bấm bắt đầu ở Menu chính).")]
        public bool isUIVisible = false;

        private void Start()
        {
            // Kiểm tra xem có Main Menu đang hiển thị hay không để ẩn bảng điều khiển
#if UNITY_2023_1_OR_NEWER
            FrameworkMainMenuUI mainMenu = FindAnyObjectByType<FrameworkMainMenuUI>();
#else
            FrameworkMainMenuUI mainMenu = FindObjectOfType<FrameworkMainMenuUI>();
#endif
            if (mainMenu != null && mainMenu.IsMenuOpen)
            {
                isUIVisible = false;
            }

            // Đăng ký nhận thông điệp cảnh báo từ Player và Storage để hiện lên bảng thông báo
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnPlayerAlert += ShowAlert;
            }
            if (StorageManager.Instance != null)
            {
                StorageManager.Instance.OnStorageAlert += ShowAlert;
            }

            // Thêm sẵn một số đồ đạc vào kho để người chơi test nghịch thử
            AddDefaultItemsToStorage();
        }

        private void OnDestroy()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnPlayerAlert -= ShowAlert;
            }
            if (StorageManager.Instance != null)
            {
                StorageManager.Instance.OnStorageAlert -= ShowAlert;
            }
        }

        private void Update()
        {
            // Lắng nghe phím tắt F1 để tắt/bật nhanh bảng điều khiển debug
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                isUIVisible = !isUIVisible;
            }
#else
            if (Input.GetKeyDown(KeyCode.F1))
            {
                isUIVisible = !isUIVisible;
            }
#endif

            if (alertTimer > 0f)
            {
                alertTimer -= Time.deltaTime;
                if (alertTimer <= 0f)
                {
                    alertMessage = "Hệ thống hoạt động bình thường.";
                }
            }
        }

        private void ShowAlert(string msg)
        {
            alertMessage = msg;
            alertTimer = 4.5f; // Hiện thông báo trong 4.5 giây
        }

        private void AddDefaultItemsToStorage()
        {
            if (StorageManager.Instance == null) return;

            // Thêm khoai lang tươi, khoai gieo khô, nhang thắp cúng và hạt giống
            if (testFreshCrop != null) StorageManager.Instance.AddItem(testFreshCrop, 15);
            if (testPreservedCrop != null) StorageManager.Instance.AddItem(testPreservedCrop, 3);
            if (testIncense != null) StorageManager.Instance.AddItem(testIncense, 5);
            if (testSeedItem != null) StorageManager.Instance.AddItem(testSeedItem, 5);
        }

        private void OnGUI()
        {
            if (!isUIVisible) return;

            // Thiết lập phong cách hiển thị chung (Sử dụng bảng màu mộc mạc)
            GUI.backgroundColor = new Color(0.18f, 0.15f, 0.12f, 0.95f); // Tông nâu đất mộc mạc
            
            // 1. Khung tiêu đề chính
            Rect headerRect = new Rect(10, 10, 520, 30);
            GUI.Box(headerRect, "<b>BẢNG ĐIỀU KHIỂN SINH TỒN - ĐẤT CÀY LÊN SỎI ĐÁ</b>");

            // 2. Phân hệ THỜI GIAN & GIAI ĐOẠN GAME
            Rect timeRect = new Rect(10, 45, 250, 130);
            GUI.Box(timeRect, "<b>THỜI GIAN & GIAI ĐOẠN</b>");
            GUILayout.BeginArea(new Rect(timeRect.x + 10, timeRect.y + 20, timeRect.width - 20, timeRect.height - 30));
            if (GameManager.Instance != null)
            {
                GUILayout.Label($"Ngày: {GameManager.Instance.CurrentDay}");
                GUILayout.Label($"Giờ: {Mathf.FloorToInt(GameManager.Instance.CurrentHour):00}:{(Mathf.FloorToInt((GameManager.Instance.CurrentHour % 1) * 60)):00}");
                
                string phaseName = GetPhaseVietnameseName(GameManager.Instance.CurrentPhase);
                GUI.color = GetPhaseColor(GameManager.Instance.CurrentPhase);
                GUILayout.Label($"GIAI ĐOẠN: {phaseName}");
                GUI.color = Color.white;
            }
            GUILayout.EndArea();

            // 3. Phân hệ THỜI TIẾT KHẮC NGHIỆT
            Rect weatherRect = new Rect(280, 45, 250, 130);
            GUI.Box(weatherRect, "<b>THỜI TIẾT MIỀN TRUNG</b>");
            GUILayout.BeginArea(new Rect(weatherRect.x + 10, weatherRect.y + 20, weatherRect.width - 20, weatherRect.height - 30));
            if (WeatherManager.Instance != null)
            {
                GUILayout.Label($"Nhiệt độ: {WeatherManager.Instance.Temperature:F1}°C");
                GUILayout.Label($"Độ ẩm: {WeatherManager.Instance.Humidity:F0}%");
                GUILayout.Label($"Sức gió: {WeatherManager.Instance.WindSpeed:F0} km/h");
                if (WeatherManager.Instance.FloodLevel > 0f)
                {
                    GUI.color = Color.cyan;
                    GUILayout.Label($"MỰC NƯỚC LŨ: {WeatherManager.Instance.FloodLevel:F2} mét!");
                    GUI.color = Color.white;
                }
                else
                {
                    GUILayout.Label("Nước lũ: Chưa lụt");
                }
            }
            GUILayout.EndArea();

            // 4. Phân hệ CHỈ SỐ SINH TỒN NHÂN VẬT
            Rect statsRect = new Rect(10, 185, 250, 195);
            GUI.Box(statsRect, "<b>CHỈ SỐ SỨC KHỎE NHÂN VẬT</b>");
            GUILayout.BeginArea(new Rect(statsRect.x + 10, statsRect.y + 20, statsRect.width - 20, statsRect.height - 30));
            if (PlayerStats.Instance != null)
            {
                // Thể hiện dạng thanh tiến trình trực quan
                GUILayout.Label("Sức khỏe (Máu):");
                DrawProgressBar(PlayerStats.Instance.HeatStress > 70f || PlayerStats.Instance.ColdStress > 70f ? Color.red : Color.green);
                
                GUILayout.Label("Thể lực (Stamina):");
                DrawProgressBar(new Color(0.95f, 0.7f, 0f)); // Màu vàng rơm
                
                GUILayout.Label("Tinh thần (Morale):");
                DrawProgressBar(new Color(0.7f, 0.4f, 0.9f)); // Màu tím thiền

                GUILayout.Label($"Stress Nhiệt: {PlayerStats.Instance.HeatStress:F0}% | Lạnh: {PlayerStats.Instance.ColdStress:F0}%");
                GUILayout.Label($"Tài sản: <color=#F4D03F><b>{PlayerStats.Instance.Coins} Xu</b></color>");
            }
            GUILayout.EndArea();

            // 5. PHÂN HỆ TÍCH CỐC PHÒNG CƠ (KHO ĐỒ & CHẾ BIẾN KHOAI GIEO)
            Rect storageRect = new Rect(280, 185, 250, 160);
            GUI.Box(storageRect, "<b>TÍCH CỐC PHÒNG CƠ (KHO)</b>");
            GUILayout.BeginArea(new Rect(storageRect.x + 10, storageRect.y + 20, storageRect.width - 20, storageRect.height - 30));
            if (StorageManager.Instance != null)
            {
                var slots = StorageManager.Instance.GetStorageSlots();
                if (slots.Count == 0)
                {
                    GUILayout.Label("Kho trống trơn!");
                }
                else
                {
                    foreach (var slot in slots)
                    {
                        GUILayout.Label($"- {slot.item.ItemName} x{slot.quantity} ({slot.item.type.ToString()})");
                    }
                }

                // Nút bấm chế tạo chế biến khoai gieo dự trữ chống bão lũ
                if (testFreshCrop != null && testPreservedCrop != null)
                {
                    if (GUILayout.Button("Chế biến Khoai Gieo (Tốn 3 khoai tươi)"))
                    {
                        StorageManager.Instance.CraftPreservedItem(testFreshCrop, testPreservedCrop, 1);
                    }
                }
            }
            GUILayout.EndArea();

            // 6. PHÂN HỆ BAN THỜ TỔ TIÊN & ĐĂNG KÝ HÀNG XÓM
            Rect altarRect = new Rect(10, 355, 520, 80);
            GUI.Box(altarRect, "<b>TÍN NGƯỠNG & TƯƠNG TÁC TÂM LINH VÀN CÔNG</b>");
            GUILayout.BeginArea(new Rect(altarRect.x + 10, altarRect.y + 20, altarRect.width - 20, altarRect.height - 30));
            GUILayout.BeginHorizontal();
            
            if (testAltar != null)
            {
                string altarStatus = testAltar.IsIncenseBurning ? "Nhang đang tỏa khói ấm áp" : "Bàn thờ chưa thắp nhang";
                GUILayout.Label($"Trạng thái: {altarStatus}");
                
                if (GUILayout.Button("Thắp nhang cầu an (Tốn 1 Nhang)"))
                {
                    testAltar.ActionBurnIncense();
                }
            }
            else
            {
                GUILayout.Label("Chưa liên kết Bàn Thờ (AncestralAltar).");
            }

            if (GUILayout.Button("Bác Năm sang Vần Công (+1 công)"))
            {
#if UNITY_2023_1_OR_NEWER
                NPCCharacter[] npcs = FindObjectsByType<NPCCharacter>(FindObjectsInactive.Exclude);
#else
                NPCCharacter[] npcs = FindObjectsOfType<NPCCharacter>();
#endif
                NPCCharacter bacNam = System.Array.Find(npcs, n => n.characterType == NPCCharacter.StoryCharacterType.BacNam);
                if (bacNam != null)
                {
                    bacNam.ModifyVanCongCredits(1);
                    bacNam.ModifyAffection(5);
                    ShowAlert("Bạn vừa sang gặt hộ bác Năm, tích lũy thêm 1 công!");
                }
                else
                {
                    ShowAlert("Không tìm thấy Bác Năm trong scene!");
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // 6.5. PHÂN HỆ HỘI THOẠI CỐT TRUYỆN & TẶNG QUÀ NHÂN VẬT
            Rect dialogueRect = new Rect(10, 440, 520, 115);
            GUI.Box(dialogueRect, "<b>HỘI THOẠI CỐT TRUYỆN & NGHĨA TÌNH LÀNG XÓM</b>");
            GUILayout.BeginArea(new Rect(dialogueRect.x + 10, dialogueRect.y + 20, dialogueRect.width - 20, dialogueRect.height - 30));
            
            // Hiển thị bóng thoại của nhân vật
            GUILayout.Label($"<b>{activeSpeakerName}:</b> <i>{activeDialogue}</i>");
            
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();

#if UNITY_2023_1_OR_NEWER
            NPCCharacter[] currentNPCs = FindObjectsByType<NPCCharacter>(FindObjectsInactive.Exclude);
#else
            NPCCharacter[] currentNPCs = FindObjectsOfType<NPCCharacter>();
#endif
            NPCCharacter oNam = System.Array.Find(currentNPCs, n => n.characterType == NPCCharacter.StoryCharacterType.BacNam);
            NPCCharacter oTham = System.Array.Find(currentNPCs, n => n.characterType == NPCCharacter.StoryCharacterType.OTham);

            if (GUILayout.Button("Hỏi chuyện Bác Năm"))
            {
                if (oNam != null)
                {
                    activeSpeakerName = oNam.NPCName;
                    activeDialogue = oNam.GetDialogue();
                }
                else ShowAlert("Không tìm thấy Bác Năm trong cảnh!");
            }

            if (GUILayout.Button("Hỏi chuyện O Thắm"))
            {
                if (oTham != null)
                {
                    activeSpeakerName = oTham.NPCName;
                    activeDialogue = oTham.GetDialogue();
                }
                else ShowAlert("Không tìm thấy O Thắm trong cảnh!");
            }

            if (GUILayout.Button("Tặng Nhang (Bác Năm)"))
            {
                if (oNam != null)
                {
                    oNam.ActionGiveGift("Incense");
                    activeSpeakerName = oNam.NPCName;
                    activeDialogue = "\"Ôi trời con chu đáo quá, đúng cây nhang bác cần thắp mùng một cầu an! Cảm ơn con nghe!\"";
                }
                else ShowAlert("Không tìm thấy Bác Năm trong cảnh!");
            }

            if (GUILayout.Button("Tặng Khoai Gieo (O Thắm)"))
            {
                if (oTham != null)
                {
                    oTham.ActionGiveGift("Khoai Gieo");
                    activeSpeakerName = oTham.NPCName;
                    activeDialogue = "\"Trời ơi khoai gieo dai dẻo dính răng ăn ngon thiệt chứ! Thành chu đáo quá o thương lắm!\"";
                }
                else ShowAlert("Không tìm thấy O Thắm trong cảnh!");
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // 7. PHÂN HỆ GIẢ LẬP TRÌNH DIỄN THIÊN TAI (DEV CONTROL)
            Rect devRect = new Rect(10, 560, 520, 165);
            GUI.Box(devRect, "<b>BÀN ĐIỀU PHỐI THIÊN TAI (DÀNH CHO ĐỘI NGŨ PHÁT TRIỂN)</b>");
            GUILayout.BeginArea(new Rect(devRect.x + 10, devRect.y + 20, devRect.width - 20, devRect.height - 30));
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Giả lập Mùa 1: Lập Nghiệp"))
            {
                GameManager.Instance?.TransitionToPhase(GamePhase.LapNghiep);
                ShowAlert("Bắt đầu lập nghiệp, khai phá bờ cõi dọn dẹp sỏi đá.");
            }
            if (GUILayout.Button("Giả lập Mùa 2: Gió Lào"))
            {
                GameManager.Instance?.TransitionToPhase(GamePhase.GioLao);
                ShowAlert("Gió Lào tràn về bỏng rát nhiệt độ lên 42°C!");
            }
            if (GUILayout.Button("Giả lập Mùa 3: Bão Lũ"))
            {
                GameManager.Instance?.TransitionToPhase(GamePhase.MuaBao);
                CommunityManager.Instance?.TriggerStormHelpSequence();
                ShowAlert("Bão siêu cấp đổ bộ! Kêu gọi xóm giềng sang đổi công chống bão!");
            }
            if (GUILayout.Button("Giả lập Mùa 4: Phù Sa"))
            {
                GameManager.Instance?.TransitionToPhase(GamePhase.PhuSa);
                ShowAlert("Bão tan nước rút bồi đắp lớp đất phù sa vàng màu mỡ!");
            }

            GUILayout.EndHorizontal();
            
            // Dòng nút phụ để test nhanh chỉ số Stress Nhiệt / Lạnh
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Thêm +35% Stress Nhiệt"))
            {
                PlayerStats.Instance?.ApplyHeatStress(35f);
                ShowAlert("Đã tăng +35% Stress Nhiệt cho Thành!");
            }
            if (GUILayout.Button("Thêm +35% Stress Lạnh"))
            {
                PlayerStats.Instance?.ApplyColdStress(35f);
                ShowAlert("Đã tăng +35% Stress Lạnh cho Thành!");
            }
            if (GUILayout.Button("Reset Toàn Bộ Stress"))
            {
                PlayerStats.Instance?.ApplyHeatStress(-100f);
                PlayerStats.Instance?.ApplyColdStress(-100f);
                ShowAlert("Đã reset chỉ số Stress về 0%!");
            }
            GUILayout.EndHorizontal();

            // Dòng nút kiểm thử Lũ lụt dâng cao / rút nước
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Dâng lũ khẩn cấp (1.6m)"))
            {
                if (WeatherManager.Instance != null)
                {
                    WeatherManager.Instance.DebugSetFloodLevel(1.6f);
                    ShowAlert("Lũ dâng lên 1.6m! Nước ngập lút đồng ruộng!");
                }
            }
            if (GUILayout.Button("Tua nhanh nước rút (0m)"))
            {
                if (WeatherManager.Instance != null)
                {
                    WeatherManager.Instance.DebugSetFloodLevel(0f);
                    ShowAlert("Nước rút hoàn toàn về 0m!");
                }
            }
            GUILayout.EndHorizontal();

            // Dòng nút kiểm thử Tài chính và Vật phẩm
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Tặng +50 Xu"))
            {
                PlayerStats.Instance?.ModifyCoins(50);
                ShowAlert("Đã cộng thêm 50 Xu tài sản!");
            }
            if (GUILayout.Button("Trừ -50 Xu"))
            {
                PlayerStats.Instance?.ModifyCoins(-50);
                ShowAlert("Đã khấu trừ 50 Xu!");
            }
            if (GUILayout.Button("Tặng +5 Khoai Tươi"))
            {
                if (StorageManager.Instance != null && testFreshCrop != null)
                {
                    StorageManager.Instance.AddItem(testFreshCrop, 5);
                    ShowAlert("Đã thêm 5 Khoai lang tươi vào kho đồ!");
                }
            }
            if (GUILayout.Button("Tặng +5 Hạt Giống"))
            {
                if (StorageManager.Instance != null && testSeedItem != null)
                {
                    StorageManager.Instance.AddItem(testSeedItem, 5);
                    ShowAlert("Đã thêm 5 Hạt giống Khoai vào kho đồ!");
                }
            }
            GUILayout.EndHorizontal();

            // Dòng nút kiểm thử nông nghiệp
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Debug: Mature Crops"))
            {
#if UNITY_2023_1_OR_NEWER
                var crops = FindObjectsByType<SownInStone.Agriculture.CropInstance>(FindObjectsInactive.Include);
#else
                var crops = FindObjectsOfType<SownInStone.Agriculture.CropInstance>();
#endif
                foreach (var crop in crops)
                {
                    crop.DebugMature();
                }
                ShowAlert("Đã ép tất cả cây trồng chín ngay lập tức!");
            }
            GUILayout.EndHorizontal();

            // Thanh trạng thái cảnh báo hệ thống màu vàng cam nổi bật
            GUI.color = new Color(1f, 0.6f, 0.1f);
            GUILayout.Label($"THÔNG BÁO: {alertMessage}");
            GUI.color = Color.white;

            GUILayout.EndArea();
        }

        private void DrawProgressBar(Color color)
        {
            // Tạo thanh progress bar đơn giản bằng GUI Layout
            Rect r = GUILayoutUtility.GetRect(180, 14);
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            GUI.DrawTexture(r, Texture2D.whiteTexture); // Vẽ nền đen
            
            GUI.color = color;
            float percent = 1f;
            if (color == Color.green || color == Color.red)
            {
                // Máu
                percent = PlayerStats.Instance != null ? PlayerStats.Instance.GetType().GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(PlayerStats.Instance) as float? ?? 100f : 100f;
                percent /= 100f;
            }
            else if (color == new Color(0.95f, 0.7f, 0f))
            {
                // Stamina
                percent = PlayerStats.Instance != null ? PlayerStats.Instance.GetType().GetField("currentStamina", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(PlayerStats.Instance) as float? ?? 100f : 100f;
                percent /= 100f;
            }
            else
            {
                // Morale
                percent = PlayerStats.Instance != null ? PlayerStats.Instance.GetType().GetField("currentMorale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(PlayerStats.Instance) as float? ?? 100f : 100f;
                percent /= 100f;
            }

            GUI.DrawTexture(new Rect(r.x, r.y, r.width * percent, r.height), Texture2D.whiteTexture); // Vẽ thanh trượt
            GUI.color = Color.white;
        }

        private string GetPhaseVietnameseName(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.LapNghiep: return "GĐ 1 - TIẾNG TRỐNG ĐÌNH LÀNG (Lập Nghiệp)";
                case GamePhase.GioLao: return "GĐ 2 - NẮNG CHÁY GIÓ LÀO (Thử Thách)";
                case GamePhase.MuaBao: return "GĐ 3 - TÌNH NGƯỜI TRONG BÃO LŨ (Sinh Tử)";
                case GamePhase.PhuSa: return "GĐ 4 - PHÙ SA SAU CƠN LŨ (Cái Kết Viên Mãn)";
                default: return phase.ToString();
            }
        }

        private Color GetPhaseColor(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.LapNghiep: return Color.green;
                case GamePhase.GioLao: return new Color(1f, 0.5f, 0f); // Cam cháy
                case GamePhase.MuaBao: return Color.red;
                case GamePhase.PhuSa: return Color.cyan;
                default: return Color.white;
            }
        }
    }
}
