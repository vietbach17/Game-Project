using UnityEngine;
using SownInStone.Core;

namespace SownInStone.Community
{
    /// <summary>
    /// Đối tượng đại diện cho một người dân làng trong game (như Bác Năm, O Thắm).
    /// Liên kết với ScriptableObject NPCData, có cơ chế đối thoại dự phòng (Fallback) cực kỳ chi tiết
    /// theo đúng 4 giai đoạn cốt truyện nếu chưa tạo file Asset tĩnh.
    /// </summary>
    public class NPCCharacter : MonoBehaviour
    {
        public enum StoryCharacterType
        {
            Custom,
            BacNam,    // Bác Năm: Lão nông cố vấn kỹ thuật và thời tiết dân gian
            OTham      // O Thắm: Người phụ nữ bộc trực chủ đại lý phân bón giàu lòng nhân ái
        }

        [Header("--- THIẾT LẬP NHÂN VẬT ---")]
        [Tooltip("Chọn nhân vật cốt truyện tương ứng để tự động lấy đối thoại tiếng Việt chi tiết.")]
        public StoryCharacterType characterType = StoryCharacterType.BacNam;

        [Tooltip("Dữ liệu ScriptableObject tùy biến (nếu dùng loại Custom).")]
        public NPCData npcData;

        [Header("--- QUAN HỆ & NGHĨA TÌNH ---")]
        [Range(0, 100)]
        [Tooltip("Điểm thân thiết hiện tại (0 - 100). Tăng khi gặt hộ, biếu quà Tết.")]
        public int Affection = 20;

        [Tooltip("Số ngày công tích lũy đổi công (Tục Vần Công) với người chơi.")]
        [SerializeField] private int vanCongCredits = 0;

        private void Start()
        {
            // Tự động đặt tên GameObject tương ứng trong Hierarchy để Designer dễ quản lý
            if (characterType == StoryCharacterType.BacNam)
            {
                gameObject.name = "NPC_BacNam (Bác Năm)";
            }
            else if (characterType == StoryCharacterType.OTham)
            {
                gameObject.name = "NPC_OTham (O Thắm)";
            }
            else if (npcData != null)
            {
                gameObject.name = $"NPC_{npcData.NPCName}";
            }
        }

        /// <summary>
        /// Cộng/Trừ điểm thân thiết của dân làng.
        /// </summary>
        public void ModifyAffection(int amount)
        {
            Affection = Mathf.Clamp(Affection + amount, 0, 100);
            Debug.Log($"[NPC] {NPCName} thay đổi điểm thân thiết: {Affection}/100.");
        }

        /// <summary>
        /// Cộng/Trừ điểm tích lũy Vần Công.
        /// </summary>
        public void ModifyVanCongCredits(int amount)
        {
            vanCongCredits += amount;
            Debug.Log($"[VẦN CÔNG] Thay đổi công nợ đổi công với {NPCName}: {vanCongCredits} công.");
        }

        public string NPCName
        {
            get
            {
                if (characterType == StoryCharacterType.BacNam) return "Bác Năm";
                if (characterType == StoryCharacterType.OTham) return "O Thắm";
                return npcData != null ? npcData.NPCName : "Người dân làng";
            }
        }

        public int VanCongCredits => vanCongCredits;

        /// <summary>
        /// Lấy hội thoại phù hợp dựa theo Giai đoạn game và điểm thân thiết.
        /// Sử dụng dữ liệu cốt truyện chi tiết làm dự phòng nếu chưa liên kết Asset.
        /// </summary>
        public string GetDialogue()
        {
            GamePhase currentPhase = GamePhase.LapNghiep;
            if (GameManager.Instance != null)
            {
                currentPhase = GameManager.Instance.CurrentPhase;
            }

            // Nếu người dùng thiết lập ScriptableObject tùy biến, ưu tiên đọc từ đó
            if (characterType == StoryCharacterType.Custom && npcData != null)
            {
                return npcData.GetDialogue(currentPhase, Affection);
            }

            // Ngược lại, trả về lời thoại cốt truyện được viết tay cực hay cho Bác Năm và O Thắm
            return GetFallbackStoryDialogue(currentPhase);
        }

        /// <summary>
        /// Kho hội thoại dự phòng biên soạn chi tiết theo tài liệu cốt truyện của bạn.
        /// </summary>
        private string GetFallbackStoryDialogue(GamePhase phase)
        {
            if (characterType == StoryCharacterType.BacNam)
            {
                switch (phase)
                {
                    case GamePhase.LapNghiep:
                        if (Affection < 30) return "\"Bám trụ lại mảnh đất sỏi cát Trường Sơn này làm chi con ơi, chó ăn đá gà ăn sỏi làm ăn cực lắm!\"";
                        if (Affection > 70) return "\"Ráng lên con! Hãy dựng lại bờ giậu tre chống gió dột, cuốc hết sỏi đá vườn rồi bác chỉ cách trồng khoai lang chịu hạn.\"";
                        return "\"Đất đai tổ tiên để lại xơ xác quá, con gánh thêm nước giếng làng tưới ẩm cho luống đất cát bạc màu đi.\"";

                    case GamePhase.GioLao:
                        if (Affection < 30) return "\"Gió Lào thổi khô rát như lò lửa thế này, làm việc giữa trưa là sốc nhiệt ngất xỉu đó con, vô mát nghỉ đi.\"";
                        if (Affection > 70) return "\"Giếng nhà con cạn rồi đúng không? Qua bưng khạp nước ngọt cuối vườn nhà bác về xài tạm, nhớ tiết kiệm tưới ớt con nghe.\"";
                        return "\"Đêm nay gió mát hơn, xóm mình ra bóng cây đa uống chè xanh om đặc, ăn miếng kẹo cu đơ hò ví dặm xua đi cái nóng rát.\"";

                    case GamePhase.MuaBao:
                        if (Affection < 30) return "\"Mưa gió trắng trời! Mau dồn đàn gà lên gác xép đi Thành! Bão siêu mạnh đổ bộ xã mình tới nơi rồi!\"";
                        if (Affection > 70) return "\"[TIẾNG KÊU CỨU TRONG BÃO LŨ] Thành ơi! Mái nhà bác sập rồi, nước sông dâng ngập quá cửa sổ gác xép rồi! Cứu bác với con ơi!\"";
                        return "\"Nước sông dâng nhanh quá, dùng thừng buộc chặt cột nhà cấp 4 đi, tích trữ khoai gieo khô lên nóc nhà cố thủ!\"";

                    case GamePhase.PhuSa:
                        if (Affection > 70) return "\"Thiên tai lũ lụt cuốn đi tất cả hoa màu rồi... Nhưng nhìn kìa con, đất cày dẫu sỏi đá dập dờn, chỉ cần lòng người đồng lòng bám đất thì sỏi đá cũng hóa cơm bùi!\"";
                        return "\"Đừng nản chí Thành ơi, xóm giềng đang gọi nhau sang dựng hộ nhà kìa. Lớp bùn non lũ để lại chính là phù sa màu mỡ nhất cho vụ chiêm xuân!\"";
                }
            }
            else if (characterType == StoryCharacterType.OTham)
            {
                switch (phase)
                {
                    case GamePhase.LapNghiep:
                        if (Affection < 30) return "\"O Thắm chủ đại lý đây con. Về quê làm nông hả Thành? Thiếu phân bón hữu cơ cải tạo cát bạc màu cứ ra chỗ o nhé.\"";
                        if (Affection > 70) return "\"Về quê bám đất thờ cúng tổ tiên là ngoan lắm con, o cằn nhằn xíu thôi chứ thiếu thốn cái gì o cho ghi sổ nợ sang năm trả!\"";
                        return "\"Hành tăm (ném) với đậu phộng o mới nhập giống tốt lắm, lấy vài cân gieo trồng ngắn ngày đi con.\"";

                    case GamePhase.GioLao:
                        if (Affection < 30) return "\"Nắng rát ruột gan thế này thì ớt cháy lá hết. O xót ruộng ớt nhà con quá, tối ra gánh nước đêm cứu ruộng đi con.\"";
                        if (Affection > 70) return "\"Mệt quá thì vào đây o om ấm chè xanh đặc nóng, o Thắm phát kẹo cu đơ cho ăn đỡ mệt, cả xóm đang hò ví dặm vui lắm!\"";
                        return "\"Gió Lào Trường Sơn khô bỏng da, o bán bớt phân đạm đắt tiền, lấy phân hữu cơ ủ phân chuồng phân xanh giữ ẩm đất nha.\"";

                    case GamePhase.MuaBao:
                        if (Affection < 30) return "\"Nghe loa phát thanh xã báo động lụt chưa con? O Thắm đóng cửa đại lý rồi, mang bao cát về chèn mái tôn lẹ đi!\"";
                        if (Affection > 70) return "\"Thành ơi, o bớt mấy thùng mì tôm sống với nước khạp ngọt o quăng lên ghe chở qua cho con trữ sinh tồn cắm trại nóc nhà nè!\"";
                        return "\"Bão to lắm, gặt chạy lũ lúa non đi con, cứu được hạt nào hay hạt nấy, đừng tham để chín là trắng tay đó!\"";

                    case GamePhase.PhuSa:
                        if (Affection > 70) return "\"Nhà o dột mất góc bếp nhưng không sao! O Thắm có mì tôm cứu trợ đây, con qua phụ o phát mì cho mấy cụ già trong xóm nghe con!\"";
                        return "\"Lũ tan rồi, ruộng đồng ngập ngụa bùn non phù sa. O Thắm chuẩn bị giống mới giá rẻ cho bà con tái thiết mùa sau đây!\"";
                }
            }

            return $"\"Chào con, Thành. Đất cày dẫu lên sỏi đá, cố gắng bám trụ làm nông sinh tồn con nghe.\"";
        }

        /// <summary>
        /// Lấy câu thoại phản hồi tương ứng khi Thành muốn giúp việc (Vần Công).
        /// </summary>
        public string GetWorkDialogue(bool hasStamina)
        {
            if (characterType == StoryCharacterType.BacNam)
            {
                if (hasStamina)
                {
                    return "\"Được quá con ơi! Bác già cả đau lưng cuốc đất không nổi, có con phụ bác cuốc giùm luống cát này đỡ biết bao. Bác ghi nợ con 1 ngày công vần công nhé!\"";
                }
                else
                {
                    return "\"Thôi thôi con ơi! Nhìn mặt mày tái mét kìa, vô mát nghỉ ngơi uống bát chè xanh đi, kẻo cảm nắng ngất xỉu ra đó bác không kham nổi đâu!\"";
                }
            }
            else if (characterType == StoryCharacterType.OTham)
            {
                if (hasStamina)
                {
                    return "\"Trời đất, Thành ngoan quá hà! Phụ o Thắm bê mấy bao lúa giống xếp lên kệ sạp đi con. Bữa sau bão lụt o Thắm chở mì tôm sang cứu trợ trả nợ công nha con!\"";
                }
                else
                {
                    return "\"Thôi, o Thắm bộc trực chứ không ác độc nha! Người mệt lả thế kia bê vác cái gì, vô uống miếng nước lọc ăn cái kẹo cu đơ nghỉ đi con!\"";
                }
            }

            if (hasStamina)
            {
                return "\"Cảm ơn con nhiều nhé! Việc này vất vả quá, bác ghi nợ con 1 ngày công vần công.\"";
            }
            else
            {
                return "\"Con mệt mỏi quá rồi, hãy nghỉ ngơi lấy lại sức khỏe đã.\"";
            }
        }

        /// <summary>
        /// Tặng quà cho NPC.
        /// </summary>
        public bool ActionGiveGift(string itemName)
        {
            bool isLoved = false;
            
            // Định nghĩa sở thích quà tặng theo cốt truyện
            if (characterType == StoryCharacterType.BacNam && itemName == "Incense") isLoved = true; // Bác Năm thích nhang cúng thờ lễ đình
            if (characterType == StoryCharacterType.OTham && itemName == "Khoai Gieo") isLoved = true; // O Thắm thích ăn khoai lang khô dẻo ngọt

            int affectionBoost = isLoved ? 20 : 8;
            int karmaBoost = isLoved ? 8 : 3;

            ModifyAffection(affectionBoost);
            CommunityManager.Instance?.ModifyGlobalKarma(karmaBoost);

            if (isLoved)
            {
                if (characterType == StoryCharacterType.BacNam)
                    Debug.Log($"[NPC] Bác Năm: \"Ôi trời con chu đáo quá, đúng cây nhang bác cần thắp mùng một cầu an! Cảm ơn con nghe!\"");
                else if (characterType == StoryCharacterType.OTham)
                    Debug.Log($"[NPC] O Thắm: \"Trời ơi khoai gieo dai dẻo dính răng ăn ngon thiệt chứ! Thành chu đáo quá o Thắm thương o bớt nợ phân bón cho nè!\"");
            }
            else
            {
                Debug.Log($"[NPC] {NPCName} nhận quà: \"Bác/O cảm ơn con nhiều nhé Thành!\"");
            }

            return true;
        }
    }
}
