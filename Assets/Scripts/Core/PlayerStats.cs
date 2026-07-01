using System;
using UnityEngine;
using SownInStone.Storage;

namespace SownInStone.Core
{
    /// <summary>
    /// Quản lý chỉ số sinh tồn của người chơi gồm Sức khỏe (Health), Thể lực (Stamina), Tinh thần (Morale).
    /// Hỗ trợ các hiệu ứng môi trường đặc trưng của miền Trung (Sốc nhiệt do Gió Lào, Cảm lạnh do mưa lụt, Hoảng loạn do bão).
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [Header("--- CHỈ SỐ CƠ BẢN ---")]
        [Tooltip("Sức khỏe hiện tại (0 - 100). Hết sức khỏe = ngất xỉu/kết thúc trò chơi.")]
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;

        [Tooltip("Thể lực (0 - 100). Tiêu hao khi làm việc đồng áng. Hồi phục khi nghỉ ngơi, ăn uống.")]
        [SerializeField] private float maxStamina = 100f;
        private float currentStamina;

        [Tooltip("Tinh thần/Ý chí sinh tồn (0 - 100). Giảm do bão lũ, hoảng loạn. Tăng khi cúng bái, sinh hoạt xóm giềng.")]
        [SerializeField] private float maxMorale = 100f;
        private float currentMorale;
        
        private bool isRescuing = false;

        [Header("--- HIỆU ỨNG THỜI TIẾT ---")]
        [Tooltip("Mức độ mất nước / sốc nhiệt do Gió Lào (0 - 100).")]
        public float HeatStress { get; private set; }
        
        [Tooltip("Mức độ nhiễm lạnh / úng nước do ngập lụt, dầm mưa (0 - 100).")]
        public float ColdStress { get; private set; }

        [Header("--- TÀI CHÍNH GIA ĐÌNH ---")]
        [Tooltip("Số lượng tiền xu hiện có.")]
        [SerializeField] private int coins = 50;

        // Các thuộc tính truy cập công khai
        public float CurrentHealth => currentHealth;
        public float CurrentStamina => currentStamina;
        public float CurrentMorale => currentMorale;
        public int Coins => coins;

        // Sự kiện gửi cho UI cập nhật khi chỉ số thay đổi
        public event Action<float, float> OnHealthChanged; // (current, max)
        public event Action<float, float> OnStaminaChanged; // (current, max)
        public event Action<float, float> OnMoraleChanged; // (current, max)
        public event Action<int> OnCoinsChanged;
        public event Action<string> OnPlayerAlert; // Gửi thông điệp cảnh báo lên UI (ví dụ: "Bạn đang bị sốc nhiệt!")

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
            // Khởi tạo các chỉ số ban đầu đầy đủ
            currentHealth = maxHealth;
            currentStamina = maxStamina;
            currentMorale = maxMorale;

            TriggerChangeEvents();
        }

        private void Update()
        {
            // Giả lập ảnh hưởng tự nhiên theo thời gian
            HandlePassiveStressModifiers();
        }

        /// <summary>
        /// Xử lý các chỉ số suy giảm gián tiếp do thời tiết khắc nghiệt.
        /// Được GameManager và WeatherManager tác động.
        /// </summary>
        private void HandlePassiveStressModifiers()
        {
            // Nếu bị sốc nhiệt cao (>70), bắt đầu trừ máu từ từ và gây mất thể lực nhanh
            if (HeatStress > 70f)
            {
                ModifyStamina(-2f * Time.deltaTime);
                ModifyHealth(-1f * Time.deltaTime);
                if (UnityEngine.Random.value < 0.001f)
                    TriggerAlert("Cơ thể đang quá nóng! Hãy vào bóng râm uống bát nước chè xanh!");
            }

            // Nếu bị lạnh buốt do dầm mưa bão lâu (>70), tụt tinh thần và trừ dần máu
            if (ColdStress > 70f)
            {
                ModifyMorale(-1.5f * Time.deltaTime);
                ModifyHealth(-0.8f * Time.deltaTime);
                if (UnityEngine.Random.value < 0.001f)
                    TriggerAlert("Cơ thể nhiễm lạnh! Bạn cần sưởi ấm hoặc ăn khoai gieo nóng!");
            }
        }

        #region HÀM ĐIỀU CHỈNH CHỈ SỐ
        
        public void ModifyHealth(float amount)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
            {
                HandlePlayerFaint();
            }
        }

        public void ModifyStamina(float amount)
        {
            currentStamina = Mathf.Clamp(currentStamina + amount, 0f, maxStamina);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        public void ModifyMorale(float amount)
        {
            currentMorale = Mathf.Clamp(currentMorale + amount, 0f, maxMorale);
            OnMoraleChanged?.Invoke(currentMorale, maxMorale);

            if (currentMorale <= 0f)
            {
                HandlePlayerFaint();
            }
        }

        /// <summary>
        /// Tăng hoặc giảm điểm stress nhiệt độ.
        /// </summary>
        public void ApplyHeatStress(float amount)
        {
            HeatStress = Mathf.Clamp(HeatStress + amount, 0f, 100f);
        }

        /// <summary>
        /// Tăng hoặc giảm điểm stress lạnh buốt.
        /// </summary>
        public void ApplyColdStress(float amount)
        {
            ColdStress = Mathf.Clamp(ColdStress + amount, 0f, 100f);
        }

        public void ModifyCoins(int amount)
        {
            if (amount != 0)
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_coins");
            }
            coins = Mathf.Max(0, coins + amount);
            OnCoinsChanged?.Invoke(coins);
        }

        public void TriggerAlert(string message)
        {
            OnPlayerAlert?.Invoke(message);
        }

        /// <summary>
        /// Sử dụng vật phẩm tiêu thụ để hồi phục chỉ số sinh lý.
        /// </summary>
        public bool UseItem(ItemData item)
        {
            if (item == null) return false;

            // Kiểm tra khả năng tiêu thụ (có giá trị hồi phục thể lực hoặc tinh thần)
            if (item.StaminaRestoreValue <= 0f && item.MoraleRestoreValue <= 0f)
            {
                TriggerAlert($"Vật phẩm {item.ItemName} không thể tiêu thụ trực tiếp!");
                return false;
            }

            // Nhang cúng chỉ được thắp tại bàn thờ
            if (item.type == ItemType.Incense)
            {
                TriggerAlert("Nhang nên được thắp tại Bàn thờ Gia tiên để cầu nguyện bình an!");
                return false;
            }

            // Không cho ăn mì tôm khi đang làm nhiệm vụ gom đồ giúp O Thắm
            if (item.ItemID == "item_mi_tom" && TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive && TutorialManager.Instance.currentStage == TutorialManager.TutorialStage.PrepareForStorm)
            {
                TriggerAlert("⚠️ Mì tôm này dùng để cất vào rương của O Thắm tránh bão, vui lòng không ăn!");
                return false;
            }

            // Không cho ăn khoai gieo khi đang làm nhiệm vụ chia sẻ lương thực
            if (item.ItemID == "item_khoai_gieo" && TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive && TutorialManager.Instance.currentStage == TutorialManager.TutorialStage.SharePreservedCrops)
            {
                TriggerAlert("⚠️ Khoai gieo này dùng để chia sẻ cho bà con dân làng, vui lòng không ăn!");
                return false;
            }

            // Tiến hành khấu trừ và phục hồi chỉ số
            if (StorageManager.Instance != null)
            {
                if (StorageManager.Instance.RemoveItem(item, 1))
                {
                    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_eat");
                    if (item.StaminaRestoreValue > 0f)
                    {
                        ModifyStamina(item.StaminaRestoreValue);
                    }
                    if (item.MoraleRestoreValue > 0f)
                    {
                        ModifyMorale(item.MoraleRestoreValue);
                    }

                    // Giảm chỉ số nhiễm lạnh ColdStress khi ăn thực phẩm nóng/khô hoặc mì tôm cứu trợ
                    bool isWarmDryFood = (item.type == ItemType.NongSanKho || item.ItemID == "item_mi_tom");
                    if (isWarmDryFood)
                    {
                        ApplyColdStress(-25f);
                    }

                    string logMsg = $"Đã dùng 1 {item.ItemName}";
                    System.Collections.Generic.List<string> statsList = new System.Collections.Generic.List<string>();
                    if (item.StaminaRestoreValue > 0f) statsList.Add($"+{item.StaminaRestoreValue} Thể lực");
                    if (item.MoraleRestoreValue > 0f) statsList.Add($"+{item.MoraleRestoreValue} Tinh thần");
                    if (isWarmDryFood) statsList.Add("-25 Lạnh");
                    if (statsList.Count > 0)
                    {
                        logMsg += $" ({string.Join(", ", statsList)})";
                    }
                    logMsg += "!";

                    TriggerAlert(logMsg);
                    return true;
                }
            }
            return false;
        }

        #endregion

        private void HandlePlayerFaint()
        {
            if (isRescuing) return;
            isRescuing = true;

            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_faint");
            OnPlayerAlert?.Invoke("Bạn đã kiệt sức hoàn toàn và ngất xỉu!");
            
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.TriggerRescueSequence();
            }

            isRescuing = false;
        }

        public void RestoreSaveState(float health, float stamina, float morale, int savedCoins)
        {
            currentHealth = health;
            currentStamina = stamina;
            currentMorale = morale;
            coins = savedCoins;
            TriggerChangeEvents();
            Debug.Log($"[PLAYER STATS] Khôi phục chỉ số từ Save: Health={currentHealth}, Stamina={currentStamina}, Morale={currentMorale}, Coins={coins}");
        }

        private void TriggerChangeEvents()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            OnMoraleChanged?.Invoke(currentMorale, maxMorale);
            OnCoinsChanged?.Invoke(coins);
        }
    }
}
