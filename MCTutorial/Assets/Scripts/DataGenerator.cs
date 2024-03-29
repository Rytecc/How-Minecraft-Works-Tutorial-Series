﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

public class DataGenerator
{
    public struct BiomeData {
        public int topBlock;
        public int topMiddleBlock;
        public int bottomMiddleBlock;
        public int bottomBlock;
    }

    public class GenData {
        public System.Action<int[,,]> OnComplete;
        public Vector3Int GenerationPoint;
    }

    private WorldGenerator GeneratorInstance;
    private Queue<GenData> DataToGenerate;
    public bool Terminate;

    private StructureGenerator structureGen;
    public DataGenerator(WorldGenerator worldGen, StructureGenerator structureGen = null) {
        GeneratorInstance = worldGen;
        DataToGenerate = new Queue<GenData>();
        this.structureGen = structureGen;

        worldGen.StartCoroutine(DataGenLoop());
    }

    public void QueueDataToGenerate(GenData data) {
        DataToGenerate.Enqueue(data);
    }

    public IEnumerator DataGenLoop()
    {
        while(Terminate == false)
        {
            if(DataToGenerate.Count > 0)
            {
                GenData gen = DataToGenerate.Dequeue();
                yield return GeneratorInstance.StartCoroutine(GenerateData(gen.GenerationPoint, gen.OnComplete));
            }

            yield return null;
        }
    }

    public IEnumerator GenerateData(Vector3Int offset, System.Action<int[,,]> callback)
    {
        Vector3Int ChunkSize = WorldGenerator.ChunkSize;
        Vector2 NoiseOffset = GeneratorInstance.NoiseOffset;
        Vector2 NoiseScale = GeneratorInstance.NoiseScale;

        float HeightIntensity = GeneratorInstance.HeightIntensity;
        float HeightOffset = GeneratorInstance.HeightOffset;

        int[,,] TempData = new int[ChunkSize.x, ChunkSize.y, ChunkSize.z];
        if (WorldGenerator.AdditiveWorldData.TryGetValue(new Vector2Int(offset.x, offset.z), out int[,,] addedData)) { // new
            TempData = addedData;
            WorldGenerator.AdditiveWorldData.Remove(new Vector2Int(offset.x, offset.z));
        }

        Task t = Task.Factory.StartNew(delegate
        {
            for (int x = 0; x < ChunkSize.x; x++)
            {
                for (int z = 0; z < ChunkSize.z; z++)
                {
                    float PerlinCoordX = NoiseOffset.x + (x + (offset.x * 16f)) / ChunkSize.x * NoiseScale.x;
                    float PerlinCoordY = NoiseOffset.y + (z + (offset.z * 16f)) / ChunkSize.z * NoiseScale.y;
                    int HeightGen = Mathf.RoundToInt(Mathf.PerlinNoise(PerlinCoordX, PerlinCoordY) * HeightIntensity + HeightOffset);

                    float biomeCoordX = NoiseOffset.x + (x + (offset.x * 16f)) / ChunkSize.x * 0.75f;
                    float biomeCoordY = NoiseOffset.y + (z + (offset.z * 16f)) / ChunkSize.z * 0.75f;
                    float biomeNoise = Mathf.PerlinNoise(biomeCoordX, biomeCoordY);
                    BiomeData data;

                    if (biomeNoise < 0.5f) {
                        data = new BiomeData {
                            topBlock = 4,
                            topMiddleBlock = 2,
                            bottomMiddleBlock = 3,
                            bottomBlock = 4
                        };
                    }
                    else {
                        data = new BiomeData {
                            topBlock = 3,
                            topMiddleBlock = 2,
                            bottomMiddleBlock = 3,
                            bottomBlock = 1,
                        };
                    }

                    for (int y = HeightGen; y >= 0; y--) {
                        int BlockTypeToAssign = 0;

                        // Set first layer to grass
                        if (y == HeightGen) BlockTypeToAssign = data.topBlock;

                        //Set next 3 layers to dirt
                        if (y < HeightGen && y > HeightGen - 4) BlockTypeToAssign = data.topMiddleBlock;

                        //Set everything between the dirt range (inclusive) and 0 (exclusive) to stone
                        if (y <= HeightGen - 4 && y > 0) BlockTypeToAssign = data.bottomMiddleBlock;

                        //Set everything at height 0 to bedrock.
                        if (y == 0) BlockTypeToAssign = data.bottomBlock;

                        if (TempData[x, y, z] == 0) {
                            TempData[x, y, z] = BlockTypeToAssign;
                        }
                    }

                    if (structureGen != null && data.topBlock == 4) {
                        structureGen.GenerateStructure(new Vector2Int(offset.x, offset.z), ref TempData, x, z);
                    }
                }
            }
        });

        yield return new WaitUntil(() => {
            return t.IsCompleted || t.IsCanceled;
        });

        if (t.Exception != null)
            Debug.LogError(t.Exception);

        WorldGenerator.WorldData.Add(offset, TempData);
        callback(TempData);
    }
}
