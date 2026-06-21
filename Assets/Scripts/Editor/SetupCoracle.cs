using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SownInStone.Editor
{
    /// <summary>
    /// Editor script to automate setting up the Coracle (Thuyền thúng),
    /// auto-scaling it, placing it flat on the ground, and attaching the Coracle script.
    /// </summary>
    public class SetupCoracle
    {
        [MenuItem("Sown In Stone/Setup Coracle")]
        public static void Setup()
        {
            // Paths
            string fbxPath = "Assets/Prefabs/Coracle/Coracle_Model.fbx";
            string texPath = "Assets/Prefabs/Coracle/Coracle_Texture.png";
            string matPath = "Assets/Prefabs/Coracle/Mat_Coracle.mat";

            // 1. Create or Load Material
            Material coracleMat = SetupMaterial(matPath, texPath);

            // 2. Load FBX Asset
            GameObject coracleFbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (coracleFbx == null)
            {
                Debug.LogError($"[SETUP CORACLE] Could not load Coracle FBX at '{fbxPath}'. Please make sure you have imported the FBX and Texture files into 'Assets/Prefabs/Coracle/' and named them exactly as: 'Coracle_Model.fbx' and 'Coracle_Texture.png'.");
                return;
            }

            // 3. Find or Create parent container under _Environment
            GameObject envParent = GameObject.Find("_Environment");
            GameObject coracleContainer = GameObject.Find("Coracle");
            if (coracleContainer == null)
            {
                coracleContainer = new GameObject("Coracle");
                if (envParent != null)
                {
                    coracleContainer.transform.SetParent(envParent.transform);
                }
                Undo.RegisterCreatedObjectUndo(coracleContainer, "Create Coracle Container");
            }
            else
            {
                Undo.RegisterCompleteObjectUndo(coracleContainer, "Update Coracle");
            }

            // Position coracle near the front yard of Thành's house (12.5, 0.0, -14.0)
            coracleContainer.transform.position = new Vector3(12.5f, 0.0f, -14.0f);
            coracleContainer.transform.rotation = Quaternion.identity;

            // Clear any existing children to avoid duplicates
            for (int i = coracleContainer.transform.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(coracleContainer.transform.GetChild(i).gameObject);
            }

            // 4. Instantiate Coracle Model
            GameObject visualObj = InstantiateVisual(coracleFbx, "VisualModel", coracleContainer.transform, coracleMat);
            if (visualObj != null)
            {
                // Auto-scale to a target height of 0.6 meters (typical depth/height of a Vietnamese coracle)
                AutoScaleObject(visualObj, 0.6f);
                // Align pivot offset to ground (Y = 0)
                AlignPivotOffset(visualObj, coracleContainer.transform, Vector3.zero);
            }

            // 5. Setup BoxCollider and Rigidbody components
            BoxCollider box = coracleContainer.GetComponent<BoxCollider>();
            if (box == null) box = coracleContainer.AddComponent<BoxCollider>();
            box.isTrigger = false;
            // Coracle is round, size approx 1.8m diameter and 0.6m height
            box.center = new Vector3(0f, 0.3f, 0f);
            box.size = new Vector3(1.8f, 0.6f, 1.8f);

            Rigidbody rb = coracleContainer.GetComponent<Rigidbody>();
            if (rb == null) rb = coracleContainer.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.mass = 150f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // 6. Attach the Coracle gameplay controller script
            var scriptType = System.Type.GetType("SownInStone.Interactions.Coracle, Assembly-CSharp");
            if (scriptType != null)
            {
                var component = coracleContainer.GetComponent(scriptType);
                if (component == null)
                {
                    coracleContainer.AddComponent(scriptType);
                }
            }

            // Save scene and mark dirty
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(coracleContainer);
                EditorSceneManager.MarkSceneDirty(coracleContainer.scene);
                bool saved = EditorSceneManager.SaveScene(coracleContainer.scene);
                AssetDatabase.SaveAssets();
                Debug.Log($"[SETUP CORACLE] Setup completed successfully. Scene saved: {saved}");
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
            visualObj.transform.localRotation = fbxAsset.transform.localRotation;
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localScale = Vector3.one;

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

            Vector3 localCenter = parentTrans.InverseTransformPoint(bounds.center);
            Vector3 localMin = parentTrans.InverseTransformPoint(bounds.min);

            Vector3 pivotCorrection = new Vector3(
                targetLocalPosition.x - (localCenter.x - visualObj.transform.localPosition.x),
                targetLocalPosition.y - (localMin.y - visualObj.transform.localPosition.y),
                targetLocalPosition.z - (localCenter.z - visualObj.transform.localPosition.z)
            );

            visualObj.transform.localPosition = pivotCorrection;
        }
    }
}
