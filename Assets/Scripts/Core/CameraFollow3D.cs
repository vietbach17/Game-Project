using UnityEngine;

namespace SownInStone.Core
{
    /// <summary>
    /// Điều khiển Camera di chuyển theo Player trong môi trường 3D với góc nghiêng và độ trễ mượt mà.
    /// </summary>
    public class CameraFollow3D : MonoBehaviour
    {
        [Header("--- THEO DÕI PLAYER ---")]
        [Tooltip("Đối tượng Camera sẽ đi theo. Nếu trống, script tự tìm Player trong scene.")]
        [SerializeField] private Transform target;

        [Tooltip("Độ trễ mượt mà khi di chuyển (giá trị càng nhỏ camera đi càng mượt/chậm).")]
        [SerializeField] private float smoothSpeed = 5f;

        [Tooltip("Khoảng cách lệch (Offset) của Camera so với Player trong không gian 3D.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -6f);

        [Tooltip("Xoay Camera nhìn xuống Player (góc nghiêng).")]
        [SerializeField] private Vector3 rotationOffset = new Vector3(50f, 0f, 0f);

        private void Start()
        {
            // Tự động tìm target là Player nếu chưa gán
            if (target == null && PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
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
                return;
            }

            // Tính toán vị trí mục tiêu
            Vector3 targetPosition = target.position + offset;
            
            // Di chuyển camera mượt mà bằng Lerp
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

            // Đồng bộ góc quay camera
            transform.rotation = Quaternion.Euler(rotationOffset);
        }
    }
}
