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
            IntroQuests,          // Stage 1: Visit 4 villagers
            TalkToOThamJob,       // Stage 1.5: Talk to O Thắm to get farming job and seeds
            ShowingFarmingSlides, // Show farming slides
            FarmingTutorial,      // Stage 2: Clean rocks, Plant seed, Water soil, Harvest
            SellCrops,            // Stage 3: Sell crops to O Thắm
            TalkToBacNamPreserve, // Stage 4: Talk to Bác Năm about food preservation
            CraftPreservedCrops,  // Stage 5: Craft 4 preserved crops
            SharePreservedCrops,  // Stage 6: Share 4 preserved crops with 4 NPCs
            PrepareForStorm,      // Stage 7: Help O Thắm and Bác Năm prepare for storm
            PrepareOwnHouse,      // Stage 8: Prepare own house
            TalkToCuBayWorship,   // Stage 9: Talk to Cụ Bảy about ancestral worship
            WorshipAltar,         // Stage 10: Burn incense at ancestral altar
            Completed             // Finished
        }

        [Header("--- TUTORIAL STATE ---")]
        public bool isTutorialActive = false;
        public TutorialStage currentStage = TutorialStage.NotStarted;

        public bool taskACompleted = false; // Talk to O Thắm
        public bool taskBCompleted = false; // Talk to Bác Năm
        public bool taskCCompleted = false; // Talk to Cụ Bảy
        public bool taskDCompleted = false; // Talk to Bé Tí

        public bool subTask1Completed = false; // Clear rocks
        public bool subTask2Completed = false; // Plant seed
        public bool subTask3Completed = false; // Water soil
        public bool subTask4Completed = false; // Harvest crop
        public int freshCropsSold = 0; // Số lượng khoai lang tươi đã bán cho O Thắm

        // Các mốc nhiệm vụ bổ sung
        public int preservedCropsCrafted = 0;
        public bool sharedOTham = false;
        public bool sharedBacNam = false;
        public bool sharedCuBay = false;
        public bool sharedBeTi = false;

        public bool oThamPrepped = false;
        public bool bacNamPrepped = false;

        private bool talkDialogueActive = false;
        private string currentTalkSpeaker = "";

        // UI Panel elements
        private GameObject hudPanel;
        private TextMeshProUGUI hudTitleText;
        private TextMeshProUGUI hudTaskAText;
        private TextMeshProUGUI hudTaskBText;
        private TextMeshProUGUI hudTaskCText;
        private TextMeshProUGUI hudTaskDText;
        
        private SownInStone.Community.NPCCharacter[] npcsInScene;

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
        public bool IsShowing => isShowing;
        public bool IsShowingFarmingSlides => isShowingFarmingSlides;

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

        private void Start()
        {
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
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
            taskCCompleted = false;
            taskDCompleted = false;
            subTask1Completed = false;
            subTask2Completed = false;
            subTask3Completed = false;
            subTask4Completed = false;
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
                    else if (speakerName.Contains("Cụ Bảy"))
                    {
                        taskCCompleted = true;
                    }
                    else if (speakerName.Contains("Bé Tí"))
                    {
                        taskDCompleted = true;
                    }
                    UpdateHUDPanel();
                    CheckIntroQuestsProgress();
                }
            }
        }

        private void CheckIntroQuestsProgress()
        {
            if (taskACompleted && taskBCompleted && taskCCompleted && taskDCompleted)
            {
                currentStage = TutorialStage.TalkToOThamJob;
                UpdateHUDPanel();
                
                // Refresh danh sách NPC để vẽ lại chấm than cho O Thắm
                npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
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

        public void StartFarmingSlideshow()
        {
            currentStage = TutorialStage.ShowingFarmingSlides;
            UpdateHUDPanel();
            isShowingFarmingSlides = true;
            currentFarmingSlide = 0;
            Time.timeScale = 0f;
        }

        private void EndFarmingSlides()
        {
            isShowingFarmingSlides = false;
            Time.timeScale = 1f;
            currentStage = TutorialStage.FarmingTutorial;
            
            // Cấp 12 hạt giống khi bắt đầu trồng trọt
            if (StorageManager.Instance != null && PlayerController.Instance != null && PlayerController.Instance.seedItem != null)
            {
                StorageManager.Instance.AddItem(PlayerController.Instance.seedItem, 12);
                SurvivalUIManager.Instance?.ShowHUDToast("<color=#2ECC71>Bạn nhận được 12 hạt giống khoai lang từ O Thắm</color>");
            }
            
            UpdateHUDPanel();
        }

        public void CompleteTutorial()
        {
            currentStage = TutorialStage.Completed;
            isTutorialActive = false;

            if (hudPanel != null)
            {
                Destroy(hudPanel);
                hudPanel = null;
            }

            SurvivalUIManager.Instance?.ShowHUDToast("Hoàn thành hướng dẫn! Bão lũ đang đổ bộ vào làng.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionToPhase(GamePhase.MuaBao);
            }
        }

        public void OnCropsSold()
        {
            if (!isTutorialActive || currentStage != TutorialStage.SellCrops) return;
            currentStage = TutorialStage.TalkToBacNamPreserve;
            UpdateHUDPanel();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        public void OnBacNamPreserveTalked()
        {
            if (!isTutorialActive || currentStage != TutorialStage.TalkToBacNamPreserve) return;
            currentStage = TutorialStage.CraftPreservedCrops;
            UpdateHUDPanel();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        public void OnPreservedCropCrafted()
        {
            if (!isTutorialActive || currentStage != TutorialStage.CraftPreservedCrops) return;
            preservedCropsCrafted++;
            if (preservedCropsCrafted >= 4)
            {
                currentStage = TutorialStage.SharePreservedCrops;
            }
            UpdateHUDPanel();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        public void OnPreservedCropShared(SownInStone.Community.NPCCharacter.StoryCharacterType characterType)
        {
            if (!isTutorialActive || currentStage != TutorialStage.SharePreservedCrops) return;

            if (characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.OTham) sharedOTham = true;
            else if (characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.BacNam) sharedBacNam = true;
            else if (characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.CuBay) sharedCuBay = true;
            else if (characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.BeTi) sharedBeTi = true;

            int sharedCount = 0;
            if (sharedOTham) sharedCount++;
            if (sharedBacNam) sharedCount++;
            if (sharedCuBay) sharedCount++;
            if (sharedBeTi) sharedCount++;

            if (sharedCount >= 4)
            {
                currentStage = TutorialStage.PrepareForStorm;
                TriggerLoudspeakerAnnouncement();
            }
            UpdateHUDPanel();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        private void TriggerLoudspeakerAnnouncement()
        {
            if (SurvivalUIManager.Instance != null)
            {
                SurvivalUIManager.Instance.ShowDialogue(
                    "[LOA PHÁT THANH XÃ]", 
                    "\"Alo! Alo! Báo động khẩn cấp! Cơn bão số 6 siêu mạnh đang hướng thẳng vào vùng biển miền Trung và sẽ đổ bộ vào đêm nay! Yêu cầu toàn thể bà con khẩn trương chằng chống nhà cửa, đắp bao cát ngăn lũ, sẵn sàng ứng phó!\""
                );
                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_warning");
            }
        }

        public void OnOThamPrepped()
        {
            if (!isTutorialActive || currentStage != TutorialStage.PrepareForStorm) return;
            oThamPrepped = true;
            CheckPrepStormComplete();
        }

        public void OnBacNamPrepped()
        {
            if (!isTutorialActive || currentStage != TutorialStage.PrepareForStorm) return;
            bacNamPrepped = true;
            CheckPrepStormComplete();
        }

        private void CheckPrepStormComplete()
        {
            if (oThamPrepped && bacNamPrepped)
            {
                currentStage = TutorialStage.PrepareOwnHouse;
            }
            UpdateHUDPanel();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        public void OnOwnHousePrepared()
        {
            if (!isTutorialActive || currentStage != TutorialStage.PrepareOwnHouse) return;
            
            if (SurvivalUIManager.Instance != null)
            {
                SurvivalUIManager.Instance.ShowDialogue(
                    "Nhà của bạn", 
                    "\"Bạn đã chất bao cát chắn các khe cửa và buộc dây chằng cố định lại mái tranh của nhà mình. Nhà cửa đã tạm thời an toàn trước giông bão.\""
                );
            }
            
            currentStage = TutorialStage.TalkToCuBayWorship;
            UpdateHUDPanel();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        public void OnCuBayWorshipTalked()
        {
            if (!isTutorialActive || currentStage != TutorialStage.TalkToCuBayWorship) return;
            currentStage = TutorialStage.WorshipAltar;
            UpdateHUDPanel();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        public void OnAltarWorshipped()
        {
            if (!isTutorialActive || currentStage != TutorialStage.WorshipAltar) return;
            CompleteTutorial();
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
            rect.anchoredPosition = new Vector2(15f, -220f);
            rect.sizeDelta = new Vector2(200f, 105f); // Tăng nhẹ chiều rộng lên 200f để chứa gọn tiêu đề nhiệm vụ không bị tràn

            Image img = hudPanel.GetComponent<Image>();
            img.color = new Color(0.08f, 0.06f, 0.05f, 0.92f); // Tối màu hơn một chút để nổi bật

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
            titleRect.anchoredPosition = new Vector2(10f, -8f);
            titleRect.sizeDelta = new Vector2(180f, 16f);

            hudTitleText = titleObj.GetComponent<TextMeshProUGUI>();
            hudTitleText.fontSize = 11; // Giảm cỡ chữ Title xuống 11
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
            taskARect.anchoredPosition = new Vector2(10f, -26f);
            taskARect.sizeDelta = new Vector2(180f, 16f);

            hudTaskAText = taskAObj.GetComponent<TextMeshProUGUI>();
            hudTaskAText.fontSize = 10; // Giảm cỡ chữ xuống 10
            hudTaskAText.color = Color.white;
            if (font != null) hudTaskAText.font = font;

            // Task B Text
            GameObject taskBObj = new GameObject("TaskBText", typeof(RectTransform), typeof(TextMeshProUGUI));
            taskBObj.transform.SetParent(hudPanel.transform, false);
            RectTransform taskBRect = taskBObj.GetComponent<RectTransform>();
            taskBRect.anchorMin = new Vector2(0f, 1f);
            taskBRect.anchorMax = new Vector2(1f, 1f);
            taskBRect.pivot = new Vector2(0f, 1f);
            taskBRect.anchoredPosition = new Vector2(10f, -42f);
            taskBRect.sizeDelta = new Vector2(180f, 16f);

            hudTaskBText = taskBObj.GetComponent<TextMeshProUGUI>();
            hudTaskBText.fontSize = 10; // Giảm cỡ chữ xuống 10
            hudTaskBText.color = Color.white;
            if (font != null) hudTaskBText.font = font;

            // Task C Text
            GameObject taskCObj = new GameObject("TaskCText", typeof(RectTransform), typeof(TextMeshProUGUI));
            taskCObj.transform.SetParent(hudPanel.transform, false);
            RectTransform taskCRect = taskCObj.GetComponent<RectTransform>();
            taskCRect.anchorMin = new Vector2(0f, 1f);
            taskCRect.anchorMax = new Vector2(1f, 1f);
            taskCRect.pivot = new Vector2(0f, 1f);
            taskCRect.anchoredPosition = new Vector2(10f, -58f);
            taskCRect.sizeDelta = new Vector2(180f, 16f);

            hudTaskCText = taskCObj.GetComponent<TextMeshProUGUI>();
            hudTaskCText.fontSize = 10; // Giảm cỡ chữ xuống 10
            hudTaskCText.color = Color.white;
            if (font != null) hudTaskCText.font = font;

            // Task D Text
            GameObject taskDObj = new GameObject("TaskDText", typeof(RectTransform), typeof(TextMeshProUGUI));
            taskDObj.transform.SetParent(hudPanel.transform, false);
            RectTransform taskDRect = taskDObj.GetComponent<RectTransform>();
            taskDRect.anchorMin = new Vector2(0f, 1f);
            taskDRect.anchorMax = new Vector2(1f, 1f);
            taskDRect.pivot = new Vector2(0f, 1f);
            taskDRect.anchoredPosition = new Vector2(10f, -74f);
            taskDRect.sizeDelta = new Vector2(180f, 16f);

            hudTaskDText = taskDObj.GetComponent<TextMeshProUGUI>();
            hudTaskDText.fontSize = 10; // Giảm cỡ chữ xuống 10
            hudTaskDText.color = Color.white;
            if (font != null) hudTaskDText.font = font;

            UpdateHUDPanel();
        }

        public void UpdateHUDPanel()
        {
            if (hudPanel == null) return;

            if (currentStage == TutorialStage.IntroQuests)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "GẶP GỠ DÂN LÀNG";
                
                hudTaskAText.text = (taskACompleted ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Qua thăm O Thắm";
                hudTaskAText.color = taskACompleted ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                
                hudTaskBText.text = (taskBCompleted ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Qua thăm Bác Năm";
                hudTaskBText.color = taskBCompleted ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskBText.gameObject.SetActive(true);
                
                hudTaskCText.text = (taskCCompleted ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Qua thăm Cụ Bảy";
                hudTaskCText.color = taskCCompleted ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskCText.gameObject.SetActive(true);
                
                hudTaskDText.text = (taskDCompleted ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Qua thăm Bé Tí";
                hudTaskDText.color = taskDCompleted ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskDText.gameObject.SetActive(true);
            }
            else if (currentStage == TutorialStage.TalkToOThamJob)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "O THẮM GIAO VIỆC";

                hudTaskAText.text = " <color=#E74C3C>☐</color> O Thắm có việc cần nhờ";
                hudTaskAText.color = Color.white;

                if (hudTaskBText != null) hudTaskBText.gameObject.SetActive(false);
                if (hudTaskCText != null) hudTaskCText.gameObject.SetActive(false);
                if (hudTaskDText != null) hudTaskDText.gameObject.SetActive(false);
            }
            else if (currentStage == TutorialStage.FarmingTutorial)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "HỌC HỎI TRỒNG TRỌT";
                
                hudTaskAText.text = (subTask1Completed ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Dọn dẹp đá trên ruộng";
                hudTaskAText.color = subTask1Completed ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                
                hudTaskBText.text = (subTask2Completed ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Gieo hạt giống khoai";
                hudTaskBText.color = subTask2Completed ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskBText.gameObject.SetActive(true);
                
                hudTaskCText.text = (subTask3Completed ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Tưới nước ẩm đất";
                hudTaskCText.color = subTask3Completed ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskCText.gameObject.SetActive(true);
                
                hudTaskDText.text = (subTask4Completed ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Thu hoạch khoai lang";
                hudTaskDText.color = subTask4Completed ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskDText.gameObject.SetActive(true);
            }
            else if (currentStage == TutorialStage.SellCrops)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "BÁN KHOAI KIẾM TIỀN";

                hudTaskAText.text = $" <color=#E74C3C>☐</color> Bán khoai tươi cho O Thắm ({freshCropsSold}/12)";
                hudTaskAText.color = Color.white;

                if (hudTaskBText != null) hudTaskBText.gameObject.SetActive(false);
                if (hudTaskCText != null) hudTaskCText.gameObject.SetActive(false);
                if (hudTaskDText != null) hudTaskDText.gameObject.SetActive(false);
            }
            else if (currentStage == TutorialStage.TalkToBacNamPreserve)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "BẢO QUẢN THỨC ĂN";

                hudTaskAText.text = " <color=#E74C3C>☐</color> Gặp Bác Năm hướng dẫn";
                hudTaskAText.color = Color.white;

                if (hudTaskBText != null) hudTaskBText.gameObject.SetActive(false);
                if (hudTaskCText != null) hudTaskCText.gameObject.SetActive(false);
                if (hudTaskDText != null) hudTaskDText.gameObject.SetActive(false);
            }
            else if (currentStage == TutorialStage.CraftPreservedCrops)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "CHẾ BIẾN KHOAI GIEO";

                hudTaskAText.text = $" <color=#E74C3C>☐</color> Chế biến khoai gieo ({preservedCropsCrafted}/4)";
                hudTaskAText.color = Color.white;

                if (hudTaskBText != null) hudTaskBText.gameObject.SetActive(false);
                if (hudTaskCText != null) hudTaskCText.gameObject.SetActive(false);
                if (hudTaskDText != null) hudTaskDText.gameObject.SetActive(false);
            }
            else if (currentStage == TutorialStage.SharePreservedCrops)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "CHIA SẺ NÔNG SẢN";

                int sharedCount = 0;
                if (sharedOTham) sharedCount++;
                if (sharedBacNam) sharedCount++;
                if (sharedCuBay) sharedCount++;
                if (sharedBeTi) sharedCount++;

                hudTaskAText.text = $" <color=#E74C3C>☐</color> Mời mọi người khoai gieo ({sharedCount}/4)";
                hudTaskAText.color = Color.white;

                if (hudTaskBText != null) hudTaskBText.gameObject.SetActive(false);
                if (hudTaskCText != null) hudTaskCText.gameObject.SetActive(false);
                if (hudTaskDText != null) hudTaskDText.gameObject.SetActive(false);
            }
            else if (currentStage == TutorialStage.PrepareForStorm)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "GIA CỐ TRƯỚC BÃO";

                hudTaskAText.text = (oThamPrepped ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Hỗ trợ O Thắm chắn lũ";
                hudTaskAText.color = oThamPrepped ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;

                hudTaskBText.text = (bacNamPrepped ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Hỗ trợ Bác Năm gia cố nhà";
                hudTaskBText.color = bacNamPrepped ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskBText.gameObject.SetActive(true);

                if (hudTaskCText != null) hudTaskCText.gameObject.SetActive(false);
                if (hudTaskDText != null) hudTaskDText.gameObject.SetActive(false);
            }
            else if (currentStage == TutorialStage.PrepareOwnHouse)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "GIA CỐ NHÀ MÌNH";

                hudTaskAText.text = " <color=#E74C3C>☐</color> Chuẩn bị bao cát bảo vệ nhà mình";
                hudTaskAText.color = Color.white;

                if (hudTaskBText != null) hudTaskBText.gameObject.SetActive(false);
                if (hudTaskCText != null) hudTaskCText.gameObject.SetActive(false);
                if (hudTaskDText != null) hudTaskDText.gameObject.SetActive(false);
            }
            else if (currentStage == TutorialStage.TalkToCuBayWorship)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "THỜ CÚNG QUÊ NHÀ";

                hudTaskAText.text = " <color=#E74C3C>☐</color> Gặp Cụ Bảy";
                hudTaskAText.color = Color.white;

                if (hudTaskBText != null) hudTaskBText.gameObject.SetActive(false);
                if (hudTaskCText != null) hudTaskCText.gameObject.SetActive(false);
                if (hudTaskDText != null) hudTaskDText.gameObject.SetActive(false);
            }
            else if (currentStage == TutorialStage.WorshipAltar)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "THẮP NHANG GIA TIÊN";

                hudTaskAText.text = " <color=#E74C3C>☐</color> Thắp nhang bàn thờ gia tiên";
                hudTaskAText.color = Color.white;

                if (hudTaskBText != null) hudTaskBText.gameObject.SetActive(false);
                if (hudTaskCText != null) hudTaskCText.gameObject.SetActive(false);
                if (hudTaskDText != null) hudTaskDText.gameObject.SetActive(false);
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
                // Tự động hoàn thành gieo hạt nếu hạt giống bằng 0 và đã có cây trong ruộng
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

                // Kiểm tra xem tất cả các subtask nông nghiệp đã xong chưa để chuyển qua bán khoai
                if (subTask1Completed && subTask2Completed && subTask3Completed && subTask4Completed)
                {
                    currentStage = TutorialStage.SellCrops;
                    UpdateHUDPanel();

                    // Refresh lại chỉ thị NPC để hiện dấu chấm hỏi/chấm than trên O Thắm để giao dịch bán khoai
                    npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
                }
            }
        }

        public void OnCropHarvested()
        {
            if (!isTutorialActive || currentStage != TutorialStage.FarmingTutorial) return;
            subTask4Completed = true;
            UpdateHUDPanel();
        }

        private void OnGUI()
        {
            // Vẽ các chỉ thị NPC lên trên cùng màn hình
            DrawNPCIndicators();

            // Vẽ các chỉ thị hướng dẫn Ruộng Đất lên màn hình
            DrawSoilIndicators();

            // Vẽ chỉ thị dẫn đến Bếp Gas
            DrawHearthIndicator();

            // Vẽ chỉ thị dẫn đến Nhà của bạn
            DrawHouseIndicator();

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

        private void DrawNPCIndicators()
        {
            if (!isTutorialActive) return;
            if (currentStage != TutorialStage.IntroQuests && 
                currentStage != TutorialStage.TalkToOThamJob && 
                currentStage != TutorialStage.SellCrops &&
                currentStage != TutorialStage.TalkToBacNamPreserve &&
                currentStage != TutorialStage.SharePreservedCrops &&
                currentStage != TutorialStage.TalkToCuBayWorship) return;
            if (Camera.main == null) return;

            bool needsRefresh = (npcsInScene == null || npcsInScene.Length == 0);
            if (!needsRefresh)
            {
                foreach (var npc in npcsInScene)
                {
                    if (npc == null)
                    {
                        needsRefresh = true;
                        break;
                    }
                }
            }

            if (needsRefresh)
            {
                npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
            }

            GUIStyle nameStyle = new GUIStyle();
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = Color.white;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.fontSize = 11;

            GUIStyle exclamationStyle = new GUIStyle();
            exclamationStyle.alignment = TextAnchor.MiddleCenter;
            exclamationStyle.normal.textColor = new Color(1f, 0.8f, 0f, 1f); // Vàng
            exclamationStyle.fontStyle = FontStyle.Bold;
            exclamationStyle.fontSize = 28;

            foreach (var npc in npcsInScene)
            {
                if (npc == null) continue;

                // Xác định xem NPC này đã hoàn thành trò chuyện chưa
                bool isCompleted = false;
                if (currentStage == TutorialStage.IntroQuests)
                {
                    if (npc.characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.OTham) isCompleted = taskACompleted;
                    else if (npc.characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.BacNam) isCompleted = taskBCompleted;
                    else if (npc.NPCName.Contains("Cụ Bảy")) isCompleted = taskCCompleted;
                    else if (npc.NPCName.Contains("Bé Tí")) isCompleted = taskDCompleted;
                    else continue;
                }
                else if (currentStage == TutorialStage.TalkToOThamJob || currentStage == TutorialStage.SellCrops)
                {
                    if (npc.characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.OTham)
                    {
                        isCompleted = false; // Luôn hiện chỉ thị trên đầu O Thắm để giao việc / mua bán
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (currentStage == TutorialStage.TalkToBacNamPreserve)
                {
                    if (npc.characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.BacNam)
                    {
                        isCompleted = false;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (currentStage == TutorialStage.SharePreservedCrops)
                {
                    if (npc.characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.OTham) isCompleted = sharedOTham;
                    else if (npc.characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.BacNam) isCompleted = sharedBacNam;
                    else if (npc.characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.CuBay) isCompleted = sharedCuBay;
                    else if (npc.characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.BeTi) isCompleted = sharedBeTi;
                    else continue;
                }
                else if (currentStage == TutorialStage.TalkToCuBayWorship)
                {
                    if (npc.characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.CuBay)
                    {
                        isCompleted = false;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                if (isCompleted) continue;

                // Chiều cao lơ lửng trên đầu NPC (khoảng 1.8m từ Visual)
                Transform visualTrans = npc.transform.Find("Visual");
                Vector3 basePos = visualTrans != null ? visualTrans.position : npc.transform.position;
                Vector3 worldPos = basePos + Vector3.up * 1.8f;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                if (screenPos.z > 0)
                {
                    float guiY = Screen.height - screenPos.y;

                    // Hiệu ứng nháy nhấp nháy cho dấu chấm than thêm phần sinh động
                    float pulse = Mathf.PingPong(Time.time * 2f, 0.3f);
                    exclamationStyle.normal.textColor = new Color(1f, 0.8f + pulse * 0.5f, 0f, 0.8f + pulse);

                    // Vẽ bóng đổ chữ màu đen trước
                    GUIStyle exShadow = new GUIStyle(exclamationStyle);
                    exShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                    GUI.Label(new Rect(screenPos.x - 20 + 1, guiY - 25 + 1, 40, 25), "!", exShadow);
                    // Vẽ dấu chấm than màu vàng
                    GUI.Label(new Rect(screenPos.x - 20, guiY - 25, 40, 25), "!", exclamationStyle);

                    // Vẽ bóng đổ cho tên NPC
                    GUIStyle nameShadow = new GUIStyle(nameStyle);
                    nameShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                    GUI.Label(new Rect(screenPos.x - 100 + 1, guiY + 1, 200, 20), npc.NPCName, nameShadow);
                    // Vẽ tên NPC
                    GUI.Label(new Rect(screenPos.x - 100, guiY, 200, 20), npc.NPCName, nameStyle);
                }
            }
        }

        private void DrawSoilIndicators()
        {
            if (!isTutorialActive || currentStage != TutorialStage.FarmingTutorial) return;
            if (Camera.main == null) return;

            // Tìm ruộng đất trong scene để hiển thị chỉ thị đường
            SoilCell[] soilCells = FindObjectsByType<SoilCell>(FindObjectsInactive.Exclude);
            if (soilCells == null || soilCells.Length == 0) return;

            GUIStyle nameStyle = new GUIStyle();
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = new Color(0.2f, 0.9f, 0.3f, 1f); // Màu xanh lá cây
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.fontSize = 11;

            GUIStyle exclamationStyle = new GUIStyle();
            exclamationStyle.alignment = TextAnchor.MiddleCenter;
            exclamationStyle.normal.textColor = new Color(0.2f, 0.9f, 0.3f, 1f);
            exclamationStyle.fontStyle = FontStyle.Bold;
            exclamationStyle.fontSize = 28;

            int drawnCount = 0;
            foreach (var cell in soilCells)
            {
                if (cell == null) continue;
                if (drawnCount >= 1) break; // Chỉ cần đánh dấu 1 ô ruộng duy nhất

                Vector3 worldPos = cell.transform.position + Vector3.up * 1.0f;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                if (screenPos.z > 0)
                {
                    float guiY = Screen.height - screenPos.y;

                    float pulse = Mathf.PingPong(Time.time * 2f, 0.3f);
                    exclamationStyle.normal.textColor = new Color(0.2f, 0.9f + pulse * 0.3f, 0.3f, 0.8f + pulse);

                    // Bóng đổ dấu "!" xanh lá
                    GUIStyle exShadow = new GUIStyle(exclamationStyle);
                    exShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                    GUI.Label(new Rect(screenPos.x - 20 + 1, guiY - 25 + 1, 40, 25), "!", exShadow);
                    
                    // Vẽ dấu "!" xanh lá cây chỉ ruộng
                    GUI.Label(new Rect(screenPos.x - 20, guiY - 25, 40, 25), "!", exclamationStyle);

                    // Tên chỉ hướng "Ruộng Đất"
                    GUIStyle nameShadow = new GUIStyle(nameStyle);
                    nameShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                    GUI.Label(new Rect(screenPos.x - 100 + 1, guiY + 1, 200, 20), "Ruộng Đất", nameShadow);
                    GUI.Label(new Rect(screenPos.x - 100, guiY, 200, 20), "Ruộng Đất", nameStyle);

                    drawnCount++;
                }
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

        private void DrawHearthIndicator()
        {
            if (!isTutorialActive || currentStage != TutorialStage.CraftPreservedCrops) return;
            if (Camera.main == null) return;

            SownInStone.Interactions.KitchenHearth hearth = FindAnyObjectByType<SownInStone.Interactions.KitchenHearth>();
            if (hearth == null) return;

            GUIStyle nameStyle = new GUIStyle();
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = new Color(0.9f, 0.4f, 0.2f, 1f); // Cam
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.fontSize = 11;

            GUIStyle exclamationStyle = new GUIStyle();
            exclamationStyle.alignment = TextAnchor.MiddleCenter;
            exclamationStyle.normal.textColor = new Color(0.9f, 0.4f, 0.2f, 1f);
            exclamationStyle.fontStyle = FontStyle.Bold;
            exclamationStyle.fontSize = 28;

            Vector3 worldPos = hearth.transform.position + Vector3.up * 1.5f;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            if (screenPos.z > 0)
            {
                float guiY = Screen.height - screenPos.y;
                float pulse = Mathf.PingPong(Time.time * 2f, 0.3f);
                exclamationStyle.normal.textColor = new Color(0.9f, 0.4f + pulse * 0.3f, 0.2f, 0.8f + pulse);

                // Bóng đổ
                GUIStyle exShadow = new GUIStyle(exclamationStyle);
                exShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                GUI.Label(new Rect(screenPos.x - 20 + 1, guiY - 25 + 1, 40, 25), "!", exShadow);
                
                // Dấu !
                GUI.Label(new Rect(screenPos.x - 20, guiY - 25, 40, 25), "!", exclamationStyle);

                // Tên "Bếp Gas"
                GUIStyle nameShadow = new GUIStyle(nameStyle);
                nameShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                GUI.Label(new Rect(screenPos.x - 100 + 1, guiY + 1, 200, 20), "Bếp Gas", nameShadow);
                GUI.Label(new Rect(screenPos.x - 100, guiY, 200, 20), "Bếp Gas", nameStyle);
            }
        }

        private void DrawHouseIndicator()
        {
            if (!isTutorialActive || currentStage != TutorialStage.PrepareOwnHouse) return;
            if (Camera.main == null) return;

            GameObject house = GameObject.Find("Thanh_House");
            if (house == null) return;

            GUIStyle nameStyle = new GUIStyle();
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = new Color(0.9f, 0.4f, 0.2f, 1f); // Cam
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.fontSize = 11;

            GUIStyle exclamationStyle = new GUIStyle();
            exclamationStyle.alignment = TextAnchor.MiddleCenter;
            exclamationStyle.normal.textColor = new Color(0.9f, 0.4f, 0.2f, 1f);
            exclamationStyle.fontStyle = FontStyle.Bold;
            exclamationStyle.fontSize = 28;

            Vector3 worldPos = house.transform.position + Vector3.up * 2.5f;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            if (screenPos.z > 0)
            {
                float guiY = Screen.height - screenPos.y;
                float pulse = Mathf.PingPong(Time.time * 2f, 0.3f);
                exclamationStyle.normal.textColor = new Color(0.9f, 0.4f + pulse * 0.3f, 0.2f, 0.8f + pulse);

                // Bóng đổ
                GUIStyle exShadow = new GUIStyle(exclamationStyle);
                exShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                GUI.Label(new Rect(screenPos.x - 20 + 1, guiY - 25 + 1, 40, 25), "!", exShadow);
                
                // Dấu !
                GUI.Label(new Rect(screenPos.x - 20, guiY - 25, 40, 25), "!", exclamationStyle);

                // Tên "Nhà Của Bạn"
                GUIStyle nameShadow = new GUIStyle(nameStyle);
                nameShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                GUI.Label(new Rect(screenPos.x - 100 + 1, guiY + 1, 200, 20), "Nhà Của Bạn", nameShadow);
                GUI.Label(new Rect(screenPos.x - 100, guiY, 200, 20), "Nhà Của Bạn", nameStyle);
            }
        }
    }
}
