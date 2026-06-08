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

        private Rigidbody rb;
        private Animator animator;
        private Vector2 moveInput;
        private Vector2 lastMoveDirection = Vector2.down; // Hướng quay mặt mặc định (nhìn xuống)
        private bool isRunning; // Cho biết người chơi đang chạy nhanh hay đi bộ
        private bool isPerformingAction; // Cho biết người chơi đang thực hiện động tác trồng trọt

        // Lưu trữ góc xoay cục bộ ban đầu của mô hình 3D để tránh bị lật ngã do sai lệch trục nhập khẩu
        private float initialVisualLocalX = 0f;
        private float initialVisualLocalZ = 0f;

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
#endif
        }

        private void Update()
        {
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

                rb.linearVelocity = new Vector3(targetMoveDir.x * currentSpeed, rb.linearVelocity.y, targetMoveDir.z * currentSpeed);
            }
            else if (moveInput.sqrMagnitude > 0.01f)
            {
                // Fallback nếu không tìm thấy Camera chính
                rb.linearVelocity = new Vector3(moveInput.x * currentSpeed, rb.linearVelocity.y, moveInput.y * currentSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
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
            animator.SetFloat("Speed", newAnimSpeed);

            // Keep other parameters for compatibility/harmlessness
            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
            animator.SetFloat("LastHorizontal", lastMoveDirection.x);
            animator.SetFloat("LastVertical", lastMoveDirection.y);
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

            // Priority 1: Clear rocks
            if (soil.RockDensity > 0f)
            {
                if (PlayerStats.Instance.CurrentStamina >= 8f)
                {
                    PlayerStats.Instance.ModifyStamina(-8f);
                    if (animator != null) animator.SetTrigger("Dig");
                    StartCoroutine(LockMovementForSeconds(digActionDuration));
                    soil.ActionClearRocks(999f);
                    Debug.Log("Cleared rocks on soil cell");
                    Debug.Log("[PLAYER] Nhặt đá sỏi cải tạo ruộng!");
                }
                else
                {
                    Debug.LogWarning("[PLAYER] Không đủ thể lực để nhặt đá!");
                }
                return;
            }

            // Priority 2: Harvest crop
            else if (soil.plantedCrop != null && soil.plantedCrop.IsReadyToHarvest())
            {
                ItemData harvestItem = soil.plantedCrop.cropData.HarvestedItem;
                if (animator != null) animator.SetTrigger("Harvest");
                StartCoroutine(LockMovementForSeconds(harvestActionDuration));
                int yield = soil.plantedCrop.ActionHarvest();
                Debug.Log("Harvested crop");
                if (StorageManager.Instance != null && harvestItem != null)
                {
                    StorageManager.Instance.AddItem(harvestItem, yield);
                    Debug.Log($"[PLAYER] Thu hoạch thành công ruộng cây: {harvestItem.ItemName} x{yield}!");
                }
                
                string itemName = (harvestItem != null && !string.IsNullOrEmpty(harvestItem.ItemName)) ? harvestItem.ItemName : "Nông sản";
                string harvestMsg = $"Thu hoạch thành công: +{yield} {itemName}";
                Debug.Log(harvestMsg);
                PlayerStats.Instance.TriggerAlert(harvestMsg);

                return;
            }

            // Priority 3: Plant seed (Moved before watering to fix pacing bug)
            else if (soil.plantedCrop == null)
            {
                if (testSeedData != null && seedItem != null)
                {
                    bool hasSeed = false;
                    if (StorageManager.Instance != null)
                    {
                        var slots = StorageManager.Instance.GetStorageSlots();
                        var seedSlot = slots.Find(s => s.item.ItemID == seedItem.ItemID);
                        if (seedSlot != null && seedSlot.quantity > 0)
                        {
                            hasSeed = true;
                            Debug.Log("Plant attempt: seed item found");
                        }
                        else
                        {
                            Debug.Log("Plant attempt: seed item not found");
                        }
                    }

                    if (hasSeed && StorageManager.Instance.RemoveItem(seedItem, 1))
                    {
                        bool success = soil.ActionPlantCrop(testSeedData);
                        if (success)
                        {
                            if (animator != null) animator.SetTrigger("Plant");
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
            else if (soil.Moisture < 40f)
            {
                if (PlayerStats.Instance.CurrentStamina >= 3f)
                {
                    PlayerStats.Instance.ModifyStamina(-3f);
                    if (animator != null) animator.SetTrigger("Water");
                    StartCoroutine(LockMovementForSeconds(waterActionDuration));
                    soil.ActionWaterSoil(50f);
                    Debug.Log("Watered soil");
                    Debug.Log("[PLAYER] Tưới nước cho đất ẩm mát!");
                }
                return;
            }

            Debug.Log("[PLAYER] Ô đất đang ở trạng thái tốt, không cần thao tác thêm.");
        }

        private void OpenTradeMenu(NPCCharacter npc)
        {
            if (SownInStone.UI.SurvivalUIManager.Instance == null) return;

            bool isPhuSa = (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.PhuSa);
            int price = isPhuSa ? 6 : 10;

            string tradeGreeting = $"\"Đại lý o Thắm bán giống Khoai Lang giá {price} xu (giá gốc 10 xu) và thu mua khoai tươi 25 xu/củ. Con muốn mua hay bán?\"";

            SownInStone.UI.SurvivalUIManager.Instance.ShowDialogueWithChoices(
                npc.NPCName,
                tradeGreeting,
                $"Mua Hạt giống (-{price} Xu)",
                () => {
                    if (PlayerStats.Instance != null && PlayerStats.Instance.Coins >= price)
                    {
                        PlayerStats.Instance.ModifyCoins(-price);
                        if (StorageManager.Instance != null && seedItem != null)
                        {
                            StorageManager.Instance.AddItem(seedItem, 1);
                            SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, $"\"Đây là hạt giống Khoai của con. Hòm đồ gia đình đã có hạt giống mới!\"");
                        }
                    }
                    else
                    {
                        int current = PlayerStats.Instance != null ? PlayerStats.Instance.Coins : 0;
                        SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, $"\"Không đủ tiền rồi con ơi! Thiếu {price - current} xu nữa nha con.\"");
                    }
                },
                "Bán Khoai tươi (25 Xu/củ)",
                () => {
                    if (StorageManager.Instance != null && testSeedData != null && testSeedData.HarvestedItem != null)
                    {
                        ItemData freshCropItem = testSeedData.HarvestedItem;
                        var slots = StorageManager.Instance.GetStorageSlots();
                        var freshSlot = slots.Find(s => s.item.ItemID == freshCropItem.ItemID);
                        int count = freshSlot != null ? freshSlot.quantity : 0;

                        if (count > 0)
                        {
                            if (StorageManager.Instance.RemoveItem(freshCropItem, count))
                            {
                                int earn = count * 25;
                                PlayerStats.Instance?.ModifyCoins(earn);
                                SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, $"\"O gom hết {count} củ khoai tươi, gửi con {earn} xu nghen!\"");
                            }
                        }
                        else
                        {
                            SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, $"\"Ủa con có củ khoai tươi nào trong kho mô mà đòi bán o nè!\"");
                        }
                    }
                },
                "Quay lại",
                () => {
                    // Mở lại menu chào của O Thắm
                    string greeting = $"\"Ủa Thành đó hả con! Ghé đại lý o chơi chút hén, hôm nay o Thắm nhập giống mới đây nè!\"";
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
            );
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
                            if (soil.RockDensity > 0f)
                                prompt = "[E] Dọn đá cải tạo ruộng";
                            else if (soil.plantedCrop != null && soil.plantedCrop.IsReadyToHarvest())
                                prompt = "[E] Thu hoạch khoai tươi";
                            else if (soil.plantedCrop == null)
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

        private void OnDrawGizmosSelected()
        {
            // Vẽ vòng tròn kiểm tra phạm vi tương tác trong Scene View của Unity Editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
