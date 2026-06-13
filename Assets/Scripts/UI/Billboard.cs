using UnityEngine;

namespace SownInStone.UI
{
    /// <summary>
    /// Giúp đối tượng visual 2D luôn quay mặt về phía Camera chính trong không gian 3D (Billboard effect).
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera != null)
            {
                // Luôn quay mặt về phía Camera chính (khóa trục Y để nhân vật đứng thẳng, không bị nghiêng)
                Vector3 targetDirection = transform.position - mainCamera.transform.position;
                targetDirection.y = 0; // Khóa góc xoay X/Z, chỉ xoay quanh trục Y
                
                if (targetDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(targetDirection);
                }
            }
        }
    }
}
