using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SownInStone.Core;
using SownInStone.Community;

namespace SownInStone.UI
{
    public class NPCQuestMarkerUI : MonoBehaviour
    {
        private NPCCharacter[] npcs;
        private Dictionary<NPCCharacter, GameObject> npcMarkers = new Dictionary<NPCCharacter, GameObject>();
        private Dictionary<NPCCharacter, TextMeshProUGUI> npcMarkerTexts = new Dictionary<NPCCharacter, TextMeshProUGUI>();

        private void Start()
        {
            npcs = FindObjectsByType<NPCCharacter>(FindObjectsInactive.Exclude);
            CreateMarkers();
        }

        private void CreateMarkers()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindAnyObjectByType<Canvas>();
            }
            if (canvas == null) return;

            foreach (var npc in npcs)
            {
                if (npc == null) continue;

                // Tạo đối tượng Marker cho từng NPC
                GameObject markerObj = new GameObject($"QuestMarker_{npc.NPCName}");
                markerObj.transform.SetParent(canvas.transform, false);

                RectTransform rect = markerObj.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(40f, 40f);
                rect.pivot = new Vector2(0.5f, 0.5f);

                TextMeshProUGUI txt = markerObj.AddComponent<TextMeshProUGUI>();
                txt.text = "!";
                txt.fontSize = 28;
                txt.fontStyle = FontStyles.Bold;
                txt.alignment = TextAlignmentOptions.Center;
                txt.color = new Color(1f, 0.85f, 0.1f, 1f); // Màu vàng gold/vàng chanh sáng

                // Thêm outline đen dày giúp chữ "!" dễ đọc trên mọi nền 3D
                txt.outlineColor = Color.black;
                txt.outlineWidth = 0.25f;

                markerObj.SetActive(false);

                npcMarkers[npc] = markerObj;
                npcMarkerTexts[npc] = txt;
            }
        }

        private void Update()
        {
            if (PlayerController.Instance == null || Camera.main == null) return;

            Vector3 playerPos = PlayerController.Instance.transform.position;
            GamePhase currentPhase = GameManager.Instance != null ? GameManager.Instance.CurrentPhase : GamePhase.LapNghiep;

            // Tính toán hiệu ứng nảy (bounce) mượt mà dựa trên thời gian hình sin
            float bounceOffset = Mathf.Sin(Time.time * 5.5f) * 0.18f;

            foreach (var npc in npcs)
            {
                if (npc == null) continue;
                if (!npcMarkers.ContainsKey(npc)) continue;

                GameObject markerObj = npcMarkers[npc];
                bool shouldShow = false;

                // 1. Kiểm tra xem NPC hiện tại có Sự kiện Giai đoạn chưa hoàn thành hay không
                if (CommunityManager.Instance != null)
                {
                    if (npc.characterType == NPCCharacter.StoryCharacterType.OTham)
                    {
                        // O Thắm: Giai đoạn 2 (Gió Lào) hoặc Giai đoạn 4 (Phù Sa)
                        if (currentPhase == GamePhase.GioLao && !CommunityManager.Instance.eventOThamFoodCompleted)
                        {
                            shouldShow = true;
                        }
                        else if (currentPhase == GamePhase.PhuSa && !CommunityManager.Instance.eventVillageRecoveryCompleted)
                        {
                            shouldShow = true;
                        }
                    }
                    else if (npc.characterType == NPCCharacter.StoryCharacterType.BacNam)
                    {
                        // Bác Năm: Giai đoạn 3 (Mưa Bão) hoặc Giai đoạn 4 (Phù Sa)
                        if (currentPhase == GamePhase.MuaBao && !CommunityManager.Instance.eventBacNamStormCompleted)
                        {
                            shouldShow = true;
                        }
                        else if (currentPhase == GamePhase.PhuSa && !CommunityManager.Instance.eventVillageRecoveryCompleted)
                        {
                            shouldShow = true;
                        }
                    }
                }

                // 2. Ẩn dấu chấm hỏi/chấm than nếu người chơi đã tiến sát lại gần (để tránh đè lên Proximity Options panel)
                float distance = Vector3.Distance(playerPos, npc.transform.position);
                if (distance <= 1.7f)
                {
                    shouldShow = false;
                }

                // 3. Ẩn nếu đang mở hội thoại hoặc cửa hàng
                if (SurvivalUIManager.Instance != null && 
                    (SurvivalUIManager.Instance.IsDialogueActive || SurvivalUIManager.Instance.IsShopOpen))
                {
                    shouldShow = false;
                }

                // Cập nhật hiển thị và vị trí
                if (shouldShow)
                {
                    Vector3 worldPos = npc.transform.position + Vector3.up * (2.3f + bounceOffset);
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                    if (screenPos.z < 0)
                    {
                        markerObj.SetActive(false);
                    }
                    else
                    {
                        if (!markerObj.activeSelf) markerObj.SetActive(true);
                        markerObj.transform.position = screenPos;
                    }
                }
                else
                {
                    if (markerObj.activeSelf) markerObj.SetActive(false);
                }
            }
        }
    }
}
