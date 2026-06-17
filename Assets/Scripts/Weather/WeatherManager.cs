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

        private float targetFloodLevel = 0f;
        private float targetTemperature = 28f;
        private float targetHumidity = 75f;
        private float targetWindSpeed = 10f;
        private float targetRainIntensity = 0f;
        private GameObject waterPlane3D;

        [Header("--- HIỆU ỨNG HẠT MƯA ---")]
        private ParticleSystem rainParticles;
        private GameObject rainParticlesObj;

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

            // Thiết lập hạt mưa
            SetupRainParticles();

            // Thiết lập mặt nước 3D
            Setup3DWaterPlane();
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

            // Nội suy mượt mà các thông số thời tiết
            Temperature = Mathf.Lerp(Temperature, targetTemperature, Time.deltaTime * 0.3f);
            Humidity = Mathf.Lerp(Humidity, targetHumidity, Time.deltaTime * 0.3f);
            WindSpeed = Mathf.Lerp(WindSpeed, targetWindSpeed, Time.deltaTime * 0.3f);
            RainIntensity = Mathf.Lerp(RainIntensity, targetRainIntensity, Time.deltaTime * 0.3f);
            FloodLevel = Mathf.Lerp(FloodLevel, targetFloodLevel, Time.deltaTime * 0.2f);

            // Cập nhật vị trí và cường độ hạt mưa
            UpdateRainParticles();

            // Cập nhật vị trí mặt nước 3D
            Update3DWaterPlane();
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
            float baseHum = 75f;
            float baseWind = 10f;
            switch (currentVisualWeather)
            {
                case WeatherType.OnDinh:
                    baseTemp = 28f + Mathf.Sin((hour - 8f) * Mathf.PI / 12f) * 4f; // 24°C - 32°C
                    baseHum = 70f - Mathf.Sin((hour - 8f) * Mathf.PI / 12f) * 15f;
                    baseWind = 10f;
                    break;

                case WeatherType.GioLao:
                    // Gió Lào kéo dài sang cả đêm, nhiệt độ ban ngày cực cao (40-42 độ C)
                    baseTemp = 36f + Mathf.Sin((hour - 8f) * Mathf.PI / 12f) * 6f; // 30°C - 42°C
                    baseHum = 25f - Mathf.Sin((hour - 8f) * Mathf.PI / 12f) * 10f; // Khô nóng cực hạn (xuống dưới 20% ẩm)
                    baseWind = 30f + Mathf.PingPong(Time.time, 15f);
                    break;

                case WeatherType.BaoLu:
                    baseTemp = 22f + Mathf.PingPong(Time.time * 0.1f, 3f); // Mưa bão lạnh ẩm
                    baseHum = 95f;
                    baseWind = 70f + Mathf.PingPong(Time.time * 0.5f, 40f); // Gió giật cấp 8 - cấp 11
                    break;
            }

            targetTemperature = baseTemp;
            targetHumidity = baseHum;
            targetWindSpeed = baseWind;

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
                    targetRainIntensity = 0f;
                    targetFloodLevel = 0f;
                    break;

                case GamePhase.GioLao:
                    currentVisualWeather = WeatherType.GioLao;
                    targetRainIntensity = 0f;
                    targetFloodLevel = 0f;
                    break;

                case GamePhase.MuaBao:
                    currentVisualWeather = WeatherType.BaoLu;
                    targetRainIntensity = 0.9f;
                    break;

                case GamePhase.PhuSa:
                    currentVisualWeather = WeatherType.OnDinh;
                    targetRainIntensity = 0.05f;
                    // Sau lũ nước rút từ từ
                    break;
            }
            Debug.Log($"[WEATHER MANAGER] Đồng bộ thời tiết sang: {currentVisualWeather.ToString()}");
        }

        private void OnHourTick(int currentHour)
        {
            if (GameManager.Instance == null) return;

            // Nếu đang trong mùa bão lũ, nước lũ dâng đều đặn mỗi giờ
            if (GameManager.Instance.CurrentPhase == GamePhase.MuaBao)
            {
                targetFloodLevel += floodRiseRatePerHour;
                Debug.Log($"[WEATHER] Nước lũ dâng mục tiêu: {targetFloodLevel:F2} mét.");
            }
            // Nếu đã sang mùa Phù Sa tái thiết, nước rút nhanh
            else if (GameManager.Instance.CurrentPhase == GamePhase.PhuSa && targetFloodLevel > 0f)
            {
                targetFloodLevel = Mathf.Max(0f, targetFloodLevel - 0.25f);
                Debug.Log($"[WEATHER] Nước lũ rút mục tiêu: {targetFloodLevel:F2} mét.");
            }
        }

        /// <summary>
        /// Đặt trực tiếp mực nước lũ cho mục đích gỡ lỗi/kiểm thử nhanh.
        /// </summary>
        public void DebugSetFloodLevel(float level)
        {
            targetFloodLevel = level;
            FloodLevel = level; // Đặt cả hai để snap ngay lập tức
            Debug.Log($"[WEATHER] Debug đặt mực nước lũ thành: {level} mét.");
        }

        private void Setup3DWaterPlane()
        {
            if (waterPlane3D != null) return;
            
            // Tạo một Plane 3D làm mặt nước
            waterPlane3D = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterPlane3D.name = "3D_Water_Plane";
            
            // Xóa Collider để tránh va chạm vật lý cản trở Player
            Collider col = waterPlane3D.GetComponent<Collider>();
            if (col != null) Destroy(col);
            
            // Đặt kích thước bao phủ khu vực ruộng vườn (8f trên Plane tương ứng 80 đơn vị thế giới)
            waterPlane3D.transform.localScale = new Vector3(8f, 1f, 8f);
            
            // Tạo material màu xanh nước biển trong suốt sử dụng shader Sprites/Default để tương thích mọi Render Pipeline
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader == null) spriteShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            if (spriteShader == null) spriteShader = Shader.Find("Standard");
            
            Material waterMat = new Material(spriteShader);
            // Xanh lam trong suốt, độ mờ 0.65f để nhìn thấu xuống đáy ruộng vườn
            waterMat.color = new Color(0.12f, 0.42f, 0.72f, 0.65f);
            
            waterPlane3D.GetComponent<MeshRenderer>().material = waterMat;
            
            // Đặt vị trí mặc định dưới mặt đất (Y = -1.0f)
            waterPlane3D.transform.position = new Vector3(4.25f, -1.0f, -5.25f);
        }

        private void Update3DWaterPlane()
        {
            if (waterPlane3D == null) return;

            // Nước dâng theo FloodLevel. Khi FloodLevel = 0, nước nằm ở Y = -1f (ẩn dưới đất)
            // Khi nước dâng, Y = -0.05f + FloodLevel (bắt đầu ngập từ Y = 0f trở lên)
            float waterY = (FloodLevel <= 0.01f) ? -1.0f : (-0.05f + FloodLevel);
            waterPlane3D.transform.position = new Vector3(4.25f, waterY, -5.25f);
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

        #region PHÂN HỆ HIỆU ỨNG HẠT MƯA BÃO (RAIN PARTICLES SYSTEM)

        private void SetupRainParticles()
        {
            rainParticlesObj = new GameObject("RainParticles", typeof(ParticleSystem));
            rainParticlesObj.transform.SetParent(this.transform);

            rainParticles = rainParticlesObj.GetComponent<ParticleSystem>();

            // Stop the system before configuring main module properties to avoid warnings
            rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // Cấu hình Particle System chính
            var main = rainParticles.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = 1.2f;
            main.startSpeed = 16f;
            main.startSize = 0.12f;
            main.gravityModifier = 1.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 800;

            // Hình dáng vùng phát (Shape)
            var shape = rainParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(25f, 1f, 1f); 
            shape.rotation = new Vector3(0f, 0f, 12f); // Bắn hạt nghiêng nhẹ theo chiều gió

            // Cấu hình di chuyển (Velocity over Lifetime) để tạo vệt mưa chéo nghiêng trái
            var velocity = rainParticles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = -5f; // Gió thổi chéo trái

            // Thiết lập tốc độ sinh hạt mặc định là 0
            var emission = rainParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;

            // Màu sắc hạt mưa (Trắng xám nhẹ trong suốt)
            var colorOverLifetime = rainParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.8f, 0.88f, 0.95f), 0f), new GradientColorKey(new Color(0.75f, 0.8f, 0.85f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.35f, 0f), new GradientAlphaKey(0.08f, 1f) }
            );
            colorOverLifetime.color = grad;

            // Thiết lập Renderer để kéo dãn hạt mưa thành sọc dài
            var renderer = rainParticlesObj.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 3f;
            renderer.velocityScale = 0.1f;
            
            // Sử dụng Material Sprites-Default mặc định để hạt mưa sắc nét
            #if UNITY_2022_1_OR_NEWER
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            #else
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            #endif

            // Bắt đầu chạy hệ thống phát
            rainParticles.Play();
        }

        private void UpdateRainParticles()
        {
            if (rainParticlesObj == null || rainParticles == null) return;

            // Đồng bộ vị trí bộ phát mưa nằm phía trên đầu Camera
            if (Camera.main != null)
            {
                // Đặt bộ phát mưa lệch nhẹ sang bên phải và phía trên Camera
                Vector3 camPos = Camera.main.transform.position;
                rainParticlesObj.transform.position = new Vector3(camPos.x + 3f, camPos.y + 6f, -2f);
            }

            // Đồng bộ cường độ hạt mưa rơi theo RainIntensity
            var emission = rainParticles.emission;
            float targetRate = RainIntensity * 400f; // Mưa bão to thì rơi 400 hạt/giây
            emission.rateOverTime = targetRate;
        }

        #endregion
    }
}
