using UnityEngine;
using SownInStone.Core;

namespace SownInStone.Weather
{
    public enum WeatherType
    {
        OnDinh,     // Thời tiết nắng mát lập nghiệp ban đầu
        NangNong,   // Nắng hè gay gắt
        GioLao,     // Gió Lào khô nóng
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

        [Header("--- HỆ THỐNG ÁNH SÁNG & SƯƠNG MÙ ---")]
        private Light directionalLight;
        private float targetLightIntensity = 1.2f;
        private Color targetLightColor = new Color(1f, 0.95f, 0.8f);
        private Color targetAmbientColor = new Color(0.2f, 0.22f, 0.25f);
        private float targetFogStart = 45f;
        private float targetFogEnd = 90f;
        private Color targetFogColor = new Color(0.75f, 0.85f, 0.95f);

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

            // Tìm Directional Light
            FindDirectionalLight();

            // Đồng bộ trạng thái run lạnh ban đầu theo thời tiết hiện tại
            SetAllCharactersShivering(currentVisualWeather == WeatherType.BaoLu);
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

            // Cập nhật ánh sáng và sương mù khí hậu
            UpdateWeatherAtmospherics();
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


                case WeatherType.MuaGiong:
                    // Mưa giông ẩm ướt nhiệt độ vừa phải, gió mát nhưng giật nhẹ chuẩn bị bão
                    baseTemp = 26f + Mathf.Sin((hour - 8f) * Mathf.PI / 12f) * 2f; // 24°C - 28°C
                    baseHum = 85f;
                    baseWind = 20f + Mathf.PingPong(Time.time, 10f); // Gió giật nhẹ chuẩn bị bão
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
            bool isMenuOpen = (FrameworkMainMenuUI.Instance != null && FrameworkMainMenuUI.Instance.IsMenuOpen);

            switch (newPhase)
            {
                case GamePhase.LapNghiep:
                    currentVisualWeather = WeatherType.OnDinh;
                    targetRainIntensity = 0f;
                    targetFloodLevel = 0f;
                    if (!isMenuOpen) SownInStone.Audio.AudioManager.Instance?.PlayAmbient("ambient_rural", 0.4f);
                    SetAllCharactersShivering(false);
                    break;


                case GamePhase.ChuanBiBao:
                    currentVisualWeather = WeatherType.MuaGiong;
                    targetRainIntensity = 0.3f;
                    targetFloodLevel = 0f;
                    if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                    {
                        if (TutorialManager.Instance.currentStage == TutorialManager.TutorialStage.RescuingNPCs)
                        {
                            targetFloodLevel = 1.2f;
                        }
                    }
                    if (!isMenuOpen) SownInStone.Audio.AudioManager.Instance?.PlayAmbient("ambient_storm", 0.5f);
                    SetAllCharactersShivering(false);
                    break;

                case GamePhase.MuaBao:
                    currentVisualWeather = WeatherType.BaoLu;
                    targetRainIntensity = 0.9f;
                    if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                    {
                        if (TutorialManager.Instance.currentStage == TutorialManager.TutorialStage.RoofSurvivalSharing)
                        {
                            targetFloodLevel = 2.0f;
                        }
                    }
                    if (!isMenuOpen) SownInStone.Audio.AudioManager.Instance?.PlayAmbient("ambient_storm", 0.9f);
                    if (SownInStone.UI.SurvivalUIManager.Instance != null)
                    {
                        SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("⚠️ CẢNH BÁO BẢO LŨ! Hãy bọc Màng Nilon phủ ruộng, xếp Bao Cát & Tấm Chắn đê cứu xóm làng!");
                    }
                    SetAllCharactersShivering(true);
                    break;

                case GamePhase.PhuSa:
                    currentVisualWeather = WeatherType.OnDinh;
                    targetRainIntensity = 0.05f;
                    if (!isMenuOpen) SownInStone.Audio.AudioManager.Instance?.PlayAmbient("ambient_rural", 0.4f);
                    SetAllCharactersShivering(false);
                    break;
            }
            Debug.Log($"[WEATHER MANAGER] Đồng bộ thời tiết sang: {currentVisualWeather.ToString()}");
        }

        /// <summary>
        /// Làm mới lại âm thanh môi trường dông bão theo phase hiện tại.
        /// </summary>
        public void RefreshWeatherAmbient()
        {
            if (GameManager.Instance != null)
            {
                HandleGamePhaseWeatherChange(GameManager.Instance.CurrentPhase);
            }
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

        public void SetFloodLevelDirectly(float level)
        {
            targetFloodLevel = level;
            FloodLevel = level;
            Update3DWaterPlane();
            Debug.Log($"[WEATHER] Đặt trực tiếp mực nước lũ thành: {level} mét.");
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
            
            // Đặt vị trí mặc định dưới mặt đất (Y = -1.5f)
            waterPlane3D.transform.position = new Vector3(4.25f, -1.5f, -5.25f);
        }

        private void Update3DWaterPlane()
        {
            if (waterPlane3D == null) return;

            // Nước dâng theo FloodLevel. Khi FloodLevel = 0, nước nằm ở Y = -1.5f (ẩn dưới đất)
            // Khi nước dâng, Y = -0.05f + FloodLevel (bắt đầu ngập từ Y = 0f trở lên)
            float waterY = (FloodLevel <= 0.01f) ? -1.5f : (-0.05f + FloodLevel);
            waterPlane3D.transform.position = new Vector3(4.25f, waterY, -5.25f);
        }

        /// <summary>
        /// Tính toán và áp các tác động sinh học trực tiếp từ thời tiết lên người chơi.
        /// </summary>
        private void ApplyWeatherStressToPlayer()
        {
            if (PlayerStats.Instance == null) return;

            // Ảnh hưởng của Gió Lào nóng cháy
            // Ảnh hưởng của mưa lạnh bão bùng
            if (currentVisualWeather == WeatherType.BaoLu)
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

            // Xoay emitter 90° quanh X để hạt phát thẳng xuống (-Y)
            rainParticlesObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            rainParticles = rainParticlesObj.GetComponent<ParticleSystem>();

            // Stop the system before configuring main module properties to avoid warnings
            rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // Cấu hình Particle System chính
            var main = rainParticles.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = 1.5f;
            main.startSpeed = 18f;       // Tốc độ rơi xuống
            main.startSize = 0.06f;
            main.gravityModifier = 0f;   // Tắt gravity của Unity — tốc độ rơi do startSpeed + hướng -Y điều khiển
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 1000;

            // Hình dáng vùng phát: Box rộng ngang, phẳng, hạt bắn theo -Z (tức là xuống đất sau khi xoay emitter)
            var shape = rainParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(30f, 30f, 0.1f); // Rộng ngang bao quanh camera
            shape.rotation = Vector3.zero;              // Không xoay thêm — đã xoay qua transform

            // Thêm gió nhẹ ngang để mưa có chút nghiêng tự nhiên
            var velocity = rainParticles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new UnityEngine.ParticleSystem.MinMaxCurve(-1.5f); // Gió nhẹ sang trái
            velocity.y = new UnityEngine.ParticleSystem.MinMaxCurve(0f);
            velocity.z = new UnityEngine.ParticleSystem.MinMaxCurve(0f);

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
                new GradientAlphaKey[] { new GradientAlphaKey(0.45f, 0f), new GradientAlphaKey(0.05f, 1f) }
            );
            colorOverLifetime.color = grad;

            // Thiết lập Renderer: Stretch theo hướng di chuyển (tạo vệt mưa dài rơi xuống)
            var renderer = rainParticlesObj.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 2.5f;
            renderer.velocityScale = 0.08f;
            renderer.cameraVelocityScale = 0f;
            
            // Sử dụng Material Sprites-Default mặc định để hạt mưa sắc nét
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            // Bắt đầu chạy hệ thống phát
            rainParticles.Play();
        }

        private void UpdateRainParticles()
        {
            if (rainParticlesObj == null || rainParticles == null) return;

            // Đồng bộ vị trí bộ phát mưa nằm phía trên đầu Camera
            if (Camera.main != null)
            {
                // Đặt bộ phát mưa lệch nhẹ sang bên phải và phía trên Camera, đồng bộ theo Z của camera
                Vector3 camPos = Camera.main.transform.position;
                rainParticlesObj.transform.position = new Vector3(camPos.x + 3f, camPos.y + 12f, camPos.z - 2f);
            }

            // Đồng bộ cường độ hạt mưa rơi theo RainIntensity
            var emission = rainParticles.emission;
            float targetRate = RainIntensity * 400f; // Mưa bão to thì rơi 400 hạt/giây
            emission.rateOverTime = targetRate;
        }

        #endregion

        private void FindDirectionalLight()
        {
            if (directionalLight == null)
            {
                GameObject lightGo = GameObject.Find("Directional Light");
                if (lightGo != null)
                {
                    directionalLight = lightGo.GetComponent<Light>();
                }
                
                if (directionalLight == null)
                {
                    Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Include);
                    foreach (var l in lights)
                    {
                        if (l.type == LightType.Directional)
                        {
                            directionalLight = l;
                            break;
                        }
                    }
                }
            }
        }

        private void UpdateWeatherAtmospherics()
        {
            if (directionalLight == null) FindDirectionalLight();

            float baseLightIntensity = 1.2f;
            Color baseLightColor = new Color(1f, 0.95f, 0.8f);
            Color baseAmbientColor = new Color(0.2f, 0.22f, 0.25f);
            float baseFogStart = 45f;
            float baseFogEnd = 90f;
            Color baseFogColor = new Color(0.75f, 0.85f, 0.95f);

            bool isInside = (PlayerController.Instance != null && PlayerController.Instance.IsInsideHouse);

            if (isInside)
            {
                baseLightIntensity = 0.5f;
                baseLightColor = new Color(0.8f, 0.85f, 0.9f);
                baseAmbientColor = new Color(0.15f, 0.15f, 0.18f);
                baseFogStart = 100f;
                baseFogEnd = 200f; 
                baseFogColor = new Color(0.1f, 0.1f, 0.1f);
            }
            else
            {
                switch (currentVisualWeather)
                {
                    case WeatherType.OnDinh:
                        baseLightIntensity = 1.3f;
                        baseLightColor = new Color(1f, 0.95f, 0.85f);
                        baseAmbientColor = new Color(0.25f, 0.27f, 0.3f);
                        baseFogStart = 50f;
                        baseFogEnd = 95f;
                        baseFogColor = new Color(0.7f, 0.82f, 0.95f);
                        break;

                    case WeatherType.MuaGiong:
                        baseLightIntensity = 0.6f;
                        baseLightColor = new Color(0.65f, 0.68f, 0.72f);
                        baseAmbientColor = new Color(0.18f, 0.2f, 0.22f);
                        baseFogStart = 25f;
                        baseFogEnd = 75f;
                        baseFogColor = new Color(0.52f, 0.55f, 0.58f);
                        break;

                    case WeatherType.BaoLu:
                        baseLightIntensity = 0.2f;
                        baseLightColor = new Color(0.4f, 0.42f, 0.45f);
                        baseAmbientColor = new Color(0.1f, 0.12f, 0.14f);
                        baseFogStart = 10f;
                        baseFogEnd = 50f; 
                        baseFogColor = new Color(0.22f, 0.24f, 0.26f);
                        break;
                }
            }

            // Tính toán hệ số tối dần khi về đêm (Từ 18h đến 5h sáng)
            float nightMultiplier = 1f;
            if (GameManager.Instance != null && !isInside)
            {
                float hour = GameManager.Instance.CurrentHour;
                if (hour >= 18f && hour < 20f)
                {
                    nightMultiplier = Mathf.Lerp(1f, 0.15f, (hour - 18f) / 2f);
                }
                else if (hour >= 20f || hour <= 4f)
                {
                    nightMultiplier = 0.15f;
                }
                else if (hour > 4f && hour <= 6f)
                {
                    nightMultiplier = Mathf.Lerp(0.15f, 1f, (hour - 4f) / 2f);
                }
                
                baseLightIntensity *= nightMultiplier;
                baseAmbientColor *= nightMultiplier;
                baseFogColor *= (0.5f + nightMultiplier * 0.5f); // Làm sương mù tối hơn vào ban đêm
            }

            targetLightIntensity = baseLightIntensity;
            targetLightColor = baseLightColor;
            targetAmbientColor = baseAmbientColor;
            targetFogStart = baseFogStart;
            targetFogEnd = baseFogEnd;
            targetFogColor = baseFogColor;

            if (directionalLight != null)
            {
                directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, targetLightIntensity, Time.deltaTime * 1.5f);
                directionalLight.color = Color.Lerp(directionalLight.color, targetLightColor, Time.deltaTime * 1.5f);
            }

            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, targetAmbientColor, Time.deltaTime * 1.5f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = Mathf.Lerp(RenderSettings.fogStartDistance, targetFogStart, Time.deltaTime * 1.5f);
            RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, targetFogEnd, Time.deltaTime * 1.5f);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetFogColor, Time.deltaTime * 1.5f);

            if (Camera.main != null)
            {
                float targetClip = RenderSettings.fogEndDistance + 5f;
                if (isInside) targetClip = 150f; 
                Camera.main.farClipPlane = Mathf.Lerp(Camera.main.farClipPlane, targetClip, Time.deltaTime * 2.0f);
            }
        }

        private void SetAllCharactersShivering(bool shivering)
        {
            // Thiết lập trạng thái run lạnh cho Player
            if (PlayerController.Instance != null)
            {
                Animator playerAnim = PlayerController.Instance.GetComponent<Animator>();
                if (playerAnim == null) playerAnim = PlayerController.Instance.GetComponentInChildren<Animator>();
                if (playerAnim != null)
                {
                    playerAnim.SetBool("isShivering", shivering);
                }
            }

            // Thiết lập trạng thái run lạnh cho tất cả các NPC trong Scene
#if UNITY_2023_1_OR_NEWER
            var npcs = FindObjectsByType<SownInStone.Community.NPCCharacter>();
#else
            var npcs = FindObjectsOfType<SownInStone.Community.NPCCharacter>();
#endif
            foreach (var npc in npcs)
            {
                if (npc != null)
                {
                    npc.SetShivering(shivering);
                }
            }
            Debug.Log($"[WEATHER] SetAllCharactersShivering: {shivering}");
        }
    }
}
