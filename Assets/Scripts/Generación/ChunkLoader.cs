using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Generation
{
    public class ChunkLoader : MonoBehaviour
    {
        public Vector3 Offset;
        public bool Enabled = true;
        public GameObject Player;
        public World World;
        private Vector3 _lastOffset = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        private float _lastRadius;
        private Vector3 _playerPosition, _position;
        private bool Stop;
        private readonly List<Vector3> _missingPositions = new List<Vector3>();
        private readonly List<Chunk> _toRemove = new List<Chunk>();

        void Awake()
        {
            StartCoroutine(this.LoadChunks());
            StartCoroutine(this.ManageChunksMesh());
        }

        void OnApplicationQuit()
        {
            Stop = true;
        }

        void Update()
        {
            if (Player != null)
            {
                _playerPosition = Player.transform.position;
            }
            _position = transform.position;
        }

        private IEnumerator LoadChunks()
        {
            while (true)
            {
                if (Stop) break;

                if (!Enabled || !World.Loaded)
                    goto SLEEP;

                Offset = World.ToChunkSpace(_playerPosition);

                if (Offset != _lastOffset || World.ChunkLoaderRadius != _lastRadius)
                {
                    _lastRadius = World.ChunkLoaderRadius;
                    _lastOffset = Offset;

                    float radiusInChunks = World.ChunkLoaderRadius * 0.5f;
                    float loadRadius = radiusInChunks * Chunk.ChunkSize;
                    float loadRadiusSqr = loadRadius * loadRadius;
                    int chunkRadius = Mathf.CeilToInt(radiusInChunks);

                    _missingPositions.Clear();

                    for (int _x = -chunkRadius; _x <= chunkRadius; _x++)
                    {
                        for (int _z = -chunkRadius; _z <= chunkRadius; _z++)
                        {
                            for (int _y = -chunkRadius; _y <= chunkRadius; _y++)
                            {
                                Vector3 chunkPos = Offset + new Vector3(_x * Chunk.ChunkSize, _y * Chunk.ChunkSize, _z * Chunk.ChunkSize);

                                Vector3 chunkCenter = chunkPos + new Vector3(Chunk.ChunkSize * 0.5f, Chunk.ChunkSize * 0.5f, Chunk.ChunkSize * 0.5f);
                                if ((chunkCenter - _playerPosition).sqrMagnitude > loadRadiusSqr)
                                    continue;

                                if (World.GetChunkByOffset(chunkPos) == null)
                                {
                                    _missingPositions.Add(chunkPos);
                                }
                            }
                        }
                    }

                    if (_missingPositions.Count > 0)
                    {
                        Vector3 forward = Player != null ? Player.transform.forward : Vector3.forward;
                        Vector3 focusPoint = _playerPosition + forward * (Chunk.ChunkSize * 3f);

                        _missingPositions.Sort((a, b) =>
                        {
                            float distA = (a - focusPoint).sqrMagnitude;
                            float distB = (b - focusPoint).sqrMagnitude;
                            return distA.CompareTo(distB);
                        });

                        int createdThisFrame = 0;
                        for (int i = 0; i < _missingPositions.Count; i++)
                        {
                            Vector3 chunkPos = _missingPositions[i];
                            if (World.GetChunkByOffset(chunkPos) == null)
                            {
                                GameObject newChunk = new GameObject("Chunk " + (chunkPos.x) + " " + (chunkPos.y) + " " + (chunkPos.z));
                                newChunk.transform.position = chunkPos;
                                newChunk.transform.SetParent(World.gameObject.transform);
                                Chunk chunk = newChunk.AddComponent<Chunk>();
                                chunk.Init(chunkPos, World);
                                chunk.Lod = 2;
                                World.AddChunk(chunkPos, chunk);

                                createdThisFrame++;
                                if (createdThisFrame >= 10)
                                {
                                    createdThisFrame = 0;
                                    yield return null;
                                    if (Stop) yield break;
                                }
                            }
                        }
                    }
                }
            SLEEP:
                yield return null;
            }
        }

        private IEnumerator ManageChunksMesh()
        {
            var wait = new WaitForSeconds(0.5f);

            while (true)
            {
                if (Stop) break;

                yield return wait;

                float radiusInChunks = World.ChunkLoaderRadius * 0.5f;
                float loadRadius = radiusInChunks * Chunk.ChunkSize;
                float unloadRadius = loadRadius + (Chunk.ChunkSize * 1.5f);
                float unloadDistSqr = unloadRadius * unloadRadius;

                _toRemove.Clear();
                lock (World.Chunks)
                {
                    foreach (var chunk in World.Chunks.Values)
                    {
                        if (chunk == null || chunk.Disposed)
                            continue;

                        Vector3 chunkCenter = chunk.Position + new Vector3(Chunk.ChunkSize * 0.5f, Chunk.ChunkSize * 0.5f, Chunk.ChunkSize * 0.5f);
                        if ((chunkCenter - _playerPosition).sqrMagnitude > unloadDistSqr)
                        {
                            _toRemove.Add(chunk);
                        }
                    }
                }

                for (int i = 0; i < _toRemove.Count; i++)
                {
                    World.RemoveChunk(_toRemove[i]);
                    if (i % 8 == 7)
                    {
                        yield return null;
                        if (Stop) yield break;
                    }
                }
            }
        }
    }
}
