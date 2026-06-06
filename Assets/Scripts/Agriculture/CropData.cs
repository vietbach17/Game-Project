using UnityEngine;

namespace SownInStone.Agriculture
{
    [CreateAssetMenu(fileName = "NewCropData", menuName = "Sown In Stone/Crop Data")]
    public class CropData : ScriptableObject
    {
        [Header("--- THÔNG TIN CĂN BẢN ---")]
        public string CropName = "Khoai lang";
        
        [Tooltip("Số ngày cần để trưởng thành từ hạt giống đến khi thu hoạch.")]
        public int DaysToMature = 5;

        [Tooltip("Giá mua hạt giống từ đại lý O Thắm (tiền xu).")]
        public int SeedPrice = 10;

        [Tooltip("Giá bán nông sản thô (nếu bán ngay không trữ tích cốc).")]
        public int BaseSellValue = 25;

        [Header("--- NHU CẦU SINH TRƯỞNG ---")]
        [Range(0f, 100f)]
        [Tooltip("Độ ẩm đất tối ưu (%) giúp cây phát triển tối đa.")]
        public float IdealMoisture = 60f;

        [Range(0f, 100f)]
        [Tooltip("Độ dinh dưỡng đất tối thiểu (%) để cây không héo úa.")]
        public float RequiredNutrients = 20f;

        [Header("--- KHẢ NĂNG CHỊU ĐỰNG THIÊN TAI ---")]
        [Tooltip("Chịu được nhiệt độ tối đa (°C) trước khi bị cháy lá.")]
        public float MaxTemperatureTolerance = 39f;

        [Tooltip("Cây có thể chịu được ngập úng không? (Ví dụ: Khoai lang, lạc ngập lũ sẽ thối củ rất nhanh).")]
        public bool CanSurviveFlooding = false;

        [Header("--- NÔNG SẢN THU HOẠCH ---")]
        [Tooltip("Vật phẩm thu hoạch được khi chín.")]
        public SownInStone.Storage.ItemData HarvestedItem;

        [Header("--- HÌNH ẢNH MINH HỌA GIAI ĐOẠN ---")]
        [Tooltip("Mảng các Sprites mô tả các giai đoạn lớn lên (Ví dụ: Mầm non -> Phát triển -> Ra hoa -> Thu hoạch).")]
        public Sprite[] GrowthStageSprites;
    }
}
