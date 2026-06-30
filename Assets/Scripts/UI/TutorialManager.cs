using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SownInStone.Core;
using SownInStone.UI;
using SownInStone.Agriculture;
using SownInStone.Storage;
using SownInStone.Weather;

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
            RescuingNPCs,         // Stage 11: Rescue 4 NPCs from flood
            RoofSurvivalSharing,  // Stage 12: Share remaining food on the roof
            PostStormCleanup,     // Stage 13: Clean up and replant crops after flood (PhuSa)
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
        public bool shouldTriggerLoudspeakerOnDialogueClose = false;

        [Header("--- SINH TỒN & CỨU HỘ ---")]
        public int rescuedNPCsCount = 0;
        public float rescueTimeRemaining = 120f;
        public bool oThamRescued = false;
        public bool bacNamRescued = false;
        public bool cuBayRescued = false;
        public bool beTiRescued = false;

        public bool oThamFed = false;
        public bool bacNamFed = false;
        public bool cuBayFed = false;
        public bool beTiFed = false;

        public bool oThamHouseCleaned = false;
        public bool bacNamHouseCleaned = false;
        public bool cuBayHouseCleaned = false;
        public bool beTiHouseCleaned = false;
        public int postStormCropsPlanted = 0;
        
        // ── Phase 2 independent-task system ─────────────────────────────
        // ── Bác Năm task (gia cố nhà: bao cát + tấm chắn lũ) ───────────
        public bool[] bacNamTargetsPlaced = new bool[10];
        public int bacNamSandbagsPlaced = 0;    // target = 4
        public int bacNamFloodBoardsPlaced = 0; // target = 2
        public bool bacNamSandbagsDone = false;
        public bool bacNamFloodBoardsDone = false;
        public bool bacNamJobAccepted = false;   // player visited Bác Năm and started

        // ── O Thắm task (cất đồ vào rương của O Thắm) ───────────────────
        public int oThamItemsStored = 0;        // target = 5 (total stored in chest)
        public bool oThamStoreDone = false;
        public int oThamCarryingCount = 0;      // how many O Thắm's items player currently holds
        public bool oThamJobAccepted = false;    // player talked to O Thắm and got items
        public bool[] oThamTargetsPlaced = new bool[10]; // kept for ghost snap positions

        // ── Own House task (tự gia cố nhà) ───────────────────
        public System.Collections.Generic.List<GameObject> ownHouseGhostFloodboards = new System.Collections.Generic.List<GameObject>();
        public bool[] ownHouseFloodboardsPlaced = new bool[4];

        // Legacy compat shims
        public int oThamBoardsPlaced = 0;
        public int ownHouseSandbagsPlaced = 0; // mapped to ownHouseFloodboardsPlacedCount internally
        public enum ActiveStormJob { None, OThamFloodboards, BacNamSandbags }
        public ActiveStormJob activeStormJob = ActiveStormJob.None;

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

        public System.Collections.Generic.List<GameObject> ghostFloodboards = new System.Collections.Generic.List<GameObject>();
        public System.Collections.Generic.List<GameObject> ghostSandbags = new System.Collections.Generic.List<GameObject>();
        public System.Collections.Generic.Dictionary<Renderer, Material[]> originalGhostMaterials = new System.Collections.Generic.Dictionary<Renderer, Material[]>();

        private void Start()
        {
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
            InitializeGhostTargets();
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
                else if (shouldTriggerLoudspeakerOnDialogueClose)
                {
                    shouldTriggerLoudspeakerOnDialogueClose = false;
                    currentStage = TutorialStage.PrepareForStorm;
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.TransitionToPhase(GamePhase.ChuanBiBao);
                    }
                    TriggerLoudspeakerAnnouncement();
                    UpdateHUDPanel();
                    npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
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
            if (!isTutorialActive) return;

            if (currentStage == TutorialStage.FarmingTutorial)
            {
                subTask2Completed = true;
                UpdateHUDPanel();
            }
            else if (currentStage == TutorialStage.PostStormCleanup)
            {
                postStormCropsPlanted++;
                CheckPostStormCleanupProgress();
            }
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
            if (StorageManager.Instance != null && PlayerController.Instance != null)
            {
                ItemData seed = PlayerController.Instance.seedItem;
#if UNITY_EDITOR
                if (seed == null)
                {
                    seed = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Seed.asset");
                }
#endif
                if (seed != null)
                {
                    StorageManager.Instance.AddItem(seed, 12);
                    SurvivalUIManager.Instance?.ShowHUDToast("<color=#2ECC71>Bạn nhận được 12 hạt giống khoai lang từ O Thắm</color>");
                }
                else
                {
                    Debug.LogError("[TUTORIAL] Không tìm thấy Hạt giống (seedItem) để cấp cho người chơi!");
                }
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
                shouldTriggerLoudspeakerOnDialogueClose = true;
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

        // ── Phase 2 initialisation ──────────────────────────────────────
        /// <summary>Reset tất cả trạng thái Phase 2. Gọi khi bước vào giai đoạn này.</summary>
        public void InitPhase2()
        {
            // Bác Năm
            bacNamSandbagsPlaced = 0;
            bacNamFloodBoardsPlaced = 0;
            bacNamSandbagsDone = false;
            bacNamFloodBoardsDone = false;
            bacNamJobAccepted = false;
            System.Array.Clear(bacNamTargetsPlaced, 0, bacNamTargetsPlaced.Length);
            // Hide ghost objects until player accepts Bác Năm's task
            foreach (var bag in ghostSandbags)   { if (bag != null) bag.SetActive(false); }
            foreach (var board in ghostFloodboards) { if (board != null) board.SetActive(false); }

            // O Thắm
            oThamItemsStored = 0;
            oThamStoreDone = false;
            oThamCarryingCount = 0;
            oThamJobAccepted = false;
            System.Array.Clear(oThamTargetsPlaced, 0, oThamTargetsPlaced.Length);

            // Legacy
            oThamPrepped = false;
            bacNamPrepped = false;
            oThamBoardsPlaced = 0;
            activeStormJob = ActiveStormJob.None;

            UpdateHUDPanel();
            EnsureOThamChestExists();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
            SurvivalUIManager.Instance?.ShowHUDToast("Đến nhà Bác Năm hoặc nhà O Thắm để nhận và làm nhiệm vụ!");
        }

        // Kept for legacy callers — both now call InitPhase2 then immediately start their specific job
        public void StartBothStormJobs() { InitPhase2(); }

        // ── Bác Năm task ────────────────────────────────────────────────
        /// <summary>
        /// Gọi khi người chơi nói chuyện với Bác Năm và nhận nhiệm vụ.
        /// Teleport lên mái nhà, kích hoạt ghost objects.
        /// </summary>
        public void StartBacNamJob()
        {
            if (currentStage != TutorialStage.PrepareForStorm) return;
            bacNamJobAccepted = true;
            activeStormJob = ActiveStormJob.BacNamSandbags;

            // Chỉ kích hoạt ván chắn trước cửa lúc bắt đầu
            foreach (var bag in ghostSandbags)    { if (bag != null) bag.SetActive(false); }
            for (int i = 0; i < Mathf.Min(ghostFloodboards.Count, 2); i++)
            {
                if (ghostFloodboards[i] != null) ghostFloodboards[i].SetActive(true);
            }

            // Giữ người chơi dưới đất nói chuyện
            SurvivalUIManager.Instance?.ShowHUDToast("Hãy lắp 2 Tấm chắn lũ trước cửa nhà Bác Năm trước!");

            UpdateHUDPanel();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        public void OnBacNamSandbagPlaced()
        {
            if (!isTutorialActive || currentStage != TutorialStage.PrepareForStorm) return;
            bacNamSandbagsPlaced++;
            if (bacNamSandbagsPlaced >= 4 && !bacNamSandbagsDone)
            {
                bacNamSandbagsDone = true;
                SurvivalUIManager.Instance?.ShowHUDToast("✓ Xếp đủ 4 bao cát quanh nhà Bác Năm!");
            }
            CheckBacNamJobComplete();
        }

        public void OnBacNamFloodBoardPlaced()
        {
            if (!isTutorialActive || currentStage != TutorialStage.PrepareForStorm) return;
            bacNamFloodBoardsPlaced++;
            if (bacNamFloodBoardsPlaced >= 2 && !bacNamFloodBoardsDone)
            {
                bacNamFloodBoardsDone = true;
                SurvivalUIManager.Instance?.ShowHUDToast("✓ Lắp đủ 2 tấm chắn cửa nhà Bác Năm!");

                // Dịch chuyển người chơi lên mái nhà để đặt bao cát
                GameObject bacNamHouse = GameObject.Find("BacNam_House");
                if (bacNamHouse != null)
                {
                    Vector3 roofCenter = bacNamHouse.transform.position + Vector3.up * 4.3f;
                    SafeTeleportPlayer(roofCenter);
                    SurvivalUIManager.Instance?.ShowHUDToast("Đã lên mái nhà Bác Năm – Hãy đặt 4 Bao cát gia cố!");
                }

                // Kích hoạt 4 bao cát chỉ dẫn trên mái
                foreach (var bag in ghostSandbags)
                {
                    if (bag != null) bag.SetActive(true);
                }
            }
            CheckBacNamJobComplete();
        }

        public void SafeTeleportPlayer(Vector3 position)
        {
            if (PlayerController.Instance != null)
            {
                var cc = PlayerController.Instance.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                var rb = PlayerController.Instance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.position = position;
                }

                PlayerController.Instance.transform.position = position;

                if (cc != null) cc.enabled = true;
                Debug.Log($"[SAFE TELEPORT] Player teleported to {position}");
            }
        }

        private void TeleportPlayerToBacNam()
        {
            var npcs = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
            var bacNamNPC = System.Array.Find(npcs, n => n.characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.BacNam);
            if (bacNamNPC != null)
            {
                SafeTeleportPlayer(bacNamNPC.transform.position + bacNamNPC.transform.forward * 1.5f);
            }
            else
            {
                GameObject bacNamHouse = GameObject.Find("BacNam_House");
                if (bacNamHouse != null)
                {
                    Vector3 yardPos = bacNamHouse.transform.position + bacNamHouse.transform.forward * 4.5f + bacNamHouse.transform.right * 1.5f;
                    yardPos.y = 0.2f;
                    SafeTeleportPlayer(yardPos);
                }
                else
                {
                    SafeTeleportPlayer(new Vector3(20f, 0.2f, 10f));
                }
            }
        }

        private void CheckBacNamJobComplete()
        {
            UpdateHUDPanel();
            if (bacNamSandbagsDone && bacNamFloodBoardsDone && !bacNamPrepped)
            {
                bacNamPrepped = true;
                TeleportPlayerToBacNam();
                SurvivalUIManager.Instance?.ShowDialogue("Bác Năm", "\"Tốt lắm Thành ơi! Nhà bác đã được gia cố bao cát và tấm chắn lũ cẩn thận rồi, cảm ơn con nhiều!\"");
                CheckBothJobsComplete();
            }
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        private int GetNoodlesCapacityLeft()
        {
            if (StorageManager.Instance == null) return 0;
            ItemData noodles = StorageManager.Instance.GetItemDataByID("item_mi_tom");
            if (noodles == null) return 0;
            int maxStack = StorageManager.Instance.GetMaxBackpackStack(noodles);
            var slot = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == noodles.ItemID);
            int currentQty = slot != null ? slot.quantity : 0;
            return Mathf.Max(0, maxStack - currentQty);
        }

        // ── O Thắm task ─────────────────────────────────────────────────
        /// <summary>
        /// Gọi khi người chơi nói chuyện với O Thắm và nhận đồ.
        /// O Thắm đưa thêm một lô đồ (tối đa 5 lần tổng cộng) vào oThamCarryingCount.
        /// Nếu balo quá đầy, thông báo về cất đồ trước.
        /// </summary>
        public void StartOThamJob()
        {
            if (currentStage != TutorialStage.PrepareForStorm) return;
            oThamJobAccepted = true;
            activeStormJob = ActiveStormJob.OThamFloodboards;

            int remaining = 5 - oThamItemsStored - oThamCarryingCount;
            if (remaining <= 0)
            {
                SurvivalUIManager.Instance?.ShowHUDToast("Bạn đã cất hoặc đang mang đủ đồ cho O Thắm rồi!");
                return;
            }

            if (StorageManager.Instance == null) return;
            ItemData noodles = StorageManager.Instance.GetItemDataByID("item_mi_tom");
            if (noodles == null)
            {
                SurvivalUIManager.Instance?.ShowHUDToast("Lỗi: Không tìm thấy vật phẩm Mì tôm cứu trợ!");
                return;
            }

            // Check if backpack can hold the noodles without overflowing
            int capacityLeft = GetNoodlesCapacityLeft();
            if (capacityLeft < remaining)
            {
                SurvivalUIManager.Instance?.ShowDialogue("O Thắm", "\"Balo con không đủ chỗ chứa thêm Mì tôm đâu! Hãy cất bớt đồ rồi quay lại lấy nhé!\"");
                SurvivalUIManager.Instance?.ShowHUDToast("⚠️ Balo đầy! Hãy cất bớt đồ khác ở hòm đồ nhà mình rồi quay lại.");
                return;
            }

            // Give noodles to player's backpack
            StorageManager.Instance.AddItem(noodles, remaining);
            oThamCarryingCount += remaining;
            
            SurvivalUIManager.Instance?.ShowDialogue("O Thắm", $"\"O gửi con {remaining} gói mì tôm cứu trợ này, mau đem cất vào rương gỗ trước cửa tiệm giúp o nhé!\"");
            SurvivalUIManager.Instance?.ShowHUDToast($"Nhận {remaining} gói Mì tôm cứu trợ – Mang ra rương gỗ trước tiệm cất!");

            UpdateHUDPanel();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        /// <summary>
        /// Gọi khi người chơi bấm E vào rương O Thắm để cất đồ đang mang.
        /// </summary>
        public void OnOThamItemStored()
        {
            if (!isTutorialActive || currentStage != TutorialStage.PrepareForStorm) return;
            
            if (StorageManager.Instance == null) return;
            ItemData noodles = StorageManager.Instance.GetItemDataByID("item_mi_tom");
            if (noodles == null) return;

            var slot = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == noodles.ItemID);
            int carryingInBackpack = slot != null ? slot.quantity : 0;

            // We only take noodles up to what O Thắm gave us for this job
            int toStore = Mathf.Min(carryingInBackpack, oThamCarryingCount);

            if (toStore <= 0)
            {
                SurvivalUIManager.Instance?.ShowHUDToast("Bạn không mang gói Mì tôm cứu trợ nào của O Thắm!");
                return;
            }

            // Remove noodles from backpack
            if (StorageManager.Instance.RemoveItem(noodles, toStore))
            {
                oThamItemsStored += toStore;
                oThamCarryingCount = Mathf.Max(0, oThamCarryingCount - toStore);

                if (oThamItemsStored >= 5 && !oThamStoreDone)
                {
                    oThamStoreDone = true;
                    oThamPrepped = true;
                    SurvivalUIManager.Instance?.ShowDialogue("O Thắm", "\"O cảm ơn con nhiều nha! Đồ đạc đã cất gọn vào rương tránh bão hết rồi, yên tâm lắm!\"");
                    CheckBothJobsComplete();
                }
                else
                {
                    int left = 5 - oThamItemsStored;
                    SurvivalUIManager.Instance?.ShowHUDToast(
                        $"Đã cất {toStore} gói Mì tôm vào rương O Thắm! ({oThamItemsStored}/5) – Còn {left} món, quay lại lấy tiếp nhé!");
                }
                UpdateHUDPanel();
            }
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        // Legacy shim for old placement callers
        public void OnOThamFloodBoardPlaced() { OnBacNamFloodBoardPlaced(); }

        private void CheckBothJobsComplete()
        {
            if (bacNamPrepped && oThamPrepped)
            {
                StartPrepareOwnHouseStage();
            }
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        public void StartPrepareOwnHouseStage()
        {
            currentStage = TutorialStage.PrepareOwnHouse;
            if (StorageManager.Instance != null && SurvivalUIManager.Instance != null)
            {
                ItemData floodboard = SurvivalUIManager.Instance.FloodBoardItem;
                if (floodboard != null)
                {
                    StorageManager.Instance.AddItem(floodboard, 4);
                    SurvivalUIManager.Instance.ShowHUDToast("Bạn nhận được 4 Tấm chắn để gia cố nhà mình");
                }
            }

            ownHouseGhostFloodboards.Clear();
            for (int i = 0; i < 4; i++) ownHouseGhostFloodboards.Add(null);
            for (int i = 0; i < 4; i++) ownHouseFloodboardsPlaced[i] = false;

            EnsureOwnHouseGhostsExist();

            UpdateHUDPanel();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        public void OnOwnHouseSandbagPlaced()
        {
            if (!isTutorialActive || currentStage != TutorialStage.PrepareOwnHouse) return;
            ownHouseSandbagsPlaced++;
            if (ownHouseSandbagsPlaced >= 4)
            {
                if (SurvivalUIManager.Instance != null)
                {
                    SurvivalUIManager.Instance.ShowDialogue(
                        "Nhà của bạn", 
                        "\"Bạn đã lắp đủ 4 tấm chắn gia cố trước cửa nhà. Ngôi nhà của bạn hiện đã sẵn sàng ứng phó với cơn bão sắp tới!\""
                    );
                }
                currentStage = TutorialStage.TalkToCuBayWorship;
                UpdateHUDPanel();
                npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
            }
            else
            {
                UpdateHUDPanel();
            }
        }

        public void OnCuBayWorshipTalked()
        {
            if (!isTutorialActive || currentStage != TutorialStage.TalkToCuBayWorship) return;
            currentStage = TutorialStage.WorshipAltar;
            UpdateHUDPanel();
            npcsInScene = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Exclude);
        }

        private System.Collections.Generic.Dictionary<string, Vector3> npcHomePositions = new System.Collections.Generic.Dictionary<string, Vector3>();

        public void StoreNPCHomePositions()
        {
            if (npcsInScene == null) return;
            npcHomePositions.Clear();
            foreach (var npc in npcsInScene)
            {
                if (npc != null)
                {
                    npcHomePositions[npc.NPCName] = npc.transform.position;
                }
            }
            if (npcHomePositions.Count < 4)
            {
                var allNPCs = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Include);
                foreach (var npc in allNPCs)
                {
                    if (npc != null && !npcHomePositions.ContainsKey(npc.NPCName))
                    {
                        npcHomePositions[npc.NPCName] = npc.transform.position;
                    }
                }
            }
        }

        public void ResetNPCsToHomePositions()
        {
            var allNPCs = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Include);
            foreach (var npc in allNPCs)
            {
                if (npc != null)
                {
                    npc.gameObject.SetActive(true);
                    var visual = npc.transform.Find("Visual");
                    if (visual != null) visual.gameObject.SetActive(true);

                    if (npcHomePositions.TryGetValue(npc.NPCName, out Vector3 homePos))
                    {
                        npc.transform.position = homePos;
                        var rb = npc.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.linearVelocity = Vector3.zero;
                            rb.position = homePos;
                        }
                    }
                }
            }
        }

        public void OnAltarWorshipped()
        {
            if (!isTutorialActive || currentStage != TutorialStage.WorshipAltar) return;
            
            // Store NPC positions first to ensure reset works correctly
            StoreNPCHomePositions();
            StartRescuingNPCsStage();
        }

        public void StartRescuingNPCsStage()
        {
            currentStage = TutorialStage.RescuingNPCs;
            rescuedNPCsCount = 0;
            rescueTimeRemaining = 120f;
            oThamRescued = false;
            bacNamRescued = false;
            cuBayRescued = false;
            beTiRescued = false;

            oThamFed = false;
            bacNamFed = false;
            cuBayFed = false;
            beTiFed = false;

            oThamHouseCleaned = false;
            bacNamHouseCleaned = false;
            cuBayHouseCleaned = false;
            beTiHouseCleaned = false;
            postStormCropsPlanted = 0;

            ResetNPCsToHomePositions();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionToPhase(GamePhase.MuaBao);
            }

            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.SetFloodLevelDirectly(1.2f);
            }

            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_emergency_alarm");

            if (SurvivalUIManager.Instance != null)
            {
                SurvivalUIManager.Instance.ShowDialogue(
                    "BÁO ĐỘNG LŨ LỤT",
                    "Nước lũ dâng cao khẩn cấp ngập lụt làng quê! Bạn hãy chạy thật nhanh qua nhà từng người (O Thắm, Bác Năm, Cụ Bảy, Bé Tí) để cõng họ sơ tán về nóc nhà Thành lánh nạn. Chú ý: Chỉ cõng được từng người một và phải cứu tất cả trước khi nước dâng ngập hoàn toàn!"
                );
            }

            UpdateHUDPanel();
        }

        public void OnNPCRescued(SownInStone.Community.NPCCharacter.StoryCharacterType charType)
        {
            if (charType == SownInStone.Community.NPCCharacter.StoryCharacterType.OTham) oThamRescued = true;
            else if (charType == SownInStone.Community.NPCCharacter.StoryCharacterType.BacNam) bacNamRescued = true;
            else if (charType == SownInStone.Community.NPCCharacter.StoryCharacterType.CuBay) cuBayRescued = true;
            else if (charType == SownInStone.Community.NPCCharacter.StoryCharacterType.BeTi) beTiRescued = true;

            rescuedNPCsCount = 0;
            if (oThamRescued) rescuedNPCsCount++;
            if (bacNamRescued) rescuedNPCsCount++;
            if (cuBayRescued) rescuedNPCsCount++;
            if (beTiRescued) rescuedNPCsCount++;

            SurvivalUIManager.Instance?.ShowHUDToast($"💚 SƠ TÁN THÀNH CÔNG: Đã đưa {rescuedNPCsCount}/4 người lên nóc nhà lánh nạn!");

            if (rescuedNPCsCount >= 4)
            {
                StartRoofSurvivalSharingStage();
            }
            else
            {
                UpdateHUDPanel();
            }
        }

        public void ResetRescueStage()
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.DropCarriedNPCForced();
                GameObject houseObj = GameObject.Find("Thanh_House");
                Vector3 groundPos = houseObj != null ? houseObj.transform.position + new Vector3(0f, 0.2f, -4.2f) : new Vector3(10.66f, 0.2f, -14.2f);
                SafeTeleportPlayer(groundPos);
                var rb = PlayerController.Instance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.position = groundPos;
                }
            }

            ResetNPCsToHomePositions();

            rescuedNPCsCount = 0;
            rescueTimeRemaining = 120f;
            oThamRescued = false;
            bacNamRescued = false;
            cuBayRescued = false;
            beTiRescued = false;

            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.SetFloodLevelDirectly(1.2f);
            }

            if (SurvivalUIManager.Instance != null)
            {
                SurvivalUIManager.Instance.ShowDialogue(
                    "THẤT BẠI SƠ TÁN",
                    "Nước lũ đã dâng ngập quá cao trước khi bạn kịp sơ tán mọi người! Hãy thử lại lần nữa để cứu sống tất cả bà con trong xóm!"
                );
            }

            UpdateHUDPanel();
        }

        private void StartRoofSurvivalSharingStage()
        {
            currentStage = TutorialStage.RoofSurvivalSharing;
            
            // Dâng mực nước lụt cao hẳn che hết vườn tược
            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.SetFloodLevelDirectly(2.0f);
            }

            // Dịch chuyển người chơi lên nóc nhà Thành cùng mọi người
            if (PlayerController.Instance != null)
            {
                GameObject houseObj = GameObject.Find("Thanh_House");
                Vector3 roofPos = houseObj != null ? houseObj.transform.position + new Vector3(0f, 3.4f, 0f) : new Vector3(10.66f, 3.4f, -10.0f);
                SafeTeleportPlayer(roofPos);
                var rb = PlayerController.Instance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.position = roofPos;
                }
            }

            // Phát mì tôm cứu trợ giống như thiết lập cũ
            if (StorageManager.Instance != null && PlayerController.Instance != null && PlayerController.Instance.seedItem != null)
            {
                // Cho thêm mì gói và khoai gieo nếu người chơi không có đủ khoai chia sẻ
                ItemData noodles = StorageManager.Instance.GetItemDataByID("item_mi_tom");
                if (noodles == null) noodles = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Noodles.asset");
                if (noodles != null) StorageManager.Instance.AddItem(noodles, 4);

                ItemData preserved = StorageManager.Instance.GetItemDataByID("item_khoai_gieo");
                if (preserved == null) preserved = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_PreservedCrop.asset");
                if (preserved != null) StorageManager.Instance.AddItem(preserved, 2);
            }

            if (SurvivalUIManager.Instance != null)
            {
                SurvivalUIManager.Instance.ShowDialogue(
                    "CẮM TRẠI NÓC NHÀ",
                    "Cả xóm đã sơ tán lên nóc nhà Thành an toàn! Bên ngoài nước lũ đang ngập úng mênh mông cô lập. Hãy tương tác với lò sưởi ấm, xô nước mưa để sinh tồn, và quan trọng nhất: Hãy chia sẻ khoai gieo dự trữ cho 4 dân làng trên nóc nhà cùng sống qua ngày!"
                );
            }

            UpdateHUDPanel();
        }

        public void FeedNPC(SownInStone.Community.NPCCharacter.StoryCharacterType charType)
        {
            if (charType == SownInStone.Community.NPCCharacter.StoryCharacterType.OTham) oThamFed = true;
            else if (charType == SownInStone.Community.NPCCharacter.StoryCharacterType.BacNam) bacNamFed = true;
            else if (charType == SownInStone.Community.NPCCharacter.StoryCharacterType.CuBay) cuBayFed = true;
            else if (charType == SownInStone.Community.NPCCharacter.StoryCharacterType.BeTi) beTiFed = true;

            int fedCount = 0;
            if (oThamFed) fedCount++;
            if (bacNamFed) fedCount++;
            if (cuBayFed) fedCount++;
            if (beTiFed) fedCount++;

            if (fedCount >= 4)
            {
                StartPostStormCleanupStage();
            }
            else
            {
                UpdateHUDPanel();
            }
        }

        public void StartPostStormCleanupStage()
        {
            currentStage = TutorialStage.PostStormCleanup;

            // Bão lụt đi qua, nước lũ rút hết về 0
            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.SetFloodLevelDirectly(0f);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionToPhase(GamePhase.PhuSa); // Chuyển sang Giai đoạn Phù Sa
            }

            // Trở lại đất liền
            if (PlayerController.Instance != null)
            {
                GameObject houseObj = GameObject.Find("Thanh_House");
                Vector3 groundPos = houseObj != null ? houseObj.transform.position + new Vector3(0f, 0.2f, -4.2f) : new Vector3(10.66f, 0.2f, -14.2f);
                SafeTeleportPlayer(groundPos);
                var rb = PlayerController.Instance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.position = groundPos;
                }
            }

            // Đưa các NPC trở về lại vị trí nhà ban đầu của họ
            ResetNPCsToHomePositions();

            if (SurvivalUIManager.Instance != null)
            {
                SurvivalUIManager.Instance.ShowDialogue(
                    "TÁI THIẾT SAU LŨ",
                    "Thiên tai đi qua, nước lũ đã rút hết để lại bùn phù sa trù phú! Tuy nhiên nhà cửa của bà con đang đầy gạch đá đổ nát. Hãy đi đến nhà từng người để phụ giúp dọn dẹp đống đổ nát, và trồng lại 4 luống cây trên ruộng nhà bạn để bắt đầu vụ mùa mới!"
                );
            }

            UpdateHUDPanel();
        }

        public void CleanHouse(SownInStone.Community.NPCCharacter.StoryCharacterType charType)
        {
            if (charType == SownInStone.Community.NPCCharacter.StoryCharacterType.OTham) oThamHouseCleaned = true;
            else if (charType == SownInStone.Community.NPCCharacter.StoryCharacterType.BacNam) bacNamHouseCleaned = true;
            else if (charType == SownInStone.Community.NPCCharacter.StoryCharacterType.CuBay || charType == SownInStone.Community.NPCCharacter.StoryCharacterType.BeTi) cuBayHouseCleaned = true;

            CheckPostStormCleanupProgress();
        }

        private void CheckPostStormCleanupProgress()
        {
            if (oThamHouseCleaned && bacNamHouseCleaned && cuBayHouseCleaned && postStormCropsPlanted >= 4)
            {
                CompleteTutorialCampaign();
            }
            else
            {
                UpdateHUDPanel();
            }
        }

        private void CompleteTutorialCampaign()
        {
            currentStage = TutorialStage.Completed;
            isTutorialActive = false;

            if (hudPanel != null)
            {
                Destroy(hudPanel);
                hudPanel = null;
            }

            if (SurvivalUIManager.Instance != null)
            {
                SurvivalUIManager.Instance.ShowDialogue(
                    "HOÀN THÀNH TÁI THIẾT",
                    "Bạn thật tuyệt vời! Làng quê nghèo đã bắt đầu nảy mầm xanh trên lớp phù sa trù phú. Cả xóm vô cùng biết ơn tấm lòng của bạn. Trò chơi sẽ tiếp tục tự động trôi ngày đến Ngày 8 để đưa ra kết cục câu chuyện dựa trên điểm Nghĩa Tình Karma của bạn!"
                );
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

        private void EnsureOwnHouseGhostsExist()
        {
            if (currentStage == TutorialStage.PrepareOwnHouse)
            {
                // Pad list to 4 elements if needed
                while (ownHouseGhostFloodboards.Count < 4)
                {
                    ownHouseGhostFloodboards.Add(null);
                }

                GameObject ownHouse = GameObject.Find("Thanh_House");
                if (ownHouse != null)
                {
                    Vector3 houseCenter = ownHouse.transform.position;
                    Vector3[] houseOffsets = new Vector3[]
                    {
                        houseCenter + new Vector3(-3.2f, 0.1f, -2.2f),
                        houseCenter + new Vector3(3.2f, 0.1f, -2.2f),
                        houseCenter + new Vector3(-3.2f, 0.1f, 1.8f),
                        houseCenter + new Vector3(3.2f, 0.1f, 1.8f)
                    };

                    for (int i = 0; i < 4; i++)
                    {
                        if (ownHouseGhostFloodboards[i] != null && Vector3.Distance(ownHouseGhostFloodboards[i].transform.position, houseOffsets[i]) > 0.5f)
                        {
                            Destroy(ownHouseGhostFloodboards[i]);
                            ownHouseGhostFloodboards[i] = null;
                        }

                        if (ownHouseFloodboardsPlaced[i])
                        {
                            if (ownHouseGhostFloodboards[i] != null)
                            {
                                ownHouseGhostFloodboards[i].SetActive(false);
                            }
                            continue;
                        }

                        if (ownHouseGhostFloodboards[i] == null)
                        {
                            string prefabPath = "Prefabs/FloodBoard";
                            GameObject prefab = Resources.Load<GameObject>(prefabPath);
                            if (prefab != null)
                            {
                                GameObject newGhost = Instantiate(prefab);
                                newGhost.name = $"OwnHouse_Ghost_Floodboard_{i}";
                                newGhost.transform.position = houseOffsets[i];
                                newGhost.transform.rotation = ownHouse.transform.rotation;
                                MakeGhostModel(newGhost);
                                newGhost.SetActive(true);
                                ownHouseGhostFloodboards[i] = newGhost;
                                Debug.Log($"[GHOST OWN HOUSE] Instantiated ghost floodboard {i} at {newGhost.transform.position}");
                            }
                        }
                    }
                }
            }
        }

        public void UpdateHUDPanel()
        {
            if (hudPanel == null) return;
            EnsureOwnHouseGhostsExist();

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

                bool isInside = (PlayerController.Instance != null && PlayerController.Instance.IsInsideHouse);

                if (!isInside)
                {
                    hudTaskAText.text = " <color=#E74C3C>☐</color> Đi vào trong nhà của mình (đến gần cửa nhà, nhấn E)";
                    hudTaskAText.color = Color.white;
                    if (hudTaskBText != null)
                    {
                        hudTaskBText.gameObject.SetActive(true);
                        hudTaskBText.text = " <color=#BDC3C7>☐</color> Tương tác với Bếp Gas để chế biến";
                        hudTaskBText.color = Color.gray;
                    }
                }
                else
                {
                    hudTaskAText.text = " <color=#2ECC71>☑</color> Đi vào trong nhà của mình";
                    hudTaskAText.color = new Color(0.7f, 0.7f, 0.7f);
                    if (hudTaskBText != null)
                    {
                        hudTaskBText.gameObject.SetActive(true);
                        hudTaskBText.text = $" <color=#E74C3C>☐</color> Chế biến khoai gieo ({preservedCropsCrafted}/4) tại Bếp Gas";
                        hudTaskBText.color = Color.white;
                    }
                }

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

                // Task A: Bác Năm – fortify house (sandbags + flood boards)
                bool bacNamAllDone = bacNamSandbagsDone && bacNamFloodBoardsDone;
                if (!bacNamJobAccepted)
                    hudTaskAText.text = " <color=#F39C12>►</color> Bác Năm – Chưa nhận việc";
                else if (bacNamAllDone)
                    hudTaskAText.text = " <color=#2ECC71>✓</color> Bác Năm – Đã gia cố xong nhà!";
                else
                    hudTaskAText.text =
                        (bacNamSandbagsDone ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") +
                        $"Bao cát ({bacNamSandbagsPlaced}/4)" +
                        "  " +
                        (bacNamFloodBoardsDone ? "<color=#2ECC71>✓</color> " : "<color=#E74C3C>☐</color> ") +
                        $"Tấm chắn ({bacNamFloodBoardsPlaced}/2)";
                hudTaskAText.color = bacNamAllDone ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;

                // Task B: O Thắm – store items in chest
                if (hudTaskBText != null)
                {
                    hudTaskBText.gameObject.SetActive(true);
                    if (!oThamJobAccepted)
                        hudTaskBText.text = " <color=#F39C12>►</color> O Thắm – Chưa nhận việc";
                    else if (oThamStoreDone)
                        hudTaskBText.text = " <color=#2ECC71>✓</color> O Thắm – Đã cất đủ đồ vào rương!";
                    else if (oThamCarryingCount > 0)
                        hudTaskBText.text = $" <color=#E8C73A>▲</color> Đang mang {oThamCarryingCount} món – Ra rương O Thắm cất!";
                    else
                        hudTaskBText.text = $" <color=#E74C3C>☐</color> O Thắm – Cất đồ vào rương ({oThamItemsStored}/5)";
                    hudTaskBText.color = oThamStoreDone ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                }

                if (hudTaskCText != null) hudTaskCText.gameObject.SetActive(false);
                if (hudTaskDText != null) hudTaskDText.gameObject.SetActive(false);
            }
            else if (currentStage == TutorialStage.PrepareOwnHouse)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "GIA CỐ NHÀ MÌNH";

                hudTaskAText.text = (ownHouseSandbagsPlaced >= 4 ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + $"Chuẩn bị tấm chắn bảo vệ nhà mình ({ownHouseSandbagsPlaced}/4)";
                hudTaskAText.color = ownHouseSandbagsPlaced >= 4 ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;

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
            else if (currentStage == TutorialStage.RescuingNPCs)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = $"CỨU HỘ DÂN LÀNG (Còn {(int)rescueTimeRemaining}s)";

                hudTaskAText.text = (oThamRescued ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Cứu hộ O Thắm";
                hudTaskAText.color = oThamRescued ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;

                hudTaskBText.text = (bacNamRescued ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Cứu hộ Bác Năm";
                hudTaskBText.color = bacNamRescued ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskBText.gameObject.SetActive(true);

                hudTaskCText.text = (cuBayRescued ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Cứu hộ Cụ Bảy";
                hudTaskCText.color = cuBayRescued ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskCText.gameObject.SetActive(true);

                hudTaskDText.text = (beTiRescued ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Cứu hộ Bé Tí";
                hudTaskDText.color = beTiRescued ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskDText.gameObject.SetActive(true);
            }
            else if (currentStage == TutorialStage.RoofSurvivalSharing)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "CHIA SẺ KHOAI GIEO NÓC NHÀ";

                hudTaskAText.text = (oThamFed ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "O Thắm (Đã no)";
                hudTaskAText.color = oThamFed ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;

                hudTaskBText.text = (bacNamFed ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Bác Năm (Đã no)";
                hudTaskBText.color = bacNamFed ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskBText.gameObject.SetActive(true);

                hudTaskCText.text = (cuBayFed ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Cụ Bảy (Đã no)";
                hudTaskCText.color = cuBayFed ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskCText.gameObject.SetActive(true);

                hudTaskDText.text = (beTiFed ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Bé Tí (Đã no)";
                hudTaskDText.color = beTiFed ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskDText.gameObject.SetActive(true);
            }
            else if (currentStage == TutorialStage.PostStormCleanup)
            {
                hudPanel.SetActive(true);
                hudTitleText.text = "TÁI THIẾT SAU LŨ LỤT";

                hudTaskAText.text = (oThamHouseCleaned ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Phụ dọn dẹp nhà O Thắm";
                hudTaskAText.color = oThamHouseCleaned ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;

                hudTaskBText.text = (bacNamHouseCleaned ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Phụ dọn dẹp nhà Bác Năm";
                hudTaskBText.color = bacNamHouseCleaned ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskBText.gameObject.SetActive(true);

                hudTaskCText.text = (cuBayHouseCleaned ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + "Phụ dọn dẹp nhà Cụ Bảy & Bé Tí";
                hudTaskCText.color = cuBayHouseCleaned ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskCText.gameObject.SetActive(true);

                hudTaskDText.text = (postStormCropsPlanted >= 4 ? " <color=#2ECC71>✓</color> " : " <color=#E74C3C>☐</color> ") + $"Gieo trồng lại hoa màu ({postStormCropsPlanted}/4)";
                hudTaskDText.color = postStormCropsPlanted >= 4 ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
                hudTaskDText.gameObject.SetActive(true);
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

            EnsureOwnHouseGhostsExist();

            if (currentStage == TutorialStage.PrepareForStorm)
            {
                if (bacNamFloodBoardsPlaced >= 2 && !bacNamSandbagsDone)
                {
                    if (PlayerController.Instance != null && PlayerController.Instance.transform.position.y < 2.0f)
                    {
                        GameObject bacNamHouse = GameObject.Find("BacNam_House");
                        if (bacNamHouse != null)
                        {
                            Vector3 roofCenter = bacNamHouse.transform.position + Vector3.up * 4.3f;
                            SafeTeleportPlayer(roofCenter);
                            SurvivalUIManager.Instance?.ShowHUDToast("Đã lên mái nhà Bác Năm – Hãy đặt 4 Bao cát gia cố!");
                        }
                    }

                    foreach (var bag in ghostSandbags)
                    {
                        if (bag != null && !bag.activeSelf) bag.SetActive(true);
                    }
                }
            }

            if (currentStage == TutorialStage.PrepareOwnHouse)
            {
                if (PlayerController.Instance != null && PlayerController.Instance.transform.position.y > 2.0f)
                {
                    GameObject bacNamHouse = GameObject.Find("BacNam_House");
                    if (bacNamHouse != null && Vector3.Distance(PlayerController.Instance.transform.position, bacNamHouse.transform.position) < 15f)
                    {
                        Vector3 yardPos = bacNamHouse.transform.position + bacNamHouse.transform.forward * 4.5f + bacNamHouse.transform.right * 1.5f;
                        yardPos.y = 0.2f;
                        SafeTeleportPlayer(yardPos);
                        SurvivalUIManager.Instance?.ShowHUDToast("Đã tự động đưa bạn xuống đất để làm nhiệm vụ gia cố nhà mình!");
                    }
                }
            }

            if (currentStage == TutorialStage.RescuingNPCs)
            {
                rescueTimeRemaining -= Time.deltaTime;
                UpdateHUDPanel();
                if (rescueTimeRemaining <= 0f)
                {
                    ResetRescueStage();
                }
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

            // Vẽ các chỉ thị đặt bao cát / ván chắn chống bão
            DrawStormPrepIndicators();

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
            // Vô hiệu hóa để sử dụng duy nhất hệ thống dấu chấm hỏi/chấm than nảy của NPCQuestMarkerUI
            return;
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

            bool isInside = (PlayerController.Instance != null && PlayerController.Instance.IsInsideHouse);

            Vector3 worldPos = Vector3.zero;
            string targetName = "";

            if (!isInside)
            {
                // Người chơi đang ở ngoài: chỉ hướng đi vào nhà của mình
                GameObject house = GameObject.Find("Thanh_House");
                if (house == null) return;
                // Vị trí cửa nhà
                worldPos = house.transform.position + new Vector3(0f, 2.5f, 2.0f);
                targetName = "Vào Trong Nhà";
            }
            else
            {
                // Người chơi đang ở trong nhà: chỉ hướng Bếp Gas
                SownInStone.Interactions.KitchenHearth hearth = FindAnyObjectByType<SownInStone.Interactions.KitchenHearth>();
                if (hearth == null) return;
                worldPos = hearth.transform.position + Vector3.up * 1.5f;
                targetName = "Bếp Gas";
            }

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

                // Tên hiển thị
                GUIStyle nameShadow = new GUIStyle(nameStyle);
                nameShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                GUI.Label(new Rect(screenPos.x - 100 + 1, guiY + 1, 200, 20), targetName, nameShadow);
                GUI.Label(new Rect(screenPos.x - 100, guiY, 200, 20), targetName, nameStyle);
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

        private void DrawStormPrepIndicators()
        {
            if (!isTutorialActive) return;
            if (Camera.main == null) return;

            GUIStyle nameStyle = new GUIStyle();
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.normal.textColor = new Color(1f, 0.6f, 0f, 1f); // Màu cam vàng
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.fontSize = 11;

            GUIStyle exclamationStyle = new GUIStyle();
            exclamationStyle.alignment = TextAnchor.MiddleCenter;
            exclamationStyle.normal.textColor = new Color(1f, 0.6f, 0f, 1f);
            exclamationStyle.fontStyle = FontStyle.Bold;
            exclamationStyle.fontSize = 32;

            if (currentStage == TutorialStage.PrepareForStorm)
            {
                // 1. Bác Năm task indicators (Sandbags on roof + Flood boards at door)
                if (bacNamJobAccepted && !bacNamPrepped)
                {
                    // Sandbags on roof
                    if (ghostSandbags.Count > 0)
                    {
                        for (int i = 0; i < ghostSandbags.Count; i++)
                        {
                            if (ghostSandbags[i] == null || bacNamTargetsPlaced[i]) continue;

                            Vector3 worldPos = ghostSandbags[i].transform.position + Vector3.up * (1.2f + Mathf.Sin(Time.time * 6f) * 0.15f);
                            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                            if (screenPos.z > 0)
                            {
                                float guiY = Screen.height - screenPos.y;
                                GUIStyle exShadow = new GUIStyle(exclamationStyle);
                                exShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                                GUI.Label(new Rect(screenPos.x - 20 + 1, guiY - 25 + 1, 40, 25), "!", exShadow);
                                GUI.Label(new Rect(screenPos.x - 20, guiY - 25, 40, 25), "!", exclamationStyle);

                                GUIStyle nameShadow = new GUIStyle(nameStyle);
                                nameShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                                GUI.Label(new Rect(screenPos.x - 100 + 1, guiY + 1, 200, 20), "Đặt bao cát mái", nameShadow);
                                GUI.Label(new Rect(screenPos.x - 100, guiY, 200, 20), "Đặt bao cát mái", nameStyle);
                            }
                        }
                    }
                    
                    // Flood boards flanking the front door
                    if (ghostFloodboards.Count > 0)
                    {
                        for (int i = 0; i < Mathf.Min(ghostFloodboards.Count, 2); i++)
                        {
                            if (ghostFloodboards[i] == null || oThamTargetsPlaced[i]) continue;

                            Vector3 worldPos = ghostFloodboards[i].transform.position + Vector3.up * (1.2f + Mathf.Sin(Time.time * 6f) * 0.15f);
                            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                            if (screenPos.z > 0)
                            {
                                float guiY = Screen.height - screenPos.y;
                                GUIStyle exShadow = new GUIStyle(exclamationStyle);
                                exShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                                GUI.Label(new Rect(screenPos.x - 20 + 1, guiY - 25 + 1, 40, 25), "!", exShadow);
                                GUI.Label(new Rect(screenPos.x - 20, guiY - 25, 40, 25), "!", exclamationStyle);

                                GUIStyle nameShadow = new GUIStyle(nameStyle);
                                nameShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                                GUI.Label(new Rect(screenPos.x - 100 + 1, guiY + 1, 200, 20), "Dựng vách chắn", nameShadow);
                                GUI.Label(new Rect(screenPos.x - 100, guiY, 200, 20), "Dựng vách chắn", nameStyle);
                            }
                        }
                    }
                }

                // 2. O Thắm task indicator: Chest location
                if (oThamJobAccepted && !oThamStoreDone)
                {
                    var chest = GameObject.FindAnyObjectByType<Interactions.OThamChest>();
                    if (chest != null)
                    {
                        Vector3 worldPos = chest.transform.position + Vector3.up * (1.5f + Mathf.Sin(Time.time * 6f) * 0.15f);
                        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                        if (screenPos.z > 0)
                        {
                            float guiY = Screen.height - screenPos.y;
                            GUIStyle exShadow = new GUIStyle(exclamationStyle);
                            exShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                            GUI.Label(new Rect(screenPos.x - 20 + 1, guiY - 25 + 1, 40, 25), "!", exShadow);
                            GUI.Label(new Rect(screenPos.x - 20, guiY - 25, 40, 25), "!", exclamationStyle);

                            GUIStyle nameShadow = new GUIStyle(nameStyle);
                            nameShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                            GUI.Label(new Rect(screenPos.x - 100 + 1, guiY + 1, 200, 20), "Rương cất mì tôm", nameShadow);
                            GUI.Label(new Rect(screenPos.x - 100, guiY, 200, 20), "Rương cất mì tôm", nameStyle);
                        }
                    }
                }
            }
            else if (currentStage == TutorialStage.PrepareOwnHouse)
            {
                // Hướng dẫn đặt bao cát cho chính nhà mình
                GameObject ownHouse = GameObject.Find("Thanh_House");
                if (ownHouse != null)
                {
                    Vector3 houseCenter = ownHouse.transform.position;
                    Vector3[] targets = new Vector3[]
                    {
                        houseCenter + new Vector3(-3.2f, 0.1f, -2.2f),
                        houseCenter + new Vector3(3.2f, 0.1f, -2.2f),
                        houseCenter + new Vector3(-3.2f, 0.1f, 1.8f),
                        houseCenter + new Vector3(3.2f, 0.1f, 1.8f)
                    };

                    for (int i = 0; i < 4; i++)
                    {
                        if (ownHouseSandbagsPlaced > i) continue;

                        Vector3 worldPos = targets[i] + Vector3.up * (1.2f + Mathf.Sin(Time.time * 6f) * 0.15f);
                        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                        if (screenPos.z > 0)
                        {
                            float guiY = Screen.height - screenPos.y;
                            
                            // Shadow
                            GUIStyle exShadow = new GUIStyle(exclamationStyle);
                            exShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                            GUI.Label(new Rect(screenPos.x - 20 + 1, guiY - 25 + 1, 40, 25), "!", exShadow);
                            
                            // Exclamation mark
                            GUI.Label(new Rect(screenPos.x - 20, guiY - 25, 40, 25), "!", exclamationStyle);

                            // Target text
                            GUIStyle nameShadow = new GUIStyle(nameStyle);
                            nameShadow.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
                            GUI.Label(new Rect(screenPos.x - 100 + 1, guiY + 1, 200, 20), "Gia cố góc nhà", nameShadow);
                            GUI.Label(new Rect(screenPos.x - 100, guiY, 200, 20), "Gia cố góc nhà", nameStyle);
                        }
                    }
                }
            }
        }

        private Material CreateGhostMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material mat = new Material(shader);
            mat.color = color;

            if (shader.name.Contains("Universal Render Pipeline"))
            {
                mat.SetFloat("_Surface", 1f); 
                mat.SetFloat("_Blend", 0f); 
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;
                mat.SetColor("_BaseColor", color);
            }
            else
            {
                mat.SetFloat("_Mode", 2); 
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.renderQueue = 3000;
            }
            return mat;
        }

        private void InitializeGhostTargets()
        {
            ghostFloodboards.Clear();
            ghostSandbags.Clear();
            originalGhostMaterials.Clear();

            GameObject bacNamHouse = GameObject.Find("BacNam_House");
            if (bacNamHouse != null)
            {
                // 1. Tạo 4 bao cát chỉ dẫn trên mái nhà Bác Năm (Đúng thiết kế ban đầu)
                Vector3 roofCenter = bacNamHouse.transform.position + Vector3.up * 3.8f;
                Vector3[] roofOffsets = new Vector3[]
                {
                    new Vector3(-1.2f, 0f, -0.8f),
                    new Vector3(1.2f, 0.1f, -0.8f),
                    new Vector3(-1.2f, 0.1f, 0.8f),
                    new Vector3(1.2f, 0f, 0.8f)
                };

                for (int i = 0; i < 4; i++)
                {
                    string prefabPath = "Prefabs/Sandbag";
                    GameObject prefab = Resources.Load<GameObject>(prefabPath);
                    if (prefab != null)
                    {
                        GameObject newGhost = Instantiate(prefab);
                        newGhost.name = $"Ghost_Sandbag_{i}";
                        newGhost.transform.position = roofCenter + roofOffsets[i];
                        newGhost.transform.rotation = bacNamHouse.transform.rotation;
                        MakeGhostModel(newGhost);
                        newGhost.SetActive(false);
                        ghostSandbags.Add(newGhost);
                    }
                    else
                    {
                        Debug.LogError("[GHOST SETUP] Không load được Prefab bao cát từ Resources!");
                    }
                }

                // 2. Tạo 2 tấm chắn chỉ dẫn trước cửa chính
                Vector3 housePos = bacNamHouse.transform.position;
                Vector3 forward = bacNamHouse.transform.forward;
                Vector3 right = bacNamHouse.transform.right;
                Vector3[] doorPositions = new Vector3[]
                {
                    housePos + forward * 4.2f + right * -1.5f,
                    housePos + forward * 4.2f + right * 1.5f
                };

                for (int i = 0; i < 2; i++)
                {
                    string prefabPath = "Prefabs/FloodBoard";
                    GameObject prefab = Resources.Load<GameObject>(prefabPath);
                    if (prefab != null)
                    {
                        GameObject newGhost = Instantiate(prefab);
                        newGhost.name = $"Ghost_FloodBoard_{i}";
                        newGhost.transform.position = doorPositions[i];
                        newGhost.transform.rotation = bacNamHouse.transform.rotation;
                        MakeGhostModel(newGhost);
                        newGhost.SetActive(false);
                        ghostFloodboards.Add(newGhost);
                    }
                    else
                    {
                        Debug.LogError("[GHOST SETUP] Không load được Prefab tấm chắn từ Resources!");
                    }
                }
            }

            EnsureOThamChestExists();
            Debug.Log($"[GHOST SETUP] Đã tạo thành công {ghostFloodboards.Count} tấm chắn và {ghostSandbags.Count} bao cát chỉ dẫn dạng Hologram.");
        }

        private void EnsureOThamChestExists()
        {
            var existingChest = GameObject.FindAnyObjectByType<Interactions.OThamChest>();
            if (existingChest != null) return; // Already exists

            // Find NPC O Thắm to position the chest in front of her shop
            var npcs = FindObjectsByType<SownInStone.Community.NPCCharacter>(FindObjectsInactive.Include);
            var oTham = System.Array.Find(npcs, n => n.characterType == SownInStone.Community.NPCCharacter.StoryCharacterType.OTham);
            if (oTham != null)
            {
                // Create a cube representing the wooden chest
                GameObject chestObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chestObj.name = "OTham_Chest";
                chestObj.transform.position = oTham.transform.position + oTham.transform.forward * 2.2f + oTham.transform.right * -1.8f;
                chestObj.transform.localScale = new Vector3(0.9f, 0.6f, 0.6f);
                chestObj.transform.rotation = oTham.transform.rotation;

                // Add OThamChest component
                chestObj.AddComponent<Interactions.OThamChest>();

                // Set its color to a brown/wooden color
                Renderer renderer = chestObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.4f, 0.25f, 0.15f); // Brown/wooden color
                }

                // Add a simple solid collider
                BoxCollider boxCol = chestObj.GetComponent<BoxCollider>();
                if (boxCol != null)
                {
                    boxCol.isTrigger = false;
                }

                Debug.Log("[CHEST SETUP] Dynamically spawned O Thắm's chest in front of her tiệm.");
            }
        }

        public void MakeGhostModel(GameObject obj)
        {
            if (obj == null) return;
            Material ghostMat = CreateGhostMaterial(new Color(1f, 0.2f, 0.2f, 0.4f));

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r != null)
                {
                    originalGhostMaterials[r] = r.sharedMaterials;
                    
                    Material[] newMats = new Material[r.sharedMaterials.Length];
                    for (int m = 0; m < newMats.Length; m++)
                    {
                        newMats[m] = ghostMat;
                    }
                    r.materials = newMats;
                }
            }

            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                if (col != null) col.isTrigger = true;
            }
        }

        public void MakeSolidModel(GameObject obj)
        {
            if (obj == null) return;
            
            // Enable solid collisions
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            foreach (var col in colliders)
            {
                if (col != null) col.isTrigger = false;
            }

            // Restore original materials
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r != null && originalGhostMaterials.ContainsKey(r))
                {
                    r.materials = originalGhostMaterials[r];
                }
            }
        }
    }
}
