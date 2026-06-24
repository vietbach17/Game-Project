using System;
using UnityEngine;

namespace SownInStone
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        private bool isShowing = false;
        private int currentSlide = 0;
        private Action onComplete;
        private Texture2D[] introImages;
        private Texture2D[] slideImages;
        private string[] slideTexts = new string[]
        {
            "Cách di chuyển: W, A, S, D. Nhấn [E] để tương tác.",
            "Cải tạo đất: Nhặt đá, tưới nước, ủ phân.",
            "Gieo hạt và tưới nước để trồng cây.",
            "Tương tác với NPC: O Thắm hoặc Bác Năm.",
            "Sinh tồn khi lũ ngập: Tránh nước và tìm nơi cao."
        };
        private readonly string[] imageNames = new string[]
        {
            "tutorial_intro",
            "tutorial_clear_rocks",
            "tutorial_plant_water",
            "tutorial_npc_interact",
            "tutorial_flood_survival"
        };
        public bool IsTutorialShown { get; private set; } = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadImages();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadImages()
        {
            // Load 5 intro movement images
            introImages = new Texture2D[5];
            for (int i = 0; i < 5; i++)
            {
                introImages[i] = Resources.Load<Texture2D>($"Textures/Tutorial/tutorial_intro/{i}");
            }

            // Load other slides from Resources
            slideImages = new Texture2D[imageNames.Length];
            for (int i = 0; i < imageNames.Length; i++)
            {
                if (i == 0)
                {
                    // Fallback to first frame of intro
                    slideImages[i] = introImages[0];
                }
                else
                {
                    slideImages[i] = Resources.Load<Texture2D>($"Textures/Tutorial/{imageNames[i]}");
                }
            }
        }

        public void ShowTutorial(Action onCompleteCallback)
        {
            if (isShowing) return;
            onComplete = onCompleteCallback;
            isShowing = true;
            currentSlide = 0;
            // Pause game time while tutorial is displayed
            Time.timeScale = 0f;
        }

        private void OnGUI()
        {
            if (!isShowing) return;

            // Dark overlay background
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Panel for tutorial content
            float panelWidth = Screen.width * 0.8f;
            float panelHeight = Screen.height * 0.8f;
            float panelX = (Screen.width - panelWidth) / 2f;
            float panelY = (Screen.height - panelHeight) / 2f;
            Rect panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);

            // Draw inner background
            GUI.color = new Color(0.12f, 0.1f, 0.08f, 0.95f);
            GUI.DrawTexture(panelRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Image area
            float imageHeight = panelHeight * 0.55f;
            Rect imageRect = new Rect(panelX + 20, panelY + 20, panelWidth - 40, imageHeight);
            Texture2D img = null;
            if (currentSlide == 0 && introImages != null && introImages.Length > 0)
            {
                int frame = Mathf.FloorToInt(Time.unscaledTime * 3f) % introImages.Length;
                img = introImages[frame] ?? slideImages[currentSlide];
            }
            else
            {
                img = slideImages[currentSlide];
            }

            if (img != null)
            {
                GUI.DrawTexture(imageRect, img, ScaleMode.ScaleToFit);
            }
            else
            {
                // Placeholder rectangle with label
                GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                GUI.DrawTexture(imageRect, Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(imageRect, "[Placeholder Image]", new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = 24, normal = { textColor = Color.white } });
            }

            // Text description below image
            Rect textRect = new Rect(panelX + 20, panelY + 20 + imageHeight + 10, panelWidth - 40, 80);
            GUIStyle textStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                normal = { textColor = new Color(0.9f, 0.85f, 0.7f) },
                wordWrap = true
            };
            GUI.Label(textRect, slideTexts[currentSlide], textStyle);

            // Navigation buttons
            float btnWidth = 120f;
            float btnHeight = 40f;
            float btnY = panelY + panelHeight - btnHeight - 20;
            float spacing = 20f;
            // Back button
            if (GUI.Button(new Rect(panelX + spacing, btnY, btnWidth, btnHeight), "< Quay Lại"))
            {
                if (currentSlide > 0) currentSlide--;
            }
            // Next button
            if (GUI.Button(new Rect(panelX + panelWidth - btnWidth - spacing, btnY, btnWidth, btnHeight), "Tiếp Theo >"))
            {
                if (currentSlide < slideTexts.Length - 1)
                {
                    currentSlide++;
                }
                else
                {
                    EndTutorial();
                }
            }
            // Skip button (centered)
            if (GUI.Button(new Rect(panelX + (panelWidth - btnWidth) / 2f, btnY, btnWidth, btnHeight), "Bỏ Qua"))
            {
                EndTutorial();
            }
        }

        private void EndTutorial()
        {
            isShowing = false;
            IsTutorialShown = true;
            // Resume game time
            Time.timeScale = 1f;
            onComplete?.Invoke();
        }
    }
}
