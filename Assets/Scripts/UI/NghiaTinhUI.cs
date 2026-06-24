using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SownInStone.Community;

namespace SownInStone.UI
{
    /// <summary>
    /// Quản lý giao diện hiển thị điểm Nghĩa Tình (HUD Nghĩa Tình) trong game.
    /// Tự động cập nhật chỉ số, tiến trình và màu sắc tương ứng theo ngưỡng thiện cảm.
    /// </summary>
    public class NghiaTinhUI : MonoBehaviour
    {
        [Header("--- HỆ THỐNG QUẢN LÝ ---")]
        [Tooltip("Tham chiếu đến CommunityManager để đọc điểm Nghĩa Tình.")]
        [SerializeField] private CommunityManager communityManager;

        [Header("--- THÀNH PHẦN GIAO DIỆN (UI COMPONENTS) ---")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private Slider progressBarSlider;
        [SerializeField] private Image progressBarFillImage;

        [Header("--- THIẾT LẬP MÀU SẮC (COLOR STATES) ---")]
        [Tooltip("Màu sắc khi điểm Nghĩa Tình thấp (0 - 30): Lòng tin cộng đồng thấp.")]
        [SerializeField] private Color lowTrustColor = new Color(0.85f, 0.25f, 0.05f, 1f); // Đỏ đất/cam cháy

        [Tooltip("Màu sắc khi điểm Nghĩa Tình trung bình (31 - 70): Lòng tin cộng đồng trung bình.")]
        [SerializeField] private Color mediumTrustColor = new Color(0.95f, 0.8f, 0.3f, 1f); // Vàng rơm/hổ phách

        [Tooltip("Màu sắc khi điểm Nghĩa Tình cao (71 - 100): Lòng tin cộng đồng cao.")]
        [SerializeField] private Color highTrustColor = new Color(0.1f, 0.65f, 0.35f, 1f); // Xanh lục bảo

        private void Start()
        {
            // Thiết lập giá trị tiêu đề mặc định nếu chưa gán
            if (titleText != null)
            {
                titleText.text = "Nghĩa Tình";
            }

            // Tìm kiếm tự động qua Singleton nếu tham chiếu trong Inspector bị bỏ trống
            if (communityManager == null)
            {
                communityManager = CommunityManager.Instance;
            }
        }

        private void Update()
        {
            if (communityManager == null)
            {
                communityManager = CommunityManager.Instance;
            }

            if (communityManager != null)
            {
                UpdateNghiaTinhDisplay(communityManager.GlobalKarma);
            }
        }

        /// <summary>
        /// Cập nhật hiển thị điểm Nghĩa Tình lên màn hình.
        /// </summary>
        private void UpdateNghiaTinhDisplay(int score)
        {
            // 1. Cập nhật Text giá trị (Ví dụ: "45 / 100")
            if (valueText != null)
            {
                valueText.text = $"{score} / 100";
            }

            // 2. Cập nhật thanh tiến trình (Fill Amount / Slider value)
            float fillRatio = Mathf.Clamp01(score / 100f);

            if (progressBarSlider != null)
            {
                progressBarSlider.value = fillRatio;
            }

            if (progressBarFillImage != null)
            {
                progressBarFillImage.fillAmount = fillRatio;
            }

            // 3. Xác định màu sắc tương ứng theo ngưỡng lòng tin
            Color targetColor;
            if (score <= 30)
            {
                targetColor = lowTrustColor;
            }
            else if (score <= 70)
            {
                targetColor = mediumTrustColor;
            }
            else
            {
                targetColor = highTrustColor;
            }

            // 4. Áp dụng màu sắc lên các ảnh lấp đầy
            if (progressBarFillImage != null)
            {
                progressBarFillImage.color = targetColor;
            }

            // Hỗ trợ tự động tìm kiếm fill image trên slider nếu slider được sử dụng
            if (progressBarSlider != null && progressBarSlider.fillRect != null)
            {
                Image sliderFillImage = progressBarSlider.fillRect.GetComponent<Image>();
                if (sliderFillImage != null)
                {
                    sliderFillImage.color = targetColor;
                }
            }
        }
    }
}
