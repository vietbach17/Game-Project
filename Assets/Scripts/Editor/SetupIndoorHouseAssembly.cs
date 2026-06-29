using UnityEngine;
using UnityEditor;
using System.IO;

namespace SownInStone.Editor
{
    public class SetupIndoorHouseAssembly : AssetPostprocessor
    {
        private static bool isAssembling = false;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (isAssembling) return;

            foreach (string str in importedAssets)
            {
                if (str.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase) && 
                   (str.Contains("KhungNha") || str.Contains("CuaChinh") || str.Contains("GiuongNgu") || str.Contains("RuongDo") || str.Contains("bep_gas") || str.Contains("Altar")))
                {
                    AssembleHouse();
                    break;
                }
            }
        }

        [MenuItem("Sown In Stone/Assemble House Interior")]
        public static void AssembleHouse()
        {
            if (isAssembling) return;
            isAssembling = true;

            try
            {
                Debug.Log("[HOUSE ASSEMBLY] Bắt đầu gán chính xác PZ = 0 cho đồ đạc và PY = 0.006 cho Rương đồ...");

                string prefabDir = "Assets/Resources/Prefabs";
                if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);

                // 1. Tìm Shader URP chuẩn
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpShader == null) urpShader = Shader.Find("Universal Render Pipeline/Simple Lit");
                if (urpShader == null) urpShader = Shader.Find("Standard");

                // Helper tạo/cập nhật Material URP
                Material CreateURPMaterial(string matPath, string texPath, Vector2 tiling)
                {
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                    Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    if (mat == null)
                    {
                        mat = new Material(urpShader);
                        AssetDatabase.CreateAsset(mat, matPath);
                    }
                    else
                    {
                        mat.shader = urpShader;
                    }
                    if (tex != null)
                    {
                        mat.mainTexture = tex;
                        mat.SetTexture("_BaseMap", tex);
                        mat.mainTextureScale = tiling;
                        mat.SetTextureScale("_BaseMap", tiling);
                        mat.color = Color.white;
                    }
                    return mat;
                }

                // 2. Tạo Vật liệu chuẩn URP cho cả 6 mô hình FBX & Nền sàn gỗ lót mới
                Vector2 defaultTile = Vector2.one;
                Material matKhungNha = CreateURPMaterial("Assets/Prefabs/CustomPrefabs/KhungNha/Mat_KhungNha.mat", "Assets/Prefabs/CustomPrefabs/KhungNha/KhungNha_Texture.png", defaultTile);
                Material matCuaChinh = CreateURPMaterial("Assets/Prefabs/CustomPrefabs/CuaChinh/Mat_CuaChinh.mat", "Assets/Prefabs/CustomPrefabs/CuaChinh/CuaChinh_Texture.png", defaultTile);
                Material matGiuongNgu = CreateURPMaterial("Assets/Prefabs/CustomPrefabs/GiuongNgu/Mat_GiuongNgu.mat", "Assets/Prefabs/CustomPrefabs/GiuongNgu/GiuongNgu_Texture.png", defaultTile);
                Material matRuongDo = CreateURPMaterial("Assets/Prefabs/CustomPrefabs/RuongDo/Mat_RuongDo.mat", "Assets/Prefabs/CustomPrefabs/RuongDo/RuongDo_Texture.png", defaultTile);
                Material matBepGas = CreateURPMaterial("Assets/Prefabs/CustomPrefabs/Bep_gas/bep_gas.mat", "Assets/Prefabs/CustomPrefabs/Bep_gas/bep_gas.png", defaultTile);
                Material matAltar = CreateURPMaterial("Assets/Prefabs/CustomPrefabs/Altar/Mat_Altar.mat", "Assets/Prefabs/CustomPrefabs/Altar/Altar_Texture.png", defaultTile);
                
                // Material Sàn gỗ lót dưới đáy nhà
                string woodTexPath = "Assets/Prefabs/CustomPrefabs/KhungNha/WoodFloor_Texture.png";
                Material matSanGo = CreateURPMaterial("Assets/Prefabs/CustomPrefabs/KhungNha/Mat_SanGoLot.mat", woodTexPath, new Vector2(3f, 3f));

                // 3. Khởi tạo Container chính
                GameObject container = new GameObject("HouseInterior");
                container.transform.localScale = Vector3.one;

                // Helper nạp trực tiếp file mô hình FBX gốc
                GameObject SpawnFBXModel(Transform parent, string fbxPath, string name, Material mat, Vector3 localPos, Vector3 localRot, Vector3 localScale)
                {
                    GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                    if (fbx == null)
                    {
                        Debug.LogError($"[HOUSE ASSEMBLY] KHÔNG THỂ LOAD FBX GỐC TẠI: {fbxPath}");
                        return null;
                    }
                    GameObject obj = Object.Instantiate(fbx, parent);
                    obj.name = name;
                    obj.transform.localPosition = localPos;
                    obj.transform.localRotation = Quaternion.Euler(localRot);
                    obj.transform.localScale = localScale;

                    foreach (var rend in obj.GetComponentsInChildren<Renderer>())
                    {
                        if (mat != null)
                        {
                            var mats = rend.sharedMaterials;
                            for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                            rend.sharedMaterials = mats;
                        }
                    }

                    BoxCollider bc = obj.AddComponent<BoxCollider>();

                    return obj;
                }

                // 4. Phóng cao Khung Nhà Gỗ chính (Tỉ lệ 600x650x600), giữ góc xoay RX: -90
                Vector3 houseScale = new Vector3(600f, 650f, 600f);
                Vector3 houseRot = new Vector3(-90f, 0f, 0f);

                GameObject houseFrame = SpawnFBXModel(container.transform, "Assets/Prefabs/CustomPrefabs/KhungNha/KhungNha_Model.fbx", "HouseFrame", matKhungNha, Vector3.zero, houseRot, houseScale);

                if (houseFrame != null)
                {
                    // 5. 🪵 MẶT SÀN GỖ LÓT ĐẶT TẠI PZ: -0.001f
                    GameObject floorBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    floorBoard.name = "WoodenFloorMesh";
                    floorBoard.transform.SetParent(houseFrame.transform, false);
                    floorBoard.transform.localPosition = new Vector3(0f, 0f, -0.001f);
                    floorBoard.transform.localRotation = Quaternion.identity;
                    floorBoard.transform.localScale = new Vector3(0.045f, 0.045f, 0.0005f);

                    MeshRenderer floorRend = floorBoard.GetComponent<MeshRenderer>();
                    if (floorRend != null)
                    {
                        floorRend.sharedMaterial = matSanGo;
                    }
                    Collider floorCol = floorBoard.GetComponent<Collider>();
                    if (floorCol != null) Object.DestroyImmediate(floorCol);


                    // 6. GÁN CHÍNH XÁC PZ = 0 CHO TẤT CẢ ĐỒ ĐẠC VÀ PY = 0.006 CHO RƯƠNG ĐỒ THEO CHỈ ĐỊNH CỦA BẠN:
                    // 🚪 CỬA CHÍNH (PY: 0.0088, PZ: 0.0012)
                    SpawnFBXModel(houseFrame.transform, "Assets/Prefabs/CustomPrefabs/CuaChinh/CuaChinh_Model.fbx", "MainDoor", matCuaChinh, new Vector3(0f, 0.0088f, 0.0012f), Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f));

                    // ⛩️ BÀN THỜ GIA TIÊN (PZ: 0)
                    SpawnFBXModel(houseFrame.transform, "Assets/Prefabs/CustomPrefabs/Altar/Altar_Model.fbx", "AncestralAltarModel", matAltar, new Vector3(0.007f, 0.005f, 0f), new Vector3(0f, 0f, 180f), new Vector3(0.1f, 0.1f, 0.1f));

                    // 📦 RƯƠNG ĐỒ (PY: 0.006, PZ: 0)
                    SpawnFBXModel(houseFrame.transform, "Assets/Prefabs/CustomPrefabs/RuongDo/RuongDo_Model.fbx", "RuongDoModel", matRuongDo, new Vector3(-0.006f, 0.006f, 0f), Vector3.zero, new Vector3(0.15f, 0.15f, 0.15f));

                    // 🛏️ GIƯỜNG NGỦ (PZ: 0)
                    SpawnFBXModel(houseFrame.transform, "Assets/Prefabs/CustomPrefabs/GiuongNgu/GiuongNgu_Model.fbx", "GiuongNguModel", matGiuongNgu, new Vector3(-0.006f, -0.005f, 0f), new Vector3(0f, 0f, 180f), new Vector3(0.3f, 0.3f, 0.3f));

                    // 🍳 BẾP GAS (PZ: 0)
                    SpawnFBXModel(houseFrame.transform, "Assets/Prefabs/CustomPrefabs/Bep_gas/bep_gas.fbx", "BepGasModel", matBepGas, new Vector3(0.005f, -0.005f, 0f), Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f));
                }

                // 7. Dựng 4 Bức tường cản vật lý kín 100% xung quanh không gian nhà
                GameObject wallsGroup = new GameObject("PhysicalWalls");
                wallsGroup.transform.SetParent(container.transform, false);

                void AddWallCollider(Vector3 center, Vector3 size)
                {
                    GameObject w = new GameObject("WallCollider");
                    w.transform.SetParent(wallsGroup.transform, false);
                    w.transform.localPosition = center;
                    BoxCollider bc = w.AddComponent<BoxCollider>();
                    bc.size = size;
                }

                // Tường sau
                AddWallCollider(new Vector3(0f, 7f, 20f), new Vector3(40f, 14f, 0.5f));
                // Tường trái
                AddWallCollider(new Vector3(-20f, 7f, 0f), new Vector3(0.5f, 14f, 40f));
                // Tường phải
                AddWallCollider(new Vector3(16f, 7f, 0f), new Vector3(0.5f, 14f, 40f));
                // Tường trước (2 bên cửa)
                AddWallCollider(new Vector3(-13f, 7f, -20f), new Vector3(14f, 14f, 0.5f));
                AddWallCollider(new Vector3(13f, 7f, -20f), new Vector3(14f, 14f, 0.5f));
                // Sàn nhà (Nằm chuẩn sàn phẳng Y: 0)
                AddWallCollider(new Vector3(0f, 0f, 0f), new Vector3(40f, 0.1f, 40f));

                // 8. Ánh sáng vàng ấm áp lan tỏa không gian nhà
                GameObject lightObj = new GameObject("IndoorWarmLight");
                lightObj.transform.SetParent(container.transform, false);
                lightObj.transform.localPosition = new Vector3(0f, 12.0f, 0f);
                Light lightComp = lightObj.AddComponent<Light>();
                lightComp.type = LightType.Point;
                lightComp.color = new Color(1f, 0.85f, 0.6f);
                lightComp.intensity = 9.5f;
                lightComp.range = 80f;

                // 9. Lưu thành Prefab hoàn chỉnh
                string prefabPath = "Assets/Resources/Prefabs/HouseInterior.prefab";
                PrefabUtility.SaveAsPrefabAsset(container, prefabPath);
                Object.DestroyImmediate(container);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"[HOUSE ASSEMBLY] Đã hoàn thành gán PZ = 0 cho đồ đạc và PY = 0.006 cho Rương đồ tại '{prefabPath}'!");
            }
            finally
            {
                isAssembling = false;
            }
        }
    }
}
