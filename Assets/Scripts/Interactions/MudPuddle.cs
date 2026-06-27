using UnityEngine;
using SownInStone.Core;

namespace SownInStone.Interactions
{
    /// <summary>
    /// Vũng bùn lầy (Mud Puddle) left behind by the flood.
    /// The player can clear it using a shovel/hoe to clean up the village path, consuming stamina.
    /// </summary>
    public class MudPuddle : MonoBehaviour
    {
        [Header("Settings")]
        public float staminaCost = 15f;
        public float clearDuration = 1.5f;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPhaseChanged += OnPhaseChangedHandler;
                UpdateVisibility(GameManager.Instance.CurrentPhase);
            }
            else
            {
                UpdateVisibility(GamePhase.LapNghiep);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPhaseChanged -= OnPhaseChangedHandler;
            }
        }

        private void OnPhaseChangedHandler(GamePhase newPhase)
        {
            UpdateVisibility(newPhase);
        }

        private void UpdateVisibility(GamePhase phase)
        {
            // Chỉ xuất hiện ở giai đoạn Phù Sa (sau mùa bão lũ)
            bool shouldBeVisible = (phase == GamePhase.PhuSa);

            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.enabled = shouldBeVisible;
            }

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = shouldBeVisible;
            }
        }

        public void Interact()
        {
            if (PlayerStats.Instance == null || PlayerController.Instance == null) return;

            // Check if player has enough stamina
            if (PlayerStats.Instance.CurrentStamina < staminaCost)
            {
                PlayerStats.Instance.TriggerAlert("Bạn không đủ thể lực để dọn dẹp vũng bùn lầy này!");
                return;
            }

            // Perform clear action
            PlayerStats.Instance.ModifyStamina(-staminaCost);
            
            // Play animation and lock player movement
            PlayerController.Instance.SetAnimTrigger("Dig");
            PlayerController.Instance.LockMovement(clearDuration);

            // Play SFX
            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_dig");

            // Alert toast
            PlayerStats.Instance.TriggerAlert("Đã dọn dẹp sạch vũng bùn đất lầy!");

            // Destroy this mud puddle
            Destroy(gameObject);
        }
    }
}
