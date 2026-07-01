using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SownInStone.Core;
using SownInStone.Community;

namespace SownInStone.UI
{
    /// <summary>
    /// Quản lý màn hình kết thúc game, chọn kịch bản kết cục tương ứng dựa trên tổng điểm Nghĩa Tình tích lũy.
    /// </summary>
    public class EndingManager : MonoBehaviour
    {
        public static EndingManager Instance { get; private set; }

        [Header("--- UI PANELS & ELEMENTS ---")]
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private TextMeshProUGUI endingTitleText;
        [SerializeField] private TextMeshProUGUI endingDescriptionText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;

        private bool endingShown = false;
        public bool IsEndingShown => endingShown;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (endingPanel != null)
            {
                endingPanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }
            if (exitButton != null)
            {
                exitButton.onClick.AddListener(ExitGame);
            }
        }

        /// <summary>
        /// Kích hoạt hiển thị màn hình kết thúc trò chơi dựa trên điểm Nghĩa Tình.
        /// </summary>
        public void ShowEnding()
        {
            if (endingShown) return;
            endingShown = true;

            // 1. Tắt di chuyển và hành động của Player
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.enabled = false;
                var rb = PlayerController.Instance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                }
            }

            // 2. Ẩn HUD chính của game
            if (SurvivalUIManager.Instance != null)
            {
                SurvivalUIManager.Instance.SetHUDVisible(false);
            }

            // 3. Đọc điểm Nghĩa Tình và hiển thị kết cục tương ứng
            int score = 0;
            if (CommunityManager.Instance != null)
            {
                score = CommunityManager.Instance.GlobalKarma;
            }

            string title = "";
            string description = "";

            if (score >= 40)
            {
                title = "<color=#2ECC71>KẾT CỤC: ĐẤT NỞ HOA (ĐẠI CÁT)</color>";
                description = $"Với điểm Nghĩa Tình là {score}, bạn đã đồng lòng cùng cả làng Bác Năm, O Thắm sơ tán an toàn và cứu sống ruộng vườn. Sau lũ, phù sa bồi đắp giúp khoai lang tươi tốt trúng mùa lớn. Làng quê nghèo miền Trung cùng nhau vực dậy tái thiết cuộc sống ấm no.";
            }
            else if (score >= 15)
            {
                title = "<color=#F1C40F>KẾT CỤC: LÁ LÀNH ĐÙM LÁ RÁCH (BÌNH AN)</color>";
                description = $"Với điểm Nghĩa Tình là {score}, tuy ruộng khoai bị thiệt hại nhẹ sau bão và cuộc sống tái thiết còn nhiều chông gai, nhưng nhờ sự đoàn kết giúp đỡ lẫn nhau của bà con, mọi người vẫn vượt qua thiên tai bình an vô sự.";
            }
            else
            {
                title = "<color=#E74C3C>KẾT CỤC: TIÊU ĐIỀU LY TÁN (TIÊU ĐIỀU)</color>";
                description = $"Với điểm Nghĩa Tình thấp ({score}), bạn ích kỷ không giúp đỡ cộng đồng và không dự trữ đủ lương thực phòng lũ. Cơn bão quét qua cuốn trôi nhà cửa, bà con phải ly tán lên thành phố kiếm sống trong sự nghèo đói.";
            }

            if (endingTitleText != null)
            {
                endingTitleText.text = title;
            }
            if (endingDescriptionText != null)
            {
                endingDescriptionText.text = description;
            }

            Time.timeScale = 0f; // Dừng game

            // 4. Hiện Ending Panel
            if (endingPanel != null)
            {
                endingPanel.SetActive(true);
            }

            Debug.Log($"[ENDING] Đã hiển thị Ending: {title} với điểm Nghĩa Tình {score}");
        }

        public void RestartGame()
        {
            Debug.Log("[ENDING] Đang tải lại Game...");
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ExitGame()
        {
            Debug.Log("[ENDING] Thoát game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
