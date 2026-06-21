using UnityEngine;
using UnityEngine.InputSystem;
using SownInStone.Core;
using SownInStone.Community;

namespace SownInStone
{
    /// <summary>
    /// Giao diện Menu Bắt đầu (Main Menu UI) nghệ thuật mộc mạc Việt Nam vẽ bằng OnGUI.
    /// Hoạt động như một lớp phủ điện ảnh (Cinematic Overlay) ngay khi vào game:
    /// Tự động dừng thời gian, hiển thị menu gỗ tuyệt đẹp, nhạc nền gợi ý và mở ra hướng dẫn luật chơi "Ký Ức Miền Trung".
    /// </summary>
    public class FrameworkMainMenuUI : MonoBehaviour
    {
        public static FrameworkMainMenuUI Instance { get; private set; }

        [Header("--- THIẾT LẬP MENU ---")]
        [Tooltip("Có hiển thị Menu ngay khi khởi động game không.")]
        [SerializeField] private bool showMenuOnStart = true;

        [Tooltip("Âm thanh gió thổi rì rào mùa hè (nếu có AudioSource).")]
        [SerializeField] private AudioSource ambientWindAudio;

        // Trạng thái hiển thị menu
        private bool isMenuOpen = true;
        public bool IsMenuOpen => isMenuOpen;
        private bool hasStartedJourney = false;
        private enum MenuTab
        {
            Main,
            KyUcMienTrung, // Hướng dẫn chơi & Luật cốt truyện
            DoiNgu,        // Đội ngũ sản xuất
            Settings,      // Cài đặt phím bấm & cẩm nang
            WarningDialog, // Cảnh báo trước khi chơi
        }
        private MenuTab currentTab = MenuTab.Main;
        private string rebindAction = "";

        // Hiệu ứng hạt bụi rơm vàng bay lãng mạn trên Menu
        private Vector2[] strawParticles;
        private int particleCount = 20;
        
        private Texture2D bgImage;
        private Texture2D transparentTex;
        
        // Hover state scales for the 6 menu buttons
        private float[] menuHoverScales = new float[6] { 1f, 1f, 1f, 1f, 1f, 1f };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            isMenuOpen = showMenuOnStart;

            if (isMenuOpen)
            {
                // Dừng thời gian game để người chơi trải nghiệm Menu điện ảnh
                Time.timeScale = 0f;
                if (ambientWindAudio != null) ambientWindAudio.Play();
                SownInStone.Audio.AudioManager.Instance?.PlayMusic("bgm_menu");
                SownInStone.Audio.AudioManager.Instance?.StopAmbient();

                // Đảm bảo ẩn bảng điều khiển sinh tồn khi menu đang mở
#if UNITY_2023_1_OR_NEWER
                FrameworkDebugUI debugUI = FindAnyObjectByType<FrameworkDebugUI>();
#else
                FrameworkDebugUI debugUI = FindObjectOfType<FrameworkDebugUI>();
#endif
                if (debugUI != null)
                {
                    debugUI.isUIVisible = false;
                }
            }

            // Khởi tạo các hạt bụi rơm bay
            strawParticles = new Vector2[particleCount];
            for (int i = 0; i < particleCount; i++)
            {
                strawParticles[i] = new Vector2(Random.Range(0, Screen.width), Random.Range(0, Screen.height));
            }

            // Load ảnh nền mới
            bgImage = Resources.Load<Texture2D>("UI/bg_home_2");

            // Tạo texture trong suốt để bắt sự kiện hover chuột
            transparentTex = new Texture2D(1, 1);
            transparentTex.SetPixel(0, 0, Color.clear);
            transparentTex.Apply();
        }

        private void Update()
        {
            // Xử lý bật/tắt Menu khi nhấn ESC
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && hasStartedJourney)
            {
                if (isMenuOpen)
                {
                    // Đang ở trong Menu (tạm dừng) -> Nếu không phải đang gán phím thì tiếp tục game
                    if (string.IsNullOrEmpty(rebindAction))
                    {
                        StartJourney();
                    }
                }
                else
                {
                    // Đang chơi game -> Mở Menu (tạm dừng)
                    PauseGame();
                }
            }

            if (isMenuOpen)
            {
                // Mô phỏng bụi rơm bay chậm rãi trong gió Lào ở màn hình Menu
                for (int i = 0; i < particleCount; i++)
                {
                    strawParticles[i].x -= 40f * Time.unscaledDeltaTime; // Bay từ phải qua trái
                    strawParticles[i].y += Mathf.Sin(Time.unscaledTime + i) * 15f * Time.unscaledDeltaTime;

                    if (strawParticles[i].x < -20)
                    {
                        strawParticles[i].x = Screen.width + 20;
                        strawParticles[i].y = Random.Range(0, Screen.height);
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!isMenuOpen) return;

            // VẼ ẢNH NỀN HOME
            if (bgImage != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bgImage, ScaleMode.ScaleAndCrop);
            }

            // 1. VẼ HẠT BỤI RƠM VÀNG LÃNG MẠN (Xóa nền đen để thấy cảnh phía sau)
            GUI.color = new Color(0.95f, 0.75f, 0.15f, 0.35f); 
            foreach (var particle in strawParticles)
            {
                GUI.DrawTexture(new Rect(particle.x, particle.y, 4, 4), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            // Tọa độ menu nằm bên trái màn hình
            float menuWidth = 500f;
            float menuX = 50f; // Căn lề trái

            // 2. TIÊU ĐỀ GAME (Reduced size by 25%: 72 -> 54)
            Rect titleRect = new Rect(menuX, 60f, menuWidth, 120f);
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleLeft;
            titleStyle.fontSize = 54;
            titleStyle.fontStyle = FontStyle.Bold;
            DrawOutlinedLabel(titleRect, "ĐẤT CÀY\nLÊN SỎI ĐÁ", titleStyle, new Color(0.96f, 0.85f, 0.6f), Color.black, 2.5f);

            // Subtitle
            Rect subtitleRect = new Rect(menuX, 185f, menuWidth, 30f);
            GUIStyle subtitleStyle = new GUIStyle();
            subtitleStyle.alignment = TextAnchor.MiddleLeft;
            subtitleStyle.fontSize = 14;
            subtitleStyle.fontStyle = FontStyle.Italic;
            DrawOutlinedLabel(subtitleRect, "Một hành trình sinh tồn và nghĩa tình nơi miền Trung Việt Nam", subtitleStyle, new Color(0.85f, 0.85f, 0.85f, 0.95f), Color.black, 1.5f);

            // Save summary
            DrawSaveSummary();

            // Bắt đầu vẽ nội dung các Tab
            float contentStartY = Screen.height - 400f;
            if (contentStartY < 250f) contentStartY = 250f;
            
            GUILayout.BeginArea(new Rect(menuX, contentStartY, menuWidth, 350f));

            switch (currentTab)
            {
                case MenuTab.Main:
                    DrawMainMenu();
                    break;
                case MenuTab.KyUcMienTrung:
                    DrawKyUcMienTrung();
                    break;
                case MenuTab.DoiNgu:
                    DrawDoiNgu();
                    break;
                case MenuTab.Settings:
                    DrawSettings();
                    break;
            }

            GUILayout.EndArea();
            
            // Version Text ở góc dưới cùng bên trái
            GUIStyle versionStyle = new GUIStyle();
            versionStyle.normal.textColor = new Color(1f, 1f, 1f, 0.8f);
            versionStyle.fontSize = 12;
            GUI.Label(new Rect(10f, Screen.height - 25f, 200f, 20f), "Version 1.0 (Alpha)", versionStyle);

            // Vẽ bảng cảnh báo đè lên trên cùng nếu đang mở
            if (currentTab == MenuTab.WarningDialog)
            {
                DrawWarningDialog();
            }
        }

        #region VẼ CÁC TRANG MENU CHÍNH
        
        private void DrawMainMenu()
        {
            GUIStyle clearBtnStyle = new GUIStyle();
            clearBtnStyle.normal.background = transparentTex;
            clearBtnStyle.fontSize = 28;
            clearBtnStyle.fontStyle = FontStyle.Bold;
            clearBtnStyle.alignment = TextAnchor.MiddleLeft;
            
            GUILayout.Space(5);
            if (hasStartedJourney)
            {
                if (DrawMenuButton(0, "Continue Game", clearBtnStyle))
                {
                    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                    StartJourney();
                }
                GUILayout.Space(5);
            }
            
            if (DrawMenuButton(1, "New Game", clearBtnStyle))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                if (hasStartedJourney)
                {
                    hasStartedJourney = false;
                }
                currentTab = MenuTab.WarningDialog;
            }
            GUILayout.Space(5);
            
            if (DrawMenuButton(2, "How To Play", clearBtnStyle))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                currentTab = MenuTab.KyUcMienTrung;
            }
            GUILayout.Space(5);
            
            if (DrawMenuButton(3, "Settings", clearBtnStyle))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                currentTab = MenuTab.Settings;
            }
            GUILayout.Space(5);
            
            if (DrawMenuButton(4, "Credits", clearBtnStyle))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                currentTab = MenuTab.DoiNgu;
            }
            GUILayout.Space(5);
            
            if (DrawMenuButton(5, "Exit", clearBtnStyle))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }

        private void DrawWarningDialog()
        {
            // Làm tối nền phía sau (Overlay mờ)
            GUI.color = new Color(0, 0, 0, 0.8f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Kích thước bảng cảnh báo
            float dialogWidth = 650f;
            float dialogHeight = 350f;
            float dialogX = (Screen.width - dialogWidth) / 2f;
            float dialogY = (Screen.height - dialogHeight) / 2f;

            // Vẽ nền đen xám cho bảng
            GUI.color = new Color(0.12f, 0.12f, 0.12f, 0.98f);
            GUI.DrawTexture(new Rect(dialogX, dialogY, dialogWidth, dialogHeight), Texture2D.whiteTexture);
            
            // Vẽ viền xám nhạt
            GUI.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            GUI.DrawTexture(new Rect(dialogX, dialogY, dialogWidth, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(dialogX, dialogY + dialogHeight - 2, dialogWidth, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(dialogX, dialogY, 2, dialogHeight), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(dialogX + dialogWidth - 2, dialogY, 2, dialogHeight), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(dialogX + 30, dialogY + 30, dialogWidth - 60, dialogHeight - 60));

            GUIStyle warningTitleStyle = new GUIStyle();
            warningTitleStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);
            warningTitleStyle.fontSize = 24;
            warningTitleStyle.fontStyle = FontStyle.Bold;
            warningTitleStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle warningBodyStyle = new GUIStyle();
            warningBodyStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            warningBodyStyle.fontSize = 15;
            warningBodyStyle.wordWrap = true;
            warningBodyStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Label("CẢNH BÁO NỘI DUNG", warningTitleStyle);
            GUILayout.Space(20);
            GUILayout.Label("- Game có chứa nội dung và chủ đề nhạy cảm, có thể gây khó chịu cho người chơi.", warningBodyStyle);
            GUILayout.Space(8);
            GUILayout.Label("- Game được lấy bối cảnh miền Trung Việt Nam và được hư cấu hóa vì mục đích giải trí.", warningBodyStyle);
            GUILayout.Space(8);
            GUILayout.Label("- Game không miệt thị, không cổ vũ cho bất kì cá nhân, tổ chức nào, tất cả cốt truyện là do ngẫu nhiên và hư cấu. Mục đích cuối cùng là tạo sự giải trí cho người chơi.", warningBodyStyle);
            
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            
            // Style cho nút bấm
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 15;
            btnStyle.fontStyle = FontStyle.Bold;
            btnStyle.fixedHeight = 45;

            // Nút Đồng ý
            GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
            if (GUILayout.Button("Nói vậy rồi thì ok thoi", btnStyle))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                StartJourney();
            }

            GUILayout.Space(20);

            // Nút Hủy
            GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            if (GUILayout.Button("Không chơi nữa", btnStyle))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                currentTab = MenuTab.Main;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DrawKyUcMienTrung()
        {
            GUIStyle bodyStyle = new GUIStyle();
            bodyStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            bodyStyle.fontSize = 14;
            bodyStyle.wordWrap = true;
            bodyStyle.alignment = TextAnchor.MiddleLeft;

            GUILayout.Label("<b>HƯỚNG DẪN LÀM NÔNG SINH TỒN:</b>", bodyStyle);
            GUILayout.Space(8);
            GUILayout.Label("1. <b>Cải tạo đất sỏi:</b> Đất ruộng ban đầu toàn sỏi đá. Thành phải nhặt đá, tưới nước và ủ phân.", bodyStyle);
            GUILayout.Space(5);
            GUILayout.Label("2. <b>Tích Cốc Phòng Cơ:</b> Độ ẩm mùa lũ lụt cực kỳ cao gây mốc. Chế biến khoai gieo phơi khô để sinh tồn.", bodyStyle);
            GUILayout.Space(5);
            GUILayout.Label("3. <b>Tục Vần Công:</b> Phụ hàng xóm để được giúp đỡ chằng chống nhà cửa khi bão.", bodyStyle);
            GUILayout.Space(5);
            GUILayout.Label("4. <b>Thắp nhang:</b> Khói cúng bái Tổ tiên giúp phục hồi Tinh thần.", bodyStyle);

            GUILayout.Space(20);
            
            GUIStyle clearBtnStyle = new GUIStyle();
            clearBtnStyle.normal.background = transparentTex;
            clearBtnStyle.hover.background = transparentTex;
            clearBtnStyle.active.background = transparentTex;
            clearBtnStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
            clearBtnStyle.hover.textColor = new Color(1f, 0.85f, 0.3f);
            clearBtnStyle.fontSize = 24;
            clearBtnStyle.fontStyle = FontStyle.Bold;
            clearBtnStyle.alignment = TextAnchor.MiddleLeft;
            
            if (GUILayout.Button("< QUAY LẠI", clearBtnStyle))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                currentTab = MenuTab.Main;
            }
        }

        private void DrawDoiNgu()
        {
            GUIStyle bodyStyle = new GUIStyle();
            bodyStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            bodyStyle.fontSize = 16;
            bodyStyle.alignment = TextAnchor.MiddleLeft;

            GUILayout.Label("<b>DỰ ÁN GAME: ĐẤT CÀY LÊN SỎI ĐÁ</b>", bodyStyle);
            GUILayout.Label("<i>Nghị lực phi thường trước thiên tai</i>", bodyStyle);
            GUILayout.Space(25);
            
            GUILayout.Label("<b>Đội Ngũ Phát Triển:</b>", bodyStyle);
            GUILayout.Label("• <b>Thiết Kế:</b> Bạn và Đội Ngũ", bodyStyle);
            GUILayout.Label("• <b>Lập Trình:</b> Antigravity AI", bodyStyle);
            GUILayout.Label("• <b>Mỹ Thuật:</b> Nhóm Bạn Lập Nghiệp", bodyStyle);

            GUILayout.Space(45);
            
            GUIStyle clearBtnStyle = new GUIStyle();
            clearBtnStyle.normal.background = transparentTex;
            clearBtnStyle.hover.background = transparentTex;
            clearBtnStyle.active.background = transparentTex;
            clearBtnStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
            clearBtnStyle.hover.textColor = new Color(1f, 0.85f, 0.3f);
            clearBtnStyle.fontSize = 24;
            clearBtnStyle.fontStyle = FontStyle.Bold;
            clearBtnStyle.alignment = TextAnchor.MiddleLeft;
            
            if (GUILayout.Button("< QUAY LẠI", clearBtnStyle))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                currentTab = MenuTab.Main;
            }
        }

        #endregion

        /// <summary>
        /// Tắt Menu Bắt đầu, chạy lại thời gian và kích hoạt nhân vật di chuyển
        /// </summary>
        private void StartJourney()
        {
            isMenuOpen = false;
            Time.timeScale = 1f; // Chạy lại thời gian trong game bình thường
            
            if (ambientWindAudio != null) ambientWindAudio.Stop();
            SownInStone.Audio.AudioManager.Instance?.StopMusic();
            SownInStone.Audio.AudioManager.Instance?.StopAmbient();
            SownInStone.Audio.AudioManager.Instance?.PlayMusic("bgm_main");

            // Kích hoạt phát âm thanh thời tiết thực tế sau khi vào game
            if (SownInStone.Weather.WeatherManager.Instance != null)
            {
                SownInStone.Weather.WeatherManager.Instance.RefreshWeatherAmbient();
            }
            
            // Kích hoạt hiển thị bảng điều khiển sinh tồn sau khi vào game
#if UNITY_2023_1_OR_NEWER
            FrameworkDebugUI debugUI = FindAnyObjectByType<FrameworkDebugUI>();
#else
            FrameworkDebugUI debugUI = FindObjectOfType<FrameworkDebugUI>();
#endif
            if (debugUI != null)
            {
                debugUI.isUIVisible = false;
            }

            // Kích hoạt hiển thị UI Canvas sinh tồn mới
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.SetUIVisibility(true);
            }

            if (!hasStartedJourney)
            {
                Debug.Log("[MAIN MENU] Cuộc hành trình trở về bám đất Trường Sơn chính thức BẮT ĐẦU!");
                PlayerStats.Instance?.ModifyMorale(20f); // Tặng thêm 20 Morale làm động lực khởi nghiệp
                hasStartedJourney = true;
            }
        }

        private void PauseGame()
        {
            isMenuOpen = true;
            currentTab = MenuTab.Main;
            Time.timeScale = 0f; // Dừng thời gian
            
            SownInStone.Audio.AudioManager.Instance?.StopMusic();
            SownInStone.Audio.AudioManager.Instance?.StopAmbient();
            SownInStone.Audio.AudioManager.Instance?.PlayMusic("bgm_menu");
            if (ambientWindAudio != null) ambientWindAudio.Play();

            // Ẩn UI sinh tồn để không bị đè lên Menu
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.SetUIVisibility(false);
            }
        }

        // --- PHÂN HỆ CÀI ĐẶT & TÙY BIẾN PHÍM BẤM NGOÀI MENU (ONGUI SETTINGS TABS) ---
        private KeyCode GetKeyBinding(string keyName, KeyCode defaultKey)
        {
            if (PlayerController.Instance != null)
            {
                switch (keyName)
                {
                    case "Key_MoveUp": return PlayerController.Instance.keyMoveUp;
                    case "Key_MoveDown": return PlayerController.Instance.keyMoveDown;
                    case "Key_MoveLeft": return PlayerController.Instance.keyMoveLeft;
                    case "Key_MoveRight": return PlayerController.Instance.keyMoveRight;
                    case "Key_Interact": return PlayerController.Instance.keyInteract;
                    case "Key_Run": return PlayerController.Instance.keyRun;
                }
            }
            return (KeyCode)PlayerPrefs.GetInt(keyName, (int)defaultKey);
        }

        private void SetKeyBinding(string keyName, KeyCode key)
        {
            PlayerPrefs.SetInt(keyName, (int)key);
            PlayerPrefs.Save();
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.LoadKeyBindings();
            }
        }

        private void DrawSettings()
        {
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.fontSize = 18;
            headerStyle.normal.textColor = new Color(0.95f, 0.85f, 0.6f);
            headerStyle.alignment = TextAnchor.MiddleLeft;

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.fontSize = 14;
            labelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            labelStyle.alignment = TextAnchor.MiddleLeft;

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 12;
            buttonStyle.fontStyle = FontStyle.Bold;

            GUILayout.Label("<b>TÙY BIẾN PHÍM BẤM</b>", headerStyle);
            GUILayout.Space(10);

            DrawRebindRow("Di chuyển lên", "Key_MoveUp", KeyCode.W, labelStyle, buttonStyle);
            DrawRebindRow("Di chuyển xuống", "Key_MoveDown", KeyCode.S, labelStyle, buttonStyle);
            DrawRebindRow("Di chuyển trái", "Key_MoveLeft", KeyCode.A, labelStyle, buttonStyle);
            DrawRebindRow("Di chuyển phải", "Key_MoveRight", KeyCode.D, labelStyle, buttonStyle);
            DrawRebindRow("Hành động", "Key_Interact", KeyCode.E, labelStyle, buttonStyle);
            DrawRebindRow("Chạy nhanh", "Key_Run", KeyCode.LeftShift, labelStyle, buttonStyle);

            GUILayout.Space(10);
            if (!string.IsNullOrEmpty(rebindAction))
            {
                GUIStyle promptStyle = new GUIStyle();
                promptStyle.fontSize = 14;
                promptStyle.fontStyle = FontStyle.Bold;
                promptStyle.normal.textColor = new Color(1f, 0.4f, 0.4f);
                promptStyle.alignment = TextAnchor.MiddleLeft;
                GUILayout.Label("NHẤN PHÍM BẤT KỲ... (ESC để hủy)", promptStyle);

                Event e = Event.current;
                if (e != null && e.isKey && e.keyCode != KeyCode.None)
                {
                    if (e.keyCode == KeyCode.Escape)
                    {
                        rebindAction = "";
                    }
                    else
                    {
                        SetKeyBinding(rebindAction, e.keyCode);
                        rebindAction = "";
                    }
                    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                    e.Use();
                }
            }

            GUILayout.Space(15);
            
            GUIStyle clearBtnStyle = new GUIStyle();
            clearBtnStyle.normal.background = transparentTex;
            clearBtnStyle.hover.background = transparentTex;
            clearBtnStyle.active.background = transparentTex;
            clearBtnStyle.normal.textColor = new Color(0.9f, 0.85f, 0.7f);
            clearBtnStyle.hover.textColor = new Color(1f, 0.85f, 0.3f);
            clearBtnStyle.fontSize = 24;
            clearBtnStyle.fontStyle = FontStyle.Bold;
            clearBtnStyle.alignment = TextAnchor.MiddleLeft;

            if (GUILayout.Button("< QUAY LẠI", clearBtnStyle))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                rebindAction = "";
                currentTab = MenuTab.Main;
            }
        }

        private void DrawRebindRow(string labelText, string keyName, KeyCode defaultKey, GUIStyle labelStyle, GUIStyle buttonStyle)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(labelText, labelStyle, GUILayout.Width(200));

            KeyCode currentKey = GetKeyBinding(keyName, defaultKey);
            string btnText = (rebindAction == keyName) ? "[Nhấn phím...]" : currentKey.ToString();

            if (GUILayout.Button(btnText, buttonStyle, GUILayout.Width(150)))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                rebindAction = keyName;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        private void DrawOutlinedLabel(Rect rect, string text, GUIStyle style, Color textColor, Color outlineColor, float outlineWidth = 1.5f)
        {
            GUIStyle outlineStyle = new GUIStyle(style);
            outlineStyle.normal.textColor = outlineColor;

            // Draw outline in 8 directions
            for (float x = -outlineWidth; x <= outlineWidth; x += outlineWidth)
            {
                for (float y = -outlineWidth; y <= outlineWidth; y += outlineWidth)
                {
                    if (x == 0 && y == 0) continue;
                    Rect outlineRect = new Rect(rect.x + x, rect.y + y, rect.width, rect.height);
                    GUI.Label(outlineRect, text, outlineStyle);
                }
            }

            // Draw subtle shadow offset
            Rect shadowRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height);
            outlineStyle.normal.textColor = new Color(0f, 0f, 0f, 0.4f);
            GUI.Label(shadowRect, text, outlineStyle);

            // Draw main text
            GUIStyle mainStyle = new GUIStyle(style);
            mainStyle.normal.textColor = textColor;
            GUI.Label(rect, text, mainStyle);
        }

        private bool DrawMenuButton(int index, string text, GUIStyle style, float height = 45f)
        {
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(text), style, GUILayout.Height(height));
            Vector2 mousePos = Event.current.mousePosition;
            bool isHovered = rect.Contains(mousePos);
            
            float targetScale = isHovered ? 1.08f : 1.0f;
            if (Event.current.type == EventType.Repaint)
            {
                menuHoverScales[index] = Mathf.MoveTowards(menuHoverScales[index], targetScale, Time.unscaledDeltaTime / 0.15f);
            }
            
            float currentScale = menuHoverScales[index];
            float scaledWidth = rect.width * currentScale;
            float scaledHeight = rect.height * currentScale;
            float offsetX = (rect.width - scaledWidth) / 2f;
            float offsetY = (rect.height - scaledHeight) / 2f;
            Rect drawRect = new Rect(rect.x + offsetX, rect.y + offsetY, scaledWidth, scaledHeight);
            
            Color normalColor = new Color(0.9f, 0.9f, 0.9f);
            Color hoverColor = new Color(0.98f, 0.85f, 0.35f); // #FAD959
            Color textColor = Color.Lerp(normalColor, hoverColor, (currentScale - 1f) / 0.08f);
            
            GUIStyle buttonStyle = new GUIStyle(style);
            buttonStyle.fontSize = Mathf.RoundToInt(style.fontSize * currentScale);
            buttonStyle.normal.textColor = textColor;
            
            if (isHovered)
            {
                Rect arrowRect = new Rect(drawRect.x - 25f, drawRect.y + (drawRect.height - drawRect.height) / 2f, 20f, drawRect.height);
                GUIStyle arrowStyle = new GUIStyle(buttonStyle);
                arrowStyle.normal.textColor = hoverColor;
                GUI.Label(arrowRect, "▶", arrowStyle);
            }
            
            DrawOutlinedLabel(drawRect, text, buttonStyle, textColor, Color.black, 1.5f);
            
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && isHovered)
            {
                Event.current.Use();
                return true;
            }
            
            return false;
        }

        private void DrawSaveSummary()
        {
            if (!hasStartedJourney) return;

            int day = 1;
            string phaseName = "Lập Nghiệp";
            int karma = 20;

            if (GameManager.Instance != null)
            {
                day = GameManager.Instance.CurrentDay;
                switch (GameManager.Instance.CurrentPhase)
                {
                    case GamePhase.LapNghiep: phaseName = "Lập Nghiệp"; break;
                    case GamePhase.GioLao: phaseName = "Gió Lào"; break;
                    case GamePhase.ChuanBiBao: phaseName = "Chuẩn Bị Bão"; break;
                    case GamePhase.MuaBao: phaseName = "Mùa Bão Lũ"; break;
                    case GamePhase.PhuSa: phaseName = "Phù Sa Sau Lũ"; break;
                }
            }
            if (CommunityManager.Instance != null)
            {
                karma = CommunityManager.Instance.GlobalKarma;
            }

            float panelWidth = 280f;
            float panelHeight = 120f;
            float panelX = 15f;
            float panelY = Screen.height - panelHeight - 40f;

            GUI.color = new Color(0.12f, 0.08f, 0.05f, 0.85f);
            GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, panelHeight), Texture2D.whiteTexture);
            
            GUI.color = new Color(0.85f, 0.7f, 0.35f, 0.8f);
            GUI.DrawTexture(new Rect(panelX, panelY, panelWidth, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panelX, panelY + panelHeight - 2, panelWidth, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panelX, panelY, 2, panelHeight), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(panelX + panelWidth - 2, panelY, 2, panelHeight), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(panelX + 15f, panelY + 10f, panelWidth - 30f, panelHeight - 20f));
            
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.fontSize = 14;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.96f, 0.85f, 0.6f);
            
            GUIStyle itemStyle = new GUIStyle();
            itemStyle.fontSize = 12;
            itemStyle.normal.textColor = Color.white;
            
            GUILayout.Label("◆ TIẾN TRÌNH HIỆN TẠI ◆", titleStyle);
            GUILayout.Space(6);
            GUILayout.Label($"Ngày canh tác: {day}", itemStyle);
            GUILayout.Label($"Giai đoạn: {phaseName}", itemStyle);
            GUILayout.Label($"Điểm Nghĩa Tình: {karma} / 100", itemStyle);
            
            GUILayout.EndArea();
        }
    }
}
