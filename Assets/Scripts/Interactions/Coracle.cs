using UnityEngine;
using System.Collections.Generic;
using SownInStone.Core;
using SownInStone.Community;
using SownInStone.Weather;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SownInStone.Interactions
{
    /// <summary>
    /// Thuyền thúng (Coracle) interaction and movement controller.
    /// Allows the player to enter, steer the boat during flood, and exit safely.
    /// Supports carrying multiple rescued NPCs and delivering them to Thanh_House roof.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class Coracle : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 4f;
        public float rotationSpeed = 60f;

        [Header("Seat Positions")]
        [Tooltip("Offset position where the player will stand/sit inside the boat.")]
        public Vector3 playerSeatOffset = new Vector3(0f, 0.2f, 0f);

        // NPC seat offsets (4 seats arranged around the player)
        private static readonly Vector3[] npcSeatOffsets = new Vector3[]
        {
            new Vector3(-0.55f, 0.35f,  0.5f),  // Slot 0
            new Vector3( 0.55f, 0.35f,  0.5f),  // Slot 1
            new Vector3(-0.55f, 0.35f, -0.4f),  // Slot 2
            new Vector3( 0.55f, 0.35f, -0.4f),  // Slot 3
        };

        // NPC delivery range from Thanh_House
        [SerializeField] private float deliveryRange = 6f;

        private Rigidbody rb;
        private BoxCollider boxCollider;
        private PlayerController activePlayer;
        private bool isOccupied;
        private Vector3 exitOffset = new Vector3(0f, 0f, -1.8f);
        private float initialY = 0.51f; // Tọa độ Y mặc định của thuyền khi chưa có lũ

        // Rescued NPCs currently on the boat
        private readonly List<NPCCharacter> rescuedNPCsOnBoard = new List<NPCCharacter>();

        // Singleton-like reference so TutorialManager can find it
        public static Coracle Instance { get; private set; }

        /// <summary>True when player is currently on board.</summary>
        public bool IsPlayerOnBoard => isOccupied && activePlayer != null;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            boxCollider = GetComponent<BoxCollider>();

            rb.useGravity = false;
            rb.isKinematic = false;
            rb.mass = 150f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;

            initialY = transform.position.y;
        }

        private void Update()
        {
            // Float boat to water surface if flood is active, else stay at initial position
            float targetY = initialY;
            if (WeatherManager.Instance != null && WeatherManager.Instance.FloodLevel > 0.01f)
            {
                targetY = -0.05f + WeatherManager.Instance.FloodLevel;
            }

            Vector3 currentPos = transform.position;
            if (currentPos.y < targetY - 0.05f || currentPos.y > targetY + 0.05f)
            {
                currentPos.y = Mathf.Lerp(currentPos.y, targetY, Time.deltaTime * 5f);
                transform.position = currentPos;
                rb.position = currentPos;
            }

            if (isOccupied && activePlayer != null)
            {
                HandleOccupiedUpdate();
            }
        }

        private void HandleOccupiedUpdate()
        {
            // Check if Thanh_House is nearby and we have NPCs to deliver
            bool nearHouse = IsNearThanhHouse();
            bool hasNPCs = rescuedNPCsOnBoard.Count > 0;

            // Build prompt string
            string prompt = $"[{activePlayer.keyInteract}] Xuống thuyền thúng";
            if (nearHouse && hasNPCs)
            {
                prompt = $"[{activePlayer.keyInteract}] Đưa {rescuedNPCsOnBoard.Count} người lên nóc nhà\n" +
                         $"[Space] Xuống thuyền thúng";
            }

            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt(prompt);
            }

            // Read inputs
            bool deliverPressed = false;
            bool exitPressed    = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                bool eKey     = Keyboard.current.eKey.wasPressedThisFrame;
                bool spaceKey = Keyboard.current.spaceKey.wasPressedThisFrame;

                if (nearHouse && hasNPCs && eKey)
                    deliverPressed = true;
                else if (spaceKey || (!hasNPCs && eKey))
                    exitPressed = true;
            }
#else
            bool eDown     = Input.GetKeyDown(activePlayer.keyInteract) || Input.GetKeyDown(KeyCode.E);
            bool spaceDown = Input.GetKeyDown(KeyCode.Space);

            if (nearHouse && hasNPCs && eDown)
                deliverPressed = true;
            else if (spaceDown || (!hasNPCs && eDown))
                exitPressed = true;
#endif

            if (deliverPressed)
            {
                DeliverNPCsToRoof();
                return;
            }

            if (exitPressed)
            {
                ExitBoat();
                return;
            }

            HandleRowingInput();
        }

        private bool IsNearThanhHouse()
        {
            GameObject house = GameObject.Find("Thanh_House");
            if (house == null) return false;
            return Vector3.Distance(transform.position, house.transform.position) <= deliveryRange;
        }

        // ─── NPC Boarding ────────────────────────────────────────────────────

        /// <summary>
        /// Add a rescued NPC onto the boat. Called by NPCProximityOptionsUI during RescuingNPCs stage.
        /// </summary>
        public bool AddNPCToBoat(NPCCharacter npc)
        {
            if (npc == null) return false;
            if (rescuedNPCsOnBoard.Contains(npc)) return false;
            if (rescuedNPCsOnBoard.Count >= npcSeatOffsets.Length) return false;

            int slot = rescuedNPCsOnBoard.Count;
            rescuedNPCsOnBoard.Add(npc);

            // Disable NPC physics so they ride the boat
            var npcRb = npc.GetComponent<Rigidbody>();
            if (npcRb != null)
            {
                npcRb.isKinematic = true;
                npcRb.useGravity  = false;
                npcRb.linearVelocity = Vector3.zero;
            }

            // Disable NPC colliders (avoid physics conflicts)
            foreach (var col in npc.GetComponentsInChildren<Collider>())
                if (col != null) col.isTrigger = true;

            // Parent NPC to boat and seat
            npc.transform.SetParent(this.transform);
            npc.transform.localPosition = npcSeatOffsets[slot];
            npc.transform.localRotation = Quaternion.identity;

            Debug.Log($"[BOAT] {npc.NPCName} đã lên thuyền (slot {slot}).");
            return true;
        }

        /// <summary>
        /// Remove all NPCs from the boat and deliver them to Thanh_House roof.
        /// Also teleports the player to the roof and starts RoofSurvivalSharing stage.
        /// </summary>
        public void DeliverNPCsToRoof()
        {
            if (rescuedNPCsOnBoard.Count == 0) return;

            var tm = TutorialManager.Instance;

            // Unparent and teleport each NPC to roof
            for (int i = rescuedNPCsOnBoard.Count - 1; i >= 0; i--)
            {
                var npc = rescuedNPCsOnBoard[i];
                if (npc == null) continue;

                npc.transform.SetParent(null);
                tm?.OnNPCDeliveredToRoof(npc.characterType);
            }
            rescuedNPCsOnBoard.Clear();

            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
            Debug.Log("[BOAT] Đã đưa tất cả NPC lên nóc nhà.");
        }

        /// <summary>Unparent all NPCs (used during reset, no delivery).</summary>
        public void ReleaseAllNPCs()
        {
            foreach (var npc in rescuedNPCsOnBoard)
            {
                if (npc == null) continue;
                npc.transform.SetParent(null);

                var npcRb = npc.GetComponent<Rigidbody>();
                if (npcRb != null)
                {
                    npcRb.isKinematic = false;
                    npcRb.useGravity  = true;
                }
            }
            rescuedNPCsOnBoard.Clear();
        }

        // ─── Rowing ──────────────────────────────────────────────────────────

        private void HandleRowingInput()
        {
            float moveVal = 0f;
            float turnVal = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    moveVal =  1f;
                else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveVal = -1f;

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) turnVal =  1f;
                else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) turnVal = -1f;
            }
#else
            moveVal = Input.GetAxis("Vertical");
            turnVal = Input.GetAxis("Horizontal");
#endif

            Vector3 moveDir = transform.forward * moveVal * moveSpeed;
            rb.linearVelocity = new Vector3(moveDir.x, 0f, moveDir.z);

            if (Mathf.Abs(moveVal) > 0.05f || Mathf.Abs(turnVal) > 0.05f)
            {
                float rotSign = moveVal >= 0f ? 1f : -1f;
                transform.Rotate(0f, turnVal * rotationSpeed * rotSign * Time.deltaTime, 0f);
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
            }
        }

        // ─── Enter / Exit ─────────────────────────────────────────────────────

        public void Interact(PlayerController player)
        {
            if (isOccupied) return;

            // Kiểm tra xem đã đến giai đoạn cứu hộ (Phase 3) chưa
            if (TutorialManager.Instance != null && TutorialManager.Instance.currentStage < TutorialManager.TutorialStage.RescuingNPCs)
            {
                SownInStone.UI.SurvivalUIManager.Instance?.ShowHUDToast("⚠️ Thuyền thúng hiện tại chưa sử dụng được. Hãy tập trung chuẩn bị trước bão lũ!");
                return;
            }

            EnterBoat(player);
        }

        private float savedPlayerY = 0.8f;

        private void EnterBoat(PlayerController player)
        {
            activePlayer  = player;
            isOccupied    = true;
            savedPlayerY  = player.transform.position.y;

            var playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.isKinematic   = true;
                playerRb.linearVelocity = Vector3.zero;
            }

            var playerCol = player.GetComponent<Collider>();
            if (playerCol != null) playerCol.enabled = false;

            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Đồng bộ hóa vật lý để giải phóng cache của CharacterController
            Physics.SyncTransforms();

            player.transform.SetParent(this.transform);
            player.transform.localPosition = playerSeatOffset;
            player.transform.localRotation = Quaternion.identity;

            player.enabled = false;

            // --- DEBUG LOG FOR CAMERA AND SCALES ---
            Debug.Log($"[BOAT-DEBUG] Player entered boat.");
            Debug.Log($"[BOAT-DEBUG] Boat: pos={transform.position}, rot={transform.rotation.eulerAngles}, scale={transform.localScale}");
            Debug.Log($"[BOAT-DEBUG] Player: parent={player.transform.parent.name}, worldPos={player.transform.position}, localPos={player.transform.localPosition}, lossyScale={player.transform.lossyScale}");
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Debug.Log($"[BOAT-DEBUG] Main Camera: parent={(mainCam.transform.parent != null ? mainCam.transform.parent.name : "none")}, worldPos={mainCam.transform.position}, rot={mainCam.transform.rotation.eulerAngles}");
                var follow = mainCam.GetComponent<CameraFollow3D>();
                if (follow != null)
                {
                    Debug.Log($"[BOAT-DEBUG] CameraFollow3D: mode={follow.CurrentMode}");
                }
            }
            // ----------------------------------------

            var playerAnim = player.GetComponentInChildren<Animator>() ?? player.GetComponent<Animator>();
            if (playerAnim != null)
            {
                playerAnim.SetFloat("Speed",      0f);
                playerAnim.SetFloat("Horizontal", 0f);
                playerAnim.SetFloat("Vertical",   0f);
            }

            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
            Debug.Log("[BOAT] Player lên thuyền thúng.");
        }

        private void ExitBoat()
        {
            if (activePlayer == null) return;

            activePlayer.enabled = true;

            var playerCol = activePlayer.GetComponent<Collider>();
            if (playerCol != null) playerCol.enabled = true;

            var cc = activePlayer.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;

            activePlayer.transform.SetParent(null);

            Vector3 exitPos = transform.position + transform.TransformDirection(exitOffset);
            exitPos.y = savedPlayerY;
            activePlayer.transform.position = exitPos;
            activePlayer.transform.rotation = transform.rotation;

            // Đồng bộ hóa vật lý để CharacterController nhận diện vị trí mới ngay lập tức
            Physics.SyncTransforms();

            var playerRb = activePlayer.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.isKinematic    = false;
                playerRb.position       = exitPos;
                playerRb.rotation       = transform.rotation;
                playerRb.linearVelocity = Vector3.zero;
            }

            if (SownInStone.UI.SurvivalUIManager.Instance != null)
                SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt("");

            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
            Debug.Log("[BOAT] Player xuống thuyền thúng.");

            isOccupied   = false;
            activePlayer = null;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
