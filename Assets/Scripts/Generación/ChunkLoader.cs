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
        private Thread _t1, _t2;
        private bool Stop;

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
            _playerPosition = Player.transform.position;
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
                    for (int _x = -World.ChunkLoaderRadius / 2; _x < World.ChunkLoaderRadius / 2; _x++)
                    {
                        for (int _z = -World.ChunkLoaderRadius / 2; _z < World.ChunkLoaderRadius / 2; _z++)
                        {
                            for (int _y = -World.ChunkLoaderRadius / 2; _y < World.ChunkLoaderRadius / 2; _y++)
                            {
                                int x = _x, y = _y, z = _z;

                                if (World.GetChunkByOffset(Offset + Vector3.Scale(new Vector3(x, y, z), new Vector3(Chunk.ChunkSize, Chunk.ChunkSize, Chunk.ChunkSize))) == null)
                                {
                                    Vector3 chunkPos = Offset + Vector3.Scale(new Vector3(x, y, z), new Vector3(Chunk.ChunkSize, Chunk.ChunkSize, Chunk.ChunkSize));
                                    GameObject NewChunk = new GameObject("Chunk " + (chunkPos.x) + " " + (chunkPos.y) + " " + (chunkPos.z));
                                    NewChunk.transform.position = chunkPos;
                                    NewChunk.transform.SetParent(World.gameObject.transform);
                                    Chunk chunk = NewChunk.AddComponent<Chunk>();
                                    chunk.Init(chunkPos, World);
                                    chunk.Lod = 2;
                                    World.AddChunk(chunkPos, chunk);
                                }
                            }
                        }
                    }
                    _lastRadius = World.ChunkLoaderRadius;
                    _lastOffset = Offset;
                    World.SortGenerationQueue();
                }
            SLEEP:
                yield return null;
            }
        }

        private IEnumerator ManageChunksMesh()
        {
            var wait = new WaitForSeconds(0.25f);
            var toRemove = new List<Chunk>();

            while (true)
            {
                if (Stop) break;

                yield return wait;

                float maxRadius = World.ChunkLoaderRadius * 0.5f * Chunk.ChunkSize;
                float maxDistSqr = maxRadius * maxRadius;

                toRemove.Clear();
                lock (World.Chunks)
                {
                    foreach (var chunk in World.Chunks.Values)
                    {
                        if (chunk == null || chunk.Disposed)
                            continue;

                        if ((chunk.Position - _playerPosition).sqrMagnitude > maxDistSqr)
                        {
                            toRemove.Add(chunk);
                        }
                    }
                }

                for (int i = 0; i < toRemove.Count; i++)
                {
                    World.RemoveChunk(toRemove[i]);
                }
            }
        }
    }
}
