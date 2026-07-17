using UnityEngine;
using SownInStone.Core;
using SownInStone.Weather;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SownInStone.Interactions
{
    /// <summary>
    /// Thuyền thúng (Coracle) interaction and movement controller.
    /// Allows the player to enter, steer the boat during flood, and exit safely.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BoxCollider))]
    public class Coracle : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 4f;
        public float rotationSpeed = 60f;

        [Header("Seat Position")]
        [Tooltip("Offset position where the player will stand/sit inside the boat.")]
        public Vector3 playerSeatOffset = new Vector3(0f, 0.2f, 0f);

        private Rigidbody rb;
        private BoxCollider boxCollider;
        private PlayerController activePlayer;
        private bool isOccupied;
        private Vector3 exitOffset = new Vector3(0f, 0f, -1.8f); // Offset to place player when exiting

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            boxCollider = GetComponent<BoxCollider>();
            
            // Set up rigidbody for arcade boat physics
            rb.useGravity = false; // Disable gravity to prevent drifting
            rb.isKinematic = false;
            rb.mass = 150f;
            // Freeze X, Y, Z rotations and Y position to lock the vertical/tilt movement
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
        }

        private void Update()
        {
            // Snap boat Y position to water plane if flood is active, otherwise stay flat on ground (Y = 0)
            float targetY = 0f;
            if (WeatherManager.Instance != null && WeatherManager.Instance.FloodLevel > 0.01f)
            {
                // Align with the 3D water plane Y calculation: -0.05f + FloodLevel
                targetY = -0.05f + WeatherManager.Instance.FloodLevel;
            }

            Vector3 currentPos = transform.position;
            // If flood level is high, float the boat
            if (currentPos.y < targetY - 0.05f || currentPos.y > targetY + 0.05f)
            {
                currentPos.y = Mathf.Lerp(currentPos.y, targetY, Time.deltaTime * 5f);
                transform.position = currentPos;
                rb.position = currentPos; // Sync Rigidbody position
            }

            if (isOccupied && activePlayer != null)
            {
                // Update interaction prompt for exiting
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt($"[{activePlayer.keyInteract}] Xuống thuyền thúng");
                }

                // Check for Exit input
                bool exitPressed = false;
#if ENABLE_INPUT_SYSTEM
                if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
                {
                    exitPressed = true;
                }
#else
                if (Input.GetKeyDown(activePlayer.keyInteract) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                {
                    exitPressed = true;
                }
#endif

                if (exitPressed)
                {
                    ExitBoat();
                    return;
                }

                HandleRowingInput();
            }
        }

        private void HandleRowingInput()
        {
            float moveVal = 0f;
            float turnVal = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveVal = 1f;
                else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveVal = -1f;

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) turnVal = 1f;
                else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) turnVal = -1f;
            }
#else
            moveVal = Input.GetAxis("Vertical");
            turnVal = Input.GetAxis("Horizontal");
#endif

            // Apply movement on XZ plane
            Vector3 moveDir = transform.forward * moveVal * moveSpeed;
            rb.linearVelocity = new Vector3(moveDir.x, 0f, moveDir.z); // Freeze Y velocity to 0

            // Apply rotation around Y axis
            if (Mathf.Abs(moveVal) > 0.05f || Mathf.Abs(turnVal) > 0.05f)
            {
                // Steer boat
                float rotationSign = moveVal >= 0f ? 1f : -1f;
                transform.Rotate(0f, turnVal * rotationSpeed * rotationSign * Time.deltaTime, 0f);
            }
            else
            {
                // Stop boat velocity when no input
                rb.linearVelocity = new Vector3(0f, 0f, 0f); // Freeze Y velocity to 0
            }
        }

        public void Interact(PlayerController player)
        {
            if (isOccupied) return;

            EnterBoat(player);
        }

        private float savedPlayerY = 0.8f;

        private void EnterBoat(PlayerController player)
        {
            activePlayer = player;
            isOccupied = true;
            savedPlayerY = player.transform.position.y;

            // 1. Disable player physics and control
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.isKinematic = true;
                playerRb.linearVelocity = Vector3.zero;
            }

            Collider playerCol = player.GetComponent<Collider>();
            if (playerCol != null)
            {
                playerCol.enabled = false;
            }

            // 2. Parent player to boat and snap to seat offset
            player.transform.SetParent(this.transform);
            player.transform.localPosition = playerSeatOffset;
            player.transform.localRotation = Quaternion.identity;

            // 3. Disable player movement component (so it doesn't process WASD or gravity)
            player.enabled = false;

            // 4. Play idle anim on player if animator exists
            Animator playerAnim = player.GetComponentInChildren<Animator>();
            if (playerAnim == null) playerAnim = player.GetComponent<Animator>();
            if (playerAnim != null)
            {
                // Play sitting or idle state
                playerAnim.SetFloat("Speed", 0f);
                playerAnim.SetFloat("Horizontal", 0f);
                playerAnim.SetFloat("Vertical", 0f);
                playerAnim.SetBool("isPaddling", true);
            }

            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
            Debug.Log("[BOAT] Player entered the coracle.");
        }

        private void ExitBoat()
        {
            if (activePlayer == null) return;

            // Restore player paddling state
            Animator playerAnim = activePlayer.GetComponentInChildren<Animator>();
            if (playerAnim == null) playerAnim = activePlayer.GetComponent<Animator>();
            if (playerAnim != null)
            {
                playerAnim.SetBool("isPaddling", false);
            }

            // 1. Enable player controller component
            activePlayer.enabled = true;

            Collider playerCol = activePlayer.GetComponent<Collider>();
            if (playerCol != null)
            {
                playerCol.enabled = true;
            }

            // 2. Unparent player first (while still kinematic)
            activePlayer.transform.SetParent(null);

            // 3. Position player next to the boat safely
            Vector3 exitPos = transform.position + transform.TransformDirection(exitOffset);
            
            // Restore saved Y coordinate so they don't sink or float
            exitPos.y = savedPlayerY;

            activePlayer.transform.position = exitPos;
            activePlayer.transform.rotation = transform.rotation;

            // 4. Enable Rigidbody physics and apply position/rotation directly to the physics engine
            Rigidbody playerRb = activePlayer.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
                playerRb.position = exitPos;
                playerRb.rotation = transform.rotation;
                playerRb.linearVelocity = Vector3.zero;
            }

            // Clear interaction prompt
            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt("");
            }

            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
            Debug.Log("[BOAT] Player exited the coracle.");

            isOccupied = false;
            activePlayer = null;
        }
    }
}
