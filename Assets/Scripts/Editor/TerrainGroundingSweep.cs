using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using SownInStone.Agriculture;

namespace SownInStone.Editor
{
    /// <summary>
    /// Scene-wide grounding sweep for the Village_Demo scene.
    /// Snaps all entities (NPCs, structures, vegetation, rocks, farming grid)
    /// to the surface of the active Unity Terrain object.
    /// 
    /// Usage: Unity menu → Sown In Stone → Terrain Grounding Sweep
    /// </summary>
    public class TerrainGroundingSweep : EditorWindow
    {
        // ─── Y offsets ────────────────────────────────────────────────────────────
        // Characters (Player / NPCs) land feet-first at exact terrain surface
        private const float CHAR_Y_OFFSET      = 0f;
        // Structures sit exactly on the terrain surface (pivot at base)
        private const float STRUCT_Y_OFFSET    = 0f;
        // Vegetation (trees, bamboo) rooted at terrain level
        private const float VEG_Y_OFFSET       = 0f;
        // Rocks sit at terrain level
        private const float ROCK_Y_OFFSET      = 0f;
        // Farming parent SoilCell_1 raised 0.02 m above terrain
        private const float SOIL_PARENT_OFFSET = 0.02f;
        // Individual soil grid cells raised 0.13 m above terrain (mesh height)
        private const float SOIL_GRID_OFFSET   = 0.13f;
        // Fences around the farm plot sit at terrain + 0.02 m
        private const float FENCE_Y_OFFSET     = 0.02f;

        // ─── NPC names ────────────────────────────────────────────────────────────
        private static readonly string[] NPC_NAMES = {
            "NPC_BacNam", "NPC_OTham", "NPC_CuBay", "NPC_BeTi"
        };

        // ─── Structural asset names ───────────────────────────────────────────────
        private static readonly string[] STRUCTURE_NAMES = {
            "Thanh_House", "BacNam_House", "OTham_Shop",
            "Village_Speaker", "Well"
        };

        // ─────────────────────────────────────────────────────────────────────────
        [MenuItem("Sown In Stone/Terrain Grounding Sweep")]
        public static void RunSweep()
        {
            // 1. Make sure Village_Demo is loaded
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Contains("Village_Demo"))
            {
                Debug.Log("[GroundSweep] Opening Assets/Scenes/Village_Demo.unity...");
                activeScene = EditorSceneManager.OpenScene(
                    "Assets/Scenes/Village_Demo.unity",
                    OpenSceneMode.Single);
            }

            // 2. Validate active Terrain
            Terrain terrain = Terrain.activeTerrain;
            if (terrain == null)
            {
                Debug.LogError(
                    "[GroundSweep] No active Terrain found in the scene. " +
                    "Ensure a Terrain GameObject is present and active. Aborting.");
                return;
            }

            Debug.Log($"[GroundSweep] Active Terrain: \"{terrain.gameObject.name}\" " +
                      $"at world position {terrain.transform.position}");

            int totalSnapped = 0;

            // 3. Snap Player
            totalSnapped += SnapByName("Player", CHAR_Y_OFFSET);

            // 4. Snap named NPCs
            foreach (var npcName in NPC_NAMES)
            {
                totalSnapped += SnapByName(npcName, CHAR_Y_OFFSET);
            }

            // 5. Snap NPCs that live under hierarchy containers
            totalSnapped += SnapChildren("Environment/_Environment/NPCs", CHAR_Y_OFFSET, "NPCs");
            totalSnapped += SnapChildren("NPCs", CHAR_Y_OFFSET, "NPCs (root)");

            // 6. Snap structural assets
            foreach (var structName in STRUCTURE_NAMES)
            {
                totalSnapped += SnapByName(structName, STRUCT_Y_OFFSET);
            }

            // 7. Snap Fences (root container children)
            totalSnapped += SnapChildren("Fences", FENCE_Y_OFFSET, "Fences");

            // 8. Snap vegetation
            totalSnapped += SnapChildren(
                "Environment/_Environment/Vegetation", VEG_Y_OFFSET, "Vegetation");

            // 9. Snap rocks under container
            totalSnapped += SnapChildren(
                "Environment/_Environment/Rocks", ROCK_Y_OFFSET, "Rocks");

            // 10. Snap loose root-level DuneRock_ objects
            int duneRockCount = SnapLooseDuneRocks();
            totalSnapped += duneRockCount;

            // 11. Agricultural Grid Realignment
            int farmSnapped = SnapFarmingPlot();
            totalSnapped += farmSnapped;

            // 12. Save scene
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool saved = EditorSceneManager.SaveOpenScenes();

            Debug.Log(
                $"[GroundSweep] ✔ Sweep complete. Snapped {totalSnapped} objects total. " +
                $"Scene saved: {saved}");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Helper: sample the active Terrain height at world (x, z)
        // ─────────────────────────────────────────────────────────────────────────

        private static float GetTerrainY(float x, float z)
        {
            // Delegate to the shared helper in LowPolyTerrainDeformer for consistency
            return LowPolyTerrainDeformer.GetTerrainY(x, z);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Snap helpers
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Snap a single GameObject by name. Returns 1 if found and snapped, 0 otherwise.</summary>
        private static int SnapByName(string name, float yOffset)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                Debug.LogWarning($"[GroundSweep] GameObject \"{name}\" not found in scene.");
                return 0;
            }
            SnapObject(go, yOffset);
            Debug.Log($"[GroundSweep] Snapped \"{go.name}\" → Y={go.transform.position.y:F4}");
            return 1;
        }

        /// <summary>Snap all direct children of a container found by path/name.</summary>
        private static int SnapChildren(string containerPath, float yOffset, string label)
        {
            var container = GameObject.Find(containerPath);
            if (container == null) return 0;

            int count = 0;
            for (int i = 0; i < container.transform.childCount; i++)
            {
                var child = container.transform.GetChild(i).gameObject;
                SnapObject(child, yOffset);
                count++;
            }
            if (count > 0)
                Debug.Log($"[GroundSweep] Snapped {count} {label} objects.");
            return count;
        }

        /// <summary>Snap all root-level GameObjects whose name starts with "DuneRock_".</summary>
        private static int SnapLooseDuneRocks()
        {
            int count = 0;
            foreach (var go in Object.FindObjectsOfType<GameObject>())
            {
                if (go.name.StartsWith("DuneRock_") && go.transform.parent == null)
                {
                    SnapObject(go, ROCK_Y_OFFSET);
                    count++;
                }
            }
            if (count > 0)
                Debug.Log($"[GroundSweep] Snapped {count} loose DuneRock objects.");
            return count;
        }

        /// <summary>
        /// Snap the 3×3 farming plot container, its 9 SoilCell_Grid children,
        /// the parent SoilCell_1, and the bep_gas parent prefab.
        /// </summary>
        private static int SnapFarmingPlot()
        {
            int count = 0;
            var allSoils = Object.FindObjectsByType<SoilCell>(FindObjectsSortMode.None);

            SoilCell parentSoil = null;
            var gridCells = new List<SoilCell>();

            foreach (var s in allSoils)
            {
                if (s.gameObject.name == "SoilCell_1")
                    parentSoil = s;
                else if (s.gameObject.name.StartsWith("SoilCell_Grid"))
                    gridCells.Add(s);
            }

            // Snap parent SoilCell_1
            if (parentSoil != null)
            {
                SnapObject(parentSoil.gameObject, SOIL_PARENT_OFFSET);
                Debug.Log($"[GroundSweep] Snapped SoilCell_1 (parent) → Y={parentSoil.transform.position.y:F4}");
                count++;
            }
            else
            {
                Debug.LogWarning("[GroundSweep] SoilCell_1 (parent) not found in scene.");
            }

            // Snap each SoilCell_Grid child
            foreach (var sc in gridCells)
            {
                SnapObject(sc.gameObject, SOIL_GRID_OFFSET);
                count++;
            }
            Debug.Log($"[GroundSweep] Snapped {gridCells.Count} SoilCell_Grid objects.");

            // Snap farm fence segments
            //  (Fences container was already swept above for village fences;
            //   run again here to be explicit, SnapChildren is idempotent)
            int fenceCount = SnapChildren("Fences", FENCE_Y_OFFSET, "Farm Fences (re-snap)");
            count += fenceCount;

            // Snap the bep_gas parent
            //   The bep_gas child has a 0.03-size BoxCollider trigger.
            //   It is a renamed child of a prefab instance whose root we find here.
            //   We climb up to the root prefab transform and snap that.
            int bepGasCount = SnapBepGasParent();
            count += bepGasCount;

            return count;
        }

        /// <summary>
        /// Locate the bep_gas named GameObject in the scene and snap the owning
        /// named prefab instance (e.g. Thanh_House) to the terrain surface.
        /// The 0.03 BoxCollider trigger center stays at (0,0,0) local space
        /// and moves automatically with its parent.
        ///
        /// NOTE: Generic scene container names (Environment, _Environment, Houses)
        /// are intentionally excluded from snapping — they must stay at Y=0.
        /// </summary>
        private static int SnapBepGasParent()
        {
            // Known scene-level container names that must NOT be snapped as entities
            var containerNames = new System.Collections.Generic.HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase)
            {
                "Environment", "_Environment", "Houses", "NPCs",
                "Vegetation", "Rocks", "DisasterObjects", "Fences"
            };

            // Search all active GameObjects for one named "bep_gas"
            GameObject bepGas = null;
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name == "bep_gas")
                {
                    bepGas = go;
                    break;
                }
            }

            if (bepGas == null)
            {
                Debug.LogWarning("[GroundSweep] bep_gas GameObject not found in scene.");
                return 0;
            }

            // Climb the hierarchy until we reach a named prefab root that is NOT
            // a generic container. The owning prefab is the highest ancestor whose
            // parent is null OR whose parent is a known container.
            Transform candidate = bepGas.transform;
            while (candidate.parent != null && !containerNames.Contains(candidate.parent.gameObject.name))
                candidate = candidate.parent;

            GameObject ownerGO = candidate.gameObject;

            // Safety guard: never snap a container by name
            if (containerNames.Contains(ownerGO.name))
            {
                Debug.LogWarning($"[GroundSweep] bep_gas hierarchy resolution landed on container " +
                                 $"\"{ownerGO.name}\" — skipping to avoid moving a scene container.");
                return 0;
            }

            SnapObject(ownerGO, STRUCT_Y_OFFSET);
            Debug.Log($"[GroundSweep] Snapped bep_gas owner \"{ownerGO.name}\" " +
                      $"→ Y={ownerGO.transform.position.y:F4}  " +
                      $"(bep_gas BoxCollider size=0.03 stays at local origin)");
            return 1;
        }

        /// <summary>
        /// Core: move a GameObject's world Y to terrain surface + yOffset.
        /// Preserves X and Z. Registers Undo.
        /// </summary>
        private static void SnapObject(GameObject go, float yOffset)
        {
            if (go == null) return;
            float x = go.transform.position.x;
            float z = go.transform.position.z;
            float targetY = GetTerrainY(x, z) + yOffset;

            Undo.RecordObject(go.transform, "Terrain Grounding Sweep");
            go.transform.position = new Vector3(x, targetY, z);
            EditorUtility.SetDirty(go.transform);
            EditorSceneManager.MarkSceneDirty(go.scene);
        }
    }
}
