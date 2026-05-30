using UnityEngine;
using SownInStone.Core;
using SownInStone.Weather;

namespace SownInStone.Agriculture
{
    public enum SoilQuality
    {
        BacMau,     // Đất cát cát bạc màu ban đầu
        TrungBinh,   // Đất sau khi được nhặt đá và ủ phân nhẹ
        PhuSa       // Đất bồi đắp phù sa sau lũ cực kỳ màu mỡ
    }

    /// <summary>
    /// Quản lý trạng thái vật lý và sinh hóa của từng ô đất nông trại.
    /// Quyết định xem cây trồng trên ô này có lớn được không.
    /// </summary>
    public class SoilCell : MonoBehaviour
    {
        [Header("--- THÀNH PHẦN ĐẤT ĐAI ---")]
        public SoilQuality quality = SoilQuality.BacMau;
        
        [Range(0f, 100f)]
        [Tooltip("Độ ẩm hiện tại của đất (%). Tưới nước để tăng. Thời tiết/Gió Lào làm bốc hơi.")]
        public float Moisture = 10f;

        [Range(0f, 100f)]
        [Tooltip("Độ dinh dưỡng hiện tại (%). Bón phân hữu cơ để tăng. Cây hút khi lớn lên.")]
        public float Nutrients = 15f;

        [Range(0f, 100f)]
        [Tooltip("Mật độ sỏi đá cản trở (%). Càng cao thì cây lớn càng chậm. Cần nhặt đá cải tạo.")]
        public float RockDensity = 80f; // Ban đầu sỏi đá rất nhiều ("Đất cày lên sỏi đá")

        [Header("--- THÀNH PHẦN CÂY TRỒNG ---")]
        [Tooltip("Cây trồng đang ký sinh trên ô đất này (nếu có).")]
        public CropInstance plantedCrop;

        [Header("--- HIỆU ỨNG HÌNH ẢNH ---")]
        [SerializeField] private SpriteRenderer soilSpriteRenderer;
        [SerializeField] private Sprite drySoilSprite;
        [SerializeField] private Sprite wetSoilSprite;
        [SerializeField] private Sprite siltSoilSprite; // Sprite đất phù sa bồi đắp màu đậm

        private void Start()
        {
            // Lắng nghe ngày mới trôi qua để cập nhật sinh học của đất
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDayChanged += OnNewDay;
            }
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDayChanged -= OnNewDay;
            }
        }

        private void Update()
        {
            // Bốc hơi nước liên tục do thời tiết thực tế
            SimulateWaterEvaporation();
        }

        /// <summary>
        /// Mô phỏng sự bay hơi nước của đất dựa vào nhiệt độ và độ ẩm không khí của WeatherManager.
        /// </summary>
        private void SimulateWaterEvaporation()
        {
            if (WeatherManager.Instance == null) return;

            // Nếu ngập lũ thì độ ẩm luôn đạt tối đa 100%
            if (WeatherManager.Instance.FloodLevel > 0.1f)
            {
                Moisture = 100f;
                return;
            }

            // Nếu mưa bão chưa ngập lũ nhưng có mưa, đất tự thấm nước
            if (WeatherManager.Instance.RainIntensity > 0.1f)
            {
                Moisture = Mathf.Clamp(Moisture + WeatherManager.Instance.RainIntensity * 15f * Time.deltaTime, 0f, 100f);
                return;
            }

            // Bay hơi nước tỷ lệ thuận với nhiệt độ không khí
            float evaporationSpeed = 0.5f; // Tốc độ bay hơi cơ bản
            if (WeatherManager.Instance.currentVisualWeather == WeatherType.GioLao)
            {
                evaporationSpeed = 3.5f; // Gió Lào thổi bay hơi nước cực nhanh!
            }

            float tempFactor = Mathf.Max(1f, WeatherManager.Instance.Temperature / 30f);
            Moisture = Mathf.Clamp(Moisture - evaporationSpeed * tempFactor * Time.deltaTime, 0f, 100f);
            
            UpdateVisuals();
        }

        /// <summary>
        /// Xử lý cập nhật đất đai mỗi ngày mới trôi qua.
        /// </summary>
        private void OnNewDay(int newDay)
        {
            // Nếu ô đất bị ngập lụt lâu trong mùa bão, chất dinh dưỡng ban đầu bị rửa trôi
            // Nhưng khi sang giai đoạn Phù Sa, đất ngập lụt được bồi đắp chất hữu cơ cực cao
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CurrentPhase == GamePhase.PhuSa && WeatherManager.Instance.FloodLevel > 0f)
                {
                    quality = SoilQuality.PhuSa;
                    Nutrients = 100f; // Bồi đắp chất phù sa
                    RockDensity = Mathf.Max(0f, RockDensity - 30f); // Phù sa phủ lấp sỏi đá cũ
                    Debug.Log("[SOIL] Ô đất đã được bồi đắp PHÙ SA màu mỡ sau lũ!");
                }
            }

            UpdateVisuals();
        }

        #region HÀM CẢI TẠO ĐẤT ĐAI
        
        /// <summary>
        /// Hành động nhặt sỏi đá. Tiêu hao stamina người chơi, giảm mật độ sỏi đá trên ô đất.
        /// </summary>
        public void ActionClearRocks(float efficiency)
        {
            if (RockDensity <= 0f) return;

            RockDensity = Mathf.Max(0f, RockDensity - efficiency);
            Debug.Log($"[SOIL] Dọn sỏi đá! Mật độ đá còn lại: {RockDensity:F1}%");

            if (RockDensity < 30f && quality == SoilQuality.BacMau)
            {
                quality = SoilQuality.TrungBinh;
                Debug.Log("[SOIL] Đất đã tơi xốp hơn, nâng lên hạng Đất Trung Bình.");
            }
            UpdateVisuals();
        }

        /// <summary>
        /// Hành động tưới nước thủ công.
        /// </summary>
        public void ActionWaterSoil(float amount)
        {
            Moisture = Mathf.Clamp(Moisture + amount, 0f, 100f);
            UpdateVisuals();
        }

        /// <summary>
        /// Bón phân xanh hoặc phân chuồng để cải tạo dinh dưỡng đất đai.
        /// </summary>
        public void ActionFertilize(float nutrientBoost)
        {
            Nutrients = Mathf.Clamp(Nutrients + nutrientBoost, 0f, 100f);
            if (quality == SoilQuality.BacMau && Nutrients > 50f && RockDensity < 40f)
            {
                quality = SoilQuality.TrungBinh;
            }
            UpdateVisuals();
        }

        /// <summary>
        /// Gieo hạt giống lên ô đất.
        /// </summary>
        public bool ActionPlantCrop(CropData seedData)
        {
            if (plantedCrop != null)
            {
                Debug.LogWarning("[SOIL] Ô đất này đã có cây trồng rồi!");
                return false;
            }

            // Tạo GameObject cây con từ code hoặc Instantiate Prefab
            GameObject cropObj = new GameObject($"Crop_{seedData.CropName}");
            cropObj.transform.SetParent(this.transform);
            cropObj.transform.localPosition = Vector3.up * 0.2f; // Định vị cây nhô lên mặt đất

            plantedCrop = cropObj.AddComponent<CropInstance>();
            plantedCrop.Initialize(seedData, this);

            Debug.Log($"[SOIL] Gieo thành công hạt giống {seedData.CropName}!");
            return true;
        }

        #endregion

        /// <summary>
        /// Cập nhật hiển thị màu sắc hoặc Sprite của ô đất tương ứng với độ ẩm và chất đất.
        /// </summary>
        public void UpdateVisuals()
        {
            if (soilSpriteRenderer == null) return;

            if (quality == SoilQuality.PhuSa)
            {
                soilSpriteRenderer.sprite = siltSoilSprite;
            }
            else
            {
                // Thay đổi giữa Sprite Đất Ướt hay Đất Khô dựa trên độ ẩm ẩm (Mốc 35%)
                soilSpriteRenderer.sprite = (Moisture > 35f) ? wetSoilSprite : drySoilSprite;
            }
        }
    }
}
