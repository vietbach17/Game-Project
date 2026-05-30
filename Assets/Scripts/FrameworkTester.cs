using UnityEngine;
using SownInStone.Core;
using SownInStone.Weather;
using SownInStone.Agriculture;
using SownInStone.Community;
using SownInStone.Storage;
using SownInStone.Interactions;

namespace SownInStone
{
    /// <summary>
    /// File chạy thử nghiệm tổng hợp (Test Bench) kết nối và xác minh hoạt động đồng bộ của tất cả các Manager.
    /// Giúp nhóm phát triển chạy thử nhanh trong Unity Editor và xem các log liên kết hệ thống.
    /// </summary>
    public class FrameworkTester : MonoBehaviour
    {
        [Header("--- THÀNH PHẦN KIỂM THỬ ---")]
        [Tooltip("Kéo ô đất ảo vào đây để test gieo hạt, tưới nước.")]
        [SerializeField] private SoilCell testSoilCell;

        [Tooltip("Dữ liệu hạt giống ảo.")]
        [SerializeField] private CropData testSeedData;

        [Tooltip("Vật phẩm nhang cúng ảo.")]
        [SerializeField] private ItemData testIncenseItem;

        [Tooltip("Bàn thờ tương tác ảo.")]
        [SerializeField] private AncestralAltar testAltar;

        private void Start()
        {
            Debug.Log("[FRAMEWORK TESTER] Khởi động hệ thống kiểm thử Đất Cày Lên Sỏi Đá...");

            // 1. Xác thực các Managers có tồn tại dạng Singletons hay không
            ValidateManagers();

            // 2. Đăng ký sự kiện thông báo của Player để test UI hiển thị log
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnPlayerAlert += (msg) => Debug.Log($"[ALERT] HỆ THỐNG BÁO: {msg}");
            }

            // 3. Đăng ký sự kiện cảnh báo mốc kho
            if (StorageManager.Instance != null)
            {
                StorageManager.Instance.OnStorageAlert += (msg) => Debug.LogWarning($"[TÍCH CỐC] BÁO KHO: {msg}");
            }

            // Chạy kịch bản gieo trồng ảo nếu có dữ liệu sẵn
            TestPlantingSequence();
        }

        private void ValidateManagers()
        {
            if (GameManager.Instance == null)
                Debug.LogError("[TEST] Thiếu GameManager trong Scene!");
            else
                Debug.Log($"[TEST] OK! GameManager hoạt động. Ngày hiện tại: {GameManager.Instance.CurrentDay}, Giai đoạn: {GameManager.Instance.CurrentPhase}");

            if (PlayerStats.Instance == null)
                Debug.LogError("[TEST] Thiếu PlayerStats trong Scene!");
            else
                Debug.Log("[TEST] OK! PlayerStats hoạt động.");

            if (WeatherManager.Instance == null)
                Debug.LogError("[TEST] Thiếu WeatherManager trong Scene!");
            else
                Debug.Log($"[TEST] OK! WeatherManager hoạt động. Nhiệt độ: {WeatherManager.Instance.Temperature}°C, Thời tiết: {WeatherManager.Instance.currentVisualWeather}");

            if (CommunityManager.Instance == null)
                Debug.LogError("[TEST] Thiếu CommunityManager trong Scene!");
            else
                Debug.Log("[TEST] OK! CommunityManager hoạt động.");

            if (StorageManager.Instance == null)
                Debug.LogError("[TEST] Thiếu StorageManager trong Scene!");
            else
                Debug.Log("[TEST] OK! StorageManager hoạt động.");
        }

        private void TestPlantingSequence()
        {
            if (testSoilCell == null || testSeedData == null)
            {
                Debug.Log("[TEST] Bỏ qua kịch bản test gieo trồng (Chưa gán SoilCell hoặc SeedData trong Inspector).");
                return;
            }

            Debug.Log("[TEST] Chạy thử quy trình gieo hạt...");
            // Thực hiện dọn đá trước
            testSoilCell.ActionClearRocks(25f);
            
            // Tưới nước
            testSoilCell.ActionWaterSoil(50f);
            
            // Gieo hạt
            bool plantSuccess = testSoilCell.ActionPlantCrop(testSeedData);
            if (plantSuccess)
            {
                Debug.Log("[TEST] Gieo hạt thành công! Cây đang bắt đầu sinh trưởng trên đất cằn.");
            }
        }

        /// <summary>
        /// Hàm này có thể được gọi bằng nút nhấn trong UI để giả lập chuyển Phase bão lũ khẩn cấp.
        /// </summary>
        [ContextMenu("Giả Lập Bão Lũ")]
        public void SimulateStormDisaster()
        {
            Debug.Log("[TEST] KÍCH HOẠT GIẢ LẬP THIÊN TAI BÃO LŨ!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TransitionToPhase(GamePhase.MuaBao);
            }

            if (CommunityManager.Instance != null)
            {
                // Gọi hàng xóm hỗ trợ Vần Công
                CommunityManager.Instance.TriggerStormHelpSequence();
            }
        }
    }
}
