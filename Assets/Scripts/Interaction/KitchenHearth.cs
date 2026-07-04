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
        private bool isProcessing = false;
        private float processingTimer = 0f;
        private int processingTargetCount = 0;
        private int processingCompleted = 0;
        private int lastSecondsRemaining = -1;

        private void Awake() { EnsureItemsLoaded(); }

        private void Update()
        {
            if (!isProcessing) return;
            processingTimer -= Time.deltaTime;

            if (processingTimer > 5.0f)
            {
                // Hiển thị trạng thái đã hoàn thành củ trước đó trong thời gian ngắn chuyển tiếp
                if (SurvivalUIManager.Instance != null)
                {
                    SurvivalUIManager.Instance.ShowHUDToast($"Đã sấy {processingCompleted}/{processingTargetCount}");
                }
            }
            else
            {
                // Tính toán thời gian đếm ngược cho củ khoai hiện tại (tối đa 5s)
                int secondsRemaining = Mathf.CeilToInt(processingTimer);
                if (secondsRemaining < 1) secondsRemaining = 1;

                int currentProductIndex = processingCompleted + 1;

                if (secondsRemaining != lastSecondsRemaining)
                {
                    lastSecondsRemaining = secondsRemaining;
                    if (SurvivalUIManager.Instance != null)
                    {
                        SurvivalUIManager.Instance.ShowHUDToast($"🍳 Đang chế biến ({currentProductIndex}/{processingTargetCount}): còn {secondsRemaining}s");
                    }
                }
            }

            if (processingTimer > 0f || processingCompleted >= processingTargetCount) return;

            if (StorageManager.Instance != null && preservedCropItem != null)
                StorageManager.Instance.AddItem(preservedCropItem, 1);
            
            processingCompleted++;
            lastSecondsRemaining = -1; // Reset để bắt đầu đếm ngược từ 5s cho củ tiếp theo

            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                TutorialManager.Instance.OnPreservedCropCrafted();

            if (processingCompleted >= processingTargetCount)
            {
                isProcessing = false;
                if (SurvivalUIManager.Instance != null)
                {
                    SurvivalUIManager.Instance.CloseDialogue();
                    SurvivalUIManager.Instance.ShowHUDToast($"Xong! +{processingTargetCount} Khoai gieo!");
                }
            }
            else 
            { 
                processingTimer = 5.8f; 
            }
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
                () => ShowQuantitySelector(),
                "Luộc Khoai Ăn Trưa (Cần 1 Khoai Tươi)",
                () => CookFreshCrop()
            );
        }

        private void ShowQuantitySelector()
        {
            Debug.Log("[KitchenHearth] ShowQuantitySelector called");
            EnsureItemsLoaded();
            Debug.Log($"[KitchenHearth] After EnsureItemsLoaded: freshCropItem={( freshCropItem?.ItemID ?? "NULL")}, preservedCropItem={(preservedCropItem?.ItemID ?? "NULL")}");
            if (freshCropItem == null || preservedCropItem == null) { CloseAndShowToast("Lỗi: Không tìm thấy vật phẩm khoai!"); return; }
            int qty = GetFreshCropQuantity();
            Debug.Log($"[KitchenHearth] Khoai tươi trong kho: {qty}");
            if (qty < 2) { CloseAndShowToast($"Không đủ Khoai Lang Tươi! Cần ít nhất 2 củ, hiện có {qty}."); return; }
            int max = Mathf.Min(qty / 2, 5);
            Debug.Log($"[KitchenHearth] max = {max}");
            if (max < 1) { CloseAndShowToast("Không đủ khoai để chế biến!"); return; }
            if (max >= 5)
                SurvivalUIManager.Instance.ShowDialogueWithChoices(
                    "Làm Khoai Gieo Khô", "Bạn có " + qty + " Khoai Tươi. Chọn số lượng:",
                    "Làm 1 cái (2 Khoai, 5s)", () => StartProcessing(1),
                    "Làm 3 cái (6 Khoai, 15s)", () => StartProcessing(3),
                    "Làm 5 cái (10 Khoai, 25s)", () => StartProcessing(5));
            else if (max >= 3)
                SurvivalUIManager.Instance.ShowDialogueWithChoices(
                    "Làm Khoai Gieo Khô", "Bạn có " + qty + " Khoai Tươi. Chọn số lượng:",
                    "Làm 1 cái (2 Khoai, 5s)", () => StartProcessing(1),
                    "Làm 3 cái (6 Khoai, 15s)", () => StartProcessing(3),
                    null, null);
            else
                SurvivalUIManager.Instance.ShowDialogueWithChoices(
                    "Làm Khoai Gieo Khô", "Chỉ đủ làm 1 cái (2 Khoai, 5s):",
                    "Làm 1 cái", () => StartProcessing(1),
                    "Hủy bỏ", () => { if (SurvivalUIManager.Instance != null) SurvivalUIManager.Instance.CloseDialogue(); },
                    null, null);
        }

        private void StartProcessing(int count)
        {
            EnsureItemsLoaded();
            int need = count * 2;
            Debug.Log($"[KitchenHearth] StartProcessing({count}): cần {need} khoai tươi");
            if (GetFreshCropQuantity() < need)
            {
                CloseAndShowToast($"Không đủ khoai! Cần {need}, hiện có {GetFreshCropQuantity()}.");
                return;
            }
            if (!StorageManager.Instance.RemoveItem(freshCropItem, need))
            {
                CloseAndShowToast("Lỗi: Không thể trừ khoai tươi khỏi kho!");
                return;
            }
            isProcessing = true;
            processingTargetCount = count;
            processingCompleted = 0;
            processingTimer = 5f;
            lastSecondsRemaining = -1; // Reset countdown second
            if (SurvivalUIManager.Instance != null)
            {
                SurvivalUIManager.Instance.CloseDialogue();
                SurvivalUIManager.Instance.ShowHUDToast("Bắt đầu chế biến " + count + " Khoai gieo...");
            }
        }

        private int GetFreshCropQuantity()
        {
            if (StorageManager.Instance == null) { Debug.LogWarning("[KitchenHearth] StorageManager.Instance is NULL!"); return 0; }
            if (freshCropItem == null) { Debug.LogWarning("[KitchenHearth] freshCropItem is NULL!"); return 0; }
            int qty = 0;
            var allSlots = StorageManager.Instance.GetStorageSlots();
            var allChest = StorageManager.Instance.GetReserveChestSlots();
            Debug.Log($"[KitchenHearth] Checking {allSlots.Count} backpack slots + {allChest.Count} chest slots for ItemID={freshCropItem.ItemID}");
            var slot = allSlots.Find(s => s.item != null && s.item.ItemID == freshCropItem.ItemID);
            var chest = allChest.Find(s => s.item != null && s.item.ItemID == freshCropItem.ItemID);
            if (slot != null) { qty += slot.quantity; Debug.Log($"[KitchenHearth] Found {slot.quantity} in backpack"); }
            if (chest != null) { qty += chest.quantity; Debug.Log($"[KitchenHearth] Found {chest.quantity} in chest"); }
            Debug.Log($"[KitchenHearth] Total fresh crop quantity: {qty}");
            return qty;
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
