using System;
using System.Collections.Generic;
using UnityEngine;
using SownInStone.Core;

namespace SownInStone.UI
{
    public enum TutorialState
    {
        IntroQuests,          // Giai đoạn 1: Bắt buộc thăm hỏi đủ 4 NPC
        ShowingFarmingSlides, // Giai đoạn trung gian: Xem slideshow hướng dẫn làm nông
        FarmingTutorial,      // Giai đoạn 2: Thực hành dọn đá, gieo hạt, tưới nước
        Completed             // Hoàn thành hoàn toàn hệ thống Tutorial
    }

    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("Tutorial State")]
        [SerializeField] private TutorialState currentState = TutorialState.IntroQuests;

        [Header("NPC Dialogue Tracking (Phase 1 Gate)")]
        // Từ điển theo dõi cờ trạng thái đàm thoại xong với 4 NPC
        private Dictionary<string, bool> npcTalkRegistry = new Dictionary<string, bool>()
        {
            { "OTham", false },
            { "BacNam", false },
            { "CuBay", false },
            { "BeTi", false }
        };

        [Header("Farming Tasks Tracking")]
        public bool subTask1Completed = false; // Dọn đá
        public bool subTask2Completed = false; // Gieo hạt
        public bool subTask3Completed = false; // Tưới nước

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
            // Đăng ký nhận sự kiện khi một hộp thoại hội thoại lớn đóng lại
            // Giả định SurvivalUIManager hoặc PlayerController phát sự kiện này
            // Trình tự: OnDialogueClosed(string npcName)
        }

        /// <summary>
        /// Hàm xử lý sự kiện: Được gọi ngay khi hộp thoại hội thoại với 1 NPC đóng lại thành công
        /// </summary>
        public void RegisterTalkComplete(string npcName)
        {
            if (currentState != TutorialState.IntroQuests) return;

            // Kiểm tra và tick hoàn thành cho NPC tương ứng dựa trên tên định danh
            if (npcTalkRegistry.ContainsKey(npcName))
            {
                npcTalkRegistry[npcName] = true;
                Debug.Log($"[Tutorial] Đã hoàn thành đàm thoại bắt buộc với: {npcName}");

                // Cập nhật lại giao diện checklist nhiệm vụ hiển thị trên HUD
                RefreshTutorialHUDCheckbox();

                // Kiểm tra xem đã hoàn thành việc nói chuyện với toàn bộ 4 NPC chưa
                if (CheckAllNPCsVisited())
                {
                    TransitionToSlideshow();
                }
            }
        }

        private bool CheckAllNPCsVisited()
        {
            foreach (var kvp in npcTalkRegistry)
            {
                if (kvp.Value == false) return false; // Còn sót NPC chưa nói chuyện xong
            }
            return true;
        }

        private void TransitionToSlideshow()
        {
            currentState = TutorialState.ShowingFarmingSlides;
            Debug.Log("[Tutorial] Xuất sắc! Đã gặp đủ 4 dân làng. Tạm dừng thời gian để mở Slideshow hướng dẫn.");

            Time.timeScale = 0f; // Đóng băng toàn cục để người chơi đọc slide an toàn

            if (SurvivalUIManager.Instance != null)
            {
                // Gọi giao diện panel slideshow (chứa 4 hình ảnh PNG thực tế trong Resources)
                SurvivalUIManager.Instance.ShowFarmingTutorialSlideshow(() => {
                    // Action Callback khi người chơi bấm "Bắt đầu làm nông" hoặc "Bỏ qua" ở slide cuối
                    Time.timeScale = 1f;
                    StartFarmingTutorialStage();
                });
            }
        }

        private void StartFarmingTutorialStage()
        {
            currentState = TutorialState.FarmingTutorial;
            Debug.Log("[Tutorial] Bắt đầu Giai đoạn 2: Thực hành cải tạo ruộng khoai lang 4x3.");
            RefreshTutorialHUDCheckbox();
        }

        // --- Callbacks nhận tín hiệu từ các hành động nông nghiệp của SoilCell ---
        public void OnRockCleared()
        {
            if (currentState != TutorialState.FarmingTutorial) return;
            subTask1Completed = true;
            CheckFarmingTutorialCompletion();
        }

        public void OnCropPlanted()
        {
            if (currentState != TutorialState.FarmingTutorial) return;
            subTask2Completed = true;
            CheckFarmingTutorialCompletion();
        }

        public void OnSoilWatered()
        {
            if (currentState != TutorialState.FarmingTutorial) return;
            subTask3Completed = true;
            CheckFarmingTutorialCompletion();
        }

        private void CheckFarmingTutorialCompletion()
        {
            RefreshTutorialHUDCheckbox();

            if (subTask1Completed && subTask2Completed && subTask3Completed)
            {
                CompleteWholeTutorial();
            }
        }

        private void CompleteWholeTutorial()
        {
            currentState = TutorialState.Completed;
            Debug.Log("[Tutorial] Hoàn thành toàn bộ hướng dẫn! Chính thức bước vào mạch truyện chính Phase 1.");

            if (SurvivalUIManager.Instance != null)
            {
                SurvivalUIManager.Instance.HideTutorialQuestPanel();
                SurvivalUIManager.Instance.ShowHUDToast("+ Vòng lặp nông nghiệp hoàn tất! Đến Altar thắp nhang cầu an.");
            }

            // Chuyển trạng thái GameManager sang mốc làm nông tự do trước bão công vần công
            GameManager.Instance.TransitionToPhase(GamePhase.LapNghiep);
        }

        private void RefreshTutorialHUDCheckbox()
        {
            if (SurvivalUIManager.Instance == null) return;

            // Xây dựng chuỗi string checklist để đổ lên text component của TutorialQuestPanel HUD
            string hudContent = "";

            if (currentState == TutorialState.IntroQuests)
            {
                hudContent = "<b>NHIỆM VỤ: THĂM HỎI BÀ CON</b>\n" +
                             $"{(npcTalkRegistry["OTham"] ? "✓" : "☐")} Qua thăm O Thắm ở sạp hàng\n" +
                             $"{(npcTalkRegistry["BacNam"] ? "✓" : "☐")} Qua thăm Bác Năm cạnh nhà\n" +
                             $"{(npcTalkRegistry["CuBay"] ? "✓" : "☐")} Qua kính chào Cụ Bảy Trưởng thôn\n" +
                             $"{(npcTalkRegistry["BeTi"] ? "✓" : "☐")} Hỏi chuyện Bé Tí quanh giếng làng";
            }
            else if (currentState == TutorialState.FarmingTutorial)
            {
                hudContent = "<b>NHIỆM VỤ: CẢI TẠO RUỘNG VƯỜN</b>\n" +
                             $"{(subTask1Completed ? "✓" : "☐")} Dọn sỏi đá cằn trên ô đất\n" +
                             $"{(subTask2Completed ? "✓" : "☐")} Cuốc xới và Gieo hạt giống khoai\n" +
                             $"{(subTask3Completed ? "✓" : "☐")} Múc nước tưới ẩm giữ mầm cây";
            }

            SurvivalUIManager.Instance.UpdateTutorialQuestPanelText(hudContent);
        }

        public void RegisterTalkStart(string npcName) { }
        public void OnDialogueClosed(string npcName) => RegisterTalkComplete(npcName);
        public void ShowTutorial() => RefreshTutorialHUDCheckbox();
        public void ShowTutorial(Action onComplete)
        {
            RefreshTutorialHUDCheckbox();
            onComplete?.Invoke();
        }
        public void InitializeTutorial()
        {
            currentState = TutorialState.IntroQuests;
            subTask1Completed = false;
            subTask2Completed = false;
            subTask3Completed = false;
            RefreshTutorialHUDCheckbox();
        }

        // --- Getters hỗ trợ cờ trạng thái kiểm tra bên ngoài ---
        public TutorialState CurrentState => currentState;
        public bool isTutorialActive => currentState != TutorialState.Completed;
        public bool IsTutorialCompleted => currentState == TutorialState.Completed;
        public bool IsNPCTalked(string npcName) => npcTalkRegistry.ContainsKey(npcName) && npcTalkRegistry[npcName];
    }
}