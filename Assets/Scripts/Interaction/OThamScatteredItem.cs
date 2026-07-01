using UnityEngine;
using SownInStone.UI;

namespace SownInStone.Interactions
{
    /// <summary>
    /// Vật phẩm rơi vãi quanh tiệm O Thắm.
    /// Khi người chơi tương tác (E), nhân vật sẽ "nhặt" đồ (đếm vào tay cầm)
    /// và vật thể biến mất khỏi mặt đất.
    /// Sau khi nhặt xong thì ra rương O Thắm bấm E để cất.
    /// </summary>
    public class OThamScatteredItem : MonoBehaviour, IInteractable
    {
        [Tooltip("Tên đồ vật hiển thị trong toast khi nhặt")]
        public string itemDisplayName = "Đồ dùng";

        private bool pickedUp = false;

        public void Interact()
        {
            if (pickedUp) return;

            // Chỉ cho nhặt trong Phase 2
            if (TutorialManager.Instance == null ||
                TutorialManager.Instance.currentStage != TutorialManager.TutorialStage.PrepareForStorm)
            {
                SurvivalUIManager.Instance?.ShowHUDToast("Hiện tại chưa cần nhặt đồ cho O Thắm.");
                return;
            }

            pickedUp = true;

            // Ghi nhớ vào TutorialManager để rương biết còn bao nhiêu đồ chưa cất
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.oThamCarryingCount++;
            }

            SurvivalUIManager.Instance?.ShowHUDToast($"Nhặt [{itemDisplayName}] – Mang ra rương O Thắm để cất!");

            // Ẩn vật phẩm khỏi Scene (không xóa để tránh lỗi reference)
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Được gọi từ FrameworkDebugUI hoặc TutorialManager để reset khi test lại.
        /// </summary>
        public void ResetItem()
        {
            pickedUp = false;
            gameObject.SetActive(true);
        }
    }

    public class OThamChest : MonoBehaviour, IInteractable
    {
        public void Interact()
        {
            if (TutorialManager.Instance == null) return;

            // Chỉ hoạt động trong Phase 2
            if (TutorialManager.Instance.currentStage != TutorialManager.TutorialStage.PrepareForStorm)
            {
                SurvivalUIManager.Instance?.ShowHUDToast("Rương đồ của O Thắm – hiện chưa cần dùng.");
                return;
            }

            TutorialManager.Instance.OnOThamItemStored();
        }
    }
}
