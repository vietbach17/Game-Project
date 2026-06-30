using System;
using System.Collections.Generic;
using UnityEngine;
using SownInStone.Core;
using SownInStone.Weather;

namespace SownInStone.Storage
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int quantity;

        public InventorySlot(ItemData item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }

    /// <summary>
    /// Quản lý hòm kho chứa nông sản và vật dụng của gia đình.
    /// Có logic đặc biệt: Tự động phân hủy/thối mốc thức ăn tươi trong mùa lụt ẩm nồm nếu không muối khô.
    /// </summary>
    public class StorageManager : MonoBehaviour
    {
        public static StorageManager Instance { get; private set; }

        [Header("--- KHO LƯU TRỮ ---")]
        [SerializeField] private List<InventorySlot> storageSlots = new List<InventorySlot>();
        [SerializeField] private List<InventorySlot> reserveChestSlots = new List<InventorySlot>();

        public event Action OnStorageChanged;
        public event Action<string> OnStorageAlert; // Báo hiệu khi có thức ăn tươi bị thối mốc

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            storageSlots.Clear(); // Khởi đầu trắng tay: xóa toàn bộ kho đồ khi bắt đầu game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDayChanged += OnNewDayDecayCheck;
            }

            AutoLoadDefaultItems();
        }

        private void AutoLoadDefaultItems()
        {
#if UNITY_EDITOR
            ItemData floodBoard = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_flood_board.asset");
            ItemData sandbag = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_sandbag.asset");
            ItemData seed = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_seed_potato.asset");
            ItemData nonLa = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_non_la.asset");
            ItemData mulch = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_plastic_mulch.asset");
            ItemData freshPotato = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_fresh_potato.asset");

            if (floodBoard != null && !storageSlots.Exists(s => s.item != null && s.item.ItemID == floodBoard.ItemID)) AddItem(floodBoard, 5);
            if (sandbag != null && !storageSlots.Exists(s => s.item != null && s.item.ItemID == sandbag.ItemID)) AddItem(sandbag, 5);
            if (seed != null && !storageSlots.Exists(s => s.item != null && s.item.ItemID == seed.ItemID)) AddItem(seed, 10);
            if (nonLa != null && !storageSlots.Exists(s => s.item != null && s.item.ItemID == nonLa.ItemID)) AddItem(nonLa, 1);
            if (mulch != null && !storageSlots.Exists(s => s.item != null && s.item.ItemID == mulch.ItemID)) AddItem(mulch, 3);
            if (freshPotato != null && !storageSlots.Exists(s => s.item != null && s.item.ItemID == freshPotato.ItemID)) AddItem(freshPotato, 10);
#endif
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDayChanged -= OnNewDayDecayCheck;
            }
        }

        /// <summary>
        /// Tìm ItemData tương ứng với ID chỉ định.
        /// Quét trong kho đồ hiện tại hoặc tự động tìm kiếm dự phòng.
        /// </summary>
        public ItemData GetItemDataByID(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            // 1. Tìm trong kho đồ hiện tại
            var slot = storageSlots.Find(s => s.item != null && s.item.ItemID.Equals(id, System.StringComparison.OrdinalIgnoreCase));
            if (slot != null) return slot.item;

            // 2. Tìm trong tài nguyên Resources nếu có
            ItemData resourceItem = Resources.Load<ItemData>($"Items/{id}");
            if (resourceItem != null) return resourceItem;

#if UNITY_EDITOR
            // 3. Fallback trong editor
            string path = "";
            if (id.Equals("item_flood_board", System.StringComparison.OrdinalIgnoreCase)) path = "Assets/Data/Item_flood_board.asset";
            else if (id.Equals("item_sandbag", System.StringComparison.OrdinalIgnoreCase)) path = "Assets/Data/Item_sandbag.asset";
            else if (id.Equals("item_seed_potato", System.StringComparison.OrdinalIgnoreCase) || id.Equals("item_seed", System.StringComparison.OrdinalIgnoreCase)) path = "Assets/Data/Item_seed_potato.asset";
            else if (id.Equals("item_non_la", System.StringComparison.OrdinalIgnoreCase)) path = "Assets/Data/Item_non_la.asset";
            else if (id.Equals("item_plastic_mulch", System.StringComparison.OrdinalIgnoreCase)) path = "Assets/Data/Item_plastic_mulch.asset";
            else if (id.Equals("item_fresh_potato", System.StringComparison.OrdinalIgnoreCase) || id.Equals("item_fresh_crop", System.StringComparison.OrdinalIgnoreCase)) path = "Assets/Data/Item_fresh_potato.asset";
            else if (id.Equals("item_khoai_gieo", System.StringComparison.OrdinalIgnoreCase)) path = "Assets/Data/Item_PreservedCrop.asset";
            else if (id.Equals("item_mi_tom", System.StringComparison.OrdinalIgnoreCase)) path = "Assets/Data/Item_Noodles.asset";

            if (!string.IsNullOrEmpty(path))
            {
                return UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
            }
#endif

            return null;
        }

        #region QUẢN LÝ THÊM/BỚT ĐỒ KHO
        
        public int GetMaxBackpackStack(ItemData item)
        {
            if (item == null) return 10;
            string id = item.ItemID != null ? item.ItemID.ToLower() : "";
            string name = item.ItemName != null ? item.ItemName.ToLower() : "";

            if (id.Contains("potato") || name.Contains("khoai"))
            {
                return 15; // Khoai lang giới hạn 15 trong balo
            }
            if (id.Contains("sandbag") || id.Contains("board") || id.Contains("mulch") || name.Contains("đá") || name.Contains("bùn") || name.Contains("gỗ"))
            {
                return 5; // Vật phẩm to/nặng giới hạn 5 trong balo
            }
            return 10; // Các vật phẩm khác (hạt giống, thực phẩm khô) giới hạn 10 trong balo
        }
        
        public void AddItem(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return;

            int maxStack = GetMaxBackpackStack(item);
            InventorySlot slot = storageSlots.Find(s => s.item != null && s.item.ItemID == item.ItemID);
            int currentQty = slot != null ? slot.quantity : 0;
            int capacityLeft = Mathf.Max(0, maxStack - currentQty);

            int toBackpack = Mathf.Min(amount, capacityLeft);
            int overflow = amount - toBackpack;

            if (toBackpack > 0)
            {
                if (slot != null)
                {
                    slot.quantity += toBackpack;
                }
                else
                {
                    storageSlots.Add(new InventorySlot(item, toBackpack));
                }
            }

            if (overflow > 0)
            {
                InventorySlot reserveSlot = reserveChestSlots.Find(s => s.item != null && s.item.ItemID == item.ItemID);
                if (reserveSlot != null)
                {
                    reserveSlot.quantity += overflow;
                }
                else
                {
                    reserveChestSlots.Add(new InventorySlot(item, overflow));
                }

                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast($"📦 Balo đầy (Max {maxStack})! x{overflow} {item.ItemName} đã tự động chuyển vào Rương đồ dự trữ ở nhà.");
                }
                Debug.Log($"[STORAGE] Balo đầy! x{overflow} {item.ItemName} tự động chuyển vào Rương dự trữ.");
            }

            OnStorageChanged?.Invoke();
            Debug.Log($"[STORAGE] Thêm vào kho balo: {item.ItemName} x{toBackpack}");
        }

        public bool RemoveItem(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return false;

            int totalAvailable = 0;
            InventorySlot bSlot = storageSlots.Find(s => s.item != null && s.item.ItemID == item.ItemID);
            InventorySlot rSlot = reserveChestSlots.Find(s => s.item != null && s.item.ItemID == item.ItemID);

            if (bSlot != null) totalAvailable += bSlot.quantity;
            if (rSlot != null) totalAvailable += rSlot.quantity;

            if (totalAvailable < amount)
            {
                Debug.LogWarning($"[STORAGE] Không đủ {item.ItemName} trong balo lẫn rương để dùng/bán!");
                return false;
            }

            int needed = amount;
            if (bSlot != null && bSlot.quantity > 0)
            {
                int take = Mathf.Min(needed, bSlot.quantity);
                bSlot.quantity -= take;
                needed -= take;
                if (bSlot.quantity == 0) storageSlots.Remove(bSlot);
            }

            if (needed > 0 && rSlot != null && rSlot.quantity > 0)
            {
                int take = Mathf.Min(needed, rSlot.quantity);
                rSlot.quantity -= take;
                needed -= take;
                if (rSlot.quantity == 0) reserveChestSlots.Remove(rSlot);
            }

            OnStorageChanged?.Invoke();
            Debug.Log($"[STORAGE] Lấy ra: {item.ItemName} x{amount}");
            return true;
        }

        public bool HasItem(ItemData item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;
            int total = 0;
            InventorySlot bSlot = storageSlots.Find(s => s.item != null && s.item.ItemID == item.ItemID);
            InventorySlot rSlot = reserveChestSlots.Find(s => s.item != null && s.item.ItemID == item.ItemID);
            if (bSlot != null) total += bSlot.quantity;
            if (rSlot != null) total += rSlot.quantity;
            return total >= amount;
        }

        public void AutoDepositOverflowItems()
        {
            int depositedCount = 0;
            for (int i = storageSlots.Count - 1; i >= 0; i--)
            {
                InventorySlot slot = storageSlots[i];
                if (slot != null && slot.item != null)
                {
                    int limit = GetMaxBackpackStack(slot.item);
                    if (slot.quantity > limit)
                    {
                        int overflow = slot.quantity - limit;
                        slot.quantity = limit;

                        InventorySlot rSlot = reserveChestSlots.Find(s => s.item != null && s.item.ItemID == slot.item.ItemID);
                        if (rSlot != null) rSlot.quantity += overflow;
                        else reserveChestSlots.Add(new InventorySlot(slot.item, overflow));

                        depositedCount += overflow;
                    }
                }
            }
            if (depositedCount > 0)
            {
                OnStorageChanged?.Invoke();
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast($"📦 Đã tự động cất {depositedCount} nông sản & thực phẩm dư thừa vào Rương dự trữ!");
                }
            }
        }

        public List<InventorySlot> GetStorageSlots() => storageSlots;
        public List<InventorySlot> GetReserveChestSlots() => reserveChestSlots;

        #endregion

        /// <summary>
        /// Logic "Tích Cốc Phòng Cơ" - Kiểm tra thối mốc mỗi ngày mới.
        /// Thức ăn tươi (NongSanTuoi) để ngoài kho mùa bão lũ sẽ bị hỏng dần.
        /// </summary>
        private void OnNewDayDecayCheck(int currentDay)
        {
            if (WeatherManager.Instance == null) return;

            float currentHumidity = WeatherManager.Instance.Humidity;
            bool isDecayDay = false;

            // Chạy qua từng slot kiểm tra hao hụt
            for (int i = storageSlots.Count - 1; i >= 0; i--)
            {
                InventorySlot slot = storageSlots[i];
                if (slot.item.type == ItemType.NongSanTuoi && slot.item.DecayRateInHumidity > 0f)
                {
                    // Tỷ lệ hỏng tăng mạnh khi độ ẩm nồm mùa mưa lũ lớn (>80% ẩm)
                    float humidityMultiplier = currentHumidity > 80f ? 2.5f : 1.0f;
                    float decayAmountFloat = slot.quantity * slot.item.DecayRateInHumidity * humidityMultiplier;
                    int decayAmount = Mathf.Max(1, Mathf.RoundToInt(decayAmountFloat));

                    slot.quantity -= decayAmount;
                    isDecayDay = true;

                    string decayMsg = $"Thời tiết nồm ẩm! {slot.item.ItemName} bị hư thối mất x{decayAmount} củ!";
                    Debug.LogWarning($"[TÍCH CỐC] {decayMsg}");
                    OnStorageAlert?.Invoke(decayMsg);

                    if (slot.quantity <= 0)
                    {
                        storageSlots.RemoveAt(i);
                        OnStorageAlert?.Invoke($"Toàn bộ số {slot.item.ItemName} của bạn đã bị thối rữa hoàn toàn do nồm ẩm!");
                    }
                }
            }

            if (isDecayDay)
            {
                OnStorageChanged?.Invoke();
                OnStorageAlert?.Invoke("Độ ẩm không khí cao đã làm thối một phần nông sản tươi trong kho! Hãy làm khoai gieo phơi khô hoặc muối dưa để lưu trữ lâu dài!");
            }
        }

        /// <summary>
        /// Logic chế biến nông sản thô tại chỗ thành đồ khô lưu trữ vĩnh viễn.
        /// </summary>
        public bool CraftPreservedItem(ItemData freshItem, ItemData preservedItem, int craftQuantity)
        {
            // Ví dụ: 3 Khoai lang tươi + 1 Stamina -> 1 Khoai Gieo khô
            if (RemoveItem(freshItem, craftQuantity * 3))
            {
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.ModifyStamina(-10f); // Tốn sức thái lát và phơi/ủ mắm
                }
                AddItem(preservedItem, craftQuantity);
                Debug.Log($"[ chế biến] Đã chế biến thành công {craftQuantity} {preservedItem.ItemName} dự trữ mùa lũ!");
                return true;
            }
            return false;
        }
    }
}
