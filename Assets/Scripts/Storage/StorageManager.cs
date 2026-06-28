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
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnDayChanged -= OnNewDayDecayCheck;
            }
        }

        #region QUẢN LÝ THÊM/BỚT ĐỒ KHO
        
        public void AddItem(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return;

            InventorySlot slot = storageSlots.Find(s => s.item.ItemID == item.ItemID);
            if (slot != null)
            {
                slot.quantity += amount;
            }
            else
            {
                storageSlots.Add(new InventorySlot(item, amount));
            }

            OnStorageChanged?.Invoke();
            Debug.Log($"[STORAGE] Thêm vào kho: {item.ItemName} x{amount}");
        }

        public bool RemoveItem(ItemData item, int amount)
        {
            if (item == null || amount <= 0) return false;

            InventorySlot slot = storageSlots.Find(s => s.item.ItemID == item.ItemID);
            if (slot == null || slot.quantity < amount)
            {
                Debug.LogWarning($"[STORAGE] Không đủ {item.ItemName} trong kho để dùng/bán!");
                return false;
            }

            slot.quantity -= amount;
            if (slot.quantity == 0)
            {
                storageSlots.Remove(slot);
            }

            OnStorageChanged?.Invoke();
            Debug.Log($"[STORAGE] Lấy ra từ kho: {item.ItemName} x{amount}");
            return true;
        }

        public List<InventorySlot> GetStorageSlots() => storageSlots;

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
