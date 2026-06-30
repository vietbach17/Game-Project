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
        [SerializeField] private VillageSpeakerBanner villageSpeakerBanner;

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

        public TextMeshProUGUI interactionPromptText;
        private TextMeshProUGUI coinsText; // Text Xu ở Top-Right

        private GameObject toastPanel;
        private TextMeshProUGUI toastText;
        private CanvasGroup toastCanvasGroup;
        private Coroutine toastCoroutine;

        private GameObject resourcePanel;
        private CanvasGroup resourceCanvasGroup;

        // --- PHÂN HỆ CỬA HÀNG O THẮM (SHOP SYSTEM) ---
        [Header("--- CÀI ĐẶT CỬA HÀNG ---")]
        [SerializeField] private ItemData seedItem;
        [SerializeField] private ItemData incenseItem;
        [SerializeField] private ItemData noodlesItem;
        [SerializeField] private ItemData nonLaItem;
        [SerializeField] private ItemData sandbagItem;
        [SerializeField] private ItemData floodBoardItem;

        private GameObject shopPanel;
        private TextMeshProUGUI shopCoinsText;
        private bool isShopOpen = false;
        private List<GameObject> shopItemRows = new List<GameObject>();

        public bool IsDialogueActive => isDialogueActive;
        public bool IsChoiceActive => isChoiceActive;
        public bool IsShopOpen => isShopOpen;
        public bool IsInventoryOpen => isInventoryOpen;
        public bool IsCommunityOpen => isCommunityOpen;
        public bool IsWeatherDetailsOpen => isWeatherDetailsOpen;
        public bool IsQuantityPopupOpen => isQuantityPopupOpen;
        public TextMeshProUGUI SpeakerNameText => speakerNameText;
        public ItemData IncenseItem => incenseItem;
        public ItemData SandbagItem => sandbagItem;
        public ItemData FloodBoardItem => floodBoardItem;

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
            gameObject.AddComponent<NPCProximityOptionsUI>();
            gameObject.AddComponent<NPCQuestMarkerUI>();
            gameObject.AddComponent<HotbarManager>();
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

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPhaseChanged += OnPhaseChangedHandler;
            }

#if UNITY_EDITOR
            // Tự động tải tài nguyên để đảm bảo không bị mất reference trong Editor
            seedItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Seed.asset");
            incenseItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Incense.asset");
            noodlesItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Noodles.asset");
            freshCropItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_FreshCrop.asset");
            preservedCropItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_PreservedCrop.asset");
            nonLaItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_non_la.asset");
            sandbagItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_sandbag.asset");
            floodBoardItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_flood_board.asset");
#endif

            // Tạo các panel bổ sung
            CreateInventoryTitle();
            CreateCommunityPanel();
            CreateWeatherDetailsPanel();
            CreateInteractionPromptUI();
            CreateToastNotificationUI();
            CreateControlsLegendUI();
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
                StorageManager.Instance.OnStorageAlert += ShowHUDToast;
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
                StorageManager.Instance.OnStorageAlert -= ShowHUDToast;
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

            // Cập nhật hiển thị của ResourcePanel dựa trên trạng thái hội thoại, cửa hàng, và kết thúc game
            if (resourceCanvasGroup != null)
            {
                bool shouldShow = !isDialogueActive && !isShopOpen;
                if (EndingManager.Instance != null && EndingManager.Instance.IsEndingShown)
                {
                    shouldShow = false;
                }
                
                float targetAlpha = shouldShow ? 1f : 0f;
                resourceCanvasGroup.alpha = Mathf.MoveTowards(resourceCanvasGroup.alpha, targetAlpha, Time.deltaTime * 5f);
                resourceCanvasGroup.blocksRaycasts = shouldShow;
                resourceCanvasGroup.interactable = shouldShow;
            }
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

                // Toggle Controls Legend (H)
                if (Keyboard.current.hKey.wasPressedThisFrame)
                {
                    ToggleControlsLegend();
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

            // Toggle Controls Legend (H)
            if (Input.GetKeyDown(KeyCode.H))
            {
                ToggleControlsLegend();
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

            // Lấy Canvas Group chính làm cha để các HUD được neo chuẩn vào góc màn hình thay vì tâm
            Transform parentCanvas = mainUICanvasGroup != null ? mainUICanvasGroup.transform : this.transform;

            // 1. Nhóm chỉ số Survival Bars (Bottom-Left)
            // Tạo background panel nhỏ gọn (ResourcePanel) chỉ chứa Thể lực (Stamina)
            resourcePanel = new GameObject("ResourcePanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            resourceCanvasGroup = resourcePanel.GetComponent<CanvasGroup>();
            resourcePanel.transform.SetParent(parentCanvas, false);
            RectTransform resRect = resourcePanel.GetComponent<RectTransform>();
            resRect.anchorMin = new Vector2(0f, 0f);
            resRect.anchorMax = new Vector2(0f, 0f);
            resRect.pivot = new Vector2(0f, 0f);
            resRect.anchoredPosition = new Vector2(20f, 20f);
            resRect.sizeDelta = new Vector2(260f, 50f); // Giảm chiều cao từ 120 xuống 50

            Image resImg = resourcePanel.GetComponent<Image>();
            resImg.color = new Color(0.08f, 0.06f, 0.05f, 0.9f);

            Outline resOutline = resourcePanel.AddComponent<Outline>();
            resOutline.effectColor = new Color(0.38f, 0.30f, 0.22f, 1f);
            resOutline.effectDistance = new Vector2(1.5f, 1.5f);

            // Bỏ thanh Sức khỏe và Tinh thần (Ẩn đi)
            if (healthSlider != null) healthSlider.gameObject.SetActive(false);
            if (moraleSlider != null) moraleSlider.gameObject.SetActive(false);

            // Định vị lại Thể lực (Stamina) lên trên cùng của resourcePanel gọn gàng
            if (staminaSlider != null)
            {
                staminaSlider.transform.SetParent(resourcePanel.transform, false);
                RectTransform r = staminaSlider.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 1f);
                r.anchorMax = new Vector2(1f, 1f);
                r.pivot = new Vector2(0.5f, 1f);
                r.anchoredPosition = new Vector2(0f, -28f); // Đẩy lên vị trí trên
                r.offsetMin = new Vector2(15f, r.offsetMin.y);
                r.offsetMax = new Vector2(-15f, r.offsetMax.y);
                r.sizeDelta = new Vector2(r.sizeDelta.x, 8f);

                GameObject labelObj = new GameObject("StaminaLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(resourcePanel.transform, false);
                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0f, 1f);
                labelRect.anchorMax = new Vector2(1f, 1f);
                labelRect.pivot = new Vector2(0f, 1f);
                labelRect.anchoredPosition = new Vector2(15f, -10f); // Đẩy lên sát nhãn
                labelRect.sizeDelta = new Vector2(230f, 16f);

                TextMeshProUGUI label = labelObj.GetComponent<TextMeshProUGUI>();
                label.text = "Thể lực";
                label.fontSize = 11;
                label.color = new Color(0.95f, 0.85f, 0.4f, 1f);
                if (font != null) label.font = font;
            }

            // 2. Nhóm thông tin Thời gian (Panel 1: Time / Season) - Thu nhỏ gọn gàng ở góc trên bên trái
            GameObject timeSeasonPanel = new GameObject("TimeSeasonPanel", typeof(RectTransform), typeof(Image));
            timeSeasonPanel.transform.SetParent(parentCanvas, false);
            RectTransform timeRect = timeSeasonPanel.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0f, 1f);
            timeRect.anchorMax = new Vector2(0f, 1f);
            timeRect.pivot = new Vector2(0f, 1f);
            timeRect.anchoredPosition = new Vector2(15f, -15f); // Đưa sát vào góc trái
            timeRect.sizeDelta = new Vector2(200f, 50f); // Giảm chiều rộng từ 280 xuống 200, chiều cao từ 70 xuống 50

            Image timeImg = timeSeasonPanel.GetComponent<Image>();
            timeImg.color = new Color(0.08f, 0.06f, 0.05f, 0.9f);

            Outline timeOutline = timeSeasonPanel.AddComponent<Outline>();
            timeOutline.effectColor = new Color(0.38f, 0.30f, 0.22f, 1f);
            timeOutline.effectDistance = new Vector2(1.5f, 1.5f);

            if (dayText != null)
            {
                dayText.transform.SetParent(timeSeasonPanel.transform, false);
                dayText.fontSize = 11; // Giảm từ 14 xuống 11
                dayText.fontStyle = FontStyles.Bold;
                dayText.color = new Color(0.95f, 0.85f, 0.4f, 1f);
                dayText.alignment = TextAlignmentOptions.Left;
                RectTransform r = dayText.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 1f);
                r.anchorMax = new Vector2(1f, 1f);
                r.pivot = new Vector2(0f, 1f);
                r.anchoredPosition = new Vector2(10f, -8f);
                r.sizeDelta = new Vector2(180f, 18f);
            }
            if (timeText != null)
            {
                timeText.gameObject.SetActive(false); // Gộp chung vào dayText để hiển thị gọn
            }
            if (phaseText != null)
            {
                phaseText.transform.SetParent(timeSeasonPanel.transform, false);
                phaseText.fontSize = 9.5f; // Giảm từ 12 xuống 9.5
                phaseText.color = Color.white;
                phaseText.alignment = TextAlignmentOptions.Left;
                RectTransform r = phaseText.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0f, 0f);
                r.anchorMax = new Vector2(1f, 0f);
                r.pivot = new Vector2(0f, 0f);
                r.anchoredPosition = new Vector2(10f, 8f);
                r.sizeDelta = new Vector2(180f, 16f);
            }

            // 2.5. Định vị và định kiểu NghiaTinhPanel dưới Time HUD sát lại (X = 15, Y = -70)
#if UNITY_2023_1_OR_NEWER
            NghiaTinhUI nghiaTinh = FindAnyObjectByType<NghiaTinhUI>();
#else
            NghiaTinhUI nghiaTinh = FindObjectOfType<NghiaTinhUI>();
#endif
            if (nghiaTinh != null)
            {
                nghiaTinh.transform.SetParent(parentCanvas, false);
                RectTransform r = nghiaTinh.GetComponent<RectTransform>();
                if (r != null)
                {
                    r.anchorMin = new Vector2(0f, 1f);
                    r.anchorMax = new Vector2(0f, 1f);
                    r.pivot = new Vector2(0f, 1f);
                    r.anchoredPosition = new Vector2(15f, -70f); // Thu hẹp khoảng cách với Panel Time
                    r.sizeDelta = new Vector2(200f, 50f); // Giảm chiều rộng từ 280 xuống 200, chiều cao từ 70 xuống 50

                    Image nghiaTinhBg = nghiaTinh.GetComponent<Image>();
                    if (nghiaTinhBg != null)
                    {
                        nghiaTinhBg.color = new Color(0.08f, 0.06f, 0.05f, 0.9f);
                    }
                    Outline nghiaTinhOutline = nghiaTinh.GetComponent<Outline>();
                    if (nghiaTinhOutline == null)
                    {
                        nghiaTinhOutline = nghiaTinh.gameObject.AddComponent<Outline>();
                    }
                    nghiaTinhOutline.effectColor = new Color(0.38f, 0.30f, 0.22f, 1f);
                    nghiaTinhOutline.effectDistance = new Vector2(1.5f, 1.5f);

                    // Style children inside NghiaTinhPanel thu nhỏ
                    Transform titleTr = nghiaTinh.transform.Find("TitleText");
                    if (titleTr != null)
                    {
                        TextMeshProUGUI tText = titleTr.GetComponent<TextMeshProUGUI>();
                        tText.fontSize = 11; // Giảm từ 14 xuống 11
                        tText.fontStyle = FontStyles.Bold;
                        tText.color = new Color(0.95f, 0.85f, 0.4f, 1f);
                        RectTransform tr = titleTr.GetComponent<RectTransform>();
                        tr.anchorMin = new Vector2(0f, 1f);
                        tr.anchorMax = new Vector2(0f, 1f);
                        tr.pivot = new Vector2(0f, 1f);
                        tr.anchoredPosition = new Vector2(10f, -8f);
                        tr.sizeDelta = new Vector2(100f, 18f);
                    }

                    Transform valueTr = nghiaTinh.transform.Find("ValueText");
                    if (valueTr != null)
                    {
                        TextMeshProUGUI vText = valueTr.GetComponent<TextMeshProUGUI>();
                        vText.fontSize = 11; // Giảm từ 14 xuống 11
                        vText.fontStyle = FontStyles.Bold;
                        vText.color = Color.white;
                        vText.alignment = TextAlignmentOptions.Right;
                        RectTransform vr = valueTr.GetComponent<RectTransform>();
                        vr.anchorMin = new Vector2(1f, 1f);
                        vr.anchorMax = new Vector2(1f, 1f);
                        vr.pivot = new Vector2(1f, 1f);
                        vr.anchoredPosition = new Vector2(-10f, -8f);
                        vr.sizeDelta = new Vector2(70f, 18f);
                    }

                    Transform barTr = nghiaTinh.transform.Find("ProgressBar");
                    if (barTr != null)
                    {
                        RectTransform br = barTr.GetComponent<RectTransform>();
                        br.anchorMin = new Vector2(0f, 0f);
                        br.anchorMax = new Vector2(1f, 0f);
                        br.pivot = new Vector2(0.5f, 0f);
                        br.anchoredPosition = new Vector2(0f, 8f); // Giảm chiều cao Y cho cân đối
                        br.offsetMin = new Vector2(10f, br.offsetMin.y);
                        br.offsetMax = new Vector2(-10f, br.offsetMax.y);
                        br.sizeDelta = new Vector2(br.sizeDelta.x, 8f); // Giảm độ rộng thanh xuống 8f
                    }
                }
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

            // 5. Cải thiện độ an toàn cho khung hội thoại (Dialogue Panel padding & layout)
            if (dialoguePanel != null)
            {
                RectTransform diagRect = dialoguePanel.GetComponent<RectTransform>();
                if (diagRect != null)
                {
                    // Giữ nguyên kiểu full-width bottom nhưng tăng chiều cao để tránh cắt chữ
                    diagRect.anchorMin = new Vector2(0f, 0f);
                    diagRect.anchorMax = new Vector2(1f, 0f);
                    diagRect.pivot = new Vector2(0.5f, 0f);
                    diagRect.anchoredPosition = new Vector2(0f, 0f);
                    diagRect.sizeDelta = new Vector2(0f, 185f); // Tăng chiều cao lên 185f
                }

                // Căn chỉnh vị trí của Speaker Name và Content Text để không bị đè và có đủ không gian
                if (speakerNameText != null)
                {
                    RectTransform specRect = speakerNameText.GetComponent<RectTransform>();
                    if (specRect != null)
                    {
                        specRect.anchorMin = new Vector2(0f, 1f);
                        specRect.anchorMax = new Vector2(1f, 1f);
                        specRect.pivot = new Vector2(0f, 1f);
                        specRect.anchoredPosition = new Vector2(30f, -12f);
                        specRect.sizeDelta = new Vector2(-60f, 25f);
                    }
                    speakerNameText.margin = new Vector4(0f, 0f, 0f, 0f);
                }

                if (dialogueContentText != null)
                {
                    RectTransform contRect = dialogueContentText.GetComponent<RectTransform>();
                    if (contRect != null)
                    {
                        contRect.anchorMin = new Vector2(0f, 0f);
                        contRect.anchorMax = new Vector2(1f, 1f);
                        contRect.pivot = new Vector2(0f, 1f);
                        contRect.offsetMin = new Vector2(30f, 15f); // 15f bottom padding cho an toàn
                        contRect.offsetMax = new Vector2(-380f, -42f); // Lề phải chừa khoảng trống cho ChoiceContainer
                    }
                    dialogueContentText.margin = new Vector4(0f, 0f, 0f, 0f);
                }
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

            if (HotbarManager.Instance != null)
            {
                HotbarManager.Instance.ToggleHotbarVisible(false);
            }

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

            if (HotbarManager.Instance != null)
            {
                HotbarManager.Instance.ToggleHotbarVisible(false);
            }

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

            if (HotbarManager.Instance != null)
            {
                HotbarManager.Instance.ToggleHotbarVisible(true);
            }

            if (TutorialManager.Instance != null && speakerNameText != null)
            {
                TutorialManager.Instance.OnDialogueClosed(speakerNameText.text);
            }
        }

        #endregion

        #region PHÂN HỆ HÒM ĐỒ DẠNG LƯỚI

        private void EnsureInventoryPanelExists()
        {
            if (inventoryPanel == null)
            {
                Transform found = transform.Find("Panel_Inventory");
                if (found == null) found = transform.Find("InventoryPanel");
                if (found != null) inventoryPanel = found.gameObject;
            }

            if (inventoryPanel == null)
            {
                inventoryPanel = new GameObject("Panel_Inventory", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                inventoryPanel.transform.SetParent(this.transform, false);
                inventoryPanel.transform.SetAsLastSibling();

                RectTransform r = inventoryPanel.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0.5f, 0.5f);
                r.anchorMax = new Vector2(0.5f, 0.5f);
                r.pivot = new Vector2(0.5f, 0.5f);
                r.anchoredPosition = Vector2.zero;
                r.sizeDelta = new Vector2(460f, 380f);

                Image img = inventoryPanel.GetComponent<Image>();
                img.color = new Color(0.12f, 0.10f, 0.08f, 0.96f); // Nâu mộc mạc sang trọng
            }

            if (itemSlotContainer == null)
            {
                Transform foundContainer = inventoryPanel.transform.Find("GridContainer");
                if (foundContainer == null) foundContainer = inventoryPanel.transform.Find("Viewport/Content");
                if (foundContainer != null) itemSlotContainer = foundContainer;
            }

            if (itemSlotContainer == null)
            {
                GameObject containerObj = new GameObject("GridContainer", typeof(RectTransform), typeof(GridLayoutGroup));
                containerObj.transform.SetParent(inventoryPanel.transform, false);
                itemSlotContainer = containerObj.transform;

                GridLayoutGroup grid = containerObj.GetComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(80f, 80f);
                grid.spacing = new Vector2(10f, 10f);
                grid.padding = new RectOffset(15, 15, 15, 15);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 5;

                SetupInventoryScrollView();
            }

            if (itemSlotPrefab == null)
            {
                itemSlotPrefab = new GameObject("ItemSlotPrefab", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
                itemSlotPrefab.SetActive(false);
                itemSlotPrefab.transform.SetParent(this.transform, false);

                RectTransform slotRect = itemSlotPrefab.GetComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(80f, 80f);

                Image slotImg = itemSlotPrefab.GetComponent<Image>();
                slotImg.color = new Color(0.15f, 0.12f, 0.09f, 0.95f);

                Outline outline = itemSlotPrefab.GetComponent<Outline>();
                outline.effectColor = new Color(0.6f, 0.45f, 0.25f, 0.8f);
                outline.effectDistance = new Vector2(1.5f, 1.5f);

                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(itemSlotPrefab.transform, false);
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = new Vector2(60f, 60f);

                GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(itemSlotPrefab.transform, false);
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(1f, 0f);
                textRect.anchorMax = new Vector2(1f, 0f);
                textRect.pivot = new Vector2(1f, 0f);
                textRect.anchoredPosition = new Vector2(-4f, 4f);
                textRect.sizeDelta = new Vector2(50f, 20f);

                TextMeshProUGUI txt = textObj.GetComponent<TextMeshProUGUI>();
                txt.fontSize = 14;
                txt.alignment = TextAlignmentOptions.Right;
                txt.color = Color.white;
            }
        }

        private GameObject storageChestPanelObj;
        private Transform storageChestContainer;

        public void OpenStorageChestUI()
        {
            if (storageChestPanelObj == null)
            {
                storageChestPanelObj = new GameObject("Panel_StorageChestUI", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                storageChestPanelObj.transform.SetParent(this.transform, false);

                RectTransform r = storageChestPanelObj.GetComponent<RectTransform>();
                r.anchorMin = new Vector2(0.5f, 0.5f);
                r.anchorMax = new Vector2(0.5f, 0.5f);
                r.pivot = new Vector2(0.5f, 0.5f);
                r.anchoredPosition = Vector2.zero;
                r.sizeDelta = new Vector2(480f, 380f);

                Image img = storageChestPanelObj.GetComponent<Image>();
                img.color = new Color(0.12f, 0.10f, 0.08f, 0.98f);
                Outline outline = storageChestPanelObj.AddComponent<Outline>();
                outline.effectColor = new Color(0.7f, 0.55f, 0.3f, 1f);
                outline.effectDistance = new Vector2(2f, 2f);

                // Header Title
                GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
                titleObj.transform.SetParent(storageChestPanelObj.transform, false);
                RectTransform tRect = titleObj.GetComponent<RectTransform>();
                tRect.anchorMin = new Vector2(0.5f, 1f);
                tRect.anchorMax = new Vector2(0.5f, 1f);
                tRect.pivot = new Vector2(0.5f, 1f);
                tRect.anchoredPosition = new Vector2(0f, -15f);
                tRect.sizeDelta = new Vector2(400f, 30f);

                TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
                titleTxt.text = "<b>📦 RƯƠNG ĐỒ DỰ TRỮ GIA ĐÌNH</b>";
                titleTxt.alignment = TextAlignmentOptions.Center;
                titleTxt.fontSize = 18;
                titleTxt.color = new Color(0.95f, 0.8f, 0.3f, 1f);
                if (speakerNameText != null) titleTxt.font = speakerNameText.font;

                // Close Button [X]
                GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                closeBtnObj.transform.SetParent(storageChestPanelObj.transform, false);
                RectTransform cRect = closeBtnObj.GetComponent<RectTransform>();
                cRect.anchorMin = new Vector2(1f, 1f);
                cRect.anchorMax = new Vector2(1f, 1f);
                cRect.pivot = new Vector2(1f, 1f);
                cRect.anchoredPosition = new Vector2(-15f, -15f);
                cRect.sizeDelta = new Vector2(30f, 30f);

                Image cImg = closeBtnObj.GetComponent<Image>();
                cImg.color = new Color(0.6f, 0.2f, 0.2f, 1f);
                Button cBtn = closeBtnObj.GetComponent<Button>();
                cBtn.onClick.AddListener(() => {
                    storageChestPanelObj.SetActive(false);
                });

                GameObject cTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                cTextObj.transform.SetParent(closeBtnObj.transform, false);
                RectTransform ctRect = cTextObj.GetComponent<RectTransform>();
                ctRect.anchorMin = Vector2.zero;
                ctRect.anchorMax = Vector2.one;
                ctRect.sizeDelta = Vector2.zero;
                TextMeshProUGUI cTxt = cTextObj.GetComponent<TextMeshProUGUI>();
                cTxt.text = "X";
                cTxt.alignment = TextAlignmentOptions.Center;
                cTxt.fontSize = 16;
                cTxt.color = Color.white;

                // Grid Container
                GameObject containerObj = new GameObject("GridContainer", typeof(RectTransform), typeof(GridLayoutGroup));
                containerObj.transform.SetParent(storageChestPanelObj.transform, false);
                RectTransform contRect = containerObj.GetComponent<RectTransform>();
                contRect.anchorMin = Vector2.zero;
                contRect.anchorMax = Vector2.one;
                contRect.offsetMin = new Vector2(20f, 20f);
                contRect.offsetMax = new Vector2(-20f, -60f);

                storageChestContainer = containerObj.transform;
                GridLayoutGroup grid = containerObj.GetComponent<GridLayoutGroup>();
                grid.cellSize = new Vector2(80f, 80f);
                grid.spacing = new Vector2(10f, 10f);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 5;
            }

            storageChestPanelObj.transform.SetAsLastSibling();
            storageChestPanelObj.SetActive(true);

            foreach (Transform child in storageChestContainer)
            {
                Destroy(child.gameObject);
            }

            List<InventorySlot> allSlots = new List<InventorySlot>();
            if (StorageManager.Instance != null)
            {
                allSlots.AddRange(StorageManager.Instance.GetStorageSlots());
                allSlots.AddRange(StorageManager.Instance.GetReserveChestSlots());
            }

            foreach (var slot in allSlots)
            {
                if (slot == null || slot.item == null) continue;
                GameObject newSlot = new GameObject("Slot", typeof(RectTransform), typeof(Image), typeof(Outline));
                newSlot.transform.SetParent(storageChestContainer, false);

                RectTransform slotRect = newSlot.GetComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(80f, 80f);

                Image slotImg = newSlot.GetComponent<Image>();
                slotImg.color = new Color(0.15f, 0.12f, 0.09f, 0.95f);

                Outline outline = newSlot.GetComponent<Outline>();
                outline.effectColor = new Color(0.6f, 0.45f, 0.25f, 0.8f);

                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(newSlot.transform, false);
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = new Vector2(60f, 60f);

                Image img = iconObj.GetComponent<Image>();
                if (slot.item.Icon != null) img.sprite = slot.item.Icon;

                GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(newSlot.transform, false);
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(1f, 0f);
                textRect.anchorMax = new Vector2(1f, 0f);
                textRect.pivot = new Vector2(1f, 0f);
                textRect.anchoredPosition = new Vector2(-4f, 4f);
                textRect.sizeDelta = new Vector2(50f, 20f);

                TextMeshProUGUI txt = textObj.GetComponent<TextMeshProUGUI>();
                txt.text = $"x{slot.quantity}";
                txt.fontSize = 14;
                txt.alignment = TextAlignmentOptions.Right;
                txt.color = Color.white;
                if (speakerNameText != null) txt.font = speakerNameText.font;
            }
        }

        public void ToggleInventory()
        {
            EnsureInventoryPanelExists();
            isInventoryOpen = !isInventoryOpen;
            if (inventoryPanel != null)
            {
                inventoryPanel.transform.SetAsLastSibling();
                CanvasGroup cg = inventoryPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = inventoryPanel.AddComponent<CanvasGroup>();
                cg.alpha = isInventoryOpen ? 1f : 0f;
                cg.blocksRaycasts = isInventoryOpen;
                cg.interactable = isInventoryOpen;

                inventoryPanel.SetActive(isInventoryOpen);
                if (isInventoryOpen)
                {
                    // Đóng các panel khác
                    CloseCommunityPanel();
                    CloseWeatherDetailsPanel();

                    RefreshInventoryUI();

                    if (HotbarManager.Instance != null && PlayerPrefs.GetInt("HotbarTutorialShown", 0) == 0)
                    {
                        HotbarManager.Instance.ShowHotbarTutorial();
                    }
                }
            }
        }

        public void RefreshInventoryUI()
        {
            EnsureInventoryPanelExists();
            if (itemSlotContainer == null || itemSlotPrefab == null || StorageManager.Instance == null) return;

            // Xóa sạch ô lưới cũ tránh bị trùng lặp
            foreach (Transform child in itemSlotContainer)
            {
                Destroy(child.gameObject);
            }

            // Tạo các ô lưới mới tổng hợp cả Balo lẫn Rương dự trữ ở nhà
            List<InventorySlot> slots = new List<InventorySlot>();
            slots.AddRange(StorageManager.Instance.GetStorageSlots());
            slots.AddRange(StorageManager.Instance.GetReserveChestSlots());

            foreach (var slot in slots)
            {
                if (slot == null || slot.item == null) continue;
                GameObject newSlot = Instantiate(itemSlotPrefab, itemSlotContainer);
                newSlot.SetActive(true);
                DragItemUI drag = newSlot.GetComponent<DragItemUI>();
                if (drag == null) drag = newSlot.AddComponent<DragItemUI>();
                drag.itemData = slot.item;

                // Căn chỉnh kích thước ô vật phẩm thành hình vuông và thêm style nền/viền
                RectTransform slotRect = newSlot.GetComponent<RectTransform>();
                if (slotRect != null)
                {
                    slotRect.sizeDelta = new Vector2(80f, 80f);
                }

                Image slotBg = newSlot.GetComponent<Image>();
                if (slotBg == null) slotBg = newSlot.AddComponent<Image>();
                slotBg.color = new Color(0.12f, 0.09f, 0.07f, 0.95f); // Nền nâu tối mộc mạc

                Outline slotOutline = newSlot.GetComponent<Outline>();
                if (slotOutline == null) slotOutline = newSlot.AddComponent<Outline>();
                slotOutline.effectColor = new Color(0.45f, 0.35f, 0.25f, 0.8f); // Viền vàng/nâu đồng bộ
                slotOutline.effectDistance = new Vector2(1.5f, 1.5f);

                // Gán hình ảnh biểu tượng vật phẩm và căn chỉnh chiếm trọn ô vuông
                Transform iconTr = newSlot.transform.Find("Icon");
                if (iconTr != null)
                {
                    RectTransform iconRect = iconTr.GetComponent<RectTransform>();
                    if (iconRect != null)
                    {
                        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                        iconRect.pivot = new Vector2(0.5f, 0.5f);
                        iconRect.anchoredPosition = Vector2.zero;
                        iconRect.sizeDelta = new Vector2(60f, 60f); // Icon kích thước 60x60 cân đối
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
                            img.sprite = Resources.Load<Sprite>("UI/gear_icon");
                            if (img.sprite != null)
                            {
                                img.color = Color.white;
                            }
                            else
                            {
                                img.sprite = null;
                                img.color = new Color(0.35f, 0.28f, 0.22f, 0.8f); // Nâu tối trung tính
                            }
                            img.enabled = true;
                        }
                    }
                }

                // Căn chỉnh chữ hiển thị số lượng ở góc dưới bên phải
                TextMeshProUGUI txt = newSlot.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = $"x{slot.quantity}";
                    txt.fontSize = 12;
                    txt.fontStyle = FontStyles.Bold;
                    txt.alignment = TextAlignmentOptions.BottomRight;
                    txt.color = new Color(0.95f, 0.85f, 0.35f, 1f); // Màu vàng gold nổi bật
                    UnityEngine.UI.Shadow shadow = txt.gameObject.GetComponent<UnityEngine.UI.Shadow>();
                    if (shadow == null)
                    {
                        shadow = txt.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                    }
                    shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
                    shadow.effectDistance = new Vector2(1f, -1f);

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

                // Thêm Button Component để nhận click và hiệu ứng hover sáng viền
                Button btn = newSlot.GetComponent<Button>();
                if (btn == null)
                {
                    btn = newSlot.AddComponent<Button>();
                }
                ColorBlock cb = btn.colors;
                cb.normalColor = Color.white;
                cb.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f); // Sáng hơn khi di chuột qua
                cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                cb.selectedColor = Color.white;
                btn.colors = cb;
                
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

            // 6. Tạo thanh cuộn dọc (Vertical Scrollbar) bên cạnh phải của Balo
            GameObject scrollbarObj = new GameObject("VerticalScrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            scrollbarObj.transform.SetParent(inventoryPanel.transform, false);

            RectTransform sbRect = scrollbarObj.GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(1f, 0f);
            sbRect.anchorMax = new Vector2(1f, 1f);
            sbRect.pivot = new Vector2(1f, 0.5f);
            sbRect.anchoredPosition = new Vector2(-8f, -5f);
            sbRect.sizeDelta = new Vector2(16f, -130f); // Rộng 16px, chừa lề trên dưới

            Image sbBg = scrollbarObj.GetComponent<Image>();
            sbBg.color = new Color(0.1f, 0.08f, 0.06f, 0.85f); // Nền thanh cuộn nâu tối

            // Tạo Sliding Area & Handle (Con trượt kéo thả)
            GameObject slidingArea = new GameObject("Sliding Area", typeof(RectTransform));
            slidingArea.transform.SetParent(scrollbarObj.transform, false);
            RectTransform saRect = slidingArea.GetComponent<RectTransform>();
            saRect.anchorMin = Vector2.zero;
            saRect.anchorMax = Vector2.one;
            saRect.sizeDelta = Vector2.zero;

            GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image), typeof(Outline));
            handleObj.transform.SetParent(slidingArea.transform, false);
            RectTransform handleRect = handleObj.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.sizeDelta = Vector2.zero;

            Image handleImg = handleObj.GetComponent<Image>();
            handleImg.color = new Color(0.55f, 0.42f, 0.25f, 0.95f); // Con trượt màu đồng vàng mộc mạc

            Outline handleOutline = handleObj.GetComponent<Outline>();
            handleOutline.effectColor = new Color(0.8f, 0.65f, 0.35f, 0.9f);
            handleOutline.effectDistance = new Vector2(1f, 1f);

            Scrollbar scrollbarComp = scrollbarObj.GetComponent<Scrollbar>();
            scrollbarComp.targetGraphic = handleImg;
            scrollbarComp.handleRect = handleRect;
            scrollbarComp.direction = Scrollbar.Direction.BottomToTop;

            scroll.verticalScrollbar = scrollbarComp;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
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
                        CloseDialogue();
                    },
                    "Hủy bỏ",
                    () => {
                        CloseDialogue();
                    }
                );
            }
            else
            {
                // 1. Xử lý Nón Lá (Equip)
                if (item.ItemID == "item_non_la")
                {
                    if (PlayerController.Instance != null && PlayerController.Instance.isWearingNonLa)
                    {
                        ShowDialogue(item.ItemName, "Bạn đang đội Nón Lá rồi!");
                        return;
                    }

                    ShowDialogueWithChoices(
                        item.ItemName,
                        "Bạn có muốn đội Nón Lá này không?\n\nHiệu quả:\n• Giảm 50% tốc độ tích lũy sốc nhiệt dưới thời tiết nắng nóng Gió Lào.",
                        "Đội nón",
                        () => {
                            if (StorageManager.Instance != null && StorageManager.Instance.RemoveItem(item, 1))
                            {
                                if (PlayerController.Instance != null)
                                {
                                    GameObject hatPrefab = Resources.Load<GameObject>("Prefabs/NonLa");
                                    PlayerController.Instance.EquipNonLa(hatPrefab);
                                }
                                PlayerStats.Instance?.TriggerAlert("Đã đội Nón Lá!");
                            }
                            RefreshInventoryUI();
                            CloseDialogue();
                        },
                        "Hủy bỏ",
                        () => {
                            CloseDialogue();
                        }
                    );
                    return;
                }

                // 2. Xử lý Bao Cát và Tấm Chắn Lũ (Placement)
                if (item.ItemID == "item_sandbag" || item.ItemID == "item_flood_board")
                {
                    bool isSandbag = item.ItemID == "item_sandbag";
                    string actionName = isSandbag ? "Đặt bao cát" : "Dựng tấm chắn";
                    string desc = isSandbag 
                        ? "Bạn có muốn đặt Bao Cát này xuống đất để chặn nước lũ ngập úng không?\n\nHiệu quả:\n• Che chắn úng lụt cho các cây trồng gần đó (bán kính 2.2m)."
                        : "Bạn có muốn dựng Tấm Chắn Lũ này xuống đất để ngăn nước ngập ruộng không?\n\nHiệu quả:\n• Che chắn úng lụt cho các cây trồng gần đó (bán kính 2.2m).";

                    ShowDialogueWithChoices(
                        item.ItemName,
                        desc,
                        actionName,
                        () => {
                            if (StorageManager.Instance != null && StorageManager.Instance.RemoveItem(item, 1))
                            {
                                if (PlayerController.Instance != null)
                                {
                                    string prefabPath = isSandbag ? "Prefabs/Sandbag" : "Prefabs/FloodBoard";
                                    GameObject prefab = Resources.Load<GameObject>(prefabPath);
                                    if (prefab != null)
                                    {
                                        Vector3 spawnPos = PlayerController.Instance.transform.position;
                                        spawnPos.y = 0.0f; // Đặt ngang mặt đất
                                        
                                        // Đặt cách người chơi một khoảng nhỏ phía trước
                                        Vector3 forwardOffset = PlayerController.Instance.transform.forward * 1.0f;
                                        forwardOffset.y = 0f;
                                        Vector3 finalPos = spawnPos + forwardOffset;
                                        bool didMakePrePlacedSolid = false;

                                        if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                                        {
                                            var stage = TutorialManager.Instance.currentStage;
                                            if (stage == TutorialManager.TutorialStage.PrepareForStorm)
                                            {
                                                if (isSandbag)
                                                {
                                                    if (TutorialManager.Instance.ghostSandbags.Count > 0)
                                                    {
                                                        int bestIndex = -1;
                                                        float minDistance = float.MaxValue;
                                                        for (int i = 0; i < TutorialManager.Instance.ghostSandbags.Count; i++)
                                                        {
                                                            if (i >= 4) break;
                                                            if (TutorialManager.Instance.bacNamTargetsPlaced[i]) continue;
                                                            Vector3 spawnPos2D = spawnPos;
                                                            Vector3 targetPos2D = TutorialManager.Instance.ghostSandbags[i].transform.position;
                                                            spawnPos2D.y = 0f;
                                                            targetPos2D.y = 0f;
                                                            float dist = Vector3.Distance(spawnPos2D, targetPos2D);
                                                            if (dist < minDistance)
                                                            {
                                                                minDistance = dist;
                                                                bestIndex = i;
                                                            }
                                                        }
                                                        
                                                        if (bestIndex != -1 && minDistance < 3.5f)
                                                        {
                                                            TutorialManager.Instance.MakeSolidModel(TutorialManager.Instance.ghostSandbags[bestIndex]);
                                                            TutorialManager.Instance.bacNamTargetsPlaced[bestIndex] = true;
                                                            TutorialManager.Instance.OnBacNamSandbagPlaced();
                                                            didMakePrePlacedSolid = true;
                                                        }
                                                        else
                                                        {
                                                            PlayerStats.Instance?.TriggerAlert("Hãy đến gần chấm chỉ dẫn trên mái để đặt!");
                                                            StorageManager.Instance?.AddItem(item, 1);
                                                            RefreshInventoryUI();
                                                            CloseDialogue();
                                                            return;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        GameObject bacNamHouse = GameObject.Find("BacNam_House");
                                                        if (bacNamHouse != null && Vector3.Distance(spawnPos, bacNamHouse.transform.position) < 15f)
                                                        {
                                                            Vector3 roofCenter = bacNamHouse.transform.position + Vector3.up * 3.8f;
                                                            Vector3[] roofOffsets = new Vector3[]
                                                            {
                                                                new Vector3(-1.2f, 0f, -0.8f),
                                                                new Vector3(1.2f, 0.1f, -0.8f),
                                                                new Vector3(-1.2f, 0.1f, 0.8f),
                                                                new Vector3(1.2f, 0f, 0.8f)
                                                            };
                                                            
                                                            int bestIndex = -1;
                                                            float minDistance = float.MaxValue;
                                                            for (int i = 0; i < 4; i++)
                                                            {
                                                                if (TutorialManager.Instance.bacNamTargetsPlaced[i]) continue;
                                                                Vector3 spawnPos2D = spawnPos;
                                                                Vector3 targetPos2D = roofCenter + roofOffsets[i];
                                                                spawnPos2D.y = 0f;
                                                                targetPos2D.y = 0f;
                                                                float dist = Vector3.Distance(spawnPos2D, targetPos2D);
                                                                if (dist < minDistance)
                                                                {
                                                                    minDistance = dist;
                                                                    bestIndex = i;
                                                                }
                                                            }
                                                            
                                                            if (bestIndex != -1 && minDistance < 3.5f)
                                                            {
                                                                finalPos = roofCenter + roofOffsets[bestIndex];
                                                                TutorialManager.Instance.bacNamTargetsPlaced[bestIndex] = true;
                                                                TutorialManager.Instance.OnBacNamSandbagPlaced();
                                                            }
                                                            else
                                                            {
                                                                PlayerStats.Instance?.TriggerAlert("Hãy đến gần chấm chỉ dẫn trên mái để đặt!");
                                                                StorageManager.Instance?.AddItem(item, 1);
                                                                RefreshInventoryUI();
                                                                CloseDialogue();
                                                                return;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (TutorialManager.Instance.ghostFloodboards.Count > 0)
                                                    {
                                                        int bestIndex = -1;
                                                        float minDistance = float.MaxValue;
                                                        for (int i = 0; i < TutorialManager.Instance.ghostFloodboards.Count; i++)
                                                        {
                                                            if (i >= 4) break;
                                                            if (TutorialManager.Instance.oThamTargetsPlaced[i]) continue;
                                                            Vector3 spawnPos2D = spawnPos;
                                                            Vector3 targetPos2D = TutorialManager.Instance.ghostFloodboards[i].transform.position;
                                                            spawnPos2D.y = 0f;
                                                            targetPos2D.y = 0f;
                                                            float dist = Vector3.Distance(spawnPos2D, targetPos2D);
                                                            if (dist < minDistance)
                                                            {
                                                                minDistance = dist;
                                                                bestIndex = i;
                                                            }
                                                        }
                                                        
                                                        if (bestIndex != -1 && minDistance < 3.5f)
                                                        {
                                                            TutorialManager.Instance.MakeSolidModel(TutorialManager.Instance.ghostFloodboards[bestIndex]);
                                                            TutorialManager.Instance.oThamTargetsPlaced[bestIndex] = true;
                                                            TutorialManager.Instance.OnOThamFloodBoardPlaced();
                                                            didMakePrePlacedSolid = true;
                                                        }
                                                        else
                                                        {
                                                            PlayerStats.Instance?.TriggerAlert("Hãy đến gần chấm chỉ dẫn trước cửa tiệm để đặt!");
                                                            StorageManager.Instance?.AddItem(item, 1);
                                                            RefreshInventoryUI();
                                                            CloseDialogue();
                                                            return;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var npcs = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
                                                        var oTham = System.Array.Find(npcs, n => n.characterType == NPCCharacter.StoryCharacterType.OTham);
                                                        if (oTham != null && Vector3.Distance(spawnPos, oTham.transform.position) < 15f)
                                                        {
                                                            Vector3 oThamPos = oTham.transform.position;
                                                            Vector3 forward = oTham.transform.forward;
                                                            Vector3 right = oTham.transform.right;

                                                            Vector3[] targets = new Vector3[]
                                                            {
                                                                oThamPos + forward * 2.5f + right * -1.8f,
                                                                oThamPos + forward * 2.5f + right * -0.6f,
                                                                oThamPos + forward * 2.5f + right * 0.6f,
                                                                oThamPos + forward * 2.5f + right * 1.8f
                                                            };

                                                            int bestIndex = -1;
                                                            float minDistance = float.MaxValue;
                                                            for (int i = 0; i < 4; i++)
                                                            {
                                                                if (TutorialManager.Instance.oThamTargetsPlaced[i]) continue;
                                                                Vector3 spawnPos2D = spawnPos;
                                                                Vector3 targetPos2D = targets[i];
                                                                spawnPos2D.y = 0f;
                                                                targetPos2D.y = 0f;
                                                                float dist = Vector3.Distance(spawnPos2D, targetPos2D);
                                                                if (dist < minDistance)
                                                                {
                                                                    minDistance = dist;
                                                                    bestIndex = i;
                                                                }
                                                            }

                                                            if (bestIndex != -1 && minDistance < 3.5f)
                                                            {
                                                                finalPos = targets[bestIndex];
                                                                TutorialManager.Instance.oThamTargetsPlaced[bestIndex] = true;
                                                                TutorialManager.Instance.OnOThamFloodBoardPlaced();
                                                            }
                                                            else
                                                            {
                                                                PlayerStats.Instance?.TriggerAlert("Hãy đến gần chấm chỉ dẫn trước cửa tiệm để đặt!");
                                                                StorageManager.Instance?.AddItem(item, 1);
                                                                RefreshInventoryUI();
                                                                CloseDialogue();
                                                                return;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (stage == TutorialManager.TutorialStage.PrepareOwnHouse)
                                            {
                                                if (isSandbag)
                                                {
                                                    GameObject ownHouse = GameObject.Find("Thanh_House");
                                                    if (ownHouse != null && Vector3.Distance(spawnPos, ownHouse.transform.position) < 15f)
                                                    {
                                                        Vector3 houseCenter = ownHouse.transform.position;
                                                        Vector3[] houseOffsets = new Vector3[]
                                                        {
                                                            houseCenter + new Vector3(-1.5f, 0.1f, -1.0f),
                                                            houseCenter + new Vector3(-0.5f, 0.1f, -1.0f),
                                                            houseCenter + new Vector3(0.5f, 0.1f, -1.0f),
                                                            houseCenter + new Vector3(1.5f, 0.1f, -1.0f)
                                                        };

                                                        int bestIndex = -1;
                                                        float minDistance = float.MaxValue;
                                                        for (int i = 0; i < 4; i++)
                                                        {
                                                            if (TutorialManager.Instance.ownHouseSandbagsPlaced > i) continue;
                                                            float dist = Vector3.Distance(spawnPos, houseOffsets[i]);
                                                            if (dist < minDistance)
                                                            {
                                                                minDistance = dist;
                                                                bestIndex = i;
                                                            }
                                                        }

                                                        if (bestIndex != -1 && minDistance < 3f)
                                                        {
                                                            finalPos = houseOffsets[bestIndex];
                                                            TutorialManager.Instance.OnOwnHouseSandbagPlaced();
                                                        }
                                                        else
                                                        {
                                                            PlayerStats.Instance?.TriggerAlert("Hãy đến gần chấm chỉ dẫn trước nhà để đặt!");
                                                            StorageManager.Instance?.AddItem(item, 1);
                                                            RefreshInventoryUI();
                                                            CloseDialogue();
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (!didMakePrePlacedSolid)
                                        {
                                            GameObject deployed = Instantiate(prefab, finalPos, Quaternion.identity);
                                            deployed.name = isSandbag ? "Deployed_Sandbag" : "Deployed_FloodBoard";
                                        }

                                        PlayerStats.Instance?.TriggerAlert($"Đã đặt {item.ItemName}!");
                                    }
                                    else
                                    {
                                        Debug.LogError($"[SURVIVAL UI] Không tìm thấy prefab {prefabPath} trong Resources!");
                                    }
                                }
                            }
                            RefreshInventoryUI();
                            CloseDialogue();
                        },
                        "Hủy bỏ",
                        () => {
                            CloseDialogue();
                        }
                    );
                    return;
                }

                // Đối với các vật phẩm không dùng trực tiếp khác (Nhang, Hạt giống, Vật liệu...)
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

            CloseDialogue();

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
            if (villageSpeakerBanner != null)
            {
                string title = "";
                string message = "";
                int day = GameManager.Instance != null ? GameManager.Instance.CurrentDay : 1;

                switch (newPhase)
                {
                    case GamePhase.LapNghiep:
                        title = "TIẾNG TRỐNG ĐÌNH LÀNG";
                        message = "Loa phát thanh xã xin thông báo: Hôm nay bà con ra đồng dọn ruộng, chuẩn bị vụ khoai đầu mùa. Mong mọi người giúp đỡ nhau, giữ gìn tình làng nghĩa xóm.";
                        break;
                    case GamePhase.GioLao:
                        title = "NẮNG CHÁY GIÓ LÀO";
                        message = "Loa phát thanh xã xin thông báo: Gió Lào đang thổi mạnh, bà con hạn chế ra đồng giữa trưa, tiết kiệm nước tưới và chú ý giữ sức khỏe.";
                        break;
                    case GamePhase.MuaBao:
                        title = "TÌNH NGƯỜI TRONG MƯA BÃO";
                        message = "Loa phát thanh xã xin thông báo: Bão lớn đang tiến vào đất liền. Đề nghị bà con chằng chống nhà cửa, kê cao lương thực và hỗ trợ các hộ neo đơn.";
                        break;
                    case GamePhase.PhuSa:
                        title = "PHÙ SA SAU CƠN LŨ";
                        message = "Loa phát thanh xã xin thông báo: Nước lũ đã rút. Bà con khẩn trương dọn bùn, khôi phục ruộng vườn và chia sẻ hạt giống để tái thiết sản xuất.";
                        break;
                }

                villageSpeakerBanner.ShowAnnouncement(title, message, day);
            }

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
            r.anchorMin = new Vector2(0.5f, 0.85f); // Upper middle
            r.anchorMax = new Vector2(0.5f, 0.85f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0f, 0f);
            r.sizeDelta = new Vector2(450f, 45f);

            Image bgImg = toastPanel.GetComponent<Image>();
            bgImg.color = new Color(0.10f, 0.08f, 0.06f, 0.95f); // Tông nâu tối sang trọng đồng bộ

            // Thêm viền nhỏ cho tinh tế sang trọng
            Outline outline = toastPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.38f, 0.30f, 0.22f, 1f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

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
            toastText.color = Color.white; // Màu trắng mặc định để các phần highlight nổi bật
            if (font != null) toastText.font = font;

            toastText.outlineColor = Color.black;
            toastText.outlineWidth = 0.20f;
            toastText.text = "";
        }

        private GameObject controlsLegendPanel;
        private bool isControlsLegendVisible = true;

        private void CreateControlsLegendUI()
        {
            TMP_FontAsset font = speakerNameText != null ? speakerNameText.font : null;

            // Container Panel
            controlsLegendPanel = new GameObject("ControlsLegendHUD", typeof(RectTransform), typeof(Image));
            controlsLegendPanel.transform.SetParent(this.transform, false);

            RectTransform r = controlsLegendPanel.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(1f, 1f); // Top Right
            r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(1f, 1f);
            r.anchoredPosition = new Vector2(-20f, -120f);
            r.sizeDelta = new Vector2(220f, 220f);

            Image bgImg = controlsLegendPanel.GetComponent<Image>();
            bgImg.color = new Color(0.08f, 0.06f, 0.05f, 0.8f);

            Outline outline = controlsLegendPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.38f, 0.30f, 0.22f, 1f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Vertical Layout Group
            VerticalLayoutGroup layout = controlsLegendPanel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 3f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            // Title
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(controlsLegendPanel.transform, false);
            TextMeshProUGUI title = titleObj.GetComponent<TextMeshProUGUI>();
            title.text = "Điều khiển";
            title.fontSize = 13.5f;
            title.fontStyle = FontStyles.Bold;
            title.color = new Color(0.95f, 0.85f, 0.4f, 1f);
            if (font != null) title.font = font;

            // Lines
            string[] lines = new string[] {
                "<b>WASD</b>   Di chuyển",
                "<b>RMB</b>    Xoay camera",
                "<b>Scroll</b> Zoom camera",
                "<b>E</b>      Làm nông / Thắp nhang",
                "<b>1-3</b>    Chọn NPC",
                "<b>I / Tab</b> Túi đồ",
                "<b>C</b>      Nghĩa Tình",
                "<b>M</b>      Thời tiết",
                "<b>F1</b>     Demo",
                "<color=#9f856f><b>H</b>      Ẩn/hiện hướng dẫn</color>"
            };

            foreach (var txtLine in lines)
            {
                GameObject lineObj = new GameObject("LegendLine", typeof(RectTransform), typeof(TextMeshProUGUI));
                lineObj.transform.SetParent(controlsLegendPanel.transform, false);
                TextMeshProUGUI txt = lineObj.GetComponent<TextMeshProUGUI>();
                txt.text = txtLine;
                txt.fontSize = 11f;
                txt.color = Color.white;
                if (font != null) txt.font = font;
            }
        }

        public void ToggleControlsLegend()
        {
            if (controlsLegendPanel != null)
            {
                isControlsLegendVisible = !isControlsLegendVisible;
                controlsLegendPanel.SetActive(isControlsLegendVisible);
            }
        }

        public void ShowHUDToast(string message)
        {
            if (toastPanel == null || toastText == null || toastCanvasGroup == null) return;

            // Làm nổi bật phần thưởng/thông báo
            string highlightedMessage = message;
            
            // Xử lý Nghĩa Tình (Ví dụ: +5 Nghĩa Tình, +10 Nghĩa Tình, +15 Nghĩa Tình, +20 Nghĩa Tình)
            highlightedMessage = System.Text.RegularExpressions.Regex.Replace(
                highlightedMessage, 
                @"\+(\d+)\s+Nghĩa\s+Tình", 
                "<color=#F4D03F>+$1 Nghĩa Tình</color>"
            );

            // Xử lý Vần công (Ví dụ: +1 Vần công)
            highlightedMessage = System.Text.RegularExpressions.Regex.Replace(
                highlightedMessage, 
                @"\+(\d+)\s+Vần\s+công", 
                "<color=#2ECC71>+$1 Vần công</color>"
            );

            // Xử lý không đủ tài nguyên
            if (highlightedMessage.Contains("Không đủ") || highlightedMessage.Contains("chưa có đủ") || highlightedMessage.Contains("không có đủ"))
            {
                highlightedMessage = $"<color=#E74C3C>{highlightedMessage}</color>";
            }

            if (toastCoroutine != null)
            {
                StopCoroutine(toastCoroutine);
            }
            toastCoroutine = StartCoroutine(HUDToastCoroutine(highlightedMessage));
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
            r.sizeDelta = new Vector2(500f, 580f);

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
            listRect.sizeDelta = new Vector2(460f, 480f);

            // Tạo các dòng vật phẩm (8 dòng)
            CreateShopRow(listContainer.transform, 0, "Hạt giống Khoai", "Dùng gieo trồng khoai lang", true, () => GetSeedPrice(), seedItem, font);
            CreateShopRow(listContainer.transform, 1, "Nhang cúng", "Dùng thắp nhang ban thờ gia tiên", true, () => 15, incenseItem, font);
            CreateShopRow(listContainer.transform, 2, "Khoai lang tươi", "Nông sản tươi thu hoạch từ ruộng", false, () => 25, freshCropItem, font);
            CreateShopRow(listContainer.transform, 3, "Khoai gieo khô", "Đặc sản phơi khô tích lũy chống lũ", false, () => 40, preservedCropItem, font);
            CreateShopRow(listContainer.transform, 4, "Mì Tôm Cứu Trợ", "Mì tôm ăn liền cứu trợ khẩn cấp", true, () => 15, noodlesItem, font);
            CreateShopRow(listContainer.transform, 5, "Nón Lá Truyền Thống", "Che nắng mưa, giảm 50% sốc nhiệt", true, () => 15, nonLaItem, font);
            CreateShopRow(listContainer.transform, 6, "Bao Cát Chống Lũ", "Dùng cản nước bảo vệ hoa màu ruộng", true, () => 8, sandbagItem, font);
            CreateShopRow(listContainer.transform, 7, "Tấm Chắn Lũ", "Tấm gỗ chắn nước bảo vệ ruộng vườn", true, () => 12, floodBoardItem, font);

            shopPanel.SetActive(false); // Ẩn mặc định
        }

        private int GetSeedPrice()
        {
            bool isPhuSa = (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.PhuSa);
            return isPhuSa ? 6 : 10;
        }

        private void CreateShopRow(Transform parent, int index, string itemName, string desc, bool isBuy, System.Func<int> priceFunc, ItemData item, TMP_FontAsset font)
        {
            float yPos = 210f - index * 60f; // Cách nhau 60 units chiều dọc để dàn đều 8 hàng trong container 480f

            GameObject row = new GameObject($"Row_{index}", typeof(RectTransform), typeof(Image));
            row.transform.SetParent(parent, false);
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 0.5f);
            rowRect.anchorMax = new Vector2(0.5f, 0.5f);
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = new Vector2(0f, yPos);
            rowRect.sizeDelta = new Vector2(450f, 60f); // Tăng chiều cao lên 60f

            row.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.05f, 0.75f); // Nền dòng nâu tối mộc mạc bán trong suốt
            
            Outline rowOutline = row.AddComponent<Outline>();
            rowOutline.effectColor = new Color(0.38f, 0.30f, 0.22f, 0.4f); // Viền nâu sáng tinh tế
            rowOutline.effectDistance = new Vector2(1f, 1f);

            // Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(row.transform, false);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(10f, 0f);
            iconRect.sizeDelta = new Vector2(56f, 56f); // Tăng kích thước icon lên 56f

            Image iconImg = iconObj.GetComponent<Image>();
            if (item != null && item.Icon != null)
            {
                iconImg.sprite = item.Icon;
                iconImg.color = Color.white;
            }
            else
            {
                iconImg.sprite = Resources.Load<Sprite>("UI/gear_icon");
                if (iconImg.sprite != null)
                {
                    iconImg.color = Color.white;
                }
                else
                {
                    iconImg.sprite = null;
                    iconImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                }
            }

            // Info Text (Tên, Mô tả và Số lượng đang có)
            GameObject infoObj = new GameObject("InfoText", typeof(RectTransform), typeof(TextMeshProUGUI));
            infoObj.transform.SetParent(row.transform, false);
            RectTransform infoRect = infoObj.GetComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0f, 0.5f);
            infoRect.anchorMax = new Vector2(1f, 0.5f);
            infoRect.pivot = new Vector2(0f, 0.5f);
            infoRect.anchoredPosition = new Vector2(76f, 0f); // Dịch chuyển để không đè icon
            infoRect.sizeDelta = new Vector2(235f, 50f);

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
            btnRect.sizeDelta = new Vector2(110f, 40f); // Tăng chiều cao nút lên 40f

            btnObj.GetComponent<Image>().color = isBuy ? new Color(0.18f, 0.48f, 0.25f, 0.95f) : new Color(0.75f, 0.38f, 0.15f, 0.95f); // Xanh lá mua, Cam đất bán
            
            Outline btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0.08f, 0.06f, 0.05f, 0.4f);
            btnOutline.effectDistance = new Vector2(1f, 1f);

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
            
            // Đăng ký sự kiện click mở popup nhập số lượng
            if (isBuy)
            {
                btn.onClick.AddListener(() => OpenQuantityPopup(item, priceFunc(), true));
            }
            else
            {
                btn.onClick.AddListener(() => OpenQuantityPopup(item, priceFunc(), false));
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
                new { name = "Mì Tôm Cứu Trợ", desc = "Mì tôm ăn liền cứu trợ khẩn cấp", isBuy = true, price = 15, item = noodlesItem },
                new { name = "Nón Lá Truyền Thống", desc = "Che nắng mưa, giảm 50% sốc nhiệt", isBuy = true, price = 15, item = nonLaItem },
                new { name = "Bao Cát Chống Lũ", desc = "Dùng cản nước bảo vệ hoa màu ruộng", isBuy = true, price = 8, item = sandbagItem },
                new { name = "Tấm Chắn Lũ", desc = "Tấm gỗ chắn nước bảo vệ ruộng vườn", isBuy = true, price = 12, item = floodBoardItem }
            };

            // 3. Quét qua từng hàng để cập nhật văn bản và nút bấm
            for (int i = 0; i < shopItemRows.Count && i < itemParams.Length; i++)
            {
                GameObject row = shopItemRows[i];
                var param = itemParams[i];

                if (row == null || param.item == null) continue;

                // Lấy số lượng đang sở hữu trong Inventory (bao gồm cả balo và rương)
                int ownedCount = 0;
                if (StorageManager.Instance != null)
                {
                    var backpackSlot = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == param.item.ItemID);
                    var chestSlot = StorageManager.Instance.GetReserveChestSlots().Find(s => s.item != null && s.item.ItemID == param.item.ItemID);
                    if (backpackSlot != null) ownedCount += backpackSlot.quantity;
                    if (chestSlot != null) ownedCount += chestSlot.quantity;
                }

                // Cập nhật Info Text
                TextMeshProUGUI infoTxt = row.transform.Find("InfoText")?.GetComponent<TextMeshProUGUI>();
                if (infoTxt != null)
                {
                    infoTxt.text = $"<b><color=#F4D03F>{param.name}</color></b>  <size=10><color=#D1C7BD>(Đang có: {ownedCount})</color></size>\n<color=#B3A394><size=9.5>{param.desc}</size></color>";
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

        // --- PHÂN HỆ POPUP NHẬP SỐ LƯỢNG (QUANTITY POPUP) ---
        private bool isQuantityPopupOpen = false;
        private ItemData popupTargetItem;
        private int popupUnitPrice = 0;
        private bool popupIsBuy = true;
        private string popupQuantityStr = "1";
        
        private void OpenQuantityPopup(ItemData item, int unitPrice, bool isBuy)
        {
            popupTargetItem = item;
            popupUnitPrice = unitPrice;
            popupIsBuy = isBuy;
            popupQuantityStr = "1";
            isQuantityPopupOpen = true;
        }

        private void OnGUI()
        {
            if (!isQuantityPopupOpen || popupTargetItem == null) return;

            // Làm mờ nền phía sau
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Kích thước của Panel Popup
            float width = 380f;
            float height = 250f;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            // Vẽ viền và box nền cho Popup
            GUI.color = new Color(0.12f, 0.12f, 0.12f, 0.98f);
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
            GUI.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            GUI.DrawTexture(new Rect(x, y, width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y + height - 2, width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y, 2, height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x + width - 2, y, 2, height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(x + 25, y + 25, width - 50, height - 50));

            GUIStyle titleStyle = new GUIStyle();
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = new Color(0.96f, 0.85f, 0.6f);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.fontSize = 16;

            GUIStyle labelStyle = new GUIStyle();
            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            labelStyle.fontSize = 13;

            string actionType = popupIsBuy ? "MUA VẬT PHẨM" : "BÁN VẬT PHẨM";
            GUILayout.Label($"<b>{actionType}: {popupTargetItem.ItemName}</b>", titleStyle);
            GUILayout.Space(15);

            int owned = 0;
            if (StorageManager.Instance != null)
            {
                var backpackSlot = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == popupTargetItem.ItemID);
                var chestSlot = StorageManager.Instance.GetReserveChestSlots().Find(s => s.item != null && s.item.ItemID == popupTargetItem.ItemID);
                if (backpackSlot != null) owned += backpackSlot.quantity;
                if (chestSlot != null) owned += chestSlot.quantity;
            }
            GUILayout.Label($"Đơn giá: <color=#F4D03F>{popupUnitPrice} Xu</color>  |  Đang sở hữu: {owned}", labelStyle);
            GUILayout.Space(12);

            // Hàng nhập số lượng
            GUILayout.BeginHorizontal();
            GUILayout.Label("Số lượng:", labelStyle, GUILayout.Width(80));
            
            GUIStyle btnQStyle = new GUIStyle(GUI.skin.button);
            btnQStyle.fontStyle = FontStyle.Bold;
            btnQStyle.fontSize = 13;

            if (GUILayout.Button("-", btnQStyle, GUILayout.Width(35), GUILayout.Height(25)))
            {
                if (int.TryParse(popupQuantityStr, out int q))
                {
                    popupQuantityStr = Mathf.Max(1, q - 1).ToString();
                }
            }
            
            GUIStyle tfStyle = new GUIStyle(GUI.skin.textField);
            tfStyle.alignment = TextAnchor.MiddleCenter;
            tfStyle.fontSize = 13;
            popupQuantityStr = GUILayout.TextField(popupQuantityStr, tfStyle, GUILayout.Width(70), GUILayout.Height(25));
            
            if (GUILayout.Button("+", btnQStyle, GUILayout.Width(35), GUILayout.Height(25)))
            {
                if (int.TryParse(popupQuantityStr, out int q))
                {
                    // Giới hạn bán không quá số lượng sở hữu
                    if (!popupIsBuy && q >= owned)
                    {
                        popupQuantityStr = owned.ToString();
                    }
                    else
                    {
                        popupQuantityStr = (q + 1).ToString();
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            // Tính toán tổng tiền
            int.TryParse(popupQuantityStr, out int quantity);
            if (quantity < 1) quantity = 1;
            
            // Nếu bán, không cho vượt quá số lượng sở hữu
            if (!popupIsBuy && quantity > owned)
            {
                quantity = owned;
                popupQuantityStr = owned.ToString();
            }

            int totalCost = quantity * popupUnitPrice;

            GUIStyle sumStyle = new GUIStyle(labelStyle) { fontStyle = FontStyle.Bold, fontSize = 14 };
            if (popupIsBuy)
            {
                sumStyle.normal.textColor = new Color(0.9f, 0.3f, 0.3f);
                GUILayout.Label($"Tổng tiền thanh toán: {totalCost} Xu", sumStyle);
            }
            else
            {
                sumStyle.normal.textColor = new Color(0.3f, 0.9f, 0.3f);
                GUILayout.Label($"Tổng tiền nhận được: +{totalCost} Xu", sumStyle);
            }
            GUILayout.Space(15);

            // Nút Xác nhận / Hủy
            GUILayout.BeginHorizontal();
            GUIStyle btnActionStyle = new GUIStyle(GUI.skin.button);
            btnActionStyle.fontStyle = FontStyle.Bold;
            btnActionStyle.fontSize = 13;

            GUI.backgroundColor = popupIsBuy ? new Color(0.8f, 0.3f, 0.3f) : new Color(0.3f, 0.8f, 0.3f);
            if (GUILayout.Button("Xác Nhận", btnActionStyle, GUILayout.Height(35)))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                ExecutePopupTransaction(quantity, totalCost);
            }

            GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f);
            if (GUILayout.Button("Hủy Bỏ", btnActionStyle, GUILayout.Height(35)))
            {
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
                isQuantityPopupOpen = false;
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void ExecutePopupTransaction(int quantity, int totalCost)
        {
            if (popupTargetItem == null) return;
            if (PlayerStats.Instance == null || StorageManager.Instance == null) return;

            if (popupIsBuy)
            {
                if (PlayerStats.Instance.Coins >= totalCost)
                {
                    PlayerStats.Instance.ModifyCoins(-totalCost);
                    StorageManager.Instance.AddItem(popupTargetItem, quantity);
                    ShowHUDToast($"Đã mua {quantity} {popupTargetItem.ItemName} (-{totalCost} Xu)");
                    isQuantityPopupOpen = false;
                    RefreshShopUI();
                    RefreshInventoryUI();
                }
                else
                {
                    ShowHUDToast("Gia đình không đủ tiền xu!");
                }
            }
            else
            {
                int owned = 0;
                var backpackSlot = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == popupTargetItem.ItemID);
                var chestSlot = StorageManager.Instance.GetReserveChestSlots().Find(s => s.item != null && s.item.ItemID == popupTargetItem.ItemID);
                if (backpackSlot != null) owned += backpackSlot.quantity;
                if (chestSlot != null) owned += chestSlot.quantity;

                if (owned >= quantity)
                {
                    if (StorageManager.Instance.RemoveItem(popupTargetItem, quantity))
                    {
                        PlayerStats.Instance.ModifyCoins(totalCost);
                        ShowHUDToast($"Đã bán {quantity} {popupTargetItem.ItemName} (+{totalCost} Xu)");

                        // Cập nhật tiến độ bán khoai cho O Thắm trong hướng dẫn
                        if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive && TutorialManager.Instance.currentStage == TutorialManager.TutorialStage.SellCrops)
                        {
                            if (popupTargetItem.ItemID == freshCropItem.ItemID)
                            {
                                TutorialManager.Instance.freshCropsSold += quantity;
                                if (TutorialManager.Instance.freshCropsSold >= 12)
                                {
                                    TutorialManager.Instance.OnCropsSold();
                                }
                                else
                                {
                                    TutorialManager.Instance.UpdateHUDPanel();
                                }
                            }
                        }

                        isQuantityPopupOpen = false;
                        RefreshShopUI();
                        RefreshInventoryUI();
                    }
                }
                else
                {
                    ShowHUDToast($"Không đủ {popupTargetItem.ItemName} để bán!");
                }
            }
        }

        /// <summary>
        /// Ẩn hoặc hiển thị toàn bộ HUD gameplay (ví dụ khi hiện Ending Panel).
        /// </summary>
        public void SetHUDVisible(bool visible)
        {
            if (mainUICanvasGroup != null)
            {
                mainUICanvasGroup.alpha = visible ? 1f : 0f;
                mainUICanvasGroup.interactable = visible;
                mainUICanvasGroup.blocksRaycasts = visible;
            }
        }

        private void OnPhaseChangedHandler(GamePhase newPhase)
        {
            if (newPhase == GamePhase.MuaBao)
            {
                ShowPhase3QuestPopup();
            }
        }

        private GameObject phase3QuestPanelObj;

        /// <summary>
        /// Hiển thị bảng thông báo danh sách Nhiệm Vụ Sinh Tồn Mùa Bão Lũ (Phase 3).
        /// </summary>
        public void ShowPhase3QuestPopup()
        {
            if (phase3QuestPanelObj != null)
            {
                phase3QuestPanelObj.SetActive(true);
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            phase3QuestPanelObj = new GameObject("Phase3QuestPanel", typeof(RectTransform), typeof(Image), typeof(Outline));
            phase3QuestPanelObj.transform.SetParent(canvas.transform, false);
            phase3QuestPanelObj.transform.SetAsLastSibling();

            RectTransform panelRect = phase3QuestPanelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(540f, 360f);

            Image bg = phase3QuestPanelObj.GetComponent<Image>();
            bg.color = new Color(0.14f, 0.11f, 0.08f, 0.98f);

            Outline outline = phase3QuestPanelObj.GetComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.7f, 0.35f, 1f);
            outline.effectDistance = new Vector2(2.5f, 2.5f);

            // 1. Tiêu đề
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(phase3QuestPanelObj.transform, false);
            TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
            titleTxt.text = "⚠️ DANH SÁCH NHIỆM VỤ MÙA BÃO LŨ (PHASE 3)";
            titleTxt.fontSize = 17;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = new Color(0.95f, 0.85f, 0.35f, 1f);

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -15f);
            titleRect.sizeDelta = new Vector2(0f, 40f);

            // 2. Nội dung các nhiệm vụ
            GameObject bodyObj = new GameObject("BodyText", typeof(RectTransform), typeof(TextMeshProUGUI));
            bodyObj.transform.SetParent(phase3QuestPanelObj.transform, false);
            TextMeshProUGUI bodyTxt = bodyObj.GetComponent<TextMeshProUGUI>();
            bodyTxt.text = "<b>1. 🌾 PHỦ MÀNG NILON BẢO VỆ RUỘNG:</b>\n" +
                           "   Dùng Màng bọc Nilon bọc toàn bộ các ô ruộng để khoai không bị ngập úng.\n\n" +
                           "<b>2. 🏠 GIA CỐ NÓC NHÀ & SINH TỒN TRONG NHÀ:</b>\n" +
                           "   Đặt Bao cát <b>[5]</b> lên nóc nhà chằng chống mái, vào nhà trú ẩn và ăn khoai gieo khô từ rương.\n\n" +
                           "<b>3. 🧡 CỨU TRỢ NGHĨA TÌNH DÂN LÀNG:</b>\n" +
                           "   Gặp <b>Bác Năm</b> & <b>O Thắm</b> bấm phím <b>[4]</b> tặng Tấm Chắn Lũ để gia cố nhà nhận điểm Nghĩa Tình.";
            bodyTxt.fontSize = 13.5f;
            bodyTxt.lineSpacing = 6;
            bodyTxt.alignment = TextAlignmentOptions.Left;
            bodyTxt.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            RectTransform bodyRect = bodyObj.GetComponent<RectTransform>();
            bodyRect.anchorMin = Vector2.zero;
            bodyRect.anchorMax = Vector2.one;
            bodyRect.offsetMin = new Vector2(25f, 70f);
            bodyRect.offsetMax = new Vector2(-25f, -60f);

            // 3. Nút Đồng Ý / Đã Nhận
            GameObject btnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnObj.transform.SetParent(phase3QuestPanelObj.transform, false);

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0f, 15f);
            btnRect.sizeDelta = new Vector2(220f, 42f);

            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = new Color(0.45f, 0.32f, 0.18f, 1f);

            Outline btnOutline = btnObj.GetComponent<Outline>();
            btnOutline.effectColor = new Color(0.8f, 0.7f, 0.4f, 1f);

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => {
                phase3QuestPanelObj.SetActive(false);
            });

            GameObject btnTextObj = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTextObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI btnTxt = btnTextObj.GetComponent<TextMeshProUGUI>();
            btnTxt.text = "ĐÃ NHẬN NHIỆM VỤ";
            btnTxt.fontSize = 13;
            btnTxt.fontStyle = FontStyles.Bold;
            btnTxt.alignment = TextAlignmentOptions.Center;
            btnTxt.color = Color.white;

            RectTransform btnTxtRect = btnTextObj.GetComponent<RectTransform>();
            btnTxtRect.anchorMin = Vector2.zero;
            btnTxtRect.anchorMax = Vector2.one;
            btnTxtRect.sizeDelta = Vector2.zero;
        }

        #endregion

        #region SCREEN FADE OVERLAY
        private CanvasGroup fadeOverlayCanvasGroup;

        private void CreateFadeOverlay()
        {
            if (fadeOverlayCanvasGroup != null) return;
            GameObject fadeObj = new GameObject("Panel_ScreenFadeOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            fadeObj.transform.SetParent(this.transform, false);
            fadeObj.transform.SetAsLastSibling();

            RectTransform rt = fadeObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image img = fadeObj.GetComponent<Image>();
            img.color = Color.black;

            fadeOverlayCanvasGroup = fadeObj.GetComponent<CanvasGroup>();
            fadeOverlayCanvasGroup.alpha = 0f;
            fadeOverlayCanvasGroup.blocksRaycasts = false;
        }

        public Coroutine FadeToBlack(float duration)
        {
            if (fadeOverlayCanvasGroup == null) CreateFadeOverlay();
            fadeOverlayCanvasGroup.transform.SetAsLastSibling();
            return StartCoroutine(FadeRoutine(fadeOverlayCanvasGroup.alpha, 1f, duration));
        }

        public Coroutine FadeFromBlack(float duration)
        {
            if (fadeOverlayCanvasGroup == null) CreateFadeOverlay();
            fadeOverlayCanvasGroup.transform.SetAsLastSibling();
            return StartCoroutine(FadeRoutine(fadeOverlayCanvasGroup.alpha, 0f, duration));
        }

        private System.Collections.IEnumerator FadeRoutine(float startAlpha, float targetAlpha, float duration)
        {
            fadeOverlayCanvasGroup.blocksRaycasts = (targetAlpha > 0.5f);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeOverlayCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }
            fadeOverlayCanvasGroup.alpha = targetAlpha;
            fadeOverlayCanvasGroup.blocksRaycasts = (targetAlpha > 0.5f);
        }
        #endregion
    }
}
