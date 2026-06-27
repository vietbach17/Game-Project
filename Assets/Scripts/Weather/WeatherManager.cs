using UnityEngine;
using SownInStone.Core;

namespace SownInStone.Weather
{
    public enum WeatherType
    {
        OnDinh,
        NangNong,
        GioLao,
        MuaGiong,
        BaoLu
    }

    public class WeatherManager : MonoBehaviour
    {
        public static WeatherManager Instance { get; private set; }

        [Header("Disaster Assets")]
        [SerializeField] private GameObject waterPlanePrefab;
        [SerializeField] private Transform disasterContainer;

        [Header("Live Weather Stats")]
        [SerializeField] private float temperature = 28.5f;
        [SerializeField] private float humidity = 65f;
        [SerializeField] private float rainIntensity = 0f;
        [SerializeField] private float floodLevel = -1.5f;
        [SerializeField] private float windSpeed = 8f;
        [SerializeField] public WeatherType currentVisualWeather = WeatherType.OnDinh;

        [Header("Lerp Speeds")]
        [SerializeField] private float floodRiseSpeed = 0.15f;
        [SerializeField] private float weatherChangeSpeed = 0.35f;

        private GameObject activeWaterPlane;
        private float targetTemperature = 28.5f;
        private float targetHumidity = 60f;
        private float targetRainIntensity = 0f;
        private float targetFloodLevel = -1.5f;
        private float targetWindSpeed = 8f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (disasterContainer != null && activeWaterPlane == null)
            {
                SpawnFloodWaterPlane();
            }

            GameManager.OnPhaseChanged += HandleWeatherPhaseChanged;
        }

        private void OnDestroy()
        {
            GameManager.OnPhaseChanged -= HandleWeatherPhaseChanged;
        }

        private void Update()
        {
            temperature = Mathf.Lerp(temperature, targetTemperature, Time.deltaTime * weatherChangeSpeed);
            humidity = Mathf.Lerp(humidity, targetHumidity, Time.deltaTime * weatherChangeSpeed);
            rainIntensity = Mathf.Lerp(rainIntensity, targetRainIntensity, Time.deltaTime * weatherChangeSpeed);
            windSpeed = Mathf.Lerp(windSpeed, targetWindSpeed, Time.deltaTime * weatherChangeSpeed);

            if (activeWaterPlane != null)
            {
                float nextY = Mathf.Lerp(activeWaterPlane.transform.position.y, targetFloodLevel, Time.deltaTime * floodRiseSpeed);
                activeWaterPlane.transform.position = new Vector3(
                    activeWaterPlane.transform.position.x,
                    nextY,
                    activeWaterPlane.transform.position.z
                );

                floodLevel = nextY;
            }

            UpdateSoilEvaporationParameters();
        }

        private void HandleWeatherPhaseChanged(GamePhase activePhase)
        {
            switch (activePhase)
            {
                case GamePhase.LapNghiep:
                    currentVisualWeather = WeatherType.OnDinh;
                    targetTemperature = 28.5f;
                    targetHumidity = 60f;
                    targetRainIntensity = 0f;
                    targetWindSpeed = 8f;
                    targetFloodLevel = -1.5f;
                    break;

                case GamePhase.GioLao:
                    currentVisualWeather = WeatherType.GioLao;
                    targetTemperature = 42.0f;
                    targetHumidity = 35f;
                    targetRainIntensity = 0f;
                    targetWindSpeed = 32f;
                    targetFloodLevel = -1.5f;
                    break;

                case GamePhase.ChuanBiBao:
                    currentVisualWeather = WeatherType.MuaGiong;
                    targetTemperature = 24.0f;
                    targetHumidity = 85f;
                    targetRainIntensity = 0.25f;
                    targetWindSpeed = 45f;
                    targetFloodLevel = -1.5f;
                    break;

                case GamePhase.MuaBao:
                    currentVisualWeather = WeatherType.BaoLu;
                    targetTemperature = 19.5f;
                    targetHumidity = 100f;
                    targetRainIntensity = 1f;
                    targetWindSpeed = 95f;
                    targetFloodLevel = 1.85f;
                    break;

                case GamePhase.PhuSa:
                    currentVisualWeather = WeatherType.OnDinh;
                    targetTemperature = 26.0f;
                    targetHumidity = 75f;
                    targetRainIntensity = 0f;
                    targetWindSpeed = 12f;
                    targetFloodLevel = -1.5f;
                    break;
            }
        }

        private void SpawnFloodWaterPlane()
        {
            if (waterPlanePrefab == null) return;

            activeWaterPlane = Instantiate(waterPlanePrefab, new Vector3(0f, floodLevel, 0f), Quaternion.identity);
            if (disasterContainer != null)
            {
                activeWaterPlane.transform.SetParent(disasterContainer);
            }

            MeshCollider col = activeWaterPlane.GetComponent<MeshCollider>();
            if (col != null) col.enabled = false;
        }

        private void UpdateSoilEvaporationParameters() { }

        public void RefreshWeatherAmbient()
        {
            HandleWeatherPhaseChanged(GameManager.Instance != null ? GameManager.Instance.CurrentPhase : GamePhase.LapNghiep);
        }

        public float CurrentTemperature => temperature;
        public float CurrentHumidity => humidity;
        public float CurrentRainIntensity => rainIntensity;
        public float CurrentFloodLevel => floodLevel;
        public float Temperature => temperature;
        public float Humidity => humidity;
        public float RainIntensity => rainIntensity;
        public float FloodLevel => floodLevel;
        public float WindSpeed => windSpeed;
    }
}
