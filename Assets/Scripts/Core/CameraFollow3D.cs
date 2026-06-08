using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SownInStone.Core
{
    /// <summary>
    /// Điều khiển Camera theo góc nhìn thứ 3 (Third Person) bám theo Player.
    /// Camera di chuyển mượt mà theo vị trí Player và chỉ xoay hướng khi người chơi
    /// nhấn giữ chuột phải (Right Mouse Button) và di chuyển chuột.
    /// </summary>
    public class CameraFollow3D : MonoBehaviour
    {
        [Header("--- THEO DÕI PLAYER ---")]
        [Tooltip("Đối tượng Camera sẽ đi theo. Nếu trống, script tự tìm Player trong scene.")]
        [SerializeField] private Transform target;

        [Tooltip("Độ trễ mượt mà khi di chuyển vị trí (giá trị càng lớn di chuyển càng nhanh/khớp).")]
        [SerializeField] private float smoothSpeed = 5f;

        [Header("--- THÔNG SỐ GÓC NHÌN THỨ 3 ---")]
        [Tooltip("Khoảng cách từ camera đến Player (4.5m - 6m).")]
        [SerializeField] private float distance = 5f;

        [Tooltip("Chiều cao của camera so với Player (2.5m - 3.5m).")]
        [SerializeField] private float height = 3f;

        [Tooltip("Góc nghiêng nhìn xuống Player hiện tại (khoảng 20° - 35°).")]
        [SerializeField] private float pitch = 25f;

        [Header("--- ĐIỀU KHIỂN XOAY XOAY ---")]
        [Tooltip("Có cho phép xoay camera thủ công bằng chuột không.")]
        [SerializeField] private bool enableManualRotation = true;

        [Tooltip("Nút chuột để giữ xoay camera (1 = Chuột phải).")]
        [SerializeField] private int rotateMouseButton = 1; 

        [Tooltip("Độ nhạy khi di chuyển chuột để xoay camera.")]
        [SerializeField] private float mouseSensitivity = 2.0f;

        [Tooltip("Góc nhìn nghiêng xuống tối thiểu (độ).")]
        [SerializeField] private float minPitch = 15f;

        [Tooltip("Góc nhìn nghiêng xuống tối đa (độ).")]
        [SerializeField] private float maxPitch = 45f;

        private float currentYaw = 0f;
        private Camera targetCamera;

        private void Start()
        {
            // Tự động tìm target là Player nếu chưa gán
            if (target == null && PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }

            targetCamera = GetComponent<Camera>();
            if (targetCamera != null)
            {
                // Thiết lập camera sang chế độ Perspective cho chuẩn góc nhìn 3D Third Person
                targetCamera.orthographic = false;
                targetCamera.fieldOfView = 60f;
            }

            // Khởi tạo góc quay ngang hiện tại khớp với hướng camera trong editor
            currentYaw = transform.eulerAngles.y;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (PlayerController.Instance != null)
                {
                    target = PlayerController.Instance.transform;
                }
                return;
            }

            // Xử lý xoay camera thủ công bằng chuột phải
            if (enableManualRotation)
            {
                float mouseX = 0f;
                float mouseY = 0f;
                bool isRMBHeld = false;

#if ENABLE_INPUT_SYSTEM
                if (Mouse.current != null)
                {
                    isRMBHeld = Mouse.current.rightButton.isPressed;
                    if (isRMBHeld)
                    {
                        // Delta của Input System trả về pixel delta nên cần chia tỷ lệ nhỏ lại
                        Vector2 delta = Mouse.current.delta.ReadValue();
                        mouseX = delta.x * 0.1f; 
                        mouseY = delta.y * 0.1f;
                    }
                }
#else
                isRMBHeld = Input.GetMouseButton(rotateMouseButton);
                if (isRMBHeld)
                {
                    mouseX = Input.GetAxis("Mouse X");
                    mouseY = Input.GetAxis("Mouse Y");
                }
#endif

                if (isRMBHeld)
                {
                    currentYaw += mouseX * mouseSensitivity;
                    pitch -= mouseY * mouseSensitivity;
                    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                }
            }

            // Tính toán vị trí mục tiêu của camera dựa trên Yaw và Pitch hiện tại
            Quaternion rotation = Quaternion.Euler(pitch, currentYaw, 0f);
            Vector3 negDistance = new Vector3(0f, 0f, -distance);
            Vector3 targetPosition = target.position + new Vector3(0f, height, 0f) + (rotation * negDistance);

            // Di chuyển vị trí camera mượt mà bằng Lerp
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            // Cập nhật hướng xoay của camera
            transform.rotation = rotation;
        }
    }
}


