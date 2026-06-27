using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SownInStone.Core;
using SownInStone.UI;
using SownInStone.Agriculture;
using SownInStone.Storage;

namespace SownInStone
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        public enum TutorialStage
        {
            NotStarted,
            IntroQuests,          // Stage 1: Visit O Thắm & Bác Năm
            ShowingFarmingSlides, // Between Stage 1 & 2: Show farming slides
            FarmingTutorial,      // Stage 2: Clean rocks, Plant seed, Water soil
            Completed             // Finished
        }

        [Header("--- TUTORIAL STATE ---")]
        public bool isTutorialActive = false;
        public TutorialStage currentStage = TutorialStage.NotStarted;

        public bool taskACompleted = false; // Talk to O Thắm
        public bool taskBCompleted = false; // Talk to Bác Năm

        public bool subTask1Completed = false; // Clear rocks
        public bool subTask2Completed = false; // Plant seed
        public bool subTask3Completed = false; // Water soil

        private bool talkDialogueActive = false;
        private string currentTalkSpeaker = "";

        // UI Panel elements
        private GameObject hudPanel;
        private TextMeshProUGUI hudTitleText;
        private TextMeshProUGUI hudTaskAText;
        private TextMeshProUGUI hudTaskBText;
        private TextMeshProUGUI hudTaskCText;

        // original intro slideshow fields
        private bool isShowing = false;
        private int currentSlide = 0;
        private Action onComplete;
        private Texture2D[] introImages;
        private Texture2D[] slideImages;
        private string[] slideTexts = new string[]
        {
            "Cách di chuyển: W, A, S, D. Nhấn [E] để tương tác.",
            "Cải tạo đất: Nhặt đá, tưới nước, ủ phân.",
            "Gieo hạt và tưới nước để trồng cây.",
            "Tương tác với NPC: O Thắm hoặc Bác Năm.",
            "Sinh tồn khi lũ ngập: Tránh nước và tìm nơi cao."
        };
        private readonly string[] imageNames = new string[]
        {
            "tutorial_intro",
            "tutorial_clear_rocks",
            "tutorial_plant_water",
            "tutorial_npc_interact",
            "tutorial_flood_survival"
        };
        public bool IsTutorialShown { get; private set; } = false;

        // Stage 2 farming slideshow fields
        private bool isShowingFarmingSlides = false;
        private int currentFarmingSlide = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadImages();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadImages()
        {
            // Load 5 intro movement images
            introImages = new Texture2D[5];
            for (int i = 0; i < 5; i++)
            {
                introImages[i] = Resources.Load<Texture2D>($"Textures/Tutorial/tutorial_intro/{i}");
            }

            // Load other slides from Resources
            slideImages = new Texture2D[imageNames.Length];
            for (int i = 0; i < imageNames.Length; i++)
            {
                if (i == 0)
                {
                    // Fallback to first frame of intro
                    slideImages[i] = introImages[0];
                }
                else
                {
                    slideImages[i] = Resources.Load<Texture2D>($"Textures/Tutorial/{imageNames[i]}");
                }
            }
        }

        public void ShowTutorial(Action onCompleteCallback)
        {
            if (isShowing) return;
            onComplete = onCompleteCallback;
            isShowing = true;
            currentSlide = 0;
            // Pause game time while tutorial is displayed
            Time.timeScale = 0f;
        }

        private void EndTutorial()
        {
            isShowing = false;
            IsTutorialShown = true;
            // Resume game time
            Time.timeScale = 1f;
            onComplete?.Invoke();
        }

        // --- NEW TUTORIAL QUEST SYSTEM METHODS ---

        public void InitializeTutorial()
        {
            isTutorialActive = true;
            currentStage = TutorialStage.IntroQuests;
            taskACompleted = false;
            taskBCompleted = false;
            subTask1Completed = false;
            subTask2Completed = false;
            subTask3Completed = false;
            talkDialogueActive = false;
            currentTalkSpeaker = "";
            isShowingFarmingSlides = false;
            currentFarmingSlide = 0;

            CreateHUDPanel();
        }

        public void RegisterTalkStart(string speakerName)
        {
            if (isTutorialActive)
            {
                talkDialogueActive = true;
                currentTalkSpeaker = speakerName;
            }
        }

        public void OnDialogueClosed(string speakerName)
        {
            if (!isTutorialActive) return;
            if (talkDialogueActive && currentTalkSpeaker == speakerName)
            {
                talkDialogueActive = false;
                if (currentStage == TutorialStage.IntroQuests)
                {
                    if (speakerName.Contains("O Thắm"))
                    {
                        taskACompleted = true;
                    }
                    else if (speakerName.Contains("Bác Năm"))
                    {
                        taskBCompleted = true;
                    }
                    UpdateHUDPanel();
                    CheckIntroQuestsProgress();
                }
            }
        }

        private void CheckIntroQuestsProgress()
        {
            if (taskACompleted && taskBCompleted)
            {
                // Trigger Stage 2 Farming Tutorial Slideshow
                currentStage = TutorialStage.ShowingFarmingSlides;
                UpdateHUDPanel();
                isShowingFarmingSlides = true;
                currentFarmingSlide = 0;
                Time.timeScale = 0f; // Pause game while slideshow is active
            }
        }

        public void OnRockCleared()
        {
            if (!isTutorialActive || currentStage != TutorialStage.FarmingTutorial) return;
            subTask1Completed = true;
            UpdateHUDPanel();
        }

        public void OnCropPlanted()
        {
            if (!isTutorialActive || currentStage != TutorialStage.FarmingTutorial) return;
            subTask2Completed = true;
            UpdateHUDPanel();
        }

        public void OnSoilWatered()
        {
            if (!isTutorialActive || currentStage != TutorialStage.FarmingTutorial) return;
            subTask3Completed = true;
            UpdateHUDPanel();
        }

        private void EndFarmingSlides()
        {
            isShowingFarmingSlides = false;
            Time.timeScale = 1f;
            currentStage = TutorialStage.FarmingTutorial;
            UpdateHUDPanel();
        }

        private void CompleteTutorial()
        {
            currentStage = TutorialStage.Completed;
            isTutorialActive = false;

            if (hudPanel != null)
            {
                Destroy(hudPanel);
                hudPanel = null;
            }

            SurvivalUIManager.Instance?.ShowHUDToast("Hoàn thành hướng dẫn! Bắt đầu chế độ tự do.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionToPhase(GamePhase.LapNghiep);
            }
        }

        private void CreateHUDPanel()
        {
            if (hudPanel != null) return;

            SurvivalUIManager uiManager = SurvivalUIManager.Instance;
            if (uiManager == null) return;

            TMP_FontAsset font = null;
            if (uiManager.SpeakerNameText != null)
            {
                font = uiManager.SpeakerNameText.font;
            }

            hudPanel = new GameObject("TutorialQuestPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            hudPanel.transform.SetParent(uiManager.transform, false);

            RectTransform rect = hudPanel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f); // Anchored top-left
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -220f);
            rect.sizeDelta = new Vector2(280f, 120f);

            Image img = hudPanel.GetComponent<Image>();
            img.color = new Color(0.08f, 0.06f, 0.05f, 0.9f);

            Outline outline = hudPanel.AddComponent<Outline>();
            outline.effectColor = new Color(0.38f, 0.30f, 0.22f, 1f);
            outline.effectDistance = new Vector2(1.5f, 1.5f);

            // Title Text
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(hudPanel.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.anchoredPosition = new Vector2(15f, -12f);
            titleRect.sizeDelta = new Vector2(250f, 20f);

            hudTitleText = titleObj.GetComponent<TextMeshProUGUI>();
            hudTitleText.fontSize = 14;
            hudTitleText.fontStyle = FontStyles.Bold;
            hudTitleText.color = new Color(0.95f, 0.85f, 0.4f, 1f); // Gold
            if (font != null) hudTitleText.font = font;
            hudTitleText.text = "NHIỆM VỤ HƯỚNG DẪN";

            // Task A Text
            GameObject taskAObj = new GameObject("TaskAText", typeof(RectTransform), typeof(TextMeshProUGUI));
            taskAObj.transform.SetParent(hudPanel.transform, false);
            RectTransform taskARect = taskAObj.GetComponent<RectTransform>();
            taskARect.anchorMin = new Vector2(0f, 1f);
            taskARect.anchorMax = new Vector2(1f, 1f);
            taskARect.pivot = new Vector2(0f, 1f);
            taskARect.anchoredPosition = new Vector2(15f, -42f);
            taskARect.sizeDelta = new Vector2(250f, 22f);

            hudTaskAText = taskAObj.GetComponent<TextMeshProUGUI>();
            hudTaskAText.fontSize = 13;
            hudTaskAText.color = Color.white;
            if (font != null) hudTaskAText.font = font;

            // Task B Text
            GameObject taskBObj = new GameObject("TaskBText", typeof(RectTransform), typeof(TextMeshProUGUI));
            taskBObj.transform.SetParent(hudPanel.transform, false);
            RectTransform taskBRect = taskBObj.GetComponent<RectTransform>();
            taskBRect.anchorMin = new Vector2(0f, 1f);
            taskBRect.anchorMax = new Vector2(1f, 1f);
            taskBRect.pivot = new Vector2(0f, 1f);
            taskBRect.anchoredPosition = new Vector2(15f, -67f);
            taskBRect.sizeDelta = new Vector2(250f, 22f);

            hudTaskBText = taskBObj.GetComponent<TextMeshProUGUI>();
            hudTaskBText.fontSize = 13;
            hudTaskBText.color = Color.white;
            if (font != null) hudTaskBText.font = font;

            // Task C Text
            GameObject taskCObj = new GameObject("TaskCText", typeof(RectTransform), typeof(TextMeshProUGUI));
            taskCObj.transform.SetParent(hudPanel.transform, false);
            RectTransform taskCRect = taskCObj.GetComponent<RectTransform>();
            taskCRect.anchorMin = new Vector2(0f, 1f);
            taskCRect.anchorMax = new Vector2(1f, 1f);
            taskCRect.pivot = new Vector2(0f, 1f);
            taskCRect.anchoredPosition = new Vector2(15f, -92f);
            taskCRect.sizeDelta = new Vector2(250f, 22f);

            hudTaskCText = taskCObj.GetComponent<TextMeshProUGUI>();
            hudTaskCText.fontSize = 13;
            hudTaskCText.color = Color.white;
            if (font != null) hudTaskCText.font = font;

            UpdateHUDPanel();
        }

        public void UpdateHUDPanel()
        {
            if (hudPanel == null) return;

            if (currentStage == TutorialStage.IntroQuests)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "NHIỆM VỤ: GẶP GỠ DÂN LÀNG";
                hudTaskAText.text = (taskACompleted ? "✓ " : "☐ ") + "Qua thăm O Thắm";
                hudTaskAText.color = taskACompleted ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskBText.text = (taskBCompleted ? "✓ " : "☐ ") + "Qua thăm Bác Năm";
                hudTaskBText.color = taskBCompleted ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskBText.gameObject.SetActive(true);
                hudTaskCText.gameObject.SetActive(false);
            }
            else if (currentStage == TutorialStage.FarmingTutorial)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "NHIỆM VỤ: HỌC HỎI TRỒNG TRỌT";
                hudTaskAText.text = (subTask1Completed ? "✓ " : "☐ ") + "Dọn dẹp đá trên ruộng";
                hudTaskAText.color = subTask1Completed ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskBText.text = (subTask2Completed ? "✓ " : "☐ ") + "Gieo hạt giống khoai";
                hudTaskBText.color = subTask2Completed ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskBText.gameObject.SetActive(true);
                hudTaskCText.text = (subTask3Completed ? "✓ " : "☐ ") + "Tưới nước ẩm đất";
                hudTaskCText.color = subTask3Completed ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskCText.gameObject.SetActive(true);
            }
            else
            {
                hudPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isTutorialActive) return;

            if (hudPanel == null && SurvivalUIManager.Instance != null)
            {
                CreateHUDPanel();
            }

            if (currentStage == TutorialStage.FarmingTutorial)
            {
                // Fallback check for planting sub-task completion if out of seeds
                if (!subTask2Completed)
                {
                    int seedCount = 0;
                    if (StorageManager.Instance != null)
                    {
                        var slots = StorageManager.Instance.GetStorageSlots();
                        var seedSlot = slots.Find(s => s.item != null && (s.item.ItemName.Contains("Hạt giống") || s.item.ItemID == "Item_Seed" || s.item.name.Contains("Seed")));
                        if (seedSlot != null)
                        {
                            seedCount = seedSlot.quantity;
                        }
                    }

                    int cropCount = 0;
                    CropInstance[] allCrops = FindObjectsByType<CropInstance>(FindObjectsInactive.Exclude);
                    foreach (var c in allCrops)
                    {
                        if (c != null && c.cropData != null)
                        {
                            cropCount++;
                        }
                    }

                    if (seedCount == 0 && cropCount >= 1)
                    {
                        subTask2Completed = true;
                        UpdateHUDPanel();
                    }
                }

                // Scan the scene for any crop instances that have successfully entered growth stage 1
                CropInstance[] crops = FindObjectsByType<CropInstance>(FindObjectsInactive.Exclude);
                foreach (var crop in crops)
                {
                    if (crop != null && crop.cropData != null)
                    {
                        float ratio = crop.cropData.DaysToMature > 0 ? (crop.currentGrowthDays / crop.cropData.DaysToMature) : 0f;
                        if (crop.currentGrowthDays > 0.01f || ratio >= 0.33f)
                        {
                            CompleteTutorial();
                            break;
                        }
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (isShowing)
            {
                int tempSlide = currentSlide;
                RenderSlideshow(ref tempSlide, introImages, slideImages, slideTexts, EndTutorial);
                currentSlide = tempSlide;
            }
            else if (isShowingFarmingSlides)
            {
                Texture2D[] farmingImages = new Texture2D[] { slideImages[1], slideImages[2] };
                string[] farmingTexts = new string[]
                {
                    "Dọn dẹp đá trên ruộng: Lại gần các khối đá nhô cao trên đất trồng và nhấn [E] để dọn sạch chúng.",
                    "Gieo hạt và tưới nước: Đến gặp O Thắm mua giống khoai gieo trồng, sau đó dùng gáo tưới ẩm đất ruộng."
                };
                int tempFarmingSlide = currentFarmingSlide;
                RenderSlideshow(ref tempFarmingSlide, null, farmingImages, farmingTexts, EndFarmingSlides);
                currentFarmingSlide = tempFarmingSlide;
            }
        }

        private void RenderSlideshow(ref int slideIndex, Texture2D[] animationFrames, Texture2D[] staticImages, string[] texts, Action onFinished)
        {
            // Dark overlay background
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Panel for tutorial content
            float panelWidth = Screen.width * 0.8f;
            float panelHeight = Screen.height * 0.8f;
            float panelX = (Screen.width - panelWidth) / 2f;
            float panelY = (Screen.height - panelHeight) / 2f;
            Rect panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);

            // Draw inner background
            GUI.color = new Color(0.12f, 0.1f, 0.08f, 0.95f);
            GUI.DrawTexture(panelRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Image area
            float imageHeight = panelHeight * 0.55f;
            Rect imageRect = new Rect(panelX + 20, panelY + 20, panelWidth - 40, imageHeight);
            Texture2D img = null;
            if (slideIndex == 0 && animationFrames != null && animationFrames.Length > 0)
            {
                int frame = Mathf.FloorToInt(Time.unscaledTime * 3f) % animationFrames.Length;
                img = animationFrames[frame] ?? staticImages[slideIndex];
            }
            else
            {
                img = staticImages[slideIndex];
            }

            if (img != null)
            {
                GUI.DrawTexture(imageRect, img, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                GUI.DrawTexture(imageRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(imageRect, "[Placeholder Image]", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 24, normal = { textColor = Color.white } });
            }

            // Text description below image
            Rect textRect = new Rect(panelX + 20, panelY + 20 + imageHeight + 10, panelWidth - 40, 80);
            GUIStyle textStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                normal = { textColor = new Color(0.9f, 0.85f, 0.7f) },
                wordWrap = true
            };
            GUI.Label(textRect, texts[slideIndex], textStyle);

            // Navigation buttons
            float btnWidth = 120f;
            float btnHeight = 40f;
            float btnY = panelY + panelHeight - btnHeight - 20;
            float spacing = 20f;

            // Back button
            if (GUI.Button(new Rect(panelX + spacing, btnY, btnWidth, btnHeight), "< Quay Lại"))
            {
                if (slideIndex > 0) slideIndex--;
            }
            // Next button
            if (GUI.Button(new Rect(panelX + panelWidth - btnWidth - spacing, btnY, btnWidth, btnHeight), "Tiếp Theo >"))
            {
                if (slideIndex < texts.Length - 1)
                {
                    slideIndex++;
                }
                else
                {
                    onFinished?.Invoke();
                }
            }
            // Skip button (centered)
            if (GUI.Button(new Rect(panelX + (panelWidth - btnWidth) / 2f, btnY, btnWidth, btnHeight), "Bỏ Qua"))
            {
                onFinished?.Invoke();
            }
        }
    }
}
