using UnityEngine;
using SownInStone.Core;
using SownInStone.Storage;
using SownInStone.UI;

namespace SownInStone.Interactions
{
    /// <summary>
    /// Bếp ga nhà Thành phục vụ chế biến thực phẩm và hồi phục thể lực.
    /// </summary>
    public class KitchenHearth : MonoBehaviour, IInteractable
    {
        [Header("--- THÔNG TIN VẬT PHẨM ---")]
        [SerializeField] private ItemData freshCropItem;
        [SerializeField] private ItemData preservedCropItem;

        private void Awake()
        {
            EnsureItemsLoaded();
        }

        private void EnsureItemsLoaded()
        {
            if (freshCropItem == null)
            {
#if UNITY_EDITOR
                freshCropItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_FreshCrop.asset");
#endif
                if (freshCropItem == null)
                {
                    freshCropItem = Resources.Load<ItemData>("Data/Item_FreshCrop");
                }
            }
            if (preservedCropItem == null)
            {
#if UNITY_EDITOR
                preservedCropItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_PreservedCrop.asset");
#endif
                if (preservedCropItem == null)
                {
                    preservedCropItem = Resources.Load<ItemData>("Data/Item_PreservedCrop");
                }
            }
        }

        /// <summary>
        /// Kích hoạt tương tác bếp ga khi người chơi nhấn E.
        /// </summary>
        public void Interact()
        {
            EnsureItemsLoaded();
            if (SurvivalUIManager.Instance == null) return;

            SurvivalUIManager.Instance.ShowDialogueWithChoices(
                "Bếp ga nhà Thành",
                "Bạn muốn thực hiện hành động nào trên bếp lò?",
                "Chế biến Khoai Gieo Khô (Cần 2 Khoai Tươi)",
                () => ProcessDryCrops(),
                "Luộc Khoai Ăn Trưa (Cần 1 Khoai Tươi)",
                () => CookFreshCrop()
            );
        }

        private void ProcessDryCrops()
        {
            EnsureItemsLoaded();
            if (StorageManager.Instance == null || freshCropItem == null || preservedCropItem == null)
            {
                CloseAndShowToast("Có lỗi xảy ra: Không tìm thấy hệ thống vật phẩm!");
                return;
            }

            // Kiểm tra số lượng khoai lang tươi trong cả Balo lẫn Rương dự trữ
            int currentQty = 0;
            var backpackSlot = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == freshCropItem.ItemID);
            var chestSlot = StorageManager.Instance.GetReserveChestSlots().Find(s => s.item != null && s.item.ItemID == freshCropItem.ItemID);
            if (backpackSlot != null) currentQty += backpackSlot.quantity;
            if (chestSlot != null) currentQty += chestSlot.quantity;

            if (currentQty >= 2)
            {
                if (StorageManager.Instance.RemoveItem(freshCropItem, 2))
                {
                    StorageManager.Instance.AddItem(preservedCropItem, 1);
                    if (SurvivalUIManager.Instance != null)
                    {
                        SurvivalUIManager.Instance.CloseDialogue();
                        SurvivalUIManager.Instance.ShowHUDToast("Bạn xắt lát phơi sấy khoai lang. Nhận +1 Khoai gieo khô dự trữ!");
                    }
                    if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                    {
                        TutorialManager.Instance.OnPreservedCropCrafted();
                    }
                }
            }
            else
            {
                if (SurvivalUIManager.Instance != null)
                {
                    SurvivalUIManager.Instance.CloseDialogue();
                    SurvivalUIManager.Instance.ShowHUDToast("Không đủ Khoai Lang Tươi để sấy khô!");
                }
            }
        }

        private void CookFreshCrop()
        {
            EnsureItemsLoaded();
            if (StorageManager.Instance == null || freshCropItem == null)
            {
                CloseAndShowToast("Có lỗi xảy ra: Không tìm thấy hệ thống vật phẩm!");
                return;
            }

            // Kiểm tra số lượng khoai lang tươi trong cả Balo lẫn Rương dự trữ
            int currentQty = 0;
            var backpackSlot = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == freshCropItem.ItemID);
            var chestSlot = StorageManager.Instance.GetReserveChestSlots().Find(s => s.item != null && s.item.ItemID == freshCropItem.ItemID);
            if (backpackSlot != null) currentQty += backpackSlot.quantity;
            if (chestSlot != null) currentQty += chestSlot.quantity;

            if (currentQty >= 1)
            {
                if (StorageManager.Instance.RemoveItem(freshCropItem, 1))
                {
                    if (PlayerStats.Instance != null)
                    {
                        PlayerStats.Instance.ModifyStamina(15f);
                    }
                    if (SurvivalUIManager.Instance != null)
                    {
                        SurvivalUIManager.Instance.CloseDialogue();
                        SurvivalUIManager.Instance.ShowHUDToast("Củ khoai luộc nóng hổi giúp bạn hồi phục +15 Thể lực.");
                    }
                }
            }
            else
            {
                if (SurvivalUIManager.Instance != null)
                {
                    SurvivalUIManager.Instance.CloseDialogue();
                    // Để kích hoạt màu đỏ cho dòng này (vì không chứa 'Không đủ'), ta chủ động thêm tag màu đỏ.
                    SurvivalUIManager.Instance.ShowHUDToast("<color=#E74C3C>Bạn không có sẵn củ khoai tươi nào để luộc!</color>");
                }
            }
        }

        private void CloseAndShowToast(string msg)
        {
            if (SurvivalUIManager.Instance != null)
            {
                SurvivalUIManager.Instance.CloseDialogue();
                SurvivalUIManager.Instance.ShowHUDToast(msg);
            }
        }
    }
}
