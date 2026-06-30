using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using SownInStone.UI;

namespace SownInStone.Editor
{
    public class SetupEndingUI
    {
        [MenuItem("Sown In Stone/Setup Ending UI")]
        public static void Setup()
        {
            // 1. Find or create Canvas
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            }

            // 2. Find or create EventSystem
            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                eventSystem = esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
            }

            // 3. Find EndingManager
            EndingManager endingManager = Object.FindAnyObjectByType<EndingManager>();
            if (endingManager == null)
            {
                GameObject managerObj = GameObject.Find("_Managers");
                if (managerObj == null)
                {
                    managerObj = new GameObject("_Managers");
                    Undo.RegisterCreatedObjectUndo(managerObj, "Create _Managers");
                }
                endingManager = managerObj.AddComponent<EndingManager>();
            }
            else
            {
                Undo.RecordObject(endingManager, "Update EndingManager");
            }

            // 4. Create EndingPanel
            Transform existingPanel = canvas.transform.Find("EndingPanel");
            if (existingPanel != null)
            {
                Undo.DestroyObjectImmediate(existingPanel.gameObject);
            }

            GameObject endingPanel = new GameObject("EndingPanel");
            endingPanel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = endingPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = endingPanel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f); // Dark background

            // 5. Create TitleText
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(endingPanel.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.8f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "KẾT CỤC";
            titleText.fontSize = 50;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            titleText.enableAutoSizing = true;
            titleText.fontSizeMin = 24;
            titleText.fontSizeMax = 60;
            titleText.fontStyle = FontStyles.Bold;

            // 6. Create DescriptionText
            GameObject descObj = new GameObject("DescriptionText");
            descObj.transform.SetParent(endingPanel.transform, false);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.1f, 0.3f);
            descRect.anchorMax = new Vector2(0.9f, 0.75f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Mô tả kết cục ở đây...";
            descText.fontSize = 28;
            descText.alignment = TextAlignmentOptions.Center;
            descText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            descText.enableWordWrapping = true;

            // 7. Create Restart Button
            GameObject restartBtnObj = CreateButton("RestartButton", endingPanel.transform, new Vector2(0.2f, 0.1f), new Vector2(0.4f, 0.2f), "CHƠI LẠI", new Color(0.18f, 0.8f, 0.44f));
            Button restartButton = restartBtnObj.GetComponent<Button>();

            // 8. Create Exit Button
            GameObject exitBtnObj = CreateButton("ExitButton", endingPanel.transform, new Vector2(0.6f, 0.1f), new Vector2(0.8f, 0.2f), "THOÁT GAME", new Color(0.9f, 0.3f, 0.23f));
            Button exitButton = exitBtnObj.GetComponent<Button>();

            // 9. Assign References
            SerializedObject so = new SerializedObject(endingManager);
            so.FindProperty("endingPanel").objectReferenceValue = endingPanel;
            so.FindProperty("endingTitleText").objectReferenceValue = titleText;
            so.FindProperty("endingDescriptionText").objectReferenceValue = descText;
            so.FindProperty("restartButton").objectReferenceValue = restartButton;
            so.FindProperty("exitButton").objectReferenceValue = exitButton;
            so.ApplyModifiedProperties();

            // Set inactive by default
            endingPanel.SetActive(false);

            Undo.RegisterCreatedObjectUndo(endingPanel, "Create Ending UI");
            
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
                Debug.Log("[SETUP ENDING UI] Setup hoàn tất! Bạn có thể xem thay đổi trong Canvas.");
            }
        }

        private static GameObject CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string buttonText, Color btnColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = btnColor;
            
            Button button = btnObj.AddComponent<Button>();
            button.targetGraphic = btnImage;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = buttonText;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;

            return btnObj;
        }
    }
}
