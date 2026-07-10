using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace SownInStone.Editor
{
    /// <summary>
    /// Editor utility: thiết lập ThuyenThung_Model trong scene.
    /// Để tránh lỗi lan truyền scale (model gốc scale 100,100,100), tool này sẽ:
    /// 1. Tạo hoặc tìm một Root Container tên là "Coracle" với scale (1,1,1)
    /// 2. Đưa ThuyenThung_Model vào làm con của "Coracle" và đưa các component điều khiển (Coracle script, Rigidbody, BoxCollider) lên root "Coracle".
    /// 3. Xoá mọi component Rigidbody, Collider trên model con ThuyenThung_Model để tránh xung đột vật lý.
    /// 4. Gán material chuẩn đã lưu (ThuyenThung_Texture.mat) thay vì tạo material tạm thời.
    /// 5. Đặt vị trí và lưu Scene.
    /// Menu: Sown In Stone → Setup Coracle (Thuyền Thúng)
    /// </summary>
    public class SetupCoracle
    {
        [MenuItem("Sown In Stone/Setup Coracle (Thuyền Thúng)")]
        public static void Setup()
        {
            // ── 1. Tìm model ThuyenThung_Model trong scene ──────────────
            GameObject modelObj = GameObject.Find("ThuyenThung_Model");
            if (modelObj == null)
            {
                // Thử tìm theo type MeshRenderer nếu tên bị thay đổi
                var allRenderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include);
                foreach (var r in allRenderers)
                {
                    if (r.name.Contains("ThuyenThung") || r.name.Contains("Coracle"))
                    {
                        modelObj = r.gameObject;
                        break;
                    }
                }
            }

            if (modelObj == null)
            {
                EditorUtility.DisplayDialog(
                    "Setup Coracle",
                    "Không tìm thấy model 'ThuyenThung_Model' trong scene!\n\n" +
                    "Hãy kéo model ThuyenThung_Model.fbx từ Assets/Prefabs/Coracle vào Hierarchy trước.",
                    "OK"
                );
                return;
            }

            // ── 2. Tạo hoặc tìm GameObject cha "Coracle" (Scale 1,1,1) ──────
            GameObject rootObj = GameObject.Find("Coracle");
            if (rootObj == null)
            {
                var existingCoracleScript = Object.FindAnyObjectByType<SownInStone.Interactions.Coracle>(FindObjectsInactive.Include);
                if (existingCoracleScript != null)
                {
                    rootObj = existingCoracleScript.gameObject;
                }
            }

            if (rootObj == null)
            {
                rootObj = new GameObject("Coracle");
                Undo.RegisterCreatedObjectUndo(rootObj, "Create Coracle Root");
            }
            else
            {
                Undo.RegisterCompleteObjectUndo(rootObj, "Setup Coracle Root");
            }

            // Đảm bảo Root Container "Coracle" luôn có scale (1,1,1) để người chơi/NPC lên thuyền không bị méo/bay lên trời!
            rootObj.transform.localScale = Vector3.one;

            // Đưa modelObj làm con của rootObj
            Undo.SetTransformParent(modelObj.transform, rootObj.transform, "Parent Model to Coracle");
            modelObj.transform.localPosition = Vector3.zero;
            modelObj.transform.localRotation = Quaternion.identity;
            // ThuyenThung_Model giữ nguyên scale gốc của nó (ví dụ: 100,100,100) để hiển thị bình thường
            modelObj.name = "ThuyenThung_Model";

            // ── 3. Dọn dẹp các component cũ trên modelObj để tránh xung đột ─
            var oldCoracle = modelObj.GetComponent<SownInStone.Interactions.Coracle>();
            if (oldCoracle != null) Undo.DestroyObjectImmediate(oldCoracle);

            var oldRb = modelObj.GetComponent<Rigidbody>();
            if (oldRb != null) Undo.DestroyObjectImmediate(oldRb);

            var oldCols = modelObj.GetComponentsInChildren<Collider>(true);
            foreach (var c in oldCols)
            {
                Undo.DestroyObjectImmediate(c);
            }

            // ── 4. Gán material chuẩn của dự án (tránh dùng material tạo động mất tích khi chạy)
            string matPath = "Assets/Prefabs/Coracle/Materials/ThuyenThung_Texture.mat";
            Material boatMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (boatMat != null)
            {
                var renderers = modelObj.GetComponentsInChildren<Renderer>(true);
                foreach (var r in renderers)
                {
                    r.sharedMaterial = boatMat;
                }
                Debug.Log($"[SETUP CORACLE] Đã gán material lưu sẵn: {matPath}");
            }
            else
            {
                Debug.LogWarning($"[SETUP CORACLE] Không tìm thấy material lưu sẵn tại '{matPath}'. Giữ nguyên material hiện tại.");
            }

            // ── 5. Thêm/Cấu hình các component lên root "Coracle" (Scale 1,1,1)
            Rigidbody rb = rootObj.GetComponent<Rigidbody>();
            if (rb == null) rb = Undo.AddComponent<Rigidbody>(rootObj);
            rb.mass            = 150f;
            rb.useGravity      = false;
            rb.isKinematic     = false;
            rb.linearDamping   = 0.8f;
            rb.angularDamping  = 0.8f;
            rb.constraints     = RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ
                               | RigidbodyConstraints.FreezePositionY;

            BoxCollider col = rootObj.GetComponent<BoxCollider>();
            if (col == null) col = Undo.AddComponent<BoxCollider>(rootObj);
            col.isTrigger = false;
            col.center    = new Vector3(0f, 0.25f, 0f);
            col.size      = new Vector3(2.2f, 0.6f, 2.2f);

            var coracleType = System.Type.GetType("SownInStone.Interactions.Coracle, Assembly-CSharp");
            if (coracleType != null)
            {
                var coracleComp = rootObj.GetComponent(coracleType);
                if (coracleComp == null)
                    coracleComp = Undo.AddComponent(rootObj, coracleType);

                var so = new SerializedObject(coracleComp);
                so.FindProperty("moveSpeed")?.SetAsFloat(5f);
                so.FindProperty("rotationSpeed")?.SetAsFloat(80f);

                var seatProp = so.FindProperty("playerSeatOffset");
                if (seatProp != null) seatProp.vector3Value = new Vector3(0f, 0.45f, 0f);

                so.FindProperty("deliveryRange")?.SetAsFloat(6f);
                so.ApplyModifiedProperties();
            }

            // ── 6. Đặt vị trí, ẩn và lưu scene ────────────────────────────
            rootObj.transform.position = new Vector3(5f, 1.15f, -12f);
            rootObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            rootObj.SetActive(false);

            EditorUtility.SetDirty(rootObj);
            EditorSceneManager.MarkSceneDirty(rootObj.scene);
            bool saved = EditorSceneManager.SaveScene(rootObj.scene);

            string msg = saved
                ? "✅ Setup Coracle hoàn tất!\n\n" +
                  "• Đã tạo root container 'Coracle' với scale (1, 1, 1)\n" +
                  "• ThuyenThung_Model đã được đưa vào trong làm con\n" +
                  "• Gán material chuẩn: ThuyenThung_Texture.mat\n" +
                  "• Trạng thái: Inactive (sẽ kích hoạt khi lũ lên ở Phase 3)\n" +
                  "• Scene đã lưu thành công!"
                : "⚠️ Setup hoàn tất nhưng lưu scene thất bại. Hãy bấm Ctrl+S.";

            EditorUtility.DisplayDialog("Setup Coracle", msg, "OK");
        }
    }
}
