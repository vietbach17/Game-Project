using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SownInStone.Editor
{
    /// <summary>
    /// Editor utility: tìm ThuyenThung_Model đã có trong scene,
    /// gắn Coracle script + BoxCollider + Rigidbody, set inactive,
    /// và gán texture đúng — rồi lưu scene.
    /// Menu: Sown In Stone → Setup Coracle (Thuyền Thúng)
    /// </summary>
    public class SetupCoracle
    {
        [MenuItem("Sown In Stone/Setup Coracle (Thuyền Thúng)")]
        public static void Setup()
        {
            // ── 1. Tìm ThuyenThung_Model trong scene ────────────────────
            GameObject boatObj = GameObject.Find("ThuyenThung_Model");

            // Nếu không tìm thấy theo tên chính xác, thử find theo type Coracle
            if (boatObj == null)
            {
                var existingCoracle = Object.FindAnyObjectByType<SownInStone.Interactions.Coracle>(FindObjectsInactive.Include);
                if (existingCoracle != null) boatObj = existingCoracle.gameObject;
            }

            if (boatObj == null)
            {
                EditorUtility.DisplayDialog(
                    "Setup Coracle",
                    "Không tìm thấy 'ThuyenThung_Model' trong scene!\n\n" +
                    "Hãy kéo file ThuyenThung_Model.fbx từ Assets/Prefabs/Coracle vào Hierarchy trước.",
                    "OK"
                );
                return;
            }

            Undo.RegisterCompleteObjectUndo(boatObj, "Setup Coracle");

            // ── 2. Gán texture URP cho tất cả Renderer trên model ───────
            string texPath = "Assets/Prefabs/Coracle/ThuyenThung_Texture.png";
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null)
            {
                var renderers = boatObj.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    // Tạo bản sao material để không ảnh hưởng shared material
                    var mat = new Material(r.sharedMaterial != null ? r.sharedMaterial : new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")));
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                    if (mat.HasProperty("_MainTex"))  mat.SetTexture("_MainTex",  tex);
                    r.sharedMaterial = mat;
                }
                Debug.Log($"[SETUP CORACLE] Đã gán texture: {texPath}");
            }
            else
            {
                Debug.LogWarning($"[SETUP CORACLE] Không tìm thấy texture tại '{texPath}' — model sẽ dùng material hiện tại.");
            }

            // ── 3. Thêm / cấu hình Rigidbody ────────────────────────────
            Rigidbody rb = boatObj.GetComponent<Rigidbody>();
            if (rb == null) rb = Undo.AddComponent<Rigidbody>(boatObj);
            rb.mass            = 150f;
            rb.useGravity      = false;
            rb.isKinematic     = false;
            rb.linearDamping   = 0.8f;
            rb.angularDamping  = 0.8f;
            rb.constraints     = RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ
                               | RigidbodyConstraints.FreezePositionY;
            Debug.Log("[SETUP CORACLE] Đã cấu hình Rigidbody.");

            // ── 4. Thêm / cấu hình BoxCollider ──────────────────────────
            BoxCollider col = boatObj.GetComponent<BoxCollider>();
            if (col == null) col = Undo.AddComponent<BoxCollider>(boatObj);
            col.isTrigger = false;
            col.center    = new Vector3(0f, 0.25f, 0f);
            col.size      = new Vector3(2.2f, 0.6f, 2.2f);

            // Xóa collider trên mesh con để tránh conflict
            var childCols = boatObj.GetComponentsInChildren<Collider>(true);
            foreach (var c in childCols)
            {
                if (c.gameObject != boatObj)
                    Undo.DestroyObjectImmediate(c);
            }
            Debug.Log("[SETUP CORACLE] Đã cấu hình BoxCollider.");

            // ── 5. Thêm / cấu hình Coracle script ───────────────────────
            var coracleType = System.Type.GetType("SownInStone.Interactions.Coracle, Assembly-CSharp");
            if (coracleType == null)
            {
                Debug.LogError("[SETUP CORACLE] Không tìm thấy class 'SownInStone.Interactions.Coracle'. Hãy đảm bảo dự án đã compile thành công.");
            }
            else
            {
                var coracleComp = boatObj.GetComponent(coracleType);
                if (coracleComp == null)
                    coracleComp = Undo.AddComponent(boatObj, coracleType);

                // Gán các field qua SerializedObject để Undo hoạt động đúng
                var so = new SerializedObject(coracleComp);
                so.FindProperty("moveSpeed")?.SetAsFloat(5f);
                so.FindProperty("rotationSpeed")?.SetAsFloat(80f);

                // playerSeatOffset
                var seatProp = so.FindProperty("playerSeatOffset");
                if (seatProp != null) seatProp.vector3Value = new Vector3(0f, 0.45f, 0f);

                // deliveryRange
                so.FindProperty("deliveryRange")?.SetAsFloat(6f);

                so.ApplyModifiedProperties();
                Debug.Log("[SETUP CORACLE] Đã gắn và cấu hình script Coracle.");
            }

            // ── 6. Đặt vị trí thuyền gần nhà Thành (trước sân) ─────────
            boatObj.transform.position = new Vector3(5f, 1.15f, -12f);
            boatObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            Debug.Log("[SETUP CORACLE] Đã đặt vị trí: (5, 1.15, -12).");

            // ── 7. Set inactive — sẽ được bật khi Phase 3 bắt đầu ───────
            boatObj.SetActive(false);
            Debug.Log("[SETUP CORACLE] Đã set ThuyenThung_Model thành Inactive.");

            // ── 8. Lưu scene ─────────────────────────────────────────────
            EditorUtility.SetDirty(boatObj);
            EditorSceneManager.MarkSceneDirty(boatObj.scene);
            bool saved = EditorSceneManager.SaveScene(boatObj.scene);

            string msg = saved
                ? "✅ Setup Coracle hoàn tất!\n\n" +
                  "• Đã gắn: Rigidbody, BoxCollider, Coracle script\n" +
                  "• Texture: ThuyenThung_Texture.png\n" +
                  "• Vị trí: (5, 1.15, -12)\n" +
                  "• Trạng thái: Inactive (sẽ hiện khi Phase 3 bắt đầu)\n" +
                  "• Scene đã được lưu!"
                : "⚠️ Setup hoàn tất nhưng lưu scene thất bại.\nVui lòng Ctrl+S để lưu thủ công.";

            EditorUtility.DisplayDialog("Setup Coracle", msg, "OK");
        }
    }

    /// <summary>Extension để set float value ngắn gọn</summary>
    internal static class SerializedPropertyExtensions
    {
        public static void SetAsFloat(this SerializedProperty prop, float val)
        {
            if (prop == null) return;
            prop.floatValue = val;
        }
    }
}
