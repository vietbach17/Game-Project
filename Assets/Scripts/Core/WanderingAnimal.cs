using UnityEngine;

namespace SownInStone.Core
{
    /// <summary>
    /// Simple AI script for animals (chickens, dogs) to wander around their starting position.
    /// Supports safe parameter checks in Animator for movement state.
    /// Uses a downward Raycast to snap the animal to the terrain surface each frame.
    /// </summary>
    public class WanderingAnimal : MonoBehaviour
    {
        [Header("Wander Settings")]
        [Tooltip("Maximum distance the animal can wander from its starting position. Set to 0 to keep static.")]
        public float wanderRadius = 3f;
        public float moveSpeed = 1f;
        public float rotationSpeed = 5f;

        [Header("Timing")]
        public float minIdleTime = 2f;
        public float maxIdleTime = 6f;

        [Header("Ground Snap")]
        [Tooltip("Layer mask for terrain / ground. Leave default (Everything) if unsure.")]
        public LayerMask groundLayerMask = ~0;  // Everything by default
        [Tooltip("How high above the animal to start the ground ray.")]
        public float raycastOriginHeight = 2f;
        [Tooltip("How fast to lerp the animal to ground Y position.")]
        public float groundSnapSpeed = 10f;

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private Animator animator;
        private float idleTimer;
        private bool isMoving;

        // Animator parameter names
        private const string IsWalkingParam = "isWalking";
        private const string SpeedParam     = "speed";
        private bool hasIsWalking;
        private bool hasSpeed;

        private void Start()
        {
            // Override settings dynamically depending on the animal type (dog vs chicken)
            if (gameObject.name.ToLower().Contains("dog"))
            {
                wanderRadius = 1.2f;
                moveSpeed = 0.4f;
                minIdleTime = 4f;
                maxIdleTime = 10f;
            }
            else if (gameObject.name.ToLower().Contains("chicken"))
            {
                wanderRadius = 1.5f;
                moveSpeed = 0.3f;
                minIdleTime = 3f;
                maxIdleTime = 8f;
            }

            // Snap to ground immediately on start
            SnapToGround(snap: true);
            startPosition = transform.position;

            animator = GetComponentInChildren<Animator>();
            if (animator == null) animator = GetComponent<Animator>();

            // Cache animator parameter existence
            if (animator != null)
            {
                hasIsWalking = HasParameter(IsWalkingParam, animator);
                hasSpeed     = HasParameter(SpeedParam, animator);
            }

            if (wanderRadius > 0.05f)
            {
                GetNewTarget();
            }
            else
            {
                isMoving  = false;
                idleTimer = Random.Range(minIdleTime, maxIdleTime);
            }
        }

        private void Update()
        {
            // Ground snap every frame (smooth)
            SnapToGround(snap: false);

            if (wanderRadius <= 0.05f)
            {
                UpdateAnimator(0f);
                // Đảm bảo con vật luôn đứng thẳng đứng trên mặt đất (X = 0, Z = 0)
                Vector3 r = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(0f, r.y, 0f);
                return;
            }

            if (isMoving)
            {
                MoveTowardsTarget();
            }
            else
            {
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f)
                {
                    GetNewTarget();
                }
            }

            // Đảm bảo con vật luôn đứng thẳng đứng trên mặt đất (X = 0, Z = 0)
            Vector3 rot = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rot.y, 0f);
        }

        private void SnapToGround(bool snap)
        {
            // Tạm thời vô hiệu hóa tất cả colliders của con vật để tránh Raycast tự đụng trúng chính nó
            Collider[] myCols = GetComponentsInChildren<Collider>();
            foreach (var col in myCols) if (col != null) col.enabled = false;

            Vector3 origin = transform.position + Vector3.up * raycastOriginHeight;
            // Tăng chiều dài raycast lên 45m để chắc chắn đụng trúng đất nếu bắt đầu ở trên cao
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastOriginHeight + 45f, groundLayerMask))
            {
                float targetY = hit.point.y;
                Vector3 pos   = transform.position;
                if (snap)
                {
                    pos.y = targetY;
                }
                else
                {
                    pos.y = Mathf.Lerp(pos.y, targetY, groundSnapSpeed * Time.deltaTime);
                }
                transform.position = pos;
            }

            // Kích hoạt lại các colliders
            foreach (var col in myCols) if (col != null) col.enabled = true;
        }

        private void MoveTowardsTarget()
        {
            Vector3 currentPos    = transform.position;
            // Keep current Y (ground-snapped), move on XZ only
            Vector3 targetPosFlat = new Vector3(targetPosition.x, currentPos.y, targetPosition.z);

            // Calculate distance flat on XZ plane
            float dist = Vector3.Distance(currentPos, targetPosFlat);
            if (dist < 0.1f)
            {
                isMoving  = false;
                idleTimer = Random.Range(minIdleTime, maxIdleTime);
                UpdateAnimator(0f);
                return;
            }

            // Move towards target
            transform.position = Vector3.MoveTowards(currentPos, targetPosFlat, moveSpeed * Time.deltaTime);

            // Rotate towards target
            Vector3 direction = (targetPosFlat - currentPos).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation   = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            UpdateAnimator(moveSpeed);
        }

        private void GetNewTarget()
        {
            Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
            targetPosition      = new Vector3(startPosition.x + randomPoint.x, startPosition.y, startPosition.z + randomPoint.y);
            isMoving            = true;
        }

        private void UpdateAnimator(float speedValue)
        {
            if (animator == null) return;

            if (hasIsWalking)
            {
                animator.SetBool(IsWalkingParam, speedValue > 0.05f);
            }
            if (hasSpeed)
            {
                animator.SetFloat(SpeedParam, speedValue);
            }
        }

        private bool HasParameter(string paramName, Animator anim)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == paramName) return true;
            }
            return false;
        }
    }
}
