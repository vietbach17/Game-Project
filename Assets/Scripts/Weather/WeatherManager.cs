using UnityEngine;
using SownInStone.Core;

namespace SownInStone.Weather
{
    public enum WeatherType
    {
        OnDinh,     // Thời tiết nắng mát lập nghiệp ban đầu
        NangNong,   // Nắng hè gay gắt
        GioLao,     // Gió Lào khô bỏng rát (nhiệt cao, ẩm thấp)
        MuaGiong,   // Mưa giông nhiệt đới ẩm ướt
        BaoLu       // Cuồng phong mưa bão, nước lũ dâng cao
    }

    /// <summary>
    /// Hệ thống quản lý toàn bộ các yếu tố khí hậu, thời tiết trong game.
    /// Tạo ra các biến thiên nhiệt độ, gió bão và mực nước lũ ngập ruộng vườn.
    /// </summary>
    public class WeatherManager : MonoBehaviour
    {
        public static WeatherManager Instance { get; private set; }

        [Header("--- THÔNG SỐ KHÍ HẬU HIỆN TẠI ---")]
        public WeatherType currentVisualWeather = WeatherType.OnDinh;
        
        [Tooltip("Nhiệt độ hiện tại (°C).")]
        public float Temperature = 28f;
        
        [Tooltip("Độ ẩm không khí hiện tại (%).")]
        public float Humidity = 75f;
        
        [Tooltip("Tốc độ gió (km/h).")]
        public float WindSpeed = 10f;
        
        [Tooltip("Cường độ mưa (0f - 1f).")]
        public float RainIntensity = 0f;

        [Header("--- BIẾN SỐ BÃO LŨ ---")]
        [Tooltip("Mực nước lũ hiện tại (mét). 0m = bình thường, >1.5m = ngập lút nông trại, rút lên nóc nhà.")]
        public float FloodLevel = 0f;
        
        [Tooltip("Tốc độ dâng của nước lũ mỗi giờ game khi đang có bão.")]
        [SerializeField] private float floodRiseRatePerHour = 0.15f;

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
            // Đăng ký nhận sự kiện chuyển giai đoạn game để đồng bộ thời tiết
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPhaseChanged += HandleGamePhaseWeatherChange;
                GameManager.Instance.OnHourChanged += OnHourTick;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPhaseChanged -= HandleGamePhaseWeatherChange;
                GameManager.Instance.OnHourChanged -= OnHourTick;
            }
        }

        private void Update()
        {
            // Cập nhật các chỉ số thời tiết gián tiếp theo thời gian thực
            SimulateDynamicFluctuations();
        }

        /// <summary>
        /// Mô phỏng sự dao động nhiệt độ tự nhiên giữa ngày và đêm, và ảnh hưởng lên người chơi.
        /// </summary>
        private void SimulateDynamicFluctuations()
        {
            if (GameManager.Instance == null) return;

            float hour = GameManager.Instance.CurrentHour;
            
            // Dao động nhiệt sinh học (nóng nhất lúc 13-14h, lạnh nhất lúc 4h sáng)
            float baseTemp = 28f;
            switch (currentVisualWeather)
            {
                case WeatherType.OnDinh:
                    baseTemp = 28f + Mathf.Sin((hour - 8f) * Mathf.PI / 12f) * 4f; // 24°C - 32°C
                    Humidity = 70f - Mathf.Sin((hour - 8f) * Mathf.PI / 12f) * 15f;
                    break;

                case WeatherType.GioLao:
                    // Gió Lào kéo dài sang cả đêm, nhiệt độ ban ngày cực cao (40-42 độ C)
                    baseTemp = 36f + Mathf.Sin((hour - 8f) * Mathf.PI / 12f) * 6f; // 30°C - 42°C
                    Humidity = 35f - Mathf.Sin((hour - 8f) * Mathf.PI / 12f) * 15f; // Khô nóng cực hạn (xuống dưới 20% ẩm)
                    WindSpeed = 30f + Mathf.PingPong(Time.time, 15f);
                    break;

                case WeatherType.BaoLu:
                    baseTemp = 22f + Mathf.PingPong(Time.time * 0.1f, 3f); // Mưa bão lạnh ẩm
                    Humidity = 95f;
                    WindSpeed = 70f + Mathf.PingPong(Time.time * 0.5f, 40f); // Gió giật cấp 8 - cấp 11
                    break;
            }

            Temperature = baseTemp;

            // Áp dụng stress thời tiết lên Player
            ApplyWeatherStressToPlayer();
        }

        /// <summary>
        /// Chuyển đổi trạng thái thời tiết chủ đạo khi GameManager báo chuyển Phase.
        /// </summary>
        private void HandleGamePhaseWeatherChange(GamePhase newPhase)
        {
            switch (newPhase)
            {
                case GamePhase.LapNghiep:
                    currentVisualWeather = WeatherType.OnDinh;
                    RainIntensity = 0f;
                    FloodLevel = 0f;
                    break;

                case GamePhase.GioLao:
                    currentVisualWeather = WeatherType.GioLao;
                    RainIntensity = 0f;
                    FloodLevel = 0f;
                    break;

                case GamePhase.MuaBao:
                    currentVisualWeather = WeatherType.BaoLu;
                    RainIntensity = 0.9f;
                    break;

                case GamePhase.PhuSa:
                    currentVisualWeather = WeatherType.OnDinh;
                    RainIntensity = 0.05f;
                    // Sau lũ nước rút dần từ từ
                    break;
            }
            Debug.Log($"[WEATHER MANAGER] Đồng bộ thời tiết sang: {currentVisualWeather.ToString()}");
        }

        /// <summary>
        /// Tick mỗi giờ game để cập nhật nước lũ hoặc dự báo thời tiết.
        /// </summary>
        private void OnHourTick(int currentHour)
        {
            if (GameManager.Instance == null) return;

            // Nếu đang trong mùa bão lũ, nước lũ dâng đều đặn mỗi giờ
            if (GameManager.Instance.CurrentPhase == GamePhase.MuaBao)
            {
                FloodLevel += floodRiseRatePerHour;
                Debug.Log($"[WEATHER] Nước lũ dâng! Mực nước hiện tại: {FloodLevel:F2} mét.");
            }
            // Nếu đã sang mùa Phù Sa tái thiết, nước rút nhanh
            else if (GameManager.Instance.CurrentPhase == GamePhase.PhuSa && FloodLevel > 0f)
            {
                FloodLevel = Mathf.Max(0f, FloodLevel - 0.25f);
                Debug.Log($"[WEATHER] Nước lũ rút. Mực nước còn lại: {FloodLevel:F2} mét.");
            }
        }

        /// <summary>
        /// Tính toán và áp các tác động sinh học trực tiếp từ thời tiết lên người chơi.
        /// </summary>
        private void ApplyWeatherStressToPlayer()
        {
            if (PlayerStats.Instance == null) return;

            // Ảnh hưởng của Gió Lào nóng cháy
            if (currentVisualWeather == WeatherType.GioLao)
            {
                if (Temperature > 38f)
                {
                    // Tăng stress nhiệt độ nếu đứng ngoài trời làm việc
                    PlayerStats.Instance.ApplyHeatStress(0.8f * Time.deltaTime);
                    PlayerStats.Instance.ApplyColdStress(-2f * Time.deltaTime); // Xóa stress lạnh
                }
            }
            // Ảnh hưởng của mưa lạnh bão bùng
            else if (currentVisualWeather == WeatherType.BaoLu)
            {
                PlayerStats.Instance.ApplyColdStress(0.6f * Time.deltaTime);
                PlayerStats.Instance.ApplyHeatStress(-2f * Time.deltaTime); // Xóa stress nóng
            }
            else
            {
                // Điều kiện mát mẻ, giảm dần các stress về 0
                PlayerStats.Instance.ApplyHeatStress(-1f * Time.deltaTime);
                PlayerStats.Instance.ApplyColdStress(-1f * Time.deltaTime);
            }
        }
    }
}
