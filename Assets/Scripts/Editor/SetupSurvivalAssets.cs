using UnityEngine;
using UnityEditor;
using System.IO;
using SownInStone.Storage;
using SownInStone.Interactions;

namespace SownInStone.Editor
{
    /// <summary>
    /// Editor script to automate setting up prefabs and ItemData for Sandbags, Flood Boards, Nón Lá, and Mud Puddles.
    /// Creates materials, builds prefabs with proper scaling/colliders, and generates scriptable assets.
    /// </summary>
    public class SetupSurvivalAssets
    {
        [MenuItem("Sown In Stone/Setup Survival Assets")]
        public static void Setup()
        {
            Debug.Log("[SETUP ASSETS] Starting setup for Sandbag, Flood Board, Nón Lá, and Mud Puddle...");

            // Ensure directories exist
            EnsureDirectory("Assets/Resources/Prefabs");
            EnsureDirectory("Assets/Data");

            // 1. Setup Materials
            Material sandbagMat = SetupMaterial("Assets/Prefabs/Sandbag/Mat_Sandbag.mat", "Assets/Prefabs/Sandbag/Sandbag_Texture.png");
            Material floodBoardMat = SetupMaterial("Assets/Prefabs/FloodBoard/Mat_FloodBoard.mat", "Assets/Prefabs/FloodBoard/FloodBoard_Texture.png");
            Material nonLaMat = SetupMaterial("Assets/Prefabs/NonLa/Mat_NonLa.mat", "Assets/Prefabs/NonLa/NonLa_Texture.png");
            Material mudPuddleMat = SetupMaterial("Assets/Prefabs/MudPuddle/Mat_MudPuddle.mat", "Assets/Prefabs/MudPuddle/MudPuddle_Texture.png");

            // 2. Load FBX Assets
            GameObject sandbagFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Sandbag/Sandbag_Model.fbx");
            GameObject floodBoardFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FloodBoard/FloodBoard_Model.fbx");
            GameObject nonLaFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NonLa/NonLa_Model.fbx");
            GameObject mudPuddleFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MudPuddle/MudPuddle_Model.fbx");

            if (sandbagFbx == null || floodBoardFbx == null || nonLaFbx == null || mudPuddleFbx == null)
            {
                Debug.LogError($"[SETUP ASSETS] Could not load all FBX models. Sandbag: {sandbagFbx != null}, FloodBoard: {floodBoardFbx != null}, NonLa: {nonLaFbx != null}, MudPuddle: {mudPuddleFbx != null}");
                return;
            }

            // 3. Create Prefabs in Resources for runtime loading
            CreateSandbagPrefab(sandbagFbx, sandbagMat);
            CreateFloodBoardPrefab(floodBoardFbx, floodBoardMat);
            CreateNonLaPrefab(nonLaFbx, nonLaMat);
            CreateMudPuddlePrefab(mudPuddleFbx, mudPuddleMat);

            // 4. Create ScriptableObject Items
            CreateItemAsset("item_sandbag", "Bao Cát Chống Lũ", "Bao cát chắc chắn dùng để tạo đê chắn nước lũ dâng cao, bảo vệ ô ruộng đất khỏi ngập úng.", ItemType.VatLieu, "Assets/Prefabs/Sandbag/Sandbag_Texture.png");
            CreateItemAsset("item_flood_board", "Tấm Chắn Lũ", "Tấm gỗ ép kiên cố dùng để ghép đập ngăn dòng nước lụt tràn qua sân vườn, bảo vệ hoa màu.", ItemType.VatLieu, "Assets/Prefabs/FloodBoard/FloodBoard_Texture.png");
            CreateItemAsset("item_non_la", "Nón Lá Truyền Thống", "Nón lá truyền thống che nắng che mưa, giúp giảm đi một nửa sự mất sức và mất nước dưới nắng nóng Gió Lào.", ItemType.VatLieu, "Assets/Prefabs/NonLa/NonLa_Texture.png");

            // 5. Place Mud Puddles in SampleScene if in edit mode
            PlaceMudPuddlesInScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SETUP ASSETS] All survival assets, prefabs, and items setup completed successfully!");
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static Material SetupMaterial(string matPath, string texPath)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");

                mat = new Material(shader);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex != null)
                {
                    mat.SetTexture("_BaseMap", tex);
                    mat.SetTexture("_MainTex", tex);
                }
                AssetDatabase.CreateAsset(mat, matPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[SETUP ASSETS] Created material: {matPath}");
            }
            return mat;
        }

        private static void CreateSandbagPrefab(GameObject fbx, Material mat)
        {
            string prefabPath = "Assets/Resources/Prefabs/Sandbag.prefab";
            GameObject container = new GameObject("Sandbag");
            GameObject visual = Object.Instantiate(fbx, container.transform);
            visual.name = "VisualModel";
            AssignMaterial(visual, mat);

            // Setup scale and pivot
            AutoScaleObject(visual, 0.4f);
            AlignPivotOffset(visual, container.transform, Vector3.zero);

            // Add scripts and colliders
            BoxCollider col = container.AddComponent<BoxCollider>();
            col.isTrigger = false;
            SetupColliderFromRenderers(container, col);

            FloodBarrier barrier = container.AddComponent<FloodBarrier>();
            barrier.isSandbag = true;
            barrier.protectionRadius = 2.2f;

            PrefabUtility.SaveAsPrefabAsset(container, prefabPath);
            Object.DestroyImmediate(container);
            Debug.Log($"[SETUP ASSETS] Created Sandbag Prefab at '{prefabPath}'");
        }

        private static void CreateFloodBoardPrefab(GameObject fbx, Material mat)
        {
            string prefabPath = "Assets/Resources/Prefabs/FloodBoard.prefab";
            GameObject container = new GameObject("FloodBoard");
            GameObject visual = Object.Instantiate(fbx, container.transform);
            visual.name = "VisualModel";
            AssignMaterial(visual, mat);

            // Setup scale and pivot
            AutoScaleObject(visual, 0.8f);
            AlignPivotOffset(visual, container.transform, Vector3.zero);

            // Add scripts and colliders
            BoxCollider col = container.AddComponent<BoxCollider>();
            col.isTrigger = false;
            SetupColliderFromRenderers(container, col);

            FloodBarrier barrier = container.AddComponent<FloodBarrier>();
            barrier.isSandbag = false;
            barrier.protectionRadius = 2.2f;

            PrefabUtility.SaveAsPrefabAsset(container, prefabPath);
            Object.DestroyImmediate(container);
            Debug.Log($"[SETUP ASSETS] Created FloodBoard Prefab at '{prefabPath}'");
        }

        private static void CreateNonLaPrefab(GameObject fbx, Material mat)
        {
            string prefabPath = "Assets/Resources/Prefabs/NonLa.prefab";
            GameObject container = new GameObject("NonLa");
            GameObject visual = Object.Instantiate(fbx, container.transform);
            visual.name = "VisualModel";
            AssignMaterial(visual, mat);

            // Setup scale (proportional diameter around 0.45m)
            AutoScaleObject(visual, 0.2f);
            AlignPivotOffset(visual, container.transform, Vector3.zero);

            PrefabUtility.SaveAsPrefabAsset(container, prefabPath);
            Object.DestroyImmediate(container);
            Debug.Log($"[SETUP ASSETS] Created NonLa Prefab at '{prefabPath}'");
        }

        private static void CreateMudPuddlePrefab(GameObject fbx, Material mat)
        {
            string prefabPath = "Assets/Resources/Prefabs/MudPuddle.prefab";
            GameObject container = new GameObject("MudPuddle");
            GameObject visual = Object.Instantiate(fbx, container.transform);
            visual.name = "VisualModel";
            AssignMaterial(visual, mat);

            // Setup scale and pivot (make it clearly visible and sit on top of the ground)
            AutoScaleObject(visual, 0.22f);
            AlignPivotOffset(visual, container.transform, Vector3.zero);

            // Add BoxCollider (trigger so player can step in to trigger prompt)
            BoxCollider col = container.AddComponent<BoxCollider>();
            col.isTrigger = true;
            SetupColliderFromRenderers(container, col);
            col.size = new Vector3(col.size.x * 1.5f, 0.5f, col.size.z * 1.5f); // Expand trigger zone horizontally

            container.AddComponent<MudPuddle>();

            PrefabUtility.SaveAsPrefabAsset(container, prefabPath);
            Object.DestroyImmediate(container);
            Debug.Log($"[SETUP ASSETS] Created MudPuddle Prefab at '{prefabPath}'");
        }


        private static void CreateItemAsset(string id, string name, string desc, ItemType type, string texPath)
        {
            string assetPath = $"Assets/Data/Item_{id.Replace("item_", "")}.asset";
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemData>();
                item.ItemID = id;
                item.ItemName = name;
                item.Description = desc;
                item.type = type;
                item.DecayRateInHumidity = 0f;
                item.StaminaRestoreValue = 0f;
                item.MoraleRestoreValue = 0f;

                // Load custom icon if png can be converted to sprite
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex != null)
                {
                    // Try to load as sprite
                    Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
                    if (icon != null)
                    {
                        item.Icon = icon;
                    }
                }

                AssetDatabase.CreateAsset(item, assetPath);
                Debug.Log($"[SETUP ASSETS] Created ItemData asset: {assetPath}");
            }
        }

        private static void AssignMaterial(GameObject obj, Material mat)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.sharedMaterial = mat;
            }
        }

        private static void AutoScaleObject(GameObject visualObj, float targetHeight)
        {
            visualObj.transform.localScale = Vector3.one;
            Renderer[] renderers = visualObj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            float currentHeight = bounds.size.y;
            if (currentHeight > 0.01f)
            {
                float scaleMultiplier = targetHeight / currentHeight;
                visualObj.transform.localScale = Vector3.one * scaleMultiplier;
            }
        }

        private static void AlignPivotOffset(GameObject visualObj, Transform parentTrans, Vector3 targetLocalPosition)
        {
            Renderer[] renderers = visualObj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            Vector3 localCenter = parentTrans.InverseTransformPoint(bounds.center);
            Vector3 localMin = parentTrans.InverseTransformPoint(bounds.min);

            Vector3 pivotCorrection = new Vector3(
                targetLocalPosition.x - (localCenter.x - visualObj.transform.localPosition.x),
                targetLocalPosition.y - (localMin.y - visualObj.transform.localPosition.y),
                targetLocalPosition.z - (localCenter.z - visualObj.transform.localPosition.z)
            );

            visualObj.transform.localPosition = pivotCorrection;
        }

        private static void SetupColliderFromRenderers(GameObject container, BoxCollider col)
        {
            Renderer[] renderers = container.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            col.center = container.transform.InverseTransformPoint(bounds.center);
            col.size = bounds.size;
        }

        private static void PlaceMudPuddlesInScene()
        {
            // Spawn 3 mud puddles in front of the house/road in SampleScene
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/MudPuddle.prefab");
            if (prefab == null) return;

            // Dọn dẹp các vũng bùn cũ nếu có để cập nhật tọa độ chiều cao mới khi chạy lại Setup
            GameObject existingRoot = GameObject.Find("MudPuddles");
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot);
            }
            GameObject existingSingle = GameObject.Find("MudPuddle");
            if (existingSingle != null)
            {
                Undo.DestroyObjectImmediate(existingSingle);
            }

            GameObject root = new GameObject("MudPuddles");
            Undo.RegisterCreatedObjectUndo(root, "Create MudPuddles Root");

            // Tọa độ Y nâng lên 0.07f để khớp với cao độ mặt đất/grass_ground và tránh bị chìm dưới cỏ
            Vector3[] positions = new Vector3[]
            {
                new Vector3(4.5f, 0.07f, -8f),
                new Vector3(7.5f, 0.07f, -10f),
                new Vector3(9f, 0.07f, -7.5f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                instance.name = $"MudPuddle_{i + 1}";
                instance.transform.SetParent(root.transform);
                instance.transform.position = positions[i];
                Undo.RegisterCreatedObjectUndo(instance, "Place MudPuddle");
            }

            Debug.Log("[SETUP ASSETS] Spawned 3 Mud Puddles in the scene at Y = 0.07f.");
        }
    }
}
