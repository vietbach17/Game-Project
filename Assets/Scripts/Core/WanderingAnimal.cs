using UnityEngine;

namespace SownInStone.Core
{
    /// <summary>
    /// Simple AI script for animals (chickens, dogs) to wander around their starting position.
    /// Supports safe parameter checks in Animator for movement state.
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

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private Animator animator;
        private float idleTimer;
        private bool isMoving;

        // Animator parameter names
        private const string IsWalkingParam = "isWalking";
        private const string SpeedParam = "speed";
        private bool hasIsWalking;
        private bool hasSpeed;

        private void Start()
        {
            startPosition = transform.position;
            animator = GetComponentInChildren<Animator>();
            if (animator == null) animator = GetComponent<Animator>();

            // Cache animator parameter existence
            if (animator != null)
            {
                hasIsWalking = HasParameter(IsWalkingParam, animator);
                hasSpeed = HasParameter(SpeedParam, animator);
            }

            if (wanderRadius > 0.05f)
            {
                GetNewTarget();
            }
            else
            {
                isMoving = false;
                idleTimer = Random.Range(minIdleTime, maxIdleTime);
            }
        }

        private void Update()
        {
            if (wanderRadius <= 0.05f)
            {
                UpdateAnimator(0f);
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
        }

        private void MoveTowardsTarget()
        {
            Vector3 currentPos = transform.position;
            Vector3 targetPosFlat = new Vector3(targetPosition.x, currentPos.y, targetPosition.z);
            
            // Calculate distance flat on XZ plane
            float dist = Vector3.Distance(currentPos, targetPosFlat);
            if (dist < 0.1f)
            {
                isMoving = false;
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
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            UpdateAnimator(moveSpeed);
        }

        private void GetNewTarget()
        {
            Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
            targetPosition = new Vector3(startPosition.x + randomPoint.x, startPosition.y, startPosition.z + randomPoint.y);
            isMoving = true;
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
