using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SownInStone.Editor
{
    /// <summary>
    /// Editor script to automate setting up the dogs and chickens,
    /// generating materials, setting up colliders, rigidbodies, and the WanderingAnimal AI.
    /// </summary>
    public class SetupAnimals
    {
        [MenuItem("Sown In Stone/Setup Animals")]
        public static void Setup()
        {
            // Paths
            string dog1FbxPath = "Assets/Prefabs/Dogs/Dog1_Model.fbx";
            string dog1TexPath = "Assets/Prefabs/Dogs/Dog1_Texture.png";
            string dog1MatPath = "Assets/Prefabs/Dogs/Mat_Dog1.mat";

            string dog2FbxPath = "Assets/Prefabs/Dogs/Dog2_Model.fbx";
            string dog2TexPath = "Assets/Prefabs/Dogs/Dog2_Texture.png";
            string dog2MatPath = "Assets/Prefabs/Dogs/Mat_Dog2.mat";

            string chickenFbxPath = "Assets/Prefabs/Chickens/Chicken_Model.fbx";
            string chickenTexPath = "Assets/Prefabs/Chickens/Chicken_Texture.png";
            string chickenMatPath = "Assets/Prefabs/Chickens/Mat_Chicken.mat";

            // 1. Create or Load Materials
            Material dog1Mat = SetupMaterial(dog1MatPath, dog1TexPath);
            Material dog2Mat = SetupMaterial(dog2MatPath, dog2TexPath);
            Material chickenMat = SetupMaterial(chickenMatPath, chickenTexPath);

            // 2. Load FBX Assets
            GameObject dog1Fbx = AssetDatabase.LoadAssetAtPath<GameObject>(dog1FbxPath);
            GameObject dog2Fbx = AssetDatabase.LoadAssetAtPath<GameObject>(dog2FbxPath);
            GameObject chickenFbx = AssetDatabase.LoadAssetAtPath<GameObject>(chickenFbxPath);

            if (dog1Fbx == null || dog2Fbx == null || chickenFbx == null)
            {
                Debug.LogError($"[SETUP ANIMALS] Could not load all animal FBX models. Dog1: {dog1Fbx != null}, Dog2: {dog2Fbx != null}, Chicken: {chickenFbx != null}");
                return;
            }

            // 3. Find or Create parent container under _Environment
            GameObject envParent = GameObject.Find("_Environment");
            GameObject animalsParent = GameObject.Find("Animals");
            if (animalsParent == null)
            {
                animalsParent = new GameObject("Animals");
                if (envParent != null)
                {
                    animalsParent.transform.SetParent(envParent.transform);
                }
                Undo.RegisterCreatedObjectUndo(animalsParent, "Create Animals Parent");
            }
            else
            {
                Undo.RegisterCompleteObjectUndo(animalsParent, "Update Animals");
            }

            animalsParent.transform.position = Vector3.zero;
            animalsParent.transform.rotation = Quaternion.identity;

            // Clear any existing children to avoid duplicates
            for (int i = animalsParent.transform.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(animalsParent.transform.GetChild(i).gameObject);
            }

            // 4. Instantiate Dog1 (Thành's house guard dog)
            // Positioned at (10.2, 0, -12.8) in front of the door, facing South (180 degrees)
            GameObject dog1Container = new GameObject("Dog_Thanh");
            dog1Container.transform.SetParent(animalsParent.transform);
            dog1Container.transform.position = new Vector3(10.15f, GetGroundHeight(new Vector3(10.15f, 0f, -5.29f)), -5.29f);
            dog1Container.transform.rotation = Quaternion.Euler(0f, 38.64f, 0f);
            dog1Container.transform.localScale = Vector3.one * 1.5f;

            GameObject dog1Visual = InstantiateVisual(dog1Fbx, "VisualModel", dog1Container.transform, dog1Mat);
            if (dog1Visual != null)
            {
                AutoScaleObject(dog1Visual, 0.7f); // target height 0.7m
                AlignPivotOffset(dog1Visual, dog1Container.transform, Vector3.zero);
            }
            SetupComponents(dog1Container, 1.0f); // Dog1: 1.0m wander radius (guarding)

            // 5. Instantiate Dog2 (Bác Năm's house guard dog)
            // Positioned at (6.0, 0, 9.0) near the door/daybed, facing South (180 degrees)
            GameObject dog2Container = new GameObject("Dog_BacNam");
            dog2Container.transform.SetParent(animalsParent.transform);
            dog2Container.transform.position = new Vector3(-5.50f, GetGroundHeight(new Vector3(-5.50f, 0f, 24.63f)), 24.63f);
            dog2Container.transform.rotation = Quaternion.Euler(0f, 176.52f, 0f);
            dog2Container.transform.localScale = Vector3.one * 1.5f;

            GameObject dog2Visual = InstantiateVisual(dog2Fbx, "VisualModel", dog2Container.transform, dog2Mat);
            if (dog2Visual != null)
            {
                AutoScaleObject(dog2Visual, 0.7f); // target height 0.7m
                AlignPivotOffset(dog2Visual, dog2Container.transform, Vector3.zero);
            }
            SetupComponents(dog2Container, 1.0f); // Dog2: 1.0m wander radius (guarding)

            // 6. Instantiate Chickens (di chuyển tự do hơn)
            // Chicken 1: near Bac Nam's yard (-0.91, 0, 4.10)
            CreateChicken(animalsParent.transform, chickenFbx, chickenMat, new Vector3(-0.91f, GetGroundHeight(new Vector3(-0.91f, 0f, 4.10f)), 4.10f), "Chicken_1", 3.5f);
            // Chicken 2: near Bac Nam's yard (5.0, 0, 10.0)
            CreateChicken(animalsParent.transform, chickenFbx, chickenMat, new Vector3(5.0f, GetGroundHeight(new Vector3(5.0f, 0f, 10.0f)), 10.0f), "Chicken_2", 3.5f);
            // Chicken 3: near Thanh's yard (8.5, 0, -13.5)
            CreateChicken(animalsParent.transform, chickenFbx, chickenMat, new Vector3(8.5f, GetGroundHeight(new Vector3(8.5f, 0f, -13.5f)), -13.5f), "Chicken_3", 3.5f);
            // Chicken 4: near O Tham's yard (2.0, 0, -14.0)
            CreateChicken(animalsParent.transform, chickenFbx, chickenMat, new Vector3(2.0f, GetGroundHeight(new Vector3(2.0f, 0f, -14.0f)), -14.0f), "Chicken_4", 3.5f);

            // Save scene and mark dirty
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(animalsParent);
                EditorSceneManager.MarkSceneDirty(animalsParent.scene);
                bool saved = EditorSceneManager.SaveScene(animalsParent.scene);
                AssetDatabase.SaveAssets();
                Debug.Log($"[SETUP ANIMALS] Setup completed successfully. Scene saved: {saved}");
            }
        }

        private static void CreateChicken(Transform parent, GameObject chickenFbx, Material chickenMat, Vector3 position, string name, float wanderRadius)
        {
            GameObject chickenContainer = new GameObject(name);
            chickenContainer.transform.SetParent(parent);
            chickenContainer.transform.position = position;
            chickenContainer.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            chickenContainer.transform.localScale = Vector3.one * Random.Range(2.0f, 2.5f);

            GameObject visual = InstantiateVisual(chickenFbx, "VisualModel", chickenContainer.transform, chickenMat);
            if (visual != null)
            {
                visual.transform.localPosition = new Vector3(1.35613f, -0.001f, 0.083f);
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one * 10.7528f;
            }
            SetupComponents(chickenContainer, wanderRadius);
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

        private static void SetupComponents(GameObject container, float wanderRadius)
        {
            // 1. Add BoxCollider
            BoxCollider box = container.GetComponent<BoxCollider>();
            if (box == null) box = container.AddComponent<BoxCollider>();
            
            // Set size appropriate for standard small animals
            if (wanderRadius < 1.5f) // Dog
            {
                box.center = new Vector3(0f, 0.35f, 0f);
                box.size = new Vector3(0.6f, 0.7f, 0.9f);
            }
            else // Chicken
            {
                box.center = new Vector3(0f, 0.225f, 0f);
                box.size = new Vector3(0.4f, 0.45f, 0.4f);
            }
            box.isTrigger = false;

            // 2. Add Rigidbody
            Rigidbody rb = container.GetComponent<Rigidbody>();
            if (rb == null) rb = container.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.mass = 10f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            // 3. Add WanderingAnimal AI
            var scriptType = System.Type.GetType("SownInStone.Core.WanderingAnimal, Assembly-CSharp");
            if (scriptType != null)
            {
                var component = container.GetComponent(scriptType);
                if (component == null)
                {
                    component = container.AddComponent(scriptType);
                }
                
                // Set fields via reflection
                var fieldWanderRadius = scriptType.GetField("wanderRadius");
                if (fieldWanderRadius != null) fieldWanderRadius.SetValue(component, wanderRadius);

                var fieldMoveSpeed = scriptType.GetField("moveSpeed");
                if (fieldMoveSpeed != null) fieldMoveSpeed.SetValue(component, wanderRadius < 1.5f ? 1.2f : 0.8f);
            }
        }

        private static float GetGroundHeight(Vector3 position)
        {
            Vector3 origin = new Vector3(position.x, 50f, position.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 100f))
            {
                return hit.point.y;
            }
            return 0f;
        }
    }
}
