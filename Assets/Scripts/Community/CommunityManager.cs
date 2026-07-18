using System.Collections.Generic;
using UnityEngine;
using SownInStone.Core;

namespace SownInStone.Community
{
    /// <summary>
    /// Điều phối điểm Karma (Nghĩa Tình) toàn bản đồ và quản lý hệ thống đổi công hỗ trợ khẩn cấp.
    /// </summary>
    public class CommunityManager : MonoBehaviour
    {
        public static CommunityManager Instance { get; private set; }

        [Header("--- NGHĨA TÌNH & NGHIỆP QUẢ ---")]
        [Tooltip("Điểm nghĩa tình chung của người chơi trong cả làng (0 - 100). Đạt càng cao càng có nhiều đặc quyền xã hội.")]
        [SerializeField] private int globalKarmaPoints = 0;

        [Header("--- SỰ KIỆN CỘNG ĐỒNG (PHASE-BASED EVENTS) ---")]
        public bool eventOThamFoodCompleted = false;
        public bool eventBacNamStormCompleted = false;
        public bool eventVillageRecoveryCompleted = false;

        [Header("--- HÀNG XÓM LÁNG GIỀNG ---")]
        [Tooltip("Danh sách toàn bộ người dân làng trong hệ thống.")]
        [SerializeField] private List<NPCCharacter> villagerNPCs = new List<NPCCharacter>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                globalKarmaPoints = 0; // Ép khởi đầu Nghĩa Tình = 0
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Tự động tìm kiếm các NPC trong Scene nếu chưa được gán tay
            if (villagerNPCs.Count == 0)
            {
#if UNITY_2023_1_OR_NEWER
                villagerNPCs.AddRange(FindObjectsByType<NPCCharacter>(FindObjectsInactive.Exclude));
#else
                villagerNPCs.AddRange(FindObjectsOfType<NPCCharacter>());
#endif
            }
        }

        /// <summary>
        /// Cộng hoặc trừ điểm Karma chung của làng.
        /// </summary>
        public void ModifyGlobalKarma(int amount)
        {
            globalKarmaPoints = Mathf.Clamp(globalKarmaPoints + amount, 0, 100);
            Debug.Log($"[COMMUNITY] Điểm Nghĩa Tình (Karma) toàn cầu thay đổi: {globalKarmaPoints}");
        }

        public int GlobalKarma => globalKarmaPoints;

        /// <summary>
        /// Tính tổng điểm Vần Công mà dân làng đang nợ người chơi.
        /// Dùng để tính toán sự giúp đỡ tự động khi bão đổ bộ.
        /// </summary>
        public int GetTotalVầnCôngCreditsAvailable()
        {
            int total = 0;
            foreach (var npc in villagerNPCs)
            {
                if (npc.VanCongCredits > 0)
                {
                    total += npc.VanCongCredits;
                }
            }
            return total;
        }

        public void TriggerStormHelpSequence()
        {
            int credits = GetTotalVầnCôngCreditsAvailable();
            string title = "";
            string description = "";
            
            if (credits >= 5)
            {
                title = "VẦN CÔNG HỖ TRỢ";
                description = "Nhờ tinh thần tương thân tương ái trước bão, cả nhà Bác Năm và O Thắm đã mang bao cát và dây thừng sang giúp chằng chống mái nhà vững chãi hộ bạn! Ngôi nhà của bạn đã được gia cố hoàn hảo.";
                Debug.Log("[VẦN CÔNG] Cả nhà Bác Năm và O Thắm mang bao cát và dây thừng sang chằng mái nhà vững chãi hộ bạn! Ngôi nhà của bạn đã được gia cố hoàn hảo!");
                PlayerStats.Instance?.ModifyMorale(15f); // Tăng tinh thần khi nhận được sự ấm áp của xóm giềng
            }
            else if (credits >= 2)
            {
                title = "VẦN CÔNG HỖ TRỢ";
                description = "Nhờ sự hỗ trợ trước bão, Bác Năm đã sang chằng chống hộ một góc nhà và phụ lùa gà vịt lên gác xép cùng bạn tránh lũ dâng.";
                Debug.Log("[VẦN CÔNG] Bác Năm sang kéo hộ dây thừng và lùa gà lên gác xép cùng bạn trước khi mưa lũ dâng.");
                PlayerStats.Instance?.ModifyMorale(5f);
            }
            else
            {
                title = "TỰ LỰC CÁNH SINH";
                description = "Do bạn không có đủ tích lũy điểm Vần Công giúp đỡ bà con trước đây, không ai sang hỗ trợ gia cố nhà cửa lúc bão lũ đổ bộ. Bạn phải đơn độc chống chọi với thiên tai.";
                Debug.LogWarning("[VẦN CÔNG] Bạn không có đủ điểm tích lũy Vần Công trước đây. Không ai sang giúp gia cố nhà cửa! Bạn phải tự chống bão một mình!");
                PlayerStats.Instance?.ModifyMorale(-10f); // Tủi thân hoảng sợ khi cô độc chống bão
            }

            if (SownInStone.UI.SurvivalUIManager.Instance != null)
            {
                SownInStone.UI.SurvivalUIManager.Instance.ShowDialogue(title, description);
            }

            // Tiêu hao bớt điểm đổi công sau khi đã sử dụng
            foreach (var npc in villagerNPCs)
            {
                if (npc.VanCongCredits > 0)
                {
                    npc.ModifyVanCongCredits(-npc.VanCongCredits); // Khấu trừ sạch công nợ
                }
            }
        }
    }
}
