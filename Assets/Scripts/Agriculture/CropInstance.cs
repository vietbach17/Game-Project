using UnityEngine;
using SownInStone.Core;
using SownInStone.Weather;

namespace SownInStone.Agriculture
{
    /// <summary>
    /// Thực thể cây trồng cụ thể đang lớn trên một ô đất.
    /// Theo dõi tiến trình lớn lên, héo úa do nắng hạn hoặc úng chết do lụt.
    /// </summary>
    public class CropInstance : MonoBehaviour
    {
        public CropData cropData { get; private set; }
        private SoilCell parentSoil;

        [Header("--- TRẠNG THÁI SINH TRƯỞNG ---")]
        [Tooltip("Số ngày tích lũy lớn lên hiện tại.")]
        public float currentGrowthDays = 0f;

        [Tooltip("Cây có bị héo úa chết khô do thiếu nước / nắng nóng quá mức không.")]
        public bool isWithered = false;

        [Tooltip("Cây có bị úng thối chết do ngập lụt lâu ngày không.")]
        public bool isRotted = false;

        private SpriteRenderer spriteRenderer;

        public void Initialize(CropData data, SoilCell soil)
        {
            cropData = data;
            parentSoil = soil;

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 5; // Hiển thị đè lên trên ô đất

            UpdateVisualSprite();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDayChanged += OnNewDayTick;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDayChanged -= OnNewDayTick;
            }
        }

        /// <summary>
        /// Logic sinh trưởng chạy vào đầu mỗi ngày mới.
        /// </summary>
        private void OnNewDayTick(int dayIndex)
        {
            if (isWithered || isRotted) return;

            CheckDisasterDamage();
            if (isWithered || isRotted)
            {
                UpdateVisualSprite();
                return;
            }

            // Tính toán hiệu suất phát triển dựa trên độ ẩm, dinh dưỡng đất và sỏi đá
            float moistureSatisfaction = 1f - Mathf.Abs(parentSoil.Moisture - cropData.IdealMoisture) / 100f;
            moistureSatisfaction = Mathf.Clamp(moistureSatisfaction, 0.1f, 1f);

            // Thiếu dinh dưỡng đất làm cây lớn cực chậm
            float nutrientFactor = parentSoil.Nutrients >= cropData.RequiredNutrients ? 1.0f : 0.2f;

            // Sỏi đá cản trở rễ cây phát triển
            float rockObstacleFactor = Mathf.Clamp01(1f - (parentSoil.RockDensity / 100f) * 0.8f);

            // Tính toán tiến trình phát triển của ngày hôm đó
            float growthToday = 1f * moistureSatisfaction * nutrientFactor * rockObstacleFactor;
            currentGrowthDays = Mathf.Min(currentGrowthDays + growthToday, cropData.DaysToMature);

            // Cây hút bớt chất dinh dưỡng trong đất sau mỗi ngày lớn
            parentSoil.Nutrients = Mathf.Max(0f, parentSoil.Nutrients - 3f);

            UpdateVisualSprite();
        }

        /// <summary>
        /// Kiểm tra xem cây trồng có bị ảnh hưởng bởi thiên tai (Gió Lào khô cháy, lũ ngập úng) hay không.
        /// </summary>
        private void CheckDisasterDamage()
        {
            if (WeatherManager.Instance == null) return;

            // 1. Kiểm tra úng lụt
            if (WeatherManager.Instance.FloodLevel > 0.3f) // Nước dâng ngập quá 30cm
            {
                if (!cropData.CanSurviveFlooding)
                {
                    isRotted = true;
                    Debug.LogWarning($"[AGRICULTURE] Cây {cropData.CropName} đã bị úng thối chết do ngập lũ!");
                    return;
                }
            }

            // 2. Kiểm tra cháy khô héo do nắng nóng Gió Lào & đất cạn nước
            if (WeatherManager.Instance.Temperature > cropData.MaxTemperatureTolerance && parentSoil.Moisture < 15f)
            {
                isWithered = true;
                Debug.LogWarning($"[AGRICULTURE] Cây {cropData.CropName} đã bị cháy lá chết khô do nắng nóng và cạn nước tưới!");
                return;
            }
        }

        /// <summary>
        /// Kiểm tra xem cây trồng đã chín hoàn toàn để thu hoạch chưa.
        /// </summary>
        public bool IsReadyToHarvest()
        {
            return !isWithered && !isRotted && (currentGrowthDays >= cropData.DaysToMature);
        }

        /// <summary>
        /// Tiến hành thu hoạch. Trả về số lượng nông sản thu được. Giải phóng ô đất.
        /// </summary>
        public int ActionHarvest()
        {
            if (!IsReadyToHarvest())
            {
                Debug.LogWarning("[AGRICULTURE] Cây chưa chín hẳn, không thể thu hoạch đạt hiệu suất!");
                return 0;
            }

            // Nông sản thu hoạch tỷ lệ thuận với chất lượng đất trồng
            int baseYield = 2;
            if (parentSoil.quality == SoilQuality.PhuSa)
            {
                baseYield = 5; // Phù sa giúp sản lượng vượt trội gấp đôi!
                Debug.Log("[AGRICULTURE] Đất phù sa trù phú mang lại vụ mùa bội thu ngọt ngào!");
            }

            Destroy(gameObject); // Xóa cây trên ô đất
            parentSoil.plantedCrop = null;

            return baseYield;
        }

        /// <summary>
        /// Cập nhật hình ảnh cây trồng dựa vào giai đoạn tăng trưởng hoặc trạng thái chết úng/héo.
        /// </summary>
        private void UpdateVisualSprite()
        {
            if (spriteRenderer == null || cropData == null) return;

            if (isWithered)
            {
                // Sử dụng màu xám cháy hoặc vẽ đè màu nâu đỏ khô héo
                spriteRenderer.color = new Color(0.5f, 0.4f, 0.3f, 1f);
                return;
            }

            if (isRotted)
            {
                // Màu đen úng mốc thối rữa
                spriteRenderer.color = new Color(0.2f, 0.3f, 0.2f, 0.8f);
                return;
            }

            // Tính tỷ lệ lớn để lấy sprite tương ứng trong mảng Sprites của CropData
            if (cropData.GrowthStageSprites == null || cropData.GrowthStageSprites.Length == 0) return;

            float ratio = currentGrowthDays / cropData.DaysToMature;
            int stageIndex = Mathf.FloorToInt(ratio * (cropData.GrowthStageSprites.Length - 1));
            stageIndex = Mathf.Clamp(stageIndex, 0, cropData.GrowthStageSprites.Length - 1);

            spriteRenderer.sprite = cropData.GrowthStageSprites[stageIndex];
            spriteRenderer.color = Color.white; // Màu bình thường tươi xanh
        }
    }
}
