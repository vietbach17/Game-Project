using UnityEngine;
using SownInStone.Core;
using SownInStone.Weather;
using SownInStone.UI;

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

        [Header("--- 3D VISUALS ---")]
        [SerializeField] public GameObject rockySoilVisual;
        [SerializeField] public GameObject cleanSoilVisual;
        [SerializeField] public GameObject tilledSoilVisual;
        [SerializeField] public GameObject wetSoilVisual;

        private bool wasFloodedDuringStorm = false;

        private void Awake()
        {
            // Tự động tìm kiếm và liên kết ruộng Cha - Con tại runtime nếu chưa được cấu hình
            // Nhận diện ruộng cha: Tên không chứa "Grid" và là SoilCell_Large, SoilCell_1 hoặc SoilCell
            bool isParentCandidate = !gameObject.name.Contains("Grid") && 
                                     (gameObject.name == "SoilCell_Large" || 
                                      gameObject.name == "SoilCell" || 
                                      gameObject.name.StartsWith("SoilCell_"));

            if (isParentCandidate)
            {
                childCells.Clear();
                // Tìm tất cả các SoilCell con có tên chứa "Grid" hoặc bắt đầu bằng "SoilCell_Grid"
                SoilCell[] allSoils = FindObjectsByType<SoilCell>();
                foreach (var s in allSoils)
                {
                    if (s != this && (s.gameObject.name.Contains("Grid") || s.gameObject.name.StartsWith("SoilCell_Grid")))
                    {
                        // Kiểm tra khoảng cách địa lý 2D trên mặt phẳng XZ (ngưỡng 6 mét)
                        float distXZ = Vector2.Distance(
                            new Vector2(transform.position.x, transform.position.z),
                            new Vector2(s.transform.position.x, s.transform.position.z)
                        );
                        if (distXZ < 6.0f)
                        {
                            childCells.Add(s);
                            s.parentField = this;
                        }
                    }
                }
                Debug.Log($"[SOIL] Tự động liên kết {childCells.Count} ô ruộng con cho ruộng cha {gameObject.name}.");
            }
        }

        private void Start()
        {
            // Lắng nghe ngày mới trôi qua để cập nhật sinh học của đất
            if (GameManager.Instance != null)
            {
                GameManager.OnDayChanged += OnNewDay;
                GameManager.OnPhaseChanged += OnPhaseChanged;
            }
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.OnDayChanged -= OnNewDay;
                GameManager.OnPhaseChanged -= OnPhaseChanged;
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
            if (newPhase == GamePhase.PhuSa)
            {
                quality = SoilQuality.PhuSa;
                Nutrients = 100f; // Dinh dưỡng tối đa
                RockDensity = 0f; // Tự động dọn sạch sỏi đá
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
                if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                {
                    TutorialManager.Instance.OnRockCleared();
                }
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
            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                TutorialManager.Instance.OnRockCleared();
            }
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
                if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                {
                    TutorialManager.Instance.OnSoilWatered();
                }
                return;
            }

            Moisture = Mathf.Clamp(Moisture + amount, 0f, 100f);
            UpdateVisuals();
            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                TutorialManager.Instance.OnSoilWatered();
            }
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
                if (anyPlanted && TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                {
                    TutorialManager.Instance.OnCropPlanted();
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
            UpdateVisuals(); // Update visual state when planted

            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                TutorialManager.Instance.OnCropPlanted();
            }
            return true;
        }

        #endregion

        /// <summary>
        /// Cập nhật hiển thị màu sắc hoặc Sprite của ô đất tương ứng với độ ẩm và chất đất.
        /// </summary>
        public void UpdateVisuals()
        {
            if (soilSpriteRenderer != null)
            {
                if (quality == SoilQuality.PhuSa)
                {
                    soilSpriteRenderer.sprite = siltSoilSprite;
                }
                else
                {
                    // Thay đổi giữa Sprite Đất Ướt hay Đất Khô dựa trên độ ẩm ẩm (Mốc 35%)
                    soilSpriteRenderer.sprite = (Moisture > 35f) ? wetSoilSprite : drySoilSprite;
                }

                // Nếu là ô ruộng lớn làm nền, ta tô màu đậm hơn một chút để làm nổi bật các ô con cát bạc màu ở trên
                if (IsParentField)
                {
                    soilSpriteRenderer.color = new Color(0.72f, 0.65f, 0.58f, 1.0f); // Màu đất cát tối hơn làm nền lót
                }
                else
                {
                    soilSpriteRenderer.color = Color.white;
                }
            }

            // Cập nhật hiển thị 3D Visuals
            if (rockySoilVisual != null) rockySoilVisual.SetActive(false);
            if (cleanSoilVisual != null) cleanSoilVisual.SetActive(false);
            if (tilledSoilVisual != null) tilledSoilVisual.SetActive(false);
            if (wetSoilVisual != null) wetSoilVisual.SetActive(false);

            GameObject activeVisual = null;
            if (RockDensity > 0f)
            {
                activeVisual = rockySoilVisual;
            }
            else if (Moisture >= 35f || quality == SoilQuality.PhuSa)
            {
                activeVisual = wetSoilVisual;
            }
            else if (plantedCrop != null)
            {
                activeVisual = tilledSoilVisual;
            }
            else
            {
                activeVisual = cleanSoilVisual;
            }

            if (activeVisual != null)
            {
                activeVisual.SetActive(true);
            }
        }

        private GameObject highlightObj;

        public void SetHighlight(bool active)
        {
            if (active)
            {
                if (highlightObj == null)
                {
                    // Nếu là 2D mode (SpriteRenderer của đất đang hoạt động)
                    if (soilSpriteRenderer != null && soilSpriteRenderer.enabled)
                    {
                        highlightObj = new GameObject("SoilHighlight");
                        highlightObj.transform.SetParent(this.transform, false);
                        highlightObj.transform.localPosition = new Vector3(0f, 0f, -0.05f);
                        highlightObj.transform.localRotation = Quaternion.identity;
                        highlightObj.transform.localScale = Vector3.one;

                        SpriteRenderer hr = highlightObj.AddComponent<SpriteRenderer>();
                        if (soilSpriteRenderer.sprite != null)
                        {
                            hr.sprite = soilSpriteRenderer.sprite;
                        }
                        hr.color = new Color(0.95f, 0.85f, 0.1f, 0.35f);
                    }
                    else
                    {
                        // 3D mode: Tạo khung viền màu vàng gold nổi bật bằng 4 thanh Cube dẹt
                        highlightObj = new GameObject("TargetHighlightFrame");
                        highlightObj.transform.SetParent(this.transform, false);
                        highlightObj.transform.localPosition = new Vector3(0f, 0.03f, 0f); // Cao hơn mặt đất 3cm
                        highlightObj.transform.localRotation = Quaternion.identity;
                        highlightObj.transform.localScale = Vector3.one;

                        Material goldMat = new Material(Shader.Find("Unlit/Color"));
                        if (goldMat != null)
                        {
                            goldMat.color = new Color(1f, 0.85f, 0.0f, 1f); // Màu vàng gold tươi sáng
                        }

                        float thickness = 0.06f;
                        float size = 2.0f; // Khớp với kích thước ô đất 2.0m

                        // North
                        GameObject north = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        DestroyImmediate(north.GetComponent<Collider>());
                        north.transform.SetParent(highlightObj.transform, false);
                        north.transform.localPosition = new Vector3(0f, 0f, size / 2f);
                        north.transform.localScale = new Vector3(size + thickness, 0.02f, thickness);
                        if (goldMat != null) north.GetComponent<Renderer>().sharedMaterial = goldMat;
                        north.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        north.GetComponent<Renderer>().receiveShadows = false;

                        // South
                        GameObject south = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        DestroyImmediate(south.GetComponent<Collider>());
                        south.transform.SetParent(highlightObj.transform, false);
                        south.transform.localPosition = new Vector3(0f, 0f, -size / 2f);
                        south.transform.localScale = new Vector3(size + thickness, 0.02f, thickness);
                        if (goldMat != null) south.GetComponent<Renderer>().sharedMaterial = goldMat;
                        south.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        south.GetComponent<Renderer>().receiveShadows = false;

                        // East
                        GameObject east = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        DestroyImmediate(east.GetComponent<Collider>());
                        east.transform.SetParent(highlightObj.transform, false);
                        east.transform.localPosition = new Vector3(size / 2f, 0f, 0f);
                        east.transform.localScale = new Vector3(thickness, 0.02f, size + thickness);
                        if (goldMat != null) east.GetComponent<Renderer>().sharedMaterial = goldMat;
                        east.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        east.GetComponent<Renderer>().receiveShadows = false;

                        // West
                        GameObject west = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        DestroyImmediate(west.GetComponent<Collider>());
                        west.transform.SetParent(highlightObj.transform, false);
                        west.transform.localPosition = new Vector3(-size / 2f, 0f, 0f);
                        west.transform.localScale = new Vector3(thickness, 0.02f, size + thickness);
                        if (goldMat != null) west.GetComponent<Renderer>().sharedMaterial = goldMat;
                        west.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        west.GetComponent<Renderer>().receiveShadows = false;
                    }
                }
                else
                {
                    highlightObj.SetActive(true);
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

        // --- CÁC PHƯƠNG THỨC HỖ TRỢ TRÌNH DIỄN (DEMO SHORTCUTS) ---

        public void DebugGrowOneStage()
        {
            if (plantedCrop != null)
            {
                plantedCrop.DebugGrowOneStage();
            }
        }

        public void DebugForceReadyToHarvest()
        {
            if (plantedCrop != null)
            {
                plantedCrop.DebugMature();
            }
        }

        public void DebugMakeWet()
        {
            Moisture = 75f;
            UpdateVisuals();
        }

        public void DebugClearRocks()
        {
            RockDensity = 0f;
            quality = SoilQuality.TrungBinh;
            UpdateVisuals();
        }

        public void DebugResetSoil()
        {
            Moisture = 10f;
            Nutrients = 15f;
            RockDensity = 80f;
            quality = SoilQuality.BacMau;
            if (plantedCrop != null)
            {
                Destroy(plantedCrop.gameObject);
                plantedCrop = null;
            }
            UpdateVisuals();
        }
    }
}
