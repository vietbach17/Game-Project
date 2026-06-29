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

        public bool IsOnRoof => isOnRoof;

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
            if (seedItem == null)
            {
                seedItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Seed.asset");
            }
            if (noodlesItem == null)
            {
                noodlesItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Noodles.asset");
            }
            if (testSeedData == null)
            {
                testSeedData = UnityEditor.AssetDatabase.LoadAssetAtPath<CropData>("Assets/Data/Crop_KhoaiLang.asset");
            }
#endif
        }

        private void HandleFloodRoofSurvival()
        {
            if (WeatherManager.Instance == null || GameManager.Instance == null) return;

            // 1. Tự động di tản lên nóc nhà khi nước ngập cao trong mùa bão lũ
            if (GameManager.Instance.CurrentPhase == GamePhase.MuaBao && WeatherManager.Instance.FloodLevel > 1.5f && !isOnRoof)
            {
                isOnRoof = true;
                
                // Teleport lên nóc nhà
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                }
                Vector3 roofPos = new Vector3(0f, 3.5f, -10f);
                transform.position = roofPos;
                if (rb != null)
                {
                    rb.position = roofPos;
                }
                
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
            // 2. Trở lại đất liền khi nước rút hoặc chuyển phase mới
            else if (isOnRoof && (GameManager.Instance.CurrentPhase != GamePhase.MuaBao || WeatherManager.Instance.FloodLevel < 0.5f))
            {
                isOnRoof = false;
                
                // Teleport về mặt đất
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                }
                Vector3 groundPos = new Vector3(0f, 0.5f, -6f);
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
        }

        private void Update()
        {
            HandleFloodRoofSurvival();

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

            // 3. Tiêu hao thể lực thụ động khi di chuyển dưới nắng hè Gió Lào gay gắt
            HandleStaminaDrainOnMove();

            // 4. Lắng nghe phím [E] hoặc [Space] để tương tác vật lý trong game
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    TryPerformInteraction();
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                TryPerformInteraction();
            }
#endif
            // Cập nhật gợi ý tương tác lên UI
            UpdateInteractionPrompt();
        }

        private void FixedUpdate()
        {
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
                Vector3 constrainedPos = rb.position;
                constrainedPos.x = Mathf.Clamp(constrainedPos.x, -2.2f, 2.2f);
                constrainedPos.z = Mathf.Clamp(constrainedPos.z, -12.0f, -8.0f);
                constrainedPos.y = 3.5f;

                rb.position = constrainedPos;
                transform.position = constrainedPos;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
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
                // Hồi phục thể lực khi đứng yên tại chỗ (+5 thể lực/giây)
                PlayerStats.Instance.ModifyStamina(5f * Time.deltaTime);
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

            // Kiểm tra xem người chơi có đang đứng gần vị trí chỉ dẫn đặt ván/bao cát bão không
            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                var stage = TutorialManager.Instance.currentStage;
                if (stage == TutorialManager.TutorialStage.PrepareForStorm)
                {
                    if (TutorialManager.Instance.activeStormJob == TutorialManager.ActiveStormJob.OThamFloodboards)
                    {
                        if (TutorialManager.Instance.ghostFloodboards.Count > 0)
                        {
                            for (int i = 0; i < TutorialManager.Instance.ghostFloodboards.Count; i++)
                            {
                                if (i >= 4) break;
                                if (!TutorialManager.Instance.oThamTargetsPlaced[i])
                                {
                                    Vector3 playerPos = transform.position;
                                    Vector3 targetPos = TutorialManager.Instance.ghostFloodboards[i].transform.position;
                                    playerPos.y = 0f;
                                    targetPos.y = 0f;
                                    if (Vector3.Distance(playerPos, targetPos) < 3.5f)
                                    {
                                        TryDirectPlaceItem("item_flood_board");
                                        return;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var oTham = System.Array.Find(FindObjectsByType<NPCCharacter>(FindObjectsInactive.Exclude), n => n.characterType == NPCCharacter.StoryCharacterType.OTham);
                            if (oTham != null)
                            {
                                Vector3 oThamPos = oTham.transform.position;
                                Vector3 forward = oTham.transform.forward;
                                Vector3 right = oTham.transform.right;
                                Vector3[] targets = new Vector3[]
                                {
                                    oThamPos + forward * 2.5f + right * -1.8f,
                                    oThamPos + forward * 2.5f + right * -0.6f,
                                    oThamPos + forward * 2.5f + right * 0.6f,
                                    oThamPos + forward * 2.5f + right * 1.8f
                                };

                                for (int i = 0; i < 4; i++)
                                {
                                    if (!TutorialManager.Instance.oThamTargetsPlaced[i])
                                    {
                                        Vector3 playerPos = transform.position;
                                        Vector3 targetPos = targets[i];
                                        playerPos.y = 0f;
                                        targetPos.y = 0f;
                                        if (Vector3.Distance(playerPos, targetPos) < 3.5f)
                                        {
                                            TryDirectPlaceItem("item_flood_board");
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (TutorialManager.Instance.activeStormJob == TutorialManager.ActiveStormJob.BacNamSandbags)
                    {
                        if (TutorialManager.Instance.ghostSandbags.Count > 0)
                        {
                            for (int i = 0; i < TutorialManager.Instance.ghostSandbags.Count; i++)
                            {
                                if (i >= 4) break;
                                if (!TutorialManager.Instance.bacNamTargetsPlaced[i])
                                {
                                    Vector3 playerPos = transform.position;
                                    Vector3 targetPos = TutorialManager.Instance.ghostSandbags[i].transform.position;
                                    playerPos.y = 0f;
                                    targetPos.y = 0f;
                                    if (Vector3.Distance(playerPos, targetPos) < 3.5f)
                                    {
                                        TryDirectPlaceItem("item_sandbag");
                                        return;
                                    }
                                }
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

                                for (int i = 0; i < 4; i++)
                                {
                                    if (!TutorialManager.Instance.bacNamTargetsPlaced[i])
                                    {
                                        Vector3 playerPos = transform.position;
                                        Vector3 targetPos = roofCenter + roofOffsets[i];
                                        playerPos.y = 0f;
                                        targetPos.y = 0f;
                                        if (Vector3.Distance(playerPos, targetPos) < 3.5f)
                                        {
                                            TryDirectPlaceItem("item_sandbag");
                                            return;
                                        }
                                    }
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
                        Vector3[] targets = new Vector3[]
                        {
                            houseCenter + new Vector3(-1.5f, 0.1f, -1.0f),
                            houseCenter + new Vector3(-0.5f, 0.1f, -1.0f),
                            houseCenter + new Vector3(0.5f, 0.1f, -1.0f),
                            houseCenter + new Vector3(1.5f, 0.1f, -1.0f)
                        };

                        for (int i = 0; i < 4; i++)
                        {
                            if (TutorialManager.Instance.ownHouseSandbagsPlaced <= i)
                            {
                                Vector3 playerPos = transform.position;
                                Vector3 targetPos = targets[i];
                                playerPos.y = 0f;
                                targetPos.y = 0f;
                                if (Vector3.Distance(playerPos, targetPos) < 3.5f)
                                {
                                    TryDirectPlaceItem("item_sandbag");
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            Debug.Log("[PLAYER] Bấm phím tương tác [E]!");

            // 0. Nếu đang ở trên nóc nhà và không có đối thoại nào đang mở
            if (isOnRoof)
            {
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    if (SownInStone.UI.SurvivalUIManager.Instance.IsDialogueActive)
                    {
                        if (!SownInStone.UI.SurvivalUIManager.Instance.IsChoiceActive)
                        {
                            SownInStone.UI.SurvivalUIManager.Instance.CloseDialogue();
                        }
                        return;
                    }

                    // Mở hội thoại lựa chọn nghỉ ngơi trên nóc nhà
                    SownInStone.UI.SurvivalUIManager.Instance.ShowDialogueWithChoices(
                        "Sinh tồn trên nóc nhà",
                        "Bạn có muốn nghỉ ngơi trên nóc nhà để chờ nước lũ rút không? (Trôi qua 1 giờ, hồi phục 15 Thể lực, giảm 10 nhiễm lạnh, hồi phục 5 tinh thần)",
                        "Nghỉ ngơi (1 giờ)",
                        () => {
                            // Thực hiện nghỉ ngơi
                            if (GameManager.Instance != null)
                            {
                                GameManager.Instance.AdvanceTime(1f);
                            }
                            if (PlayerStats.Instance != null)
                            {
                                PlayerStats.Instance.ModifyStamina(15f);
                                PlayerStats.Instance.ApplyColdStress(-10f);
                                PlayerStats.Instance.ModifyMorale(5f);
                            }
                            if (SownInStone.UI.SurvivalUIManager.Instance != null)
                            {
                                SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(
                                    "Sinh tồn trên nóc nhà", 
                                    "Bạn đã chợp mắt một lúc. Thể lực và tinh thần phục hồi nhẹ, cơ thể ấm áp hơn."
                                );
                            }
                        },
                        "Hủy bỏ",
                        () => {
                            if (SownInStone.UI.SurvivalUIManager.Instance != null)
                            {
                                SownInStone.UI.SurvivalUIManager.Instance.CloseDialogue();
                            }
                        }
                    );
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
                            Coracle boat = closestCollider.GetComponent<Coracle>();
                            if (boat != null)
                            {
                            prompt = $"[{keyInteract}] Lên thuyền thúng";
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
            foreach (var col in colliders)
            {
                if (col.gameObject == gameObject) continue;

                // Kiểm tra xem đối tượng có chứa bất kỳ Script tương tác nào không
                bool isInteractable = col.GetComponent<IInteractable>() != null ||
                                      col.GetComponent<NPCCharacter>() != null ||
                                      col.GetComponent<AncestralAltar>() != null ||
                                      col.GetComponent<Coracle>() != null ||
                                      col.GetComponent<SoilCell>() != null ||
                                      col.GetComponent<MudPuddle>() != null;

                if (isInteractable)
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closest = col;
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
                                        if (i >= 4) break;
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
                                        TutorialManager.Instance.OnOThamFloodBoardPlaced();
                                        didMakePrePlacedSolid = true;
                                    }
                                }
                                else
                                {
                                    var npcs = FindObjectsByType<NPCCharacter>(FindObjectsInactive.Exclude);
                                    var oTham = System.Array.Find(npcs, n => n.characterType == NPCCharacter.StoryCharacterType.OTham);
                                    if (oTham != null)
                                    {
                                        Vector3 oThamPos = oTham.transform.position;
                                        Vector3 forward = oTham.transform.forward;
                                        Vector3 right = oTham.transform.right;
                                        Vector3[] targets = new Vector3[]
                                        {
                                            oThamPos + forward * 2.5f + right * -1.8f,
                                            oThamPos + forward * 2.5f + right * -0.6f,
                                            oThamPos + forward * 2.5f + right * 0.6f,
                                            oThamPos + forward * 2.5f + right * 1.8f
                                        };
                                        int bestIndex = -1;
                                        float minDistance = float.MaxValue;
                                        for (int i = 0; i < 4; i++)
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
                                            TutorialManager.Instance.OnOThamFloodBoardPlaced();
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
    }
}
