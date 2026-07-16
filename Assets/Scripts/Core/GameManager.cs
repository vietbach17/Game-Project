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
        ChuanBiBao, // Giai đoạn 2: Chuẩn bị bão, loa phát thanh báo bão
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
        [SerializeField] private float currentHour = 20f; // Bắt đầu lúc 20:00 tối để bầu trời tối
        
        [Tooltip("Tốc độ trôi thời gian (Số giây ngoài đời thực cho 1 giờ trong game). Cài đặt 12.5f giây tương ứng 1 ngày = 5 phút thực tế.")]
        [SerializeField] private float secondsPerGameHour = 12.5f; 

        [Header("--- GIAI ĐOẠN GAME ---")]
        [Tooltip("Giai đoạn game hiện tại.")]
        [SerializeField] private GamePhase currentPhase = GamePhase.LapNghiep;

        [Header("--- THIẾT LẬP MỐC CHUYỂN GIAI ĐOẠN ---")]
        [Tooltip("Số ngày bước vào để chuyển sang Gió Lào (Đầu ngày này sẽ chuyển).")]
        [SerializeField] private int lapNghiepDaysLimit = 8;
        
        [Tooltip("Số ngày bước vào để chuyển sang Chuẩn bị bão (Đầu ngày này sẽ chuyển).")]
        [SerializeField] private int gioLaoDaysLimit = 15;

        [Tooltip("Số ngày bước vào để chuyển sang Mùa Bão (Đầu ngày này sẽ chuyển).")]
        [SerializeField] private int chuanBiBaoDaysLimit = 18;
        
        [Tooltip("Số ngày bước vào để chuyển sang Phù Sa (Đầu ngày này sẽ chuyển).")]
        [SerializeField] private int muaBaoDaysLimit = 23;

        [Tooltip("Số ngày bước vào để kết thúc game và hiện Ending screen.")]
        [SerializeField] private int endingDayLimit = 8;

        // Sự kiện thông báo khi chuyển giai đoạn, đổi ngày mới, đổi giờ
        public event Action<GamePhase> OnPhaseChanged;
        public event Action<int> OnDayChanged;
        public event Action<int> OnHourChanged; // Gửi giờ chẵn (int) phục vụ hiệu ứng ánh sáng / AI sinh hoạt

        private int lastTriggeredHour = -1;
        private bool hasWarnedSleepTonight = false;

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
                hasWarnedSleepTonight = false;
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

            if (hourInt == 23 && !hasWarnedSleepTonight)
            {
                hasWarnedSleepTonight = true;
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("😴 Trời đã khuya (23:00)! Nhân vật đang rất buồn ngủ, hãy mau về nhà đi ngủ!");
                }
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_warning");
            }
        }

        /// <summary>
        /// Kiểm tra mốc ngày trôi qua để chuyển tiếp giai đoạn cốt truyện chính.
        /// Các giá trị ngày này có thể cấu hình lại dễ dàng trong Inspector.
        /// </summary>
        private void CheckPhaseTransitionProgress()
        {
            if (currentDay == 3 && currentPhase == GamePhase.LapNghiep)
            {
                TransitionToPhase(GamePhase.ChuanBiBao);
            }
            else if (currentDay == 5 && currentPhase == GamePhase.ChuanBiBao)
            {
                TransitionToPhase(GamePhase.MuaBao);
            }
            else if (currentDay == 7 && currentPhase == GamePhase.MuaBao)
            {
                TransitionToPhase(GamePhase.PhuSa);
            }

            if (currentDay >= 8)
            {
                if (SownInStone.UI.EndingManager.Instance != null)
                {
                    SownInStone.UI.EndingManager.Instance.ShowEnding();
                }
            }
        }

        public void TransitionToPhase(GamePhase newPhase)
        {
            currentPhase = newPhase;
            
            // Cập nhật ngày tương ứng với giai đoạn để đồng bộ UI và progression
            if (newPhase == GamePhase.LapNghiep) currentDay = 1;
            else if (newPhase == GamePhase.ChuanBiBao) currentDay = 3;
            else if (newPhase == GamePhase.MuaBao) currentDay = 5;
            else if (newPhase == GamePhase.PhuSa) currentDay = 7;

            OnDayChanged?.Invoke(currentDay);
            OnPhaseChanged?.Invoke(currentPhase);

            // Kích hoạt chuỗi sự kiện Vần Công hỗ trợ chống bão khi bước vào mùa mưa bão
            if (newPhase == GamePhase.MuaBao)
            {
                SownInStone.Community.CommunityManager.Instance?.TriggerStormHelpSequence();
            }

            // Tự động phủ phù sa cho toàn bộ ruộng đất khi chuyển sang GĐ Phù Sa
            if (newPhase == GamePhase.PhuSa)
            {
                var soils = FindObjectsByType<SownInStone.Agriculture.SoilCell>(FindObjectsInactive.Exclude);
                foreach (var s in soils)
                {
                    if (s != null)
                    {
                        s.quality = SownInStone.Agriculture.SoilQuality.PhuSa;
                        s.Nutrients = 100f;
                        s.RockDensity = 0f;
                        s.UpdateVisuals();
                    }
                }
                Debug.Log($"[GAME MANAGER] Đã tự động bồi đắp PHÙ SA và dọn sạch sỏi đá cho {soils.Length} ô đất.");
            }
            
            // Play phase change warning chime
            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_warning");

            Debug.Log($"[GAME MANAGER] Chuyển sang Giai Đoạn: {newPhase.ToString()}, Ngày: {currentDay}");
        }

        /// <summary>
        /// Tua nhanh thời gian trong game theo số giờ truyền vào.
        /// Tự động cập nhật chuyển phase và kích hoạt OnHourChanged qua mỗi mốc giờ chẵn.
        /// </summary>
        public void AdvanceTime(float hours)
        {
            float timeToAdvance = hours;
            while (timeToAdvance > 0f)
            {
                float step = Mathf.Min(timeToAdvance, 1f); // Xử lý tối đa 1 giờ mỗi bước
                currentHour += step;
                timeToAdvance -= step;

                if (currentHour >= 24f)
                {
                    currentHour -= 24f;
                    currentDay++;
                    OnDayChanged?.Invoke(currentDay);
                    CheckPhaseTransitionProgress();
                }

                int newHourInt = Mathf.FloorToInt(currentHour);
                if (newHourInt != lastTriggeredHour)
                {
                    lastTriggeredHour = newHourInt;
                    OnHourChanged?.Invoke(newHourInt);
                }
            }
        }

        public void RestoreSaveState(int day, GamePhase phase)
        {
            currentDay = day;
            currentPhase = phase;
            OnDayChanged?.Invoke(currentDay);
            OnPhaseChanged?.Invoke(currentPhase);
            Debug.Log($"[GAME MANAGER] Khôi phục tiến trình từ Save: Ngày {currentDay}, Giai đoạn {currentPhase}");
        }

        public void SetTime(float hour)
        {
            currentHour = Mathf.Clamp(hour, 0f, 23.99f);
            int newHourInt = Mathf.FloorToInt(currentHour);
            if (newHourInt != lastTriggeredHour)
            {
                lastTriggeredHour = newHourInt;
                OnHourChanged?.Invoke(newHourInt);
            }
        }

        public void SkipToMorningFiveAM()
        {
            if (currentHour >= 5f)
            {
                currentDay++;
                OnDayChanged?.Invoke(currentDay);
                CheckPhaseTransitionProgress();
            }
            currentHour = 5f;
            hasWarnedSleepTonight = false;
            int newHourInt = 5;
            lastTriggeredHour = newHourInt;
            OnHourChanged?.Invoke(newHourInt);
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
            return false;
        }
        #endregion
    }
}
