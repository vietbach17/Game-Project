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

        private enum SettingsSubTab
        {
            ControlsAndVolume,
            NpcInfo,
            GameStats
        }
        private SettingsSubTab currentSettingsSubTab = SettingsSubTab.ControlsAndVolume;
        
        private bool showSaveConfirmation = false;
        private float saveConfirmationTime = 0f;
        private Vector2 settingsScrollPosition = Vector2.zero;

        // Hiệu ứng hạt bụi rơm vàng bay lãng mạn trên Menu
        private Vector2[] strawParticles;
        private int particleCount = 20;
        
        private Texture2D bgImage;
        private Texture2D transparentTex;
        private Texture2D gearIconTex;
        
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
        // Ensure TutorialManager exists
        if (TutorialManager.Instance == null)
        {
            var tmGO = new GameObject("TutorialManager");
            tmGO.AddComponent<TutorialManager>();
        }
        }

        private void Start()
        {
            isMenuOpen = showMenuOnStart;

            if (PlayerPrefs.GetInt("Save_HasStarted", 0) == 1)
            {
                hasStartedJourney = true;
            }

            if (isMenuOpen)
            {
                // Dừng thời gian game để người chơi trải nghiệm Menu điện ảnh
                Time.timeScale = 0f;
                if (ambientWindAudio != null) ambientWindAudio.Play();
                SownInStone.Audio.AudioManager.Instance?.PlayMusic("bgm_menu");
                SownInStone.Audio.AudioManager.Instance?.StopAmbient();

                // Đảm bảo con trỏ chuột luôn hiện ở màn hình sảnh
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible   = true;

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
            gearIconTex = Resources.Load<Texture2D>("UI/gear_icon");

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
            if (!isMenuOpen)
            {
                if (hasStartedJourney)
                {
                    DrawGearIconInGameplay();
                }
                return;
            }

            // Draw background or gameplay overlay
            if (hasStartedJourney)
            {
                // Semi-transparent dark overlay so player sees the gameplay behind it
                GUI.color = new Color(0.08f, 0.08f, 0.08f, 0.85f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
            else
            {
                // Full main menu background at start
                if (bgImage != null)
                {
                    GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), bgImage, ScaleMode.ScaleAndCrop);
                }

                // Straw particles
                GUI.color = new Color(0.95f, 0.75f, 0.15f, 0.35f); 
                foreach (var particle in strawParticles)
                {
                    GUI.DrawTexture(new Rect(particle.x, particle.y, 4, 4), Texture2D.whiteTexture);
                }
                GUI.color = Color.white;
            }

            // Tọa độ menu nằm bên trái màn hình
            float menuWidth = 500f;
            float menuX = 50f; // Căn lề trái

            if (hasStartedJourney)
            {
                // Simple pause header
                Rect pauseTitleRect = new Rect(menuX, 60f, menuWidth, 50f);
                GUIStyle pauseTitleStyle = new GUIStyle();
                pauseTitleStyle.alignment = TextAnchor.MiddleLeft;
                pauseTitleStyle.fontSize = 32;
                pauseTitleStyle.fontStyle = FontStyle.Bold;
                DrawOutlinedLabel(pauseTitleRect, "ĐÃ TẠM DỪNG", pauseTitleStyle, new Color(0.96f, 0.85f, 0.6f), Color.black, 2f);
            }
            else
            {
                // Original big game title and subtitle
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
            }

            // Bắt đầu vẽ nội dung các Tab (Tối ưu Y và chiều cao khi trong game)
            float contentStartY = 250f;
            if (hasStartedJourney)
            {
                contentStartY = 140f; // Đẩy lên cao hơn khi ở trong màn chơi
            }
            float areaHeight = Screen.height - contentStartY - 45f;
            
            GUILayout.BeginArea(new Rect(menuX, contentStartY, menuWidth, areaHeight));

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
                    LoadGame();
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
                
                // Clear old save for a clean New Game
                PlayerPrefs.DeleteKey("Save_HasStarted");
                PlayerPrefs.DeleteKey("Save_Day");
                PlayerPrefs.DeleteKey("Save_Phase");
                PlayerPrefs.DeleteKey("Save_Health");
                PlayerPrefs.DeleteKey("Save_Stamina");
                PlayerPrefs.DeleteKey("Save_Morale");
                PlayerPrefs.DeleteKey("Save_Coins");
                PlayerPrefs.Save();
                hasStartedJourney = false;

                bool enableTutorial = PlayerPrefs.GetInt("ShowTutorialSetting", 1) == 1;
                if (enableTutorial && TutorialManager.Instance != null)
                {
                    isMenuOpen = false;
                    TutorialManager.Instance.ShowTutorial(() => StartJourney());
                }
                else
                {
                    StartJourney();
                }
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

            // Khóa lại con trỏ chuột theo mode camera hiện tại khi vào game
            SownInStone.Core.CameraFollow3D.Instance?.SetCameraMode(
                SownInStone.Core.CameraFollow3D.Instance.CurrentMode
            );

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
                
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.InitializeTutorial();
                }
            }
        }

        private void PauseGame()
        {
            isMenuOpen = true;
            currentTab = MenuTab.Settings;
            Time.timeScale = 0f; // Dừng thời gian
            
            SownInStone.Audio.AudioManager.Instance?.StopMusic();
            SownInStone.Audio.AudioManager.Instance?.StopAmbient();
            SownInStone.Audio.AudioManager.Instance?.PlayMusic("bgm_menu");
            if (ambientWindAudio != null) ambientWindAudio.Play();

            // Trả lại con trỏ chuột khi mở menu
            SownInStone.Core.CameraFollow3D.Instance?.ReleaseCursor();

            // Ẩn UI sinh tồn để không bị đè lên Menu
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.SetUIVisibility(false);
            }
        }

        private void DrawGearIconInGameplay()
        {
            if (gearIconTex == null) return;

            float size = 45f;
            float x = Screen.width - size - 20f;
            float y = Screen.height - size - 20f;
            Rect gearRect = new Rect(x, y, size, size);

            // Background subtle box for clarity
            GUI.color = new Color(0.12f, 0.1f, 0.08f, 0.5f);
            GUI.DrawTexture(gearRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            Vector2 mousePos = Event.current.mousePosition;
            bool isHovered = gearRect.Contains(mousePos);
            if (isHovered)
            {
                GUI.color = new Color(0.98f, 0.85f, 0.35f, 1f);
            }
            else
            {
                GUI.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
            }

            GUI.DrawTexture(gearRect, gearIconTex, ScaleMode.ScaleToFit);
            GUI.color = Color.white;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && isHovered)
            {
                Event.current.Use();
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                PauseGame();
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

            // Draw sub-tab headers
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = currentSettingsSubTab == SettingsSubTab.ControlsAndVolume ? new Color(0.85f, 0.7f, 0.35f, 1f) : Color.white;
            if (GUILayout.Button("Thiết lập & Âm lượng", buttonStyle, GUILayout.Height(30)))
            {
                currentSettingsSubTab = SettingsSubTab.ControlsAndVolume;
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
            }
            GUI.backgroundColor = currentSettingsSubTab == SettingsSubTab.NpcInfo ? new Color(0.85f, 0.7f, 0.35f, 1f) : Color.white;
            if (GUILayout.Button("Thông tin các NPC", buttonStyle, GUILayout.Height(30)))
            {
                currentSettingsSubTab = SettingsSubTab.NpcInfo;
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
            }
            GUI.backgroundColor = currentSettingsSubTab == SettingsSubTab.GameStats ? new Color(0.85f, 0.7f, 0.35f, 1f) : Color.white;
            if (GUILayout.Button("Thông số game", buttonStyle, GUILayout.Height(30)))
            {
                currentSettingsSubTab = SettingsSubTab.GameStats;
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            // Calculate height dynamically
            float currentStartY = hasStartedJourney ? 140f : 250f;
            float currentAreaHeight = Screen.height - currentStartY - 45f;
            float scrollHeight = currentAreaHeight - 95f;

            settingsScrollPosition = GUILayout.BeginScrollView(settingsScrollPosition, GUILayout.Width(490f), GUILayout.Height(scrollHeight));

            if (currentSettingsSubTab == SettingsSubTab.ControlsAndVolume)
            {
                GUILayout.Label("<b>TÙY BIẾN PHÍM BẤM</b>", headerStyle);
                GUILayout.Space(5);

                DrawRebindRow("Di chuyển lên", "Key_MoveUp", KeyCode.W, labelStyle, buttonStyle);
                DrawRebindRow("Di chuyển xuống", "Key_MoveDown", KeyCode.S, labelStyle, buttonStyle);
                DrawRebindRow("Di chuyển trái", "Key_MoveLeft", KeyCode.A, labelStyle, buttonStyle);
                DrawRebindRow("Di chuyển phải", "Key_MoveRight", KeyCode.D, labelStyle, buttonStyle);
                DrawRebindRow("Hành động", "Key_Interact", KeyCode.E, labelStyle, buttonStyle);
                DrawRebindRow("Chạy nhanh", "Key_Run", KeyCode.LeftShift, labelStyle, buttonStyle);

                GUILayout.Space(10);
                GUILayout.Label("<b>ÂM LƯỢNG GAME</b>", headerStyle);
                GUILayout.Space(5);

                // Music Volume Slider
                GUILayout.BeginHorizontal();
                GUILayout.Label("Nhạc nền (BGM)", labelStyle, GUILayout.Width(200));
                float currentMusicVol = SownInStone.Audio.AudioManager.Instance != null ? SownInStone.Audio.AudioManager.Instance.MusicVolume : 0.5f;
                float newMusicVol = GUILayout.HorizontalSlider(currentMusicVol, 0f, 1f, GUILayout.Width(150));
                GUILayout.Label($" {Mathf.RoundToInt(newMusicVol * 100)}%", labelStyle, GUILayout.Width(50));
                if (newMusicVol != currentMusicVol && SownInStone.Audio.AudioManager.Instance != null)
                {
                    SownInStone.Audio.AudioManager.Instance.MusicVolume = newMusicVol;
                }
                GUILayout.EndHorizontal();

                // SFX Volume Slider
                GUILayout.BeginHorizontal();
                GUILayout.Label("Hiệu ứng (SFX)", labelStyle, GUILayout.Width(200));
                float currentSFXVol = SownInStone.Audio.AudioManager.Instance != null ? SownInStone.Audio.AudioManager.Instance.SFXVolume : 1f;
                float newSFXVol = GUILayout.HorizontalSlider(currentSFXVol, 0f, 1f, GUILayout.Width(150));
                GUILayout.Label($" {Mathf.RoundToInt(newSFXVol * 100)}%", labelStyle, GUILayout.Width(50));
                if (newSFXVol != currentSFXVol && SownInStone.Audio.AudioManager.Instance != null)
                {
                    SownInStone.Audio.AudioManager.Instance.SFXVolume = newSFXVol;
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                GUILayout.Label("<b>HƯỚNG DẪN TÂN THỦ</b>", headerStyle);
                GUILayout.Space(5);

                bool showTutorial = PlayerPrefs.GetInt("ShowTutorialSetting", 1) == 1;
                string toggleText = showTutorial ? "Đang Bật [ON]" : "Đang Tắt [OFF]";

                GUILayout.BeginHorizontal();
                GUILayout.Label("Hiển thị hướng dẫn khi chơi mới", labelStyle, GUILayout.Width(200));
                if (GUILayout.Button(toggleText, buttonStyle, GUILayout.Width(150)))
                {
                    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                    PlayerPrefs.SetInt("ShowTutorialSetting", showTutorial ? 0 : 1);
                    PlayerPrefs.Save();
                }
                GUILayout.EndHorizontal();

                // ── GÓC NHÌN CAMERA ───────────────────────────────────────
                GUILayout.Space(10);
                GUILayout.Label("<b>GÓC NHÌN CAMERA</b>", headerStyle);
                GUILayout.Space(5);

                GUIStyle camBtnStyle = new GUIStyle(GUI.skin.button);
                camBtnStyle.fontSize    = 12;
                camBtnStyle.fontStyle   = FontStyle.Bold;
                camBtnStyle.fixedHeight = 32;

                int savedCamMode = PlayerPrefs.GetInt("CameraMode", 0);

                string[] camModeLabels = new string[]
                {
                    "📷 Góc nhìn thứ 3",
                    "🎥 Góc nhìn cố định",
                    "👁 Góc nhìn thứ nhất"
                };
                string[] camModeDesc = new string[]
                {
                    "Nhìn từ sau lưng Thành, kéo chuột phải để xoay.",
                    "Camera đứng yên nhìn xuống theo góc isometric.",
                    "Nhìn từ mắt Thành, chuột để xoay hướng nhìn."
                };

                GUILayout.BeginHorizontal();
                for (int i = 0; i < 3; i++)
                {
                    bool isActive = (savedCamMode == i);
                    GUI.backgroundColor = isActive ? new Color(0.9f, 0.7f, 0.2f, 1f) : Color.white;
                    GUIStyle thisBtnStyle = new GUIStyle(camBtnStyle);
                    if (isActive)
                    {
                        thisBtnStyle.normal.textColor  = Color.black;
                        thisBtnStyle.fontStyle         = FontStyle.Bold;
                    }
                    if (GUILayout.Button(camModeLabels[i], thisBtnStyle))
                    {
                        SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                        PlayerPrefs.SetInt("CameraMode", i);
                        PlayerPrefs.Save();
                        // Áp dụng ngay lập tức nếu đang chơi
                        if (SownInStone.Core.CameraFollow3D.Instance != null)
                        {
                            SownInStone.Core.CameraFollow3D.Instance.SetCameraMode(
                                (SownInStone.Core.CameraFollow3D.CameraMode)i
                            );
                        }
                    }
                }
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();

                // Mô tả mode đang chọn
                GUIStyle camDescStyle = new GUIStyle(labelStyle);
                camDescStyle.fontStyle = FontStyle.Italic;
                camDescStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);
                GUILayout.Label($"  ➤ {camModeDesc[savedCamMode]}", camDescStyle);

                GUIStyle hintStyle = new GUIStyle(labelStyle);
                hintStyle.normal.textColor = new Color(0.6f, 0.8f, 0.6f);
                hintStyle.fontSize = 12;
                GUILayout.Label("  Phím tắt [V] trong lúc chơi để chuyển góc nhìn nhanh.", hintStyle);
                // ── KẾT THÚC GÓC NHÌN CAMERA ─────────────────────────────

                GUILayout.Space(10);
                GUILayout.Label("<b>TIẾN TRÌNH CHƠI</b>", headerStyle);
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Lưu Game", buttonStyle, GUILayout.Width(130), GUILayout.Height(30)))
                {
                    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                    SaveGame();
                    showSaveConfirmation = true;
                    saveConfirmationTime = Time.unscaledTime;
                }
                
                if (hasStartedJourney)
                {
                    GUILayout.Space(15);
                    if (GUILayout.Button("Thoát ra Menu chính", buttonStyle, GUILayout.Width(180), GUILayout.Height(30)))
                    {
                        SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                        QuitToMainMenu();
                    }
                }
                GUILayout.EndHorizontal();

                if (showSaveConfirmation)
                {
                    GUILayout.Space(3);
                    GUIStyle successStyle = new GUIStyle(labelStyle) { fontStyle = FontStyle.Bold };
                    successStyle.normal.textColor = new Color(0.3f, 0.9f, 0.3f);
                    GUILayout.Label("✔ Đã lưu tiến trình chơi thành công!", successStyle);
                    if (Time.unscaledTime - saveConfirmationTime > 3f)
                    {
                        showSaveConfirmation = false;
                    }
                }

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
            }
            else if (currentSettingsSubTab == SettingsSubTab.NpcInfo)
            {
                GUILayout.Label("<b>HỒ SƠ CÁC NHÂN VẬT (NPC)</b>", headerStyle);
                GUILayout.Space(10);

                DrawNpcRow("Thành", "Nhân vật chính. Người thanh niên đầy nghị lực trở về quê hương bám đất bám làng để gieo hạt sinh tồn vượt qua thiên tai bão lũ miền Trung.", labelStyle);
                DrawNpcRow("O Thắm", "Chủ tiệm tạp hóa xinh đẹp của làng. Nơi bạn ghé qua trao đổi mua các loại hạt giống nông sản hoặc bán củ khoai củ sắn tích cốc phòng cơ.", labelStyle);
                DrawNpcRow("Bác Năm", "Người hàng xóm hiền hậu, chất phác. Tục vần công giúp bạn và Bác tương trợ nhau dọn dẹp, chằng chống nhà cửa khi bão giông sắp đổ bộ.", labelStyle);
            }
            else if (currentSettingsSubTab == SettingsSubTab.GameStats)
            {
                GUILayout.Label("<b>THÔNG SỐ MÔI TRƯỜNG & CHỈ SỐ SINH TỒN</b>", headerStyle);
                GUILayout.Space(10);

                if (SownInStone.Core.PlayerStats.Instance != null)
                {
                    GUILayout.Label("<b>Chỉ số người chơi:</b>", labelStyle);
                    GUILayout.Label($"  • Sức khỏe: {Mathf.RoundToInt(SownInStone.Core.PlayerStats.Instance.CurrentHealth)} / 100", labelStyle);
                    GUILayout.Label($"  • Thể lực: {Mathf.RoundToInt(SownInStone.Core.PlayerStats.Instance.CurrentStamina)} / 100", labelStyle);
                    GUILayout.Label($"  • Tinh thần: {Mathf.RoundToInt(SownInStone.Core.PlayerStats.Instance.CurrentMorale)} / 100", labelStyle);
                    GUILayout.Label($"  • Nhiễm lạnh: {Mathf.RoundToInt(SownInStone.Core.PlayerStats.Instance.ColdStress)}%", labelStyle);
                    GUILayout.Label($"  • Sốc nhiệt: {Mathf.RoundToInt(SownInStone.Core.PlayerStats.Instance.HeatStress)}%", labelStyle);
                    GUILayout.Label($"  • Tiền tích lũy: {SownInStone.Core.PlayerStats.Instance.Coins} xu", labelStyle);
                    GUILayout.Space(10);
                }

                if (SownInStone.Weather.WeatherManager.Instance != null)
                {
                    GUILayout.Label("<b>Thông số thời tiết:</b>", labelStyle);
                    string weatherName = "";
                    switch (SownInStone.Weather.WeatherManager.Instance.currentVisualWeather)
                    {
                        case SownInStone.Weather.WeatherType.OnDinh: weatherName = "Bình thường / Nắng dịu"; break;
                        case SownInStone.Weather.WeatherType.NangNong: weatherName = "Nắng nóng gay gắt"; break;
                        case SownInStone.Weather.WeatherType.GioLao: weatherName = "Gió Lào bỏng rát"; break;
                        case SownInStone.Weather.WeatherType.MuaGiong: weatherName = "Mưa giông"; break;
                        case SownInStone.Weather.WeatherType.BaoLu: weatherName = "Mưa bão & Lũ ngập"; break;
                    }
                    GUILayout.Label($"  • Thời tiết: {weatherName}", labelStyle);
                    GUILayout.Label($"  • Nhiệt độ: {SownInStone.Weather.WeatherManager.Instance.Temperature:F1} °C", labelStyle);
                    GUILayout.Label($"  • Độ ẩm: {Mathf.RoundToInt(SownInStone.Weather.WeatherManager.Instance.Humidity)}%", labelStyle);
                    GUILayout.Label($"  • Tốc độ gió: {SownInStone.Weather.WeatherManager.Instance.WindSpeed:F1} km/h", labelStyle);
                    GUILayout.Label($"  • Cường độ mưa: {Mathf.RoundToInt(SownInStone.Weather.WeatherManager.Instance.RainIntensity * 100)}%", labelStyle);
                    GUILayout.Label($"  • Mực nước lũ: {SownInStone.Weather.WeatherManager.Instance.FloodLevel:F2} m", labelStyle);
                    GUILayout.Space(10);
                }

                if (SownInStone.Core.GameManager.Instance != null)
                {
                    GUILayout.Label("<b>Thời gian & Giai đoạn cốt truyện:</b>", labelStyle);
                    GUILayout.Label($"  • Ngày chơi: Ngày {SownInStone.Core.GameManager.Instance.CurrentDay}", labelStyle);
                    int hour = Mathf.FloorToInt(SownInStone.Core.GameManager.Instance.CurrentHour);
                    int min = Mathf.FloorToInt((SownInStone.Core.GameManager.Instance.CurrentHour - hour) * 60);
                    GUILayout.Label($"  • Giờ hiện tại: {hour:D2}:{min:D2}", labelStyle);
                    
                    string phaseName = "";
                    switch (SownInStone.Core.GameManager.Instance.CurrentPhase)
                    {
                        case SownInStone.Core.GamePhase.LapNghiep: phaseName = "Giai đoạn 1: Lập nghiệp / Cải tạo đất"; break;
                        case SownInStone.Core.GamePhase.GioLao: phaseName = "Giai đoạn 2: Nắng hạn / Gió Lào"; break;
                        case SownInStone.Core.GamePhase.ChuanBiBao: phaseName = "Chuẩn bị bão giông"; break;
                        case SownInStone.Core.GamePhase.MuaBao: phaseName = "Giai đoạn 3: Mưa bão / Thiên tai lũ lụt"; break;
                        case SownInStone.Core.GamePhase.PhuSa: phaseName = "Giai đoạn 4: Đắp đập phù sa / Tái thiết"; break;
                    }
                    GUILayout.Label($"  • Giai đoạn: {phaseName}", labelStyle);
                }
            }

            GUILayout.EndScrollView();

            GUILayout.Space(10);
            
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
                if (hasStartedJourney)
                {
                    StartJourney();
                }
                else
                {
                    currentTab = MenuTab.Main;
                }
            }
        }

        private void DrawNpcRow(string name, string desc, GUIStyle labelStyle)
        {
            GUILayout.Label($"<b>★ {name}</b>", labelStyle);
            GUILayout.Label($"<color=#cccccc>{desc}</color>", new GUIStyle(labelStyle) { fontSize = 12, wordWrap = true });
            GUILayout.Space(10);
        }

        private void SaveGame()
        {
            PlayerPrefs.SetInt("Save_HasStarted", 1);
            if (GameManager.Instance != null)
            {
                PlayerPrefs.SetInt("Save_Day", GameManager.Instance.CurrentDay);
                PlayerPrefs.SetInt("Save_Phase", (int)GameManager.Instance.CurrentPhase);
            }
            if (PlayerStats.Instance != null)
            {
                PlayerPrefs.SetFloat("Save_Health", PlayerStats.Instance.CurrentHealth);
                PlayerPrefs.SetFloat("Save_Stamina", PlayerStats.Instance.CurrentStamina);
                PlayerPrefs.SetFloat("Save_Morale", PlayerStats.Instance.CurrentMorale);
                PlayerPrefs.SetInt("Save_Coins", PlayerStats.Instance.Coins);
            }
            PlayerPrefs.Save();
            Debug.Log("[SAVE SYSTEM] Tiến trình game đã được lưu vào PlayerPrefs!");
        }

        private void LoadGame()
        {
            if (PlayerPrefs.GetInt("Save_HasStarted", 0) == 0) return;

            hasStartedJourney = true;

            int day = PlayerPrefs.GetInt("Save_Day", 1);
            GamePhase phase = (GamePhase)PlayerPrefs.GetInt("Save_Phase", 0);
            float health = PlayerPrefs.GetFloat("Save_Health", 100f);
            float stamina = PlayerPrefs.GetFloat("Save_Stamina", 100f);
            float morale = PlayerPrefs.GetFloat("Save_Morale", 100f);
            int savedCoins = PlayerPrefs.GetInt("Save_Coins", 50);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestoreSaveState(day, phase);
            }
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.RestoreSaveState(health, stamina, morale, savedCoins);
            }
            Debug.Log("[SAVE SYSTEM] Tiến trình game đã được khôi phục thành công từ PlayerPrefs!");
        }

        private void QuitToMainMenu()
        {
            // Auto save before quitting
            SaveGame();

            // Set state to show main startup screen
            isMenuOpen = true;
            currentTab = MenuTab.Main;
            Time.timeScale = 0f; // Keep game paused
            
            // Switch music to main menu music
            SownInStone.Audio.AudioManager.Instance?.StopMusic();
            SownInStone.Audio.AudioManager.Instance?.StopAmbient();
            SownInStone.Audio.AudioManager.Instance?.PlayMusic("bgm_menu");
            if (ambientWindAudio != null) ambientWindAudio.Play();

            // Hide survival UI panel
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.SetUIVisibility(false);
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
