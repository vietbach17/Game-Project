using UnityEngine;
using SownInStone.Core;
using SownInStone.Weather;

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
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.mass = 150f;
            // Freeze X and Z rotations to keep boat flat, and freeze Y velocity/rotation manually if needed
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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
            }

            if (isOccupied && activePlayer != null)
            {
                // Update interaction prompt for exiting
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.SetInteractionPrompt($"[{activePlayer.keyInteract}] Xuống thuyền thúng");
                }

                // Check for Exit input
                if (Input.GetKeyDown(activePlayer.keyInteract) || Input.GetKeyDown(KeyCode.E))
                {
                    ExitBoat();
                    return;
                }

                HandleRowingInput();
            }
        }

        private void HandleRowingInput()
        {
            // Get inputs from vertical/horizontal axes or player controller keys
            float moveInput = Input.GetAxis("Vertical");
            float turnInput = Input.GetAxis("Horizontal");

            // Apply movement on XZ plane
            Vector3 moveDir = transform.forward * moveInput * moveSpeed;
            rb.velocity = new Vector3(moveDir.x, rb.velocity.y, moveDir.z);

            // Apply rotation around Y axis
            if (Mathf.Abs(moveInput) > 0.05f || Mathf.Abs(turnInput) > 0.05f)
            {
                // Steer boat
                float rotationSign = moveInput >= 0f ? 1f : -1f;
                transform.Rotate(0f, turnInput * rotationSpeed * rotationSign * Time.deltaTime, 0f);
            }
            else
            {
                // Stop boat velocity when no input
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
        }

        public void Interact(PlayerController player)
        {
            if (isOccupied) return;

            EnterBoat(player);
        }

        private void EnterBoat(PlayerController player)
        {
            activePlayer = player;
            isOccupied = true;

            // 1. Disable player physics and control
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.isKinematic = true;
                playerRb.velocity = Vector3.zero;
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
                playerAnim.SetFloat("speed", 0f);
                playerAnim.SetBool("isWalking", false);
            }

            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
            Debug.Log("[BOAT] Player entered the coracle.");
        }

        private void ExitBoat()
        {
            if (activePlayer == null) return;

            // 1. Enable player components first
            activePlayer.enabled = true;

            Collider playerCol = activePlayer.GetComponent<Collider>();
            if (playerCol != null)
            {
                playerCol.enabled = true;
            }

            Rigidbody playerRb = activePlayer.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
            }

            // 2. Unparent player
            activePlayer.transform.SetParent(null);

            // 3. Position player next to the boat safely
            Vector3 exitPos = transform.position + transform.TransformDirection(exitOffset);
            
            // Adjust height to stay on ground or float level
            float groundY = 0f;
            if (WeatherManager.Instance != null && WeatherManager.Instance.FloodLevel > 0.01f)
            {
                groundY = -0.05f + WeatherManager.Instance.FloodLevel;
            }
            exitPos.y = groundY;

            activePlayer.transform.position = exitPos;
            activePlayer.transform.rotation = transform.rotation;

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
