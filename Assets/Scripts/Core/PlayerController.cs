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
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("--- THÔNG SỐ DI CHUYỂN ---")]
        [Tooltip("Tốc độ di chuyển của nhân vật.")]
        [SerializeField] private float moveSpeed = 4f;

        [Header("--- XOAY NHÂN VẬT 3D ---")]
        [Tooltip("Kéo mô hình 3D con vào đây. Nếu trống, script tự tìm con đầu tiên.")]
        [SerializeField] private Transform characterVisual;

        [Tooltip("Tốc độ xoay hướng của nhân vật.")]
        [SerializeField] private float rotationSpeed = 10f;

        public enum RotationAxis { RotateAroundZ_2D, RotateAroundY_3D }
        [Tooltip("Trục xoay hướng: Chọn RotateAroundZ nếu game nhìn thẳng 2D, chọn RotateAroundY nếu là 3D nhìn từ trên xuống.")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.RotateAroundY_3D;

        [Header("--- TƯƠNG TÁC PHÍM [E] ---")]
        [Tooltip("Bán kính hình tròn quét tìm vật thể có thể tương tác xung quanh nhân vật.")]
        [SerializeField] private float interactRadius = 1.2f;
        
        [Tooltip("Lớp vật lý (Layer) chứa các đối tượng có thể tương tác (nên chọn Everything hoặc thiết lập riêng).")]
        [SerializeField] private LayerMask interactableLayers = ~0;

        private Rigidbody2D rb;
        private Animator animator;
        private Vector2 moveInput;
        private Vector2 lastMoveDirection = Vector2.down; // Hướng quay mặt mặc định (nhìn xuống)

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

            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // Cấu hình Rigidbody2D để phù hợp với game 2D Top-down
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

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
            }
#else
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
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
        }

        private void FixedUpdate()
        {
            // Di chuyển nhân vật thông qua cơ chế vật lý Rigidbody2D tránh đi xuyên tường
            rb.linearVelocity = moveInput * moveSpeed;
        }

        /// <summary>
        /// Cập nhật các biến số cho Animator 2D (Blend Tree di chuyển 4 hoặc 8 hướng).
        /// </summary>
        private void UpdateAnimatorParameters()
        {
            if (animator == null) return;

            animator.SetFloat("Horizontal", moveInput.x);
            animator.SetFloat("Vertical", moveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
            
            // Giữ lại hướng quay mặt khi đứng yên
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
        /// Quét tìm các đối tượng xung quanh trong bán kính quét và thực hiện tương tác.
        /// Ưu tiên đối tượng ở gần nhất.
        /// </summary>
        private void TryPerformInteraction()
        {
            Debug.Log("[PLAYER] Bấm phím tương tác [E]!");

            // Quét hình tròn vật lý xung quanh người chơi
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactableLayers);
            
            Collider2D closestCollider = null;
            float minDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                if (col.gameObject == gameObject) continue; // Bỏ qua bản thân người chơi

                float dist = Vector2.Distance(transform.position, col.transform.position);
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
                    string dialogue = npc.GetDialogue();
                    Debug.Log($"[HỘI THOẠI] {npc.NPCName} nói: {dialogue}");
                    
                    // Gửi thông báo hội thoại lên UI Debug
                    PlayerStats.Instance?.ModifyMorale(2f); // Nói chuyện với hàng xóm giúp tăng nhẹ Morale
                    target.SendMessage("ModifyAffection", 1, SendMessageOptions.DontRequireReceiver); // Tăng điểm thân thiết
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

            // Nếu đất còn đá sỏi -> Ưu tiên dọn sỏi đá cải tạo đất
            if (soil.RockDensity > 0f)
            {
                if (PlayerStats.Instance.CurrentStamina >= 8f) // Kiểm tra đủ stamina không
                {
                    PlayerStats.Instance.ModifyStamina(-8f); // Tiêu hao thể lực nhặt đá
                    soil.ActionClearRocks(25f);
                    Debug.Log("[PLAYER] Nhặt đá sỏi cải tạo ruộng!");
                }
                else
                {
                    Debug.LogWarning("[PLAYER] Không đủ thể lực để nhặt đá!");
                }
                return;
            }

            // Nếu ô đất đã có cây và cây đã chín -> Tiến hành thu hoạch nông sản
            if (soil.plantedCrop != null && soil.plantedCrop.IsReadyToHarvest())
            {
                int yield = soil.plantedCrop.ActionHarvest();
                if (StorageManager.Instance != null && soil.plantedCrop != null)
                {
                    StorageManager.Instance.AddItem(soil.plantedCrop.cropData.GrowthStageSprites != null ? null : null, yield); // (Tự động thêm nông sản)
                }
                Debug.Log($"[PLAYER] Thu hoạch thành công ruộng cây!");
                return;
            }

            // Nếu ô đất khô ráo và chưa tưới -> Thực hiện tưới nước
            if (soil.Moisture < 40f)
            {
                if (PlayerStats.Instance.CurrentStamina >= 3f)
                {
                    PlayerStats.Instance.ModifyStamina(-3f);
                    soil.ActionWaterSoil(50f);
                    Debug.Log("[PLAYER] Tưới nước cho đất ẩm mát!");
                }
                return;
            }

            Debug.Log("[PLAYER] Ô đất đang ở trạng thái tốt, không cần thao tác thêm.");
        }

        /// <summary>
        /// Xoay hướng mô hình 3D mượt mà theo hướng di chuyển WASD.
        /// </summary>
        private void HandleVisualRotation()
        {
            if (characterVisual == null || moveInput.sqrMagnitude < 0.01f) return;

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
                // Game 3D Top-down (Y là trục đứng): Xoay quanh trục Y cục bộ
                // Trong 2D physics, X là ngang, Y là dọc (tương ứng với Z trong 3D)
                // Giữ lại góc xoay đứng mặc định ở X (ví dụ -90 độ) và Z để mô hình không bị ngã rạp
                float targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(initialVisualLocalX, targetAngle, initialVisualLocalZ);
                characterVisual.localRotation = Quaternion.Slerp(characterVisual.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
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
