using UnityEngine;

namespace SownInStone.Core
{
    /// <summary>
    /// Tạo mặt biển (ocean plane) lớn bao quanh mảnh đất,
    /// tạo chiều sâu và cảm giác đảo cho cảnh quan làng quê miền Trung.
    /// Mặt biển có hiệu ứng sóng nhẹ nhàng bằng vertex animation.
    /// Gắn script này vào bất kỳ GameObject nào trong scene.
    /// </summary>
    public class OceanGenerator : MonoBehaviour
    {
        public static OceanGenerator Instance { get; private set; }

        [Header("--- KÍCH THƯỚC BIỂN ---")]
        [Tooltip("Kích thước mặt biển (mét). Nên lớn hơn terrain nhiều lần.")]
        [SerializeField] private float oceanSize = 500f;

        [Tooltip("Độ phân giải grid (số ô mỗi chiều). Cao hơn = sóng mượt hơn nhưng tốn hiệu năng.")]
        [SerializeField] private int gridResolution = 50;

        [Header("--- VỊ TRÍ ---")]
        [Tooltip("Độ cao mặt biển (Y). Nên thấp hơn terrain để tạo chiều sâu. Giá trị âm = biển nằm dưới đất.")]
        [SerializeField] private float oceanY = -0.8f;

        [Header("--- MÀU SẮC ---")]
        [Tooltip("Màu nước biển nông (gần bờ).")]
        [SerializeField] private Color shallowColor = new Color(0.15f, 0.55f, 0.65f, 0.85f);
        
        [Tooltip("Màu nước biển sâu (xa bờ).")]
        [SerializeField] private Color deepColor = new Color(0.05f, 0.18f, 0.35f, 0.92f);

        [Header("--- HIỆU ỨNG SÓNG ---")]
        [Tooltip("Biên độ sóng (mét).")]
        [SerializeField] private float waveAmplitude = 0.15f;
        
        [Tooltip("Tần số sóng.")]
        [SerializeField] private float waveFrequency = 0.8f;
        
        [Tooltip("Tốc độ sóng.")]
        [SerializeField] private float waveSpeed = 0.6f;

        [Header("--- LỖ TRỐNG CHO ĐẤT LIỀN ---")]
        [Tooltip("Bán kính vùng đất liền không có nước (mét). Tự động tính từ terrain nếu = 0.")]
        [SerializeField] private float landRadius = 40f;

        [Tooltip("Độ rộng vùng chuyển tiếp bờ biển (mét). Nước mờ dần vào đất.")]
        [SerializeField] private float shoreTransition = 8f;

        // Runtime
        private Mesh oceanMesh;
        private Vector3[] baseVertices;
        private Vector3[] animatedVertices;
        private Color[] vertexColors;
        private MeshFilter meshFilter;
        private Vector3 terrainCenter; // Trung tâm terrain (auto-detected)

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            DetectTerrainCenter();
            GenerateOcean();
        }

        /// <summary>
        /// Tự động tìm trung tâm terrain từ scene.
        /// </summary>
        private void DetectTerrainCenter()
        {
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                Vector3 terrainPos = terrain.transform.position;
                Vector3 terrainSize = terrain.terrainData.size;
                terrainCenter = new Vector3(
                    terrainPos.x + terrainSize.x / 2f,
                    0f,
                    terrainPos.z + terrainSize.z / 2f
                );
                Debug.Log($"[OceanGenerator] Terrain center detected: {terrainCenter}");
            }
            else
            {
                terrainCenter = Vector3.zero;
                Debug.LogWarning("[OceanGenerator] No terrain found, ocean centered at origin.");
            }
        }

        private void Update()
        {
            if (oceanMesh == null || baseVertices == null) return;
            AnimateWaves();
        }

        private void GenerateOcean()
        {
            // Tạo child GameObject cho mặt biển
            GameObject oceanGO = new GameObject("Ocean_Plane");
            oceanGO.transform.SetParent(transform);
            oceanGO.transform.localPosition = new Vector3(0f, oceanY, 0f);
            oceanGO.layer = LayerMask.NameToLayer("Water");
            if (oceanGO.layer == -1) oceanGO.layer = 0; // Fallback nếu không có layer Water

            // MeshFilter & MeshRenderer
            meshFilter = oceanGO.AddComponent<MeshFilter>();
            MeshRenderer renderer = oceanGO.AddComponent<MeshRenderer>();

            // Tạo material nước biển URP với vertex color
            Shader oceanShader = Shader.Find("Custom/OceanVertexColor");
            if (oceanShader == null)
            {
                Debug.LogWarning("[OceanGenerator] Custom/OceanVertexColor shader not found! Falling back to URP/Unlit.");
                oceanShader = Shader.Find("Universal Render Pipeline/Unlit");
            }
            Material waterMat = new Material(oceanShader);
            waterMat.color = shallowColor;
            waterMat.renderQueue = 3000;
            renderer.material = waterMat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;

            // Tạo mesh
            oceanMesh = CreateOceanMesh();
            meshFilter.mesh = oceanMesh;
        }

        private Mesh CreateOceanMesh()
        {
            int res = gridResolution;
            int vertCount = (res + 1) * (res + 1);
            
            baseVertices = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];
            vertexColors = new Color[vertCount];
            int[] triangles = new int[res * res * 6];

            float halfSize = oceanSize / 2f;
            float step = oceanSize / res;

            // Tạo vertices
            for (int z = 0; z <= res; z++)
            {
                for (int x = 0; x <= res; x++)
                {
                    int idx = z * (res + 1) + x;
                    float worldX = -halfSize + x * step;
                    float worldZ = -halfSize + z * step;

                    baseVertices[idx] = new Vector3(worldX, 0f, worldZ);
                    uvs[idx] = new Vector2((float)x / res, (float)z / res);

                    // Tính khoảng cách đến trung tâm terrain (có offset) để tạo gradient màu và lỗ trống
                    float dx2 = worldX - terrainCenter.x;
                    float dz2 = worldZ - terrainCenter.z;
                    float dist = Mathf.Sqrt(dx2 * dx2 + dz2 * dz2);
                    
                    // Gradient màu: gần bờ = nông (shallowColor), xa = sâu (deepColor)
                    float colorT = Mathf.Clamp01((dist - landRadius) / (oceanSize * 0.3f));
                    Color vertColor = Color.Lerp(shallowColor, deepColor, colorT);

                    // Vùng đất liền: ẩn nước bằng alpha = 0
                    if (dist < landRadius)
                    {
                        vertColor.a = 0f;
                    }
                    else if (dist < landRadius + shoreTransition)
                    {
                        // Vùng chuyển tiếp bờ biển — alpha tăng dần
                        float shoreT = (dist - landRadius) / shoreTransition;
                        vertColor.a *= shoreT;
                    }

                    vertexColors[idx] = vertColor;
                }
            }

            // Tạo triangles
            int triIdx = 0;
            for (int z = 0; z < res; z++)
            {
                for (int x = 0; x < res; x++)
                {
                    int bottomLeft = z * (res + 1) + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = (z + 1) * (res + 1) + x;
                    int topRight = topLeft + 1;

                    triangles[triIdx++] = bottomLeft;
                    triangles[triIdx++] = topLeft;
                    triangles[triIdx++] = topRight;

                    triangles[triIdx++] = bottomLeft;
                    triangles[triIdx++] = topRight;
                    triangles[triIdx++] = bottomRight;
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "Ocean_Mesh";
            mesh.vertices = baseVertices;
            mesh.uv = uvs;
            mesh.colors = vertexColors;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Copy để dùng cho animation
            animatedVertices = new Vector3[baseVertices.Length];
            System.Array.Copy(baseVertices, animatedVertices, baseVertices.Length);

            return mesh;
        }

        private void AnimateWaves()
        {
            float time = Time.time * waveSpeed;

            for (int i = 0; i < baseVertices.Length; i++)
            {
                Vector3 basePos = baseVertices[i];
                float dx2 = basePos.x - terrainCenter.x;
                float dz2 = basePos.z - terrainCenter.z;
                float dist = Mathf.Sqrt(dx2 * dx2 + dz2 * dz2);

                // Không animate sóng trong vùng đất liền
                if (dist < landRadius)
                {
                    animatedVertices[i] = basePos;
                    continue;
                }

                // Tính chiều cao sóng — nhiều lớp sóng chồng lên nhau
                float wave1 = Mathf.Sin(basePos.x * waveFrequency + time) * waveAmplitude;
                float wave2 = Mathf.Sin(basePos.z * waveFrequency * 0.7f + time * 1.3f) * waveAmplitude * 0.5f;
                float wave3 = Mathf.Sin((basePos.x + basePos.z) * waveFrequency * 0.5f + time * 0.8f) * waveAmplitude * 0.3f;

                // Sóng mạnh hơn ở xa bờ
                float waveStrength = Mathf.Clamp01((dist - landRadius) / (shoreTransition * 2f));
                float totalWave = (wave1 + wave2 + wave3) * waveStrength;

                animatedVertices[i] = new Vector3(basePos.x, basePos.y + totalWave, basePos.z);
            }

            oceanMesh.vertices = animatedVertices;
            oceanMesh.RecalculateNormals();
        }

        /// <summary>
        /// Cho phép điều chỉnh độ cao mặt biển runtime (VD: khi lũ dâng).
        /// </summary>
        public void SetOceanHeight(float y)
        {
            oceanY = y;
            Transform oceanTransform = transform.Find("Ocean_Plane");
            if (oceanTransform != null)
            {
                oceanTransform.localPosition = new Vector3(0f, oceanY, 0f);
            }
        }
    }
}
