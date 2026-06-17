using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using SownInStone.Core;
using SownInStone.Weather;
using SownInStone.Storage;
using SownInStone.Community;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SownInStone.UI
{
    /// <summary>
    /// Bộ quản lý giao diện sinh tồn tập trung (Singleton UI Canvas Manager) chuẩn uGUI của Unity.
    /// Đã được tinh chỉnh giao diện nhỏ gọn chuẩn góc nhìn thứ 3 (Third Person).
    /// </summary>
    public class SurvivalUIManager : MonoBehaviour
    {
        public static SurvivalUIManager Instance { get; private set; }

        [Header("--- CANVAS GROUP ---")]
        [Tooltip("Kéo CanvasGroup chính của cụm UI này vào để ẩn/hiển thị đồng bộ.")]
        [SerializeField] private CanvasGroup mainUICanvasGroup;

        [Header("--- CHỈ SỐ SINH TỒN ---")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private Slider staminaSlider;
        [SerializeField] private Slider moraleSlider;

        [Header("--- THỜI GIAN & THỜI TIẾT ---")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private TextMeshProUGUI weatherText;

        [Header("--- KHUNG HỘI THOẠI (DIALOGUE) ---")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueContentText;
        [Tooltip("Tốc độ chạy chữ hội thoại từng ký tự (số giây trễ).")]
        [SerializeField] private float typingSpeed = 0.025f;

        [Header("--- HÒM ĐỒ (INVENTORY) ---")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform itemSlotContainer;
        [Tooltip("Prefab của ô vật phẩm lưới chứa Image Icon và Text số lượng.")]
        [SerializeField] private GameObject itemSlotPrefab;

        [Header("--- CÀI ĐẶT CHẾ TẠO KHOAI GIEO ---")]
        [SerializeField] private ItemData freshCropItem;
        [SerializeField] private ItemData preservedCropItem;

        [Header("--- HIỆU ỨNG THIÊN TAI ---")]
        [SerializeField] private Sprite waterWavesSprite;
        [Range(0f, 1f)]
        [SerializeField] private float maxWaterAlpha = 0.5f;

        private Image waterOverlayImage;
        private RectTransform waterOverlayRect;

        [Header("--- HIỆU ỨNG SỐC NHIỆT & LẠNH ---")]
        [Range(0f, 1f)]
        [SerializeField] private float maxOverlayAlpha = 0.6f;
        [SerializeField] private Color heatColor = new Color(0.85f, 0.25f, 0.05f); // Cam đất rực cháy
        [SerializeField] private Color coldColor = new Color(0.35f, 0.65f, 0.85f); // Xanh buốt giá

        private Image heatOverlayImage;
        private Image coldOverlayImage;
        private Coroutine dialogueCoroutine;
        private bool isInventoryOpen = false;
        private bool isDialogueActive = false;

        // Banner thông báo chuyển Phase
        private GameObject phaseAnnouncementObj;
        private CanvasGroup phaseAnnouncementCanvasGroup;
        private TextMeshProUGUI phaseAnnouncementText;
        private Coroutine announcementCoroutine;

        // Các biến điều phối lựa chọn hội thoại
        private GameObject choiceContainer;
        private Button choice1Button;
        private Button choice2Button;
        private Button choice3Button;
        private TextMeshProUGUI choice1Text;
        private TextMeshProUGUI choice2Text;
        private TextMeshProUGUI choice3Text;
        private System.Action onChoice1Selected;
        private System.Action onChoice2Selected;
        private System.Action onChoice3Selected;
        private bool isChoiceActive = false;

        private TextMeshProUGUI inventoryCoinsText;

        // --- CÁC PANEL MỚI THÊM CHO GÓC NHÌN THỨ 3 ---
        private GameObject communityPanel;
        private TextMeshProUGUI communityText;
        private bool isCommunityOpen = false;

        private GameObject weatherDetailsPanel;
        private TextMeshProUGUI weatherDetailsText;
        private bool isWeatherDetailsOpen = false;

        private TextMeshProUGUI interactionPromptText;
        private TextMeshProUGUI coinsText; // Text Xu ở Top-Right

        private GameObject toastPanel;
        private TextMeshProUGUI toastText;
        private CanvasGroup toastCanvasGroup;
        private Coroutine toastCoroutine;

        // --- PHÂN HỆ CỬA HÀNG O THẮM (SHOP SYSTEM) ---
        [Header("--- CÀI ĐẶT CỬA HÀNG ---")]
        [SerializeField] private ItemData seedItem;
        [SerializeField] private ItemData incenseItem;
        [SerializeField] private ItemData noodlesItem;

        private GameObject shopPanel;
        private TextMeshProUGUI shopCoinsText;
        private bool isShopOpen = false;
        private List<GameObject> shopItemRows = new List<GameObject>();

        public bool IsDialogueActive => isDialogueActive;
        public bool IsChoiceActive => isChoiceActive;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Cấu hình lại layout mặc định cho gọn gàng (Top-left, Top-right, Bottom-left)
            ReorganizeUILayout();
            SetupInventoryScrollView();

            // Thiết lập các lớp phủ vignette
            SetupVignetteOverlays();

            // Thiết lập lớp phủ nước lũ
            SetupWaterOverlay();

            // Thiết lập các nút lựa chọn hội thoại
            SetupChoiceButtons();

            // Thiết lập Banner thông báo chuyển Giai đoạn
            SetupPhaseAnnouncementUI();

            // Thiết lập coin text ở đáy bảng hòm đồ
            SetupInventoryCoinsText();

#if UNITY_EDITOR
            // Tự động tải tài nguyên nếu thiếu để Designer không phải kéo thả
            if (seedItem == null)
            {
                seedItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Seed.asset");
            }
            if (incenseItem == null)
            {
                incenseItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Incense.asset");
            }
            if (noodlesItem == null)
            {
                noodlesItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Noodles.asset");
            }
#endif

            // Tạo các panel bổ sung
            CreateInventoryTitle();
            CreateCommunityPanel();
            CreateWeatherDetailsPanel();
            CreateInteractionPromptUI();
            CreateToastNotificationUI();
            CreateShopPanel();

            // Mặc định ẩn hòm đồ và khung hội thoại lúc khởi động
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (dialoguePanel != null) dialoguePanel.SetActive(false);

            // Kiểm tra trạng thái Menu chính để ẩn/hiện UI phù hợp
#if UNITY_2023_1_OR_NEWER
            FrameworkMainMenuUI mainMenu = FindAnyObjectByType<FrameworkMainMenuUI>();
#else
            FrameworkMainMenuUI mainMenu = FindObjectOfType<FrameworkMainMenuUI>();
#endif
            if (mainMenu != null && mainMenu.IsMenuOpen)
            {
                SetUIVisibility(false);
            }
            else
            {
                SetUIVisibility(true);
            }

            // Đăng ký sự kiện làm mới hòm đồ khi kho có thay đổi và sự kiện chuyển phase
            if (StorageManager.Instance != null)
            {
                StorageManager.Instance.OnStorageChanged += RefreshInventoryUI;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPhaseChanged += TriggerPhaseAnnouncement;
            }
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnPlayerAlert += ShowHUDToast;
            }
        }

        private void OnDestroy()
        {
            if (StorageManager.Instance != null)
            {
                StorageManager.Instance.OnStorageChanged -= RefreshInventoryUI;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPhaseChanged -= TriggerPhaseAnnouncement;
            }
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnPlayerAlert -= ShowHUDToast;
            }
        }

        private void Update()
        {
            // 1. Cập nhật các thanh chỉ số sinh lý
            UpdateSurvivalStats();

            // 2. Cập nhật đồng hồ và thời tiết miền Trung
            UpdateDateTimeAndWeather();

            // 2.5. Cập nhật lớp phủ viền màn hình nhiệt độ/lạnh
            UpdateVignetteOverlays();

            // Cập nhật cuộn sóng nước lũ
            UpdateWaterOverlay();

            // Cập nhật dữ liệu cho các Panel phụ khi chúng đang mở
            if (isCommunityOpen) UpdateCommunityPanelData();
            if (isWeatherDetailsOpen) UpdateWeatherDetailsPanelData();

            // 3. Lắng nghe phím bấm tương thích cả 2 Input System
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                // Toggle Inventory (I / Tab)
                if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame)
                {
                    ToggleInventory();
                }

                // Toggle Community Panel (C)
                if (Keyboard.current.cKey.wasPressedThisFrame)
                {
                    ToggleCommunityPanel();
                }

                // Toggle Weather Details Panel (M)
                if (Keyboard.current.mKey.wasPressedThisFrame)
                {
                    ToggleWeatherDetailsPanel();
                }

                if (isDialogueActive && !isChoiceActive && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
                {
                    CloseDialogue();
                }

                if (isChoiceActive)
                {
                    if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
                    {
                        SelectChoice(1);
                    }
                    else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
                    {
                        SelectChoice(2);
                    }
                    else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
                    {
                        SelectChoice(3);
                    }
                }
            }
#else
            // Toggle Inventory (I / Tab)
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
            }

            // Toggle Community Panel (C)
            if (Input.GetKeyDown(KeyCode.C))
            {
                ToggleCommunityPanel();
            }

            // Toggle Weather Details Panel (M)
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleWeatherDetailsPanel();
            }

            if (isDialogueActive && !isChoiceActive && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space)))
            {
                CloseDialogue();
            }

            if (isChoiceActive)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                {
                    SelectChoice(1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                {
                    SelectChoice(2);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                {
                    SelectChoice(3);
                }
            }
#endif
        }

        /// <summary>
        /// Sắp xếp lại cấu trúc và vị trí của các phần tử HUD để trông gọn gàng, đậm chất góc nhìn thứ 3.
        /// </summary>
        private void ReorganizeUILayout()
        {
            // Tìm font chữ chung từ SpeakerText để đồng bộ hóa
            TMP_FontAsset font = speakerNameText != null ? speakerNameText.font : null;

            // 1. Nhóm chỉ số Survival Bars (Bottom-Left)
            if (healthSlider != null)
            {
                RectTransform r = healthSlider.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 0f);
                r.anchorMax = new Vector2(0f, 0f);
                r.pivot = new Vector2(0f, 0f);
                r.anchoredPosition = new Vector2(25f, 95f);
                r.sizeDelta = new Vector2(220f, 15f);
            }
            if (staminaSlider != null)
            {
                RectTransform r = staminaSlider.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 0f);
                r.anchorMax = new Vector2(0f, 0f);
                r.pivot = new Vector2(0f, 0f);
                r.anchoredPosition = new Vector2(25f, 60f);
                r.sizeDelta = new Vector2(220f, 12f);
            }
            if (moraleSlider != null)
            {
                RectTransform r = moraleSlider.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 0f);
                r.anchorMax = new Vector2(0f, 0f);
                r.pivot = new Vector2(0f, 0f);
                r.anchoredPosition = new Vector2(25f, 25f);
                r.sizeDelta = new Vector2(220f, 12f);
            }

            // 2. Nhóm thông tin Thời gian (Top-Left)
            if (dayText != null)
            {
                dayText.fontSize = 16;
                dayText.alignment = TextAlignmentOptions.Left;
                RectTransform r = dayText.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 1f);
                r.anchorMax = new Vector2(0f, 1f);
                r.pivot = new Vector2(0f, 1f);
                r.anchoredPosition = new Vector2(25f, -25f);
                r.sizeDelta = new Vector2(250f, 25f);
            }
            if (timeText != null)
            {
                timeText.gameObject.SetActive(false); // Gộp chung vào dayText để hiển thị gọn
            }
            if (phaseText != null)
            {
                phaseText.fontSize = 13;
                phaseText.color = new Color(0.9f, 0.75f, 0.5f);
                phaseText.alignment = TextAlignmentOptions.Left;
                RectTransform r = phaseText.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 1f);
                r.anchorMax = new Vector2(0f, 1f);
                r.pivot = new Vector2(0f, 1f);
                r.anchoredPosition = new Vector2(25f, -50f);
                r.sizeDelta = new Vector2(250f, 20f);
            }

            // 3. Nhóm thông tin Thời tiết và Tài sản (Top-Right)
            if (weatherText != null)
            {
                weatherText.fontSize = 14;
                weatherText.alignment = TextAlignmentOptions.Right;
                RectTransform r = weatherText.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(1f, 1f);
                r.anchorMax = new Vector2(1f, 1f);
                r.pivot = new Vector2(1f, 1f);
                r.anchoredPosition = new Vector2(-25f, -25f);
                r.sizeDelta = new Vector2(350f, 25f);
            }

            // Tạo thêm text Coins ở Top-Right dưới WeatherText
            GameObject coinsObj = new GameObject("CoinsTopRightText", typeof(RectTransform), typeof(TextMeshProUGUI));
            coinsObj.transform.SetParent(this.transform, false);
            RectTransform coinsRect = coinsObj.GetComponent<RectTransform>();
            coinsRect.anchorMin = new Vector2(1f, 1f);
            coinsRect.anchorMax = new Vector2(1f, 1f);
            coinsRect.pivot = new Vector2(1f, 1f);
            coinsRect.anchoredPosition = new Vector2(-25f, -50f);
            coinsRect.sizeDelta = new Vector2(200f, 20f);

            coinsText = coinsObj.GetComponent<TextMeshProUGUI>();
            coinsText.alignment = TextAlignmentOptions.Right;
            coinsText.fontSize = 14;
            coinsText.color = new Color(0.95f, 0.8f, 0.3f, 1f); // Vàng rơm
            if (font != null) coinsText.font = font;

            // 4. Nhóm Hòm đồ (Inventory)
            if (inventoryPanel != null)
            {
                // Tinh chỉnh Inventory Panel ở giữa góc màn hình
                RectTransform r = inventoryPanel.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0.5f, 0.5f);
                r.anchorMax = new Vector2(0.5f, 0.5f);
                r.pivot = new Vector2(0.5f, 0.5f);
                r.anchoredPosition = new Vector2(0f, 0f);
                r.sizeDelta = new Vector2(400f, 320f);

                // Thêm nền bán trong suốt sang trọng
                Image img = inventoryPanel.GetComponent<Image>();
                if (img != null) img.color = new Color(0.12f, 0.1f, 0.08f, 0.95f);
            }
        }

        /// <summary>
        /// Ẩn hoặc hiển thị toàn bộ hệ thống UI Canvas thông qua CanvasGroup.
        /// </summary>
        public void SetUIVisibility(bool visible)
        {
            if (mainUICanvasGroup != null)
            {
                mainUICanvasGroup.alpha = visible ? 1f : 0f;
                mainUICanvasGroup.interactable = visible;
                mainUICanvasGroup.blocksRaycasts = visible;
            }
        }

        /// <summary>
        /// Đồng bộ 3 cột chỉ số cơ bản của người chơi.
        /// </summary>
        private void UpdateSurvivalStats()
        {
            if (PlayerStats.Instance == null) return;

            // Health (Máu) kèm hiệu ứng nhấp nháy đỏ báo động khi dưới 20%
            float health = PlayerStats.Instance.CurrentHealth;
            if (healthSlider != null)
            {
                healthSlider.value = health / 100f;
            }

            if (healthFillImage != null)
            {
                if (health < 20f)
                {
                    float pingPong = Mathf.PingPong(Time.time * 4f, 1f);
                    healthFillImage.color = Color.Lerp(new Color(0.65f, 0.23f, 0.17f), Color.red, pingPong);
                }
                else
                {
                    healthFillImage.color = new Color(0.65f, 0.23f, 0.17f); // Đỏ đất bazan mộc mạc
                }
            }

            // Thể lực (Stamina)
            if (staminaSlider != null)
            {
                staminaSlider.value = PlayerStats.Instance.CurrentStamina / 100f;
            }

            // Tinh thần (Morale)
            if (moraleSlider != null)
            {
                moraleSlider.value = PlayerStats.Instance.CurrentMorale / 100f;
            }
        }

        /// <summary>
        /// Đồng bộ thời gian ngày giờ và chu kỳ thời tiết bão lũ.
        /// </summary>
        private void UpdateDateTimeAndWeather()
        {
            if (GameManager.Instance != null)
            {
                int hour = Mathf.FloorToInt(GameManager.Instance.CurrentHour);
                int minute = Mathf.FloorToInt((GameManager.Instance.CurrentHour % 1) * 60);

                if (dayText != null)
                {
                    dayText.text = $"Ngày {GameManager.Instance.CurrentDay} | {hour:00}:{minute:00}";
                }

                if (phaseText != null)
                {
                    phaseText.text = $"Mùa: {GetPhaseVietnameseName(GameManager.Instance.CurrentPhase)}";
                }
            }

            if (WeatherManager.Instance != null)
            {
                string desc = GetWeatherDesc(WeatherManager.Instance.currentVisualWeather);
                if (weatherText != null)
                {
                    weatherText.text = $"{WeatherManager.Instance.Temperature:F1}°C | {desc}";
                }
            }

            // Cập nhật số tiền xu Top-Right
            if (coinsText != null && PlayerStats.Instance != null)
            {
                coinsText.text = $"{PlayerStats.Instance.Coins} Xu";
            }
        }

        #region PHÂN HỆ HỘI THOẠI CHẠY CHỮ (TYPEWRITER)

        public void ShowDialogue(string speaker, string content)
        {
            if (dialoguePanel == null) return;

            dialoguePanel.SetActive(true);
            isDialogueActive = true;
            isChoiceActive = false;

            if (choiceContainer != null)
            {
                choiceContainer.SetActive(false);
            }

            if (speakerNameText != null)
            {
                speakerNameText.text = speaker;
            }

            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }

            dialogueCoroutine = StartCoroutine(TypeDialogueCoroutine(content));
        }

        public void ShowDialogueWithChoices(string speaker, string content, 
            string choice1Label, System.Action onChoice1, 
            string choice2Label, System.Action onChoice2,
            string choice3Label = null, System.Action onChoice3 = null)
        {
            if (dialoguePanel == null) return;

            dialoguePanel.SetActive(true);
            isDialogueActive = true;
            isChoiceActive = true;

            // Đăng ký callback
            onChoice1Selected = onChoice1;
            onChoice2Selected = onChoice2;
            onChoice3Selected = onChoice3;

            if (speakerNameText != null)
            {
                speakerNameText.text = speaker;
            }

            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }

            dialogueCoroutine = StartCoroutine(TypeDialogueCoroutine(content));

            // Kích hoạt hiển thị nút lựa chọn
            if (choiceContainer == null)
            {
                SetupChoiceButtons();
            }

            if (choiceContainer != null)
            {
                choiceContainer.SetActive(true);

                // Điều chỉnh kích thước và bật/tắt nút 3 tùy thuộc vào nhãn
                RectTransform containerRect = choiceContainer.GetComponent<RectTransform>();
                if (!string.IsNullOrEmpty(choice3Label))
                {
                    if (containerRect != null) containerRect.sizeDelta = new Vector2(540f, 45f);
                    if (choice3Button != null) choice3Button.gameObject.SetActive(true);
                    if (choice3Text != null) choice3Text.text = $"3. {choice3Label}";
                }
                else
                {
                    if (containerRect != null) containerRect.sizeDelta = new Vector2(360f, 45f);
                    if (choice3Button != null) choice3Button.gameObject.SetActive(false);
                }
            }

            if (choice1Text != null) choice1Text.text = $"1. {choice1Label}";
            if (choice2Text != null) choice2Text.text = $"2. {choice2Label}";
        }

        private IEnumerator TypeDialogueCoroutine(string content)
        {
            if (dialogueContentText == null) yield break;

            dialogueContentText.text = "";
            foreach (char letter in content.ToCharArray())
            {
                dialogueContentText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
            dialogueCoroutine = null;
        }

        public void CloseDialogue()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
            isDialogueActive = false;
            isChoiceActive = false;
            
            if (choiceContainer != null)
            {
                choiceContainer.SetActive(false);
            }
        }

        #endregion

        #region PHÂN HỆ HÒM ĐỒ DẠNG LƯỚI

        public void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(isInventoryOpen);
                if (isInventoryOpen)
                {
                    // Đóng các panel khác
                    CloseCommunityPanel();
                    CloseWeatherDetailsPanel();

                    RefreshInventoryUI();
                }
            }
        }

        public void RefreshInventoryUI()
        {
            if (itemSlotContainer == null || itemSlotPrefab == null || StorageManager.Instance == null) return;

            // Xóa sạch ô lưới cũ tránh bị trùng lặp
            foreach (Transform child in itemSlotContainer)
            {
                Destroy(child.gameObject);
            }

            // Tạo các ô lưới mới
            var slots = StorageManager.Instance.GetStorageSlots();
            foreach (var slot in slots)
            {
                GameObject newSlot = Instantiate(itemSlotPrefab, itemSlotContainer);

                // Căn chỉnh kích thước ô vật phẩm thành hình vuông
                RectTransform slotRect = newSlot.GetComponent<RectTransform>();
                if (slotRect != null)
                {
                    slotRect.sizeDelta = new Vector2(80f, 80f);
                }

                // Gán hình ảnh biểu tượng vật phẩm và căn chỉnh chiếm trọn ô vuông
                Transform iconTr = newSlot.transform.Find("Icon");
                if (iconTr != null)
                {
                    RectTransform iconRect = iconTr.GetComponent<RectTransform>();
                    if (iconRect != null)
                    {
                        iconRect.anchorMin = Vector2.zero;
                        iconRect.anchorMax = Vector2.one;
                        iconRect.pivot = new Vector2(0.5f, 0.5f);
                        iconRect.offsetMin = Vector2.zero;
                        iconRect.offsetMax = Vector2.zero;
                    }

                    Image img = iconTr.GetComponent<Image>();
                    if (img != null)
                    {
                        if (slot.item.Icon != null)
                        {
                            img.sprite = slot.item.Icon;
                            img.color = Color.white;
                            img.enabled = true;
                        }
                        else
                        {
                            img.sprite = null;
                            img.color = new Color(0.35f, 0.28f, 0.22f, 0.8f); // Nâu tối trung tính
                            img.enabled = true;
                        }
                    }
                }

                // Căn chỉnh chữ hiển thị số lượng ở góc dưới bên phải
                TextMeshProUGUI txt = newSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = $"x{slot.quantity}";
                    txt.fontSize = 11;
                    txt.alignment = TextAlignmentOptions.BottomRight;
                    txt.color = new Color(0.95f, 0.9f, 0.3f, 1f); // Màu vàng gold nổi bật

                    RectTransform txtRect = txt.GetComponent<RectTransform>();
                    if (txtRect != null)
                    {
                        txtRect.anchorMin = new Vector2(0f, 0f);
                        txtRect.anchorMax = new Vector2(1f, 1f);
                        txtRect.pivot = new Vector2(1f, 0f);
                        txtRect.offsetMin = new Vector2(5f, 5f);
                        txtRect.offsetMax = new Vector2(-5f, -5f);
                    }
                }

                // Thêm Button Component để nhận click
                Button btn = newSlot.GetComponent<Button>();
                if (btn == null)
                {
                    btn = newSlot.AddComponent<Button>();
                }
                
                ItemData currentItem = slot.item;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnItemSlotClicked(currentItem));
            }

            // Cập nhật số lượng tiền xu hiện có của gia đình
            if (inventoryCoinsText != null && PlayerStats.Instance != null)
            {
                inventoryCoinsText.text = $"Tài sản gia đình: <color=#F4D03F>{PlayerStats.Instance.Coins} Xu</color>";
            }
        }

        /// <summary>
        /// Tạo cấu trúc ScrollView và GridLayoutGroup cho Hòm đồ (Inventory) để xếp ô vuông và cuộn chuột.
        /// </summary>
        private void SetupInventoryScrollView()
        {
            if (inventoryPanel == null || itemSlotContainer == null) return;

            // Kiểm tra xem đã thiết lập Viewport chưa
            if (inventoryPanel.transform.Find("Viewport") != null) return;

            // 1. Tạo đối tượng Viewport làm mặt nạ che phần tràn
            GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            viewportObj.transform.SetParent(inventoryPanel.transform, false);

            RectTransform viewRect = viewportObj.GetComponent<RectTransform>();
            viewRect.anchorMin = new Vector2(0f, 0f);
            viewRect.anchorMax = new Vector2(1f, 1f);
            viewRect.pivot = new Vector2(0.5f, 0.5f);
            viewRect.offsetMin = new Vector2(25f, 60f);  // Chừa lề dưới cho tiền xu
            viewRect.offsetMax = new Vector2(-25f, -70f); // Chừa lề trên cho tiêu đề

            // Làm trong suốt Viewport
            viewportObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

            // 2. Chuyển container hòm đồ vào làm con của Viewport
            itemSlotContainer.SetParent(viewportObj.transform, false);

            // 3. Gắn ScrollRect vào inventoryPanel để nhận sự kiện cuộn chuột
            ScrollRect scroll = inventoryPanel.GetComponent<ScrollRect>();
            if (scroll == null)
            {
                scroll = inventoryPanel.AddComponent<ScrollRect>();
            }
            scroll.content = itemSlotContainer.GetComponent<RectTransform>();
            scroll.viewport = viewRect;
            scroll.horizontal = false; // Tắt cuộn ngang
            scroll.vertical = true;   // Bật cuộn dọc
            scroll.scrollSensitivity = 25f;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            // 4. Thiết lập GridLayoutGroup cho container để tự động xếp các ô
            GridLayoutGroup grid = itemSlotContainer.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = itemSlotContainer.gameObject.AddComponent<GridLayoutGroup>();
            }

            // Gỡ bỏ các component layout cũ để tránh xung đột
            var vertical = itemSlotContainer.GetComponent<VerticalLayoutGroup>();
            if (vertical != null) Destroy(vertical);
            var horizontal = itemSlotContainer.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null) Destroy(horizontal);
            var layoutElement = itemSlotContainer.GetComponent<LayoutElement>();
            if (layoutElement != null) Destroy(layoutElement);

            grid.cellSize = new Vector2(80f, 80f); // Mỗi ô vuông kích thước 80x80
            grid.spacing = new Vector2(12f, 12f);  // Khoảng cách giữa các ô
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;

            // 5. Thêm ContentSizeFitter để tự động tăng chiều cao container theo số hàng
            ContentSizeFitter fitter = itemSlotContainer.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = itemSlotContainer.gameObject.AddComponent<ContentSizeFitter>();
            }
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            // Thiết lập RectTransform của container
            RectTransform containerRect = itemSlotContainer.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0f, 1f); // Neo lên đỉnh
            containerRect.anchorMax = new Vector2(1f, 1f);
            containerRect.pivot = new Vector2(0.5f, 1f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(0f, 200f);
        }

        public void ActionCraftPreservedItem()
        {
            if (StorageManager.Instance == null || freshCropItem == null || preservedCropItem == null) return;

            bool success = StorageManager.Instance.CraftPreservedItem(freshCropItem, preservedCropItem, 1);
            if (success)
            {
                RefreshInventoryUI();
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi click vào một ô vật phẩm trong hòm đồ.
        /// </summary>
        private void OnItemSlotClicked(ItemData item)
        {
            if (item == null) return;

            // Nếu vật phẩm có thể tiêu thụ trực tiếp (hồi Stamina hoặc Morale) và không phải là Nhang/Hạt giống
            if ((item.StaminaRestoreValue > 0f || item.MoraleRestoreValue > 0f) && 
                item.type != ItemType.Incense && item.type != ItemType.HatGiong)
            {
                string usageText = $"Bạn có muốn sử dụng 1 {item.ItemName} không?\n\nHiệu quả:";
                if (item.StaminaRestoreValue > 0f) usageText += $"\n• +{item.StaminaRestoreValue} Thể lực";
                if (item.MoraleRestoreValue > 0f) usageText += $"\n• +{item.MoraleRestoreValue} Tinh thần";

                ShowDialogueWithChoices(
                    "Hành lý gia đình",
                    usageText,
                    "Sử dụng",
                    () => {
                        if (PlayerStats.Instance != null)
                        {
                            PlayerStats.Instance.UseItem(item);
                        }
                        RefreshInventoryUI();
                    },
                    "Hủy bỏ",
                    () => {
                        CloseDialogue();
                    }
                );
            }
            else
            {
                // Đối với các vật phẩm không dùng trực tiếp (Nhang, Hạt giống, Vật liệu...)
                string descText = $"{item.Description}";
                if (item.type == ItemType.Incense)
                {
                    descText += "\n\n<color=#F4D03F>Hướng dẫn:</color> Nhang nên được mang đến Bàn thờ Gia tiên hoặc Am thờ Thổ Địa ngoài vườn để thắp cúng.";
                }
                else if (item.type == ItemType.HatGiong)
                {
                    descText += "\n\n<color=#F4D03F>Hướng dẫn:</color> Hạt giống dùng gieo trực tiếp lên các ô ruộng đất trống đã dọn sạch sỏi đá và tưới ẩm.";
                }
                else if (item.type == ItemType.VatLieu)
                {
                    descText += "\n\n<color=#F4D03F>Hướng dẫn:</color> Vật liệu chằng chống nhà. Dân làng sẽ tự động dùng khi bão lụt đổ bộ.";
                }

                ShowDialogue(item.ItemName, descText);
            }
        }

        #endregion

        #region PHÂN HỆ LỚP PHỦ VIỀN MÀN HÌNH (VIGNETTE OVERLAYS)

        private void SetupVignetteOverlays()
        {
            Sprite vignetteSprite = CreateProceduralVignetteSprite();

            // 1. Tạo lớp phủ Sốc Nhiệt (Heat Overlay)
            GameObject heatObj = new GameObject("HeatOverlay", typeof(RectTransform), typeof(Image));
            heatObj.transform.SetParent(this.transform, false);
            heatObj.transform.SetAsFirstSibling();
            
            RectTransform heatRect = heatObj.GetComponent<RectTransform>();
            heatRect.anchorMin = Vector2.zero;
            heatRect.anchorMax = Vector2.one;
            heatRect.offsetMin = Vector2.zero;
            heatRect.offsetMax = Vector2.zero;

            heatOverlayImage = heatObj.GetComponent<Image>();
            heatOverlayImage.sprite = vignetteSprite;
            heatOverlayImage.color = new Color(heatColor.r, heatColor.g, heatColor.b, 0f);
            heatOverlayImage.raycastTarget = false;

            // 2. Tạo lớp phủ Cảm Lạnh (Cold Overlay)
            GameObject coldObj = new GameObject("ColdOverlay", typeof(RectTransform), typeof(Image));
            coldObj.transform.SetParent(this.transform, false);
            coldObj.transform.SetAsFirstSibling();
            
            RectTransform coldRect = coldObj.GetComponent<RectTransform>();
            coldRect.anchorMin = Vector2.zero;
            coldRect.anchorMax = Vector2.one;
            coldRect.offsetMin = Vector2.zero;
            coldRect.offsetMax = Vector2.zero;

            coldOverlayImage = coldObj.GetComponent<Image>();
            coldOverlayImage.sprite = vignetteSprite;
            coldOverlayImage.color = new Color(coldColor.r, coldColor.g, coldColor.b, 0f);
            coldOverlayImage.raycastTarget = false;
        }

        private Sprite CreateProceduralVignetteSprite()
        {
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                    float alpha = Mathf.Clamp01(dist * dist * 1.5f); 
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            tex.wrapMode = TextureWrapMode.Clamp;

            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
        }

        private void UpdateVignetteOverlays()
        {
            if (PlayerStats.Instance == null) return;

            float heatStress = PlayerStats.Instance.HeatStress;
            float coldStress = PlayerStats.Instance.ColdStress;

            float pulse = 1f;
            if (heatStress > 70f || coldStress > 70f)
            {
                pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.15f;
            }

            float targetHeatAlpha = (heatStress / 100f) * maxOverlayAlpha * pulse;
            float targetColdAlpha = (coldStress / 100f) * maxOverlayAlpha * pulse;

            if (heatOverlayImage != null)
            {
                Color c = heatOverlayImage.color;
                c.a = Mathf.Lerp(c.a, targetHeatAlpha, Time.deltaTime * 3f);
                heatOverlayImage.color = c;
            }

            if (coldOverlayImage != null)
            {
                Color c = coldOverlayImage.color;
                c.a = Mathf.Lerp(c.a, targetColdAlpha, Time.deltaTime * 3f);
                coldOverlayImage.color = c;
            }
        }

        private void SetupWaterOverlay()
        {
            if (waterWavesSprite == null) return;

            GameObject waterObj = new GameObject("WaterOverlay", typeof(RectTransform), typeof(Image));
            waterObj.transform.SetParent(this.transform, false);
            
            waterObj.transform.SetAsFirstSibling();
            int siblingIndex = 0;
            if (heatOverlayImage != null) siblingIndex++;
            if (coldOverlayImage != null) siblingIndex++;
            waterObj.transform.SetSiblingIndex(siblingIndex);

            waterOverlayRect = waterObj.GetComponent<RectTransform>();
            waterOverlayRect.anchorMin = Vector2.zero;
            waterOverlayRect.anchorMax = Vector2.one;
            waterOverlayRect.offsetMin = new Vector2(-128f, -128f);
            waterOverlayRect.offsetMax = new Vector2(128f, 128f);

            waterOverlayImage = waterObj.GetComponent<Image>();
            waterOverlayImage.sprite = waterWavesSprite;
            waterOverlayImage.type = Image.Type.Tiled;
            waterOverlayImage.color = new Color(0.35f, 0.6f, 0.8f, 0f);
            waterOverlayImage.raycastTarget = false;
        }

        private void UpdateWaterOverlay()
        {
            if (waterOverlayImage == null || WeatherManager.Instance == null) return;

            float floodLevel = WeatherManager.Instance.FloodLevel;
            float targetAlpha = Mathf.Clamp01(floodLevel / 2.5f) * maxWaterAlpha;

            Color c = waterOverlayImage.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 2f);
            waterOverlayImage.color = c;

            if (c.a > 0.01f && waterOverlayRect != null)
            {
                float xOffset = Mathf.Repeat(Time.time * 20f, 128f) - 64f;
                float yOffset = Mathf.Repeat(Time.time * -15f, 128f) - 64f;
                waterOverlayRect.anchoredPosition = new Vector2(xOffset, yOffset);
            }
        }

        #endregion

        #region PHÂN HỆ LỰA CHỌN ĐỐI THOẠI (DIALOGUE CHOICES SETUP)

        private void SetupChoiceButtons()
        {
            if (dialoguePanel == null) return;

            choiceContainer = new GameObject("ChoiceContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            choiceContainer.transform.SetParent(dialoguePanel.transform, false);

            RectTransform containerRect = choiceContainer.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(1f, 0.5f);
            containerRect.anchorMax = new Vector2(1f, 0.5f);
            containerRect.pivot = new Vector2(1f, 0.5f);
            containerRect.anchoredPosition = new Vector2(-30f, 0f);
            containerRect.sizeDelta = new Vector2(360f, 45f);

            HorizontalLayoutGroup layout = choiceContainer.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 15f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            TMP_FontAsset font = speakerNameText != null ? speakerNameText.font : null;

            GameObject btn1Obj = CreateProceduralButton("Choice1Button", "1. Lựa chọn 1", font, out choice1Button, out choice1Text);
            btn1Obj.transform.SetParent(choiceContainer.transform, false);
            choice1Button.onClick.AddListener(() => SelectChoice(1));

            GameObject btn2Obj = CreateProceduralButton("Choice2Button", "2. Lựa chọn 2", font, out choice2Button, out choice2Text);
            btn2Obj.transform.SetParent(choiceContainer.transform, false);
            choice2Button.onClick.AddListener(() => SelectChoice(2));

            GameObject btn3Obj = CreateProceduralButton("Choice3Button", "3. Lựa chọn 3", font, out choice3Button, out choice3Text);
            btn3Obj.transform.SetParent(choiceContainer.transform, false);
            choice3Button.onClick.AddListener(() => SelectChoice(3));

            choiceContainer.SetActive(false);
        }

        private GameObject CreateProceduralButton(string name, string text, TMP_FontAsset font, out Button button, out TextMeshProUGUI textComponent)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            
            Image img = btnObj.GetComponent<Image>();
            img.color = new Color(0.2f, 0.14f, 0.08f, 1f); // Nâu gỗ đậm

            button = btnObj.GetComponent<Button>();
            
            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            textComponent = textObj.GetComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontSize = 13;
            textComponent.color = new Color(0.95f, 0.9f, 0.85f, 1f); // Trắng ngà rơm ấm áp
            if (font != null)
            {
                textComponent.font = font;
            }

            return btnObj;
        }

        private void SelectChoice(int choiceIndex)
        {
            if (!isChoiceActive) return;

            isChoiceActive = false;
            if (choiceContainer != null)
            {
                choiceContainer.SetActive(false);
            }

            if (choiceIndex == 1)
            {
                onChoice1Selected?.Invoke();
            }
            else if (choiceIndex == 2)
            {
                onChoice2Selected?.Invoke();
            }
            else if (choiceIndex == 3)
            {
                onChoice3Selected?.Invoke();
            }
        }

        private void SetupPhaseAnnouncementUI()
        {
            phaseAnnouncementObj = new GameObject("PhaseAnnouncementPanel", typeof(RectTransform), typeof(CanvasGroup));
            phaseAnnouncementObj.transform.SetParent(this.transform, false);
            
            RectTransform panelRect = phaseAnnouncementObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0.4f);
            panelRect.anchorMax = new Vector2(1f, 0.6f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            phaseAnnouncementCanvasGroup = phaseAnnouncementObj.GetComponent<CanvasGroup>();
            phaseAnnouncementCanvasGroup.alpha = 0f;
            phaseAnnouncementCanvasGroup.interactable = false;
            phaseAnnouncementCanvasGroup.blocksRaycasts = false;

            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(phaseAnnouncementObj.transform, false);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.65f);

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(phaseAnnouncementObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 10f);
            textRect.offsetMax = new Vector2(-20f, -10f);

            phaseAnnouncementText = textObj.GetComponent<TextMeshProUGUI>();
            phaseAnnouncementText.alignment = TextAlignmentOptions.Center;
            phaseAnnouncementText.fontSize = 24;
            phaseAnnouncementText.fontStyle = FontStyles.Bold;
            phaseAnnouncementText.color = new Color(0.95f, 0.8f, 0.3f, 1f);
            if (speakerNameText != null)
            {
                phaseAnnouncementText.font = speakerNameText.font;
            }
        }

        private void TriggerPhaseAnnouncement(GamePhase newPhase)
        {
            if (announcementCoroutine != null)
            {
                StopCoroutine(announcementCoroutine);
            }
            announcementCoroutine = StartCoroutine(PhaseAnnouncementCoroutine(newPhase));
        }

        private IEnumerator PhaseAnnouncementCoroutine(GamePhase newPhase)
        {
            if (phaseAnnouncementText == null || phaseAnnouncementCanvasGroup == null) yield break;

            string phaseNumber = "";
            string phaseName = "";
            switch (newPhase)
            {
                case GamePhase.LapNghiep:
                    phaseNumber = "GIAI ĐOẠN 1";
                    phaseName = "TIẾNG TRỐNG ĐÌNH LÀNG (LẬP NGHIỆP)";
                    break;
                case GamePhase.GioLao:
                    phaseNumber = "GIAI ĐOẠN 2";
                    phaseName = "NẮNG CHÁY GIÓ LÀO (THỬ THÁCH)";
                    break;
                case GamePhase.MuaBao:
                    phaseNumber = "GIAI ĐOẠN 3";
                    phaseName = "TÌNH NGƯỜI TRONG BÃO LŨ (SINH TỬ)";
                    break;
                case GamePhase.PhuSa:
                    phaseNumber = "GIAI ĐOẠN 4";
                    phaseName = "PHÙ SA SAU CƠN LŨ (TÁI THIẾT)";
                    break;
            }

            phaseAnnouncementText.text = $"<size=18>{phaseNumber}</size>\n<color=#F4D03F>{phaseName}</color>";
            
            float elapsed = 0f;
            float duration = 1.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                phaseAnnouncementCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            phaseAnnouncementCanvasGroup.alpha = 1f;

            yield return new WaitForSeconds(3.5f);

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                phaseAnnouncementCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            phaseAnnouncementCanvasGroup.alpha = 0f;
            announcementCoroutine = null;
        }

        private void SetupInventoryCoinsText()
        {
            if (inventoryPanel == null) return;

            GameObject coinsTextObj = new GameObject("CoinsText", typeof(RectTransform), typeof(TextMeshProUGUI));
            coinsTextObj.transform.SetParent(inventoryPanel.transform, false);
            RectTransform coinsRect = coinsTextObj.GetComponent<RectTransform>();
            coinsRect.anchorMin = new Vector2(0f, 0f);
            coinsRect.anchorMax = new Vector2(1f, 0f);
            coinsRect.pivot = new Vector2(0.5f, 0f);
            coinsRect.anchoredPosition = new Vector2(0f, 15f);
            coinsRect.sizeDelta = new Vector2(300f, 30f);

            inventoryCoinsText = coinsTextObj.GetComponent<TextMeshProUGUI>();
            inventoryCoinsText.alignment = TextAlignmentOptions.Center;
            inventoryCoinsText.fontSize = 14;
            inventoryCoinsText.color = new Color(0.95f, 0.9f, 0.85f, 1f);
            if (speakerNameText != null)
            {
                inventoryCoinsText.font = speakerNameText.font;
            }
        }

        private void CreateInventoryTitle()
        {
            if (inventoryPanel == null) return;
            TMP_FontAsset font = speakerNameText != null ? speakerNameText.font : null;

            GameObject titleObj = new GameObject("InventoryTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(inventoryPanel.transform, false);

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
            titleRect.sizeDelta = new Vector2(360f, 30f);

            TextMeshProUGUI title = titleObj.GetComponent<TextMeshProUGUI>();
            title.text = "<b>TÍCH CÓP PHÒNG CƠ</b>";
            title.alignment = TextAlignmentOptions.Center;
            title.fontSize = 16;
            title.color = new Color(0.95f, 0.8f, 0.3f, 1f);
            if (font != null) title.font = font;
        }

        #endregion

        #region TẠO VÀ QUẢN LÝ CÁC PANEL MỚI CHO GÓC NHÌN THỨ 3

        // --- GỢI Ý TƯƠNG TÁC (INTERACTION PROMPT) ---
        private void CreateInteractionPromptUI()
        {
            TMP_FontAsset font = speakerNameText != null ? speakerNameText.font : null;

            GameObject promptObj = new GameObject("InteractionPrompt", typeof(RectTransform), typeof(TextMeshProUGUI));
            promptObj.transform.SetParent(this.transform, false);

            RectTransform r = promptObj.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.2f); // Ở góc dưới chính giữa
            r.anchorMax = new Vector2(0.5f, 0.2f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0f, 0f);
            r.sizeDelta = new Vector2(500f, 40f);

            interactionPromptText = promptObj.GetComponent<TextMeshProUGUI>();
            interactionPromptText.alignment = TextAlignmentOptions.Center;
            interactionPromptText.fontSize = 15;
            interactionPromptText.fontStyle = FontStyles.Bold;
            interactionPromptText.color = new Color(0.95f, 0.9f, 0.8f, 1f);
            if (font != null) interactionPromptText.font = font;

            // Thêm viền chữ cho nổi bật trên môi trường 3D
            interactionPromptText.outlineColor = Color.black;
            interactionPromptText.outlineWidth = 0.2f;

            interactionPromptText.text = ""; // Ẩn mặc định
        }

        public void SetInteractionPrompt(string prompt)
        {
            if (interactionPromptText != null)
            {
                interactionPromptText.text = prompt;
            }
        }

        // --- HỆ THỐNG THÔNG BÁO HUD TOAST ---
        private void CreateToastNotificationUI()
        {
            TMP_FontAsset font = speakerNameText != null ? speakerNameText.font : null;

            // 1. Toast Panel (Container with semi-transparent background and CanvasGroup)
            toastPanel = new GameObject("HUDToastNotification", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            toastPanel.transform.SetParent(this.transform, false);

            RectTransform r = toastPanel.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.82f); // Upper middle
            r.anchorMax = new Vector2(0.5f, 0.82f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0f, 0f);
            r.sizeDelta = new Vector2(450f, 45f);

            Image bgImg = toastPanel.GetComponent<Image>();
            bgImg.color = new Color(0.12f, 0.1f, 0.08f, 0.85f); // Semi-transparent dark

            toastCanvasGroup = toastPanel.GetComponent<CanvasGroup>();
            toastCanvasGroup.alpha = 0f; // Hidden initially
            toastCanvasGroup.interactable = false;
            toastCanvasGroup.blocksRaycasts = false;

            // 2. Toast Text (Child of panel)
            GameObject textObj = new GameObject("ToastText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(toastPanel.transform, false);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15f, 5f);
            textRect.offsetMax = new Vector2(-15f, -5f);

            toastText = textObj.GetComponent<TextMeshProUGUI>();
            toastText.alignment = TextAlignmentOptions.Center;
            toastText.fontSize = 14;
            toastText.fontStyle = FontStyles.Bold;
            toastText.color = new Color(0.95f, 0.85f, 0.4f, 1f); // Warm yellow/beige text
            if (font != null) toastText.font = font;

            toastText.outlineColor = Color.black;
            toastText.outlineWidth = 0.15f;
            toastText.text = "";
        }

        public void ShowHUDToast(string message)
        {
            if (toastPanel == null || toastText == null || toastCanvasGroup == null) return;

            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
            }
            toastCoroutine = StartCoroutine(HUDToastCoroutine(message));
        }

        private IEnumerator HUDToastCoroutine(string message)
        {
            toastText.text = message;

            float elapsed = 0f;
            float fadeDuration = 0.25f;

            // Fade in
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                toastCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            toastCanvasGroup.alpha = 1f;

            // Wait visible
            yield return new WaitForSeconds(2.5f);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeDuration * 2f) // Fade out slightly slower
            {
                elapsed += Time.deltaTime;
                toastCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / (fadeDuration * 2f));
                yield return null;
            }
            toastCanvasGroup.alpha = 0f;
            toastCoroutine = null;
        }

        // --- BẢNG ĐỒNG BÀO LÀNG XÓM (COMMUNITY PANEL - PHÍM C) ---
        private void CreateCommunityPanel()
        {
            TMP_FontAsset font = speakerNameText != null ? speakerNameText.font : null;

            communityPanel = new GameObject("Panel_Community", typeof(RectTransform), typeof(Image));
            communityPanel.transform.SetParent(this.transform, false);

            RectTransform r = communityPanel.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.5f);
            r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0f, 0f);
            r.sizeDelta = new Vector2(400f, 320f);

            Image bg = communityPanel.GetComponent<Image>();
            bg.color = new Color(0.12f, 0.1f, 0.08f, 0.95f); // Nâu tối mộc mạc

            // Chữ tiêu đề
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(communityPanel.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
            titleRect.sizeDelta = new Vector2(360f, 30f);

            TextMeshProUGUI title = titleObj.GetComponent<TextMeshProUGUI>();
            title.text = "<b>TÌNH NGHĨA ĐỒNG BÀO XÓM GIỀNG</b>";
            title.alignment = TextAlignmentOptions.Center;
            title.fontSize = 16;
            title.color = new Color(0.95f, 0.8f, 0.3f, 1f);
            if (font != null) title.font = font;

            // Chữ nội dung
            GameObject contentObj = new GameObject("ContentText", typeof(RectTransform), typeof(TextMeshProUGUI));
            contentObj.transform.SetParent(communityPanel.transform, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(25f, 25f);
            contentRect.offsetMax = new Vector2(-25f, -60f);

            communityText = contentObj.GetComponent<TextMeshProUGUI>();
            communityText.alignment = TextAlignmentOptions.TopLeft;
            communityText.fontSize = 13;
            communityText.lineSpacing = 10f;
            communityText.color = new Color(0.95f, 0.9f, 0.85f, 1f);
            if (font != null) communityText.font = font;

            communityPanel.SetActive(false); // Ẩn mặc định
        }

        public void ToggleCommunityPanel()
        {
            isCommunityOpen = !isCommunityOpen;
            if (communityPanel != null)
            {
                communityPanel.SetActive(isCommunityOpen);
                if (isCommunityOpen)
                {
                    // Đóng các panel khác
                    if (isInventoryOpen) ToggleInventory();
                    CloseWeatherDetailsPanel();

                    UpdateCommunityPanelData();
                }
            }
        }

        private void CloseCommunityPanel()
        {
            isCommunityOpen = false;
            if (communityPanel != null) communityPanel.SetActive(false);
        }

        private void UpdateCommunityPanelData()
        {
            if (communityText == null) return;

            string info = "";

#if UNITY_2023_1_OR_NEWER
            NPCCharacter[] npcs = FindObjectsByType<NPCCharacter>(FindObjectsInactive.Exclude);
#else
            NPCCharacter[] npcs = FindObjectsOfType<NPCCharacter>();
#endif
            NPCCharacter bacNam = System.Array.Find(npcs, n => n.characterType == NPCCharacter.StoryCharacterType.BacNam);
            NPCCharacter oTham = System.Array.Find(npcs, n => n.characterType == NPCCharacter.StoryCharacterType.OTham);

            int vanCongBacNam = bacNam != null ? bacNam.VanCongCredits : 0;
            int affectionBacNam = bacNam != null ? bacNam.Affection : 0;

            int vanCongOTham = oTham != null ? oTham.VanCongCredits : 0;
            int affectionOTham = oTham != null ? oTham.Affection : 0;

            info += $"<b>Bác Năm (Láng giềng thân hữu):</b>\n";
            info += $"  • Cảm tình: <color=#F4D03F>{affectionBacNam} điểm</color>\n";
            info += $"  • Ngày công vần công: {vanCongBacNam} công\n\n";

            info += $"<b>O Thắm (Đại lý hạt giống):</b>\n";
            info += $"  • Cảm tình: <color=#F4D03F>{affectionOTham} điểm</color>\n";
            info += $"  • Ngày công vần công: {vanCongOTham} công\n\n";

            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.MuaBao)
            {
                info += "<color=red><b>BÃO LŨ:</b> Đang trong mùa lũ! Tình làng nghĩa xóm sẽ giúp chằng chống nhà cửa tốt hơn.</color>";
            }
            else
            {
                info += "Hãy đến phụ giúp hàng xóm (phím [E]) để cùng nhau chống bão lũ!";
            }

            communityText.text = info;
        }

        // --- BẢNG THÔNG TIN THỜI TIẾT CHI TIẾT (WEATHER DETAILS PANEL - PHÍM M) ---
        private void CreateWeatherDetailsPanel()
        {
            TMP_FontAsset font = speakerNameText != null ? speakerNameText.font : null;

            weatherDetailsPanel = new GameObject("Panel_WeatherDetails", typeof(RectTransform), typeof(Image));
            weatherDetailsPanel.transform.SetParent(this.transform, false);

            RectTransform r = weatherDetailsPanel.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.5f);
            r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0f, 0f);
            r.sizeDelta = new Vector2(400f, 320f);

            Image bg = weatherDetailsPanel.GetComponent<Image>();
            bg.color = new Color(0.12f, 0.1f, 0.08f, 0.95f); // Nâu tối

            // Tiêu đề
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(weatherDetailsPanel.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
            titleRect.sizeDelta = new Vector2(360f, 30f);

            TextMeshProUGUI title = titleObj.GetComponent<TextMeshProUGUI>();
            title.text = "<b>ĐÀI KHÍ TƯỢNG VÀ THIÊN TAI</b>";
            title.alignment = TextAlignmentOptions.Center;
            title.fontSize = 16;
            title.color = new Color(0.95f, 0.8f, 0.3f, 1f);
            if (font != null) title.font = font;

            // Nội dung
            GameObject contentObj = new GameObject("ContentText", typeof(RectTransform), typeof(TextMeshProUGUI));
            contentObj.transform.SetParent(weatherDetailsPanel.transform, false);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(25f, 25f);
            contentRect.offsetMax = new Vector2(-25f, -60f);

            weatherDetailsText = contentObj.GetComponent<TextMeshProUGUI>();
            weatherDetailsText.alignment = TextAlignmentOptions.TopLeft;
            weatherDetailsText.fontSize = 13;
            weatherDetailsText.lineSpacing = 10f;
            weatherDetailsText.color = new Color(0.95f, 0.9f, 0.85f, 1f);
            if (font != null) weatherDetailsText.font = font;

            weatherDetailsPanel.SetActive(false); // Ẩn mặc định
        }

        public void ToggleWeatherDetailsPanel()
        {
            isWeatherDetailsOpen = !isWeatherDetailsOpen;
            if (weatherDetailsPanel != null)
            {
                weatherDetailsPanel.SetActive(isWeatherDetailsOpen);
                if (isWeatherDetailsOpen)
                {
                    // Đóng các panel khác
                    if (isInventoryOpen) ToggleInventory();
                    CloseCommunityPanel();

                    UpdateWeatherDetailsPanelData();
                }
            }
        }

        private void CloseWeatherDetailsPanel()
        {
            isWeatherDetailsOpen = false;
            if (weatherDetailsPanel != null) weatherDetailsPanel.SetActive(false);
        }

        private void UpdateWeatherDetailsPanelData()
        {
            if (weatherDetailsText == null || WeatherManager.Instance == null || PlayerStats.Instance == null) return;

            string info = "";
            string weatherState = GetWeatherDesc(WeatherManager.Instance.currentVisualWeather);

            info += $"<b>Dự báo thời tiết:</b> {weatherState}\n";
            info += $"  • Nhiệt độ môi trường: <color=#F4D03F>{WeatherManager.Instance.Temperature:F1}°C</color>\n";
            info += $"  • Độ ẩm không khí: {WeatherManager.Instance.Humidity:F0}%\n";
            info += $"  • Cường độ sức gió: {WeatherManager.Instance.WindSpeed:F1} km/h\n";
            info += $"  • Mực nước lũ lụt: {(WeatherManager.Instance.FloodLevel > 0f ? $"<color=cyan>{WeatherManager.Instance.FloodLevel:F2} mét</color>" : "Bình thường (Chưa ngập)")}\n\n";

            info += "<b>Thể trạng cơ thể Thành:</b>\n";
            info += $"  • Stress Nhiệt (Gió Lào): {PlayerStats.Instance.HeatStress:F0}%\n";
            info += $"  • Stress Lạnh (Mưa lũ): {PlayerStats.Instance.ColdStress:F0}%\n\n";

            if (PlayerStats.Instance.HeatStress > 70f)
            {
                info += "<color=red><b>CẢNH BÁO:</b> Cơ thể đang bị sốc nhiệt cực độ! Hãy vào bóng râm.</color>";
            }
            else if (PlayerStats.Instance.ColdStress > 70f)
            {
                info += "<color=cyan><b>CẢNH BÁO:</b> Cơ thể đang bị nhiễm lạnh! Hãy thắp nhang sưởi ấm.</color>";
            }
            else
            {
                info += "Thể trạng Thành đang ở mức an toàn ổn định.";
            }

            weatherDetailsText.text = info;
        }

        #endregion

        private string GetPhaseVietnameseName(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.LapNghiep: return "Tiếng Trống Đình Làng (Lập Nghiệp)";
                case GamePhase.GioLao: return "Nắng Cháy Gió Lào (Gió Tây Nam)";
                case GamePhase.MuaBao: return "Tình Người Trong Lũ (Mưa Bão)";
                case GamePhase.PhuSa: return "Phù Sa Sau Lũ (Cải Tạo Tái Thiết)";
                default: return phase.ToString();
            }
        }

        private string GetWeatherDesc(WeatherType type)
        {
            switch (type)
            {
                case WeatherType.OnDinh: return "Bình thường";
                case WeatherType.GioLao: return "Nắng nóng Gió Lào";
                case WeatherType.MuaGiong: return "Mưa dông lớn";
                case WeatherType.BaoLu: return "Bão lũ thiên tai";
                default: return type.ToString();
            }
        }

        #region PHÂN HỆ CỬA HÀNG RUNTIME (SHOP SYSTEM INTERACTION)

        private void CreateShopPanel()
        {
            TMP_FontAsset font = speakerNameText != null ? speakerNameText.font : null;

            // 1. Khởi tạo panel chính
            shopPanel = new GameObject("Panel_Shop", typeof(RectTransform), typeof(Image));
            shopPanel.transform.SetParent(this.transform, false);

            RectTransform r = shopPanel.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.5f, 0.5f);
            r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0f, 0f);
            r.sizeDelta = new Vector2(500f, 400f);

            Image bg = shopPanel.GetComponent<Image>();
            bg.color = new Color(0.12f, 0.14f, 0.18f, 0.96f); // Nâu tối ánh xanh đen sang trọng

            // 2. Chữ tiêu đề
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(shopPanel.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -20f);
            titleRect.sizeDelta = new Vector2(460f, 30f);

            TextMeshProUGUI title = titleObj.GetComponent<TextMeshProUGUI>();
            title.text = "<b>ĐẠI LÝ PHÂN GIỐNG O THẮM</b>";
            title.alignment = TextAlignmentOptions.Center;
            title.fontSize = 18;
            title.color = new Color(0.95f, 0.8f, 0.3f, 1f); // Màu vàng gold
            if (font != null) title.font = font;

            // 3. Số dư tiền xu
            GameObject coinsObj = new GameObject("CoinsText", typeof(RectTransform), typeof(TextMeshProUGUI));
            coinsObj.transform.SetParent(shopPanel.transform, false);
            RectTransform coinsRect = coinsObj.GetComponent<RectTransform>();
            coinsRect.anchorMin = new Vector2(0.5f, 1f);
            coinsRect.anchorMax = new Vector2(0.5f, 1f);
            coinsRect.pivot = new Vector2(0.5f, 1f);
            coinsRect.anchoredPosition = new Vector2(0f, -50f);
            coinsRect.sizeDelta = new Vector2(460f, 25f);

            shopCoinsText = coinsObj.GetComponent<TextMeshProUGUI>();
            shopCoinsText.alignment = TextAlignmentOptions.Center;
            shopCoinsText.fontSize = 14;
            shopCoinsText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            if (font != null) shopCoinsText.font = font;

            // 4. Nút đóng (X)
            GameObject closeBtnObj = new GameObject("Button_Close", typeof(RectTransform), typeof(Image), typeof(Button));
            closeBtnObj.transform.SetParent(shopPanel.transform, false);
            RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 1f);
            closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-15f, -15f);
            closeRect.sizeDelta = new Vector2(30f, 30f);

            closeBtnObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            Button closeBtn = closeBtnObj.GetComponent<Button>();
            closeBtn.onClick.AddListener(() => ToggleShop(false));

            GameObject closeTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            closeTextObj.transform.SetParent(closeBtnObj.transform, false);
            RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;
            TextMeshProUGUI closeText = closeTextObj.GetComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.fontSize = 14;
            closeText.color = Color.white;
            if (font != null) closeText.font = font;

            // 5. Container chứa danh sách các mặt hàng
            GameObject listContainer = new GameObject("ItemsList", typeof(RectTransform));
            listContainer.transform.SetParent(shopPanel.transform, false);
            RectTransform listRect = listContainer.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.5f, 0.5f);
            listRect.anchorMax = new Vector2(0.5f, 0.5f);
            listRect.pivot = new Vector2(0.5f, 0.5f);
            listRect.anchoredPosition = new Vector2(0f, -40f);
            listRect.sizeDelta = new Vector2(460f, 300f);

            // Tạo các dòng vật phẩm (5 dòng)
            CreateShopRow(listContainer.transform, 0, "Hạt giống Khoai", "Dùng gieo trồng khoai lang", true, () => GetSeedPrice(), seedItem, font);
            CreateShopRow(listContainer.transform, 1, "Nhang cúng", "Dùng thắp nhang ban thờ gia tiên", true, () => 15, incenseItem, font);
            CreateShopRow(listContainer.transform, 2, "Khoai lang tươi", "Nông sản tươi thu hoạch từ ruộng", false, () => 25, freshCropItem, font);
            CreateShopRow(listContainer.transform, 3, "Khoai gieo khô", "Đặc sản phơi khô tích lũy chống lũ", false, () => 40, preservedCropItem, font);
            CreateShopRow(listContainer.transform, 4, "Mì Tôm Cứu Trợ", "Mì tôm ăn liền cứu trợ khẩn cấp", true, () => 15, noodlesItem, font);

            shopPanel.SetActive(false); // Ẩn mặc định
        }

        private int GetSeedPrice()
        {
            bool isPhuSa = (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.PhuSa);
            return isPhuSa ? 6 : 10;
        }

        private void CreateShopRow(Transform parent, int index, string itemName, string desc, bool isBuy, System.Func<int> priceFunc, ItemData item, TMP_FontAsset font)
        {
            float yPos = 120f - index * 60f; // Cách nhau 60 units chiều dọc

            GameObject row = new GameObject($"Row_{index}", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(parent, false);
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0.5f);
            rowRect.anchorMax = new Vector2(0.5f, 0.5f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = new Vector2(0f, yPos);
            rowRect.sizeDelta = new Vector2(450f, 50f);

            row.GetComponent<Image>().color = new Color(0.18f, 0.2f, 0.25f, 0.6f); // Nền dòng xám dịu

            // Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(row.transform, false);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(10f, 0f);
            iconRect.sizeDelta = new Vector2(40f, 40f);

            Image iconImg = iconObj.GetComponent<Image>();
            if (item != null && item.Icon != null)
            {
                iconImg.sprite = item.Icon;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            }

            // Info Text (Tên, Mô tả và Số lượng đang có)
            GameObject infoObj = new GameObject("InfoText", typeof(RectTransform), typeof(TextMeshProUGUI));
            infoObj.transform.SetParent(row.transform, false);
            RectTransform infoRect = infoObj.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0f, 0.5f);
            infoRect.anchorMax = new Vector2(1f, 0.5f);
            infoRect.pivot = new Vector2(0f, 0.5f);
            infoRect.anchoredPosition = new Vector2(60f, 0f);
            infoRect.sizeDelta = new Vector2(250f, 40f);

            TextMeshProUGUI infoTxt = infoObj.GetComponent<TextMeshProUGUI>();
            infoTxt.alignment = TextAlignmentOptions.Left;
            infoTxt.fontSize = 11;
            infoTxt.color = Color.white;
            if (font != null) infoTxt.font = font;

            // Action Button
            GameObject btnObj = new GameObject("Button_Action", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(row.transform, false);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1f, 0.5f);
            btnRect.anchorMax = new Vector2(1f, 0.5f);
            btnRect.pivot = new Vector2(1f, 0.5f);
            btnRect.anchoredPosition = new Vector2(-10f, 0f);
            btnRect.sizeDelta = new Vector2(110f, 35f);

            btnObj.GetComponent<Image>().color = isBuy ? new Color(0.2f, 0.6f, 0.3f, 0.9f) : new Color(0.8f, 0.5f, 0.1f, 0.9f); // Xanh lá mua, Cam đất bán

            GameObject btnTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.offsetMin = Vector2.zero;
            btnTextRect.offsetMax = Vector2.zero;

            TextMeshProUGUI btnText = btnTextObj.GetComponent<TextMeshProUGUI>();
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontSize = 11;
            btnText.color = Color.white;
            if (font != null) btnText.font = font;

            Button btn = btnObj.GetComponent<Button>();
            
            // Đăng ký sự kiện click
            if (isBuy)
            {
                btn.onClick.AddListener(() => BuyItem(item, priceFunc()));
            }
            else
            {
                btn.onClick.AddListener(() => SellItem(item, priceFunc()));
            }

            // Lưu row để RefreshUI cập nhật chữ
            shopItemRows.Add(row);
        }

        public void ToggleShop(bool open)
        {
            isShopOpen = open;
            if (shopPanel != null)
            {
                shopPanel.SetActive(isShopOpen);
            }
            if (isShopOpen)
            {
                RefreshShopUI();
                // Tự động đóng các bảng UI khác để tránh đè giao diện
                if (inventoryPanel != null) inventoryPanel.SetActive(false);
                isInventoryOpen = false;
                if (communityPanel != null) communityPanel.SetActive(false);
                isCommunityOpen = false;
                if (weatherDetailsPanel != null) weatherDetailsPanel.SetActive(false);
                isWeatherDetailsOpen = false;
            }
        }

        public void RefreshShopUI()
        {
            if (shopPanel == null || !isShopOpen) return;

            // 1. Cập nhật Coins
            if (shopCoinsText != null && PlayerStats.Instance != null)
            {
                shopCoinsText.text = $"Tài sản gia đình: <color=#F4D03F>{PlayerStats.Instance.Coins} Xu</color>";
            }

            // 2. Định nghĩa lại danh sách thông số các mặt hàng để đồng bộ lại giao diện
            var itemParams = new[]
            {
                new { name = "Hạt giống Khoai", desc = "Dùng gieo trồng khoai lang", isBuy = true, price = GetSeedPrice(), item = seedItem },
                new { name = "Nhang cúng", desc = "Dùng thắp nhang ban thờ gia tiên", isBuy = true, price = 15, item = incenseItem },
                new { name = "Khoai lang tươi", desc = "Nông sản tươi thu hoạch từ ruộng", isBuy = false, price = 25, item = freshCropItem },
                new { name = "Khoai gieo khô", desc = "Đặc sản phơi khô tích lũy chống lũ", isBuy = false, price = 40, item = preservedCropItem },
                new { name = "Mì Tôm Cứu Trợ", desc = "Mì tôm ăn liền cứu trợ khẩn cấp", isBuy = true, price = 15, item = noodlesItem }
            };

            // 3. Quét qua từng hàng để cập nhật văn bản và nút bấm
            for (int i = 0; i < shopItemRows.Count && i < itemParams.Length; i++)
            {
                GameObject row = shopItemRows[i];
                var param = itemParams[i];

                if (row == null || param.item == null) continue;

                // Lấy số lượng đang sở hữu trong Inventory
                int ownedCount = 0;
                if (StorageManager.Instance != null)
                {
                    var slots = StorageManager.Instance.GetStorageSlots();
                    var slot = slots.Find(s => s.item.ItemID == param.item.ItemID);
                    ownedCount = slot != null ? slot.quantity : 0;
                }

                // Cập nhật Info Text
                TextMeshProUGUI infoTxt = row.transform.Find("InfoText")?.GetComponent<TextMeshProUGUI>();
                if (infoTxt != null)
                {
                    infoTxt.text = $"<b>{param.name}</b> (Đang có: {ownedCount})\n<size=9.5>{param.desc}</size>";
                }

                // Cập nhật Button Text và tương tác
                Button btn = row.transform.Find("Button_Action")?.GetComponent<Button>();
                TextMeshProUGUI btnText = btn?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
                if (btn != null && btnText != null)
                {
                    if (param.isBuy)
                    {
                        btnText.text = $"Mua (-{param.price} Xu)";
                        // Tắt nút nếu không đủ tiền
                        btn.interactable = (PlayerStats.Instance != null && PlayerStats.Instance.Coins >= param.price);
                    }
                    else
                    {
                        btnText.text = $"Bán (+{param.price} Xu)";
                        // Tắt nút nếu không có đồ để bán
                        btn.interactable = (ownedCount > 0);
                    }
                }
            }
        }

        private void BuyItem(ItemData item, int price)
        {
            if (item == null) return;
            if (PlayerStats.Instance == null || StorageManager.Instance == null) return;

            if (PlayerStats.Instance.Coins >= price)
            {
                PlayerStats.Instance.ModifyCoins(-price);
                StorageManager.Instance.AddItem(item, 1);
                ShowHUDToast($"Đã mua 1 {item.ItemName} (-{price} Xu)");
                RefreshShopUI();
                RefreshInventoryUI();
            }
            else
            {
                ShowHUDToast("Gia đình không đủ tiền xu!");
            }
        }

        private void SellItem(ItemData item, int price)
        {
            if (item == null) return;
            if (PlayerStats.Instance == null || StorageManager.Instance == null) return;

            // Kiểm tra xem có đồ trong Inventory không
            var slots = StorageManager.Instance.GetStorageSlots();
            var slot = slots.Find(s => s.item.ItemID == item.ItemID);
            int count = slot != null ? slot.quantity : 0;

            if (count > 0)
            {
                if (StorageManager.Instance.RemoveItem(item, 1))
                {
                    PlayerStats.Instance.ModifyCoins(price);
                    ShowHUDToast($"Đã bán 1 {item.ItemName} (+{price} Xu)");
                    RefreshShopUI();
                    RefreshInventoryUI();
                }
            }
            else
            {
                ShowHUDToast($"Không có {item.ItemName} để bán!");
            }
        }

        #endregion
    }
}
