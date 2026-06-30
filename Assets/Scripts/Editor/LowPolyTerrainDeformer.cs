using UnityEngine;
using UnityEditor;
using System.IO;

namespace SownInStone.Editor
{
    public class LowPolyTerrainDeformer : EditorWindow
    {
        [MenuItem("Sown In Stone/Terrain Deformer Window")]
        public static void ShowWindow()
        {
            GetWindow<LowPolyTerrainDeformer>("Terrain Deformer");
        }

        [MenuItem("Sown In Stone/Deform Ground Terrain")]
        public static void DeformTerrainMenuItem()
        {
            DeformTerrain();
        }

        private void OnGUI()
        {
            GUILayout.Label("Low Poly Terrain Deformer", EditorStyles.boldLabel);
            GUILayout.Space(10);
            if (GUILayout.Button("Deform Ground Terrain"))
            {
                DeformTerrain();
            }
        }

        public static float CalculateY(float x, float z)
        {
            float dist = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
            if (dist <= 14f) return 0f;

            // Transition from 14m (0 height) to 23m (full height potential)
            float t = Mathf.Clamp01((dist - 14f) / (23f - 14f));
            float falloff = 0.5f * (1f - Mathf.Cos(t * Mathf.PI));

            // Tighter, sharper crests and troughs (higher frequency)
            float noiseScale = 0.45f;
            float perlin = Mathf.PerlinNoise(x * noiseScale + 123.456f, z * noiseScale + 789.101f);

            // Allow outer boundary vertices to scale up between +1.2f and +1.8f
            float height = 1.2f + perlin * (1.8f - 1.2f);

            return falloff * height;
        }

        public static void DeformTerrain()
        {
            Debug.Log("[TerrainDeformer] Starting terrain deformation...");

            // 1. Scene checks
            var groundGO = GameObject.Find("Ground_Main");
            if (groundGO == null)
            {
                Debug.LogError("[TerrainDeformer] Ground_Main GameObject not found in the scene.");
                return;
            }

            // 2. Generate mesh parameters
            float width = 50f;
            float depth = 50f;
            int gridSize = 20;

            float dx = width / gridSize;
            float dz = depth / gridSize;

            // Since we want flat-shaded (faceted) look, we must duplicate vertices per triangle.
            // A grid has gridSize * gridSize quads.
            // Each quad has 2 triangles = 6 vertices.
            int numQuads = gridSize * gridSize;
            int totalVerts = numQuads * 6;

            Vector3[] vertices = new Vector3[totalVerts];
            Vector2[] uvs = new Vector2[totalVerts];
            int[] triangles = new int[totalVerts];

            int vertIdx = 0;

            for (int r = 0; r < gridSize; r++)
            {
                for (int c = 0; c < gridSize; c++)
                {
                    float x0 = -width / 2f + c * dx;
                    float x1 = x0 + dx;
                    float z0 = -depth / 2f + r * dz;
                    float z1 = z0 + dz;

                    float y00 = CalculateY(x0, z0);
                    float y10 = CalculateY(x1, z0);
                    float y01 = CalculateY(x0, z1);
                    float y11 = CalculateY(x1, z1);

                    // Corner positions
                    Vector3 p00 = new Vector3(x0, y00, z0);
                    Vector3 p10 = new Vector3(x1, y10, z0);
                    Vector3 p01 = new Vector3(x0, y01, z1);
                    Vector3 p11 = new Vector3(x1, y11, z1);

                    // UVs
                    Vector2 uv00 = new Vector2((float)c / gridSize, (float)r / gridSize);
                    Vector2 uv10 = new Vector2((float)(c + 1) / gridSize, (float)r / gridSize);
                    Vector2 uv01 = new Vector2((float)c / gridSize, (float)(r + 1) / gridSize);
                    Vector2 uv11 = new Vector2((float)(c + 1) / gridSize, (float)(r + 1) / gridSize);

                    // Triangle 1: p00 -> p01 -> p11
                    vertices[vertIdx] = p00;
                    uvs[vertIdx] = uv00;
                    triangles[vertIdx] = vertIdx;
                    vertIdx++;

                    vertices[vertIdx] = p01;
                    uvs[vertIdx] = uv01;
                    triangles[vertIdx] = vertIdx;
                    vertIdx++;

                    vertices[vertIdx] = p11;
                    uvs[vertIdx] = uv11;
                    triangles[vertIdx] = vertIdx;
                    vertIdx++;

                    // Triangle 2: p00 -> p11 -> p10
                    vertices[vertIdx] = p00;
                    uvs[vertIdx] = uv00;
                    triangles[vertIdx] = vertIdx;
                    vertIdx++;

                    vertices[vertIdx] = p11;
                    uvs[vertIdx] = uv11;
                    triangles[vertIdx] = vertIdx;
                    vertIdx++;

                    vertices[vertIdx] = p10;
                    uvs[vertIdx] = uv10;
                    triangles[vertIdx] = vertIdx;
                    vertIdx++;
                }
            }

            // Create Mesh
            Mesh mesh = new Mesh();
            mesh.name = "Ground_LowPoly_Dunes";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Save mesh asset
            string directoryPath = "Assets/Meshes";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string assetPath = "Assets/Meshes/Ground_LowPoly_Dunes.asset";
            Mesh existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (existingMesh != null)
            {
                // Overwrite mesh content safely
                existingMesh.Clear();
                existingMesh.vertices = mesh.vertices;
                existingMesh.uv = mesh.uv;
                existingMesh.triangles = mesh.triangles;
                existingMesh.RecalculateNormals();
                existingMesh.RecalculateBounds();
                EditorUtility.SetDirty(existingMesh);
                AssetDatabase.SaveAssets();
                Debug.Log($"[TerrainDeformer] Overwrote existing mesh asset at {assetPath}");
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, assetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[TerrainDeformer] Created new mesh asset at {assetPath}");
            }

            AssetDatabase.Refresh();

            // Re-load the saved mesh to make sure references are correct
            Mesh finalMesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);

            // 3. Apply to Ground_Main GameObject
            var mf = groundGO.GetComponent<MeshFilter>();
            if (mf == null)
            {
                mf = groundGO.AddComponent<MeshFilter>();
            }
            mf.sharedMesh = finalMesh;

            var mc = groundGO.GetComponent<MeshCollider>();
            if (mc == null)
            {
                mc = groundGO.AddComponent<MeshCollider>();
            }
            mc.sharedMesh = finalMesh;

            // Reset Ground_Main scale to (1,1,1) if it isn't already, since our mesh covers 50x50m footprint in local space
            groundGO.transform.localScale = Vector3.one;

            EditorUtility.SetDirty(groundGO);
            Debug.Log("[TerrainDeformer] Applied Ground_LowPoly_Dunes mesh to Ground_Main MeshFilter and MeshCollider.");

            // 4. Delete or hide Sandy_Patches and its children
            var sandyPatches = GameObject.Find("Sandy_Patches");
            if (sandyPatches != null)
            {
                Undo.DestroyObjectImmediate(sandyPatches);
                Debug.Log("[TerrainDeformer] Sandy_Patches container destroyed.");
            }

            // Also check for "DryGrass_Patch" just in case there is any separate object
            var dryGrassPatch = GameObject.Find("DryGrass_Patch");
            if (dryGrassPatch != null)
            {
                Undo.DestroyObjectImmediate(dryGrassPatch);
                Debug.Log("[TerrainDeformer] DryGrass_Patch gameobject destroyed.");
            }

            // 5. Ground palm trees, rocks, player and NPCs
            GroundEnvironmentObjects();

            // Save scene changes
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[TerrainDeformer] Terrain deformed and scene saved successfully!");
        }

        private static void GroundEnvironmentObjects()
        {
            Debug.Log("[TerrainDeformer] Starting terrain grounding for environmental objects...");

            if (Terrain.activeTerrain == null)
            {
                Debug.LogWarning("[TerrainDeformer] No active Terrain found — falling back to CalculateY() math.");
            }

            // Find all vegetation under Vegetation container
            var vegetation = GameObject.Find("Environment/_Environment/Vegetation");
            int vegCount = 0;
            if (vegetation != null)
            {
                for (int i = 0; i < vegetation.transform.childCount; i++)
                {
                    GroundObject(vegetation.transform.GetChild(i).gameObject);
                    vegCount++;
                }
            }
            Debug.Log($"[TerrainDeformer] Grounded {vegCount} vegetation items.");

            // Find all rocks under Rocks container
            var rocks = GameObject.Find("Environment/_Environment/Rocks");
            int rockCount = 0;
            if (rocks != null)
            {
                for (int i = 0; i < rocks.transform.childCount; i++)
                {
                    GroundObject(rocks.transform.GetChild(i).gameObject);
                    rockCount++;
                }
            }

            // Also find any loose root-level DuneRock_ objects
            var allGOs = Object.FindObjectsOfType<GameObject>();
            foreach (var go in allGOs)
            {
                if (go.name.StartsWith("DuneRock_") && go.transform.parent == null)
                {
                    GroundObject(go);
                    rockCount++;
                }
            }
            Debug.Log($"[TerrainDeformer] Grounded {rockCount} rocks.");

            // Ground Player
            var player = GameObject.Find("Player");
            if (player != null)
            {
                GroundObject(player);
                Debug.Log("[TerrainDeformer] Grounded Player.");
            }

            // Ground NPCs (check both hierarchy paths)
            var npcs = GameObject.Find("Environment/_Environment/NPCs");
            if (npcs == null) npcs = GameObject.Find("NPCs");
            int npcCount = 0;
            if (npcs != null)
            {
                for (int i = 0; i < npcs.transform.childCount; i++)
                {
                    GroundObject(npcs.transform.GetChild(i).gameObject);
                    npcCount++;
                }
            }
            Debug.Log($"[TerrainDeformer] Grounded {npcCount} NPCs.");
        }

        /// <summary>
        /// Returns the world-space Y height of the active Unity Terrain at position (x, z).
        /// Falls back to CalculateY() if no Terrain is present.
        /// </summary>
        public static float GetTerrainY(float x, float z)
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                float sampledHeight = terrain.SampleHeight(new Vector3(x, 0f, z));
                return terrain.transform.position.y + sampledHeight;
            }
            // Fallback: use the legacy mathematical height formula
            return CalculateY(x, z);
        }

        /// <summary>
        /// Snaps a GameObject's Y position to the active Terrain surface at its current (X, Z).
        /// Records an Undo operation and marks the scene dirty.
        /// </summary>
        public static void GroundObjectToTerrain(GameObject go, float yOffset = 0f)
        {
            if (go == null) return;
            float x = go.transform.position.x;
            float z = go.transform.position.z;
            float targetY = GetTerrainY(x, z) + yOffset;

            Undo.RecordObject(go.transform, "Ground Object To Terrain");
            go.transform.position = new Vector3(x, targetY, z);
            EditorUtility.SetDirty(go.transform);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
        }

        private static void GroundObject(GameObject go)
        {
            GroundObjectToTerrain(go, 0f);
        }
    }
}
