using UnityEditor;
using UnityEngine;

namespace SownInStone.Editor
{
    public class TutorialTextureImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            // Apply high quality sprite settings to any textures in the Tutorial folder
            if (assetPath.Contains("Textures/Tutorial"))
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                
                // Set texture type to Sprite (2D and UI)
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                
                // Disable Mip Maps to prevent blurriness
                textureImporter.mipmapEnabled = false;
                
                // Use Bilinear filter
                textureImporter.filterMode = FilterMode.Bilinear;
                
                // Use Uncompressed (High Quality) to avoid compression artifacts
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;

                // Configure platform settings for high resolution & quality
                TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                platformSettings.maxTextureSize = 2048;
                platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
                platformSettings.format = TextureImporterFormat.RGBA32;
                platformSettings.overridden = true;
                textureImporter.SetPlatformTextureSettings(platformSettings);
            }
        }

        [MenuItem("Sown In Stone/Reimport Tutorial Textures")]
        public static void ReimportTutorialTextures()
        {
            string[] folders = new string[] { "Assets/Textures/Tutorial", "Assets/Resources/Textures/Tutorial" };
            string[] guids = AssetDatabase.FindAssets("t:Texture", folders);
            
            if (guids == null || guids.Length == 0)
            {
                Debug.LogWarning("No textures found under Assets/Textures/Tutorial or Assets/Resources/Textures/Tutorial!");
                return;
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            
            Debug.Log($"Reimported {guids.Length} tutorial textures with high-quality Sprite settings (MipMaps disabled, Compression disabled).");
        }
    }
}
