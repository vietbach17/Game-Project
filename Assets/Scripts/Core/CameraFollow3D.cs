using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SownInStone.Core
{
    /// <summary>
    /// Roblox-like third-person camera that follows the player, supports right mouse rotation,
    /// scroll wheel zoom, and simple collision safety.
    /// </summary>
    public class CameraFollow3D : MonoBehaviour
    {
        [Header("--- TARGETS ---")]
        [Tooltip("The target the camera will follow.")]
        [SerializeField] private Transform target;

        [Header("--- ZOOM SETTINGS ---")]
        [Tooltip("Default distance from target.")]
        [SerializeField] private float distance = 1.9f;
        [SerializeField] private float minDistance = 1.3f;
        [SerializeField] private float maxDistance = 3.5f;
        [SerializeField] private float zoomSpeed = 2f;

        [Header("--- HEIGHT & ANGLE ---")]
        [Tooltip("Vertical height of the orbit pivot relative to target position.")]
        [SerializeField] private float pivotHeight = 1.35f;
        [SerializeField] private float minPitch = 10f;
        [SerializeField] private float maxPitch = 55f;
        [SerializeField] private float defaultPitch = 10f;

        [Header("--- ROTATION SENSITIVITY ---")]
        [SerializeField] private float yawSensitivity = 2f;
        [SerializeField] private float pitchSensitivity = 2f;

        [Header("--- SMOOTHING ---")]
        [Tooltip("Time to smooth camera movement.")]
        [SerializeField] private float smoothTime = 0.02f;

        [Header("--- COLLISION SAFETY ---")]
        [SerializeField] private LayerMask collisionLayers = ~0;
        [SerializeField] private float cameraRadius = 0.2f;

        // Current rotation state
        private float currentYaw = 0f;
        private float currentPitch = 10f;
        
        // Target values for smoothing zoom
        private float targetDistance;
        private Vector3 currentVelocity;
        private Camera targetCamera;

        private void Start()
        {
            ResetCameraToTargetImmediate();
        }

        private void OnEnable()
        {
            ResetCameraToTargetImmediate();
        }

        public void ResetCameraToTargetImmediate()
        {
            if (target == null)
            {
                if (PlayerController.Instance != null)
                {
                    target = PlayerController.Instance.transform;
                }
                else
                {
                    PlayerController pc = FindAnyObjectByType<PlayerController>();
                    if (pc != null)
                    {
                        target = pc.transform;
                    }
                }
            }

            targetCamera = GetComponent<Camera>();
            if (targetCamera != null)
            {
                targetCamera.orthographic = false;
                targetCamera.fieldOfView = 60f;
            }

            // Snap yaw to target's current rotation if available, otherwise default to 0
            if (target != null)
            {
                currentYaw = target.eulerAngles.y;
            }
            else
            {
                currentYaw = 0f;
            }
            currentPitch = defaultPitch;
            targetDistance = distance;

            if (target != null)
            {
                Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
                Vector3 pivot = target.position + Vector3.up * pivotHeight;
                Vector3 position = pivot - (rotation * Vector3.forward * distance);
                transform.position = position;
                transform.rotation = Quaternion.LookRotation(pivot - position, Vector3.up);
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (PlayerController.Instance != null)
                {
                    target = PlayerController.Instance.transform;
                }
                else
                {
                    PlayerController pc = FindAnyObjectByType<PlayerController>();
                    if (pc != null)
                    {
                        target = pc.transform;
                    }
                }
                if (target == null) return;
            }

            // 1. Mouse wheel zoom
            float scroll = 0f;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                scroll = Mouse.current.scroll.ReadValue().y * 0.01f;
            }
#else
            scroll = Input.GetAxis("Mouse ScrollWheel") * 10f;
#endif
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetDistance = Mathf.Clamp(targetDistance - scroll * zoomSpeed, minDistance, maxDistance);
            }
            distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * 10f);

            // 2. Right mouse camera rotation
            float mouseX = 0f;
            float mouseY = 0f;
            bool isRMBHeld = false;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                isRMBHeld = Mouse.current.rightButton.isPressed;
                if (isRMBHeld)
                {
                    Vector2 delta = Mouse.current.delta.ReadValue();
                    mouseX = delta.x * 0.1f;
                    mouseY = delta.y * 0.1f;
                }
            }
#else
            isRMBHeld = Input.GetMouseButton(1);
            if (isRMBHeld)
            {
                mouseX = Input.GetAxis("Mouse X");
                mouseY = Input.GetAxis("Mouse Y");
            }
#endif

            if (isRMBHeld)
            {
                currentYaw += mouseX * yawSensitivity;
                currentPitch -= mouseY * pitchSensitivity;
                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            }

            // 3. Calculate desired camera rotation and position
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 pivot = target.position + Vector3.up * pivotHeight;
            Vector3 desiredDirection = rotation * Vector3.back;

            // 4. Collision Safety (SphereCast from pivot to desired camera position)
            float checkDistance = distance;
            RaycastHit[] hits = Physics.SphereCastAll(pivot, cameraRadius, desiredDirection, distance, collisionLayers, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
            foreach (var hitInfo in hits)
            {
                if (hitInfo.collider.transform == target || hitInfo.collider.transform.IsChildOf(target))
                    continue;
                
                checkDistance = Mathf.Clamp(hitInfo.distance, minDistance, distance);
                break;
            }
            Vector3 finalTargetPos = pivot + desiredDirection * checkDistance;

            // 5. Smooth camera movement
            transform.position = Vector3.SmoothDamp(transform.position, finalTargetPos, ref currentVelocity, smoothTime);
            
            // 6. Always look at pivot
            transform.rotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);

            // 7. Temporary runtime viewport centering check
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }
            if (targetCamera != null)
            {
                Vector3 vp = targetCamera.WorldToViewportPoint(target.position + Vector3.up * 1.0f);
                if (vp.x < 0.47f || vp.x > 0.53f)
                {
                    Debug.LogWarning($"[CameraFollow3D] Player viewport X is offset: {vp.x}");
                }
            }
        }
    }
}


