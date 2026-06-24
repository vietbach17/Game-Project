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
            OTham,     // O Thắm: Người phụ nữ bộc trực chủ đại lý phân bón giàu lòng nhân ái
            CuBay,     // Cụ Bảy
            BeTi       // Bé Tí
        }

        [Header("--- THIẾT LẬP NHÂN VẬT ---")]
        [Tooltip("Chọn nhân vật cốt truyện tương ứng để tự động lấy đối thoại tiếng Việt chi tiết.")]
        public StoryCharacterType characterType = StoryCharacterType.BacNam;

        [Tooltip("Dữ liệu ScriptableObject tùy biến (nếu dùng loại Custom).")]
        public NPCData npcData;

        [Header("--- MÔ HÌNH 3D NHÂN VẬT ---")]
        [Tooltip("Kéo file FBX hoặc Prefab mô hình 3D của nhân vật vào đây để tự động hiển thị.")]
        public GameObject visualModelPrefab;

        [Range(0, 100)]
        [Tooltip("Điểm thân thiết hiện tại (0 - 100). Tăng khi gặt hộ, biếu quà Tết.")]
        public int Affection = 20;

        [Tooltip("Số ngày công tích lũy đổi công (Tục Vần Công) với người chơi.")]
        [SerializeField] private int vanCongCredits = 0;

        private bool hasTalkedThisSession = false;

        public bool CanReceiveTalkReward()
        {
            return !hasTalkedThisSession;
        }

        public void MarkTalkedToday()
        {
            hasTalkedThisSession = true;
        }

        private Quaternion defaultRotation;

        private void Start()
        {
            defaultRotation = transform.rotation;

            // Tự động đặt tên GameObject tương ứng trong Hierarchy để Designer dễ quản lý
            if (characterType == StoryCharacterType.BacNam)
            {
                gameObject.name = "NPC_BacNam (Bác Năm)";
            }
            else if (characterType == StoryCharacterType.OTham)
            {
                gameObject.name = "NPC_OTham (O Thắm)";
            }
            else if (characterType == StoryCharacterType.CuBay)
            {
                gameObject.name = "NPC_CuBay (Cụ Bảy)";
            }
            else if (characterType == StoryCharacterType.BeTi)
            {
                gameObject.name = "NPC_BeTi (Bé Tí)";
            }
            else if (npcData != null)
            {
                gameObject.name = $"NPC_{npcData.NPCName}";
            }

            // Tự động kiểm tra và sửa hiển thị visual ở Runtime
            EnsureVisualModel();
        }

        /// <summary>
        /// Đảm bảo mô hình 3D hiển thị đúng và thay thế các placeholder (như capsule, sprite rỗng).
        /// </summary>
        public void EnsureVisualModel()
        {
            Transform existingVisual = transform.Find("Visual");
            bool isPlaceholder = false;

            if (existingVisual != null)
            {
                // Nếu có SpriteRenderer nhưng không có Sprite -> Placeholder cần thay thế
                var spriteRen = existingVisual.GetComponent<SpriteRenderer>();
                if (spriteRen != null && spriteRen.sprite == null)
                {
                    isPlaceholder = true;
                }

                // Nếu có MeshFilter và đang sử dụng Mesh mặc định là Capsule -> Placeholder cần thay thế
                var meshFilter = existingVisual.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null && (meshFilter.sharedMesh.name.Contains("Capsule") || meshFilter.sharedMesh.name == "Default-Capsule"))
                {
                    isPlaceholder = true;
                }
            }
            else
            {
                isPlaceholder = true;
            }

            if (isPlaceholder && visualModelPrefab != null)
            {
                Debug.Log($"[NPC RUNTIME] Tự động tải và dựng mô hình 3D cho {gameObject.name}...");
                
                // Xóa Visual cũ
                if (existingVisual != null)
                {
                    Destroy(existingVisual.gameObject);
                }

                // Khởi tạo visual mới từ FBX model
                GameObject modelObj = Instantiate(visualModelPrefab, transform);
                modelObj.name = "Visual";
                
                // Căn chỉnh vị trí, góc xoay và tỉ lệ giống như thiết lập chuẩn
                modelObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                
                float scaleVal = 1.0f;
                float localY = -0.5f;
                if (characterType == StoryCharacterType.BacNam)
                {
                    scaleVal = 1.22f;
                    localY = 0.54f;
                }
                else if (characterType == StoryCharacterType.OTham)
                {
                    scaleVal = 1.30f;
                    localY = 0.61f;
                }
                
                modelObj.transform.localPosition = new Vector3(0f, localY, 0f);
                modelObj.transform.localScale = new Vector3(scaleVal, scaleVal, scaleVal);

                // Cập nhật lại BoxCollider nếu cần thiết
                BoxCollider boxCol = GetComponent<BoxCollider>();
                if (boxCol != null)
                {
                    boxCol.isTrigger = true;
                    boxCol.center = new Vector3(0f, 1f, 0f);
                    boxCol.size = new Vector3(1.2f, 2f, 1.2f);
                }
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
                if (characterType == StoryCharacterType.CuBay) return "Cụ Bảy";
                if (characterType == StoryCharacterType.BeTi) return "Bé Tí";
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

                    case GamePhase.ChuanBiBao:
                        if (Affection < 30) return "\"Bão sắp vô rồi Thành ơi! Bác nghe loa phóng thanh giục giã dữ lắm. Lo dọn ruộng gặt non lẹ đi con!\"";
                        if (Affection > 70) return "\"Mái tôn hay mái lá nhà con chưa gia cố thì qua bác lấy thừng với bao cát bác cho mượn chèn lên mái!\"";
                        return "\"Loa phóng thanh xã đang báo bão cấp 11 hướng thẳng vô quê mình. Tranh thủ vần công cùng bà con chằng chống nhà nghe con.\"";

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

                    case GamePhase.ChuanBiBao:
                        if (Affection < 30) return "\"Thành ơi, lo chằng nhà lẹ đi! Bão to lắm đó, o Thắm đang bận bán mấy bao cát cứu hộ đầu làng nè!\"";
                        if (Affection > 70) return "\"Nhà con lá đơn sơ quá, qua o cho mượn thừng với mấy bao cát chèn mái, có thiếu tiền o cho ghi nợ luôn!\"";
                        return "\"O mới nhập thêm mì tôm với dầu gió dự trữ bão lụt, con có mua thì ghé tiệm o lấy sớm nha.\"";

                    case GamePhase.MuaBao:
                        if (Affection < 30) return "\"Nghe loa phát thanh xã báo động lụt chưa con? O Thắm đóng cửa đại lý rồi, mang bao cát về chèn mái tôn lẹ đi!\"";
                        if (Affection > 70) return "\"Thành ơi, o bớt mấy thùng mì tôm sống với nước khạp ngọt o quăng lên ghe chở qua cho con trữ sinh tồn cắm trại nóc nhà nè!\"";
                        return "\"Bão to lắm, gặt chạy lũ lúa non đi con, cứu được hạt nào hay hạt nấy, đừng tham để chín là trắng tay đó!\"";

                    case GamePhase.PhuSa:
                        if (Affection > 70) return "\"Nhà o dột mất góc bếp nhưng không sao! O Thắm có mì tôm cứu trợ đây, con qua phụ o phát mì cho mấy cụ già trong xóm nghe con!\"";
                        return "\"Lũ tan rồi, ruộng đồng ngập ngụa bùn non phù sa. O Thắm chuẩn bị giống mới giá rẻ cho bà con tái thiết mùa sau đây!\"";
                }
            }
            else if (characterType == StoryCharacterType.CuBay)
            {
                switch (phase)
                {
                    case GamePhase.LapNghiep:
                        return "\"Con à, đất lội thì nghèo, đất cát thì heo hút. Nhưng bám trụ Trường Sơn gieo khoai gieo ném thì có hạt ăn hạt gieo.\"";
                    case GamePhase.GioLao:
                        return "\"Cái Gió Lào Trường Sơn này nó thiêu đốt cỏ cây. Lấy rơm rạ hay lá cọ phủ gốc khoai mới giữ được ẩm đó con.\"";
                    case GamePhase.ChuanBiBao:
                        return "\"Chim bay về núi là bão sắp đổ bộ rồi. Lo chèn bao cát chắc chắn lên mái tranh kẻo gió cuốn bay tuốt.\"";
                    case GamePhase.MuaBao:
                        return "\"Bão lũ trắng trời thế này nguy hiểm quá con ơi. Ráng giữ an toàn ở nơi cao ráo nghe con.\"";
                    case GamePhase.PhuSa:
                        return "\"Bùn sa bồi đắp sau lũ trù phú lắm. Đừng lo con ơi, phù sa này sẽ gánh cho vụ mùa sau xanh mướt đất làng.\"";
                }
            }
            else if (characterType == StoryCharacterType.BeTi)
            {
                switch (phase)
                {
                    case GamePhase.LapNghiep:
                        return "\"Chú Thành ơi, con đói quá... Nhưng con vẫn giúp mẹ nhặt củi khô ở góc vườn nè!\"";
                    case GamePhase.GioLao:
                        return "\"Nắng to quá chú Thành ơi, chân con giẫm lên cát nóng bỏng rát luôn. Con cầu trời mau mưa xuống chú trồng khoai lang nha.\"";
                    case GamePhase.ChuanBiBao:
                        return "\"Mây đen thui che kín trời rồi chú Thành ơi. Con sợ tiếng sấm lắm, chú dắt con vô nhà trú bão với nha!\"";
                    case GamePhase.MuaBao:
                        return "\"Chú Thành ơi, gió thổi mạnh làm nhà con lung lay quá, con sợ lắm chú ơi... Chú nắm tay con được hông?\"";
                    case GamePhase.PhuSa:
                        return "\"Lũ lụt trôi hết đồ chơi của con rồi... Nhưng chú Thành ơi, chú cháu mình trồng khoai vụ mới nha, con phụ chú tưới nước chịu hông?\"";
                }
            }

            return $"\"Chào con, Thành. Đất cày dẫu lên sỏi đá, cố gắng bám trụ làm nông sinh tồn con nghe.\"";
        }

        /// <summary>
        /// Lấy câu thoại phản hồi tương ứng khi Thành muốn giúp việc (Vần Công).
        /// </summary>
        public string GetWorkDialogue(bool hasStamina)
        {
            GamePhase currentPhase = GameManager.Instance != null ? GameManager.Instance.CurrentPhase : GamePhase.LapNghiep;
            if (characterType == StoryCharacterType.BacNam)
            {
                if (hasStamina)
                {
                    if (currentPhase == GamePhase.ChuanBiBao)
                    {
                        return "\"Tốt quá con ơi! Mau bê giùm bác mấy bao cát chất lên mái tôn, rồi lấy thừng buộc chặt chân cột lại. Bão vô thổi bay mái nhà bác mất! Cảm ơn con nhiều, bác ghi nhận 1 ngày công chằng chống nhà nghe!\"";
                    }
                    return "\"Được quá con ơi! Bác già cả đau lưng cuốc đất không nổi, có con phụ bác cuốc giùm luống cát này đỡ biết bao. Bác ghi nợ con 1 ngày công vần công nhé!\"";
                }
                else
                {
                    if (currentPhase == GamePhase.ChuanBiBao)
                    {
                        return "\"Thôi con ơi, thở không ra hơi thế kia bê bao cát sao nổi! Vô nghỉ uống bát nước chè xanh đi, để bác tự ráng chèn dây thép cũng được.\"";
                    }
                    return "\"Thôi thôi con ơi! Nhìn mặt mày tái mét kìa, vô mát nghỉ ngơi uống bát chè xanh đi, kẻo cảm nắng ngất xỉu ra đó bác không kham nổi đâu!\"";
                }
            }
            else if (characterType == StoryCharacterType.OTham)
            {
                if (hasStamina)
                {
                    if (currentPhase == GamePhase.ChuanBiBao)
                    {
                        return "\"Trời đất, Thành ngoan quá hà! Phụ o Thắm xếp mấy bao cát chèn mái đại lý tạp hóa lẹ đi con. Bão giật dữ dằn lắm đó. Bác Năm ghi chép công nợ vần công cho con đợt chống bão này nha!\"";
                    }
                    return "\"Trời đất, Thành ngoan quá hà! Phụ o Thắm bê mấy bao lúa giống xếp lên kệ sạp đi con. Bữa sau bão lụt o Thắm chở mì tôm sang cứu trợ trả nợ công nha con!\"";
                }
                else
                {
                    if (currentPhase == GamePhase.ChuanBiBao)
                    {
                        return "\"Loa báo bão giục ghê quá nhưng con mệt lử rồi đừng ráng! Cứ vô hiên đại lý nghỉ ngơi ăn cái kẹo cu đơ, để o nhờ mấy đứa thanh niên trong xóm phụ sau.\"";
                    }
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

        private Coroutine lookAtCoroutine;

        public void LookAtPlayer(Transform player)
        {
            if (player == null) return;
            if (lookAtCoroutine != null)
            {
                StopCoroutine(lookAtCoroutine);
            }
            lookAtCoroutine = StartCoroutine(LookAtCoroutine(player));
        }

        private System.Collections.IEnumerator LookAtCoroutine(Transform player)
        {
            float duration = 0.15f;
            float elapsed = 0f;
            Quaternion startRot = transform.rotation;
            
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                if (characterType == StoryCharacterType.OTham || characterType == StoryCharacterType.BacNam)
                {
                    targetRot *= Quaternion.Euler(0, 180, 0);
                }

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / duration);
                    yield return null;
                }
                transform.rotation = targetRot;
            }
            lookAtCoroutine = null;
        }

        public void ReturnToDefaultRotation()
        {
            if (SownInStone.UI.SurvivalUIManager.Instance != null && SownInStone.UI.SurvivalUIManager.Instance.IsDialogueActive)
            {
                return;
            }

            if (lookAtCoroutine != null)
            {
                StopCoroutine(lookAtCoroutine);
            }
            lookAtCoroutine = StartCoroutine(ReturnToDefaultCoroutine());
        }

        private System.Collections.IEnumerator ReturnToDefaultCoroutine()
        {
            float duration = 0.35f;
            float elapsed = 0f;
            Quaternion startRot = transform.rotation;

            while (elapsed < duration)
            {
                if (SownInStone.UI.SurvivalUIManager.Instance != null && SownInStone.UI.SurvivalUIManager.Instance.IsDialogueActive)
                {
                    lookAtCoroutine = null;
                    yield break;
                }

                elapsed += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(startRot, defaultRotation, elapsed / duration);
                yield return null;
            }
            transform.rotation = defaultRotation;
            lookAtCoroutine = null;
        }
    }
}
