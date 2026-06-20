using System.Collections;
using UnityEngine;
using TMPro;

namespace SownInStone.UI
{
    /// <summary>
    /// Quản lý Giao diện Loa Phát Thanh Xã và Banner thông báo Giai đoạn (Phase Banner).
    /// Tự động hiển thị tiêu đề giai đoạn, thông tin ngày, và thông điệp loa khẩn cấp,
    /// sau đó tự động ẩn mượt mà qua CanvasGroup sau 5 giây.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class VillageSpeakerBanner : MonoBehaviour
    {
        [Header("--- THÀNH PHẦN GIAO DIỆN (UI COMPONENTS) ---")]
        [SerializeField] private TextMeshProUGUI phaseTitleText;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("--- HIỆU ỨNG & THỜI GIAN ---")]
        [Tooltip("Thời gian hiển thị thông báo đầy đủ trước khi bắt đầu mờ dần (giây).")]
        [SerializeField] private float displayDuration = 5f;
        [Tooltip("Thời gian hiệu ứng làm mờ dần (giây).")]
        [SerializeField] private float fadeDuration = 0.5f;

        private CanvasGroup canvasGroup;
        private Coroutine announcementCoroutine;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            // Ẩn mặc định khi khởi chạy
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Kích hoạt hiển thị thông báo loa phát thanh xã.
        /// </summary>
        public void ShowAnnouncement(string phaseTitle, string message, int day)
        {
            if (phaseTitleText != null)
            {
                phaseTitleText.text = phaseTitle;
            }

            if (dayText != null)
            {
                dayText.text = $"Ngày {day}";
            }

            if (messageText != null)
            {
                messageText.text = message;
            }

            if (announcementCoroutine != null)
            {
                StopCoroutine(announcementCoroutine);
            }

            announcementCoroutine = StartCoroutine(AnnouncementFlow());
        }

        private IEnumerator AnnouncementFlow()
        {
            // 1. Fade In
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // 2. Chờ thời gian hiển thị
            yield return new WaitForSeconds(displayDuration);

            // 3. Fade Out
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            announcementCoroutine = null;
        }
    }
}
