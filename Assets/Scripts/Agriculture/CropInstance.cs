using UnityEngine;
using SownInStone.Core;
using SownInStone.Weather;
using SownInStone.Interactions;

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

        private GameObject visual3DObj;
        private Transform stemTransform;
        private Transform leaf1Transform;
        private Transform leaf2Transform;
        private Renderer stemRenderer;
        private Renderer leaf1Renderer;
        private Renderer leaf2Renderer;
        private int currentVisual3DStage = -1;

        private void Create3DPlaceholder()
        {
            if (visual3DObj != null) return;

            // Create parent visual object to hold stem and leaves
            visual3DObj = new GameObject("Visual3D");
            visual3DObj.transform.SetParent(this.transform, false);
            visual3DObj.transform.localPosition = Vector3.zero;
            visual3DObj.transform.localRotation = Quaternion.identity;

            // 1. Stem (Cylinder)
            GameObject stemObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            if (stemObj.GetComponent<CapsuleCollider>() != null)
            {
                Destroy(stemObj.GetComponent<CapsuleCollider>());
            }
            stemObj.transform.SetParent(visual3DObj.transform, false);
            stemObj.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            stemObj.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);
            stemTransform = stemObj.transform;
            stemRenderer = stemObj.GetComponent<Renderer>();

            // 2. Leaf 1 (Flattened Cube)
            GameObject leaf1Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (leaf1Obj.GetComponent<BoxCollider>() != null)
            {
                Destroy(leaf1Obj.GetComponent<BoxCollider>());
            }
            leaf1Obj.transform.SetParent(visual3DObj.transform, false);
            leaf1Obj.transform.localPosition = new Vector3(0.15f, 0.3f, 0f);
            leaf1Obj.transform.localRotation = Quaternion.Euler(20f, 0f, 45f);
            leaf1Obj.transform.localScale = new Vector3(0.3f, 0.05f, 0.15f);
            leaf1Transform = leaf1Obj.transform;
            leaf1Renderer = leaf1Obj.GetComponent<Renderer>();

            // 3. Leaf 2 (Flattened Cube)
            GameObject leaf2Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (leaf2Obj.GetComponent<BoxCollider>() != null)
            {
                Destroy(leaf2Obj.GetComponent<BoxCollider>());
            }
            leaf2Obj.transform.SetParent(visual3DObj.transform, false);
            leaf2Obj.transform.localPosition = new Vector3(-0.15f, 0.3f, 0f);
            leaf2Obj.transform.localRotation = Quaternion.Euler(20f, 0f, -45f);
            leaf2Obj.transform.localScale = new Vector3(0.3f, 0.05f, 0.15f);
            leaf2Transform = leaf2Obj.transform;
            leaf2Renderer = leaf2Obj.GetComponent<Renderer>();
        }

        private void UpdateVisual3D(int stageIndex)
        {
            if (cropData.GrowthStagePrefabs != null && cropData.GrowthStagePrefabs.Length > 0)
            {
                int prefabIndex = Mathf.Clamp(stageIndex, 0, cropData.GrowthStagePrefabs.Length - 1);
                GameObject targetPrefab = cropData.GrowthStagePrefabs[prefabIndex];
                if (targetPrefab != null)
                {
                    if (visual3DObj == null || currentVisual3DStage != prefabIndex || stemTransform != null)
                    {
                        Cleanup3DVisual();
                        visual3DObj = Instantiate(targetPrefab, this.transform, false);
                        visual3DObj.name = "Visual3D";
                        visual3DObj.transform.localPosition = Vector3.zero;
                        visual3DObj.transform.localRotation = Quaternion.identity;
                        currentVisual3DStage = prefabIndex;
                        stemTransform = null;
                    }

                    Color overlayColor = Color.white;
                    if (isWithered) overlayColor = new Color(0.5f, 0.4f, 0.3f, 1f);
                    else if (isRotted) overlayColor = new Color(0.15f, 0.2f, 0.15f, 0.8f);
                    foreach (var r in visual3DObj.GetComponentsInChildren<Renderer>())
                    {
                        r.material.color = overlayColor;
                    }
                    return;
                }
            }

            if (visual3DObj == null) Create3DPlaceholder();

            Color stemColor = Color.white;
            Color leafColor = Color.white;

            if (isWithered)
            {
                Color brown = new Color(0.5f, 0.4f, 0.3f, 1f);
                stemColor = brown;
                leafColor = brown;
            }
            else if (isRotted)
            {
                Color rot = new Color(0.15f, 0.2f, 0.15f, 0.8f);
                stemColor = rot;
                leafColor = rot;
            }
            else
            {
                if (stageIndex == 0)
                {
                    // Seedling: stem green, leaves bright green
                    stemColor = new Color(0.2f, 0.7f, 0.2f, 1f);
                    leafColor = new Color(0.1f, 1.0f, 0.1f, 1f);
                }
                else if (stageIndex == 1)
                {
                    // Growing: stem darker green, leaves healthy green
                    stemColor = new Color(0.05f, 0.4f, 0.05f, 1f);
                    leafColor = new Color(0.0f, 0.7f, 0.0f, 1f);
                }
                else
                {
                    // Ready: stem green/brown, leaves yellow-green
                    stemColor = new Color(0.4f, 0.35f, 0.2f, 1f);
                    leafColor = new Color(0.8f, 0.9f, 0.0f, 1f);
                }
            }

            if (stemRenderer != null) stemRenderer.material.color = stemColor;
            if (leaf1Renderer != null) leaf1Renderer.material.color = leafColor;
            if (leaf2Renderer != null) leaf2Renderer.material.color = leafColor;

            if (stageIndex == 0)
            {
                // Seedling: short stem, small leaves
                if (stemTransform != null)
                {
                    stemTransform.localPosition = new Vector3(0f, 0.1f, 0f);
                    stemTransform.localScale = new Vector3(0.08f, 0.1f, 0.08f);
                }
                if (leaf1Transform != null)
                {
                    leaf1Transform.localPosition = new Vector3(0.08f, 0.15f, 0f);
                    leaf1Transform.localScale = new Vector3(0.15f, 0.03f, 0.08f);
                }
                if (leaf2Transform != null)
                {
                    leaf2Transform.localPosition = new Vector3(-0.08f, 0.15f, 0f);
                    leaf2Transform.localScale = new Vector3(0.15f, 0.03f, 0.08f);
                }
            }
            else if (stageIndex == 1)
            {
                // Growing: taller stem, medium leaves
                if (stemTransform != null)
                {
                    stemTransform.localPosition = new Vector3(0f, 0.25f, 0f);
                    stemTransform.localScale = new Vector3(0.12f, 0.25f, 0.12f);
                }
                if (leaf1Transform != null)
                {
                    leaf1Transform.localPosition = new Vector3(0.2f, 0.35f, 0f);
                    leaf1Transform.localScale = new Vector3(0.35f, 0.05f, 0.15f);
                }
                if (leaf2Transform != null)
                {
                    leaf2Transform.localPosition = new Vector3(-0.2f, 0.35f, 0f);
                    leaf2Transform.localScale = new Vector3(0.35f, 0.05f, 0.15f);
                }
            }
            else
            {
                // Ready to harvest: tallest stem, largest leaves
                if (stemTransform != null)
                {
                    stemTransform.localPosition = new Vector3(0f, 0.4f, 0f);
                    stemTransform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
                }
                if (leaf1Transform != null)
                {
                    leaf1Transform.localPosition = new Vector3(0.3f, 0.6f, 0f);
                    leaf1Transform.localScale = new Vector3(0.5f, 0.06f, 0.2f);
                }
                if (leaf2Transform != null)
                {
                    leaf2Transform.localPosition = new Vector3(-0.3f, 0.6f, 0f);
                    leaf2Transform.localScale = new Vector3(0.5f, 0.06f, 0.2f);
                }
            }
        }

        private void Cleanup3DVisual()
        {
            if (visual3DObj != null)
            {
                if (stemRenderer != null)
                {
                    Material mat = stemRenderer.material;
                    if (mat != null) Destroy(mat);
                }
                if (leaf1Renderer != null)
                {
                    Material mat = leaf1Renderer.material;
                    if (mat != null) Destroy(mat);
                }
                if (leaf2Renderer != null)
                {
                    Material mat = leaf2Renderer.material;
                    if (mat != null) Destroy(mat);
                }
                Destroy(visual3DObj);
                visual3DObj = null;
                currentVisual3DStage = -1;
            }
        }

        private static Sprite cachedFallbackSprite;

        private static Sprite GetFallbackSprite()
        {
            if (cachedFallbackSprite == null)
            {
                Texture2D tex = new Texture2D(8, 8);
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        tex.SetPixel(x, y, Color.white);
                    }
                }
                tex.Apply();
                cachedFallbackSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f));
            }
            return cachedFallbackSprite;
        }

        public void Initialize(CropData data, SoilCell soil)
        {
            cropData = data;
            parentSoil = soil;

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 20; // Hiển thị đè lên trên ô đất (sortingOrder 10)

            // Maintain local position and rotation aligned with parent to avoid shearing (Y-offset 0.25)
            transform.localPosition = new Vector3(0f, 0f, -0.25f);
            transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

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
            Cleanup3DVisual();
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
                    // Check if protected by a flood barrier
                    bool isProtected = false;
                    FloodBarrier[] barriers = Object.FindObjectsByType<FloodBarrier>(FindObjectsSortMode.None);
                    foreach (var barrier in barriers)
                    {
                        if (barrier != null && barrier.enabled)
                        {
                            float distance = Vector3.Distance(transform.position, barrier.transform.position);
                            if (distance <= barrier.protectionRadius)
                            {
                                isProtected = true;
                                break;
                            }
                        }
                    }

                    if (!isProtected)
                    {
                        isRotted = true;
                        Debug.LogWarning($"[AGRICULTURE] Cây {cropData.CropName} đã bị úng thối chết do ngập lũ!");
                        return;
                    }
                    else
                    {
                        Debug.Log($"[AGRICULTURE] Cây {cropData.CropName} được che chắn bởi bao cát / tấm chắn lũ!");
                    }
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
        /// Ép cây trồng chín lập tức phục vụ debug kiểm thử.
        /// </summary>
        public void DebugMature()
        {
            if (cropData != null)
            {
                currentGrowthDays = cropData.DaysToMature;
                isWithered = false;
                isRotted = false;
                UpdateVisualSprite();
            }
        }

        /// <summary>
        /// Tiến cây thêm 1 giai đoạn sinh trưởng để debug/trình diễn.
        /// </summary>
        public void DebugGrowOneStage()
        {
            if (cropData == null) return;
            int numStages = 3;
            if (cropData.GrowthStageSprites != null && cropData.GrowthStageSprites.Length > 0)
            {
                numStages = cropData.GrowthStageSprites.Length;
            }
            float daysPerStage = cropData.DaysToMature / (float)(numStages - 1);
            currentGrowthDays = Mathf.Min(currentGrowthDays + daysPerStage, cropData.DaysToMature);
            isWithered = false;
            isRotted = false;
            UpdateVisualSprite();
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

        private void UpdateVisualSprite()
        {
            if (spriteRenderer == null || cropData == null) return;

            // Maintain local position and rotation aligned with parent to avoid shearing (Y-offset 0.25)
            transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            transform.localPosition = new Vector3(0f, 0f, -0.25f);

            float ratio = currentGrowthDays / cropData.DaysToMature;
            int stageIndex = 0;
            Sprite selectedSprite = null;

            if (cropData.GrowthStageSprites != null && cropData.GrowthStageSprites.Length > 0)
            {
                stageIndex = Mathf.FloorToInt(ratio * (cropData.GrowthStageSprites.Length - 1));
                stageIndex = Mathf.Clamp(stageIndex, 0, cropData.GrowthStageSprites.Length - 1);
                selectedSprite = cropData.GrowthStageSprites[stageIndex];
            }
            else
            {
                if (ratio >= 0.8f) stageIndex = 2;
                else if (ratio >= 0.33f) stageIndex = 1;
                else stageIndex = 0;
            }

            // Determine Color and Scale based on state and stage
            Color stageColor = Color.white;
            Vector3 targetScale = Vector3.one;

            if (isWithered)
            {
                stageColor = new Color(0.5f, 0.4f, 0.3f, 1f);
                targetScale = new Vector3(1.2f, 1.0f, 1f);
            }
            else if (isRotted)
            {
                stageColor = new Color(0.15f, 0.2f, 0.15f, 0.8f);
                targetScale = new Vector3(1.2f, 1.0f, 1f);
            }
            else
            {
                if (stageIndex == 0)
                {
                    stageColor = new Color(0.1f, 1.0f, 0.1f, 1f); // bright green
                    targetScale = new Vector3(0.8f, 0.8f, 1f);
                }
                else if (stageIndex == 1)
                {
                    stageColor = new Color(0.0f, 0.7f, 0.0f, 1f); // deeper green
                    targetScale = new Vector3(1.2f, 1.2f, 1f);
                }
                else
                {
                    stageColor = new Color(0.8f, 0.9f, 0.0f, 1f); // yellow-green
                    targetScale = new Vector3(1.6f, 1.6f, 1f);
                }
            }

            // Check if the sprite is null or a default UI sprite name
            bool isInvalid = selectedSprite == null;
            if (selectedSprite != null)
            {
                string sName = selectedSprite.name;
                if (sName == "Knob" || sName == "InputFieldBackground" || sName == "UISprite" || sName == "Background" || sName == "Checkmark")
                {
                    isInvalid = true;
                }
            }

            if (isInvalid)
            {
                // Use 3D Visual
                spriteRenderer.enabled = false;
                UpdateVisual3D(stageIndex);
                transform.localScale = Vector3.one; // 3D model handles its own scaling internally
            }
            else
            {
                // Use 2D Sprite Visual
                spriteRenderer.enabled = true;
                Cleanup3DVisual();
                spriteRenderer.sprite = selectedSprite;
                spriteRenderer.color = stageColor;
                transform.localScale = targetScale;
            }
        }

        private void Update()
        {
            if (isWithered || isRotted) return;
            if (cropData == null || currentGrowthDays >= cropData.DaysToMature) return;

            // Chỉ bắt đầu và tiếp tục sinh trưởng khi đất đã được tưới ẩm (Moisture >= 35f)
            if (parentSoil != null && parentSoil.Moisture >= 35f)
            {
                float growthSpeed = cropData.DaysToMature / 15f; 
                currentGrowthDays = Mathf.Min(currentGrowthDays + growthSpeed * Time.deltaTime, cropData.DaysToMature);
            }
            
            UpdateVisualSprite();
        }

        private void OnGUI()
        {
            if (isWithered || isRotted) return;
            if (cropData == null || currentGrowthDays >= cropData.DaysToMature) return;
            if (Camera.main == null) return;

            // Xác định tọa độ 3D phía trên cây trồng (offset Y là 0.8 mét)
            Vector3 worldPos = transform.position + Vector3.up * 0.8f;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            if (screenPos.z > 0)
            {
                float guiY = Screen.height - screenPos.y;
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 11;

                // Vẽ bóng đổ chữ màu đen
                GUIStyle shadow = new GUIStyle(style);
                shadow.normal.textColor = Color.black;

                if (parentSoil != null && parentSoil.Moisture < 35f)
                {
                    style.normal.textColor = new Color(0.9f, 0.3f, 0.3f, 1f); // Màu đỏ
                    GUI.Label(new Rect(screenPos.x - 60 + 1, guiY - 15 + 1, 120, 30), "Cần tưới nước", shadow);
                    GUI.Label(new Rect(screenPos.x - 60, guiY - 15, 120, 30), "Cần tưới nước", style);
                }
                else
                {
                    // Tính số giây đếm ngược còn lại
                    float ratio = currentGrowthDays / cropData.DaysToMature;
                    float remainingSeconds = 15f * (1f - ratio);
                    if (remainingSeconds <= 0.1f) return;

                    style.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                    GUI.Label(new Rect(screenPos.x - 50 + 1, guiY - 15 + 1, 100, 30), $"{remainingSeconds:F0}s", shadow);
                    GUI.Label(new Rect(screenPos.x - 50, guiY - 15, 100, 30), $"{remainingSeconds:F0}s", style);
                }
            }
        }
    }
}
