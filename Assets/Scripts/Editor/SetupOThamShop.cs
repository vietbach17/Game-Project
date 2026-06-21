using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SownInStone.Editor
{
    /// <summary>
    /// Editor script to automate setting up O Tham's shop (both the House and the Stall),
    /// auto-scaling them, correcting pivot offsets to place them on the ground,
    /// and positioning NPC O Tham correctly between them.
    /// </summary>
    public class SetupOThamShop
    {
        [MenuItem("Sown In Stone/Setup O Tham Shop")]
        public static void Setup()
        {
            // File paths for House (first model: 0620062116)
            string houseFbxPath = "Assets/Prefabs/OTham_Shop/OTham_House_Model.fbx";
            string houseTexPath = "Assets/Prefabs/OTham_Shop/OTham_House_Texture.png";
            string houseMatPath = "Assets/Prefabs/OTham_Shop/Mat_OTham_House.mat";

            // File paths for Stall (second model: 0620065520)
            string stallFbxPath = "Assets/Prefabs/OTham_Shop/OTham_Stall_Model.fbx";
            string stallTexPath = "Assets/Prefabs/OTham_Shop/OTham_Stall_Texture.png";
            string stallMatPath = "Assets/Prefabs/OTham_Shop/Mat_OTham_Stall.mat";

            // 1. Create or Load Materials
            Material houseMat = SetupMaterial(houseMatPath, houseTexPath);
            Material stallMat = SetupMaterial(stallMatPath, stallTexPath);

            // 2. Load FBX Assets
            GameObject houseFbx = AssetDatabase.LoadAssetAtPath<GameObject>(houseFbxPath);
            GameObject stallFbx = AssetDatabase.LoadAssetAtPath<GameObject>(stallFbxPath);

            if (houseFbx == null)
            {
                Debug.LogError($"[SETUP SHOP] Could not load House FBX at '{houseFbxPath}'.");
                return;
            }
            if (stallFbx == null)
            {
                Debug.LogError($"[SETUP SHOP] Could not load Stall FBX at '{stallFbxPath}'.");
                return;
            }

            // 3. Find and Destroy the old OTham_Shop GameObject to avoid prefab lock issues
            GameObject oldShop = GameObject.Find("OTham_Shop");
            if (oldShop != null)
            {
                Debug.Log("[SETUP SHOP] Destroying old OTham_Shop to apply clean layout...");
                Undo.DestroyObjectImmediate(oldShop);
            }

            GameObject shopObj = new GameObject("OTham_Shop");
            Undo.RegisterCreatedObjectUndo(shopObj, "Create OTham_Shop");

            // Position shop in the empty space left of Thanh's house (X: 4.5, Z: -10)
            shopObj.transform.position = new Vector3(4.5f, 0.0f, -10.0f);
            shopObj.transform.rotation = Quaternion.identity;

            // 4. Instantiate House
            GameObject houseObj = InstantiateVisual(houseFbx, "HouseModel", shopObj.transform, houseMat);
            if (houseObj != null)
            {
                // Auto-scale house to a height of 4.5 meters
                AutoScaleObject(houseObj, 4.5f);
                // Center house and place its bottom exactly at Y = 0
                AlignPivotOffset(houseObj, shopObj.transform, Vector3.zero);
            }

            // 5. Instantiate Stall (placed 2.5 meters in front of the house)
            GameObject stallObj = InstantiateVisual(stallFbx, "StallModel", shopObj.transform, stallMat);
            if (stallObj != null)
            {
                // Auto-scale stall to a height of 1.2 meters
                AutoScaleObject(stallObj, 1.2f);
                // Center stall and place its bottom exactly at Y = 0, shifted forward to Z = -2.5
                AlignPivotOffset(stallObj, shopObj.transform, new Vector3(0f, 0f, -2.5f));
            }

            // 6. Set up BoxCollider on OTham_Shop covering the house structure
            BoxCollider boxCol = shopObj.GetComponent<BoxCollider>();
            if (boxCol == null)
            {
                boxCol = shopObj.AddComponent<BoxCollider>();
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
                    Vector3 localCenter = shopObj.transform.InverseTransformPoint(bounds.center);
                    boxCol.center = localCenter;
                    boxCol.size = bounds.size;
                    Debug.Log($"[SETUP SHOP] Configured Shop BoxCollider: Center={boxCol.center}, Size={boxCol.size}");
                }
            }

            // 7. Reposition NPC O Thắm between the house and the stall (Z = -11.2)
            GameObject npcOTham = GameObject.Find("NPC_OTham");
            if (npcOTham == null) npcOTham = GameObject.Find("NPC_OTham (O Thắm)");
            if (npcOTham != null)
            {
                // Positioned right behind the stall (Z = -11.2) facing forward (Y rotation = 180)
                npcOTham.transform.position = new Vector3(4.5f, 0.5f, -11.2f);
                npcOTham.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                // Auto-scale O Tham to a natural height of 1.7 meters
                Transform npcVisual = npcOTham.transform.Find("Visual");
                if (npcVisual != null)
                {
                    AutoScaleObject(npcVisual.gameObject, 1.7f);
                    // Center O Tham horizontally and align her feet to local Y = -0.5f (world Y = 0)
                    AlignPivotOffset(npcVisual.gameObject, npcOTham.transform, new Vector3(0f, -0.5f, 0f));
                }

                // Adjust trigger collider of NPC O Tham to cover the shop counter area
                BoxCollider npcBox = npcOTham.GetComponent<BoxCollider>();
                if (npcBox != null)
                {
                    npcBox.center = new Vector3(0f, 1f, 1.2f); // Move center forward to the stall front
                    npcBox.size = new Vector3(3f, 2f, 3f);     // Expand trigger radius
                }
                Debug.Log("[SETUP SHOP] Positioned NPC_OTham, scaled her, and aligned interaction bounds.");
            }

            // 8. Mark scene dirty and save
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(shopObj);
                EditorSceneManager.MarkSceneDirty(shopObj.scene);
                bool saved = EditorSceneManager.SaveScene(shopObj.scene);
                AssetDatabase.SaveAssets();
                Debug.Log($"[SETUP SHOP] Shop setup completed successfully: {saved}");
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
