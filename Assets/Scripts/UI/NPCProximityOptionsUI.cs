using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using SownInStone.Core;
using SownInStone.Community;
using SownInStone.Storage;
using SownInStone.Interactions;

namespace SownInStone.UI
{
    public class NPCProximityOptionsUI : MonoBehaviour
    {
        private NPCCharacter[] npcs;
        private NPCCharacter activeNPC;
        
        private GameObject panelObj;
        private RectTransform panelRect;
        private CanvasGroup canvasGroup;
        private TextMeshProUGUI titleText;
        private List<GameObject> optionButtons = new List<GameObject>();
        
        private struct ProximityOption
        {
            public string label;
            public Action action;
        }
        
        private List<ProximityOption> currentOptions = new List<ProximityOption>();
        
        private float targetAlpha = 0f;
        private float fadeSpeed = 7.5f; // Khoảng 0.13 giây để hoàn thành fade (1 / 7.5)

        private void Start()
        {
            // Tìm tất cả NPC trong scene
            npcs = FindObjectsByType<NPCCharacter>(FindObjectsInactive.Exclude);
            
            // Tự động tạo giao diện
            CreateProximityUI();
        }

        private void CreateProximityUI()
        {
            // Tìm Canvas chính trong cảnh
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindAnyObjectByType<Canvas>();
            }
            if (canvas == null) return;

            // Tạo GameObject cha cho panel
            panelObj = new GameObject("NPCProximityPanel");
            panelObj.transform.SetParent(canvas.transform, false);
            
            panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(280f, 130f); // Thu nhỏ kích thước lại (280px)
            panelRect.pivot = new Vector2(0.5f, 0f); // Trọng tâm ở đáy giữa panel để dễ định vị trên đầu NPC

            // Thêm CanvasGroup để xử lý fade in/out mượt mà
            canvasGroup = panelObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            // Thêm Image nền tối bán trong suốt (Phong cách mộc mạc cao cấp)
            Image bgImage = panelObj.AddComponent<Image>();
            bgImage.color = new Color(0.10f, 0.08f, 0.06f, 0.95f); // Tông nâu tối hơn, sang trọng hơn

            // Thêm outline khung gỗ/vàng nhạt tinh tế
            Outline outline = panelObj.AddComponent<Outline>();
            outline.effectColor = new Color(0.38f, 0.30f, 0.22f, 1f);
            outline.effectDistance = new Vector2(2f, 2f);

            // Thêm Vertical Layout Group để xếp chồng các lựa chọn
            VerticalLayoutGroup layout = panelObj.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 10, 10);
            layout.spacing = 6f; // Spacing 6px đồng bộ
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            // Tạo tiêu đề (Tên NPC)
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panelObj.transform, false);
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.fontSize = 13;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(0.98f, 0.85f, 0.35f, 1f); // Màu vàng ấm nổi bật
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.text = "NPC NAME";
            
            // Đặt kích thước cố định cho Text tiêu đề
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(260f, 20f);

            // Ẩn mặc định
            panelObj.SetActive(false);
        }

        private void Update()
        {
            if (PlayerController.Instance == null || panelObj == null) return;

            // Xử lý hiệu ứng làm mờ dần (Fade)
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
                
                // Trạng thái tương tác vật lý tùy thuộc vào alpha
                if (canvasGroup.alpha > 0.01f)
                {
                    if (!panelObj.activeSelf) panelObj.SetActive(true);
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
                else
                {
                    if (panelObj.activeSelf) panelObj.SetActive(false);
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
            }

            // Ẩn bảng lựa chọn nếu đang mở Hội thoại lớn hoặc Cửa hàng
            if (SurvivalUIManager.Instance != null && 
                (SurvivalUIManager.Instance.IsDialogueActive || SurvivalUIManager.Instance.IsShopOpen))
            {
                targetAlpha = 0f;
                activeNPC = null;
                return;
            }

            // Tìm NPC gần nhất
            NPCCharacter closestNPC = null;
            float minDistance = float.MaxValue;
            Vector3 playerPos = PlayerController.Instance.transform.position;

            foreach (var npc in npcs)
            {
                if (npc == null) continue;
                float dist = Vector3.Distance(playerPos, npc.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestNPC = npc;
                }
            }

            // Khoảng cách kích hoạt hiển thị proximity UI (1.7 mét)
            if (closestNPC != null && minDistance <= 1.7f)
            {
                if (activeNPC != closestNPC)
                {
                    if (activeNPC != null)
                    {
                        activeNPC.ReturnToDefaultRotation();
                    }
                    activeNPC = closestNPC;
                    ConfigureOptionsForNPC(activeNPC);
                }

                targetAlpha = 1f;

                // Cập nhật vị trí panel lơ lửng trên đầu NPC (được giới hạn màn hình)
                UpdatePanelPosition(activeNPC);

                // Lắng nghe phím số 1, 2, 3 để kích hoạt lựa chọn nhanh
                HandleKeyboardInput();

                // Loại bỏ trùng lặp: ẩn prompt tương tác [E] ở đáy màn hình nếu nó đang hiển thị trò chuyện NPC
                if (SurvivalUIManager.Instance != null && 
                    SurvivalUIManager.Instance.interactionPromptText != null &&
                    SurvivalUIManager.Instance.interactionPromptText.text.Contains("Trò chuyện với"))
                {
                    SurvivalUIManager.Instance.SetInteractionPrompt("");
                }
            }
            else
            {
                targetAlpha = 0f;
                if (activeNPC != null)
                {
                    activeNPC.ReturnToDefaultRotation();
                    activeNPC = null;
                }
            }
        }

        private void UpdatePanelPosition(NPCCharacter npc)
        {
            if (Camera.main == null) return;

            Transform visualTrans = npc.transform.Find("Visual");
            Vector3 basePos = visualTrans != null ? visualTrans.position : npc.transform.position;
            Vector3 worldPos = basePos + Vector3.up * 1.8f;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // Nếu đằng sau camera, ẩn đi
            if (screenPos.z < 0)
            {
                targetAlpha = 0f;
                return;
            }

            // Giới hạn (Clamp) tọa độ UI để không tràn ra ngoài các cạnh màn hình
            float halfWidth = panelRect.rect.width / 2f;
            float height = panelRect.rect.height;
            screenPos.x = Mathf.Clamp(screenPos.x, halfWidth + 10f, Screen.width - halfWidth - 10f);
            screenPos.y = Mathf.Clamp(screenPos.y, 10f, Screen.height - height - 10f);

            panelObj.transform.position = screenPos;
        }

        private void ConfigureOptionsForNPC(NPCCharacter npc)
        {
            titleText.text = npc.NPCName;
            
            // Clear các nút cũ
            foreach (var btn in optionButtons)
            {
                Destroy(btn);
            }
            optionButtons.Clear();
            currentOptions.Clear();

            GamePhase currentPhase = GameManager.Instance != null ? GameManager.Instance.CurrentPhase : GamePhase.LapNghiep;
            var stage = (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive) 
                ? TutorialManager.Instance.currentStage 
                : TutorialManager.TutorialStage.NotStarted;

            // Khi hướng dẫn đang kích hoạt, thiết lập các lựa chọn tương tác chuyên biệt cho từng Stage
            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                if (stage == TutorialManager.TutorialStage.IntroQuests ||
                    stage == TutorialManager.TutorialStage.FarmingTutorial ||
                    stage == TutorialManager.TutorialStage.CraftPreservedCrops ||
                    stage == TutorialManager.TutorialStage.PrepareOwnHouse ||
                    stage == TutorialManager.TutorialStage.ProtectFarmland ||
                    stage == TutorialManager.TutorialStage.WorshipAltar)
                {
                    currentOptions.Add(new ProximityOption 
                    { 
                        label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                        action = () => TriggerTalk(npc) 
                    });
                }
                else if (stage == TutorialManager.TutorialStage.TalkToOThamJob)
                {
                    if (npc.characterType == NPCCharacter.StoryCharacterType.OTham)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Hỏi O Thắm nhờ việc gì", 
                            action = () => TriggerOThamJobTalk(npc) 
                        });
                    }
                    else
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                            action = () => TriggerTalk(npc) 
                        });
                    }
                }
                else if (stage == TutorialManager.TutorialStage.SellCrops)
                {
                    if (npc.characterType == NPCCharacter.StoryCharacterType.OTham)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Bán khoai lang cho O Thắm", 
                            action = () => TriggerTrade(npc) 
                        });
                    }
                    else
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                            action = () => TriggerTalk(npc) 
                        });
                    }
                }
                else if (stage == TutorialManager.TutorialStage.TalkToBacNamPreserve)
                {
                    if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Hỏi Bác Năm cách bảo quan", 
                            action = () => TriggerBacNamPreserveTalk(npc) 
                        });
                    }
                    else
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                            action = () => TriggerTalk(npc) 
                        });
                    }
                }
                else if (stage == TutorialManager.TutorialStage.SharePreservedCrops)
                {
                    bool alreadyShared = false;
                    if (npc.characterType == NPCCharacter.StoryCharacterType.OTham) alreadyShared = TutorialManager.Instance.sharedOTham;
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam) alreadyShared = TutorialManager.Instance.sharedBacNam;
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.CuBay) alreadyShared = TutorialManager.Instance.sharedCuBay;
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BeTi) alreadyShared = TutorialManager.Instance.sharedBeTi;

                    if (!alreadyShared)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Tặng khoai gieo tự trồng", 
                            action = () => TriggerPreservedCropGift(npc) 
                        });
                    }
                    else
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                            action = () => TriggerTalk(npc) 
                        });
                    }
                }
                else if (stage == TutorialManager.TutorialStage.PrepareForStorm)
                {
                    if (npc.characterType == NPCCharacter.StoryCharacterType.OTham)
                    {
                        if (!TutorialManager.Instance.oThamJobAccepted)
                        {
                            currentOptions.Add(new ProximityOption 
                            { 
                                label = "[1] Nhận việc: Giúp O Thắm cất dọn đồ đạc", 
                                action = () => TriggerOThamPrep(npc) 
                            });
                        }
                        else if (!TutorialManager.Instance.oThamStoreDone)
                        {
                            if (TutorialManager.Instance.oThamCarryingCount > 0)
                            {
                                currentOptions.Add(new ProximityOption 
                                { 
                                    label = "[1] Hỏi chuyện O Thắm", 
                                    action = () => TriggerOThamReminder(npc) 
                                });
                            }
                            else
                            {
                                currentOptions.Add(new ProximityOption 
                                { 
                                    label = "[1] Lấy thêm đồ cần cất từ O Thắm", 
                                    action = () => TriggerOThamPrep(npc) 
                                });
                            }
                        }
                        else
                        {
                            currentOptions.Add(new ProximityOption 
                            { 
                                label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                                action = () => TriggerTalk(npc) 
                            });
                        }
                    }
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam)
                    {
                        bool bacNamAllDone = TutorialManager.Instance.bacNamSandbagsDone && TutorialManager.Instance.bacNamFloodBoardsDone;
                        if (!TutorialManager.Instance.bacNamJobAccepted)
                        {
                            currentOptions.Add(new ProximityOption 
                            { 
                                label = "[1] Nhận việc: Giúp Bác Năm gia cố nhà", 
                                action = () => TriggerBacNamPrep(npc) 
                            });
                        }
                        else if (!bacNamAllDone)
                        {
                            currentOptions.Add(new ProximityOption 
                            { 
                                label = "[1] Hỏi chuyện Bác Năm", 
                                action = () => TriggerBacNamReminder(npc) 
                            });
                        }
                        else
                        {
                            currentOptions.Add(new ProximityOption 
                            { 
                                label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                                action = () => TriggerTalk(npc) 
                            });
                        }
                    }
                    else
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                            action = () => TriggerTalk(npc) 
                        });
                    }
                }
                else if (stage == TutorialManager.TutorialStage.TalkToCuBayWorship)
                {
                    if (npc.characterType == NPCCharacter.StoryCharacterType.CuBay)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Hỏi chuyện Cụ Bảy", 
                            action = () => TriggerCuBayWorshipTalk(npc) 
                        });
                    }
                    else
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                            action = () => TriggerTalk(npc) 
                        });
                    }
                }
                else if (stage == TutorialManager.TutorialStage.RescuingNPCs)
                {
                    bool isRescued = false;
                    if (npc.characterType == NPCCharacter.StoryCharacterType.OTham) isRescued = TutorialManager.Instance.oThamRescued;
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam) isRescued = TutorialManager.Instance.bacNamRescued;
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.CuBay) isRescued = TutorialManager.Instance.cuBayRescued;
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BeTi) isRescued = TutorialManager.Instance.beTiRescued;

                    if (!isRescued)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[E] Cứu hộ lên mái nhà", 
                            action = () => TriggerNPCRescue(npc) 
                        });
                    }
                    else
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "Đã sơ tán lên nóc nhà an toàn", 
                            action = null 
                        });
                    }
                }
                else if (stage == TutorialManager.TutorialStage.RoofSurvivalSharing)
                {
                    bool isFed = false;
                    if (npc.characterType == NPCCharacter.StoryCharacterType.OTham) isFed = TutorialManager.Instance.oThamFed;
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam) isFed = TutorialManager.Instance.bacNamFed;
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.CuBay) isFed = TutorialManager.Instance.cuBayFed;
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BeTi) isFed = TutorialManager.Instance.beTiFed;

                    if (!isFed)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[E] Chia sẻ lương thực cứu trợ", 
                            action = () => TriggerNPCRoofFoodShare(npc) 
                        });
                    }
                    else
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "Đã ăn no ấm bụng", 
                            action = null 
                        });
                    }
                }
                else if (stage == TutorialManager.TutorialStage.PostStormCleanup)
                {
                    bool isCleaned = false;
                    if (npc.characterType == NPCCharacter.StoryCharacterType.OTham) isCleaned = TutorialManager.Instance.oThamHouseCleaned;
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam) isCleaned = TutorialManager.Instance.bacNamHouseCleaned;
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.CuBay || npc.characterType == NPCCharacter.StoryCharacterType.BeTi) isCleaned = TutorialManager.Instance.cuBayHouseCleaned;

                    if (!isCleaned)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[E] Phụ dọn dẹp đống đổ nát (-15 Thể lực)", 
                            action = () => TriggerNPCPostStormCleanup(npc) 
                        });
                    }
                    else
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "Đã dọn dẹp xong nhà cửa", 
                            action = null 
                        });
                    }
                }
            }
            else
            {
                // Xác định các lựa chọn có sẵn cho NPC dựa trên Giai đoạn cốt truyện

                if (npc.characterType == NPCCharacter.StoryCharacterType.OTham)
                {
                    if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive && TutorialManager.Instance.currentStage == TutorialManager.TutorialStage.TalkToOThamJob)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Hỏi O Thắm nhờ việc gì", 
                            action = () => TriggerOThamJobTalk(npc) 
                        });
                    }
                    else if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive && TutorialManager.Instance.currentStage == TutorialManager.TutorialStage.SellCrops)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Bán khoai lang cho O Thắm", 
                            action = () => TriggerTrade(npc) 
                        });
                    }
                    else
                    {
                        // Option 1: Trò chuyện
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                            action = () => TriggerTalk(npc) 
                        });

                        // Option 2: Giúp việc
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[2] Giúp việc (+1 Vần công, +10 NT)", 
                            action = () => TriggerWork(npc) 
                        });

                        // Option 3: Giao dịch hoặc Cửa hàng
                        bool recoveryEventActive = GameManager.Instance != null && 
                                                currentPhase == GamePhase.PhuSa &&
                                                CommunityManager.Instance != null &&
                                                !CommunityManager.Instance.eventVillageRecoveryCompleted;

                        if (recoveryEventActive)
                        {
                            currentOptions.Add(new ProximityOption 
                            { 
                                label = "[3] Tái thiết ruộng (Sự kiện +20 NT)", 
                                action = () => TriggerRecoveryEvent(npc) 
                            });
                        }
                        else
                        {
                            currentOptions.Add(new ProximityOption 
                            { 
                                label = "[3] Giao dịch / Cửa hàng", 
                                action = () => TriggerTrade(npc) 
                            });
                        }

                        // Option 4: Đóng góp khoai/lương thực
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[4] Đóng góp khoai/lương thực (+5 NT)", 
                            action = () => TriggerOThamDonation(npc) 
                        });
                    }
                }
                else if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam)
                {
                    // Option 1: Trò chuyện
                    currentOptions.Add(new ProximityOption 
                    { 
                        label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                        action = () => TriggerTalk(npc) 
                    });

                    // Option 2: Giúp việc hoặc Sự kiện Mưa Bão
                    bool bacNamEventActive = GameManager.Instance != null && 
                                           currentPhase == GamePhase.MuaBao &&
                                           CommunityManager.Instance != null &&
                                           !CommunityManager.Instance.eventBacNamStormCompleted;

                    if (bacNamEventActive)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[2] Chằng chống nhà (Sự kiện +15 NT)", 
                            action = () => TriggerBacNamEvent(npc) 
                        });
                    }
                    else
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[2] Vần công gia cố nhà (+1 Vần công, +10 NT)", 
                            action = () => TriggerWork(npc) 
                        });
                    }

                    // Option 3: Chia sẻ lương thực hoặc Sự kiện Tái Thiết
                    bool recoveryEventActive = GameManager.Instance != null && 
                                            currentPhase == GamePhase.PhuSa &&
                                            CommunityManager.Instance != null &&
                                            !CommunityManager.Instance.eventVillageRecoveryCompleted;

                    if (recoveryEventActive)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[3] Tái thiết ruộng (Sự kiện +20 NT)", 
                            action = () => TriggerRecoveryEvent(npc) 
                        });
                    }
                    else
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[3] Chia sẻ lương thực (+15 NT)", 
                            action = () => TriggerShareFood(npc) 
                        });
                    }
                }
                else if (npc.characterType == NPCCharacter.StoryCharacterType.CuBay)
                {
                    // Option 1: Trò chuyện
                    currentOptions.Add(new ProximityOption 
                    { 
                        label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                        action = () => TriggerTalk(npc) 
                    });

                    // Option 2: Cứu trợ lương thực cho Cụ Bảy (chỉ xuất hiện trong Phase Mưa Bão)
                    if (currentPhase == GamePhase.MuaBao)
                    {
                        currentOptions.Add(new ProximityOption 
                        { 
                            label = "[2] Cứu trợ lương thực (+20 NT)", 
                            action = () => TriggerCuBayRescueEvent(npc) 
                        });
                    }
                }
                else if (npc.characterType == NPCCharacter.StoryCharacterType.BeTi)
                {
                    // Option 1: Trò chuyện
                    currentOptions.Add(new ProximityOption 
                    { 
                        label = "[1] Trò chuyện (+5 Nghĩa Tình)", 
                        action = () => TriggerTalk(npc) 
                    });
                }
            }



            // Tạo các Button UI đồng bộ kích thước chuẩn (Chiều rộng 360px, Chiều cao 36px)
            float totalHeight = 39f; // Tiêu đề + padding ban đầu
            for (int i = 0; i < currentOptions.Count; i++)
            {
                int index = i;
                GameObject btnObj = new GameObject($"OptionButton_{i}");
                btnObj.transform.SetParent(panelObj.transform, false);
                
                RectTransform btnRect = btnObj.AddComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(260f, 30f); // Size cố định đồng bộ tất cả các nút

                Image btnImage = btnObj.AddComponent<Image>();
                btnImage.color = new Color(0.24f, 0.20f, 0.16f, 1f); // Nâu đất vừa

                Button btn = btnObj.AddComponent<Button>();
                if (currentOptions[index].action == null)
                {
                    btn.interactable = false;
                }
                btn.onClick.AddListener(() => {
                    if (PlayerController.Instance != null)
                    {
                        npc.LookAtPlayer(PlayerController.Instance.transform);
                    }
                    if (currentOptions[index].action != null)
                    {
                        currentOptions[index].action();
                    }
                });

                // Thêm hiệu ứng màu nút khi di chuột
                ColorBlock colors = btn.colors;
                colors.normalColor = new Color(0.24f, 0.20f, 0.16f, 1f);
                colors.highlightedColor = new Color(0.35f, 0.30f, 0.25f, 1f);
                colors.pressedColor = new Color(0.15f, 0.12f, 0.10f, 1f);
                btn.colors = colors;

                // Thêm viền nhỏ cho tinh tế sang trọng
                Outline btnOutline = btnObj.AddComponent<Outline>();
                btnOutline.effectColor = new Color(0.42f, 0.34f, 0.26f, 0.8f);
                btnOutline.effectDistance = new Vector2(1f, 1f);

                // Thêm Text của nút (căn giữa hoàn hảo)
                GameObject txtObj = new GameObject("Text");
                txtObj.transform.SetParent(btnObj.transform, false);
                
                TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
                txt.fontSize = 11.5f;
                txt.color = Color.white;
                txt.alignment = TextAlignmentOptions.Center;
                txt.text = currentOptions[i].label;

                RectTransform txtRect = txtObj.GetComponent<RectTransform>();
                txtRect.anchorMin = Vector2.zero;
                txtRect.anchorMax = Vector2.one;
                txtRect.offsetMin = Vector2.zero;
                txtRect.offsetMax = Vector2.zero;

                optionButtons.Add(btnObj);
                totalHeight += 36f; // 30px height + 6px spacing
            }

            // Điều chỉnh chiều cao panel theo số lượng nút
            panelRect.sizeDelta = new Vector2(280f, totalHeight);
        }

        private void HandleKeyboardInput()
        {
            if (Keyboard.current == null || activeNPC == null) return;

            bool pressed1 = Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame;
            bool pressed2 = Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame;
            bool pressed3 = Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame;
            bool pressed4 = Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame;
            bool pressedE = Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;

            if (pressed1 || pressed2 || pressed3 || pressed4 || pressedE)
            {
                if (PlayerController.Instance != null)
                {
                    activeNPC.LookAtPlayer(PlayerController.Instance.transform);
                }
            }

            if (pressedE)
            {
                int eOptionIndex = currentOptions.FindIndex(o => o.label.StartsWith("[E]", System.StringComparison.OrdinalIgnoreCase));
                if (eOptionIndex != -1)
                {
                    if (currentOptions[eOptionIndex].action != null)
                    {
                        currentOptions[eOptionIndex].action();
                        return;
                    }
                }
            }
            else if (currentOptions.Count >= 1 && pressed1)
            {
                if (currentOptions[0].action != null) currentOptions[0].action();
            }
            else if (currentOptions.Count >= 2 && pressed2)
            {
                if (currentOptions[1].action != null) currentOptions[1].action();
            }
            else if (currentOptions.Count >= 3 && pressed3)
            {
                if (currentOptions[2].action != null) currentOptions[2].action();
            }
            else if (currentOptions.Count >= 4 && pressed4)
            {
                if (currentOptions[3].action != null) currentOptions[3].action();
            }
        }

        // --- CÁC HÀM XỬ LÝ LỰA CHỌN TƯƠNG TÁC (TÁI SỬ DỤNG LOGIC CHUẨN) ---



        private void TriggerTalk(NPCCharacter npc)
        {
            string dialogue = npc.GetDialogue();
            SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, dialogue);
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.RegisterTalkStart(npc.NPCName);
            }
            PlayerStats.Instance?.ModifyMorale(2f);
            npc.ModifyAffection(1);
            if (npc.CanReceiveTalkReward())
            {
                CommunityManager.Instance?.ModifyGlobalKarma(5);
                string msg = "";
                if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam)
                {
                    msg = "Bác Năm động viên bạn bám đất giữ làng. +5 Nghĩa Tình";
                }
                else if (npc.characterType == NPCCharacter.StoryCharacterType.OTham)
                {
                    msg = "O Thắm quý tấm lòng của bạn. +5 Nghĩa Tình";
                }
                else if (npc.characterType == NPCCharacter.StoryCharacterType.CuBay)
                {
                    msg = "Cụ Bảy chia sẻ kinh nghiệm dân gian bổ ích. +5 Nghĩa Tình";
                }
                else if (npc.characterType == NPCCharacter.StoryCharacterType.BeTi)
                {
                    msg = "Bé Tí cười hồn nhiên khi thấy bạn. +5 Nghĩa Tình";
                }
                else
                {
                    msg = npc.NPCName + " quý tấm lòng của bạn. +5 Nghĩa Tình";
                }
                SurvivalUIManager.Instance?.ShowHUDToast(msg);
                npc.MarkTalkedToday();
            }
            targetAlpha = 0f; // Ẩn ngay khi mở dialogue toàn màn hình
        }

        private void TriggerCuBayRescueEvent(NPCCharacter npc)
        {
            if (StorageManager.Instance == null) return;
            var slots = StorageManager.Instance.GetStorageSlots();
            
            int freshCount = 0;
            int preservedCount = 0;
            int noodlesCount = 0;
            
            InventorySlot freshSlot = slots.Find(s => s.item != null && s.item.ItemID == "item_fresh_crop");
            InventorySlot preservedSlot = slots.Find(s => s.item != null && s.item.ItemID == "item_khoai_gieo");
            InventorySlot noodlesSlot = slots.Find(s => s.item != null && s.item.ItemID == "item_mi_tom");
            
            if (freshSlot != null) freshCount = freshSlot.quantity;
            if (preservedSlot != null) preservedCount = preservedSlot.quantity;
            if (noodlesSlot != null) noodlesCount = noodlesSlot.quantity;
            
            int totalFood = freshCount + preservedCount + noodlesCount;
            
            if (totalFood >= 5)
            {
                int needed = 5;
                if (needed > 0 && noodlesSlot != null && noodlesCount > 0)
                {
                    int take = Mathf.Min(needed, noodlesCount);
                    StorageManager.Instance.RemoveItem(noodlesSlot.item, take);
                    needed -= take;
                }
                if (needed > 0 && preservedSlot != null && preservedCount > 0)
                {
                    int take = Mathf.Min(needed, preservedCount);
                    StorageManager.Instance.RemoveItem(preservedSlot.item, take);
                    needed -= take;
                }
                if (needed > 0 && freshSlot != null && freshCount > 0)
                {
                    int take = Mathf.Min(needed, freshCount);
                    StorageManager.Instance.RemoveItem(freshSlot.item, take);
                    needed -= take;
                }
                
                CommunityManager.Instance?.ModifyGlobalKarma(20);
                npc.ModifyAffection(20); // Reward affection as well
                SurvivalUIManager.Instance?.ShowHUDToast("Bạn mang lương thực cứu trợ cho Cụ Bảy qua mùa lũ. +20 Nghĩa Tình!");
                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"Cảm ơn tấm lòng của con! Lương thực này quý giá vô cùng trong những ngày bão lụt tăm tối này.\"");
            }
            else
            {
                SurvivalUIManager.Instance?.ShowHUDToast("<color=#E74C3C>Không đủ 5 phần lương thực để cứu trợ!</color>");
                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"Con cứ giữ lấy mà ăn phòng thân, cụ già rồi, ăn uống đáng bao nhiêu đâu con.\"");
            }
            targetAlpha = 0f;
        }

        private void TriggerOThamJobTalk(NPCCharacter npc)
        {
            targetAlpha = 0f;
            SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"O có chút quà tặng cho con làm vốn trồng trọt nè, cầm lấy và chăm chỉ nha con\"");
            
            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                TutorialManager.Instance.StartFarmingSlideshow();
            }
        }

        private void TriggerBacNamPreserveTalk(NPCCharacter npc)
        {
            targetAlpha = 0f;
            SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"Muốn tích cốc phòng cơ chống mưa lụt thì phải biết làm khoai gieo con ơi. Con đi lại góc bếp lửa trước nhà mình chế biến 4 củ khoai gieo đi.\"");
            
            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                TutorialManager.Instance.OnBacNamPreserveTalked();
            }
        }

        private void TriggerPreservedCropGift(NPCCharacter npc)
        {
            if (StorageManager.Instance == null) return;

            var slots = StorageManager.Instance.GetStorageSlots();
            var preservedSlot = slots.Find(s => s.item != null && (s.item.ItemID == "item_khoai_gieo" || s.item.ItemName.Contains("Khoai Gieo") || s.item.name.Contains("PreservedCrop")));
            int preservedCount = preservedSlot != null ? preservedSlot.quantity : 0;

            if (preservedCount >= 1)
            {
                if (StorageManager.Instance.RemoveItem(preservedSlot.item, 1))
                {
                    npc.ModifyAffection(15);
                    CommunityManager.Instance?.ModifyGlobalKarma(10);
                    
                    string dialog = "";
                    if (npc.characterType == NPCCharacter.StoryCharacterType.OTham)
                        dialog = "\"Cảm ơn Thành nghe, khoai gieo dẻo ngọt lắm con, o Thắm quý con lắm!\"";
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam)
                        dialog = "\"Khoai gieo ngọt thơm bùi lắm con ơi, tích cốc phòng cơ như thế này là tốt lắm!\"";
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.CuBay)
                        dialog = "\"Cảm ơn tấm lòng của cháu, người già như cụ quý nhất những món quà mộc mạc thế này.\"";
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BeTi)
                        dialog = "\"Oa, khoai gieo chú Thành tặng ngọt quá, con cảm ơn chú Thành nha!\"";

                    SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, dialog);

                    if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                    {
                        TutorialManager.Instance.RegisterTalkStart(npc.NPCName);
                        TutorialManager.Instance.OnPreservedCropShared(npc.characterType);
                    }
                }
            }
            else
            {
                SurvivalUIManager.Instance?.ShowHUDToast("<color=#E74C3C>Không đủ Khoai gieo trong kho để tặng!</color>");
            }
            targetAlpha = 0f;
        }

        private void TriggerOThamPrep(NPCCharacter npc)
        {
            targetAlpha = 0f;
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.StartOThamJob();
            }
        }

        private void TriggerBacNamPrep(NPCCharacter npc)
        {
            targetAlpha = 0f;
            SurvivalUIManager.Instance?.ShowDialogue(
                npc.NPCName, 
                "\"Thành ơi, bão sắp đổ bộ rồi! Bác cần con giúp 2 việc: Đặt 4 bao cát quanh nền nhà để chống lũ rồi dựng thêm 2 tấm chắn lũ ở cửa trước để nước khỏi tràn vào nhà!\""
            );
            if (StorageManager.Instance != null && SurvivalUIManager.Instance != null)
            {
                ItemData sandbag = SurvivalUIManager.Instance.SandbagItem;
                if (sandbag != null)
                {
                    StorageManager.Instance.AddItem(sandbag, 4);
                }
                ItemData floodBoard = SurvivalUIManager.Instance.FloodBoardItem;
                if (floodBoard != null)
                {
                    StorageManager.Instance.AddItem(floodBoard, 2);
                }
                SurvivalUIManager.Instance.ShowHUDToast("Nhận 4 Bao cát + 2 Tấm chắn lũ. Bấm số [2]/[4] trên Hotbar, click chuột để đặt!");
            }
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.StartBacNamJob(); // calls StartBothStormJobs internally
            }
        }

        private void TriggerOThamReminder(NPCCharacter npc)
        {
            targetAlpha = 0f;
            int count = TutorialManager.Instance != null ? TutorialManager.Instance.oThamCarryingCount : 0;
            SurvivalUIManager.Instance?.ShowDialogue(
                npc.NPCName, 
                $"\"Con đang giúp o đó mà! Đem cất {count} gói Mì tôm o vừa đưa vào rương gỗ trước tiệm giúp o đi nhé!\""
            );
        }

        private void TriggerBacNamReminder(NPCCharacter npc)
        {
            targetAlpha = 0f;
            SurvivalUIManager.Instance?.ShowDialogue(
                npc.NPCName, 
                "\"Cố lên Thành ơi! Đặt nốt 4 bao cát và 2 tấm chắn lũ rồi nhà bác an toàn trước bão!\""
            );
        }

        private void TriggerCuBayWorshipTalk(NPCCharacter npc)
        {
            targetAlpha = 0f;
            SurvivalUIManager.Instance?.ShowDialogue(
                npc.NPCName, 
                "\"Này Thành, cháu mới quay về làng, hãy đi thắp nhang thờ cúng chốn quê này ở bàn thờ gia tiên trước nhà nhé. Uống nước nhớ nguồn, đây là nén nhang cụ tặng cháu để thắp cúng lễ.\""
            );
            
            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
            {
                if (StorageManager.Instance != null)
                {
                    ItemData incense = SurvivalUIManager.Instance != null ? SurvivalUIManager.Instance.IncenseItem : null;
                    if (incense == null)
                    {
                        var altar = FindAnyObjectByType<AncestralAltar>();
                        if (altar != null)
                        {
                            incense = altar.IncenseItem;
                        }
                    }
                    if (incense == null)
                    {
                        incense = Resources.Load<ItemData>("Data/Item_Incense");
                    }
                    if (incense == null)
                    {
                        incense = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Incense.asset");
                    }
                    if (incense != null)
                    {
                        StorageManager.Instance.AddItem(incense, 1);
                        SurvivalUIManager.Instance?.ShowHUDToast("Bạn nhận được 1 Nén Nhang từ Cụ Bảy");
                    }
                }
                
                TutorialManager.Instance.OnCuBayWorshipTalked();
            }
        }

        private void TriggerWork(NPCCharacter npc)
        {
            float currentStamina = PlayerStats.Instance != null ? PlayerStats.Instance.CurrentStamina : 0f;
            if (currentStamina >= 20f)
            {
                PlayerStats.Instance.ModifyStamina(-20f);
                npc.ModifyVanCongCredits(1);
                npc.ModifyAffection(5);
                CommunityManager.Instance?.ModifyGlobalKarma(10);
                
                string toast = npc.characterType == NPCCharacter.StoryCharacterType.BacNam ?
                    "Bạn giúp Bác Năm sửa lại việc nhà. +1 Vần công, +10 Nghĩa Tình" :
                    "Bạn giúp O Thắm một việc. +1 Vần công, +10 Nghĩa Tình";
                SurvivalUIManager.Instance?.ShowHUDToast(toast);
                
                string workDialog = npc.GetWorkDialogue(true);
                SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, workDialog);
            }
            else
            {
                string workDialog = npc.GetWorkDialogue(false);
                SurvivalUIManager.Instance.ShowDialogue(npc.NPCName, workDialog);
            }
            targetAlpha = 0f;
        }

        private void TriggerOThamEvent(NPCCharacter npc)
        {
            var slots = StorageManager.Instance.GetStorageSlots();
            var foodSlot = slots.Find(s => (s.item.ItemID == "item_fresh_crop" || s.item.ItemID == "item_khoai_gieo" || s.item.ItemID == "item_mi_tom") && s.quantity >= 2);
            if (foodSlot != null)
            {
                StorageManager.Instance.RemoveItem(foodSlot.item, 2);
                CommunityManager.Instance.eventOThamFoodCompleted = true;
                CommunityManager.Instance.ModifyGlobalKarma(10);
                SurvivalUIManager.Instance?.ShowHUDToast("Bạn hỗ trợ O Thắm trong mùa Gió Lào. +10 Nghĩa Tình");
                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"Cảm ơn con nhiều nha Thành! Số lương thực này o sẽ dùng để san sẻ cho bà con lân cận.\"");
            }
            else
            {
                SurvivalUIManager.Instance?.ShowHUDToast("Bạn không có đủ 2 lương thực để hỗ trợ.");
                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"Không sao đâu con, ráng giữ sức bám đất vượt qua đợt khô hạn này nghe.\"");
            }
            targetAlpha = 0f;
        }

        private void TriggerOThamDonation(NPCCharacter npc)
        {
            if (StorageManager.Instance == null || PlayerStats.Instance == null) return;

            var slots = StorageManager.Instance.GetStorageSlots();
            var freshSlot = slots.Find(s => s.item != null && s.item.ItemID == "item_fresh_crop" && s.quantity >= 1);

            if (freshSlot != null)
            {
                if (StorageManager.Instance.RemoveItem(freshSlot.item, 1))
                {
                    CommunityManager.Instance?.ModifyGlobalKarma(5);
                    npc.ModifyAffection(2);
                    SurvivalUIManager.Instance?.ShowHUDToast("Đóng góp thành công! Trừ 1 Khoai tươi. Nhận +5 Nghĩa Tình.");
                    SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"O cảm ơn tấm lòng nghĩa tình của con nha Thành! Làng mình rất cần những người như con vụ này.\"");
                }
            }
            else if (PlayerStats.Instance.Coins >= 20)
            {
                PlayerStats.Instance.ModifyCoins(-20);
                CommunityManager.Instance?.ModifyGlobalKarma(5);
                npc.ModifyAffection(2);
                SurvivalUIManager.Instance?.ShowHUDToast("Đóng góp thành công! Trừ 20 Xu. Nhận +5 Nghĩa Tình.");
                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"O cảm ơn tấm lòng nghĩa tình của con nha Thành! Số tiền này o sẽ gom góp mua thêm ván đê chống lụt.\"");
            }
            else
            {
                SurvivalUIManager.Instance?.ShowHUDToast("<color=#E74C3C>Bạn không có đủ 1 củ Khoai tươi hoặc 20 xu để quyên góp!</color>");
                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"Không sao đâu con, giữ sức ăn học phòng thân đi con.\"");
            }
            targetAlpha = 0f;
        }

        private void TriggerBacNamEvent(NPCCharacter npc)
        {
            if (npc.VanCongCredits >= 1)
            {
                npc.ModifyVanCongCredits(-1);
                CommunityManager.Instance.eventBacNamStormCompleted = true;
                CommunityManager.Instance.ModifyGlobalKarma(15);
                SurvivalUIManager.Instance?.ShowHUDToast("Bạn cùng bà con chằng chống nhà Bác Năm. +15 Nghĩa Tình");
                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"Cảm ơn con và bà con chòm xóm! Nhà cửa bác vững vàng rồi, không lo gió lốc cuốn bay mái lá nữa!\"");

                // Visual feedback: activate pre-placed sandbag cluster on BacNam's house
                GameObject bacNamHouse = GameObject.Find("BacNam_House");
                if (bacNamHouse != null)
                {
                    Transform sandbagCluster = bacNamHouse.transform.Find("Visual_Sandbags_Event");
                    if (sandbagCluster != null)
                    {
                        sandbagCluster.gameObject.SetActive(true);
                        Debug.Log("[EVENT] Visual_Sandbags_Event activated on BacNam_House.");
                    }
                }
            }
            else
            {
                SurvivalUIManager.Instance?.ShowHUDToast("Bạn không có đủ 1 Vần công để chằng chống nhà.");
                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"Không sao đâu con, gió to lắm, lo giữ lấy an toàn cho bản thân trước nghe!\"");
            }
            targetAlpha = 0f;
        }

        private void TriggerRecoveryEvent(NPCCharacter npc)
        {
            var slots = StorageManager.Instance.GetStorageSlots();
            var seedSlot = slots.Find(s => s.item.ItemID == "item_seed_khoai" && s.quantity >= 2);
            if (seedSlot != null)
            {
                StorageManager.Instance.RemoveItem(seedSlot.item, 2);
                CommunityManager.Instance.eventVillageRecoveryCompleted = true;
                CommunityManager.Instance.ModifyGlobalKarma(20);
                SurvivalUIManager.Instance?.ShowHUDToast("Bạn góp sức tái thiết ruộng làng sau lũ. +20 Nghĩa Tình");
                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, npc.characterType == NPCCharacter.StoryCharacterType.BacNam ? 
                    "\"Tốt quá con ơi! Bác sẽ mang số hạt giống này để nhân giống cấy vụ chiêm sắp tới!\"" : 
                    "\"Tốt quá con ơi! O sẽ mang số hạt giống này phân phát cho bà con gieo cấy vụ mới!\"");
            }
            else
            {
                var foodSlot = slots.Find(s => (s.item.ItemID == "item_fresh_crop" || s.item.ItemID == "item_khoai_gieo" || s.item.ItemID == "item_mi_tom") && s.quantity >= 2);
                if (foodSlot != null)
                {
                    StorageManager.Instance.RemoveItem(foodSlot.item, 2);
                    CommunityManager.Instance.eventVillageRecoveryCompleted = true;
                    CommunityManager.Instance.ModifyGlobalKarma(20);
                    SurvivalUIManager.Instance?.ShowHUDToast("Bạn góp sức tái thiết ruộng làng sau lũ. +20 Nghĩa Tình");
                    SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, npc.characterType == NPCCharacter.StoryCharacterType.BacNam ? 
                        "\"Tốt quá con ơi! Có thêm chút lương thực này bác chia bớt cho mấy đứa nhỏ trong xóm!\"" : 
                        "\"Tốt quá con ơi! O sẽ đem chia phần lương thực này cho nhà cụ Năm cơm cháo chống đói!\"");
                }
                else
                {
                    SurvivalUIManager.Instance?.ShowHUDToast("Bạn không có đủ 2 hạt giống hoặc 2 lương thực để đóng góp.");
                    SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, npc.characterType == NPCCharacter.StoryCharacterType.BacNam ? 
                        "\"Không có gì đâu con, vụ lụt cuốn trôi hết cả rồi, ai cũng khó khăn như nhau mà!\"" : 
                        "\"Chưa có đồ đóng góp cũng không sao đâu con, giữ gìn sức khỏe là chính!\"");
                }
            }
            targetAlpha = 0f;
        }

        private void TriggerTrade(NPCCharacter npc)
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.OpenTradeMenu(npc);
            }
            targetAlpha = 0f;
        }

        private void TriggerShareFood(NPCCharacter npc)
        {
            var slots = StorageManager.Instance.GetStorageSlots();
            InventorySlot foodSlot = null;
            int amountToConsume = 0;
            
            foodSlot = slots.Find(s => s.item.ItemID == "item_fresh_crop" && s.quantity >= 5);
            if (foodSlot != null)
            {
                amountToConsume = 5;
            }
            else
            {
                foodSlot = slots.Find(s => s.item.ItemID == "item_khoai_gieo" && s.quantity >= 2);
                if (foodSlot != null)
                {
                    amountToConsume = 2;
                }
                else
                {
                    foodSlot = slots.Find(s => s.item.ItemID == "item_mi_tom" && s.quantity >= 2);
                    if (foodSlot != null)
                    {
                        amountToConsume = 2;
                    }
                }
            }
            
            if (foodSlot != null)
            {
                StorageManager.Instance.RemoveItem(foodSlot.item, amountToConsume);
                CommunityManager.Instance?.ModifyGlobalKarma(15);
                SurvivalUIManager.Instance?.ShowHUDToast("Bạn chia sẻ lương thực cho Bác Năm. +15 Nghĩa Tình");
                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"Ôi quý hóa quá! Cảm ơn con đã chia sẻ lương thực ấm áp nghĩa tình này nha Thành!\"");
            }
            else
            {
                SurvivalUIManager.Instance?.ShowHUDToast("Bạn chưa có đủ lương thực để chia sẻ.");
                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, "\"Không sao đâu con, giữ ăn để sinh tồn qua mùa mưa bão con nhé!\"");
            }
            targetAlpha = 0f;
        }

        private void TriggerNPCRescue(NPCCharacter npc)
        {
            if (TutorialManager.Instance == null) return;
            TutorialManager.Instance.OnNPCRescued(npc.characterType);
            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_place_object");
            targetAlpha = 0f;
            if (activeNPC != null)
            {
                activeNPC.ReturnToDefaultRotation();
                activeNPC = null;
            }
        }

        private void TriggerNPCRoofFoodShare(NPCCharacter npc)
        {
            if (StorageManager.Instance == null || PlayerStats.Instance == null) return;

            // Tìm khoai gieo khô
            var slotKhoaiGieo = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == "item_khoai_gieo");
            // Tìm mì tôm
            var slotMiTom = StorageManager.Instance.GetStorageSlots().Find(s => s.item != null && s.item.ItemID == "item_mi_tom");

            InventorySlot slotToUse = null;
            if (slotKhoaiGieo != null && slotKhoaiGieo.quantity > 0)
            {
                slotToUse = slotKhoaiGieo;
            }
            else if (slotMiTom != null && slotMiTom.quantity > 0)
            {
                slotToUse = slotMiTom;
            }

            if (slotToUse != null)
            {
                ItemData itemToUse = slotToUse.item;
                if (StorageManager.Instance.RemoveItem(itemToUse, 1))
                {
                    CommunityManager.Instance?.ModifyGlobalKarma(15);
                    npc.ModifyAffection(15);

                    if (TutorialManager.Instance != null)
                    {
                        TutorialManager.Instance.FeedNPC(npc.characterType);
                    }

                    SurvivalUIManager.Instance?.ShowHUDToast($"🧡 Đã chia sẻ {itemToUse.ItemName} cho {npc.NPCName}! (+15 Nghĩa Tình)");
                    
                    string dialogue = "";
                    if (npc.characterType == NPCCharacter.StoryCharacterType.OTham)
                        dialogue = $"\"Cảm ơn con nghe Thành! Ăn {itemToUse.ItemName} ấm lòng hẳn. O Thắm sẽ không quên ơn cứu mạng này!\"";
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam)
                        dialogue = $"\"Nghĩa tình xóm giềng hoạn nạn có nhau quý lắm con. Bác Năm cảm ơn con nhiều!\"";
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.CuBay)
                        dialogue = $"\"Tấm lòng của con bồi đắp nghĩa tình làng xóm. Cầu mong gia tiên phù hộ con tai qua nạn khỏi.\"";
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BeTi)
                        dialogue = $"\"Ngon quá chú Thành ơi! Con hết đói bụng rồi. Con cảm ơn chú Thành nhiều chú nha!\"";

                    SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, dialogue);
                }
            }
            else
            {
                SurvivalUIManager.Instance?.ShowHUDToast("<color=#E74C3C>Bạn không còn củ Khoai Gieo hay gói Mì Tôm nào trong balo để chia sẻ!</color>");
            }
            targetAlpha = 0f;
            if (activeNPC != null)
            {
                activeNPC.ReturnToDefaultRotation();
                activeNPC = null;
            }
        }

        private void TriggerNPCPostStormCleanup(NPCCharacter npc)
        {
            if (PlayerStats.Instance == null || TutorialManager.Instance == null) return;

            if (PlayerStats.Instance.CurrentStamina >= 15f)
            {
                PlayerStats.Instance.ModifyStamina(-15f);
                CommunityManager.Instance?.ModifyGlobalKarma(10);
                npc.ModifyAffection(10);

                SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_wood_hit");

                TutorialManager.Instance.CleanHouse(npc.characterType);
                SurvivalUIManager.Instance?.ShowHUDToast($"✓ Đã giúp nhà {npc.NPCName} dọn dẹp đống đổ nát! (-15 Thể lực, +10 Nghĩa Tình)");
                
                string dialogue = "";
                if (npc.characterType == NPCCharacter.StoryCharacterType.OTham)
                    dialogue = "\"Thành chu đáo quá, dọn hộ o đống gỗ sập này đỡ biết bao nhiêu mồ hôi công sức!\"";
                else if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam)
                    dialogue = "\"Cảm ơn con nghe Thành! Lũ rút để lại bao nhiêu là bùn đất gạch ngói vỡ, có con dọn giùm bác già mừng lắm.\"";
                else if (npc.characterType == NPCCharacter.StoryCharacterType.CuBay || npc.characterType == NPCCharacter.StoryCharacterType.BeTi)
                    dialogue = "\"Tốt quá con ơi, đống bùn non trước ngõ trơn trượt được con dọn đi cụ với bé Tí đi lại an toàn rồi.\"";

                SurvivalUIManager.Instance?.ShowDialogue(npc.NPCName, dialogue);
            }
            else
            {
                SurvivalUIManager.Instance?.ShowHUDToast("<color=#E74C3C>Bạn không đủ 15 Thể lực để giúp dọn dẹp nặng nhọc!</color>");
            }
            targetAlpha = 0f;
            if (activeNPC != null)
            {
                activeNPC.ReturnToDefaultRotation();
                activeNPC = null;
            }
        }
    }
}
