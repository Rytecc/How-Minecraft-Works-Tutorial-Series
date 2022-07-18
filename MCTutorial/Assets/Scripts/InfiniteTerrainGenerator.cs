using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrainGenerator : MonoBehaviour
{

    [SerializeField] private Transform Player;
    [SerializeField] private int RenderDistance;
    private WorldGenerator GeneratorInstance;
    private List<Vector2Int> CoordsToRemove;

    // Start is called before the first frame update
    void Start()
    {
        GeneratorInstance = GetComponent<WorldGenerator>();
        CoordsToRemove = new List<Vector2Int>();
    }

    // Update is called once per frame
    void Update()
    {
        int plrChunkX = (int)Player.position.x / WorldGenerator.ChunkSize.x;
        int plrChunkY = (int)Player.position.z / WorldGenerator.ChunkSize.z;
        CoordsToRemove.Clear();

        foreach (KeyValuePair<Vector2Int, GameObject> activeChunk in WorldGenerator.ActiveChunks)
        {
            CoordsToRemove.Add(activeChunk.Key);
        }

        for (int x = plrChunkX - RenderDistance; x <= plrChunkX + RenderDistance; x++)
        {
            for (int y = plrChunkY - RenderDistance; y <= plrChunkY + RenderDistance; y++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);
                if (!WorldGenerator.ActiveChunks.ContainsKey(chunkCoord))
                {
                    // GeneratorInstance.EnqueueChunkToCreate(chunkCoord);
                    StartCoroutine(GeneratorInstance.CreateChunk(chunkCoord));
                }

                CoordsToRemove.Remove(chunkCoord);
            }
        }

        foreach (Vector2Int coord in CoordsToRemove)
        {
            GameObject chunkToDelete = WorldGenerator.ActiveChunks[coord];
            WorldGenerator.ActiveChunks.Remove(coord);
            Destroy(chunkToDelete);
        }
    }
}
