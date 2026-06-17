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
        [SerializeField] private float interactRadius = 1.2f;
        
        [Tooltip("Lớp vật lý (Layer) chứa các đối tượng có thể tương tác (nên chọn Everything hoặc thiết lập riêng).")]
        [SerializeField] private LayerMask interactableLayers = ~0;

        [Header("--- TRỒNG TRỌT KIỂM THỬ ---")]
        [Tooltip("Hạt giống dùng để gieo hạt khi ruộng đất trống và ẩm.")]
        [SerializeField] private CropData testSeedData;

        [Tooltip("Vật phẩm Hạt giống thực tế trong kho đồ tiêu hao khi gieo trồng.")]
        [SerializeField] private ItemData seedItem;

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
                seedItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_PreservedCrop.asset");
            }
            if (noodlesItem == null)
            {
                noodlesItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Noodles.asset");
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
                
                // Sử dụng MovePosition để di chuyển mượt mà, tôn trọng va chạm và tránh kẹt vật lý
                Vector3 nextPos = rb.position + new Vector3(targetMoveDir.x * currentSpeed, 0f, targetMoveDir.z * currentSpeed) * Time.fixedDeltaTime;
                rb.MovePosition(nextPos);
                rb.linearVelocity = Vector3.zero;
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
                
                Vector3 nextPos = rb.position + new Vector3(fallbackMoveDir.x * currentSpeed, 0f, fallbackMoveDir.z * currentSpeed) * Time.fixedDeltaTime;
                rb.MovePosition(nextPos);
                rb.linearVelocity = Vector3.zero;
            }
            else
            {
                debugTargetMoveDir = Vector3.zero;
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                }
            }

            if (isOnRoof)
            {
                Vector3 constrainedPos = rb.position;
                constrainedPos.x = Mathf.Clamp(constrainedPos.x, -2.2f, 2.2f);
                constrainedPos.z = Mathf.Clamp(constrainedPos.z, -12.0f, -8.0f);
                constrainedPos.y = 3.5f;

                rb.position = constrainedPos;
                transform.position = constrainedPos;
                rb.linearVelocity = Vector3.zero;
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
            if (moveInput.sqrMagnitude > 0.01f && PlayerStats.Instance != null && WeatherManager.Instance != null)
            {
                // Nếu đang di chuyển giữa trưa nắng Gió Lào
                if (WeatherManager.Instance.currentVisualWeather == WeatherType.GioLao && WeatherManager.Instance.Temperature > 38f)
                {
                    // Tiêu hao thêm 1.5 Stamina mỗi giây khi di chuyển ngoài nắng nóng
                    PlayerStats.Instance.ModifyStamina(-1.5f * Time.deltaTime);
                }
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
        /// Quét tìm các đối tượng xung quanh trong bán kính quét và thực hiện tương tác.
        /// Ưu tiên đối tượng ở gần nhất.
        /// </summary>
        private void TryPerformInteraction()
        {
            if (isPerformingAction) return;

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

            // Quét hình cầu vật lý 3D xung quanh người chơi
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactRadius, interactableLayers);
            
            Collider closestCollider = null;
            float minDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                if (col.gameObject == gameObject) continue; // Bỏ qua bản thân người chơi

                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestCollider = col;
                }
            }

            if (closestCollider != null)
            {
                GameObject target = closestCollider.gameObject;
                Debug.Log($"[PLAYER] Đã tìm thấy đối tượng gần nhất: {target.name}");

                // 1. Tương tác với Dân làng (NPC Bác Năm, O Thắm)
                NPCCharacter npc = target.GetComponent<NPCCharacter>();
                if (npc != null)
                {
                    // Nếu khung đối thoại mới đang mở và không có lựa chọn đang chờ, nhấn E sẽ đóng nó lại
                    if (SownInStone.UI.SurvivalUIManager.Instance != null && SownInStone.UI.SurvivalUIManager.Instance.IsDialogueActive)
                    {
                        if (!SownInStone.UI.SurvivalUIManager.Instance.IsChoiceActive)
                        {
                            SownInStone.UI.SurvivalUIManager.Instance.CloseDialogue();
                        }
                        return;
                    }

                    // Mở hội thoại tương ứng
                    if (SownInStone.UI.SurvivalUIManager.Instance != null)
                    {
                        string greeting = $"\"Chào con, Thành! Hôm nay ghé qua bác/o chơi có việc chi không?\"";
                        if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam)
                        {
                            greeting = "\"Chào Thành! Việc cải tạo mảnh ruộng cát bạc màu gian khổ đến đâu rồi con?\"";
                        }
                        else if (npc.characterType == NPCCharacter.StoryCharacterType.OTham)
                        {
                            greeting = "\"Ủa Thành đó hả con! Ghé đại lý o chơi chút hén, hôm nay o Thắm nhập giống mới đây nè!\"";
                        }

                        if (npc.characterType == NPCCharacter.StoryCharacterType.OTham)
                        {
                            SownInStone.UI.SurvivalUIManager.Instance.ShowDialogueWithChoices(
                                npc.NPCName,
                                greeting,
                                "Trò chuyện",
                                () => {
                                    string dialogue = npc.GetDialogue();
                                    SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, dialogue);
                                    PlayerStats.Instance?.ModifyMorale(2f);
                                    npc.ModifyAffection(1);
                                },
                                "Giúp việc (Vần công)",
                                () => {
                                    float currentStamina = PlayerStats.Instance != null ? PlayerStats.Instance.CurrentStamina : 0f;
                                    if (currentStamina >= 20f)
                                    {
                                        PlayerStats.Instance.ModifyStamina(-20f);
                                        npc.ModifyVanCongCredits(1);
                                        npc.ModifyAffection(5);
                                        string workDialog = npc.GetWorkDialogue(true);
                                        SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, workDialog);
                                    }
                                    else
                                    {
                                        string workDialog = npc.GetWorkDialogue(false);
                                        SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, workDialog);
                                    }
                                },
                                "Giao dịch (Mua/Bán)",
                                () => {
                                    OpenTradeMenu(npc);
                                }
                            );
                        }
                        else
                        {
                            SownInStone.UI.SurvivalUIManager.Instance.ShowDialogueWithChoices(
                                npc.NPCName,
                                greeting,
                                "Trò chuyện",
                                () => {
                                    string dialogue = npc.GetDialogue();
                                    SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, dialogue);
                                    PlayerStats.Instance?.ModifyMorale(2f);
                                    npc.ModifyAffection(1);
                                },
                                "Giúp việc (Vần công)",
                                () => {
                                    float currentStamina = PlayerStats.Instance != null ? PlayerStats.Instance.CurrentStamina : 0f;
                                    if (currentStamina >= 20f)
                                    {
                                        PlayerStats.Instance.ModifyStamina(-20f);
                                        npc.ModifyVanCongCredits(1);
                                        npc.ModifyAffection(5);
                                        string workDialog = npc.GetWorkDialogue(true);
                                        SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, workDialog);
                                    }
                                    else
                                    {
                                        string workDialog = npc.GetWorkDialogue(false);
                                        SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, workDialog);
                                    }
                                }
                            );
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

                int totalYield = 0;
                ItemData harvestItem = null;

                if (activeSoil.IsParentField)
                {
                    foreach (var child in activeSoil.childCells)
                    {
                        if (child != null && child.plantedCrop != null && child.plantedCrop.IsReadyToHarvest())
                        {
                            if (harvestItem == null) harvestItem = child.plantedCrop.cropData.HarvestedItem;
                            totalYield += child.plantedCrop.ActionHarvest();
                        }
                    }
                }
                else
                {
                    harvestItem = activeSoil.plantedCrop.cropData.HarvestedItem;
                    totalYield = activeSoil.plantedCrop.ActionHarvest();
                }

                if (totalYield > 0 && StorageManager.Instance != null && harvestItem != null)
                {
                    StorageManager.Instance.AddItem(harvestItem, totalYield);
                    Debug.Log($"[PLAYER] Thu hoạch thành công ruộng cây: {harvestItem.ItemName} x{totalYield}!");
                    string itemName = !string.IsNullOrEmpty(harvestItem.ItemName) ? harvestItem.ItemName : "Nông sản";
                    string harvestMsg = $"Thu hoạch thành công: +{totalYield} {itemName}";
                    PlayerStats.Instance.TriggerAlert(harvestMsg);
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

            if (canPlant)
            {
                if (testSeedData != null && seedItem != null)
                {
                    int availableSeeds = 0;
                    if (StorageManager.Instance != null)
                    {
                        var slots = StorageManager.Instance.GetStorageSlots();
                        var seedSlot = slots.Find(s => s.item.ItemID == seedItem.ItemID);
                        if (seedSlot != null && seedSlot.quantity > 0)
                        {
                            availableSeeds = seedSlot.quantity;
                        }
                    }

                    int seedsToPlant = Mathf.Min(emptySlots, availableSeeds);

                    if (seedsToPlant > 0)
                    {
                        if (StorageManager.Instance.RemoveItem(seedItem, seedsToPlant))
                        {
                            if (activeSoil.IsParentField)
                            {
                                int planted = 0;
                                foreach (var child in activeSoil.childCells)
                                {
                                    if (child != null && child.plantedCrop == null && planted < seedsToPlant)
                                    {
                                        if (child.ActionPlantCrop(testSeedData))
                                        {
                                            planted++;
                                        }
                                    }
                                }
                                PlayerStats.Instance.TriggerAlert($"Đã gieo {planted} hạt giống!");
                            }
                            else
                            {
                                activeSoil.ActionPlantCrop(testSeedData);
                            }

                            SetAnimTrigger("Plant");
                            StartCoroutine(LockMovementForSeconds(plantActionDuration));
                            Debug.Log("Plant success: seed consumed");
                        }
                    }
                    else
                    {
                        Debug.Log("Plant failed: no seeds");
                        SownInStone.UI.SurvivalUIManager.Instance?.ShowDialogue("Hệ thống", "Không có hạt giống Khoai Lang trong kho đồ! Hãy đến gặp đại lý O Thắm để mua giống tái thiết.");
                    }
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
                    activeSoil.ActionWaterSoil(50f);
                    Debug.Log($"Watered {dryCells} cell(s)");
                    PlayerStats.Instance.TriggerAlert($"Đã tưới nước cho {dryCells} ô ruộng!");
                }
                return;
            }

            Debug.Log("[PLAYER] Ô đất đang ở trạng thái tốt, không cần thao tác thêm.");
        }

        private void OpenTradeMenu(NPCCharacter npc)
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
                SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt("[E] Nghỉ ngơi trên nóc nhà");
                return;
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, interactRadius, interactableLayers);
            Collider closestCollider = null;
            float minDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                if (col.gameObject == gameObject) continue;
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestCollider = col;
                }
            }

            if (closestCollider != null)
            {
                string prompt = "";
                NPCCharacter npc = closestCollider.GetComponent<NPCCharacter>();
                if (npc != null)
                {
                    prompt = $"[E] Trò chuyện với {npc.NPCName}";
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

                            if (cellsWithRocks > 0)
                                prompt = "[E] Dọn đá cải tạo ruộng";
                            else if (hasReadyCrops)
                                prompt = "[E] Thu hoạch khoai tươi";
                            else if (emptySlots > 0)
                                prompt = "[E] Gieo hạt giống khoai";
                            else if (soil.Moisture < 40f)
                                prompt = "[E] Tưới nước cho đất ẩm";
                            else
                                prompt = "Đất đã được gieo hạt";
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

        private void SetAnimFloat(string paramName, float value)
        {
            if (animator != null && animatorParams.Contains(paramName))
            {
                animator.SetFloat(paramName, value);
            }
        }

        private void SetAnimTrigger(string paramName)
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

        private void OnDrawGizmosSelected()
        {
            // Vẽ vòng tròn kiểm tra phạm vi tương tác trong Scene View của Unity Editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
