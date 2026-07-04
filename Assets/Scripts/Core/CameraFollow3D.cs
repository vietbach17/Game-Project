using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SownInStone.Core
{
    /// <summary>
    /// Camera hỗ trợ 3 góc nhìn:
    ///   1. ThirdPerson  — Roblox-style follow cam.
    ///   2. Fixed        — Camera cố định nhìn xuống theo góc isometric.
    ///   3. FirstPerson  — Camera gắn vào đầu nhân vật, nhìn từ mắt Thành (mặc định).
    /// Chuyển đổi qua menu Cài đặt (Settings).
    /// </summary>
    public class CameraFollow3D : MonoBehaviour
    {
        // ─── Singleton ────────────────────────────────────────────────────────
        public static CameraFollow3D Instance { get; private set; }

        // ─── Camera Mode ──────────────────────────────────────────────────────
        public enum CameraMode { ThirdPerson = 0, Fixed = 1, FirstPerson = 2 }
        private CameraMode currentMode = CameraMode.FirstPerson;
        public CameraMode CurrentMode => currentMode;

        [Header("--- TARGETS ---")]
        [Tooltip("The target the camera will follow.")]
        [SerializeField] private Transform target;

        // ════════════════════════════════════════════════════════════════════
        //  THIRD PERSON SETTINGS
        // ════════════════════════════════════════════════════════════════════
        [Header("--- [ThirdPerson] ZOOM ---")]
        [SerializeField] private float distance     = 1.9f;
        [SerializeField] private float minDistance  = 1.3f;
        [SerializeField] private float maxDistance  = 3.5f;
        [SerializeField] private float zoomSpeed    = 2f;

        [Header("--- [ThirdPerson] HEIGHT & ANGLE ---")]
        [SerializeField] private float pivotHeight  = 1.35f;
        [SerializeField] private float minPitch     = 10f;
        [SerializeField] private float maxPitch     = 55f;
        [SerializeField] private float defaultPitch = 10f;

        [Header("--- [ThirdPerson] ROTATION SENSITIVITY ---")]
        [SerializeField] private float yawSensitivity   = 2f;
        [SerializeField] private float pitchSensitivity = 2f;

        [Header("--- [ThirdPerson] SMOOTHING ---")]
        [SerializeField] private float smoothTime = 0.02f;

        [Header("--- [ThirdPerson] COLLISION SAFETY ---")]
        [SerializeField] private LayerMask collisionLayers = ~0;
        [SerializeField] private float cameraRadius = 0.2f;

        // ════════════════════════════════════════════════════════════════════
        //  FIXED CAMERA SETTINGS
        // ════════════════════════════════════════════════════════════════════
        [Header("--- [Fixed] ISOMETRIC SETTINGS ---")]
        [Tooltip("Chiều cao camera cố định so với mặt đất.")]
        [SerializeField] private float fixedHeight      = 12f;
        [Tooltip("Độ lùi ra sau của camera cố định (trục Z).")]
        [SerializeField] private float fixedZOffset     = -8f;
        [Tooltip("Góc nghiêng xuống (Pitch) của camera cố định (°).")]
        [SerializeField] private float fixedPitch       = 50f;
        [Tooltip("Tốc độ camera cố định bám theo player.")]
        [SerializeField] private float fixedFollowSpeed = 4f;

        // ════════════════════════════════════════════════════════════════════
        //  FIRST PERSON SETTINGS
        // ════════════════════════════════════════════════════════════════════
        [Header("--- [FirstPerson] EYE SETTINGS ---")]
        [Tooltip("Chiều cao mắt nhân vật (so với gốc transform).")]
        [SerializeField] private float eyeHeight        = 1.45f;
        [Tooltip("FOV khi ở góc nhìn thứ nhất.")]
        [SerializeField] private float fpsFOV           = 75f;
        [Tooltip("Độ nhạy chuột ngang (Yaw) FPS.")]
        [SerializeField] private float fpsYawSensitivity   = 2f;
        [Tooltip("Độ nhạy chuột dọc (Pitch) FPS.")]
        [SerializeField] private float fpsPitchSensitivity = 2f;
        [Tooltip("Giới hạn góc nhìn lên/xuống FPS (°).")]
        [SerializeField] private float fpsPitchClamp    = 80f;

        // ─── Runtime state ────────────────────────────────────────────────
        private float currentYaw    = 0f;
        private float currentPitch  = 10f;
        private float targetDistance;
        private Vector3 currentVelocity;

        // FPS state
        private float fpsCurrentYaw   = 0f;
        private float fpsCurrentPitch = 0f;

        // Cache
        private Camera targetCamera;
        private Renderer[] characterRenderers;
        private bool renderersHidden = false;

        // Cursor state
        private bool isCursorLocked = false;
        private bool wasUIOpen = false;

        private bool ShouldReleaseCursor()
        {
            if (FrameworkMainMenuUI.Instance != null && FrameworkMainMenuUI.Instance.IsMenuOpen)
                return true;

            if (SownInStone.UI.EndingManager.Instance != null && SownInStone.UI.EndingManager.Instance.IsEndingShown)
                return true;

            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                if (SownInStone.UI.SurvivalUIManager.Instance.IsShopOpen || 
                    SownInStone.UI.SurvivalUIManager.Instance.IsDialogueActive || 
                    SownInStone.UI.SurvivalUIManager.Instance.IsChoiceActive ||
                    SownInStone.UI.SurvivalUIManager.Instance.IsInventoryOpen ||
                    SownInStone.UI.SurvivalUIManager.Instance.IsCommunityOpen ||
                    SownInStone.UI.SurvivalUIManager.Instance.IsWeatherDetailsOpen)
                    return true;
            }

            if (TutorialManager.Instance != null)
            {
                if (TutorialManager.Instance.IsShowing || 
                    TutorialManager.Instance.IsShowingFarmingSlides ||
                    TutorialManager.Instance.currentStage == TutorialManager.TutorialStage.ShowingFarmingSlides)
                    return true;
            }

            if (FrameworkDebugUI.Instance != null && FrameworkDebugUI.Instance.isUIVisible)
                return true;

            return false;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  LIFECYCLE
        // ─────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            // Luôn đặt mặc định là góc nhìn thứ 3 (ThirdPerson) khi mới bắt đầu chơi
            currentMode = CameraMode.ThirdPerson;
            SetCharacterRenderersVisible(true);
            ResetCameraToTargetImmediate();
        }

        private void OnEnable()
        {
            ResetCameraToTargetImmediate();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PUBLIC API
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>Gọi từ menu/UI khi mở để trả lại con trỏ chuột.</summary>
        public void ReleaseCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            isCursorLocked   = false;
        }

        /// <summary>Phím [P] — bật/tắt hiển thị con trỏ chuột trong lúc chơi.</summary>
        public void ToggleCursorVisibility()
        {
            if (isCursorLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible   = true;
                isCursorLocked   = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible   = false;
                isCursorLocked   = true;
            }
        }

        /// <summary>Áp dụng trạng thái cursor phù hợp với mode hiện tại.</summary>
        private void ApplyCursorState()
        {
            // Chỉ tự động khóa trỏ chuột ở chế độ góc nhìn thứ nhất (FirstPerson)
            bool shouldLock = (currentMode == CameraMode.FirstPerson);
            if (shouldLock)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible   = false;
                isCursorLocked   = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible   = true;
                isCursorLocked   = false;
            }
        }

        /// <summary>Chuyển góc nhìn camera và lưu vào PlayerPrefs.</summary>
        public void SetCameraMode(CameraMode mode)
        {
            // Restore renderers trước khi đổi mode
            SetCharacterRenderersVisible(true);

            currentMode = mode;
            PlayerPrefs.SetInt("CameraMode", (int)mode);
            PlayerPrefs.Save();

            ResetCameraToTargetImmediate();

            // Ẩn character nếu FPS
            if (mode == CameraMode.FirstPerson)
            {
                SetCharacterRenderersVisible(false);
            }

            ApplyCursorState();
        }

        /// <summary>Cycle qua 3 mode (dùng cho phím V).</summary>
        public void CycleCameraMode()
        {
            int next = ((int)currentMode + 1) % 3;
            SetCameraMode((CameraMode)next);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  RESET / INIT
        // ─────────────────────────────────────────────────────────────────────
        public void ResetCameraToTargetImmediate()
        {
            // Auto-find target
            if (target == null)
            {
                if (PlayerController.Instance != null)
                    target = PlayerController.Instance.transform;
                else
                {
                    PlayerController pc = FindAnyObjectByType<PlayerController>();
                    if (pc != null) target = pc.transform;
                }
            }

            targetCamera = GetComponent<Camera>();

            switch (currentMode)
            {
                case CameraMode.ThirdPerson:
                    InitThirdPerson();
                    break;
                case CameraMode.Fixed:
                    InitFixed();
                    break;
                case CameraMode.FirstPerson:
                    InitFirstPerson();
                    break;
            }
        }

        private void InitThirdPerson()
        {
            if (targetCamera != null)
            {
                targetCamera.orthographic = false;
                targetCamera.fieldOfView  = 60f;
            }
            if (target != null) currentYaw = target.eulerAngles.y;
            else currentYaw = 0f;
            currentPitch   = defaultPitch;
            targetDistance = distance;

            // Bắt đầu chế độ ThirdPerson với cursor tự do (giữ chuột phải mới xoay)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            isCursorLocked   = false;

            if (target != null)
            {
                Quaternion rot  = Quaternion.Euler(currentPitch, currentYaw, 0f);
                Vector3 pivot   = target.position + Vector3.up * pivotHeight;
                Vector3 pos     = pivot - (rot * Vector3.forward * distance);
                transform.position = pos;
                transform.rotation = Quaternion.LookRotation(pivot - pos, Vector3.up);
            }
        }

        private void InitFixed()
        {
            if (targetCamera != null)
            {
                targetCamera.orthographic = false;
                targetCamera.fieldOfView  = 60f;
            }
            if (target != null)
            {
                Vector3 desiredPos = new Vector3(
                    target.position.x,
                    target.position.y + fixedHeight,
                    target.position.z + fixedZOffset
                );
                transform.position = desiredPos;
                transform.rotation = Quaternion.Euler(fixedPitch, 0f, 0f);
            }

            // Fixed: hiển thị cursor bình thường
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            isCursorLocked   = false;
        }

        private void InitFirstPerson()
        {
            if (targetCamera != null)
            {
                targetCamera.orthographic = false;
                targetCamera.fieldOfView  = fpsFOV;
            }
            if (target != null)
            {
                fpsCurrentYaw   = target.eulerAngles.y;
                fpsCurrentPitch = 0f;
            }
            CacheCharacterRenderers();
            SetCharacterRenderersVisible(false);

            // Ẩn con trỏ chuột
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            isCursorLocked   = true;
        }


        // ─────────────────────────────────────────────────────────────────────
        //  UPDATE
        // ─────────────────────────────────────────────────────────────────────
        private void LateUpdate()
        {
            // Auto-find target if lost
            if (target == null)
            {
                if (PlayerController.Instance != null)
                    target = PlayerController.Instance.transform;
                else
                {
                    PlayerController pc = FindAnyObjectByType<PlayerController>();
                    if (pc != null) target = pc.transform;
                }
                if (target == null) return;
            }

            // Tự động giải phóng con trỏ chuột khi có các giao diện / hướng dẫn hiện lên
            bool isUIOpen = ShouldReleaseCursor();
            if (isUIOpen != wasUIOpen)
            {
                if (isUIOpen)
                {
                    ReleaseCursor();
                }
                else
                {
                    bool shouldLock = (currentMode == CameraMode.FirstPerson);
                    if (shouldLock)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        isCursorLocked = true;
                    }
                    else
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        isCursorLocked = false;
                    }
                }
                wasUIOpen = isUIOpen;
            }

            // Đảm bảo chắc chắn trạng thái không bị lỗi bởi Editor/Focus
            if (isUIOpen && Cursor.lockState != CursorLockMode.None)
            {
                ReleaseCursor();
            }



            // Phím [P] để bật/tắt con trỏ chuột
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            {
                ToggleCursorVisibility();
            }
#else
            if (Input.GetKeyDown(KeyCode.P))
            {
                ToggleCursorVisibility();
            }
#endif

            switch (currentMode)
            {
                case CameraMode.ThirdPerson: UpdateThirdPerson(); break;
                case CameraMode.Fixed:       UpdateFixed();       break;
                case CameraMode.FirstPerson: UpdateFirstPerson(); break;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  THIRD PERSON UPDATE
        // ─────────────────────────────────────────────────────────────────────
        private void UpdateThirdPerson()
        {
            // 1. Mouse wheel zoom (Tắt zoom khi đang mở Balo hoặc thao tác UI)
            float scroll = 0f;
            bool isUIOpen = (SownInStone.UI.SurvivalUIManager.Instance != null && SownInStone.UI.SurvivalUIManager.Instance.IsInventoryOpen) ||
                            (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject());

            if (!isUIOpen)
            if (!ShouldReleaseCursor())
            {
#if ENABLE_INPUT_SYSTEM
                if (Mouse.current != null)
                    scroll = Mouse.current.scroll.ReadValue().y * 0.01f;
#else
                scroll = Input.GetAxis("Mouse ScrollWheel") * 10f;
#endif
            }
            if (Mathf.Abs(scroll) > 0.01f)
                targetDistance = Mathf.Clamp(targetDistance - scroll * zoomSpeed, minDistance, maxDistance);
            distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * 10f);

            // 2. Rotation — chỉ xoay khi giữ chuột phải (RMB)
            float mouseX = 0f, mouseY = 0f;
            bool isRMBHeld = Input.GetMouseButton(1);
            
            if (!isUIOpen)
            {
                if (isRMBHeld)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    isCursorLocked = true;

#if ENABLE_INPUT_SYSTEM
                    if (Mouse.current != null)
                    {
                        Vector2 delta = Mouse.current.delta.ReadValue();
                        mouseX = delta.x * 0.1f;
                        mouseY = delta.y * 0.1f;
                    }
#else
                    mouseX = Input.GetAxis("Mouse X");
                    mouseY = Input.GetAxis("Mouse Y");
#endif
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    isCursorLocked = false;
                }
            }
            
            currentYaw   += mouseX * yawSensitivity;
            currentPitch -= mouseY * pitchSensitivity;
            currentPitch  = Mathf.Clamp(currentPitch, minPitch, maxPitch);

            // 3. Position & collision
            Quaternion rotation    = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 pivot          = target.position + Vector3.up * pivotHeight;
            Vector3 desiredDir     = rotation * Vector3.back;
            float   checkDistance  = distance;

            bool isInsideHouse = PlayerController.Instance != null && PlayerController.Instance.IsInsideHouse;
            if (isInsideHouse)
            {
                checkDistance = 1.2f; // Góc nhìn 3D cận cảnh ngang vai PUBG khi ở trong nhà
            }
            else
            {
                RaycastHit[] hits = Physics.SphereCastAll(pivot, cameraRadius, desiredDir, distance, collisionLayers, QueryTriggerInteraction.Ignore);
                System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
                foreach (var h in hits)
                {
                    if (h.collider.transform == target || h.collider.transform.IsChildOf(target)) continue;
                    
                    // Bỏ qua va chạm camera với Địa hình (Terrain), Ruộng đất (SoilCell/Ruong), Vũng bùn (MudPuddle), NPC, cây cối, giếng nước, nhà cửa và cửa hàng
                    if (h.collider.GetComponent<Terrain>() != null || 
                        h.collider.GetComponentInParent<SownInStone.Community.NPCCharacter>() != null ||
                        h.collider.GetComponent<SownInStone.Community.NPCCharacter>() != null ||
                        h.collider.name.Contains("Terrain") || 
                        h.collider.name.Contains("Soil") || 
                        h.collider.name.Contains("Ruong") || 
                        h.collider.name.Contains("Mud") ||
                        h.collider.name.Contains("NPC") ||
                        h.collider.name.Contains("Banana") ||
                        h.collider.name.Contains("Chuoi") ||
                        h.collider.name.Contains("Tree") ||
                        h.collider.name.Contains("Plant") ||
                        h.collider.name.Contains("Well") ||
                        h.collider.name.Contains("Gieng") ||
                        h.collider.name.Contains("House") ||
                        h.collider.name.Contains("Shop") ||
                        h.collider.name.Contains("Stall")) 
                        continue;

                    checkDistance = Mathf.Clamp(h.distance, minDistance, distance);
                    break;
                }
            }

            Vector3 finalPos = pivot + desiredDir * checkDistance;
            transform.position = Vector3.SmoothDamp(transform.position, finalPos, ref currentVelocity, smoothTime);
            transform.rotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);

            // FOV guard
            if (targetCamera == null) targetCamera = GetComponent<Camera>();
            if (targetCamera != null)
            {
                if (!Mathf.Approximately(targetCamera.fieldOfView, 60f)) targetCamera.fieldOfView = 60f;
                Vector3 vp = targetCamera.WorldToViewportPoint(target.position + Vector3.up * 1.0f);
                if (vp.x < 0.47f || vp.x > 0.53f)
                    Debug.LogWarning($"[CameraFollow3D] Player viewport X is offset: {vp.x}");
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  FIXED CAMERA UPDATE
        // ─────────────────────────────────────────────────────────────────────
        private void UpdateFixed()
        {
            // Camera bám player nhẹ nhàng nhưng không xoay, không zoom
            Vector3 desiredPos = new Vector3(
                target.position.x,
                target.position.y + fixedHeight,
                target.position.z + fixedZOffset
            );
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * fixedFollowSpeed);
            transform.rotation = Quaternion.Euler(fixedPitch, 0f, 0f);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  FIRST PERSON UPDATE
        // ─────────────────────────────────────────────────────────────────────
        private void UpdateFirstPerson()
        {
            // Đặt camera vào vị trí mắt nhân vật
            Vector3 eyePos = target.position + Vector3.up * eyeHeight;
            transform.position = eyePos;

            // Xoay bằng chuột (luôn luôn, không cần giữ RMB)
            float mouseX = 0f, mouseY = 0f;
            if (!ShouldReleaseCursor())
            {
#if ENABLE_INPUT_SYSTEM
                if (Mouse.current != null)
                {
                    Vector2 delta = Mouse.current.delta.ReadValue();
                    mouseX = delta.x * 0.1f;
                    mouseY = delta.y * 0.1f;
                }
#else
                mouseX = Input.GetAxis("Mouse X");
                mouseY = Input.GetAxis("Mouse Y");
#endif
            }
            fpsCurrentYaw   += mouseX * fpsYawSensitivity;
            fpsCurrentPitch -= mouseY * fpsPitchSensitivity;
            fpsCurrentPitch  = Mathf.Clamp(fpsCurrentPitch, -fpsPitchClamp, fpsPitchClamp);

            transform.rotation = Quaternion.Euler(fpsCurrentPitch, fpsCurrentYaw, 0f);

            // Giữ FOV đúng
            if (targetCamera == null) targetCamera = GetComponent<Camera>();
            if (targetCamera != null && !Mathf.Approximately(targetCamera.fieldOfView, fpsFOV))
                targetCamera.fieldOfView = fpsFOV;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CHARACTER RENDERER HELPERS
        // ─────────────────────────────────────────────────────────────────────
        private void CacheCharacterRenderers()
        {
            if (target == null) return;
            characterRenderers = target.GetComponentsInChildren<Renderer>(true);
        }

        private void SetCharacterRenderersVisible(bool visible)
        {
            if (characterRenderers == null) CacheCharacterRenderers();
            if (characterRenderers == null) return;

            foreach (var r in characterRenderers)
            {
                if (r != null) r.enabled = visible;
            }
            renderersHidden = !visible;
        }

        private void OnDisable()
        {
            // Đảm bảo restore renderer khi script bị tắt
            if (renderersHidden) SetCharacterRenderersVisible(true);
        }
    }
}
