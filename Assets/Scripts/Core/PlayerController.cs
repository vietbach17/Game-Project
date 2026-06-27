using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SownInStone.UI;
using SownInStone.Community;
using SownInStone.Storage;
using SownInStone.Agriculture;

namespace SownInStone.Core
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5.0f;
        [SerializeField] private float runSpeedMultiplier = 1.5f;
        [SerializeField] private float interactRadius = 2.5f;
        public KeyCode keyMoveUp = KeyCode.W;
        public KeyCode keyMoveDown = KeyCode.S;
        public KeyCode keyMoveLeft = KeyCode.A;
        public KeyCode keyMoveRight = KeyCode.D;
        public KeyCode keyInteract = KeyCode.E;
        public KeyCode keyRun = KeyCode.LeftShift;
        public bool isWearingNonLa = false;
        public SoilCell CurrentTargetSoilCell { get; private set; }

        [Header("Evacuation Status (Runtime)")]
        private bool isEvacuationActive = false;
        private int rescuedCount = 0;
        private float evacuationTimer = 0f;

        // Danh sách nội bộ theo dõi 4 NPC cốt lõi đã được cứu hộ chưa
        private HashSet<string> evacuatedNPCs = new HashSet<string>();

        private Rigidbody rb;
        private bool isPerformingAction = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // Đăng ký nhận sự kiện chuyển Phase từ GameManager
            GameManager.OnPhaseChanged += HandlePhaseChangedEvent;
        }

        private void OnDestroy()
        {
            GameManager.OnPhaseChanged -= HandlePhaseChangedEvent;
        }

        private void Update()
        {
            // Nếu sự kiện chạy lũ đang chạy, cập nhật đồng hồ đếm ngược nội bộ
            if (isEvacuationActive)
            {
                evacuationTimer -= Time.deltaTime;

                if (evacuationTimer <= 0f)
                {
                    evacuationTimer = 0f;
                    isEvacuationActive = false;
                    HandleEvacuationFailure(); // Hết giờ -> Thua cuộc
                }
            }
        }

        /// <summary>
        /// Lắng nghe sự kiện chuyển Phase để chủ động kích hoạt cơ chế đếm ngược
        /// </summary>
        private void HandlePhaseChangedEvent(GamePhase newPhase)
        {
            if (newPhase == GamePhase.MuaBao)
            {
                // Kích hoạt trạng thái chạy lũ khẩn cấp
                isEvacuationActive = true;
                evacuationTimer = 45f; // 45 giây đếm ngược
                rescuedCount = 0;
                evacuatedNPCs.Clear();
            }
        }

        /// <summary>
        /// Logic xử lý Tương tác cứu hộ dắt NPC: Gọi khi bấm phím tắt hoặc click Proximity Panel của NPC
        /// </summary>
        public void TryEvacuateNPC(NPCCharacter npc)
        {
            if (!isEvacuationActive) return;

            string npcID = npc.CharacterType.ToString(); // "OTham", "BacNam", "CuBay", "BeTi"

            if (evacuatedNPCs.Contains(npcID))
            {
                SurvivalUIManager.Instance.ShowHUDToast("Người này đã được dắt đến nơi an toàn!");
                return;
            }

            // Tiến hành cứu hộ dắt NPC
            evacuatedNPCs.Add(npcID);
            rescuedCount++;

            // 1. Cộng mạnh điểm Nghĩa Tình vào hệ thống
            CommunityManager.Instance.AddKarma(15);

            // 2. Ẩn GameObject của NPC đó đi (Đã dắt lên vùng cao khuất camera)
            npc.gameObject.SetActive(false);

            // 3. Phát thông báo Toast Regex cổ vũ tinh thần người chơi
            SurvivalUIManager.Instance.ShowHUDToast($"+ Đã đưa {npc.NPCName} đi sơ tán! ({rescuedCount}/4)");

            // Kiểm tra xem đã dắt đủ cả 4 NPC cốt lõi chưa
            if (rescuedCount == 4)
            {
                isEvacuationActive = false;
                StartCoroutine(TransitionToRoofRefugeRoutine());
            }
        }

        /// <summary>
        /// Routine xử lý kết cục Thành công: Chuyển Phase lên nóc nhà lánh nạn an toàn
        /// </summary>
        private IEnumerator TransitionToRoofRefugeRoutine()
        {
            yield return new WaitForSeconds(0.5f);

            // Khóa di chuyển và tương tác tạm thời để chạy màn hình đen
            isPerformingAction = true;

            // Phát lệnh chuyển mốc hậu bão, WeatherManager tự động chuyển skybox dâng lũ cực đại
            GameManager.Instance.TransitionToPhase(GamePhase.PhuSa);

            // Dịch chuyển tức thời (Teleport) Thành lên tọa độ nóc nhà Thanh_House
            transform.position = new Vector3(0f, 3.5f, -10f);
            rb.linearVelocity = Vector3.zero;

            // Kích hoạt lại toàn bộ 4 NPC xuất hiện đứng trú ẩn cùng Thành trên mái nhà
            ActivateNPCsOnRoof();

            // Cấp phát 5 mì tôm cứu trợ vào kho đồ của người chơi
            ItemData noodleItem = StorageManager.Instance.GetItemDataByID("item_noodles");
            if (noodleItem != null)
            {
                StorageManager.Instance.AddItem(noodleItem, 5);
            }

            SurvivalUIManager.Instance.ShowHUDToast("✓ Dân làng an toàn trú ẩn trên nóc nhà! Nhận 5 Mì tôm cứu trợ.");
            isPerformingAction = false;
        }

        /// <summary>
        /// Logic xử lý kết cục Thất bại: Chậm chân để lũ cuốn ngập đầu
        /// </summary>
        private void HandleEvacuationFailure()
        {
            isPerformingAction = true; // Khóa điều khiển hoàn toàn
            rb.linearVelocity = Vector3.zero;

            Debug.Log("[PlayerController] Hết giờ! Nước lũ dâng cao cuốn ngập ngôi làng.");

            if (SurvivalUIManager.Instance != null)
            {
                // Gọi màn hình báo thua cuộc chuyên dụng (Drown Game Over screen)
                SurvivalUIManager.Instance.ShowDrownGameOverPanel();
            }
        }

        private void ActivateNPCsOnRoof()
        {
            // Logic tìm kiếm lại các reference NPC trong scene để SetActive(true) 
            // và xếp đặt tọa độ đứng xung quanh vùng Roof_Anchor_Nodes của Thanh_House
        }

        public void LoadKeyBindings()
        {
            keyMoveUp = (KeyCode)PlayerPrefs.GetInt("Key_MoveUp", (int)KeyCode.W);
            keyMoveDown = (KeyCode)PlayerPrefs.GetInt("Key_MoveDown", (int)KeyCode.S);
            keyMoveLeft = (KeyCode)PlayerPrefs.GetInt("Key_MoveLeft", (int)KeyCode.A);
            keyMoveRight = (KeyCode)PlayerPrefs.GetInt("Key_MoveRight", (int)KeyCode.D);
            keyInteract = (KeyCode)PlayerPrefs.GetInt("Key_Interact", (int)KeyCode.E);
            keyRun = (KeyCode)PlayerPrefs.GetInt("Key_Run", (int)KeyCode.LeftShift);
        }

        public void SetAnimTrigger(string triggerName)
        {
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null) animator.SetTrigger(triggerName);
        }

        public void LockMovement(float duration)
        {
            StartCoroutine(LockMovementRoutine(duration));
        }

        private IEnumerator LockMovementRoutine(float duration)
        {
            isPerformingAction = true;
            yield return new WaitForSeconds(duration);
            isPerformingAction = false;
        }

        public void TriggerRescueSequence()
        {
            transform.position += Vector3.up * 0.5f;
        }

        public void EquipNonLa(GameObject hatPrefab)
        {
            isWearingNonLa = true;
            if (hatPrefab == null) return;

            GameObject hat = Instantiate(hatPrefab, transform);
            hat.name = "Equipped_NonLa";
            hat.transform.localPosition = new Vector3(0f, 1.8f, 0f);
        }

        public void OpenTradeMenu(NPCCharacter npc)
        {
            SurvivalUIManager.Instance?.ToggleShop(true);
        }

        public float CurrentEvacuationTimer => evacuationTimer;
        public bool IsEvacuationActive => isEvacuationActive;
    }
}