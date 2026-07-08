using UnityEngine;
using SownInStone.Core;
using SownInStone.Storage;

namespace SownInStone.Interactions
{
    /// <summary>
    /// Bàn thờ Tổ tiên trong nhà hoặc Am thờ Thổ Địa ngoài sân vườn.
    /// Tương tác thắp nhang giúp phục hồi Morale (Tinh thần) và giảm hoảng sợ cho người chơi trước bão lũ.
    /// </summary>
    public class AncestralAltar : MonoBehaviour
    {
        [Header("--- THÔNG TIN TƯƠNG TÁC ---")]
        public string AltarName = "Bàn thờ Gia tiên";
        
        [Tooltip("Vật phẩm nhang cần có trong kho để thắp.")]
        [SerializeField] private ItemData incenseItem;

        [Header("--- TRẠNG THÁI KHÓI NHANG ---")]
        [Tooltip("Số giờ game nhang sẽ cháy sau khi thắp.")]
        [SerializeField] private float burnDurationHours = 3f;
        
        [Tooltip("Thời gian nhang còn cháy lại (giờ game).")]
        private float remainingBurnTime = 0f;

        [Header("--- HIỆU ỨNG TÁC ĐỘNG ---")]
        [Tooltip("Tốc độ phục hồi Tinh thần (Morale) mỗi giây thực tế khi nhang đang cháy.")]
        [SerializeField] private float moraleRestoreRate = 2f;

        [Tooltip("Hạt khói nhang (Unity Particle System).")]
        [SerializeField] private ParticleSystem smokeParticles;

#if UNITY_EDITOR
        private void Awake()
        {
            if (incenseItem == null)
            {
                incenseItem = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Item_Incense.asset");
            }
        }
#endif

        private void Start()
        {
            if (smokeParticles != null)
            {
                smokeParticles.Stop();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHourChanged += OnHourTick;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHourChanged -= OnHourTick;
            }
        }

        private void Update()
        {
            // Nếu nhang đang cháy, phục hồi tinh thần cho người chơi
            if (remainingBurnTime > 0f)
            {
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.ModifyMorale(moraleRestoreRate * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Hành động tương tác thắp nhang từ người chơi.
        /// </summary>
        public bool ActionBurnIncense()
        {
            if (remainingBurnTime > 0f)
            {
                Debug.Log($"[{AltarName}] Nhang vẫn đang cháy ấm cúng, khói thơm nghi ngút.");
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("Nhang vẫn đang cháy ấm cúng, khói thơm nghi ngút.");
                }
                return false;
            }

            // Kiểm tra và khấu trừ 1 cây nhang trong kho đồ
            if (StorageManager.Instance != null && incenseItem != null)
            {
                if (StorageManager.Instance.RemoveItem(incenseItem, 1))
                {
                    remainingBurnTime = burnDurationHours;
                    
                    if (smokeParticles != null)
                    {
                        smokeParticles.Play();
                    }

                    // Tăng nóng hổi 10 điểm tinh thần ngay lập tức khi thắp nhang
                    if (PlayerStats.Instance != null)
                    {
                        PlayerStats.Instance.ModifyMorale(10f);
                    }
                    
                    SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_altar");
                    Debug.Log($"[{AltarName}] Bạn thắp nhang cúi đầu cầu nguyện bình an. Khói nhang tỏa hương ấm cúng, xua tan hoảng sợ bão giông.");
                    
                    if (SownInStone.UI.SurvivalUIManager.Instance != null)
                    {
                        SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("Bạn thắp nhang thành kính cầu nguyện tổ tiên. +10 Tinh thần");
                    }
                    
                    if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.LapNghiep)
                    {
                        if (PlayerController.Instance != null)
                        {
                            PlayerController.Instance.IsPerformingAction = true;
                        }

                        var cutscenePlayer = gameObject.AddComponent<SownInStone.UI.IntroVideoPlayer>();
                        cutscenePlayer.PlayVideoClip("UI/storm_cutscene", () =>
                        {
                            if (PlayerController.Instance != null)
                            {
                                PlayerController.Instance.IsPerformingAction = false;
                            }
                            // Chuyển sang Phase 2: Chuẩn bị bão — KHÔNG gọi OnAltarWorshipped()
                            // vì hàm đó sẽ gọi StartRescuingNPCsStage() → TransitionToPhase(MuaBao) làm nhảy thẳng Phase 3.
                            GameManager.Instance.TransitionToPhase(GamePhase.ChuanBiBao);
                            
                            if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                            {
                                // Khởi động nhiệm vụ chuẩn bị bão Phase 2 cho tutorial
                                TutorialManager.Instance.currentStage = TutorialManager.TutorialStage.PrepareForStorm;
                                TutorialManager.Instance.InitPhase2();
                            }
                        });
                        return true;
                    }

                    if (TutorialManager.Instance != null && TutorialManager.Instance.isTutorialActive)
                    {
                        TutorialManager.Instance.OnAltarWorshipped();
                    }
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[{AltarName}] Bạn không có Nhang (Incense) để thắp cúng lễ!");
                    if (SownInStone.UI.SurvivalUIManager.Instance != null)
                    {
                        SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("Không đủ Nhang cúng để thực hiện hành động này!");
                    }
                }
            }
            else
            {
                // Fallback check if incenseItem is somehow not initialized
                if (SownInStone.UI.SurvivalUIManager.Instance != null)
                {
                    SownInStone.UI.SurvivalUIManager.Instance.ShowHUDToast("Không đủ Nhang cúng để thực hiện hành động này!");
                }
            }
            return false;
        }

        /// <summary>
        /// Tick mỗi giờ game trôi qua để giảm thời gian cháy của nhang.
        /// </summary>
        private void OnHourTick(int currentHour)
        {
            if (remainingBurnTime > 0f)
            {
                remainingBurnTime = Mathf.Max(0f, remainingBurnTime - 1f);
                if (remainingBurnTime <= 0f)
                {
                    if (smokeParticles != null)
                    {
                        smokeParticles.Stop();
                    }
                    Debug.Log($"[{AltarName}] Nhang đã tàn.");
                }
            }
        }

        public bool IsIncenseBurning => remainingBurnTime > 0f;
        public ItemData IncenseItem => incenseItem;
    }
}
