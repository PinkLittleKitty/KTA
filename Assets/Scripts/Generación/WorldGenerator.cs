using System;
using UnityEngine;

namespace Assets.Generation
{
    public class WorldGenerator : IDisposable
    {
        public const float SpawnRadius = 64f;
        public static Vector3 SpawnPosition = Vector3.forward * 32f;

        private float[] _sampleBuffer;

        public void Generate(float[] densities, Vector3 offsets, int chunkSize)
        {
            float scale = 0.025f, amplitude = 64f;
            int lerp = 4;
            int pointsPerAxis = chunkSize + 1;
            int sliceSize = pointsPerAxis * pointsPerAxis;

            int sampleCount = (chunkSize / lerp) + 1;
            if (_sampleBuffer == null || _sampleBuffer.Length < sampleCount)
            {
                _sampleBuffer = new float[sampleCount];
            }

            float spawnRadiusSqr = SpawnRadius * SpawnRadius;

            for (int x = 0; x <= chunkSize; x++)
            {
                float worldX = x + offsets.x;
                float dx = SpawnPosition.x - worldX;
                float dxSqr = dx * dx;

                for (int y = 0; y <= chunkSize; y++)
                {
                    float worldY = y + offsets.y;
                    float dy = SpawnPosition.y - worldY;
                    float dxySqr = dxSqr + dy * dy;

                    for (int i = 0; i < sampleCount; i++)
                    {
                        float worldZ = (i * lerp) + offsets.z;
                        _sampleBuffer[i] = (float)OpenSimplexNoise.Evaluate(worldX * scale, worldY * scale, worldZ * scale) * amplitude;
                    }

                    int xSlicePlusYRow = x * sliceSize + y * pointsPerAxis;

                    for (int z = 0; z <= chunkSize; z++)
                    {
                        int sampleIdx = z / lerp;
                        int remainder = z % lerp;

                        float density;
                        if (remainder == 0)
                        {
                            density = _sampleBuffer[sampleIdx];
                        }
                        else
                        {
                            float prev = _sampleBuffer[sampleIdx];
                            float next = _sampleBuffer[sampleIdx + 1];
                            density = Mathf.Lerp(prev, next, (float)remainder / lerp);
                        }

                        float worldZ = z + offsets.z;
                        float dz = SpawnPosition.z - worldZ;
                        if ((dxySqr + dz * dz) < spawnRadiusSqr)
                        {
                            density = 0;
                        }

                        densities[xSlicePlusYRow + z] = density;
                    }
                }
            }
        }

        public void Dispose()
        {
        }
    }
}