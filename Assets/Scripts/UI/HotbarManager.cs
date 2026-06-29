using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SownInStone.Storage;
using SownInStone.Core;

namespace SownInStone.UI
{
    public class HotbarSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        public int slotIndex;
        public HotbarManager manager;

        public void OnDrop(PointerEventData eventData)
        {
            if (DragItemUI.DraggedItem != null && manager != null)
            {
                manager.AssignItemToSlot(slotIndex, DragItemUI.DraggedItem);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (manager != null)
                {
                    manager.OnHotbarSlotClicked(slotIndex);
                }
            }
        }
    }

    public class DragItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public static ItemData DraggedItem;
        public ItemData itemData;
        private Canvas parentCanvas;
        private GameObject dragVisual;
        private CanvasGroup canvasGroup;

        private void Start()
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemData == null) return;
            DraggedItem = itemData;

            dragVisual = new GameObject("DragVisual", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            dragVisual.transform.SetParent(parentCanvas != null ? parentCanvas.transform : transform.root, false);
            dragVisual.transform.SetAsLastSibling();

            Image img = dragVisual.GetComponent<Image>();
            img.sprite = itemData.Icon;
            img.raycastTarget = false;

            RectTransform rt = dragVisual.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(60f, 60f);

            canvasGroup = dragVisual.GetComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.8f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragVisual != null && parentCanvas != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentCanvas.transform as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint
                );
                dragVisual.GetComponent<RectTransform>().anchoredPosition = localPoint;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragVisual != null)
            {
                Destroy(dragVisual);
            }
            DraggedItem = null;
        }
    }

    /// <summary>
    /// Quản lý Thanh vật phẩm nhanh 5 ô ở phía dưới màn hình (Hotbar / Quickbar System).
    /// Hỗ trợ kéo thả từ Balo (Inventory) và sử dụng phím 1..5.
    /// </summary>
    public class HotbarManager : MonoBehaviour
    {
        public static HotbarManager Instance { get; private set; }

        [Header("--- HOTBAR CONFIGURATION ---")]
        [Tooltip("Danh sách 5 vật phẩm gán tương ứng phím 1 đến 5.")]
        public ItemData[] assignedItems = new ItemData[5];

        [SerializeField] private GameObject hotbarPanel;
        [SerializeField] private Transform slotsContainer;

        private List<GameObject> slotUIList = new List<GameObject>();

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
            AutoAssignDefaultItems();
            CreateHotbarUI();
            RefreshHotbarUI();

            if (StorageManager.Instance != null)
            {
                StorageManager.Instance.OnStorageChanged += RefreshHotbarUI;
            }
        }

        private void OnDestroy()
        {
            if (StorageManager.Instance != null)
            {
                StorageManager.Instance.OnStorageChanged -= RefreshHotbarUI;
            }
        }

        private void AutoAssignDefaultItems()
        {
            // Tự động nạp mặc định các vật phẩm tiện ích nếu chưa gán
#if UNITY_EDITOR
            if (assignedItems[0] == null)
                assignedItems[0] = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_seed_potato.asset");
            if (assignedItems[1] == null)
                assignedItems[1] = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_sandbag.asset");
            if (assignedItems[2] == null)
                assignedItems[2] = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_flood_board.asset");
            if (assignedItems[3] == null)
                assignedItems[3] = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_non_la.asset");
            if (assignedItems[4] == null)
                assignedItems[4] = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_incense.asset");
#endif
        }

        private void CreateHotbarUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            if (hotbarPanel == null)
            {
                hotbarPanel = new GameObject("HotbarPanel", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
                hotbarPanel.transform.SetParent(canvas.transform, false);

                RectTransform panelRect = hotbarPanel.GetComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0f);
                panelRect.anchorMax = new Vector2(0.5f, 0f);
                panelRect.pivot = new Vector2(0.5f, 0f);
                panelRect.anchoredPosition = new Vector2(0f, 15f);
                panelRect.sizeDelta = new Vector2(380f, 75f);

                Image bg = hotbarPanel.GetComponent<Image>();
                bg.color = new Color(0.12f, 0.09f, 0.07f, 0.85f);

                Outline outline = hotbarPanel.AddComponent<Outline>();
                outline.effectColor = new Color(0.45f, 0.35f, 0.25f, 0.9f);
                outline.effectDistance = new Vector2(2f, 2f);

                HorizontalLayoutGroup layout = hotbarPanel.GetComponent<HorizontalLayoutGroup>();
                layout.padding = new RectOffset(10, 10, 8, 8);
                layout.spacing = 8f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                slotsContainer = hotbarPanel.transform;
            }

            foreach (Transform child in slotsContainer)
            {
                Destroy(child.gameObject);
            }
            slotUIList.Clear();

            for (int i = 0; i < 5; i++)
            {
                GameObject slot = new GameObject($"HotbarSlot_{i + 1}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(HotbarSlotUI));
                slot.transform.SetParent(slotsContainer, false);

                RectTransform slotRect = slot.GetComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(62f, 62f);

                Image slotBg = slot.GetComponent<Image>();
                slotBg.color = new Color(0.18f, 0.14f, 0.10f, 0.95f);

                Outline slotOutline = slot.GetComponent<Outline>();
                slotOutline.effectColor = new Color(0.55f, 0.45f, 0.30f, 0.8f);
                slotOutline.effectDistance = new Vector2(1.5f, 1.5f);

                HotbarSlotUI slotScript = slot.GetComponent<HotbarSlotUI>();
                slotScript.slotIndex = i;
                slotScript.manager = this;

                // 1. Nhãn số phím tắt (1..5) góc trên bên trái
                GameObject keyTextObj = new GameObject("KeyText", typeof(RectTransform), typeof(TextMeshProUGUI));
                keyTextObj.transform.SetParent(slot.transform, false);
                TextMeshProUGUI keyTxt = keyTextObj.GetComponent<TextMeshProUGUI>();
                keyTxt.text = (i + 1).ToString();
                keyTxt.fontSize = 13;
                keyTxt.fontStyle = FontStyles.Bold;
                keyTxt.color = new Color(0.95f, 0.85f, 0.4f, 1f);

                RectTransform keyRect = keyTextObj.GetComponent<RectTransform>();
                keyRect.anchorMin = new Vector2(0f, 1f);
                keyRect.anchorMax = new Vector2(0f, 1f);
                keyRect.pivot = new Vector2(0f, 1f);
                keyRect.anchoredPosition = new Vector2(4f, -2f);
                keyRect.sizeDelta = new Vector2(20f, 20f);

                // 2. Icon hình ảnh vật phẩm
                GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObj.transform.SetParent(slot.transform, false);
                Image iconImg = iconObj.GetComponent<Image>();
                iconImg.raycastTarget = false;

                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;
                iconRect.sizeDelta = new Vector2(45f, 45f);

                // 3. Số lượng góc dưới bên phải
                GameObject countObj = new GameObject("CountText", typeof(RectTransform), typeof(TextMeshProUGUI));
                countObj.transform.SetParent(slot.transform, false);
                TextMeshProUGUI countTxt = countObj.GetComponent<TextMeshProUGUI>();
                countTxt.alignment = TextAlignmentOptions.BottomRight;
                countTxt.fontSize = 11;
                countTxt.fontStyle = FontStyles.Bold;
                countTxt.color = Color.white;

                RectTransform countRect = countObj.GetComponent<RectTransform>();
                countRect.anchorMin = new Vector2(1f, 0f);
                countRect.anchorMax = new Vector2(1f, 0f);
                countRect.pivot = new Vector2(1f, 0f);
                countRect.anchoredPosition = new Vector2(-4f, 2f);
                countRect.sizeDelta = new Vector2(30f, 18f);

                slotUIList.Add(slot);
            }
        }

        public void RefreshHotbarUI()
        {
            if (slotUIList.Count < 5) CreateHotbarUI();

            for (int i = 0; i < 5; i++)
            {
                GameObject slotObj = slotUIList[i];
                ItemData item = assignedItems[i];

                Image iconImg = slotObj.transform.Find("Icon")?.GetComponent<Image>();
                TextMeshProUGUI countTxt = slotObj.transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();

                if (item != null)
                {
                    int qty = 0;
                    if (StorageManager.Instance != null)
                    {
                        var slotInStorage = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == item.ItemID);
                        if (slotInStorage != null) qty = slotInStorage.quantity;
                    }

                    if (iconImg != null)
                    {
                        iconImg.sprite = item.Icon;
                        iconImg.enabled = true;
                        iconImg.color = (qty > 0) ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.6f);
                    }

                    if (countTxt != null)
                    {
                        countTxt.text = (qty > 0) ? $"x{qty}" : "0";
                        countTxt.color = (qty > 0) ? new Color(0.95f, 0.85f, 0.35f, 1f) : Color.gray;
                    }
                }
                else
                {
                    if (iconImg != null) iconImg.enabled = false;
                    if (countTxt != null) countTxt.text = "";
                }
            }
        }

        public void AssignItemToSlot(int slotIndex, ItemData item)
        {
            if (slotIndex < 0 || slotIndex >= 5) return;
            assignedItems[slotIndex] = item;
            RefreshHotbarUI();

            if (SurvivalUIManager.Instance != null && item != null)
            {
                SurvivalUIManager.Instance.ShowHUDToast($"Đã gán {item.ItemName} vào phím phím [{slotIndex + 1}]!");
            }
        }

        public void OnHotbarSlotClicked(int slotIndex)
        {
            UseHotbarSlot(slotIndex);
        }

        public void UseHotbarSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 5) return;
            ItemData item = assignedItems[slotIndex];

            if (item == null)
            {
                if (SurvivalUIManager.Instance != null)
                {
                    SurvivalUIManager.Instance.ShowHUDToast($"Phím [{slotIndex + 1}] chưa gán vật phẩm!");
                }
                return;
            }

            // Gọi logic sử dụng vật phẩm tương ứng
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.UseItemFromHotbar(item);
            }
        }

        public void ToggleHotbarVisible(bool visible)
        {
            if (hotbarPanel != null)
            {
                hotbarPanel.SetActive(visible);
            }
        }

        private GameObject tutorialPanelObj;

        /// <summary>
        /// Hiển thị bảng Popup Hướng dẫn kéo thả vật phẩm Balo vào thanh phím tắt.
        /// </summary>
        public void ShowHotbarTutorial()
        {
            if (tutorialPanelObj != null)
            {
                tutorialPanelObj.SetActive(true);
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            tutorialPanelObj = new GameObject("HotbarTutorialPanel", typeof(RectTransform), typeof(Image), typeof(Outline));
            tutorialPanelObj.transform.SetParent(canvas.transform, false);
            tutorialPanelObj.transform.SetAsLastSibling();

            RectTransform panelRect = tutorialPanelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(500f, 320f);

            Image bg = tutorialPanelObj.GetComponent<Image>();
            bg.color = new Color(0.14f, 0.11f, 0.08f, 0.98f);

            Outline outline = tutorialPanelObj.GetComponent<Outline>();
            outline.effectColor = new Color(0.8f, 0.65f, 0.35f, 1f);
            outline.effectDistance = new Vector2(2.5f, 2.5f);

            // 1. Tiêu đề
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(tutorialPanelObj.transform, false);
            TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
            titleTxt.text = "📖 HƯỚNG DẪN PHÍM TẮT & SẮP XẾP BALO";
            titleTxt.fontSize = 18;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = new Color(0.95f, 0.85f, 0.4f, 1f);

            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -15f);
            titleRect.sizeDelta = new Vector2(0f, 40f);

            // 2. Nội dung các bước hướng dẫn
            GameObject bodyObj = new GameObject("BodyText", typeof(RectTransform), typeof(TextMeshProUGUI));
            bodyObj.transform.SetParent(tutorialPanelObj.transform, false);
            TextMeshProUGUI bodyTxt = bodyObj.GetComponent<TextMeshProUGUI>();
            bodyTxt.text = "<b>BƯỚC 1:</b> Mở Balo gia đình bằng phím <b>[TAB]</b> hoặc <b>[I]</b>.\n\n" +
                           "<b>BƯỚC 2:</b> Giữ chuột trái vào vật phẩm và <b>KÉO THẢ</b> vào 1 trong 5 ô nhanh bên dưới.\n\n" +
                           "<b>BƯỚC 3:</b> Bấm các phím <b>[1] [2] [3] [4] [5]</b> để sử dụng vật phẩm tức thì!";
            bodyTxt.fontSize = 14;
            bodyTxt.lineSpacing = 8;
            bodyTxt.alignment = TextAlignmentOptions.Left;
            bodyTxt.color = new Color(0.9f, 0.9f, 0.9f, 1f);

            RectTransform bodyRect = bodyObj.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 1f);
            bodyRect.offsetMin = new Vector2(25f, 70f);
            bodyRect.offsetMax = new Vector2(-25f, -60f);

            // 3. Nút Đóng / Đã hiểu
            GameObject btnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnObj.transform.SetParent(tutorialPanelObj.transform, false);

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
                tutorialPanelObj.SetActive(false);
                PlayerPrefs.SetInt("HotbarTutorialShown", 1);
            });

            GameObject btnTextObj = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
            btnTextObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI btnTxt = btnTextObj.GetComponent<TextMeshProUGUI>();
            btnTxt.text = "ĐÃ HIỂU (BẮT ĐẦU THỬ)";
            btnTxt.fontSize = 13;
            btnTxt.fontStyle = FontStyles.Bold;
            btnTxt.alignment = TextAlignmentOptions.Center;
            btnTxt.color = Color.white;

            RectTransform btnTxtRect = btnTextObj.GetComponent<RectTransform>();
            btnTxtRect.anchorMin = Vector2.zero;
            btnTxtRect.anchorMax = Vector2.one;
            btnTxtRect.sizeDelta = Vector2.zero;
        }
    }
}
