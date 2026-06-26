using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SownInStone.Core;

namespace SownInStone.Interactions
{
    /// <summary>
    /// Gắn vào GameObject khu vực đá gà (gần model gà + Bé Tí).
    /// Khi player đến gần sẽ hiện UI "Đá gà không cu?".
    /// </summary>
    public class CockfightingZone : MonoBehaviour
    {
        [Header("--- CÀI ĐẶT KHU VỰC ---")]
        [Tooltip("Bán kính phát hiện player (mét).")]
        [SerializeField] private float triggerRadius = 2.5f;

        [Tooltip("Điểm hiển thị UI (đỉnh đầu gà/Bé Tí). Để trống → dùng vị trí object này.")]
        [SerializeField] private Transform uiAnchor;

        [Tooltip("Chiều cao offset UI (pixel) so với vị trí trên màn hình.")]
        [SerializeField] private float uiHeightOffset = 120f;

        // ─── Runtime ─────────────────────────────────────────────────────────
        private bool isPlayerNearby = false;
        private GameObject panelObj;
        private CanvasGroup canvasGroup;
        private CockfightingMinigame minigame;

        // ─────────────────────────────────────────────────────────────────────
        private void Start()
        {
            BuildProximityUI();

            minigame = FindAnyObjectByType<CockfightingMinigame>();
            if (minigame == null)
            {
                var go = new GameObject("CockfightingMinigame");
                minigame = go.AddComponent<CockfightingMinigame>();
            }
        }

        private void Update()
        {
            if (PlayerController.Instance == null) return;

            float dist = Vector3.Distance(
                transform.position,
                PlayerController.Instance.transform.position
            );
            isPlayerNearby = dist <= triggerRadius;

            float targetAlpha = isPlayerNearby ? 1f : 0f;
            if (canvasGroup != null)
            {
                canvasGroup.alpha          = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * 8f);
                bool active                = isPlayerNearby && canvasGroup.alpha > 0.5f;
                canvasGroup.interactable   = active;
                canvasGroup.blocksRaycasts = active;
            }

            // Nhấn phím 2 để chơi
            if (isPlayerNearby && minigame != null && canvasGroup != null && canvasGroup.alpha > 0.5f)
            {
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.digit2Key.wasPressedThisFrame)
                {
                    OnClickFight();
                }
#else
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    OnClickFight();
                }
#endif
            }

            UpdatePanelPosition();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  XÂY DỰNG UI
        // ─────────────────────────────────────────────────────────────────────
        private void BuildProximityUI()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            panelObj = new GameObject("CockfightingPromptPanel");
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(300f, 120f);
            panelRect.pivot     = new Vector2(0.5f, 0f);

            canvasGroup                    = panelObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha              = 0f;
            canvasGroup.interactable       = false;
            canvasGroup.blocksRaycasts     = false;

            Image bg  = panelObj.AddComponent<Image>();
            bg.color  = new Color(0.12f, 0.07f, 0.03f, 0.95f);

            Outline outline           = panelObj.AddComponent<Outline>();
            outline.effectColor       = new Color(0.95f, 0.75f, 0.2f, 1f);
            outline.effectDistance    = new Vector2(2f, 2f);

            VerticalLayoutGroup vl        = panelObj.AddComponent<VerticalLayoutGroup>();
            vl.padding                    = new RectOffset(14, 14, 14, 14);
            vl.spacing                    = 10f;
            vl.childAlignment             = TextAnchor.UpperCenter;
            vl.childControlHeight         = false;
            vl.childControlWidth          = true;
            vl.childForceExpandHeight     = false;
            vl.childForceExpandWidth      = true;

            // Tiêu đề
            GameObject titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI title = titleGO.AddComponent<TextMeshProUGUI>();
            title.text      = "\U0001f413  Đá gà không cu?";
            title.fontSize  = 17;
            title.fontStyle = FontStyles.Bold;
            title.color     = new Color(1f, 0.88f, 0.28f);
            title.alignment = TextAlignmentOptions.Center;
            titleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(272f, 26f);

            // Hàng nút
            GameObject btnRow = new GameObject("BtnRow");
            btnRow.transform.SetParent(panelObj.transform, false);
            btnRow.GetComponent<RectTransform>().sizeDelta = new Vector2(272f, 40f);

            HorizontalLayoutGroup hl   = btnRow.AddComponent<HorizontalLayoutGroup>();
            hl.spacing                 = 14f;
            hl.childAlignment          = TextAnchor.MiddleCenter;
            hl.childControlWidth       = false;
            hl.childControlHeight      = false;
            hl.childForceExpandWidth   = false;
            hl.childForceExpandHeight  = false;

            MakeButton(btnRow.transform, "\U0001f94a ĐÁ GÀ! [2]",
                new Color(0.82f, 0.30f, 0.08f), 138f, OnClickFight);
            MakeButton(btnRow.transform, "Thôi",
                new Color(0.28f, 0.28f, 0.28f), 90f, OnClickCancel);
        }

        private void MakeButton(Transform parent, string label, Color color,
                                 float width, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = new GameObject(label);
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 38f);

            Image img     = go.AddComponent<Image>();
            img.color     = color;

            Button btn    = go.AddComponent<Button>();
            btn.targetGraphic = img;
            ColorBlock cb = btn.colors;
            cb.highlightedColor = color * 1.3f;
            cb.pressedColor     = color * 0.7f;
            btn.colors          = cb;
            btn.onClick.AddListener(onClick);

            GameObject txtGO = new GameObject("Lbl");
            txtGO.transform.SetParent(go.transform, false);
            TextMeshProUGUI txt = txtGO.AddComponent<TextMeshProUGUI>();
            txt.text      = label;
            txt.fontSize  = 14;
            txt.fontStyle = FontStyles.Bold;
            txt.color     = Color.white;
            txt.alignment = TextAlignmentOptions.Center;

            RectTransform rt  = txtGO.GetComponent<RectTransform>();
            rt.anchorMin      = Vector2.zero;
            rt.anchorMax      = Vector2.one;
            rt.offsetMin      = Vector2.zero;
            rt.offsetMax      = Vector2.zero;
        }

        private void UpdatePanelPosition()
        {
            if (panelObj == null) return;
            Camera cam = Camera.main;
            if (cam == null) return;

            Transform anchor = (uiAnchor != null) ? uiAnchor : transform;
            Vector3 wp = anchor.position + Vector3.up * 1.5f;
            Vector3 sp = cam.WorldToScreenPoint(wp);

            if (sp.z < 0f) { panelObj.SetActive(false); return; }
            panelObj.SetActive(true);

            panelObj.GetComponent<RectTransform>().position = new Vector3(
                Mathf.Clamp(sp.x, 150f, Screen.width  - 150f),
                Mathf.Clamp(sp.y + uiHeightOffset, 65f, Screen.height - 65f),
                0f
            );
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CALLBACKS
        // ─────────────────────────────────────────────────────────────────────
        private void OnClickFight()
        {
            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
            if (minigame != null)
            {
                isPlayerNearby = false;   // ẩn panel khi mini-game mở
                minigame.StartMinigame();
            }
        }

        private void OnClickCancel()
        {
            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
            Gizmos.DrawSphere(transform.position, triggerRadius);
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
#endif
    }
}
