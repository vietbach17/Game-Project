using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SownInStone.Agriculture;

namespace SownInStone.Editor
{
    /// <summary>
    /// Editor script to link the 3D Visual GameObjects to the SoilCell component fields.
    /// This fixes the links in memory/serialize state so they don't get lost.
    /// </summary>
    public class SetupSoilVisuals
    {
        [MenuItem("Sown In Stone/Setup 3D Soil Visuals")]
        public static void Setup()
        {
            // Find all SoilCell components in the scene
            SoilCell[] allSoils = Object.FindObjectsByType<SoilCell>(FindObjectsSortMode.None);
            
            int linkedCount = 0;
            foreach (var s in allSoils)
            {
                string name = s.gameObject.name;
                if (name.StartsWith("SoilCell_Grid") || name == "SoilCell")
                {
                    Transform visualRoot = s.transform.Find("Visual");
                    if (visualRoot != null)
                    {
                        Transform rocky = visualRoot.Find("Rocky_Soil");
                        Transform clean = visualRoot.Find("Clean_Soil");
                        Transform tilled = visualRoot.Find("Tilled_Soil");
                        Transform wet = visualRoot.Find("Wet_Soil");
                        
                        Undo.RecordObject(s, "Link 3D Soil Visuals");
                        s.rockySoilVisual = rocky != null ? rocky.gameObject : null;
                        s.cleanSoilVisual = clean != null ? clean.gameObject : null;
                        s.tilledSoilVisual = tilled != null ? tilled.gameObject : null;
                        s.wetSoilVisual = wet != null ? wet.gameObject : null;
                        
                        // Disable SpriteRenderer if present
                        SpriteRenderer sr = s.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            Undo.RecordObject(sr, "Disable SpriteRenderer");
                            sr.enabled = false;
                        }
                        
                        // Configure BoxCollider
                        BoxCollider bc = s.GetComponent<BoxCollider>();
                        if (bc != null)
                        {
                            Undo.RecordObject(bc, "Configure BoxCollider");
                            bc.size = new Vector3(2f, 0.3f, 2f);
                            bc.center = new Vector3(0f, 0.1f, 0f);
                        }
                        
                        // Configure Transform scale and rotation
                        Undo.RecordObject(s.transform, "Align Soil Transform");
                        s.transform.localScale = Vector3.one;
                        s.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
                        
                        EditorUtility.SetDirty(s);
                        linkedCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"[SETUP SOILS] No child 'Visual' found under {name}!");
                    }
                }
            }
            
            if (linkedCount > 0)
            {
                if (!Application.isPlaying)
                {
                    var activeScene = EditorSceneManager.GetActiveScene();
                    EditorSceneManager.MarkSceneDirty(activeScene);
                    bool saved = EditorSceneManager.SaveScene(activeScene);
                    Debug.Log($"[SETUP SOILS] Successfully linked 3D Visual fields for {linkedCount} cells and saved scene: {saved}");
                }
                else
                {
                    Debug.Log($"[SETUP SOILS] Successfully linked 3D Visual fields for {linkedCount} cells (PlayMode active, scene not saved).");
                }
            }
            else
            {
                Debug.LogWarning("[SETUP SOILS] No cells processed or linked!");
            }
        }
    }
}
