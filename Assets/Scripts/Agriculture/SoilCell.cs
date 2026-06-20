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

        [Header("--- LIÊN KẾT Ô ĐẤT CHA-CON ---")]
        [Tooltip("Ô đất cha (đại diện cho toàn bộ lưới ruộng).")]
        public SoilCell parentField;
        [Tooltip("Danh sách các ô đất con.")]
        public System.Collections.Generic.List<SoilCell> childCells = new System.Collections.Generic.List<SoilCell>();
        public bool IsParentField => childCells != null && childCells.Count > 0;

        [Header("--- HIỆU ỨNG HÌNH ẢNH ---")]
        [SerializeField] private SpriteRenderer soilSpriteRenderer;
        [SerializeField] private Sprite drySoilSprite;
        [SerializeField] private Sprite wetSoilSprite;
        [SerializeField] private Sprite siltSoilSprite; // Sprite đất phù sa bồi đắp màu đậm

        private bool wasFloodedDuringStorm = false;

        private void Awake()
        {
            // Tự động tìm kiếm và liên kết ruộng Cha - Con tại runtime nếu chưa được cấu hình
            if (gameObject.name == "SoilCell_Large")
            {
                childCells.Clear();
                // Tìm tất cả các SoilCell con có tên chứa "SoilCell_Grid"
                SoilCell[] allSoils = FindObjectsByType<SoilCell>();
                foreach (var s in allSoils)
                {
                    if (s != this && s.gameObject.name.StartsWith("SoilCell_Grid"))
                    {
                        childCells.Add(s);
                        s.parentField = this;
                    }
                }
                Debug.Log($"[SOIL] Tự động liên kết {childCells.Count} ô ruộng con cho SoilCell_Large.");
            }
        }

        private void Start()
        {
            // Lắng nghe ngày mới trôi qua để cập nhật sinh học của đất
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDayChanged += OnNewDay;
                GameManager.Instance.OnPhaseChanged += OnPhaseChanged;
            }
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDayChanged -= OnNewDay;
                GameManager.Instance.OnPhaseChanged -= OnPhaseChanged;
            }
        }

        private void Update()
        {
            // Bốc hơi nước liên tục do thời tiết thực tế
            SimulateWaterEvaporation();

            // Ghi nhận ngập lụt sâu trong bão lũ
            if (WeatherManager.Instance != null && WeatherManager.Instance.FloodLevel > 0.5f)
            {
                wasFloodedDuringStorm = true;
            }
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
            UpdateVisuals();
        }

        /// <summary>
        /// Tự động bồi đắp chất dinh dưỡng phù sa khi bão tan bước sang GĐ 4
        /// </summary>
        private void OnPhaseChanged(GamePhase newPhase)
        {
            if (newPhase == GamePhase.PhuSa && wasFloodedDuringStorm)
            {
                quality = SoilQuality.PhuSa;
                Nutrients = 100f; // Dinh dưỡng tối đa
                RockDensity = Mathf.Max(0f, RockDensity - 30f); // Che phủ sỏi đá
                wasFloodedDuringStorm = false;
                Debug.Log($"[SOIL] Ô đất {gameObject.name} đã được bồi đắp PHÙ SA trù phú sau lũ!");
                UpdateVisuals();
            }
        }

        #region HÀM CẢI TẠO ĐẤT ĐAI
        
        /// <summary>
        /// Hành động nhặt sỏi đá. Tiêu hao stamina người chơi, giảm mật độ sỏi đá trên ô đất.
        /// </summary>
        public void ActionClearRocks(float efficiency)
        {
            if (IsParentField)
            {
                foreach (var child in childCells)
                {
                    if (child != null) child.ActionClearRocks(efficiency);
                }
                RockDensity = 0f;
                quality = SoilQuality.TrungBinh;
                UpdateVisuals();
                return;
            }

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
            if (IsParentField)
            {
                foreach (var child in childCells)
                {
                    if (child != null) child.ActionWaterSoil(amount);
                }
                Moisture = amount;
                UpdateVisuals();
                return;
            }

            Moisture = Mathf.Clamp(Moisture + amount, 0f, 100f);
            UpdateVisuals();
        }

        /// <summary>
        /// Bón phân xanh hoặc phân chuồng để cải tạo dinh dưỡng đất đai.
        /// </summary>
        public void ActionFertilize(float nutrientBoost)
        {
            if (IsParentField)
            {
                foreach (var child in childCells)
                {
                    if (child != null) child.ActionFertilize(nutrientBoost);
                }
                Nutrients = nutrientBoost;
                UpdateVisuals();
                return;
            }

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
            if (IsParentField)
            {
                bool anyPlanted = false;
                foreach (var child in childCells)
                {
                    if (child != null && child.plantedCrop == null)
                    {
                        if (child.ActionPlantCrop(seedData))
                        {
                            anyPlanted = true;
                        }
                    }
                }
                return anyPlanted;
            }

            if (plantedCrop != null)
            {
                Debug.LogWarning("[SOIL] Ô đất này đã có cây trồng rồi!");
                return false;
            }

            // Tạo GameObject cây con từ code hoặc Instantiate Prefab
            GameObject cropObj = new GameObject($"Crop_{seedData.CropName}");
            cropObj.transform.SetParent(this.transform, false);
            cropObj.transform.localPosition = new Vector3(0f, 0f, -0.08f); // Định vị cây nhô lên mặt đất
            cropObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // Đứng thẳng upright

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

            // Nếu là ô ruộng lớn làm nền, ta tô màu đậm hơn một chút để làm nổi bật 9 ô con cát bạc màu ở trên
            if (gameObject.name == "SoilCell_Large")
            {
                soilSpriteRenderer.color = new Color(0.72f, 0.65f, 0.58f, 1.0f); // Màu đất cát tối hơn làm nền lót
            }
            else
            {
                soilSpriteRenderer.color = Color.white;
            }
        }

        private GameObject highlightObj;

        public void SetHighlight(bool active)
        {
            if (active)
            {
                if (highlightObj == null)
                {
                    highlightObj = new GameObject("SoilHighlight");
                    highlightObj.transform.SetParent(this.transform, false);
                    highlightObj.transform.localPosition = new Vector3(0f, 0f, -0.05f);
                    highlightObj.transform.localRotation = Quaternion.identity;
                    highlightObj.transform.localScale = Vector3.one;

                    SpriteRenderer hr = highlightObj.AddComponent<SpriteRenderer>();
                    if (soilSpriteRenderer != null)
                    {
                        hr.sprite = soilSpriteRenderer.sprite;
                        hr.size = soilSpriteRenderer.size;
                    }
                    hr.color = new Color(0.95f, 0.85f, 0.1f, 0.35f); // Màu vàng bán trong suốt
                }
                else
                {
                    highlightObj.SetActive(true);
                    SpriteRenderer hr = highlightObj.GetComponent<SpriteRenderer>();
                    if (hr != null && soilSpriteRenderer != null)
                    {
                        hr.sprite = soilSpriteRenderer.sprite;
                    }
                }
            }
            else
            {
                if (highlightObj != null && highlightObj.activeSelf)
                {
                    highlightObj.SetActive(false);
                }
            }
        }
    }
}
