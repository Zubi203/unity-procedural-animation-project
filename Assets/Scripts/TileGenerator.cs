using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    [SerializeField] private int noiseSampleSize = 300;
    [SerializeField] private float noiseScale = 1f;
    [SerializeField] private float maxHeight = 1f;

    private MeshFilter mapMeshFilter;
    private MeshCollider mapMeshCollider;
    private MeshRenderer mapMeshRenderer;

    void Start()
    {
        mapMeshFilter = GetComponent<MeshFilter>();
        mapMeshCollider = GetComponent<MeshCollider>();
        mapMeshRenderer = GetComponent<MeshRenderer>();
        GenerateMap();
    }

    void GenerateMap()
    {
        float[,] heightMap = NoiseGenerator.GenerateNoiseMap(noiseSampleSize, noiseScale);

        Vector3[] verts = mapMeshFilter.mesh.vertices;

        for (int x = 0; x < noiseSampleSize; x++)
        {
            for (int z = 0; z < noiseSampleSize; z++)
            {
                int index = (x * noiseSampleSize) + z;
                verts[index].y = heightMap[x, z] * maxHeight;
            }
        }

        mapMeshFilter.mesh.vertices = verts;
        mapMeshFilter.mesh.RecalculateBounds();
        mapMeshFilter.mesh.RecalculateNormals();

        mapMeshCollider.sharedMesh = mapMeshFilter.mesh;

    }
}
