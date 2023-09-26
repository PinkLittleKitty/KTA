using System;
using UnityEngine;

namespace Assets.Generation
{
    public class WorldGenerator : IDisposable
    {
        public const float SpawnRadius = 64f;
        public static Vector3 SpawnPosition = Vector3.forward * 32f;

        public void BuildArray(float[][][] densities, int chunkSize)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                densities[x] = new float[chunkSize][];

                for (int y = 0; y < chunkSize; y++)
                {
                    densities[x][y] = new float[chunkSize];
                }
            }
        }

        public void Generate(float[][][] densities, Vector3 offsets, int chunkSize)
        {
            float scale = 0.025f, amplitude = 64f;
            int lerp = 4;

            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    float[] values = new float[chunkSize / lerp];
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = (float)OpenSimplexNoise.Evaluate((x + offsets.x) * scale, (y + offsets.y) * scale, (i * lerp + offsets.z) * scale) * amplitude;
                    }

                    for (int z = 0; z < chunkSize; z++)
                    {
                        float prev = values[(int)(z / lerp)];
                        float next = values[(int)Mathf.Min(z / lerp + 1, values.Length - 1)];
                        densities[x][y][z] = Mathf.Lerp(prev, next, (float)(z / (float)lerp));

                        // Make a sphere on spawn point
                        densities[x][y][z] = ((SpawnPosition - new Vector3(x + offsets.x, y + offsets.y, z + offsets.z)).sqrMagnitude < SpawnRadius * SpawnRadius) ? 0 : densities[x][y][z];
                    }
                }
            }
        }

        public void Dispose()
        {
            
        }
    }
}