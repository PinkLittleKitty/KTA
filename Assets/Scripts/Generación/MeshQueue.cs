using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation
{
    public class MeshQueue
    {
        public GameObject _player;
        public List<Chunk> Queue = new List<Chunk>();
        public Dictionary<Chunk, int> _queueDict = new Dictionary<Chunk, int>();
        public bool Stop { get; set; }
        private World _world;
        private ClosestChunk _closestChunkComparer = new ClosestChunk();
        private int _exceptionCount = 0;

        public MeshQueue(World World)
        {
            bool useThreadPool = false;

            if (useThreadPool)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate (object state)
                { Start(); }));
            }
            else
            {
                new Thread(Start).Start();
            }
            this._player = World.Player;
            this._world = World;
        }

        public void Sort()
        {
            try
            {
                if (Queue.Count <= 1)
                    return;

                _closestChunkComparer.PlayerPos = _world.PlayerPosition + _world.PlayerOrientation * Chunk.ChunkSize * 4f;
                
                Queue.RemoveAll(chunk => ReferenceEquals(chunk, null));
                
                var keysToRemove = _queueDict.Keys.Where(chunk => ReferenceEquals(chunk, null)).ToList();
                foreach (var key in keysToRemove)
                {
                    _queueDict.Remove(key);
                }
                
                Queue.Sort(_closestChunkComparer);
            }
            catch (Exception e)
            {
                Debug.LogError("Error al ordenar la cola de mesh: " + e.Message);
                Debug.LogError("Stack trace: " + e.StackTrace);
                
                Queue.Clear();
                _queueDict.Clear();
            }
        }

        public bool Contains(Chunk ChunkToCheck)
        {
            lock (Queue) return _queueDict.ContainsKey(ChunkToCheck);
        }

        public void Add(Chunk ChunkToBuild)
        {
            if (ChunkToBuild == null)
                return;

            lock (Queue)
            {
                if (!_queueDict.ContainsKey(ChunkToBuild))
                {
                    _queueDict.Add(ChunkToBuild, 0);
                    Queue.Add(ChunkToBuild);
                }
            }
        }

        public void Remove(Chunk ChunkToBuild)
        {
            lock (Queue)
            {
                Queue.Remove(ChunkToBuild);
                if (_queueDict.ContainsKey(ChunkToBuild))
                    _queueDict.Remove(ChunkToBuild);
            }
        }

        public void Start()
        {
            while (true)
            {
                Chunk workingChunk = null;
                try
                {
                    if (Stop)
                        break;

                    Thread.Sleep(5);
                    _world.MeshQueue = Queue.Count;

                    lock (Queue)
                    {
                        Sort();
                        workingChunk = Queue.FirstOrDefault();
                        Queue.Remove(workingChunk);
                        if (workingChunk != null && _queueDict.ContainsKey(workingChunk))
                            _queueDict.Remove(workingChunk);
                    }

                    if (workingChunk != null)
                        workingChunk.Build();
                }
                catch (Exception e)
                {
                    Debug.LogError("Error en MeshQueue: " + e.Message);
                    Debug.LogError(e.StackTrace);
                    
                    if (workingChunk != null)
                    {
                        ThreadManager.ExecuteOnMainThread(() => _world.RemoveChunk(workingChunk));
                    }
                }
            }
        }
    }
}