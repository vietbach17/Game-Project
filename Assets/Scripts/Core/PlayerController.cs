using UnityEngine;
using SownInStone.Agriculture;
using SownInStone.Community;
using SownInStone.Interactions;
using SownInStone.Weather;
using SownInStone.Storage;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SownInStone.Core
{
    /// <summary>
    /// Điều khiển nhân vật chính (Thành) di chuyển bằng bàn phím WASD / Phím mũi tên.
    /// Hỗ trợ: Phát thông số cho Animator 2D, tiêu hao thể lực khi chạy dưới nắng Gió Lào,
    /// và bấm phím [E] để tương tác thực tế với NPC, Bàn thờ, Ô đất trồng trọt.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("--- THÔNG SỐ DI CHUYỂN ---")]
        [Tooltip("Tốc độ đi bộ của nhân vật.")]
        [SerializeField] private float walkSpeed = 3f;

        [Tooltip("Tốc độ chạy của nhân vật.")]
        [SerializeField] private float runSpeed = 6f;

        [Tooltip("Tốc độ di chuyển của nhân vật (để tương thích ngược).")]
        [SerializeField] private float moveSpeed = 4f;

        [Header("--- THỜI GIAN KHÓA HÀNH ĐỘNG ---")]
        [Tooltip("Thời gian khóa di chuyển khi nhặt đá (giây).")]
        [SerializeField] private float digActionDuration = 1.2f;

        [Tooltip("Thời gian khóa di chuyển khi tưới nước (giây).")]
        [SerializeField] private float waterActionDuration = 1.2f;

        [Tooltip("Thời gian khóa di chuyển khi gieo hạt (giây).")]
        [SerializeField] private float plantActionDuration = 1.0f;

        [Tooltip("Thời gian khóa di chuyển khi thu hoạch (giây).")]
        [SerializeField] private float harvestActionDuration = 1.2f;

        [Header("--- XOAY NHÂN VẬT 3D ---")]
        [Tooltip("Kéo mô hình 3D con vào đây. Nếu trống, script tự tìm con đầu tiên.")]
        [SerializeField] private Transform characterVisual;

        [Tooltip("Tốc độ xoay hướng của nhân vật.")]
        [SerializeField] private float rotationSpeed = 10f;

        public enum RotationAxis { RotateAroundZ_2D, RotateAroundY_3D }
        [Tooltip("Trục xoay hướng: Chọn RotateAroundZ nếu game nhìn thẳng 2D, chọn RotateAroundY nếu là 3D nhìn từ trên xuống.")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.RotateAroundY_3D;

        [Header("--- TƯƠNG TÁC PHÍM [E] ---")]
        [Tooltip("Bán kính hình cầu quét tìm vật thể có thể tương tác xung quanh nhân vật.")]
        [SerializeField] private float interactRadius = 2.5f;
        
        [Tooltip("Lớp vật lý (Layer) chứa các đối tượng có thể tương tác (nên chọn Everything hoặc thiết lập riêng).")]
        [SerializeField] private LayerMask interactableLayers = ~0;

        [Header("--- TRỒNG TRỌT KIỂM THỬ ---")]
        [Tooltip("Hạt giống dùng để gieo hạt khi ruộng đất trống và ẩm.")]
        [SerializeField] private CropData testSeedData;

        [Tooltip("Vật phẩm Hạt giống thực tế trong kho đồ tiêu hao khi gieo trồng.")]
        [SerializeField] public ItemData seedItem;

        [Header("--- LŨ LỤT & SINH TỒN TRÊN NÓC NHÀ ---")]
        [Tooltip("Mì tôm cứu trợ khi dâng lũ.")]
        [SerializeField] private ItemData noodlesItem;
        private bool isOnRoof = false;
        private bool isInsideHouse = false;
        private GameObject houseInteriorInstance;

        [Header("--- SINH TỒN NÓC NHÀ ---")]
        private GameObject roofPlatformInstance;
        private GameObject roofCampfireInstance;
        private GameObject roofWaterCollectorInstance;
        private GameObject roofStoveInstance;
        private float campfireBurnTime = 45f;
        private float collectedRainwater = 0f;
        private float playerBodyTemp = 37f;
        private float lastCrateSpawnTime = 0f;
        private System.Collections.Generic.List<GameObject> spawnedCrates = new System.Collections.Generic.List<GameObject>();

        public bool IsOnRoof => isOnRoof;
        public bool IsInsideHouse => isInsideHouse;

        [Header("--- HỆ THỐNG CHẮN LŨ (TASK 2) ---")]
        [Tooltip("Vật phẩm Vách chắn nước trong kho đồ.")]
        public ItemData floodBoardItem;

        [Tooltip("Vật phẩm Bao cát trong kho đồ.")]
        public ItemData sandbagItem;

        [Tooltip("Prefab Vách chắn nước 3D xuất hiện ngoài Scene.")]
        public GameObject floodBoardPrefab;

        [Tooltip("Prefab Bao cát 3D xuất hiện ngoài Scene.")]
        public GameObject sandbagPrefab;

        private Rigidbody rb;
        private Animator animator;
        private Vector2 moveInput;
        private Vector2 lastMoveDirection = Vector2.down; // Hướng quay mặt mặc định (nhìn xuống)
        private bool isRunning; // Cho biết người chơi đang chạy nhanh hay đi bộ
        private bool isPerformingAction; // Cho biết người chơi đang thực hiện động tác trồng trọt

        // Lưu trữ góc xoay cục bộ ban đầu của mô hình 3D để tránh bị lật ngã do sai lệch trục nhập khẩu
        private float initialVisualLocalX = 0f;
        private float initialVisualLocalZ = 0f;

        private System.Collections.Generic.HashSet<string> animatorParams = new System.Collections.Generic.HashSet<string>();
        private SoilCell currentTargetSoil;
        public SoilCell CurrentTargetSoilCell => currentTargetSoil;

        public bool IsPerformingAction
        {
            get => isPerformingAction;
            set
            {
                isPerformingAction = value;
                if (value)
                {
                    moveInput = Vector2.zero;
                    if (rb != null)
                    {
                        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                    }
                }
            }
        }

        [Header("--- XEM TRƯỚC VỊ TRÍ ĐẶT (GHOST PREVIEW) ---")]
        private bool isPlacingPreview = false;
        private ItemData previewItemData;
        private GameObject previewPrefab;
        private string previewItemName;
        private GameObject activeGhostObject;
        private float previewRotationY = 0f;

        [Header("--- PHÍM BẤM HỖ TRỢ TƯƠNG THÍCH ---")]
        public KeyCode keyMoveUp = KeyCode.W;
        public KeyCode keyMoveDown = KeyCode.S;
        public KeyCode keyMoveLeft = KeyCode.A;
        public KeyCode keyMoveRight = KeyCode.D;
        public KeyCode keyInteract = KeyCode.E;
        public KeyCode keyRun = KeyCode.LeftShift;

        public void LoadKeyBindings()
        {
            keyMoveUp = (KeyCode)PlayerPrefs.GetInt("Key_MoveUp", (int)KeyCode.W);
            keyMoveDown = (KeyCode)PlayerPrefs.GetInt("Key_MoveDown", (int)KeyCode.S);
            keyMoveLeft = (KeyCode)PlayerPrefs.GetInt("Key_MoveLeft", (int)KeyCode.A);
            keyMoveRight = (KeyCode)PlayerPrefs.GetInt("Key_MoveRight", (int)KeyCode.D);
            keyInteract = (KeyCode)PlayerPrefs.GetInt("Key_Interact", (int)KeyCode.E);
            keyRun = (KeyCode)PlayerPrefs.GetInt("Key_Run", (int)KeyCode.LeftShift);
        }

        public void TriggerRescueSequence()
        {
            Debug.Log("[PlayerController] TriggerRescueSequence compatibility stub called.");
            // Hồi phục 30 Health / 30 Stamina / 20 Morale khi ngất xỉu
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.ModifyHealth(30f);
                PlayerStats.Instance.ModifyStamina(30f);
                PlayerStats.Instance.ModifyMorale(20f);
            }
            
            // Teleport về nhà Thành
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
            Vector3 homePos = new Vector3(0f, 0.5f, -6f);
            transform.position = homePos;
            if (rb != null)
            {
                rb.position = homePos;
            }
        }

        private void Awake()
        {
            LoadKeyBindings();
            AutoLoadTask2References();

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator != null)
            {
                animator.applyRootMotion = false; // Tắt root motion để Rigidbody có thể di chuyển nhân vật bình thường
                foreach (var param in animator.parameters)
                {
                    animatorParams.Add(param.name);
                }
            }

            // Cấu hình Rigidbody để phù hợp với game 3D Top-down (di chuyển phẳng X/Z)
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

            // Tự động tìm mô hình 3D con nếu chưa gán trong Inspector
            if (characterVisual == null && transform.childCount > 0)
            {
                characterVisual = transform.GetChild(0);
            }

            // Ghi nhớ góc xoay cục bộ ban đầu (ví dụ GLB từ Tripo mặc định xoay -90 độ trên trục X)
            if (characterVisual != null)
            {
                initialVisualLocalX = characterVisual.localRotation.eulerAngles.x;
                initialVisualLocalZ = characterVisual.localRotation.eulerAngles.z;
            }

#if UNITY_EDITOR
            // Tự động tải tài nguyên để đảm bảo không bị mất reference trong Editor
            seedItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Seed.asset");
            noodlesItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Noodles.asset");
            testSeedData = UnityEditor.AssetDatabase.LoadAssetAtPath<CropData>("Assets/Data/Crop_KhoaiLang.asset");
#endif
        }

        private void HandleFloodRoofSurvival()
        {
            if (WeatherManager.Instance == null || GameManager.Instance == null) return;

            // 1. Tự động di tản lên nóc nhà khi nước ngập cao trong mùa bão lũ
            if (GameManager.Instance.CurrentPhase == GamePhase.MuaBao && WeatherManager.Instance.FloodLevel > 1.5f && !isOnRoof)
            {
                // Trong stage RescuingNPCs: người chơi phải tự cứu đủ 4 người rồi game mới tele lên nóc nhà.
                // Không được auto-tele trong lúc đang làm nhiệm vụ cứu hộ.
                bool isRescueMission = TutorialManager.Instance != null &&
                                       TutorialManager.Instance.isTutorialActive &&
                                       TutorialManager.Instance.currentStage == TutorialManager.TutorialStage.RescuingNPCs;
                if (!isRescueMission)
                {
                    isOnRoof = true;

                    // Teleport lên nóc nhà
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                    }

                    GameObject houseObj = GameObject.Find("Thanh_House");
                    Vector3 roofPos = houseObj != null ? houseObj.transform.position + new Vector3(0f, 3.4f, 0f) : new Vector3(10.66f, 3.4f, -10.0f);

                    transform.position = roofPos;
                    if (rb != null)
                    {
                        rb.position = roofPos;
                    }

                    // Khởi tạo các đối tượng sinh tồn trên nóc nhà
                    SetupRoofSurvivalObjects();

                    // Phát mì tôm cứu trợ
                    if (StorageManager.Instance != null && noodlesItem != null)
                    {
                        StorageManager.Instance.AddItem(noodlesItem, 5);
                    }

                    // Hiển thị thông báo cứu trợ
                    if (SownInStone.UI.SurvivalUIManager.Instance != null)
                    {
                        SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(
                            "Thông báo thiên tai",
                            "Nước lũ dâng cao ngập lút ruộng vườn! Bạn đã phải di tản lên nóc nhà lánh nạn. Hãy cố gắng sưởi ấm và ăn mì cứu trợ để sinh tồn qua đợt thiên tai!"
                        );
                    }
                }
            }
            // 2. Trở lại đất liền khi nước rút hoặc chuyển phase mới
            else if (isOnRoof && (GameManager.Instance.CurrentPhase != GamePhase.MuaBao || WeatherManager.Instance.FloodLevel < 0.5f))
            {
                isOnRoof = false;
                
                // Dọn dẹp các đối tượng trên nóc nhà
                CleanupRoofSurvivalObjects();
                
                // Teleport về mặt đất
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                }
                GameObject houseObj = GameObject.Find("Thanh_House");
                Vector3 groundPos = houseObj != null ? houseObj.transform.position + new Vector3(0f, 0.2f, -4.2f) : new Vector3(10.66f, 0.2f, -14.2f);
                transform.position = groundPos;
                if (rb != null)
                {
                    rb.position = groundPos;
                }
                
                // Hiển thị thông báo trở lại đất liền
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(
                        "Thiên tai đi qua", 
                        "Nước lũ đã rút! Bạn có thể trở lại mặt đất để bắt đầu dọn dẹp và khôi phục lại ruộng vườn."
                    );
                }
            }

            // 3. Xử lý tác động khi người chơi trượt chân rơi xuống nước lũ trong bão
            if (isOnRoof && transform.position.y < 2.0f)
            {
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.ApplyColdStress(15f * Time.deltaTime);
                    PlayerStats.Instance.ModifyHealth(-5f * Time.deltaTime);
                }
                if (UnityEngine.Random.value < 0.005f)
                {
                    SownInStone.UI.SurvivalUIManager.Instance?.ShowHUDToast("⚠️ BẠN ĐÃ BỊ RƠI XUỐNG NƯỚC LŨ! Hãy đi đến cửa nhà bấm [E] để leo trở lại lên nóc nhà!");
                }
            }
        }

        private void SetupRoofSurvivalObjects()
        {
            CleanupRoofSurvivalObjects();

            GameObject houseObj = GameObject.Find("Thanh_House");
            Vector3 center = houseObj != null ? houseObj.transform.position : transform.position;

            // 1. Nền đứng nóc nhà (invisible platform) để người chơi đứng vững
            roofPlatformInstance = GameObject.CreatePrimitive(PrimitiveType.Plane);
            roofPlatformInstance.name = "RoofSurvivalPlatform";
            roofPlatformInstance.transform.position = center + new Vector3(0f, 2.7f, 0f);
            roofPlatformInstance.transform.localScale = new Vector3(0.5f, 1f, 0.5f); // 5m x 5m platform
            
            var rendPlatform = roofPlatformInstance.GetComponent<Renderer>();
            if (rendPlatform != null) rendPlatform.enabled = false;

            // 2. Lò Sưởi Ấm (Campfire)
            roofCampfireInstance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            roofCampfireInstance.name = "RoofCampfire";
            roofCampfireInstance.transform.position = center + new Vector3(-1.2f, 2.85f, 1.2f);
            roofCampfireInstance.transform.localScale = new Vector3(0.4f, 0.15f, 0.4f);
            Destroy(roofCampfireInstance.GetComponent<Collider>());
            var rendCamp = roofCampfireInstance.GetComponent<Renderer>();
            if (rendCamp != null) rendCamp.material.color = Color.red;

            // 3. Xô nước mưa (Rainwater Collector)
            roofWaterCollectorInstance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            roofWaterCollectorInstance.name = "RoofWaterCollector";
            roofWaterCollectorInstance.transform.position = center + new Vector3(1.2f, 2.9f, 1.2f);
            roofWaterCollectorInstance.transform.localScale = new Vector3(0.3f, 0.25f, 0.3f);
            Destroy(roofWaterCollectorInstance.GetComponent<Collider>());
            var rendWater = roofWaterCollectorInstance.GetComponent<Renderer>();
            if (rendWater != null) rendWater.material.color = Color.grey;

            // 4. Bếp gas mini (Mini Stove)
            roofStoveInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofStoveInstance.name = "RoofMiniStove";
            roofStoveInstance.transform.position = center + new Vector3(1.2f, 2.85f, -1.2f);
            roofStoveInstance.transform.localScale = new Vector3(0.4f, 0.15f, 0.4f);
            Destroy(roofStoveInstance.GetComponent<Collider>());
            var rendStove = roofStoveInstance.GetComponent<Renderer>();
            if (rendStove != null) rendStove.material.color = Color.grey;

            campfireBurnTime = 45f;
            collectedRainwater = 0f;
            playerBodyTemp = 37f;
            lastCrateSpawnTime = Time.time;
        }

        private void CleanupRoofSurvivalObjects()
        {
            if (roofPlatformInstance != null) { Destroy(roofPlatformInstance); roofPlatformInstance = null; }
            if (roofCampfireInstance != null) { Destroy(roofCampfireInstance); roofCampfireInstance = null; }
            if (roofWaterCollectorInstance != null) { Destroy(roofWaterCollectorInstance); roofWaterCollectorInstance = null; }
            if (roofStoveInstance != null) { Destroy(roofStoveInstance); roofStoveInstance = null; }

            foreach (var crate in spawnedCrates)
            {
                if (crate != null) Destroy(crate);
            }
            spawnedCrates.Clear();
        }

        private void UpdateRoofSurvivalGameplay()
        {
            if (!isOnRoof) return;

            if (GameManager.Instance == null || WeatherManager.Instance == null) return;

            // 1. Cập nhật lò sưởi
            if (roofCampfireInstance != null)
            {
                campfireBurnTime = Mathf.Max(0f, campfireBurnTime - Time.deltaTime);
                var rend = roofCampfireInstance.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = campfireBurnTime > 0f ? new Color(1.0f, 0.3f, 0f) : Color.black;
                }
            }

            // 2. Hứng nước mưa sạch
            if (roofWaterCollectorInstance != null && WeatherManager.Instance.RainIntensity > 0.1f)
            {
                collectedRainwater = Mathf.Min(5.0f, collectedRainwater + WeatherManager.Instance.RainIntensity * 0.05f * Time.deltaTime);
                var rend = roofWaterCollectorInstance.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = Color.Lerp(Color.grey, new Color(0f, 0.5f, 1.0f), collectedRainwater / 5.0f);
                }
            }

            // 3. Lạnh buốt & Thân nhiệt
            if (PlayerStats.Instance != null)
            {
                bool isNearFire = roofCampfireInstance != null && campfireBurnTime > 0f && Vector3.Distance(transform.position, roofCampfireInstance.transform.position) <= 2.2f && transform.position.y >= 2.0f;
                if (isNearFire)
                {
                    playerBodyTemp = Mathf.Min(37.0f, playerBodyTemp + 0.4f * Time.deltaTime);
                    PlayerStats.Instance.ApplyColdStress(-6.0f * Time.deltaTime);
                }
                else
                {
                    // Lạnh hơn nếu dầm nước ở dưới nhà
                    float tempLoss = transform.position.y < 2.0f ? 0.25f : 0.04f;
                    playerBodyTemp = Mathf.Max(32.0f, playerBodyTemp - tempLoss * Time.deltaTime);
                    
                    float coldIncrease = transform.position.y < 2.0f ? 15.0f : 2.5f;
                    PlayerStats.Instance.ApplyColdStress(coldIncrease * Time.deltaTime);
                }

                if (playerBodyTemp < 35.0f)
                {
                    PlayerStats.Instance.ModifyHealth(-1.8f * Time.deltaTime);
                    PlayerStats.Instance.ModifyMorale(-1.2f * Time.deltaTime);
                    if (UnityEngine.Random.value < 0.002f)
                    {
                        PlayerStats.Instance.TriggerAlert("⚠️ THÂN NHIỆT HẠ THẤP! Hãy di chuyển lại gần lò sưởi ấm hoặc ăn uống!");
                    }
                }
            }

            // 4. Sinh hòm tiếp tế
            if (Time.time - lastCrateSpawnTime > 22.0f)
            {
                lastCrateSpawnTime = Time.time;
                SpawnSupplyCrate();
            }

            // 5. Di chuyển hòm tiếp tế trôi nổi
            float waterY = WeatherManager.Instance.FloodLevel > 0.5f ? WeatherManager.Instance.FloodLevel : 0.5f;
            for (int i = spawnedCrates.Count - 1; i >= 0; i--)
            {
                var crate = spawnedCrates[i];
                if (crate == null)
                {
                    spawnedCrates.RemoveAt(i);
                    continue;
                }

                crate.transform.position += new Vector3(0f, 0f, -1.0f) * Time.deltaTime;
                Vector3 pos = crate.transform.position;
                pos.y = waterY + 0.1f;
                crate.transform.position = pos;

                if (crate.transform.position.z < transform.position.z - 8.0f)
                {
                    Destroy(crate);
                    spawnedCrates.RemoveAt(i);
                }
            }

            // 6. Tương tác HUD
            UpdateRoofInteractionPrompts();
        }

        private void SpawnSupplyCrate()
        {
            GameObject houseObj = GameObject.Find("Thanh_House");
            Vector3 center = houseObj != null ? houseObj.transform.position : transform.position;
            
            Vector3 spawnPos = center + new Vector3(UnityEngine.Random.Range(-5f, 5f), 1f, 10f);
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "SupplyCrate_Floated";
            crate.transform.position = spawnPos;
            crate.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            
            var rend = crate.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = new Color(0.6f, 0.4f, 0.2f);
            }
            
            spawnedCrates.Add(crate);
        }

        private void UpdateRoofInteractionPrompts()
        {
            if (SownInStone.UI.SurvivalUIManager.Instance == null) return;

            string prompt = "";

            // Nếu người chơi bị rơi xuống lũ
            if (isOnRoof && transform.position.y < 2.0f)
            {
                GameObject houseObj = GameObject.Find("Thanh_House");
                Vector3 doorPos = houseObj != null ? houseObj.transform.TransformPoint(new Vector3(0f, 0f, 2.0f)) : new Vector3(10.66f, 0f, -8.0f);
                if (Vector3.Distance(transform.position, doorPos) <= 2.5f)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt("[E] Leo lên nóc nhà lánh nạn");
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        Vector3 roofPos = houseObj != null ? houseObj.transform.position + new Vector3(0f, 3.4f, 0f) : new Vector3(10.66f, 3.4f, -10.0f);
                        transform.position = roofPos;
                        if (rb != null)
                        {
                            rb.linearVelocity = Vector3.zero;
                            rb.position = roofPos;
                        }
                        SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("🪜 Đã leo lên nóc nhà lánh nạn thành công!");
                    }
                    return;
                }
            }
            else
            {
                // Kiểm tra hòm tiếp tế
                GameObject nearestCrate = null;
                float bestDist = 3.5f;
                foreach (var crate in spawnedCrates)
                {
                    if (crate == null) continue;
                    float dist = Vector3.Distance(transform.position, crate.transform.position);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        nearestCrate = crate;
                    }
                }

                if (nearestCrate != null)
                {
                    prompt = "[E] Vớt hòm gỗ tiếp tế trôi dạt";
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        spawnedCrates.Remove(nearestCrate);
                        Destroy(nearestCrate);
                        
                        SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_place_object");
                        
                        int rand = UnityEngine.Random.Range(0, 3);
                        if (rand == 0 && StorageManager.Instance != null && noodlesItem != null)
                        {
                            StorageManager.Instance.AddItem(noodlesItem, 2);
                            SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("📦 VỚT TIẾP TẾ: Nhận được 2 gói Mì Tôm Cứu Trợ!");
                        }
                        else if (rand == 1 && StorageManager.Instance != null)
                        {
                            ItemData board = StorageManager.Instance.GetItemDataByID("item_flood_board");
#if UNITY_EDITOR
                            if (board == null) board = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_flood_board.asset");
#endif
                            if (board != null)
                            {
                                StorageManager.Instance.AddItem(board, 1);
                            }
                            SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("📦 VỚT TIẾP TẾ: Nhận được 1 Tấm vách gỗ để làm củi đốt!");
                        }
                        else if (StorageManager.Instance != null)
                        {
                            ItemData preserved = StorageManager.Instance.GetItemDataByID("item_khoai_gieo");
#if UNITY_EDITOR
                            if (preserved == null) preserved = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_PreservedCrop.asset");
#endif
                            if (preserved != null)
                            {
                                StorageManager.Instance.AddItem(preserved, 2);
                            }
                            SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("📦 VỚT TIẾP TẾ: Nhận được 2 củ Khoai Gieo sấy khô củi lửa!");
                        }
                        return;
                    }
                }
                else
                {
                    // Lò sưởi
                    if (roofCampfireInstance != null && Vector3.Distance(transform.position, roofCampfireInstance.transform.position) <= 2.2f)
                    {
                        string state = campfireBurnTime > 0f ? $"đang ấm (còn {(int)campfireBurnTime} giây)" : "đã tắt ngúm";
                        prompt = $"[E] Thêm củi sưởi ấm ({state})";
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            if (StorageManager.Instance != null)
                            {
                                ItemData board = StorageManager.Instance.GetItemDataByID("item_flood_board");
#if UNITY_EDITOR
                                if (board == null) board = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_flood_board.asset");
#endif
                                
                                ItemData preserved = StorageManager.Instance.GetItemDataByID("item_khoai_gieo");
#if UNITY_EDITOR
                                if (preserved == null) preserved = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_PreservedCrop.asset");
#endif

                                var boardSlot = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == "item_flood_board");
                                var preservedSlot = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == "item_khoai_gieo");

                                if (boardSlot != null && boardSlot.quantity > 0)
                                {
                                    StorageManager.Instance.RemoveItem(board, 1);
                                    campfireBurnTime = Mathf.Min(90f, campfireBurnTime + 45f);
                                    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_place_object");
                                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("🔥 LÒ SƯỞI: Đã đốt 1 Tấm vách gỗ làm củi! Lò sưởi ấm trở lại.");
                                }
                                else if (preservedSlot != null && preservedSlot.quantity > 0)
                                {
                                    StorageManager.Instance.RemoveItem(preserved, 1);
                                    campfireBurnTime = Mathf.Min(90f, campfireBurnTime + 20f);
                                    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_place_object");
                                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("🔥 LÒ SƯỞI: Đã đốt 1 Khoai khô làm củi lửa!");
                                }
                                else
                                {
                                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("❌ Thất bại: Bạn không có sẵn Ván gỗ hoặc Khoai khô trong túi để làm củi đốt!");
                                }
                            }
                            return;
                        }
                    }
                    // Xô nước mưa
                    else if (roofWaterCollectorInstance != null && Vector3.Distance(transform.position, roofWaterCollectorInstance.transform.position) <= 2.2f)
                    {
                        prompt = $"Xô nước mưa sạch: Hứng được {collectedRainwater:F1}/5.0 lít. [E] Múc uống trực tiếp";
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            if (collectedRainwater >= 1f)
                            {
                                collectedRainwater -= 1f;
                                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_eat");
                                if (PlayerStats.Instance != null)
                                {
                                    PlayerStats.Instance.ModifyStamina(15f);
                                    PlayerStats.Instance.ApplyColdStress(-5f);
                                }
                                SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("💧 XÔ NƯỚC MƯA: Đã uống 1 lít nước mưa sạch! (+15 Thể lực, -5 Lạnh)");
                            }
                            else
                            {
                                SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("❌ Xô chưa hứng đủ 1.0 lít nước mưa sạch để múc uống!");
                            }
                            return;
                        }
                    }
                    // Bếp gas mini
                    else if (roofStoveInstance != null && Vector3.Distance(transform.position, roofStoveInstance.transform.position) <= 2.2f)
                    {
                        prompt = "[E] Nấu ăn (Cần 1 Mì gói + 1 Lít nước mưa sạch)";
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            if (StorageManager.Instance != null)
                            {
                                var noodlesSlot = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == "item_mi_tom");
                                if (noodlesSlot != null && noodlesSlot.quantity > 0 && collectedRainwater >= 1.0f)
                                {
                                    StorageManager.Instance.RemoveItem(noodlesItem, 1);
                                    collectedRainwater -= 1.0f;
                                    
                                    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_eat");
                                    if (PlayerStats.Instance != null)
                                    {
                                        PlayerStats.Instance.ModifyHealth(40f);
                                        PlayerStats.Instance.ModifyStamina(50f);
                                        PlayerStats.Instance.ModifyMorale(30f);
                                        PlayerStats.Instance.ApplyColdStress(-60f);
                                    }
                                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("🍜 BẾP MINI: Nấu bát mì tôm cứu trợ nóng hổi! (+40 Máu, +50 Thể lực, +30 Tinh thần)");
                                }
                                else
                                {
                                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("❌ Thất bại: Bạn cần tối thiểu 1 gói Mì ăn liền trong balo và 1.0 lít nước mưa trong xô!");
                                }
                            }
                            return;
                        }
                    }
                }
            }

            SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt(prompt);
        }

        private void Update()
        {
            HandleFloodRoofSurvival();
            UpdateRoofSurvivalGameplay();

            // 1. Nhận dữ liệu di chuyển từ WASD / Phím mũi tên (Tự động tương thích cả 2 Input System)
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                float moveX = 0f;
                float moveY = 0f;

                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY = 1f;
                else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY = -1f;

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
                else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;

                moveInput = new Vector2(moveX, moveY);

                // Nhận diện phím Shift để chạy nhanh
                isRunning = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            }
#else
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            // Nhận diện phím Shift để chạy nhanh
            isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif

            // Bình thường hóa vector để tốc độ đi chéo không bị nhanh hơn đi thẳng
            if (moveInput.sqrMagnitude > 0.01f)
            {
                moveInput.Normalize();
                lastMoveDirection = moveInput; // Lưu lại hướng di chuyển cuối cùng để quay mặt
            }

            // 1.5. Thực hiện xoay hướng nhân vật 3D smoothly
            HandleVisualRotation();

            // 2. Cập nhật các thông số Animator phục vụ việc vẽ Sprite vẽ tay động
            UpdateAnimatorParameters();

            // Xử lý chế độ xem trước vị trí đặt vật thể (Ghost Placement Preview)
            if (isPlacingPreview)
            {
                UpdatePlacementPreview();
                return;
            }

            // 4. Lắng nghe phím [E] hoặc [Space] và phím nhanh [1..5]
            bool isUIOpen = false;
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                isUIOpen = SownInStone.UI.SurvivalUIManager.Instance.IsDialogueActive ||
                           SownInStone.UI.SurvivalUIManager.Instance.IsShopOpen ||
                           SownInStone.UI.SurvivalUIManager.Instance.IsQuantityPopupOpen ||
                           SownInStone.UI.SurvivalUIManager.Instance.IsInventoryOpen ||
                           SownInStone.UI.SurvivalUIManager.Instance.IsCommunityOpen ||
                           SownInStone.UI.SurvivalUIManager.Instance.IsWeatherDetailsOpen;
            }

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (!isUIOpen && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
                {
                    TryPerformInteraction();
                }

                if (!isUIOpen)
                {
                    if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) SownInStone.UI.HotbarManager.Instance?.UseHotbarSlot(0);
                    if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) SownInStone.UI.HotbarManager.Instance?.UseHotbarSlot(1);
                    if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) SownInStone.UI.HotbarManager.Instance?.UseHotbarSlot(2);
                    if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame) SownInStone.UI.HotbarManager.Instance?.UseHotbarSlot(3);
                    if (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame) SownInStone.UI.HotbarManager.Instance?.UseHotbarSlot(4);
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.F2))
            {
                GameManager.Instance?.SetTime(22.95f);
                SownInStone.UI.SurvivalUIManager.Instance?.ShowHUDToast("⚙️ [DEBUG] Đã tua thời gian sang ban đêm (22:57)! Chuẩn bị nhận cảnh báo 23:00!");
            }

            if (!isUIOpen && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
            {
                TryPerformInteraction();
            }

            if (!isUIOpen)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) SownInStone.UI.HotbarManager.Instance?.UseHotbarSlot(0);
                if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) SownInStone.UI.HotbarManager.Instance?.UseHotbarSlot(1);
                if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) SownInStone.UI.HotbarManager.Instance?.UseHotbarSlot(2);
                if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) SownInStone.UI.HotbarManager.Instance?.UseHotbarSlot(3);
                if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) SownInStone.UI.HotbarManager.Instance?.UseHotbarSlot(4);
            }
#endif
            // Cập nhật gợi ý tương tác lên UI
            UpdateInteractionPrompt();

            // Xử lý tiêu hao/hồi phục thể lực
            HandleStaminaDrainOnMove();
        }

        private void FixedUpdate()
        {
            // Khôi phục vị trí nếu nhân vật lỡ bị rơi ra ngoài map dưới mặt đất
            if (transform.position.y < -50f)
            {
                GameObject houseObj = GameObject.Find("Thanh_House");
                Vector3 spawnPos = houseObj != null ? houseObj.transform.position + new Vector3(0f, 0.565f, -4.2f) : new Vector3(10.66f, 0.565f, -14.2f);
                transform.position = spawnPos;
                if (rb != null)
                {
                    rb.position = spawnPos;
                    rb.linearVelocity = Vector3.zero;
                }
            }

            // Khóa di chuyển nếu đang thực hiện động tác trồng trọt
            if (isPerformingAction)
            {
                if (rb != null)
                {
                    rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                }
                debugCurrentSpeed = 0f;
                debugTargetMoveDir = Vector3.zero;
                return;
            }

            // Di chuyển nhân vật thông qua cơ chế vật lý Rigidbody 3D tránh đi xuyên tường
            // Tính toán tốc độ cơ bản tùy theo việc người chơi giữ Shift để chạy nhanh
            float baseSpeed = isRunning ? runSpeed : walkSpeed;
            float currentSpeed = baseSpeed;

            if (WeatherManager.Instance != null && WeatherManager.Instance.FloodLevel > 0.5f)
            {
                // Giảm 40% tốc độ di chuyển khi lội nước lụt sâu hơn 0.5m (flood penalty)
                currentSpeed *= 0.6f;
            }

            debugCurrentSpeed = currentSpeed;

            // Tính toán hướng di chuyển camera-relative trong không gian 3D
            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            if (cameraTransform != null && moveInput.sqrMagnitude > 0.01f)
            {
                Vector3 cameraForward = cameraTransform.forward;
                Vector3 cameraRight = cameraTransform.right;

                cameraForward.y = 0f;
                cameraRight.y = 0f;

                cameraForward.Normalize();
                cameraRight.Normalize();

                Vector3 targetMoveDir = cameraForward * moveInput.y + cameraRight * moveInput.x;
                if (targetMoveDir.sqrMagnitude > 0.01f)
                {
                    targetMoveDir.Normalize();
                }

                debugTargetMoveDir = targetMoveDir;
                rb.linearVelocity = new Vector3(targetMoveDir.x * currentSpeed, rb.linearVelocity.y, targetMoveDir.z * currentSpeed);
            }
            else if (moveInput.sqrMagnitude > 0.01f)
            {
                // Fallback nếu không tìm thấy Camera chính
                Vector3 fallbackMoveDir = new Vector3(moveInput.x, 0f, moveInput.y);
                if (fallbackMoveDir.sqrMagnitude > 0.01f)
                {
                    fallbackMoveDir.Normalize();
                }
                debugTargetMoveDir = fallbackMoveDir;
                rb.linearVelocity = new Vector3(fallbackMoveDir.x * currentSpeed, rb.linearVelocity.y, fallbackMoveDir.z * currentSpeed);
            }
            else
            {
                debugTargetMoveDir = Vector3.zero;
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }

            if (isOnRoof)
            {
                // Khi trên nóc nhà: chỉ giữ vận tốc Y = 0 để không rơi xuống.
                // Việc khóa cứng tọa độ X/Z đã được xóa để người chơi đi chuyển tự do khắp nóc nhà.
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            }
            else if (!isInsideHouse)
            {
                // Tự động snap độ cao Y của nhân vật theo địa hình Terrain thực tế ngoài trời
                // giúp đi lên/xuống dốc tự nhiên mà không cần trọng lực Rigidbody (tránh rơi tự do ra ngoài map)
                if (UnityEngine.Terrain.activeTerrain != null)
                {
                    float terrainHeight = UnityEngine.Terrain.activeTerrain.SampleHeight(transform.position) + UnityEngine.Terrain.activeTerrain.transform.position.y;
                    Vector3 pos = transform.position;
                    pos.y = terrainHeight + 0.565f; // Thêm offset 0.565f để nhân vật đứng vững trên bề mặt thay vì lún nửa người
                    transform.position = pos;
                    if (rb != null)
                    {
                        rb.position = pos;
                        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                    }
                }
            }
        }

        /// <summary>
        /// Cập nhật các biến số cho Animator 2D (Blend Tree di chuyển 4 hoặc 8 hướng).
        /// </summary>
        private void UpdateAnimatorParameters()
        {
            if (animator == null) return;

            float targetAnimSpeed = 0f;
            if (isPerformingAction)
            {
                // Tốc độ di chuyển về 0 khi đang thực hiện động tác để Animator quay về Idle
                targetAnimSpeed = 0f;
            }
            else if (moveInput.sqrMagnitude > 0.01f)
            {
                // Animator Speed mapping: 0.5f khi đi bộ, 1.0f khi chạy nhanh
                targetAnimSpeed = isRunning ? 1.0f : 0.5f;
            }
            else
            {
                // 0.0f khi đứng im (Idle)
                targetAnimSpeed = 0f;
            }

            float currentAnimSpeed = animator.GetFloat("Speed");
            float newAnimSpeed = Mathf.MoveTowards(currentAnimSpeed, targetAnimSpeed, Time.deltaTime * 5f);
            SetAnimFloat("Speed", newAnimSpeed);

            // Keep other parameters for compatibility/harmlessness
            SetAnimFloat("Horizontal", moveInput.x);
            SetAnimFloat("Vertical", moveInput.y);
            SetAnimFloat("LastHorizontal", lastMoveDirection.x);
            SetAnimFloat("LastVertical", lastMoveDirection.y);
        }

        /// <summary>
        /// Tiêu hao thể lực nếu chạy liên tục ngoài nắng nóng mùa hè (Gió Lào).
        /// </summary>
        private void HandleStaminaDrainOnMove()
        {
            if (PlayerStats.Instance == null) return;

            if (moveInput.sqrMagnitude > 0.01f)
            {
                // Nếu đang di chuyển giữa trưa nắng Gió Lào
                if (WeatherManager.Instance != null && WeatherManager.Instance.currentVisualWeather == WeatherType.GioLao && WeatherManager.Instance.Temperature > 38f)
                {
                    // Tiêu hao thêm 1.5 Stamina mỗi giây khi di chuyển ngoài nắng nóng
                    PlayerStats.Instance.ModifyStamina(-1.5f * Time.deltaTime);
                }
            }
            else
            {
                // Hồi phục thể lực khi đứng yên tại chỗ (+3 thể lực/giây)
                PlayerStats.Instance.ModifyStamina(3f * Time.deltaTime);
            }
        }

        /// <summary>
        /// Coroutine tạm thời khóa di chuyển của người chơi khi đang làm vườn.
        /// </summary>
        private System.Collections.IEnumerator LockMovementForSeconds(float duration)
        {
            isPerformingAction = true;
            yield return new WaitForSeconds(duration);
            isPerformingAction = false;
        }

        /// <summary>
        /// Khóa di chuyển của người chơi trong khoảng thời gian nhất định.
        /// </summary>
        public void LockMovement(float duration)
        {
            StartCoroutine(LockMovementForSeconds(duration));
        }

        /// <summary>
        /// Quét tìm các đối tượng xung quanh trong bán kính quét và thực hiện tương tác.
        /// Ưu tiên đối tượng ở gần nhất.
        /// </summary>
        private void TryPerformInteraction()
        {
            if (SownInStone.UI.SurvivalUIManager.Instance != null && SownInStone.UI.SurvivalUIManager.Instance.IsDialogueActive)
            {
                return;
            }

            if (isPerformingAction) return;



            Debug.Log("[PLAYER] Bấm phím tương tác [E]!");

            // Tương tác trực tiếp vào/ra nhà trú ẩn và các đồ nội thất trong nhà
            if (isInsideHouse)
            {
                if (houseInteriorInstance != null)
                {
                    Transform mainDoor = houseInteriorInstance.transform.Find("HouseFrame/MainDoor");
                    if (mainDoor == null)
                    {
                        foreach (var t in houseInteriorInstance.GetComponentsInChildren<Transform>())
                        {
                            if (t.name == "MainDoor") { mainDoor = t; break; }
                        }
                    }
                    if (mainDoor != null && Vector3.Distance(transform.position, mainDoor.position) <= 1.8f)
                    {
                        ExitHouse();
                        return;
                    }

                    Transform giuong = houseInteriorInstance.transform.Find("HouseFrame/GiuongNguModel");
                    if (giuong == null)
                    {
                        foreach (var t in houseInteriorInstance.GetComponentsInChildren<Transform>())
                        {
                            if (t.name == "GiuongNguModel") { giuong = t; break; }
                        }
                    }
                    if (giuong != null && Vector3.Distance(transform.position, giuong.position) <= 2.5f)
                    {
                        StartCoroutine(PerformSleepSequence());
                        return;
                    }

                    Transform bep = houseInteriorInstance.transform.Find("HouseFrame/BepGasModel");
                    if (bep == null)
                    {
                        foreach (var t in houseInteriorInstance.GetComponentsInChildren<Transform>())
                        {
                            if (t.name == "BepGasModel") { bep = t; break; }
                        }
                    }
                    if (bep != null && Vector3.Distance(transform.position, bep.position) <= 2.5f)
                    {
                        KitchenHearth hearth = bep.GetComponent<KitchenHearth>();
                        if (hearth == null)
                        {
                            hearth = bep.gameObject.AddComponent<KitchenHearth>();
                        }
                        hearth.Interact();
                        return;
                    }

                    Transform altar = houseInteriorInstance.transform.Find("HouseFrame/AncestralAltarModel");
                    if (altar == null)
                    {
                        foreach (var t in houseInteriorInstance.GetComponentsInChildren<Transform>())
                        {
                            if (t.name == "AncestralAltarModel") { altar = t; break; }
                        }
                    }
                    if (altar != null && Vector3.Distance(transform.position, altar.position) <= 2.5f)
                    {
                        if (PlayerStats.Instance != null)
                        {
                            PlayerStats.Instance.ModifyMorale(30f);
                        }
                        SownInStone.UI.SurvivalUIManager.Instance?.ShowHUDToast("⛩️ Đã thắp nhang cầu nguyện thành kính! Phục hồi tinh thần.");
                        return;
                    }

                    Transform ruong = houseInteriorInstance.transform.Find("HouseFrame/RuongDoModel");
                    if (ruong == null)
                    {
                        foreach (var t in houseInteriorInstance.GetComponentsInChildren<Transform>())
                        {
                            if (t.name == "RuongDoModel") { ruong = t; break; }
                        }
                    }
                    if (ruong != null && Vector3.Distance(transform.position, ruong.position) <= 2.5f)
                    {
                        if (SownInStone.Storage.StorageManager.Instance != null)
                        {
                            SownInStone.Storage.StorageManager.Instance.AutoDepositOverflowItems();
                        }
                        SownInStone.UI.SurvivalUIManager.Instance?.OpenStorageChestUI();
                        SownInStone.UI.SurvivalUIManager.Instance?.ShowHUDToast("📦 Đã mở rương đồ dự trữ!");
                        return;
                    }
                }
                return;
            }
            else
            {
                GameObject thanhHouse = GameObject.Find("Thanh_House");
                Vector3 doorPos = thanhHouse != null ? thanhHouse.transform.TransformPoint(new Vector3(0f, 0f, 2.0f)) : new Vector3(10.66f, 0f, -8.0f);
                if (Vector3.Distance(transform.position, doorPos) <= 2.5f)
                {
                    if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive && TutorialManager.Instance.currentStage == TutorialManager.TutorialStage.RescuingNPCs)
                    {
                        if (carriedNPC != null)
                        {
                            DropCarriedNPCAtThanhHouse();
                        }
                        else
                        {
                            SownInStone.UI.SurvivalUIManager.Instance?.ShowHUDToast("⚠️ Bão lũ đang dồn dập! Hãy chạy đi cứu giúp dân làng trước!");
                        }
                    }
                    else
                    {
                        EnterHouse();
                    }
                    return;
                }
            }

            // Quét hình cầu vật lý 3D xung quanh người chơi và lấy đối tượng có thể tương tác gần nhất
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactRadius, interactableLayers);
            Collider closestCollider = GetClosestInteractable(colliders);

            if (closestCollider != null)
            {
                GameObject target = closestCollider.gameObject;
                Debug.Log($"[PLAYER] Đã tìm thấy đối tượng tương tác gần nhất: {target.name}");

                // 1. Tương tác với Dân làng (NPC Bác Năm, O Thắm) - Đã vô hiệu hóa tương tác trực tiếp E/Space theo yêu cầu thiết kế mới
                NPCCharacter npc = target.GetComponent<NPCCharacter>();
                if (npc != null)
                {
                    // Nếu khung đối thoại mới đang mở và không có lựa chọn đang chờ, nhấn E/Space vẫn có thể đóng nó lại
                    if (SownInStone.UI.SurvivalUIManager.Instance != null && SownInStone.UI.SurvivalUIManager.Instance.IsDialogueActive)
                    {
                        if (!SownInStone.UI.SurvivalUIManager.Instance.IsChoiceActive)
                        {
                            SownInStone.UI.SurvivalUIManager.Instance.CloseDialogue();
                        }
                    }
                    return;
                }


                // 2. Tương tác với Bàn thờ gia tiên (Thắp nhang)
                AncestralAltar altar = target.GetComponent<AncestralAltar>();
                if (altar != null)
                {
                    altar.ActionBurnIncense();
                    return;
                }

                // 2.3. Tương tác qua Interface IInteractable (như Bếp ga, v.v.)
                IInteractable interactable = target.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                    return;
                }

                // 2.5. Tương tác với Thuyền thúng
                Coracle boat = target.GetComponent<Coracle>();
                if (boat != null)
                {
                    boat.Interact(this);
                    return;
                }

                // 2.7. Tương tác với Vũng bùn lầy
                MudPuddle mud = target.GetComponent<MudPuddle>();
                if (mud != null)
                {
                    mud.Interact();
                    return;
                }

                // 3. Tương tác với Ô đất ruộng vườn (Tưới nước, nhặt đá, gieo hạt, thu hoạch)
                SoilCell soil = target.GetComponent<SoilCell>();
                if (soil != null)
                {
                    HandleSoilInteraction(soil);
                    return;
                }
            }
            else
            {
                Debug.Log("[PLAYER] Không có đối tượng nào ở gần để tương tác.");
            }
        }

        /// <summary>
        /// Xử lý logic tương tác thông minh với ô ruộng đất.
        /// </summary>
        private void HandleSoilInteraction(SoilCell soil)
        {
            if (PlayerStats.Instance == null) return;

            SoilCell activeSoil = soil.parentField != null ? soil.parentField : soil;

            // Priority 1: Clear rocks
            int cellsWithRocks = 0;
            if (activeSoil.IsParentField)
            {
                foreach (var child in activeSoil.childCells)
                {
                    if (child != null && child.RockDensity > 0f) cellsWithRocks++;
                }
            }
            else
            {
                if (activeSoil.RockDensity > 0f) cellsWithRocks = 1;
            }

            if (cellsWithRocks > 0)
            {
                float staminaCost = 8f * cellsWithRocks;
                if (PlayerStats.Instance.CurrentStamina >= 8f)
                {
                    float actualCost = Mathf.Min(PlayerStats.Instance.CurrentStamina, staminaCost);
                    PlayerStats.Instance.ModifyStamina(-actualCost);
                    SetAnimTrigger("Dig");
                    StartCoroutine(LockMovementForSeconds(digActionDuration));
                    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_clear_rocks");
                    activeSoil.ActionClearRocks(999f);
                    Debug.Log($"Cleared rocks on {cellsWithRocks} cell(s)");
                    PlayerStats.Instance.TriggerAlert($"Đã nhặt đá cải tạo {cellsWithRocks} ô ruộng!");
                }
                else
                {
                    Debug.LogWarning("[PLAYER] Không đủ thể lực để nhặt đá!");
                    PlayerStats.Instance.TriggerAlert("Không đủ thể lực để dọn đá!");
                }
                return;
            }

            // Priority 2: Harvest crop
            bool hasReadyCrops = false;
            if (activeSoil.IsParentField)
            {
                foreach (var child in activeSoil.childCells)
                {
                    if (child != null && child.plantedCrop != null && child.plantedCrop.IsReadyToHarvest())
                    {
                        hasReadyCrops = true;
                        break;
                    }
                }
            }
            else
            {
                hasReadyCrops = (activeSoil.plantedCrop != null && activeSoil.plantedCrop.IsReadyToHarvest());
            }

            if (hasReadyCrops)
            {
                SetAnimTrigger("Harvest");
                StartCoroutine(LockMovementForSeconds(harvestActionDuration));
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_harvest");

                int totalYield = 0;
                ItemData harvestItem = null;
                bool isSilted = false;

                if (activeSoil.IsParentField)
                {
                    foreach (var child in activeSoil.childCells)
                    {
                        if (child != null && child.plantedCrop != null && child.plantedCrop.IsReadyToHarvest())
                        {
                            if (harvestItem == null) harvestItem = child.plantedCrop.cropData.HarvestedItem;
                            int yield = child.plantedCrop.ActionHarvest();
                            if (child.quality == SoilQuality.PhuSa || (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.PhuSa))
                            {
                                yield *= 2;
                                isSilted = true;
                            }
                            totalYield += yield;
                        }
                    }
                }
                else
                {
                    harvestItem = activeSoil.plantedCrop.cropData.HarvestedItem;
                    int yield = activeSoil.plantedCrop.ActionHarvest();
                    if (activeSoil.quality == SoilQuality.PhuSa || (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.PhuSa))
                    {
                        yield *= 2;
                        isSilted = true;
                    }
                    totalYield = yield;
                }

                if (totalYield > 0 && StorageManager.Instance != null && harvestItem != null)
                {
                    StorageManager.Instance.AddItem(harvestItem, totalYield);
                    
                    if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                    {
                        TutorialManager.Instance.OnCropHarvested();
                    }

                    Debug.Log($"[PLAYER] Thu hoạch thành công ruộng cây: {harvestItem.ItemName} x{totalYield}!");
                    string itemName = !string.IsNullOrEmpty(harvestItem.ItemName) ? harvestItem.ItemName : "Nông sản";
                    
                    if (isSilted)
                    {
                        string siltMsg = $"Đất phù sa màu mỡ giúp nhân đôi sản lượng! Bạn thu hoạch được +{totalYield} {itemName}.";
                        if (SownInStone.UI.SurvivalUIManager.Instance != null)
                        {
                            SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast(siltMsg);
                        }
                        PlayerStats.Instance.TriggerAlert(siltMsg);
                    }
                    else
                    {
                        string harvestMsg = $"Thu hoạch thành công: +{totalYield} {itemName}";
                        PlayerStats.Instance.TriggerAlert(harvestMsg);
                    }
                }
                return;
            }

            // Priority 3: Plant seed
            bool canPlant = false;
            int emptySlots = 0;
            if (activeSoil.IsParentField)
            {
                foreach (var child in activeSoil.childCells)
                {
                    if (child != null && child.plantedCrop == null) emptySlots++;
                }
                canPlant = (emptySlots > 0);
            }
            else
            {
                canPlant = (activeSoil.plantedCrop == null);
                if (canPlant) emptySlots = 1;
            }

            int availableSeeds = 0;
            if (testSeedData != null && seedItem != null && StorageManager.Instance != null)
            {
                var slots = StorageManager.Instance.GetStorageSlots();
                var seedSlot = slots.Find(s => s.item.ItemID == seedItem.ItemID);
                if (seedSlot != null && seedSlot.quantity > 0)
                {
                    availableSeeds = seedSlot.quantity;
                }
            }

            bool isRestrictedByTutorial = TutorialManager.Instance != null && 
                                           TutorialManager.Instance.isTutorialActive && 
                                           TutorialManager.Instance.subTask2Completed && 
                                           !TutorialManager.Instance.subTask4Completed && 
                                           TutorialManager.Instance.currentStage != TutorialManager.TutorialStage.SellCrops;

            if (canPlant && availableSeeds > 0 && !isRestrictedByTutorial)
            {
                if (testSeedData != null && seedItem != null)
                {
                    var plantable = GetPlantableCellsNear(activeSoil);
                    int bulkCount = Mathf.Min(availableSeeds, plantable.Count);

                    string title = "Gieo hạt giống";
                    string msg = "Bạn muốn làm gì?";

                    SownInStone.UI.SurvivalUIManager.Instance.ShowDialogueWithChoices(
                        title,
                        msg,
                        $"Trồng hàng loạt ({bulkCount} ô)",
                        () => {
                            PerformBulkPlant(activeSoil, bulkCount);
                        },
                        "Chỉ trồng ô này",
                        () => {
                            PerformSinglePlant(activeSoil);
                        },
                        "Hủy",
                        () => {
                            SownInStone.UI.SurvivalUIManager.Instance.CloseDialogue();
                        }
                    );
                }
                else
                {
                    Debug.LogWarning("[PLAYER] Chưa gán Hạt giống (TestSeedData) hoặc Vật phẩm Hạt giống (SeedItem) trong PlayerController!");
                }
                return;
            }

            // Priority 4: Water soil
            int dryCells = 0;
            if (activeSoil.IsParentField)
            {
                foreach (var child in activeSoil.childCells)
                {
                    if (child != null && child.Moisture < 40f) dryCells++;
                }
            }
            else
            {
                if (activeSoil.Moisture < 40f) dryCells = 1;
            }

            if (dryCells > 0)
            {
                float staminaCost = 3f * dryCells;
                if (PlayerStats.Instance.CurrentStamina >= 3f)
                {
                    float actualCost = Mathf.Min(PlayerStats.Instance.CurrentStamina, staminaCost);
                    PlayerStats.Instance.ModifyStamina(-actualCost);
                    SetAnimTrigger("Water");
                    StartCoroutine(LockMovementForSeconds(waterActionDuration));
                    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_water");
                    activeSoil.ActionWaterSoil(50f);
                    Debug.Log($"Watered {dryCells} cell(s)");
                    PlayerStats.Instance.TriggerAlert($"Đã tưới nước cho {dryCells} ô ruộng!");
                }
                return;
            }

            Debug.Log("[PLAYER] Ô đất đang ở trạng thái tốt, không cần thao tác thêm.");
        }

        public void OpenTradeMenu(NPCCharacter npc)
        {
            if (SownInStone.UI.SurvivalUIManager.Instance == null) return;
            SownInStone.UI.SurvivalUIManager.Instance.ToggleShop(true);
        }

        /// <summary>
        /// Xoay hướng mô hình 3D mượt mà theo hướng di chuyển WASD.
        /// </summary>
        private void HandleVisualRotation()
        {
            if (characterVisual == null || moveInput.sqrMagnitude < 0.01f) return;

            Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
            if (cameraTransform != null && rotationAxis == RotationAxis.RotateAroundY_3D)
            {
                Vector3 cameraForward = cameraTransform.forward;
                Vector3 cameraRight = cameraTransform.right;

                cameraForward.y = 0f;
                cameraRight.y = 0f;

                cameraForward.Normalize();
                cameraRight.Normalize();

                Vector3 targetMoveDir = cameraForward * moveInput.y + cameraRight * moveInput.x;
                if (targetMoveDir.sqrMagnitude > 0.01f)
                {
                    targetMoveDir.Normalize();
                }

                // Xoay quanh trục Y cục bộ dựa trên hướng di chuyển camera-relative thực tế
                float targetAngle = Mathf.Atan2(targetMoveDir.x, targetMoveDir.z) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(initialVisualLocalX, targetAngle, initialVisualLocalZ);
                characterVisual.localRotation = Quaternion.Slerp(characterVisual.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                if (rotationAxis == RotationAxis.RotateAroundZ_2D)
                {
                    // Game 2D truyền thống: Xoay quanh trục Z cục bộ
                    float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
                    // Trừ 90 độ nếu mô hình mặc định hướng lên trên, giữ lại góc xoay ban đầu ở X và Z
                    Quaternion targetRotation = Quaternion.Euler(initialVisualLocalX, initialVisualLocalZ, targetAngle - 90f);
                    characterVisual.localRotation = Quaternion.Slerp(characterVisual.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                else
                {
                    // Game 3D Top-down truyền thống không camera
                    float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
                    Quaternion targetRotation = Quaternion.Euler(initialVisualLocalX, targetAngle, initialVisualLocalZ);
                    characterVisual.localRotation = Quaternion.Slerp(characterVisual.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }

        private void UpdateInteractionPrompt()
        {
            if (SownInStone.UI.SurvivalUIManager.Instance == null) return;

            if (isInsideHouse)
            {
                string prompt = "";
                if (houseInteriorInstance != null)
                {
                    Transform mainDoor = houseInteriorInstance.transform.Find("HouseFrame/MainDoor");
                    if (mainDoor == null)
                    {
                        foreach (var t in houseInteriorInstance.GetComponentsInChildren<Transform>())
                        {
                            if (t.name == "MainDoor") { mainDoor = t; break; }
                        }
                    }
                    if (mainDoor != null && Vector3.Distance(transform.position, mainDoor.position) <= 1.8f)
                    {
                        prompt = "[E] Ra ngoài sân";
                    }
                    else
                    {
                        Transform giuong = houseInteriorInstance.transform.Find("HouseFrame/GiuongNguModel");
                        if (giuong == null)
                        {
                            foreach (var t in houseInteriorInstance.GetComponentsInChildren<Transform>())
                            {
                                if (t.name == "GiuongNguModel") { giuong = t; break; }
                            }
                        }
                        if (giuong != null && Vector3.Distance(transform.position, giuong.position) <= 2.5f)
                        {
                            prompt = "[E] Đi ngủ (Phục hồi 100%)";
                        }
                        else
                        {
                            Transform bep = houseInteriorInstance.transform.Find("HouseFrame/BepGasModel");
                            if (bep == null)
                            {
                                foreach (var t in houseInteriorInstance.GetComponentsInChildren<Transform>())
                                {
                                    if (t.name == "BepGasModel") { bep = t; break; }
                                }
                            }
                            if (bep != null && Vector3.Distance(transform.position, bep.position) <= 2.5f)
                            {
                                prompt = "[E] Nấu nướng / Chế biến";
                            }
                            else
                            {
                                Transform altar = houseInteriorInstance.transform.Find("HouseFrame/AncestralAltarModel");
                                if (altar == null)
                                {
                                    foreach (var t in houseInteriorInstance.GetComponentsInChildren<Transform>())
                                    {
                                        if (t.name == "AncestralAltarModel") { altar = t; break; }
                                    }
                                }
                                if (altar != null && Vector3.Distance(transform.position, altar.position) <= 2.5f)
                                {
                                    prompt = "[E] Thắp nhang";
                                }
                                else
                                {
                                    Transform ruong = houseInteriorInstance.transform.Find("HouseFrame/RuongDoModel");
                                    if (ruong == null)
                                    {
                                        foreach (var t in houseInteriorInstance.GetComponentsInChildren<Transform>())
                                        {
                                            if (t.name == "RuongDoModel") { ruong = t; break; }
                                        }
                                    }
                                    if (ruong != null && Vector3.Distance(transform.position, ruong.position) <= 2.5f)
                                    {
                                        prompt = "[E] Mở rương đồ dự trữ";
                                    }
                                }
                            }
                        }
                    }
                }
                SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt(prompt);
                return;
            }

            GameObject houseObj = GameObject.Find("Thanh_House");
            Vector3 targetDoorPos = houseObj != null ? houseObj.transform.TransformPoint(new Vector3(0f, 0f, 2.0f)) : new Vector3(10.66f, 0f, -8.0f);
            if (Vector3.Distance(transform.position, targetDoorPos) <= 2.5f)
            {
                SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt("[E] Vào bên trong nhà trú ẩn");
                return;
            }

            if (isOnRoof)
            {
                if (currentTargetSoil != null)
                {
                    currentTargetSoil.SetHighlight(false);
                    currentTargetSoil = null;
                }
                SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt("[E] Nghỉ ngơi trên nóc nhà");
                return;
            }

            // Quét hình cầu vật lý 3D xung quanh người chơi và lấy đối tượng tương tác gần nhất
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactRadius, interactableLayers);
            Collider closestCollider = GetClosestInteractable(colliders);

            SoilCell targetSoil = null;
            if (closestCollider != null)
            {
                targetSoil = closestCollider.GetComponent<SoilCell>();
            }

            if (currentTargetSoil != targetSoil)
            {
                if (currentTargetSoil != null)
                {
                    currentTargetSoil.SetHighlight(false);
                }
                currentTargetSoil = targetSoil;
                if (currentTargetSoil != null)
                {
                    currentTargetSoil.SetHighlight(true);
                }
            }
            else if (currentTargetSoil != null)
            {
                currentTargetSoil.SetHighlight(true);
            }

            if (closestCollider != null)
            {
                string prompt = "";
                NPCCharacter npc = closestCollider.GetComponent<NPCCharacter>();
                if (npc != null)
                {
                    prompt = "";
                }
                else
                {
                    AncestralAltar altar = closestCollider.GetComponent<AncestralAltar>();
                    if (altar != null)
                    {
                        prompt = "[E] Thắp nhang bàn thờ";
                    }
                    else
                    {
                        KitchenHearth hearth = closestCollider.GetComponent<KitchenHearth>();
                        if (hearth != null)
                        {
                            prompt = "[E] Nấu nướng / Chế biến";
                        }
                        else
                        {
                            OThamChest chest = closestCollider.GetComponent<OThamChest>();
                            if (chest != null)
                            {
                                prompt = "[E] Cất mì tôm cứu trợ";
                            }
                            else
                            {
                                OThamScatteredItem scatteredItem = closestCollider.GetComponent<OThamScatteredItem>();
                                if (scatteredItem != null)
                                {
                                    prompt = $"[E] Nhặt {scatteredItem.itemDisplayName}";
                                }
                                else
                                {
                                    Coracle boat = closestCollider.GetComponent<Coracle>();
                                    if (boat != null)
                                    {
                                        if (TutorialManager.Instance != null && TutorialManager.Instance.currentStage < TutorialManager.TutorialStage.RescuingNPCs)
                                        {
                                            prompt = "Thuyền thúng (Chưa sử dụng được)";
                                        }
                                        else
                                        {
                                            prompt = $"[{keyInteract}] Lên thuyền thúng";
                                        }
                                    }
                                    else
                                    {
                                        MudPuddle mud = closestCollider.GetComponent<MudPuddle>();
                                        if (mud != null)
                                        {
                                            prompt = $"[{keyInteract}] Dọn dẹp bùn đất";
                                        }
                                        else
                                        {
                                            SoilCell soil = closestCollider.GetComponent<SoilCell>();
                                            if (soil != null)
                                            {
                                                SoilCell activeSoil = soil.parentField != null ? soil.parentField : soil;

                                                int cellsWithRocks = 0;
                                                int emptySlots = 0;
                                                bool hasReadyCrops = false;
                                                int dryCells = 0;

                                                if (activeSoil.IsParentField)
                                                {
                                                    foreach (var child in activeSoil.childCells)
                                                    {
                                                        if (child != null)
                                                        {
                                                            if (child.RockDensity > 0f) cellsWithRocks++;
                                                            if (child.plantedCrop == null) emptySlots++;
                                                            if (child.plantedCrop != null && child.plantedCrop.IsReadyToHarvest()) hasReadyCrops = true;
                                                            if (child.Moisture < 40f) dryCells++;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (activeSoil.RockDensity > 0f) cellsWithRocks = 1;
                                                    if (activeSoil.plantedCrop == null) emptySlots = 1;
                                                    if (activeSoil.plantedCrop != null && activeSoil.plantedCrop.IsReadyToHarvest()) hasReadyCrops = true;
                                                    if (activeSoil.Moisture < 40f) dryCells = 1;
                                                }

                                                int availableSeeds = 0;
                                                if (testSeedData != null && seedItem != null && StorageManager.Instance != null)
                                                {
                                                    var slots = StorageManager.Instance.GetStorageSlots();
                                                    var seedSlot = slots.Find(s => s.item != null && s.item.ItemID == seedItem.ItemID);
                                                    if (seedSlot != null && seedSlot.quantity > 0)
                                                    {
                                                        availableSeeds = seedSlot.quantity;
                                                    }
                                                }

                                                if (cellsWithRocks > 0)
                                                    prompt = "[E] Dọn đá cải tạo ruộng";
                                                else if (hasReadyCrops)
                                                    prompt = "[E] Thu hoạch khoai tươi";
                                                else if (emptySlots > 0 && availableSeeds > 0 && !(TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive && TutorialManager.Instance.subTask2Completed))
                                                    prompt = "[E] Gieo hạt giống khoai";
                                                else if (dryCells > 0)
                                                    prompt = "[E] Tưới nước cho đất ẩm";
                                                else
                                                    prompt = "Đất đã được gieo hạt";

                                                string stateInfo = activeSoil.IsProtectedByFloodBarrier() ? "Được bảo vệ" : (activeSoil.Moisture >= 90f ? "NGẬP ÚNG!" : "An toàn");
                                                prompt += $" (Ẩm: {Mathf.RoundToInt(activeSoil.Moisture)}% - {stateInfo})";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
            }

                SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt(prompt);
            }
            else
            {
                SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt("");
            }
        }

        [Header("--- TRANG PHỤC & THIẾT BỊ ---")]
        public bool isWearingNonLa = false;
        private GameObject activeHatVisual;

        public void EquipNonLa(GameObject hatPrefab)
        {
            isWearingNonLa = true;
            
            // Destroy existing hat visual if any
            if (activeHatVisual != null)
            {
                Destroy(activeHatVisual);
            }

            if (hatPrefab != null)
            {
                // Find head bone or parent to visual
                Transform headBone = FindHeadBone(characterVisual != null ? characterVisual : transform);
                activeHatVisual = Instantiate(hatPrefab);
                activeHatVisual.name = "Equipped_NonLa";
                
                if (headBone != null)
                {
                    activeHatVisual.transform.SetParent(headBone, false);
                    activeHatVisual.transform.localPosition = new Vector3(0f, 0.15f, 0f); // Slight offset above head bone
                    activeHatVisual.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f); // Align standard FBX import
                    activeHatVisual.transform.localScale = Vector3.one * 0.8f;
                }
                else
                {
                    // Fallback to parent transform head level
                    activeHatVisual.transform.SetParent(transform, false);
                    activeHatVisual.transform.localPosition = new Vector3(0f, 1.6f, 0f);
                    activeHatVisual.transform.localRotation = Quaternion.identity;
                    activeHatVisual.transform.localScale = Vector3.one * 0.8f;
                }
            }
        }

        private Transform FindHeadBone(Transform current)
        {
            if (current.name.ToLower().Contains("head") || current.name.ToLower().Contains("bip001 head")) return current;
            for (int i = 0; i < current.childCount; i++)
            {
                Transform found = FindHeadBone(current.GetChild(i));
                if (found != null) return found;
            }
            return null;
        }

        private void SetAnimFloat(string paramName, float value)
        {
            if (animator != null && animatorParams.Contains(paramName))
            {
                animator.SetFloat(paramName, value);
            }
        }

        public void SetAnimTrigger(string paramName)
        {
            if (animator != null && animatorParams.Contains(paramName))
            {
                animator.SetTrigger(paramName);
            }
        }

        private Vector3 debugTargetMoveDir;
        private float debugCurrentSpeed;

        private void OnGUI()
        {
            if (FrameworkDebugUI.Instance == null || !FrameworkDebugUI.Instance.isUIVisible) return;

            GUI.color = Color.red;
            GUI.Box(new Rect(10, 150, 270, 220), "--- PLAYER MOVEMENT DEBUG ---");
            GUI.Label(new Rect(20, 170, 250, 20), $"Pos: {transform.position}");
            GUI.Label(new Rect(20, 190, 250, 20), $"RB Pos: {(rb != null ? rb.position : Vector3.zero)}");
            GUI.Label(new Rect(20, 210, 250, 20), $"Vel: {(rb != null ? rb.linearVelocity : Vector3.zero)}");
            GUI.Label(new Rect(20, 230, 250, 20), $"Input: {moveInput}");
            GUI.Label(new Rect(20, 250, 250, 20), $"TargetMoveDir: {debugTargetMoveDir}");
            GUI.Label(new Rect(20, 270, 250, 20), $"Speed: {debugCurrentSpeed}");
            GUI.Label(new Rect(20, 290, 250, 20), $"Kinematic: {(rb != null ? rb.isKinematic : false)}");
            GUI.Label(new Rect(20, 310, 250, 20), $"Constraints: {(rb != null ? rb.constraints : RigidbodyConstraints.None)}");
            GUI.Label(new Rect(20, 330, 250, 20), $"Cam: {(Camera.main != null ? Camera.main.name : "null")}");
            GUI.Label(new Rect(20, 350, 250, 20), $"RootMotion: {(animator != null ? animator.applyRootMotion.ToString() : "null")}");
        }

        private System.Collections.Generic.List<SownInStone.Agriculture.SoilCell> GetPlantableCellsNear(SownInStone.Agriculture.SoilCell targetCell)
        {
            var list = new System.Collections.Generic.List<SownInStone.Agriculture.SoilCell>();
            var allSoils = UnityEngine.Object.FindObjectsByType<SownInStone.Agriculture.SoilCell>(UnityEngine.FindObjectsSortMode.None);
            foreach (var sc in allSoils)
            {
                if (sc != null && sc.gameObject.activeInHierarchy && sc.enabled && !sc.IsParentField)
                {
                    if (sc.RockDensity <= 0f && sc.plantedCrop == null)
                    {
                        list.Add(sc);
                    }
                }
            }

            // Sắp xếp ô mục tiêu hiện tại lên đầu tiên, các ô còn lại theo khoảng cách tăng dần
            list.Sort((a, b) => {
                if (a == targetCell) return -1;
                if (b == targetCell) return 1;
                float distA = UnityEngine.Vector3.Distance(a.transform.position, targetCell.transform.position);
                float distB = UnityEngine.Vector3.Distance(b.transform.position, targetCell.transform.position);
                return distA.CompareTo(distB);
            });

            return list;
        }

        private void PerformSinglePlant(SownInStone.Agriculture.SoilCell cell)
        {
            if (StorageManager.Instance != null && seedItem != null && testSeedData != null)
            {
                if (StorageManager.Instance.RemoveItem(seedItem, 1))
                {
                    cell.ActionPlantCrop(testSeedData);
                    SetAnimTrigger("Plant");
                    StartCoroutine(LockMovementForSeconds(plantActionDuration));
                    PlayerStats.Instance.TriggerAlert("Đã gieo 1 hạt giống!");
                    SownInStone.UI.SurvivalUIManager.Instance?.CloseDialogue();
                }
            }
        }

        private void PerformBulkPlant(SownInStone.Agriculture.SoilCell targetCell, int count)
        {
            if (StorageManager.Instance != null && seedItem != null && testSeedData != null)
            {
                var plantable = GetPlantableCellsNear(targetCell);
                int actualCount = Mathf.Min(count, plantable.Count);
                if (actualCount > 0)
                {
                    if (StorageManager.Instance.RemoveItem(seedItem, actualCount))
                    {
                        int planted = 0;
                        for (int i = 0; i < actualCount; i++)
                        {
                            if (plantable[i].ActionPlantCrop(testSeedData))
                            {
                                planted++;
                            }
                        }
                        SetAnimTrigger("Plant");
                        StartCoroutine(LockMovementForSeconds(plantActionDuration));
                        PlayerStats.Instance.TriggerAlert($"Đã gieo {planted} hạt giống!");
                        SownInStone.UI.SurvivalUIManager.Instance?.CloseDialogue();
                    }
                }
            }
        }

        private Collider GetClosestInteractable(Collider[] colliders)
        {
            Collider closest = null;
            float minDistance = float.MaxValue;

            // First pass: try to find actual interactable objects (chest, altar, soil, boat, mud puddle)
            foreach (var col in colliders)
            {
                if (col.gameObject == gameObject) continue;

                bool isRealInteractable = col.GetComponent<IInteractable>() != null ||
                                          col.GetComponent<AncestralAltar>() != null ||
                                          col.GetComponent<Coracle>() != null ||
                                          col.GetComponent<SoilCell>() != null ||
                                          col.GetComponent<MudPuddle>() != null;

                if (isRealInteractable)
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closest = col;
                    }
                }
            }

            // Second pass: if no real interactable object is found, check for NPCs
            if (closest == null)
            {
                minDistance = float.MaxValue;
                foreach (var col in colliders)
                {
                    if (col.gameObject == gameObject) continue;

                    if (col.GetComponent<NPCCharacter>() != null)
                    {
                        float dist = Vector3.Distance(transform.position, col.transform.position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            closest = col;
                        }
                    }
                }
            }

            return closest;
        }

        private void TryDirectPlaceItem(string itemId)
        {
            var inv = StorageManager.Instance.GetStorageSlots();
            if (inv == null) return;

            ItemData targetItem = null;
            foreach (var slot in inv)
            {
                if (slot != null && slot.item != null && slot.item.ItemID == itemId)
                {
                    targetItem = slot.item;
                    break;
                }
            }

            if (targetItem == null)
            {
                string name = itemId == "item_sandbag" ? "Bao cát" : "Tấm chắn lũ";
                PlayerStats.Instance?.TriggerAlert($"Bạn không có {name} trong túi đồ!");
                return;
            }

            if (StorageManager.Instance.RemoveItem(targetItem, 1))
            {
                string prefabPath = itemId == "item_sandbag" ? "Prefabs/Sandbag" : "Prefabs/FloodBoard";
                GameObject prefab = Resources.Load<GameObject>(prefabPath);
                if (prefab != null)
                {
                    Vector3 spawnPos = transform.position;
                    spawnPos.y = 0f;
                    Vector3 forwardOffset = transform.forward * 1.0f;
                    forwardOffset.y = 0f;
                    Vector3 finalPos = spawnPos + forwardOffset;
                    bool didMakePrePlacedSolid = false;

                    if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                    {
                        var stage = TutorialManager.Instance.currentStage;
                        if (stage == TutorialManager.TutorialStage.PrepareForStorm)
                        {
                            if (itemId == "item_sandbag")
                            {
                                if (TutorialManager.Instance.ghostSandbags.Count > 0)
                                {
                                    int bestIndex = -1;
                                    float minDistance = float.MaxValue;
                                    for (int i = 0; i < TutorialManager.Instance.ghostSandbags.Count; i++)
                                    {
                                        if (i >= 4) break;
                                        if (TutorialManager.Instance.bacNamTargetsPlaced[i]) continue;
                                        Vector3 playerPos = transform.position;
                                        Vector3 targetPos = TutorialManager.Instance.ghostSandbags[i].transform.position;
                                        playerPos.y = 0f;
                                        targetPos.y = 0f;
                                        float dist = Vector3.Distance(playerPos, targetPos);
                                        if (dist < minDistance)
                                        {
                                            minDistance = dist;
                                            bestIndex = i;
                                        }
                                    }
                                    if (bestIndex != -1 && minDistance < 3.5f)
                                    {
                                        TutorialManager.Instance.MakeSolidModel(TutorialManager.Instance.ghostSandbags[bestIndex]);
                                        TutorialManager.Instance.bacNamTargetsPlaced[bestIndex] = true;
                                        TutorialManager.Instance.OnBacNamSandbagPlaced();
                                        didMakePrePlacedSolid = true;
                                    }
                                }
                                else
                                {
                                    GameObject bacNamHouse = GameObject.Find("BacNam_House");
                                    if (bacNamHouse != null)
                                    {
                                        Vector3 roofCenter = bacNamHouse.transform.position + Vector3.up * 3.8f;
                                        Vector3[] roofOffsets = new Vector3[]
                                        {
                                            new Vector3(-1.2f, 0f, -0.8f),
                                            new Vector3(1.2f, 0.1f, -0.8f),
                                            new Vector3(-1.2f, 0.1f, 0.8f),
                                            new Vector3(1.2f, 0f, 0.8f)
                                        };
                                        int bestIndex = -1;
                                        float minDistance = float.MaxValue;
                                        for (int i = 0; i < 4; i++)
                                        {
                                            if (TutorialManager.Instance.bacNamTargetsPlaced[i]) continue;
                                            Vector3 playerPos = transform.position;
                                            Vector3 targetPos = roofCenter + roofOffsets[i];
                                            playerPos.y = 0f;
                                            targetPos.y = 0f;
                                            float dist = Vector3.Distance(playerPos, targetPos);
                                            if (dist < minDistance)
                                            {
                                                minDistance = dist;
                                                bestIndex = i;
                                            }
                                        }
                                        if (bestIndex != -1 && minDistance < 3.5f)
                                        {
                                            finalPos = roofCenter + roofOffsets[bestIndex];
                                            TutorialManager.Instance.bacNamTargetsPlaced[bestIndex] = true;
                                            TutorialManager.Instance.OnBacNamSandbagPlaced();
                                        }
                                    }
                                }
                            }
                            else // Floodboard
                            {
                                if (TutorialManager.Instance.ghostFloodboards.Count > 0)
                                {
                                    int bestIndex = -1;
                                    float minDistance = float.MaxValue;
                                    for (int i = 0; i < TutorialManager.Instance.ghostFloodboards.Count; i++)
                                    {
                                        if (i >= 2) break; // Bac Nam task requires 2 floodboards
                                        if (TutorialManager.Instance.oThamTargetsPlaced[i]) continue;
                                        Vector3 playerPos = transform.position;
                                        Vector3 targetPos = TutorialManager.Instance.ghostFloodboards[i].transform.position;
                                        playerPos.y = 0f;
                                        targetPos.y = 0f;
                                        float dist = Vector3.Distance(playerPos, targetPos);
                                        if (dist < minDistance)
                                        {
                                            minDistance = dist;
                                            bestIndex = i;
                                        }
                                    }
                                    if (bestIndex != -1 && minDistance < 3.5f)
                                    {
                                        TutorialManager.Instance.MakeSolidModel(TutorialManager.Instance.ghostFloodboards[bestIndex]);
                                        TutorialManager.Instance.oThamTargetsPlaced[bestIndex] = true;
                                        TutorialManager.Instance.OnBacNamFloodBoardPlaced();
                                        didMakePrePlacedSolid = true;
                                    }
                                }
                                else
                                {
                                    GameObject bacNamHouse = GameObject.Find("BacNam_House");
                                    if (bacNamHouse != null)
                                    {
                                        Vector3 housePos = bacNamHouse.transform.position;
                                        Vector3 forward = bacNamHouse.transform.forward;
                                        Vector3 right = bacNamHouse.transform.right;
                                        Vector3[] targets = new Vector3[]
                                        {
                                            housePos + forward * 4.2f + right * -1.5f,
                                            housePos + forward * 4.2f + right * 1.5f
                                        };
                                        int bestIndex = -1;
                                        float minDistance = float.MaxValue;
                                        for (int i = 0; i < 2; i++)
                                        {
                                            if (TutorialManager.Instance.oThamTargetsPlaced[i]) continue;
                                            Vector3 playerPos = transform.position;
                                            Vector3 targetPos = targets[i];
                                            playerPos.y = 0f;
                                            targetPos.y = 0f;
                                            float dist = Vector3.Distance(playerPos, targetPos);
                                            if (dist < minDistance)
                                            {
                                                minDistance = dist;
                                                bestIndex = i;
                                            }
                                        }
                                        if (bestIndex != -1 && minDistance < 3.5f)
                                        {
                                            finalPos = targets[bestIndex];
                                            TutorialManager.Instance.oThamTargetsPlaced[bestIndex] = true;
                                            TutorialManager.Instance.OnBacNamFloodBoardPlaced();
                                        }
                                    }
                                }
                            }
                        }
                        else if (stage == TutorialManager.TutorialStage.PrepareOwnHouse)
                        {
                            GameObject ownHouse = GameObject.Find("Thanh_House");
                            if (ownHouse != null)
                            {
                                Vector3 houseCenter = ownHouse.transform.position;
                                Vector3[] houseOffsets = new Vector3[]
                                {
                                    houseCenter + new Vector3(-1.5f, 0.1f, -1.0f),
                                    houseCenter + new Vector3(-0.5f, 0.1f, -1.0f),
                                    houseCenter + new Vector3(0.5f, 0.1f, -1.0f),
                                    houseCenter + new Vector3(1.5f, 0.1f, -1.0f)
                                };
                                int bestIndex = -1;
                                float minDistance = float.MaxValue;
                                for (int i = 0; i < 4; i++)
                                {
                                    if (TutorialManager.Instance.ownHouseSandbagsPlaced > i) continue;
                                    Vector3 playerPos = transform.position;
                                    Vector3 targetPos = houseOffsets[i];
                                    playerPos.y = 0f;
                                    targetPos.y = 0f;
                                    float dist = Vector3.Distance(playerPos, targetPos);
                                    if (dist < minDistance)
                                    {
                                        minDistance = dist;
                                        bestIndex = i;
                                    }
                                }
                                if (bestIndex != -1 && minDistance < 3.5f)
                                {
                                    finalPos = houseOffsets[bestIndex];
                                    TutorialManager.Instance.OnOwnHouseSandbagPlaced();
                                }
                            }
                        }
                    }

                    if (!didMakePrePlacedSolid)
                    {
                        GameObject deployed = Instantiate(prefab, finalPos, Quaternion.identity);
                        deployed.name = itemId == "item_sandbag" ? "Deployed_Sandbag" : "Deployed_FloodBoard";
                    }

                    PlayerStats.Instance?.TriggerAlert($"Đã đặt {targetItem.ItemName}!");
                }
            }
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.RefreshInventoryUI();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Vẽ vòng tròn kiểm tra phạm vi tương tác trong Scene View của Unity Editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }

        /// <summary>
        /// Tự động tải tham chiếu Task 2 nếu trong Inspector chưa kéo thả.
        /// </summary>
        private void AutoLoadTask2References()
        {
#if UNITY_EDITOR
            if (floodBoardItem == null)
                floodBoardItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_flood_board.asset");
            if (sandbagItem == null)
                sandbagItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_sandbag.asset");
            if (floodBoardPrefab == null)
                floodBoardPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/FloodBoard.prefab");
            if (sandbagPrefab == null)
                sandbagPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Sandbag.prefab");
#endif
        }

        private void OnValidate()
        {
            AutoLoadTask2References();
        }

        /// <summary>
        /// Thử mở chế độ xem trước vị trí đặt vật thể (Ghost Preview).
        /// </summary>
        private void TryPlaceBarrier(ItemData itemData, GameObject barrierPrefab, string itemName)
        {
            if (itemData == null || barrierPrefab == null)
            {
                AutoLoadTask2References();
            }

            if (itemData == null || barrierPrefab == null)
            {
                Debug.LogWarning($"[CHẮN LŨ] Chưa gán ItemData hoặc Prefab cho {itemName} trên PlayerController!");
                return;
            }

            if (StorageManager.Instance != null && !StorageManager.Instance.HasItem(itemData, 1))
            {
                // Tự động cấp 5 vật phẩm vào kho đồ nếu chưa có để kiểm thử thuận tiện
                StorageManager.Instance.AddItem(itemData, 5);
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast($"Đã tự động cấp 5x {itemName} vào kho để test!");
                }
            }

            StartPlacementPreview(itemData, barrierPrefab, itemName);
        }

        private void StartPlacementPreview(ItemData itemData, GameObject barrierPrefab, string itemName)
        {
            CancelPlacementPreview();

            isPlacingPreview = true;
            previewItemData = itemData;
            previewPrefab = barrierPrefab;
            previewItemName = itemName;
            previewRotationY = 0f;

            activeGhostObject = Instantiate(barrierPrefab);
            
            // Phóng to kích thước bao cát và chỉnh tư thế nằm ngang chắc chắn
            if (itemName == "Bao cát" || (sandbagItem != null && itemData.ItemID == sandbagItem.ItemID))
            {
                activeGhostObject.transform.localScale = new Vector3(2.2f, 1.8f, 2.2f);
            }

            // Tắt toàn bộ Collider và script logic để ghost object không gây xung đột vật lý
            foreach (var col in activeGhostObject.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }
            foreach (var barrier in activeGhostObject.GetComponentsInChildren<Interactions.FloodBarrier>())
            {
                barrier.enabled = false;
            }
            foreach (var mono in activeGhostObject.GetComponentsInChildren<MonoBehaviour>())
            {
                if (mono != null) mono.enabled = false;
            }

            // Tạo vật liệu Hologram trong suốt màu xanh bảo vệ (Ghost Effect)
            Shader ghostShader = Shader.Find("Sprites/Default");
            if (ghostShader == null) ghostShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            if (ghostShader == null) ghostShader = Shader.Find("Unlit/Transparent");

            Material ghostMat = new Material(ghostShader);
            ghostMat.color = new Color(0.2f, 0.85f, 1f, 0.5f); // Màu xanh lam trong suốt dạng Hologram

            foreach (var renderer in activeGhostObject.GetComponentsInChildren<Renderer>())
            {
                Material[] ghostMats = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < ghostMats.Length; i++)
                {
                    ghostMats[i] = ghostMat;
                }
                renderer.materials = ghostMats;
            }

            Debug.Log($"[CHẮN LŨ] Đang ở chế độ xem trước vị trí đặt {itemName}");
        }

        private void UpdatePlacementPreview()
        {
            if (activeGhostObject == null)
            {
                CancelPlacementPreview();
                return;
            }

            // Cập nhật vị trí mô hình xem trước theo con trỏ chuột và hỗ trợ xếp chồng (Stacking)
            Vector3 targetPos = transform.position + transform.forward * 1.5f;
            Camera mainCam = Camera.main;
            bool isHittingBarrier = false;
            if (mainCam != null)
            {
                Vector3 mouseScreenPos = Input.mousePosition;
#if ENABLE_INPUT_SYSTEM
                if (Mouse.current != null)
                {
                    Vector2 mPos = Mouse.current.position.ReadValue();
                    mouseScreenPos = new Vector3(mPos.x, mPos.y, 0f);
                }
#endif
                Ray ray = mainCam.ScreenPointToRay(mouseScreenPos);
                if (Physics.Raycast(ray, out RaycastHit mouseHit, 100f))
                {
                    targetPos = mouseHit.point;
                    // Hỗ trợ xếp chồng bao cát/vật chắn lên nhau khi chỉ chuột vào vật thể cũ
                    if (mouseHit.collider != null && mouseHit.collider.GetComponentInParent<Interactions.FloodBarrier>() != null)
                    {
                        targetPos.y = mouseHit.collider.bounds.max.y;
                        isHittingBarrier = true;
                    }
                }
                else if (Physics.Raycast(targetPos + Vector3.up * 3f, Vector3.down, out RaycastHit hit, 10f))
                {
                    targetPos.y = hit.point.y;
                }
            }

            // [FIX] Nếu đứng dưới mặt đất và không xếp chồng, ép độ cao preview luôn nằm trên nền đất (Terrain/Ground) để không bị cản bởi nhà/tường
            if (transform.position.y < 2.0f && !isHittingBarrier)
            {
                float groundY = transform.position.y;
                if (UnityEngine.Terrain.activeTerrain != null)
                {
                    groundY = UnityEngine.Terrain.activeTerrain.SampleHeight(targetPos) + UnityEngine.Terrain.activeTerrain.transform.position.y;
                }
                else
                {
                    RaycastHit[] hits = Physics.RaycastAll(new Vector3(targetPos.x, targetPos.y + 10f, targetPos.z), Vector3.down, 25f);
                    float bestY = transform.position.y;
                    float minHitDist = float.MaxValue;
                    foreach (var h in hits)
                    {
                        if (h.collider != null && !h.collider.isTrigger)
                        {
                            string cName = h.collider.name.ToLower();
                            if (cName.Contains("player") || cName.Contains("npc") || cName.Contains("preview") || cName.Contains("ghost") || cName.Contains("house") || cName.Contains("door") || cName.Contains("wall") || cName.Contains("roof"))
                                continue;

                            if (h.distance < minHitDist)
                            {
                                minHitDist = h.distance;
                                bestY = h.point.y;
                            }
                        }
                    }
                    groundY = bestY;
                }
                targetPos.y = groundY;
            }

            // --- Snap vào ghost hướng dẫn gần nhất (nếu đang trong tutorial) ---
            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                GameObject closestGhost = null;
                float closestDist = 2.5f; // bán kính snap
                var stage = TutorialManager.Instance.currentStage;

                if (stage == TutorialManager.TutorialStage.PrepareForStorm && previewItemData != null)
                {
                    if (previewItemData.ItemID == "item_sandbag")
                    {
                        for (int gi = 0; gi < Mathf.Min(TutorialManager.Instance.ghostSandbags.Count, 4); gi++)
                        {
                            if (TutorialManager.Instance.bacNamTargetsPlaced != null && gi < TutorialManager.Instance.bacNamTargetsPlaced.Length && TutorialManager.Instance.bacNamTargetsPlaced[gi]) continue;
                            var g = TutorialManager.Instance.ghostSandbags[gi];
                            if (g == null) continue;
                            float d = Vector3.Distance(targetPos, g.transform.position);
                            if (d < closestDist) { closestDist = d; closestGhost = g; }
                        }
                    }
                    else if (previewItemData.ItemID == "item_flood_board")
                    {
                        for (int gi = 0; gi < Mathf.Min(TutorialManager.Instance.ghostFloodboards.Count, 2); gi++)
                        {
                            if (TutorialManager.Instance.oThamTargetsPlaced != null && gi < TutorialManager.Instance.oThamTargetsPlaced.Length && TutorialManager.Instance.oThamTargetsPlaced[gi]) continue;
                            var g = TutorialManager.Instance.ghostFloodboards[gi];
                            if (g == null) continue;
                            float d = Vector3.Distance(targetPos, g.transform.position);
                            if (d < closestDist) { closestDist = d; closestGhost = g; }
                        }
                    }
                }
                else if (stage == TutorialManager.TutorialStage.PrepareOwnHouse && previewItemData != null && previewItemData.ItemID == "item_flood_board")
                {
                    for (int gi = 0; gi < Mathf.Min(TutorialManager.Instance.ownHouseGhostFloodboards.Count, 4); gi++)
                    {
                        if (TutorialManager.Instance.ownHouseFloodboardsPlaced != null && gi < TutorialManager.Instance.ownHouseFloodboardsPlaced.Length && TutorialManager.Instance.ownHouseFloodboardsPlaced[gi]) continue;
                        var g = TutorialManager.Instance.ownHouseGhostFloodboards[gi];
                        if (g == null) continue;
                        float d = Vector3.Distance(targetPos, g.transform.position);
                        if (d < closestDist) { closestDist = d; closestGhost = g; }
                    }
                }

                if (closestGhost != null)
                {
                    targetPos = closestGhost.transform.position;
                }
            }

            activeGhostObject.transform.position = targetPos;

            // Xoay mô hình xem trước đồng bộ theo hướng đứng thực tế của nhân vật (characterVisual)
            Quaternion charRot = transform.rotation;
            if (characterVisual != null)
            {
                charRot = characterVisual.rotation;
            }
            activeGhostObject.transform.rotation = charRot * Quaternion.Euler(0, previewRotationY, 0);

            // Cập nhật hướng dẫn UI dạng hàng dọc
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt(
                    $"• <b>[Chuột Trái / Enter]</b>: Đặt {previewItemName}\n" +
                    $"• <b>[R]</b>: Xoay góc vật thể\n" +
                    $"• <b>[Chuột Phải / Esc]</b>: Hủy bỏ"
                );
            }

            // Bấm [R] để xoay góc 45 độ
#if ENABLE_INPUT_SYSTEM
            bool rotateKey = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            bool confirmKey = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) || 
                              (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame));
            bool cancelKey = (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame) || 
                             (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame);
#else
            bool rotateKey = Input.GetKeyDown(KeyCode.R);
            bool confirmKey = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E);
            bool cancelKey = Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape);
#endif

            if (rotateKey)
            {
                previewRotationY += 45f;
                if (previewRotationY >= 360f) previewRotationY -= 360f;
            }

            if (confirmKey)
            {
                ConfirmPlacement();
            }
            else if (cancelKey)
            {
                CancelPlacementPreview();
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("Đã hủy đặt vật phẩm.");
                }
            }
        }

        private void ConfirmPlacement()
        {
            if (activeGhostObject == null || previewItemData == null || previewPrefab == null)
            {
                CancelPlacementPreview();
                return;
            }

            if (StorageManager.Instance != null && StorageManager.Instance.RemoveItem(previewItemData, 1))
            {
                Vector3 finalPos = activeGhostObject.transform.position;
                Quaternion finalRot = activeGhostObject.transform.rotation;
                Vector3 finalScale = activeGhostObject.transform.localScale;
                string placedName = previewItemName;
                string itemId = previewItemData.ItemID;

                CancelPlacementPreview();

                bool didSnap = false;

                if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                {
                    var stage = TutorialManager.Instance.currentStage;
                    if (stage == TutorialManager.TutorialStage.PrepareForStorm)
                    {
                        if (itemId == "item_sandbag")
                        {
                            if (TutorialManager.Instance.ghostSandbags.Count > 0)
                            {
                                int bestIndex = -1;
                                float minDistance = float.MaxValue;
                                for (int i = 0; i < TutorialManager.Instance.ghostSandbags.Count; i++)
                                {
                                    if (i >= 4) break;
                                    if (TutorialManager.Instance.bacNamTargetsPlaced[i]) continue;
                                    
                                    float dist = Vector3.Distance(finalPos, TutorialManager.Instance.ghostSandbags[i].transform.position);
                                    if (dist < minDistance)
                                    {
                                        minDistance = dist;
                                        bestIndex = i;
                                    }
                                }
                                if (bestIndex != -1 && minDistance < 3.5f)
                                {
                                    TutorialManager.Instance.MakeSolidModel(TutorialManager.Instance.ghostSandbags[bestIndex]);
                                    TutorialManager.Instance.bacNamTargetsPlaced[bestIndex] = true;
                                    TutorialManager.Instance.OnBacNamSandbagPlaced();
                                    didSnap = true;
                                }
                            }
                        }
                        else if (itemId == "item_flood_board")
                        {
                            if (TutorialManager.Instance.ghostFloodboards.Count > 0)
                            {
                                int bestIndex = -1;
                                float minDistance = float.MaxValue;
                                for (int i = 0; i < TutorialManager.Instance.ghostFloodboards.Count; i++)
                                {
                                    if (i >= 2) break; // Bac Nam task requires 2 floodboards
                                    if (TutorialManager.Instance.oThamTargetsPlaced[i]) continue;
                                    
                                    float dist = Vector3.Distance(finalPos, TutorialManager.Instance.ghostFloodboards[i].transform.position);
                                    if (dist < minDistance)
                                    {
                                        minDistance = dist;
                                        bestIndex = i;
                                    }
                                }
                                if (bestIndex != -1 && minDistance < 3.5f)
                                {
                                    TutorialManager.Instance.MakeSolidModel(TutorialManager.Instance.ghostFloodboards[bestIndex]);
                                    TutorialManager.Instance.oThamTargetsPlaced[bestIndex] = true;
                                    TutorialManager.Instance.OnBacNamFloodBoardPlaced();
                                    didSnap = true;
                                }
                            }
                        }
                    }
                    else if (stage == TutorialManager.TutorialStage.PrepareOwnHouse)
                    {
                        if (itemId == "item_flood_board")
                        {
                            if (TutorialManager.Instance.ownHouseGhostFloodboards.Count > 0)
                            {
                                int bestIndex = -1;
                                float minDistance = float.MaxValue;
                                for (int i = 0; i < TutorialManager.Instance.ownHouseGhostFloodboards.Count; i++)
                                {
                                    if (i >= 4) break;
                                    if (TutorialManager.Instance.ownHouseFloodboardsPlaced[i]) continue;
                                    
                                    float dist = Vector3.Distance(finalPos, TutorialManager.Instance.ownHouseGhostFloodboards[i].transform.position);
                                    if (dist < minDistance)
                                    {
                                        minDistance = dist;
                                        bestIndex = i;
                                    }
                                }
                                if (bestIndex != -1 && minDistance < 3.5f)
                                {
                                    TutorialManager.Instance.MakeSolidModel(TutorialManager.Instance.ownHouseGhostFloodboards[bestIndex]);
                                    TutorialManager.Instance.ownHouseFloodboardsPlaced[bestIndex] = true;
                                    TutorialManager.Instance.OnOwnHouseSandbagPlaced();
                                    didSnap = true;
                                }
                            }
                        }
                    }
                }

                if (!didSnap)
                {
                    GameObject placedGo = Instantiate(previewPrefab, finalPos, finalRot);
                    placedGo.transform.localScale = finalScale;
                }

                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast($"Đã đặt 1x {placedName} gia cố!");
                }

                Debug.Log($"[CHẮN LŨ] XÁC NHẬN ĐẶT thành công {placedName} tại {finalPos}");
            }
            else
            {
                CancelPlacementPreview();
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast($"Trong kho không đủ {previewItemName} để đặt!");
                }
            }
        }

        private void CancelPlacementPreview()
        {
            isPlacingPreview = false;
            if (activeGhostObject != null)
            {
                Destroy(activeGhostObject);
                activeGhostObject = null;
            }
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt("");
            }
        }

        /// <summary>
        /// Xử lý khi sử dụng vật phẩm từ thanh phím tắt Hotbar 1..5.
        /// </summary>
        public void UseItemFromHotbar(ItemData item)
        {
            if (item == null) return;

            if (item.ItemID == "item_sandbag" || (sandbagItem != null && item.ItemID == sandbagItem.ItemID))
            {
                TryPlaceBarrier(sandbagItem, sandbagPrefab, "Bao cát");
            }
            else if (item.ItemID == "item_flood_board" || (floodBoardItem != null && item.ItemID == floodBoardItem.ItemID))
            {
                TryPlaceBarrier(floodBoardItem, floodBoardPrefab, "Vách chắn nước");
            }
            else if (item.ItemID == "item_plastic_mulch")
            {
                ApplyPlasticMulchToAllFields(item);
            }
            else if (item.StaminaRestoreValue > 0f || item.MoraleRestoreValue > 0f)
            {
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.UseItem(item);
                }
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.RefreshInventoryUI();
                }
            }
            else
            {
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast($"Sử dụng {item.ItemName}");
                }
            }
        }

        /// <summary>
        /// Phủ màng bọc nilon bảo vệ cho toàn bộ các ô ruộng trong làng trước bão lũ.
        /// </summary>
        public void ApplyPlasticMulchToAllFields(ItemData mulchItem)
        {
            if (StorageManager.Instance != null && !StorageManager.Instance.RemoveItem(mulchItem, 1))
            {
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("Trong kho không còn Màng Bọc Nilon!");
                }
                return;
            }

            GameObject mulchPrefab = Resources.Load<GameObject>("Prefabs/PlasticMulch");
            if (mulchPrefab == null)
            {
                #if UNITY_EDITOR
                mulchPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/PlasticMulch.prefab");
                if (mulchPrefab == null)
                {
                    mulchPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlasticMulch/MangNilon_Model.fbx");
                }
                #endif
            }
            var soils = FindObjectsByType<SoilCell>(FindObjectsInactive.Exclude);
            int count = 0;
            foreach (var soil in soils)
            {
                if (soil != null && !soil.IsParentField)
                {
                    soil.ApplyPlasticMulch(mulchPrefab);
                    count++;
                }
            }

            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast($"🌾 Đã phủ Màng Bọc Nilon bảo vệ toàn bộ {count} ô ruộng trước bão lũ!");
            }
            Debug.Log($"[PLASTIC MULCH] Đã phủ thành công Màng nilon bảo vệ {count} ô ruộng!");

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnPlasticMulchApplied();
            }
        }

        /// <summary>
        /// Chuyển đổi màn hình/vị trí người chơi vào bên trong KhungNha trú ẩn bão lũ.
        /// </summary>
        public void EnterHouse()
        {
            isInsideHouse = true;

            Vector3 indoorSpawnPoint = new Vector3(100f, 0f, 100f);

            if (houseInteriorInstance == null)
            {
                houseInteriorInstance = GameObject.Find("HouseInterior");
            }

            // Nếu nhà trong scene bị rỗng không có mô hình con, lập tức xóa bỏ để nạp lại
            if (houseInteriorInstance != null && houseInteriorInstance.transform.childCount == 0)
            {
                DestroyImmediate(houseInteriorInstance);
                houseInteriorInstance = null;
            }

            if (houseInteriorInstance == null)
            {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/HouseInterior");
#if UNITY_EDITOR
                if (prefab == null)
                {
                    prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/HouseInterior.prefab");
                }
#endif
                if (prefab != null)
                {
                    houseInteriorInstance = Instantiate(prefab, indoorSpawnPoint, Quaternion.identity);
                    houseInteriorInstance.name = "HouseInterior";
                }
            }

            Vector3 targetIndoorPos = indoorSpawnPoint + new Vector3(0f, 0.565f, 0f);
            if (houseInteriorInstance != null)
            {
                houseInteriorInstance.transform.position = indoorSpawnPoint;
                houseInteriorInstance.transform.localScale = Vector3.one;

                foreach (Transform child in houseInteriorInstance.transform)
                {
                    child.gameObject.SetActive(true);
                }

                // Giữ lại toàn bộ các Collider để cản di chuyển (không chạy xuyên tường, giường, bếp...)
                // và đảm bảo các vùng tương tác (Trigger/Collider của KitchenHearth, Altar) không bị xoá mất.
                /*
                foreach (var col in houseInteriorInstance.GetComponentsInChildren<Collider>())
                {
                    if (col.gameObject.transform.parent != null && col.gameObject.transform.parent.name == "PhysicalWalls")
                    {
                        continue; // Giữ lại 4 tường cản vật lý để nhân vật KHÔNG ĐI XUYÊN TƯỜNG được
                    }
                    Destroy(col);
                }
                */

                foreach (var rend in houseInteriorInstance.GetComponentsInChildren<Renderer>())
                {
                    if (rend != null)
                    {
                        rend.enabled = true; // Hiển thị 100% tất cả tường gỗ và nội thất kín bao trọn không nhìn xuyên
                    }
                }

                targetIndoorPos = houseInteriorInstance.transform.position + new Vector3(-1.0f, 0.565f, -2.0f);
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.position = targetIndoorPos;
            }
            transform.position = targetIndoorPos;

            // Góc nhìn hiện tại đã được giữ nguyên theo tùy chỉnh của người chơi, không ép về ThirdPerson nữa
            
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("🏠 Bạn đã vào bên trong nhà an toàn tránh bão lũ!");
            }
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.UpdateHUDPanel();
            }
        }

        public void ExitHouse()
        {
            isInsideHouse = false;
            GameObject houseObj = GameObject.Find("Thanh_House");
            Vector3 outdoorPos = houseObj != null ? houseObj.transform.TransformPoint(new Vector3(0f, 1.0f, 4.2f)) : new Vector3(10.66f, 1.0f, -5.8f);

            // Bắn raycast xuống dưới để dò bề mặt Ground/Terrain thực tế, tránh nhân vật bị chôn chân dưới đất
            RaycastHit hit;
            if (Physics.Raycast(outdoorPos + Vector3.up * 5f, Vector3.down, out hit, 15f, LayerMask.GetMask("Ground", "Default", "Terrain")))
            {
                outdoorPos.y = hit.point.y + 0.565f;
            }
            else
            {
                outdoorPos.y = 0.565f;
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.position = outdoorPos;
            }
            transform.position = outdoorPos;

            // Góc nhìn hiện tại đã được giữ nguyên theo tùy chỉnh của người chơi, không ép về ThirdPerson nữa
            
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("🚪 Bạn đã ra ngoài sân trước cửa nhà!");
            }
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.UpdateHUDPanel();
            }
        }

        private System.Collections.IEnumerator PerformSleepSequence()
        {
            isPerformingAction = true;

            // Nếu đang ở giai đoạn chờ ngủ trước bão → phát video ngay, không cần fade/skip thời gian
            if (TutorialManager.Instance != null &&
                TutorialManager.Instance.isTutorialActive &&
                TutorialManager.Instance.currentStage == TutorialManager.TutorialStage.WaitForSleep)
            {
                // Phát video ngulucbao.mp4 ngay lập tức (OnPlayerSlept xử lý video + chuyển Phase 3)
                isPerformingAction = false; // OnPlayerSlept sẽ set IsPerformingAction lại
                TutorialManager.Instance.OnPlayerSlept();
                yield break;
            }

            // --- Flow ngủ bình thường (các stage khác) ---
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.FadeToBlack(1.5f);
            }
            yield return new WaitForSeconds(1.5f);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SkipToMorningFiveAM();
            }

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.ModifyHealth(100f);
                PlayerStats.Instance.ModifyStamina(100f);
                PlayerStats.Instance.ModifyMorale(100f);
            }

            yield return new WaitForSeconds(0.5f);

            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.FadeFromBlack(1.5f);
                SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("🛏️ Bạn đã ngủ một giấc thật ngon! Trời đã sáng lúc 05:00 AM, thể lực và tinh thần phục hồi 100%.");
            }

            yield return new WaitForSeconds(1.0f);
            isPerformingAction = false;
        }

        [Header("--- LOGIC CÕNG NPC CỨU HỘ ---")]
        private NPCCharacter carriedNPC = null;
        public NPCCharacter CarriedNPC => carriedNPC;

        public void CarryNPCSurge(NPCCharacter npc)
        {
            if (carriedNPC != null)
            {
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("⚠️ Bạn đang cõng một người rồi! Hãy đưa họ về nhà Thành trước.");
                }
                return;
            }

            carriedNPC = npc;
            
            // Ẩn visual model của NPC trong scene để tạo cảm giác cõng đi
            var visual = npc.transform.Find("Visual");
            if (visual != null)
            {
                visual.gameObject.SetActive(false);
            }

            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast($"🏃‍♂️ CÕNG CỨU HỘ: Đang cõng {npc.NPCName}! Hãy chạy nhanh về nhà Thành.");
            }
        }

        public void DropCarriedNPCForced()
        {
            if (carriedNPC != null)
            {
                var visual = carriedNPC.transform.Find("Visual");
                if (visual != null)
                {
                    visual.gameObject.SetActive(true);
                }
                carriedNPC = null;
            }
        }

        private void DropCarriedNPCAtThanhHouse()
        {
            if (carriedNPC == null) return;

            GameObject houseObj = GameObject.Find("Thanh_House");
            Vector3 center = houseObj != null ? houseObj.transform.position : transform.position;
            
            // Đặt NPC lên nóc nhà tại một góc ngẫu nhiên
            Vector3 roofSpot = center + new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), 5.2f, UnityEngine.Random.Range(-1.5f, 1.5f));
            
            carriedNPC.transform.position = roofSpot;
            var rbNPC = carriedNPC.GetComponent<Rigidbody>();
            if (rbNPC != null)
            {
                rbNPC.linearVelocity = Vector3.zero;
                rbNPC.position = roofSpot;
            }

            var visual = carriedNPC.transform.Find("Visual");
            if (visual != null)
            {
                visual.gameObject.SetActive(true);
            }

            // Gọi hàm trong TutorialManager để ghi nhận cứu hộ
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnNPCRescued(carriedNPC.characterType);
            }

            carriedNPC = null;
            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_place_object");
        }
    }
}
