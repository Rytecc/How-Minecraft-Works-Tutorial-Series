using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public static Dictionary<Vector3Int, int[,,]> WorldData;
    public static Dictionary<Vector2Int, GameObject> ActiveChunks;
    public static readonly Vector3Int ChunkSize = new Vector3Int(16, 256, 16);

    [SerializeField] private TextureLoader TextureLoaderInstance;
    [SerializeField] private Material ChunkMaterial;
    [Space]
    public Vector2 NoiseScale = Vector2.one;
    public Vector2 NoiseOffset = Vector2.zero;
    [Space]
    public int HeightOffset = 60;
    public float HeightIntensity = 5f;

    private ChunkMeshCreator meshCreator;
    private DataGenerator dataCreator;
    void Start()
    {
        WorldData = new Dictionary<Vector3Int, int[,,]>();
        ActiveChunks = new Dictionary<Vector2Int, GameObject>();
        meshCreator = new ChunkMeshCreator(TextureLoaderInstance, this);
        dataCreator = new DataGenerator(this);
    }

    public IEnumerator CreateChunk(Vector2Int ChunkCoord)
    {
        Vector3Int pos = new Vector3Int(ChunkCoord.x, 0, ChunkCoord.y);
        string chunkName = $"Chunk {ChunkCoord.x} {ChunkCoord.y}";

        GameObject newChunk = new GameObject(chunkName, new System.Type[]
        {
            typeof(MeshRenderer),
            typeof(MeshFilter),
            typeof(MeshCollider)
        });

        newChunk.transform.position = new Vector3(ChunkCoord.x * 16, 0f, ChunkCoord.y * 16);
        ActiveChunks.Add(ChunkCoord, newChunk);

        int[,,] dataToApply = WorldData.ContainsKey(pos) ? WorldData[pos] : null;
        Mesh meshToUse = null;

        if (dataToApply == null)
        {
            dataCreator.QueueDataToGenerate(new DataGenerator.GenData
            {
                GenerationPoint = pos,
                OnComplete = x => dataToApply = x
            });

            yield return new WaitUntil(() => dataToApply != null);
        }

        meshCreator.QueueDataToDraw(new ChunkMeshCreator.CreateMesh
        {
            DataToDraw = dataToApply,
            OnComplete = x => meshToUse = x
        });

        yield return new WaitUntil(() => meshToUse != null);

        if(newChunk != null)
        {
            MeshRenderer newChunkRenderer = newChunk.GetComponent<MeshRenderer>();
            MeshFilter newChunkFilter = newChunk.GetComponent<MeshFilter>();
            MeshCollider collider = newChunk.GetComponent<MeshCollider>();

            newChunkFilter.mesh = meshToUse;
            newChunkRenderer.material = ChunkMaterial;
            collider.sharedMesh = newChunkFilter.mesh;
        }
    }

    public void UpdateChunk(Vector2Int ChunkCoord)
    {
        if (ActiveChunks.ContainsKey(ChunkCoord))
        {
            Vector3Int DataCoords = new Vector3Int(ChunkCoord.x, 0, ChunkCoord.y);

            GameObject TargetChunk = ActiveChunks[ChunkCoord];
            MeshFilter targetFilter = TargetChunk.GetComponent<MeshFilter>();
            MeshCollider targetCollider = TargetChunk.GetComponent<MeshCollider>();

            StartCoroutine(meshCreator.CreateMeshFromData(WorldData[DataCoords], x =>
            {
                targetFilter.mesh = x;
                targetCollider.sharedMesh = x;
            }));
        }
    }

    public void SetBlock(Vector3Int WorldPosition, int BlockType = 0)
    {
        Vector2Int coords = GetChunkCoordsFromPosition(WorldPosition);
        Vector3Int DataPosition = new Vector3Int(coords.x, 0, coords.y);

        if (WorldData.ContainsKey(DataPosition))
        {
            Vector3Int coordsToChange = WorldToLocalCoords(WorldPosition, coords);
            WorldData[DataPosition][coordsToChange.x, coordsToChange.y, coordsToChange.z] = BlockType;
            UpdateChunk(coords);
        }
    }

    private Vector2Int GetChunkCoordsFromPosition(Vector3 WorldPosition)
    {
        return new Vector2Int(
            Mathf.FloorToInt(WorldPosition.x / ChunkSize.x),
            Mathf.FloorToInt(WorldPosition.z / ChunkSize.z)
        );
    }

    private Vector3Int WorldToLocalCoords(Vector3Int WorldPosition, Vector2Int Coords)
    {
        return new Vector3Int
        {
            x = WorldPosition.x - Coords.x * ChunkSize.x,
            y = WorldPosition.y,
            z = WorldPosition.z - Coords.y * ChunkSize.z
        };
    }
}