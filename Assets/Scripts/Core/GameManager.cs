using System;
using UnityEngine;

namespace SownInStone.Core
{
    /// <summary>
    /// Các giai đoạn chính của cốt truyện và gameplay mô phỏng sinh tồn miền Trung.
    /// </summary>
    public enum GamePhase
    {
        LapNghiep,  // Giai đoạn 1: Khởi đầu phục hồi bờ cõi, cải tạo đất
        GioLao,     // Giai đoạn 2: Nắng cháy, Gió Tây Nam cực độ hạn hán
        MuaBao,     // Giai đoạn 3: Bão lũ cuồng phong, nước sông dâng cô lập
        PhuSa       // Giai đoạn 4: Phù sa sau lũ, tái thiết và mừng công
    }

    /// <summary>
    /// Điều phối chính toàn bộ vòng lặp thời gian (Ngày/Giờ), quản lý giai đoạn game
    /// và thông báo các thay đổi quan trọng đến hệ thống khác.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("--- THỜI GIAN TRONG GAME ---")]
        [Tooltip("Số ngày hiện tại đã trôi qua kể từ khi trở về quê.")]
        [SerializeField] private int currentDay = 1;
        
        [Tooltip("Giờ hiện tại trong ngày (0 - 23).")]
        [SerializeField] private float currentHour = 6f; // Bắt đầu lúc 6:00 sáng
        
        [Tooltip("Tốc độ trôi thời gian (Số giây ngoài đời thực cho 1 giờ trong game).")]
        [SerializeField] private float secondsPerGameHour = 30f; 

        [Header("--- GIAI ĐOẠN GAME ---")]
        [Tooltip("Giai đoạn game hiện tại.")]
        [SerializeField] private GamePhase currentPhase = GamePhase.LapNghiep;

        // Sự kiện thông báo khi chuyển giai đoạn, đổi ngày mới, đổi giờ
        public event Action<GamePhase> OnPhaseChanged;
        public event Action<int> OnDayChanged;
        public event Action<int> OnHourChanged; // Gửi giờ chẵn (int) phục vụ hiệu ứng ánh sáng / AI sinh hoạt

        private int lastTriggeredHour = -1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            UpdateGameTime();
        }

        /// <summary>
        /// Cập nhật thời gian trôi của một ngày.
        /// </summary>
        private void UpdateGameTime()
        {
            currentHour += Time.deltaTime / secondsPerGameHour;

            if (currentHour >= 24f)
            {
                currentHour -= 24f;
                currentDay++;
                OnDayChanged?.Invoke(currentDay);
                
                // Kiểm tra điều kiện tự động chuyển Phase cốt truyện nếu đạt mốc ngày
                CheckPhaseTransitionProgress();
            }

            // Kích hoạt sự kiện giờ chẵn
            int hourInt = Mathf.FloorToInt(currentHour);
            if (hourInt != lastTriggeredHour)
            {
                lastTriggeredHour = hourInt;
                OnHourChanged?.Invoke(hourInt);
            }
        }

        /// <summary>
        /// Kiểm tra mốc ngày trôi qua để chuyển tiếp giai đoạn cốt truyện chính.
        /// Các giá trị ngày này có thể cấu hình lại dễ dàng trong Inspector.
        /// </summary>
        private void CheckPhaseTransitionProgress()
        {
            if (currentDay == 10 && currentPhase == GamePhase.LapNghiep)
            {
                TransitionToPhase(GamePhase.GioLao);
            }
            else if (currentDay == 20 && currentPhase == GamePhase.GioLao)
            {
                TransitionToPhase(GamePhase.MuaBao);
            }
            else if (currentDay == 28 && currentPhase == GamePhase.MuaBao)
            {
                TransitionToPhase(GamePhase.PhuSa);
            }
        }

        /// <summary>
        /// Kích hoạt chuyển đổi giai đoạn game thủ công hoặc tự động.
        /// </summary>
        public void TransitionToPhase(GamePhase newPhase)
        {
            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log($"[GAME MANAGER] Chuyển sang Giai Đoạn: {newPhase.ToString()}");
        }

        #region GETTERS VÀ SETTERS CƠ BẢN
        public int CurrentDay => currentDay;
        public float CurrentHour => currentHour;
        public GamePhase CurrentPhase => currentPhase;
        
        /// <summary>
        /// Kiểm tra xem hiện tại có phải là khoảng thời gian làm việc an toàn trong mùa hè hay không.
        /// Giữa trưa (11h - 15h) mùa Gió Lào cực kỳ khắc nghiệt.
        /// </summary>
        public bool IsDangerousNoon()
        {
            return currentPhase == GamePhase.GioLao && (currentHour >= 11f && currentHour <= 15f);
        }
        #endregion
    }
}
