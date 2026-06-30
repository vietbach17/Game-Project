using UnityEngine;
using UnityEditor;
using System.IO;
using SownInStone.Storage;

namespace SownInStone.Editor
{
    public class SetupPlasticMulchAsset : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                if (str.Contains("MangNilon.png") || str.Contains("PlasticMulch"))
                {
                    Setup();
                    break;
                }
            }
        }

        [MenuItem("Sown In Stone/Setup Plastic Mulch Asset")]
        public static void Setup()
        {
            Debug.Log("[SETUP MULCH] Bắt đầu cấu hình Asset Màng Bọc Nilon với hình ảnh MangNilon.png...");

            string prefabDir = "Assets/Resources/Prefabs";
            string dataDir = "Assets/Data";
            if (!Directory.Exists(prefabDir)) Directory.CreateDirectory(prefabDir);
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

            // 1. Cấu hình TextureImporter cho hình ảnh Icon MangNilon.png
            string iconPath = "Assets/Prefabs/PlasticMulch/MangNilon.png";
            TextureImporter iconImporter = AssetImporter.GetAtPath(iconPath) as TextureImporter;
            if (iconImporter != null && iconImporter.textureType != TextureImporterType.Sprite)
            {
                iconImporter.textureType = TextureImporterType.Sprite;
                iconImporter.spriteImportMode = SpriteImportMode.Single;
                iconImporter.SaveAndReimport();
            }

            // 2. Tạo Material từ Texture Màng Nilon (MangNilon_Texture.png)
            string matPath = "Assets/Prefabs/PlasticMulch/Mat_PlasticMulch.mat";
            string texPath = "Assets/Prefabs/PlasticMulch/MangNilon_Texture.png";
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            Shader mulchShader = Shader.Find("Universal Render Pipeline/Lit");
            if (mulchShader == null) mulchShader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (mulchShader == null) mulchShader = Shader.Find("Standard");

            if (mat == null)
            {
                mat = new Material(mulchShader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = mulchShader;
            }
            if (tex != null)
            {
                mat.mainTexture = tex;
                mat.color = Color.white;
            }

            // 3. Load Model FBX
            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlasticMulch/MangNilon_Model.fbx");
            if (fbx != null)
            {
                string prefabPath = "Assets/Resources/Prefabs/PlasticMulch.prefab";
                GameObject container = new GameObject("PlasticMulch");
                GameObject visual = Object.Instantiate(fbx, container.transform);
                visual.name = "VisualModel";

                foreach (var rend in visual.GetComponentsInChildren<Renderer>())
                {
                    if (rend == null) continue;
                    var mats = rend.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] == null || mats[i].shader == null || mats[i].shader.name.Contains("Error"))
                        {
                            mats[i] = mat;
                        }
                    }
                    rend.sharedMaterials = mats;
                }

                PrefabUtility.SaveAsPrefabAsset(container, prefabPath);
                Object.DestroyImmediate(container);
                Debug.Log($"[SETUP MULCH] Đã tạo thành công Prefab tại '{prefabPath}'");
            }

            // 4. Tạo ItemData cho Màng Bọc Nilon và gán Icon từ MangNilon.png
            string itemAssetPath = "Assets/Data/Item_plastic_mulch.asset";
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(itemAssetPath);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ItemData>();
                AssetDatabase.CreateAsset(item, itemAssetPath);
            }

            item.ItemID = "item_plastic_mulch";
            item.ItemName = "Màng Bọc Nilon Chống Bão";
            item.Description = "Màng bọc nilon chuyên dụng phủ bảo vệ toàn bộ các ô ruộng khỏi giông bão, ngập úng và xói mòn đất.";
            item.type = ItemType.VatLieu;

            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (iconSprite != null)
            {
                item.Icon = iconSprite;
            }

            EditorUtility.SetDirty(item);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SETUP MULCH] Đã gán thành công hình ảnh MangNilon.png làm Icon cho Màng Bọc Nilon!");
        }
    }
}
