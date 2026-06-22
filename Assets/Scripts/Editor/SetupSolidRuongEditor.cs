using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using SownInStone.Agriculture;

namespace SownInStone.Editor
{
    public class SetupSolidRuongEditor
    {
        [MenuItem("Sown In Stone/Setup Solid Ruong (3x3)")]
        public static void Setup()
        {
            // Mở scene Village_Demo.unity
            var activeScene = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(activeScene.name) || !activeScene.name.Contains("Village_Demo"))
            {
                Debug.Log("[SETUP RUONG] Đang mở scene Assets/Scenes/Village_Demo.unity...");
                activeScene = EditorSceneManager.OpenScene("Assets/Scenes/Village_Demo.unity", OpenSceneMode.Single);
            }

            // 1. Tìm ruộng cha SoilCell_1
            SoilCell parentSoil = null;
            SoilCell[] allSoils = Object.FindObjectsByType<SoilCell>(FindObjectsSortMode.None);
            List<SoilCell> childSoils = new List<SoilCell>();

            foreach (var s in allSoils)
            {
                if (s.gameObject.name == "SoilCell_1")
                {
                    parentSoil = s;
                }
                else if (s.gameObject.name.StartsWith("SoilCell_Grid"))
                {
                    childSoils.Add(s);
                }
            }

            if (parentSoil == null)
            {
                Debug.LogError("[SETUP RUONG] Không tìm thấy ruộng cha SoilCell_1!");
                return;
            }

            Debug.Log($"[SETUP RUONG] Tìm thấy ruộng cha SoilCell_1 and {childSoils.Count} ô ruộng con.");

            // 2. Định nghĩa vị trí 9 ô con (3x3 solid)
            // Tâm ruộng cha: x = -8.0, z = -10.0
            // Khoảng cách 1.5 mét
            Vector3[] newPositions = new Vector3[]
            {
                new Vector3(-9.5f, 0.13f, -11.5f), // SoilCell_Grid1
                new Vector3(-8.0f, 0.13f, -11.5f), // SoilCell_Grid2
                new Vector3(-6.5f, 0.13f, -11.5f), // SoilCell_Grid3
                new Vector3(-9.5f, 0.13f, -10.0f), // SoilCell_Grid4
                new Vector3(-8.0f, 0.13f, -10.0f), // SoilCell_Grid5
                new Vector3(-6.5f, 0.13f, -10.0f), // SoilCell_Grid6
                new Vector3(-9.5f, 0.13f, -8.5f),  // SoilCell_Grid7
                new Vector3(-8.0f, 0.13f, -8.5f),  // SoilCell_Grid8
                new Vector3(-6.5f, 0.13f, -8.5f)   // SoilCell_Grid9
            };

            // Sắp xếp các ô con cũ theo tên để dễ ánh xạ
            childSoils.Sort((a, b) => a.gameObject.name.CompareTo(b.gameObject.name));

            // Sắp xếp lại hoặc gán vị trí cho 9 ô con
            int cellsToProcess = Mathf.Min(9, childSoils.Count);
            
            // Đảm bảo ruộng cha được reset sạch childCells
            Undo.RecordObject(parentSoil, "Reset parent field childCells");
            parentSoil.childCells.Clear();

            for (int i = 0; i < cellsToProcess; i++)
            {
                SoilCell sc = childSoils[i];
                Undo.RecordObject(sc.gameObject, "Rename SoilCell");
                sc.gameObject.name = $"SoilCell_Grid{i + 1}";

                Undo.RecordObject(sc.transform, "Align SoilCell Transform");
                sc.transform.position = newPositions[i];
                sc.transform.localScale = Vector3.one;
                sc.transform.rotation = Quaternion.Euler(180f, 0f, 0f);

                // Gán parentField
                Undo.RecordObject(sc, "Link parentField");
                sc.parentField = parentSoil;
                parentSoil.childCells.Add(sc);

                // Cấu hình BoxCollider
                BoxCollider bc = sc.GetComponent<BoxCollider>();
                if (bc != null)
                {
                    Undo.RecordObject(bc, "Setup BoxCollider");
                    bc.size = new Vector3(2f, 0.3f, 2f);
                    bc.center = new Vector3(0f, 0.1f, 0f);
                }

                // Cấu hình SpriteRenderer ẩn đi (vì dùng 3D visuals)
                SpriteRenderer sr = sc.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Undo.RecordObject(sr, "Disable SpriteRenderer");
                    sr.enabled = false;
                }

                EditorUtility.SetDirty(sc);
            }

            // Xóa các ô con thừa (nếu ban đầu có 12 ô)
            if (childSoils.Count > 9)
            {
                for (int i = 9; i < childSoils.Count; i++)
                {
                    Debug.Log($"[SETUP RUONG] Xóa ô ruộng thừa: {childSoils[i].gameObject.name}");
                    Undo.DestroyObjectImmediate(childSoils[i].gameObject);
                }
            }

            // Đồng bộ ruộng cha
            Undo.RecordObject(parentSoil.transform, "Align Parent transform");
            parentSoil.transform.position = new Vector3(-8f, 0.02f, -10f);
            parentSoil.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            
            BoxCollider parentCollider = parentSoil.GetComponent<BoxCollider>();
            if (parentCollider != null)
            {
                Undo.RecordObject(parentCollider, "Setup Parent Collider");
                parentCollider.size = new Vector3(6f, 0.3f, 6f);
                parentCollider.center = new Vector3(0f, 0.05f, 0f);
            }

            EditorUtility.SetDirty(parentSoil);

            // 3. Setup hàng rào bao quanh ruộng
            // Tìm đối tượng cha Fences
            GameObject fencesRoot = GameObject.Find("Fences");
            if (fencesRoot == null)
            {
                fencesRoot = new GameObject("Fences");
                Undo.RegisterCreatedObjectUndo(fencesRoot, "Create Fences Root");
            }

            // Xóa tất cả các transform con hiện tại dưới Fences
            List<GameObject> oldFences = new List<GameObject>();
            foreach (Transform child in fencesRoot.transform)
            {
                oldFences.Add(child.gameObject);
            }
            foreach (var go in oldFences)
            {
                Undo.DestroyObjectImmediate(go);
            }

            // Load model Fence2.fbx làm hàng rào
            GameObject fenceModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Fence/Fence2.fbx");
            if (fenceModel == null)
            {
                Debug.LogError("[SETUP RUONG] Không tìm thấy model Fence2.fbx tại Assets/Prefabs/Fence/Fence2.fbx");
            }
            else
            {
                // Tạo 12 thanh hàng rào bao quanh
                // Kích thước phủ: x: -10.5 -> -5.5, z: -12.5 -> -7.5
                // x khoảng cách: -9.66, -8.0, -6.33
                // z khoảng cách: -11.66, -10.0, -8.33
                
                // Nam (dưới) z = -12.2f (sát mép ruộng)
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-9.66f, 0.02f, -12.2f), Quaternion.Euler(0f, 0f, 0f));
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-8.00f, 0.02f, -12.2f), Quaternion.Euler(0f, 0f, 0f));
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-6.33f, 0.02f, -12.2f), Quaternion.Euler(0f, 0f, 0f));

                // Bắc (trên) z = -7.8f (sát mép ruộng)
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-9.66f, 0.02f, -7.8f), Quaternion.Euler(0f, 0f, 0f));
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-8.00f, 0.02f, -7.8f), Quaternion.Euler(0f, 0f, 0f));
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-6.33f, 0.02f, -7.8f), Quaternion.Euler(0f, 0f, 0f));

                // Tây (trái) x = -10.2f
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-10.2f, 0.02f, -11.66f), Quaternion.Euler(0f, 90f, 0f));
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-10.2f, 0.02f, -10.00f), Quaternion.Euler(0f, 90f, 0f));
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-10.2f, 0.02f, -8.33f), Quaternion.Euler(0f, 90f, 0f));

                // Đông (phải) x = -5.8f
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-5.8f, 0.02f, -11.66f), Quaternion.Euler(0f, 90f, 0f));
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-5.8f, 0.02f, -10.00f), Quaternion.Euler(0f, 90f, 0f));
                CreateFenceSegment(fenceModel, fencesRoot.transform, new Vector3(-5.8f, 0.02f, -8.33f), Quaternion.Euler(0f, 90f, 0f));

                Debug.Log("[SETUP RUONG] Đã tái tạo 12 thanh hàng rào bao quanh mảnh ruộng.");
            }

            // Save scene
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool saved = EditorSceneManager.SaveScene(activeScene);
            Debug.Log($"[SETUP RUONG] Hoàn tất thiết lập lưới ruộng solid 3x3 và rào chắn. Đã lưu scene: {saved}");
        }

        private static void CreateFenceSegment(GameObject prefab, Transform parent, Vector3 pos, Quaternion rot)
        {
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (go != null)
            {
                Undo.RegisterCreatedObjectUndo(go, "Create Fence Segment");
                go.name = "FenceSegment";
                go.transform.SetParent(parent, false);
                go.transform.position = pos;
                go.transform.rotation = rot;
                go.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }
}
