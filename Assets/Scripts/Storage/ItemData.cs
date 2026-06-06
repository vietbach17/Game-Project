using UnityEngine;

namespace SownInStone.Storage
{
    public enum ItemType
    {
        NongSanTuoi,   // Khoai lang, lạc, ớt mới hái (dễ hỏng khi ngập lụt, nồm ẩm)
        NongSanKho,    // Khoai gieo phơi khô, dưa nhút muối (để cực lâu, cứu sinh mùa bão)
        NuocNgot,      // Nước gánh giếng, nước mưa trữ lu khạp
        Incense,       // Nhang cúng Tổ tiên / Thổ địa giúp nâng tinh thần
        VatLieu,       // Tre, mây, dây thừng, bao cát chằng chống nhà
        HatGiong       // Hạt giống cây trồng
    }

    /// <summary>
    /// Dữ liệu ScriptableObject định nghĩa cho từng loại vật phẩm/tài nguyên trong game.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemData", menuName = "Sown In Stone/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("--- THÔNG TIN VẬT PHẨM ---")]
        public string ItemID = "item_khoai_gieo";
        public string ItemName = "Khoai Gieo";
        
        [TextArea(2, 4)]
        public string Description = "Khoai lang thái lát phơi khô nắng giòn đặc sản miền Trung, dai dẻo ngọt bùi, trữ cực lâu không mốc.";

        public ItemType type = ItemType.NongSanKho;

        public Sprite Icon;

        [Header("--- CHỈ SỐ SINH TỒN ---")]
        [Tooltip("Lượng thể lực hồi phục khi ăn/uống vật phẩm này.")]
        public float StaminaRestoreValue = 25f;

        [Tooltip("Lượng tinh thần (Morale) hồi phục khi dùng (Ví dụ: uống chè xanh ấm, cúng tế nhang).")]
        public float MoraleRestoreValue = 10f;

        [Tooltip("Tỷ lệ bị thối mốc mỗi ngày khi độ ẩm không khí tăng cao trong mùa lũ (0f - 1f). 0f = vĩnh viễn.")]
        public float DecayRateInHumidity = 0f; // Khoai gieo phơi khô có DecayRate = 0%!
    }
}
