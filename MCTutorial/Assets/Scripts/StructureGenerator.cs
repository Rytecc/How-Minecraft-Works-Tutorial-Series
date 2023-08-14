using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureGenerator : MonoBehaviour
{
    [System.Serializable]
    public class StructureBlockInfo {
        public Vector3Int offsetFromOPoint;
        public int typeToAssign;
    };

    [SerializeField] private StructureBlockInfo[] StructureInfo;

    [Range(0f, 1f)]
    [SerializeField] private float genThreshold;
    private System.Random randomGen;

    private void Awake() {
        randomGen = new System.Random(1337);
    }

    private void applyStructure(ref int[,,] dataToModify, Vector2Int originCoords, int x, int y, int z) {
        for (int i = 0; i < StructureInfo.Length; i++) {
            StructureBlockInfo info = StructureInfo[i];
            Vector3Int p = new Vector3Int {
                x = x + info.offsetFromOPoint.x,
                y = y + info.offsetFromOPoint.y,
                z = z + info.offsetFromOPoint.z
            };

            try {
                dataToModify[p.x, p.y, p.z] = info.typeToAssign;
            }
            catch (System.IndexOutOfRangeException) {
                int worldX = p.x + (originCoords.x * 16);
                int worldY = p.y;
                int worldZ = p.z + (originCoords.y * 16);
                Vector3Int pos = new Vector3Int(worldX, worldY, worldZ);

                Vector2Int newCoords = WorldGenerator.GetChunkCoordsFromPosition(pos);
                Vector3Int chunkCoords = WorldGenerator.WorldToLocalCoords(pos, newCoords);

                if (WorldGenerator.AdditiveWorldData.ContainsKey(newCoords)) {
                    WorldGenerator.AdditiveWorldData[newCoords][chunkCoords.x, chunkCoords.y, chunkCoords.z] = info.typeToAssign;
                }
                else {
                    int[,,] emptyData = new int[WorldGenerator.ChunkSize.x, WorldGenerator.ChunkSize.y, WorldGenerator.ChunkSize.z];
                    emptyData[chunkCoords.x, chunkCoords.y, chunkCoords.z] = info.typeToAssign;
                    WorldGenerator.AdditiveWorldData.Add(newCoords, emptyData);
                }
            }
        }
    }

    private int getTopBlockFromDataXZ(int[,,] data, int x, int z) {
        for (int y = WorldGenerator.ChunkSize.y - 1; y >= 0; y--) {
            if (data[x, y, z] != 0) {
                return y;
            }
        }

        return -1;
    }

    public void GenerateStructure(Vector2Int chunkCoords, ref int[,,] dataToModify, int x, int z) {
        float randomValue = (float)randomGen.NextDouble();
        if (randomValue >= genThreshold) {
            applyStructure(ref dataToModify, chunkCoords, x, getTopBlockFromDataXZ(dataToModify, x, z), z);
        }
    }
}