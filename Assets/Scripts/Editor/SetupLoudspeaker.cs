using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SownInStone.Editor
{
    /// <summary>
    /// Editor script to automate setting up the Loudspeaker,
    /// generating material, configuring colliders, and adding AudioSource.
    /// </summary>
    public class SetupLoudspeaker
    {
        [MenuItem("Sown In Stone/Setup Loudspeaker")]
        public static void Setup()
        {
            // Paths
            string fbxPath = "Assets/Prefabs/Loudspeaker/Loudspeaker_Model.fbx";
            string texPath = "Assets/Prefabs/Loudspeaker/Loudspeaker_Texture.png";
            string matPath = "Assets/Prefabs/Loudspeaker/Mat_Loudspeaker.mat";

            // 1. Create or Load Material
            Material speakerMat = SetupMaterial(matPath, texPath);

            // 2. Load FBX Asset
            GameObject speakerFbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (speakerFbx == null)
            {
                Debug.LogError($"[SETUP LOUDSPEAKER] Could not load Loudspeaker FBX at '{fbxPath}'.");
                return;
            }

            // 3. Find or Create parent container under _Environment
            GameObject envParent = GameObject.Find("_Environment");
            GameObject speakerContainer = GameObject.Find("Loudspeaker");
            if (speakerContainer == null)
            {
                speakerContainer = new GameObject("Loudspeaker");
                if (envParent != null)
                {
                    speakerContainer.transform.SetParent(envParent.transform);
                }
                Undo.RegisterCreatedObjectUndo(speakerContainer, "Create Loudspeaker Container");
            }
            else
            {
                Undo.RegisterCompleteObjectUndo(speakerContainer, "Update Loudspeaker");
            }

            // Position loudspeaker near the central road (0, 0, -13) between O Thắm's shop and Thành's house
            speakerContainer.transform.position = new Vector3(0.0f, 0.0f, -13.0f);
            speakerContainer.transform.rotation = Quaternion.identity;

            // Clear any existing children to avoid duplicates
            for (int i = speakerContainer.transform.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(speakerContainer.transform.GetChild(i).gameObject);
            }

            // 4. Instantiate Loudspeaker Model
            GameObject visualObj = InstantiateVisual(speakerFbx, "VisualModel", speakerContainer.transform, speakerMat);
            if (visualObj != null)
            {
                // Auto-scale to a target height of 6.0 meters (for a realistic tall utility pole)
                AutoScaleObject(visualObj, 6.0f);
                // Align pivot offset to ground (Y = 0)
                AlignPivotOffset(visualObj, speakerContainer.transform, Vector3.zero);
            }

            // 5. Configure BoxCollider for physical blocking (prevent walking through the pole)
            BoxCollider box = speakerContainer.GetComponent<BoxCollider>();
            if (box == null) box = speakerContainer.AddComponent<BoxCollider>();
            box.isTrigger = false;
            // The pole is narrow but tall
            box.center = new Vector3(0f, 3.0f, 0f);
            box.size = new Vector3(0.5f, 6.0f, 0.5f);

            // 6. Add AudioSource for radio music and warnings
            AudioSource audio = speakerContainer.GetComponent<AudioSource>();
            if (audio == null) audio = speakerContainer.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.loop = true;
            audio.spatialBlend = 1.0f; // 3D sound
            audio.minDistance = 3.0f;
            audio.maxDistance = 25.0f;
            audio.rolloffMode = AudioRolloffMode.Linear;

            // Save scene and mark dirty
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(speakerContainer);
                EditorSceneManager.MarkSceneDirty(speakerContainer.scene);
                bool saved = EditorSceneManager.SaveScene(speakerContainer.scene);
                AssetDatabase.SaveAssets();
                Debug.Log($"[SETUP LOUDSPEAKER] Setup completed successfully. Scene saved: {saved}");
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
