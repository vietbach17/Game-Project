using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SownInStone.Editor
{
    /// <summary>
    /// Editor script to automate setting up Thành's house (the player's house),
    /// auto-scaling it, correcting pivot offsets to place it on the ground,
    /// rotating it to face the player, and adding a BoxCollider.
    /// </summary>
    public class SetupThanhHouse
    {
        [MenuItem("Sown In Stone/Setup Thanh House")]
        public static void Setup()
        {
            // File paths for Thanh's House
            string fbxPath = "Assets/Prefabs/Thanh_House/Meshy_AI_Stylized_low_poly_3D__0620084846_texture.fbx";
            string texPath = "Assets/Prefabs/Thanh_House/Meshy_AI_Stylized_low_poly_3D__0620084846_texture.png";
            string matPath = "Assets/Prefabs/Thanh_House/Mat_Thanh_House.mat";

            // 1. Create or Load Material
            Material houseMat = SetupMaterial(matPath, texPath);

            // 2. Load FBX Asset
            GameObject houseFbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (houseFbx == null)
            {
                Debug.LogError($"[SETUP THANH HOUSE] Could not load House FBX at '{fbxPath}'.");
                return;
            }

            // 3. Find and Destroy the old Thanh_House GameObject to avoid duplicates/prefab lock issues
            GameObject oldHouse = GameObject.Find("Thanh_House");
            if (oldHouse == null) oldHouse = GameObject.Find("Thanh_House (1)"); // check potential copy names

            if (oldHouse != null)
            {
                Debug.Log("[SETUP THANH HOUSE] Destroying old Thanh_House to apply clean layout...");
                Undo.DestroyObjectImmediate(oldHouse);
            }

            // Create new container under the "Houses" parent if possible
            GameObject housesParent = GameObject.Find("Houses");
            GameObject houseContainer = new GameObject("Thanh_House");
            Undo.RegisterCreatedObjectUndo(houseContainer, "Create Thanh_House Container");

            if (housesParent != null)
            {
                houseContainer.transform.SetParent(housesParent.transform);
            }

            // Position Thành's house at world (X: 10.66, Y: 0.0, Z: -10.0)
            // Rotated 180 degrees to face the player/main character
            houseContainer.transform.position = new Vector3(10.66f, 0.0f, -10.0f);
            houseContainer.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            // 4. Instantiate House Model
            GameObject houseObj = InstantiateVisual(houseFbx, "HouseModel", houseContainer.transform, houseMat);
            if (houseObj != null)
            {
                // Auto-scale house to a target height of 4.5 meters
                AutoScaleObject(houseObj, 4.5f);
                // Center house horizontally and place its bottom exactly at local Y = 0
                AlignPivotOffset(houseObj, houseContainer.transform, Vector3.zero);
            }

            // 5. Set up BoxCollider on Thanh_House covering the main house structure
            BoxCollider boxCol = houseContainer.GetComponent<BoxCollider>();
            if (boxCol == null)
            {
                boxCol = houseContainer.AddComponent<BoxCollider>();
            }
            boxCol.isTrigger = false;

            if (houseObj != null)
            {
                Renderer[] houseRenderers = houseObj.GetComponentsInChildren<Renderer>();
                if (houseRenderers.Length > 0)
                {
                    Bounds bounds = houseRenderers[0].bounds;
                    foreach (var r in houseRenderers)
                    {
                        bounds.Encapsulate(r.bounds);
                    }
                    Vector3 localCenter = houseContainer.transform.InverseTransformPoint(bounds.center);
                    boxCol.center = localCenter;
                    boxCol.size = bounds.size;
                    Debug.Log($"[SETUP THANH HOUSE] Configured House BoxCollider: Center={boxCol.center}, Size={boxCol.size}");
                }
            }

            // 6. Mark scene dirty and save
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(houseContainer);
                EditorSceneManager.MarkSceneDirty(houseContainer.scene);
                bool saved = EditorSceneManager.SaveScene(houseContainer.scene);
                AssetDatabase.SaveAssets();
                Debug.Log($"[SETUP THANH HOUSE] House setup completed successfully: {saved}");
            }
        }

        private static Material SetupMaterial(string matPath, string texPath)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
                if (litShader == null) litShader = Shader.Find("Standard");

                mat = new Material(litShader);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex != null)
                {
                    mat.SetTexture("_BaseMap", tex); // URP
                    mat.SetTexture("_MainTex", tex); // Standard
                }
                AssetDatabase.CreateAsset(mat, matPath);
                AssetDatabase.SaveAssets();
            }
            return mat;
        }

        private static GameObject InstantiateVisual(GameObject fbxAsset, string name, Transform parent, Material mat)
        {
            GameObject visualObj = null;
            if (!Application.isPlaying)
            {
                visualObj = PrefabUtility.InstantiatePrefab(fbxAsset) as GameObject;
            }
            if (visualObj == null)
            {
                visualObj = Object.Instantiate(fbxAsset);
            }

            visualObj.name = name;
            visualObj.transform.SetParent(parent);
            
            // CRITICAL: Preserve the FBX's import rotation to keep the model upright!
            visualObj.transform.localRotation = fbxAsset.transform.localRotation;
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = Vector3.one;

            // Assign Material to all MeshRenderers in the model
            var renderers = visualObj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.sharedMaterial = mat;
            }
            return visualObj;
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

            // Convert bounds center and minimum to parent's local space
            Vector3 localCenter = parentTrans.InverseTransformPoint(bounds.center);
            Vector3 localMin = parentTrans.InverseTransformPoint(bounds.min);

            // Compute the corrective offset to align horizontal center to 0 and bottom to Y = 0 relative to targetLocalPosition
            Vector3 pivotCorrection = new Vector3(
                targetLocalPosition.x - (localCenter.x - visualObj.transform.localPosition.x),
                targetLocalPosition.y - (localMin.y - visualObj.transform.localPosition.y),
                targetLocalPosition.z - (localCenter.z - visualObj.transform.localPosition.z)
            );

            visualObj.transform.localPosition = pivotCorrection;
        }
    }
}
