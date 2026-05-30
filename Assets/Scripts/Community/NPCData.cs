using System.Collections.Generic;
using UnityEngine;
using SownInStone.Core;

namespace SownInStone.Community
{
    [System.Serializable]
    public class PhaseDialogue
    {
        public GamePhase phase;
        [TextArea(3, 5)]
        public List<string> lowAffectionDialogues;  // Đối thoại khi điểm thân thiết thấp (<30)
        [TextArea(3, 5)]
        public List<string> midAffectionDialogues;  // Đối thoại khi điểm thân thiết trung bình (30 - 70)
        [TextArea(3, 5)]
        public List<string> highAffectionDialogues; // Đối thoại khi điểm thân thiết cao (>70)
    }

    /// <summary>
    /// ScriptableObject định nghĩa dữ liệu tĩnh, tiểu sử và hội thoại theo từng Giai đoạn của nhân vật.
    /// Giúp Designer dễ dàng thêm bớt hội thoại cốt truyện ngay trong Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "NewNPCData", menuName = "Sown In Stone/NPC Data")]
    public class NPCData : ScriptableObject
    {
        [Header("--- TIỂU SỬ NHÂN VẬT ---")]
        public string NPCName = "Bác Năm";
        public Sprite Avatar;
        
        [TextArea(2, 4)]
        public string Biography = "Lão nông dày dặn kinh nghiệm xóm giềng. Sẵn lòng chỉ dạy Thành kinh nghiệm đồng áng và ca dao tục ngữ đoán thời tiết.";

        [Header("--- HỘI THOẠI CỐT TRUYỆN THEO GIAI ĐOẠN ---")]
        [Tooltip("Danh sách hội thoại phân nhánh theo từng Giai Đoạn của game.")]
        public List<PhaseDialogue> phaseDialogues = new List<PhaseDialogue>();

        [Header("--- SỞ THÍCH QUÀ TẶNG ---")]
        [Tooltip("Tên những vật phẩm NPC này rất thích nhận (để cộng thêm nhiều Affection).")]
        public List<string> lovedItems = new List<string>();

        /// <summary>
        /// Lấy hội thoại phù hợp dựa theo Giai đoạn game và mức độ thân thiết hiện tại.
        /// </summary>
        public string GetDialogue(GamePhase currentPhase, int currentAffection)
        {
            // Tìm đối thoại của Phase hiện tại
            PhaseDialogue dialogueGroup = phaseDialogues.Find(d => d.phase == currentPhase);
            if (dialogueGroup == null)
            {
                return $"... (NPC {NPCName} đang bận rộn suy nghĩ) ...";
            }

            List<string> selectedList;
            if (currentAffection >= 70)
            {
                selectedList = dialogueGroup.highAffectionDialogues;
            }
            else if (currentAffection <= 30)
            {
                selectedList = dialogueGroup.lowAffectionDialogues;
            }
            else
            {
                selectedList = dialogueGroup.midAffectionDialogues;
            }

            if (selectedList == null || selectedList.Count == 0)
            {
                return $"\"Chào con, Thành. Thời tiết dạo này đổi thay quá.\"";
            }

            // Trả về một câu ngẫu nhiên trong danh sách phù hợp
            int randIndex = Random.Range(0, selectedList.Count);
            return selectedList[randIndex];
        }
    }
}
