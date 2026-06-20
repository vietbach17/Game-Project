using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SownInStone.Editor
{
    /// <summary>
    /// Editor script to automate setting up Bác Năm's house and his bamboo daybed (chõng tre),
    /// auto-scaling them, correcting pivot offsets to place them on the ground,
    /// and positioning NPC Bác Năm correctly near the daybed.
    /// </summary>
    public class SetupBacNamHouse
    {
        [MenuItem("Sown In Stone/Setup Bac Nam House")]
        public static void Setup()
        {
            // File paths for House
            string houseFbxPath = "Assets/Prefabs/BacNam_House/BacNam_House_Model.fbx";
            string houseTexPath = "Assets/Prefabs/BacNam_House/BacNam_House_Texture.png";
            string houseMatPath = "Assets/Prefabs/BacNam_House/Mat_BacNam_House.mat";

            // File paths for Bamboo Daybed (Chõng tre)
            string daybedFbxPath = "Assets/Prefabs/BacNam_House/BacNam_Daybed_Model.fbx";
            string daybedTexPath = "Assets/Prefabs/BacNam_House/BacNam_Daybed_Texture.png";
            string daybedMatPath = "Assets/Prefabs/BacNam_House/Mat_BacNam_Daybed.mat";

            // 1. Create or Load Materials
            Material houseMat = SetupMaterial(houseMatPath, houseTexPath);
            Material daybedMat = SetupMaterial(daybedMatPath, daybedTexPath);

            // 2. Load FBX Assets
            GameObject houseFbx = AssetDatabase.LoadAssetAtPath<GameObject>(houseFbxPath);
            GameObject daybedFbx = AssetDatabase.LoadAssetAtPath<GameObject>(daybedFbxPath);

            if (houseFbx == null)
            {
                Debug.LogError($"[SETUP BAC NAM] Could not load House FBX at '{houseFbxPath}'.");
                return;
            }
            if (daybedFbx == null)
            {
                Debug.LogError($"[SETUP BAC NAM] Could not load Daybed FBX at '{daybedFbxPath}'.");
                return;
            }

            // 3. Find and Destroy the old BacNam_House GameObject to avoid duplicates/prefab lock issues
            GameObject oldHouse = GameObject.Find("BacNam_House");
            if (oldHouse == null) oldHouse = GameObject.Find("BacNam_House (1)"); // check potential copy names
            
            if (oldHouse != null)
            {
                Debug.Log("[SETUP BAC NAM] Destroying old BacNam_House to apply clean layout...");
                Undo.DestroyObjectImmediate(oldHouse);
            }

            // Create new container under the "Houses" parent if possible
            GameObject housesParent = GameObject.Find("Houses");
            GameObject houseContainer = new GameObject("BacNam_House");
            Undo.RegisterCreatedObjectUndo(houseContainer, "Create BacNam_House Container");

            if (housesParent != null)
            {
                houseContainer.transform.SetParent(housesParent.transform);
            }

            // Position Bác Năm's house at world (X: 8.0, Y: 0.0, Z: 12.0)
            // Rotated 180 degrees to face the player/main character
            houseContainer.transform.position = new Vector3(8.0f, 0.0f, 12.0f);
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

            // 5. Instantiate Daybed (Chõng tre) in front of the house
            // Placed at local (-0.5, 0.0, 4.5) relative to the 180-degree rotated container,
            // which translates to world (8.5, 0.0, 7.5) next to Bác Năm.
            GameObject daybedObj = InstantiateVisual(daybedFbx, "DaybedModel", houseContainer.transform, daybedMat);
            if (daybedObj != null)
            {
                // Auto-scale daybed to a height of 1.2 meters to make it proportional and clear
                AutoScaleObject(daybedObj, 1.2f);
                // Align pivot and position
                AlignPivotOffset(daybedObj, houseContainer.transform, new Vector3(-0.5f, 0.0f, 4.5f));
            }

            // 6. Set up BoxCollider on BacNam_House covering the main house structure
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
                    Debug.Log($"[SETUP BAC NAM] Configured House BoxCollider: Center={boxCol.center}, Size={boxCol.size}");
                }
            }

            // 7. Reposition NPC Bác Năm near the daybed (X: 7.0, Y: 0.5, Z: 7.5) facing forward/player
            GameObject npcBacNam = GameObject.Find("NPC_BacNam");
            if (npcBacNam == null) npcBacNam = GameObject.Find("NPC_BacNam (Bác Năm)");
            if (npcBacNam != null)
            {
                npcBacNam.transform.position = new Vector3(7.0f, 0.5f, 7.5f);
                npcBacNam.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                // Auto-scale Bác Năm to a natural height of 1.7 meters
                Transform npcVisual = npcBacNam.transform.Find("Visual");
                if (npcVisual != null)
                {
                    AutoScaleObject(npcVisual.gameObject, 1.7f);
                    // Center and align feet to Y = 0 (local Y = -0.5f)
                    AlignPivotOffset(npcVisual.gameObject, npcBacNam.transform, new Vector3(0f, -0.5f, 0f));
                }

                // Adjust trigger collider of NPC Bác Năm to cover the daybed area for easy interaction
                BoxCollider npcBox = npcBacNam.GetComponent<BoxCollider>();
                if (npcBox != null)
                {
                    npcBox.center = new Vector3(0.75f, 1f, 0f); // Cover daybed area next to Bác Năm (world X: 7.75, Z: 7.5)
                    npcBox.size = new Vector3(3.5f, 2.0f, 3.0f);
                }
                Debug.Log("[SETUP BAC NAM] Positioned NPC_BacNam, scaled his visual, and aligned interaction bounds.");
            }

            // 8. Mark scene dirty and save
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(houseContainer);
                EditorSceneManager.MarkSceneDirty(houseContainer.scene);
                bool saved = EditorSceneManager.SaveScene(houseContainer.scene);
                AssetDatabase.SaveAssets();
                Debug.Log($"[SETUP BAC NAM] House and Daybed setup completed successfully: {saved}");
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
