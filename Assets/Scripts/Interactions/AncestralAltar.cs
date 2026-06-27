using UnityEngine;
using SownInStone.Core;
using SownInStone.UI;
using SownInStone.Storage;

namespace SownInStone.Interactions
{
    public class AncestralAltar : MonoBehaviour
    {
        [Header("Altar Settings")]
        [SerializeField] private int moraleReward = 10; // Điểm tinh thần hồi phục
        private bool isIncenseBurning;

        public bool IsIncenseBurning => isIncenseBurning;
        public void ActionBurnIncense() => Interact();

        /// <summary>
        /// Hàm tương tác chính thức: Gọi khi Player đứng trong vùng Trigger của Altar và nhấn [E]
        /// </summary>
        public void Interact()
        {
            // 1. Kiểm tra xem người chơi đã hoàn thành giai đoạn 1 của Tutorial chưa (Gặp đủ 4 NPC)
            if (TutorialManager.Instance != null && !TutorialManager.Instance.IsTutorialCompleted)
            {
                if (TutorialManager.Instance.CurrentState == TutorialState.IntroQuests)
                {
                    SurvivalUIManager.Instance.ShowHUDToast("Không thể thắp nhang! Hãy đi thăm hỏi đủ 4 dân làng trước.");
                    return;
                }
            }

            // 2. Chặn tương tác nếu bão lũ đang diễn ra hoặc đã qua bão
            if (GameManager.Instance != null && !GameManager.Instance.IsBeforeStorm)
            {
                SurvivalUIManager.Instance.ShowHUDToast("Bàn thờ gia tiên đã được thắp nhang cầu an từ trước bão.");
                return;
            }

            // 3. Tiến hành kiểm tra vật phẩm Nhang Cúng trong kho đồ của StorageManager
            ItemData incenseData = StorageManager.Instance.GetItemDataByID("item_incense");

            if (incenseData == null)
            {
                SurvivalUIManager.Instance.ShowHUDToast("Không tìm thấy dữ liệu vật phẩm Nhang cúng!");
                return;
            }

            int ownedIncense = StorageManager.Instance.GetItemQuantity(incenseData);

            if (ownedIncense > 0)
            {
                // Thực hiện tiêu hao 1 cây nhang cúng trong túi đồ
                StorageManager.Instance.RemoveItem(incenseData, 1);

                // Hồi phục +10 điểm Tinh thần (Morale) cho người chơi, clamp tối đa 100
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.RestoreMorale(moraleReward);
                }

                isIncenseBurning = true;

                // Phát thông báo Toast Regex tôn kính lên HUD góc màn hình
                SurvivalUIManager.Instance.ShowHUDToast("Thành thắp nén nhang cầu an cho làng xã. +10 Tinh thần");

                // --- TRIGGER CỐT LÕI: Gọi GameManager ép hệ thống sập bão và dâng nước lũ tức thì ---
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TriggerStormCrisis();
                }
            }
            else
            {
                // Thông báo báo đỏ cảnh báo tài nguyên thiếu hụt
                SurvivalUIManager.Instance.ShowHUDToast("Không đủ vật phẩm! Bạn cần có Nhang Cúng trong kho đồ.");
            }
        }
    }
}