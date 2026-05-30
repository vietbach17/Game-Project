using UnityEngine;
using SownInStone.Core;

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
        private enum MenuTab
        {
            Main,
            KyUcMienTrung, // Hướng dẫn chơi & Luật cốt truyện
            DoiNgu,        // Đội ngũ sản xuất
        }
        private MenuTab currentTab = MenuTab.Main;

        // Hiệu ứng hạt bụi rơm vàng bay lãng mạn trên Menu
        private Vector2[] strawParticles;
        private int particleCount = 20;

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
        }

        private void Update()
        {
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

            // 1. VẼ NỀN HOÀNG HÔN CÁT CHÁY MIỀN TRUNG (Bán trong suốt)
            GUI.color = new Color(0.12f, 0.08f, 0.05f, 0.96f); // Tông cam đất đậm cực đẹp
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 2. VẼ HẠT BỤI RƠM VÀNG LÃNG MẠN
            GUI.color = new Color(0.95f, 0.75f, 0.15f, 0.35f); // Màu rơm vàng óng
            foreach (var particle in strawParticles)
            {
                GUI.DrawTexture(new Rect(particle.x, particle.y, 4, 4), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            // Thiết lập màu sắc khung gỗ Menu
            GUI.backgroundColor = new Color(0.25f, 0.18f, 0.12f, 1f); // Màu nâu gỗ lim đậm

            // Tọa độ trung tâm màn hình
            float menuWidth = 550f;
            float menuHeight = 480f;
            float menuX = (Screen.width - menuWidth) / 2f;
            float menuY = (Screen.height - menuHeight) / 2f;

            // Khung Gỗ Menu Trung Tâm
            Rect menuRect = new Rect(menuX, menuY, menuWidth, menuHeight);
            GUI.Box(menuRect, "");

            // 3. TIÊU ĐỀ GAME NGHỆ THUẬT
            Rect titleRect = new Rect(menuX + 20, menuY + 25, menuWidth - 40, 80);
            GUI.Box(titleRect, "");
            
            // Vẽ Logo chữ nghệ thuật
            GUIStyle titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.96f, 0.64f, 0.15f); // Màu vàng cam lúa chín
            titleStyle.fontSize = 28;
            titleStyle.fontStyle = FontStyle.Bold;
            GUI.Label(titleRect, "<b>ĐẤT CÀY LÊN SỎI ĐÁ</b>", titleStyle);

            Rect subTitleRect = new Rect(menuX + 20, menuY + 80, menuWidth - 40, 20);
            GUIStyle subTitleStyle = new GUIStyle();
            subTitleStyle.alignment = TextAnchor.MiddleCenter;
            subTitleStyle.normal.textColor = new Color(0.85f, 0.8f, 0.75f, 0.8f);
            subTitleStyle.fontSize = 12;
            subTitleStyle.fontStyle = FontStyle.Italic;
            GUI.Label(subTitleRect, "— SOWN IN STONE —", subTitleStyle);

            // Bắt đầu vẽ nội dung các Tab
            GUILayout.BeginArea(new Rect(menuX + 30, menuY + 115, menuWidth - 60, menuHeight - 135));
            GUILayout.Space(15);

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
            }

            GUILayout.EndArea();
        }

        #region VẼ CÁC TRANG MENU CHÍNH
        
        private void DrawMainMenu()
        {
            // Thiết lập kích thước chữ nút bấm gỗ to rõ ràng
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 16;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.fixedHeight = 45;
            
            GUILayout.Space(10);
            GUI.color = new Color(0.15f, 0.85f, 0.15f, 1f); // Nút Bắt đầu nổi bật màu xanh lá trù phú
            if (GUILayout.Button("VỀ QUÊ BÁM ĐẤT (BẮT ĐẦU CHƠI)", buttonStyle))
            {
                StartJourney();
            }
            GUI.color = Color.white;

            GUILayout.Space(15);
            if (GUILayout.Button("KÝ ỨC MIỀN TRUNG (HƯỚNG DẪN LUẬT)", buttonStyle))
            {
                currentTab = MenuTab.KyUcMienTrung;
            }

            GUILayout.Space(15);
            if (GUILayout.Button("ĐỘI NGŨ SẢN XUẤT (CREDITS)", buttonStyle))
            {
                currentTab = MenuTab.DoiNgu;
            }

            GUILayout.Space(15);
            GUI.color = new Color(0.85f, 0.15f, 0.15f, 1f); // Nút thoát màu đỏ rực
            if (GUILayout.Button("THOÁT GAME (EXIT)", buttonStyle))
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            GUI.color = Color.white;

            // Dòng trích dẫn danh ngôn dân gian miền Trung ý nghĩa dưới đáy Menu
            GUILayout.Space(45);
            GUIStyle quoteStyle = new GUIStyle();
            quoteStyle.alignment = TextAnchor.MiddleCenter;
            quoteStyle.normal.textColor = new Color(0.75f, 0.7f, 0.65f);
            quoteStyle.fontSize = 12;
            quoteStyle.fontStyle = FontStyle.Italic;
            GUILayout.Label("\"Tháng bảy kiến bò, chỉ lo lại lụt...\nĐất cày dẫu lên sỏi đá, lòng người không ngã sỏi đá cũng hóa cơm bùi.\"", quoteStyle);
        }

        private void DrawKyUcMienTrung()
        {
            // Trang hướng dẫn chơi mộc mạc dân dã
            GUIStyle bodyStyle = new GUIStyle();
            bodyStyle.normal.textColor = new Color(0.9f, 0.85f, 0.8f);
            bodyStyle.fontSize = 13;
            bodyStyle.wordWrap = true;

            GUILayout.Label("<b>HƯỚNG DẪN LÀM NÔNG SINH TỒN MIỀN TRUNG:</b>", bodyStyle);
            GUILayout.Space(8);
            GUILayout.Label("1. <b>Cải tạo đất sỏi:</b> Đất ruộng ban đầu toàn sỏi đá. Thành phải tiêu hao Thể lực (Stamina) nhặt đá, ra giếng gánh nước tưới ẩm và ủ phân chuồng/phân xanh cải tạo đất bạc màu.", bodyStyle);
            GUILayout.Label("2. <b>Tích Cốc Phòng Cơ:</b> Độ ẩm mùa lũ lụt cực kỳ cao gây mốc nông sản tươi. Bạn bắt buộc phải chế biến khoai tươi thành <b>Khoai gieo phơi khô</b> hoặc làm <b>Dưa muối dưa nhút</b> để trữ cắm trại sinh tồn trên mái nhà mùa lũ lụt.", bodyStyle);
            GUILayout.Label("3. <b>Tục Vần Công (Đổi Công):</b> Hãy sang phụ hàng xóm (Bác Năm, O Thắm) việc đồng áng để tích lũy ngày công. Khi siêu bão đổ bộ, cả xóm sẽ tự động sang chằng chống mái nhà và lùa đàn gà hộ bạn vượt qua hoạn nạn!", bodyStyle);
            GUILayout.Label("4. <b>Thắp nhang Bàn thờ:</b> Thắp nhang khói cúng bái Tổ tiên/Thổ địa giúp Thành hồi phục Morale (Tinh thần), xua tan hoảng loạn khi bão lũ cuồng phong rít qua khe vách.", bodyStyle);

            GUILayout.Space(20);
            if (GUILayout.Button("QUAY LẠI MENU CHÍNH"))
            {
                currentTab = MenuTab.Main;
            }
        }

        private void DrawDoiNgu()
        {
            // Màn hình giới thiệu team sản xuất
            GUIStyle bodyStyle = new GUIStyle();
            bodyStyle.normal.textColor = new Color(0.9f, 0.85f, 0.8f);
            bodyStyle.fontSize = 14;
            bodyStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Space(30);
            GUILayout.Label("<b>DỰ ÁN GAME: ĐẤT CÀY LÊN SỎI ĐÁ</b>", bodyStyle);
            GUILayout.Label("<i>Nghị lực phi thường và nghĩa tình đồng bào miền Trung trước thiên tai</i>", bodyStyle);
            GUILayout.Space(25);
            
            GUILayout.Label("<b>Đội Ngũ Phát Triển (Credits):</b>", bodyStyle);
            GUILayout.Label("• <b>Trưởng Nhóm / Thiết Kế:</b> Bạn và Đội Ngũ Lập Trình", bodyStyle);
            GUILayout.Label("• <b>Đồng Hành Lập Trình:</b> Antigravity AI Assistant", bodyStyle);
            GUILayout.Label("• <b>Mỹ Thuật / Âm Thanh:</b> Nhóm Bạn Lập Nghiệp Làng Xóm", bodyStyle);

            GUILayout.Space(45);
            if (GUILayout.Button("QUAY LẠI MENU CHÍNH"))
            {
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
            
            // Kích hoạt hiển thị bảng điều khiển sinh tồn sau khi vào game
#if UNITY_2023_1_OR_NEWER
            FrameworkDebugUI debugUI = FindAnyObjectByType<FrameworkDebugUI>();
#else
            FrameworkDebugUI debugUI = FindObjectOfType<FrameworkDebugUI>();
#endif
            if (debugUI != null)
            {
                debugUI.isUIVisible = true;
            }

            Debug.Log("[MAIN MENU] Cuộc hành trình trở về bám đất Trường Sơn chính thức BẮT ĐẦU!");
            PlayerStats.Instance?.ModifyMorale(20f); // Tặng thêm 20 Morale làm động lực khởi nghiệp
        }
    }
}
