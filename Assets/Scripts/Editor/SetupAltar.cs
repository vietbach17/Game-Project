using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SownInStone.Editor
{
    /// <summary>
    /// Editor script to automate setting up the Ancestral Altar (Bàn thờ Thiên/Tổ tiên),
    /// auto-scaling it, correcting pivot offsets to place it on the ground,
    /// and configuring its interaction trigger bounds.
    /// </summary>
    public class SetupAltar
    {
        [MenuItem("Sown In Stone/Setup Altar")]
        public static void Setup()
        {
            // File paths for Altar
            string fbxPath = "Assets/Prefabs/Altar/Altar_Model.fbx";
            string texPath = "Assets/Prefabs/Altar/Altar_Texture.png";
            string matPath = "Assets/Prefabs/Altar/Mat_Altar.mat";

            // 1. Create or Load Material
            Material altarMat = SetupMaterial(matPath, texPath);

            // 2. Load FBX Asset
            GameObject altarFbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (altarFbx == null)
            {
                Debug.LogError($"[SETUP ALTAR] Could not load Altar FBX at '{fbxPath}'.");
                return;
            }

            // 3. Find AncestralAltar in the scene
            GameObject altarContainer = GameObject.Find("AncestralAltar");
            if (altarContainer == null)
            {
                altarContainer = new GameObject("AncestralAltar");
                // Try to add the interaction script
                var scriptType = System.Type.GetType("SownInStone.Interactions.AncestralAltar, Assembly-CSharp");
                if (scriptType != null)
                {
                    altarContainer.AddComponent(scriptType);
                }
                Undo.RegisterCreatedObjectUndo(altarContainer, "Create AncestralAltar Container");
            }
            else
            {
                Undo.RegisterCompleteObjectUndo(altarContainer, "Update AncestralAltar");
            }

            // Move container to the bottom-left corner of Thành's house (X: 7.5, Y: 0.0, Z: -13.0)
            altarContainer.transform.position = new Vector3(7.5f, 0.0f, -13.0f);
            altarContainer.transform.rotation = Quaternion.identity;

            // Destroy any existing child visual objects to avoid duplication
            for (int i = altarContainer.transform.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(altarContainer.transform.GetChild(i).gameObject);
            }

            // 4. Instantiate Altar Model under container
            GameObject visualObj = InstantiateVisual(altarFbx, "VisualModel", altarContainer.transform, altarMat);
            if (visualObj != null)
            {
                // Auto-scale altar to a target height of 1.8 meters
                AutoScaleObject(visualObj, 1.8f);
                // Center horizontally and place its bottom exactly at local Y = 0
                AlignPivotOffset(visualObj, altarContainer.transform, Vector3.zero);
            }

            // 5. Configure BoxCollider trigger on container for player interaction
            BoxCollider boxCol = altarContainer.GetComponent<BoxCollider>();
            if (boxCol == null)
            {
                boxCol = altarContainer.AddComponent<BoxCollider>();
            }
            boxCol.isTrigger = true;
            boxCol.center = new Vector3(0f, 0.9f, 0f);
            boxCol.size = new Vector3(2.5f, 1.8f, 2.5f);

            // 6. Mark scene dirty and save
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(altarContainer);
                EditorSceneManager.MarkSceneDirty(altarContainer.scene);
                bool saved = EditorSceneManager.SaveScene(altarContainer.scene);
                AssetDatabase.SaveAssets();
                Debug.Log($"[SETUP ALTAR] Altar setup completed successfully: {saved}");
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
