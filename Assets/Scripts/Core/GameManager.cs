using System;
using UnityEngine;
using SownInStone.UI;
using SownInStone.Weather;

namespace System.Runtime.CompilerServices
{
    internal class IsExternalInit { }
}

namespace SownInStone.Core
{
    public enum GamePhase
    {
        LapNghiep,      // Mốc bắt đầu làm nông (Phase 1)
        GioLao,         // Mốc nắng hạn gió Lào (Phase 2)
        ChuanBiBao,     // Mốc nhận biết thời tiết xấu (Phase 2)
        MuaBao,         // Siêu sự kiện bão lũ dâng cao + Chạy lũ (Chuyển tiếp)
        PhuSa,          // Nước rút, tái thiết trên đất phù sa (Phase 2)
        EndGame         // Kết thúc đánh giá Nghĩa Tình
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Time Configurations")]
        [SerializeField] private int currentDay = 1;
        [SerializeField] private float timeOfDay = 8f; // 8h sáng
        [SerializeField] private float dayLengthInSeconds = 300f; // 1 ngày = 5 phút thực

        [Header("Phase Settings")]
        [SerializeField] private GamePhase currentPhase = GamePhase.LapNghiep;

        // Sự kiện thông báo toàn hệ thống
        public static event Action<int> OnDayChanged;
        public static event Action<GamePhase> OnPhaseChanged;

        private bool isTimerRunning = true;

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

        private void Start()
        {
            // Phát sự kiện khởi tạo ban đầu để HUD cập nhật hiển thị
            OnDayChanged?.Invoke(currentDay);
            OnPhaseChanged?.Invoke(currentPhase);
        }

        private void Update()
        {
            if (!isTimerRunning) return;

            // Đồng hồ đếm thời gian hằng ngày
            timeOfDay += (Time.deltaTime / dayLengthInSeconds) * 24f;

            if (timeOfDay >= 24f)
            {
                timeOfDay = 0f;
                currentDay++;
                OnDayChanged?.Invoke(currentDay);

                // Tự động chuyển mốc chuẩn bị bão vào ngày 3 nếu chưa thắp nhang
                if (currentDay == 3 && currentPhase == GamePhase.LapNghiep)
                {
                    TransitionToPhase(GamePhase.ChuanBiBao);
                }
            }
        }

        /// <summary>
        /// Hàm cốt lõi: Trigger bão khẩn cấp gọi trực tiếp từ AncestralAltar.cs sau khi thắp nhang
        /// </summary>
        public void TriggerStormCrisis()
        {
            if (currentPhase == GamePhase.LapNghiep || currentPhase == GamePhase.ChuanBiBao)
            {
                Debug.Log("[GameManager] Nghi thức cầu an hoàn tất. Kích hoạt siêu sự kiện bão lũ khẩn cấp!");
                TransitionToPhase(GamePhase.MuaBao);
            }
        }

        /// <summary>
        /// Hàm chuyển đổi Giai đoạn toàn cục
        /// </summary>
        public void TransitionToPhase(GamePhase newPhase)
        {
            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(currentPhase);

            // Điều phối thời tiết và HUD tương ứng theo từng mốc
            switch (currentPhase)
            {
                case GamePhase.MuaBao:
                    // Dừng đồng hồ ngày/đêm để tập trung vào gameplay đếm ngược sinh tồn
                    isTimerRunning = false;
                    // Bật đồng hồ đếm ngược 45 giây chạy lũ trên HUD
                    if (SurvivalUIManager.Instance != null)
                    {
                        SurvivalUIManager.Instance.StartEvacuationCountdown(45f);
                    }
                    break;

                case GamePhase.PhuSa:
                    // Nước rút, trả lại chu kỳ ngày đêm để bà con tái thiết làng quê
                    Time.timeScale = 1f;
                    isTimerRunning = true;
                    break;
            }
        }

        // Các hàm Getter hỗ trợ kiểm tra dữ liệu runtime
        public void RestoreSaveState(int savedDay, float savedTimeOfDay, GamePhase savedPhase)
        {
            currentDay = savedDay;
            timeOfDay = savedTimeOfDay;
            TransitionToPhase(savedPhase);
            OnDayChanged?.Invoke(currentDay);
        }

        public void RestoreSaveState(int savedDay, GamePhase savedPhase)
        {
            RestoreSaveState(savedDay, 8f, savedPhase);
        }

        public int CurrentDay => currentDay;
        public float TimeOfDay => timeOfDay;
        public float CurrentHour => timeOfDay;
        public GamePhase CurrentPhase => currentPhase;
        public bool IsBeforeStorm => currentPhase == GamePhase.LapNghiep || currentPhase == GamePhase.GioLao || currentPhase == GamePhase.ChuanBiBao;
    }
}