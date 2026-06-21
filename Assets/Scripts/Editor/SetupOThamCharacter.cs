using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SownInStone.Community;
using SownInStone.UI;

namespace SownInStone.Editor
{
    [InitializeOnLoad]
    public class SetupOThamCharacter
    {
        static SetupOThamCharacter()
        {
            // Đã tắt tự động chạy để tránh ghi đè mô hình nhân vật mới của bạn.
            // EditorApplication.delayCall += RunSetupOnLoad;
            // EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void RunSetupOnLoad()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("[SETUP] Auto running NPC setup on compilation/load...");
                Setup();
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                Debug.Log("[SETUP] Play mode exiting edit mode - running setup to ensure latest state...");
                Setup();
            }
        }

        [MenuItem("Sown In Stone/Setup O Tham Character")]
        public static void Setup()
        {
            string fbxPath = "Assets/Character/NPCs/O_Tham/O_Tham.fbx";
            
            // 1. Find the O Thắm NPC GameObject in the active scene
            GameObject npcObj = GameObject.Find("NPC_OTham");
            if (npcObj == null)
            {
                npcObj = GameObject.Find("NPC_OTham (O Thắm)");
            }

            if (npcObj == null)
            {
                // Try searching all root objects in active scene (in case it is inactive)
                var activeScene = EditorSceneManager.GetActiveScene();
                var rootObjs = activeScene.GetRootGameObjects();
                foreach (var root in rootObjs)
                {
                    if (root.name == "NPC_OTham" || root.name == "NPC_OTham (O Thắm)")
                    {
                        npcObj = root;
                        break;
                    }
                    // Search children recursively
                    Transform t = root.transform.Find("NPC_OTham");
                    if (t == null) t = root.transform.Find("NPC_OTham (O Thắm)");
                    if (t != null)
                    {
                        npcObj = t.gameObject;
                        break;
                    }
                }
            }

            if (npcObj == null)
            {
                Debug.LogWarning("[SETUP] Could not find NPC_OTham in the active scene.");
                return;
            }

            Debug.Log($"[SETUP] Found NPC_OTham in scene: '{npcObj.scene.name}' (Path: '{npcObj.scene.path}')");

            // 2. Position next to Bác Năm if found
            GameObject bacNamObj = GameObject.Find("NPC_BacNam");
            if (bacNamObj == null)
            {
                bacNamObj = GameObject.Find("NPC_BacNam (Bác Năm)");
            }

            if (bacNamObj != null)
            {
                Vector3 newPos = bacNamObj.transform.position + new Vector3(-2.5f, 0f, -1f);
                npcObj.transform.position = newPos;
                Debug.Log($"[SETUP] Positioned NPC_OTham next to NPC_BacNam at: {newPos}");
            }
            else
            {
                npcObj.transform.position = new Vector3(4.5f, 0.5f, 9f);
                Debug.Log("[SETUP] NPC_BacNam not found. Positioned NPC_OTham at default: (4.5, 0.5, 9)");
            }

            // 3. Load FBX Asset
            GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxAsset == null)
            {
                Debug.LogWarning($"[SETUP] Could not load FBX at '{fbxPath}'. Trying fallback...");
                fbxPath = "Assets/Character/Farmer/Model/Player_Base.fbx";
                fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            }

            if (fbxAsset == null)
            {
                Debug.LogError("[SETUP] Could not load any character FBX model. Setup failed.");
                return;
            }

            Debug.Log($"[SETUP] Successfully loaded character FBX: '{fbxPath}'");
            
            // Check bounds/scale of loaded FBX to debug size issues
            var renderers = fbxAsset.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                Debug.Log($"[SETUP] FBX Renderer found: '{r.name}' with bounds size: {r.bounds.size}, local scale: {r.transform.localScale}");
            }

            // 4. Clean up old Visual children
            // Destroy all children named "Visual" or containing SpriteRenderer/Billboard
            for (int i = npcObj.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = npcObj.transform.GetChild(i);
                if (child.name == "Visual" || child.GetComponent<SpriteRenderer>() != null || child.GetComponent<Billboard>() != null)
                {
                    Debug.Log($"[SETUP] Removing old visual child: '{child.name}'");
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }

            // 5. Instantiate new FBX visual
            GameObject visualObj = null;
            if (!Application.isPlaying)
            {
                visualObj = PrefabUtility.InstantiatePrefab(fbxAsset) as GameObject;
            }
            
            if (visualObj == null)
            {
                visualObj = Object.Instantiate(fbxAsset);
            }

            visualObj.name = "Visual";
            visualObj.transform.SetParent(npcObj.transform);
            
            // Apply scale and alignment offsets
            // Note: If the character model is too small or too large, we adjust here.
            visualObj.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            visualObj.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            visualObj.transform.localScale = Vector3.one;

            // 5b. Thiết lập Material và Texture cho mô hình 3D (Đồng bộ từ file .png)
            string matPath = "Assets/Character/NPCs/O_Tham/M_OTham.mat";
            Material oThamMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (oThamMat == null)
            {
                Debug.Log("[SETUP] Material cho O Thắm chưa tồn tại. Tiến hành tạo mới...");
                Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLitShader == null) urpLitShader = Shader.Find("Standard");
                
                oThamMat = new Material(urpLitShader);
                
                // Tải Texture
                string texPath = "Assets/Character/NPCs/O_Tham/O_Tham.png";
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (tex != null)
                {
                    oThamMat.SetTexture("_BaseMap", tex); // URP Lit
                    oThamMat.SetTexture("_MainTex", tex); // Standard/Fallback
                    Debug.Log($"[SETUP] Đã gán Texture từ '{texPath}' vào Material mới.");
                }
                else
                {
                    Debug.LogWarning($"[SETUP] Không tìm thấy Texture tại '{texPath}'!");
                }
                
                AssetDatabase.CreateAsset(oThamMat, matPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[SETUP] Đã lưu Material mới tại '{matPath}'");
            }

            // Gán Material cho tất cả các MeshRenderer của visualObj
            var meshRenderers = visualObj.GetComponentsInChildren<Renderer>();
            foreach (var r in meshRenderers)
            {
                r.sharedMaterial = oThamMat;
            }

            Debug.Log($"[SETUP] Created 3D visual child '{visualObj.name}' and assigned textured material under NPC_OTham.");

            // 6. Ensure BoxCollider is set up correctly
            BoxCollider boxCol = npcObj.GetComponent<BoxCollider>();
            if (boxCol == null)
            {
                boxCol = npcObj.AddComponent<BoxCollider>();
            }
            boxCol.isTrigger = true;
            boxCol.center = new Vector3(0f, 1f, 0f);
            boxCol.size = new Vector3(1.2f, 2f, 1.2f);

            // 7. Ensure NPCCharacter component has correct StoryCharacterType
            NPCCharacter npcComp = npcObj.GetComponent<NPCCharacter>();
            if (npcComp == null)
            {
                npcComp = npcObj.AddComponent<NPCCharacter>();
            }
            npcComp.characterType = NPCCharacter.StoryCharacterType.OTham;
            npcComp.visualModelPrefab = fbxAsset;

            // Set animator controller
            Animator anim = visualObj.GetComponent<Animator>();
            if (anim == null) anim = visualObj.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                string controllerPath = "Assets/Character/NPCs/O_Tham/OThamAnimator.controller";
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                if (controller != null)
                {
                    anim.runtimeAnimatorController = controller;
                    Debug.Log($"[SETUP] Assigned Animator Controller from '{controllerPath}' to O Thắm.");
                }
                else
                {
                    Debug.LogWarning($"[SETUP] Could not load Animator Controller at '{controllerPath}'!");
                }
            }

            // 8. Mark scene dirty and save
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(npcObj);
                EditorUtility.SetDirty(visualObj);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(npcObj.scene);
                bool saved = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(npcObj.scene);
                Debug.Log($"[SETUP] Marked scene '{npcObj.scene.name}' dirty and saved: {saved}");
                AssetDatabase.SaveAssets();
            }
        }
    }
}
