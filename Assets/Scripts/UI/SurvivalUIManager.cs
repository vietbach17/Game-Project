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
    /// Giúp người chơi theo dõi Máu, Thể lực, Tinh thần, hòm đồ và đọc hội thoại điện ảnh từ dân làng.
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

            // 3. Lắng nghe phím mở hòm đồ (Tab/I) và đóng thoại (Space/Escape) tương thích cả 2 Input System
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.iKey.wasPressedThisFrame)
                {
                    ToggleInventory();
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
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventory();
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
                if (dayText != null) dayText.text = $"Ngày: {GameManager.Instance.CurrentDay}";

                if (timeText != null)
                {
                    int hour = Mathf.FloorToInt(GameManager.Instance.CurrentHour);
                    int minute = Mathf.FloorToInt((GameManager.Instance.CurrentHour % 1) * 60);
                    timeText.text = $"Giờ: {hour:00}:{minute:00}";
                }

                if (phaseText != null)
                {
                    phaseText.text = GetPhaseVietnameseName(GameManager.Instance.CurrentPhase);
                }
            }

            if (WeatherManager.Instance != null && weatherText != null)
            {
                string desc = GetWeatherDesc(WeatherManager.Instance.currentVisualWeather);
                if (WeatherManager.Instance.FloodLevel > 0f)
                {
                    desc += $" | Lụt: {WeatherManager.Instance.FloodLevel:F1} mét";
                }
                weatherText.text = $"Thời tiết: {desc} ({WeatherManager.Instance.Temperature:F1}°C)";
            }
        }

        #region PHÂN HỆ HỘI THOẠI CHẠY CHỮ (TYPEWRITER)
        
        /// <summary>
        /// Hiển thị khung hội thoại và chạy lời thoại từng ký tự.
        /// </summary>
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

        /// <summary>
        /// Hiển thị hội thoại kèm theo 2 hoặc 3 lựa chọn cho người chơi.
        /// </summary>
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

        /// <summary>
        /// Đóng khung hội thoại lại.
        /// </summary>
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
        
        /// <summary>
        /// Đóng hoặc mở hòm đồ của người chơi.
        /// </summary>
        public void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(isInventoryOpen);
                if (isInventoryOpen)
                {
                    RefreshInventoryUI();
                }
            }
        }

        /// <summary>
        /// Làm mới lưới hiển thị vật phẩm theo kho đồ thực tế.
        /// </summary>
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

                // Gán chữ hiển thị tên và số lượng
                TextMeshProUGUI txt = newSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = $"{slot.item.ItemName}\nx{slot.quantity}";
                }

                // Gán hình ảnh biểu tượng vật phẩm
                Image img = newSlot.transform.Find("Icon")?.GetComponent<Image>();
                if (img != null && slot.item.Icon != null)
                {
                    img.sprite = slot.item.Icon;
                    img.enabled = true;
                }
            }

            // Cập nhật số lượng tiền xu hiện có của gia đình
            if (inventoryCoinsText != null && PlayerStats.Instance != null)
            {
                inventoryCoinsText.text = $"Tài sản gia đình: <color=#F4D03F>{PlayerStats.Instance.Coins} Xu</color>";
            }
        }

        /// <summary>
        /// Nút tương tác chế biến khoai gieo trực tiếp từ UI.
        /// Tiêu hao 3 khoai tươi + 10 thể lực tạo ra 1 khoai gieo khô.
        /// </summary>
        public void ActionCraftPreservedItem()
        {
            if (StorageManager.Instance == null || freshCropItem == null || preservedCropItem == null) return;

            bool success = StorageManager.Instance.CraftPreservedItem(freshCropItem, preservedCropItem, 1);
            if (success)
            {
                RefreshInventoryUI();
            }
        }

        #endregion

        #region PHÂN HỆ LỚP PHỦ VIỀN MÀN HÌNH (VIGNETTE OVERLAYS)

        private void SetupVignetteOverlays()
        {
            // Tạo Sprite Vignette mờ bằng code
            Sprite vignetteSprite = CreateProceduralVignetteSprite();

            // 1. Tạo lớp phủ Sốc Nhiệt (Heat Overlay)
            GameObject heatObj = new GameObject("HeatOverlay", typeof(RectTransform), typeof(Image));
            heatObj.transform.SetParent(this.transform, false);
            heatObj.transform.SetAsFirstSibling(); // Đưa xuống dưới cùng để không đè lên các UI Sliders, Text
            
            RectTransform heatRect = heatObj.GetComponent<RectTransform>();
            heatRect.anchorMin = Vector2.zero;
            heatRect.anchorMax = Vector2.one;
            heatRect.offsetMin = Vector2.zero;
            heatRect.offsetMax = Vector2.zero;

            heatOverlayImage = heatObj.GetComponent<Image>();
            heatOverlayImage.sprite = vignetteSprite;
            heatOverlayImage.color = new Color(heatColor.r, heatColor.g, heatColor.b, 0f);
            heatOverlayImage.raycastTarget = false; // Tránh chặn click chuột vào game

            // 2. Tạo lớp phủ Cảm Lạnh (Cold Overlay)
            GameObject coldObj = new GameObject("ColdOverlay", typeof(RectTransform), typeof(Image));
            coldObj.transform.SetParent(this.transform, false);
            coldObj.transform.SetAsFirstSibling(); // Đưa xuống dưới cùng
            
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
                    // Hàm mũ giúp tâm trong suốt hẳn và rìa ngoài mờ đục dần
                    float alpha = Mathf.Clamp01(dist * dist * 1.5f); 
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            
            // Đặt chế độ lặp clamped để tránh viền bị sọc đen
            tex.wrapMode = TextureWrapMode.Clamp;

            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
        }

        private void UpdateVignetteOverlays()
        {
            if (PlayerStats.Instance == null) return;

            float heatStress = PlayerStats.Instance.HeatStress;
            float coldStress = PlayerStats.Instance.ColdStress;

            // Hiệu ứng mạch đập (pulsing effect) khi stress cao (>70%) để cảnh báo nguy kịch
            float pulse = 1f;
            if (heatStress > 70f || coldStress > 70f)
            {
                // Mạch đập nhịp tim dao động từ 0.85 đến 1.15
                pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.15f;
            }

            // Tính toán Alpha mục tiêu
            float targetHeatAlpha = (heatStress / 100f) * maxOverlayAlpha * pulse;
            float targetColdAlpha = (coldStress / 100f) * maxOverlayAlpha * pulse;

            // Áp dụng mượt mà (smooth lerp) tránh thay đổi đột ngột gây giật mình
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
            
            // Xếp trên thảm cỏ và các lớp phủ viền nhưng dưới các UI HUD khác
            waterObj.transform.SetAsFirstSibling();
            int siblingIndex = 0;
            if (heatOverlayImage != null) siblingIndex++;
            if (coldOverlayImage != null) siblingIndex++;
            waterObj.transform.SetSiblingIndex(siblingIndex);

            waterOverlayRect = waterObj.GetComponent<RectTransform>();
            // Làm lớn hơn màn hình một chút để phục vụ việc trượt ảnh cuộn sóng
            waterOverlayRect.anchorMin = Vector2.zero;
            waterOverlayRect.anchorMax = Vector2.one;
            waterOverlayRect.offsetMin = new Vector2(-128f, -128f);
            waterOverlayRect.offsetMax = new Vector2(128f, 128f);

            waterOverlayImage = waterObj.GetComponent<Image>();
            waterOverlayImage.sprite = waterWavesSprite;
            waterOverlayImage.type = Image.Type.Tiled;
            waterOverlayImage.color = new Color(0.35f, 0.6f, 0.8f, 0f); // Xanh dương trong suốt nước lũ
            waterOverlayImage.raycastTarget = false;
        }

        private void UpdateWaterOverlay()
        {
            if (waterOverlayImage == null || WeatherManager.Instance == null) return;

            float floodLevel = WeatherManager.Instance.FloodLevel;

            // Tính toán độ đục của nước lũ (càng lụt sâu nước càng đậm)
            // Giả định lụt tối đa ở mức 2.5 mét nước sẽ đạt max opacity
            float targetAlpha = Mathf.Clamp01(floodLevel / 2.5f) * maxWaterAlpha;

            Color c = waterOverlayImage.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 2f);
            waterOverlayImage.color = c;

            // Nếu nước lụt dâng lên, thực hiện cuộn trượt ảnh để tạo hiệu ứng sóng chảy
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

            // Tạo ChoiceContainer
            choiceContainer = new GameObject("ChoiceContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            choiceContainer.transform.SetParent(dialoguePanel.transform, false);

            RectTransform containerRect = choiceContainer.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(1f, 0.5f); // Neo ở cạnh phải, căn giữa dọc
            containerRect.anchorMax = new Vector2(1f, 0.5f);
            containerRect.pivot = new Vector2(1f, 0.5f);
            containerRect.anchoredPosition = new Vector2(-30f, 0f); // Lệch trái 30px
            containerRect.sizeDelta = new Vector2(360f, 45f);

            HorizontalLayoutGroup layout = choiceContainer.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 15f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            // Font TMP mặc định từ Text có sẵn để đồng bộ phông chữ
            TMP_FontAsset font = speakerNameText != null ? speakerNameText.font : null;

            // 1. Nút lựa chọn 1
            GameObject btn1Obj = CreateProceduralButton("Choice1Button", "1. Lựa chọn 1", font, out choice1Button, out choice1Text);
            btn1Obj.transform.SetParent(choiceContainer.transform, false);
            choice1Button.onClick.AddListener(() => SelectChoice(1));

            // 2. Nút lựa chọn 2
            GameObject btn2Obj = CreateProceduralButton("Choice2Button", "2. Lựa chọn 2", font, out choice2Button, out choice2Text);
            btn2Obj.transform.SetParent(choiceContainer.transform, false);
            choice2Button.onClick.AddListener(() => SelectChoice(2));

            // 3. Nút lựa chọn 3
            GameObject btn3Obj = CreateProceduralButton("Choice3Button", "3. Lựa chọn 3", font, out choice3Button, out choice3Text);
            btn3Obj.transform.SetParent(choiceContainer.transform, false);
            choice3Button.onClick.AddListener(() => SelectChoice(3));

            choiceContainer.SetActive(false); // Mặc định ẩn đi
        }

        private GameObject CreateProceduralButton(string name, string text, TMP_FontAsset font, out Button button, out TextMeshProUGUI textComponent)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            
            // Cấu hình hình ảnh nền nút (Nâu gỗ lim đậm đồng bộ phong cách mộc mạc)
            Image img = btnObj.GetComponent<Image>();
            img.color = new Color(0.2f, 0.14f, 0.08f, 1f); // Nâu gỗ đậm

            button = btnObj.GetComponent<Button>();
            
            // Tạo text con bên trong nút
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

            // Thực thi hành động tương ứng
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
            panelRect.anchorMin = new Vector2(0f, 0.4f); // Căn giữa màn hình dọc
            panelRect.anchorMax = new Vector2(1f, 0.6f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            phaseAnnouncementCanvasGroup = phaseAnnouncementObj.GetComponent<CanvasGroup>();
            phaseAnnouncementCanvasGroup.alpha = 0f;
            phaseAnnouncementCanvasGroup.interactable = false;
            phaseAnnouncementCanvasGroup.blocksRaycasts = false;

            // Hình nền mờ đằng sau chữ thông báo
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(phaseAnnouncementObj.transform, false);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.65f); // Đen mờ sang trọng

            // Tạo chữ thông báo
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
            phaseAnnouncementText.color = new Color(0.95f, 0.8f, 0.3f, 1f); // Màu vàng rơm ấm áp
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
            
            // Fade In
            float elapsed = 0f;
            float duration = 1.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                phaseAnnouncementCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            phaseAnnouncementCanvasGroup.alpha = 1f;

            // Chờ hiển thị
            yield return new WaitForSeconds(3.5f);

            // Fade Out
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

        #endregion

        private string GetPhaseVietnameseName(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.LapNghiep: return "Tiếng Trống Đình Làng (GĐ 1)";
                case GamePhase.GioLao: return "Nắng Cháy Gió Lào (GĐ 2)";
                case GamePhase.MuaBao: return "Tình Người Trong Lũ (GĐ 3)";
                case GamePhase.PhuSa: return "Phù Sa Sau Cơn Lũ (GĐ 4)";
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
    }
}
