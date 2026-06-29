using System.Collections.Generic;
using UnityEngine;
using SownInStone.Community;

namespace SownInStone.Interactions
{
    /// <summary>
    /// Component placed on Sandbags and Flood Boards.
    /// Used by SoilCells to detect protection from flood water.
    /// </summary>
    public class FloodBarrier : MonoBehaviour
    {
        public static List<FloodBarrier> ActiveBarriers = new List<FloodBarrier>();

        [Header("Settings")]
        [Tooltip("The protection radius of this barrier.")]
        public float protectionRadius = 2.2f;

        [Tooltip("Type of barrier: True = Sandbag, False = Flood Board.")]
        public bool isSandbag = true;

        private void OnEnable()
        {
            if (!ActiveBarriers.Contains(this))
            {
                ActiveBarriers.Add(this);
            }

            // Kiểm tra xem vị trí đặt bao cát có nằm trên nóc nhà (độ cao Y >= 2.2m) để chằng chống mái nhà không
            if (transform.position.y >= 2.2f)
            {
                CommunityManager.Instance?.ModifyGlobalKarma(10);
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("🏠 ĐÃ GIA CỐ NÓC NHÀ: Đặt bao cát chằng chống mái nhà an toàn trước cuồng phong! (+10 Nghĩa Tình)");
                }
            }

            // Kiểm tra xem vị trí đặt có nằm gần nhà dân làng (NPC) để thưởng điểm Nghĩa Tình không
            var npcs = FindObjectsByType<NPCCharacter>(FindObjectsInactive.Exclude);
            foreach (var npc in npcs)
            {
                if (npc != null && Vector3.Distance(transform.position, npc.transform.position) <= 8f)
                {
                    CommunityManager.Instance?.ModifyGlobalKarma(15);
                    if (SownInStone.UI.SurvivalUIManager.Instance != null)
                    {
                        SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast($"🧡 NGHĨA TÌNH: Đã gia cố chắn lũ hỗ trợ nhà {npc.NPCName}! (+15 Nghĩa Tình)");
                    }
                    break;
                }
            }
        }

        private void OnDisable()
        {
            if (ActiveBarriers.Contains(this))
            {
                ActiveBarriers.Remove(this);
            }
        }
    }
}

