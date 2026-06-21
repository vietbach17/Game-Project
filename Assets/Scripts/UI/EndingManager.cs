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

            if (score <= 29)
            {
                title = "ĐẤT SỎI ĐÁ CẰN";
                description = "Thành sống sót qua mùa thiên tai, nhưng vì thiếu sự sẻ chia, ngôi làng dần trở nên vắng lặng. Một số hộ dân rời quê đi nơi khác. Mảnh đất vẫn còn đó, nhưng tình người đã thưa dần.";
            }
            else if (score <= 70)
            {
                title = "LÁ LÀNH ĐÙM LÁ RÁCH";
                description = "Ngôi làng vượt qua bão lũ với nhiều mất mát. Dù còn khó khăn, bà con vẫn cùng nhau sửa lại mái nhà, dọn bùn ngoài ruộng và bắt đầu một mùa vụ mới.";
            }
            else
            {
                title = "ĐẤT CÀY NỞ HOA";
                description = "Nhờ sự sẻ chia và tinh thần vần công, cả làng cùng nhau vượt qua thiên tai. Khi nước rút, lớp phù sa để lại mầm sống mới. Thành không chỉ giữ được mảnh đất tổ tiên, mà còn tìm lại được quê hương trong tình nghĩa đồng bào.";
            }

            if (endingTitleText != null)
            {
                endingTitleText.text = title;
            }
            if (endingDescriptionText != null)
            {
                endingDescriptionText.text = description;
            }

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
