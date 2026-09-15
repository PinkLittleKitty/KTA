using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Generation
{
    public class MeshQueue
    {
        public GameObject _player;
        public List<Chunk> Queue = new List<Chunk>();
        private readonly HashSet<Chunk> _queueSet = new HashSet<Chunk>();
        private readonly AutoResetEvent _workSignal = new AutoResetEvent(false);
        private readonly Thread[] _workers;
        private volatile bool _stop;
        private volatile bool _needsSort = false;

        public bool Stop
        {
            get => _stop;
            set
            {
                _stop = value;
                if (value)
                {
                    _workSignal.Set();
                }
            }
        }

        private World _world;
        private ClosestChunk _closestChunkComparer = new ClosestChunk();

        public MeshQueue(World World)
        {
            this._player = World.Player;
            this._world = World;

            int workerCount = Mathf.Clamp(SystemInfo.processorCount / 2, 2, 4);
            _workers = new Thread[workerCount];

            for (int i = 0; i < workerCount; i++)
            {
                _workers[i] = new Thread(Start)
                {
                    IsBackground = true,
                    Name = $"MeshQueueWorker_{i}"
                };
                _workers[i].Start();
            }
        }

        public void Sort()
        {
            lock (Queue)
            {
                try
                {
                    if (Queue.Count <= 1)
                    {
                        _needsSort = false;
                        return;
                    }

                    _closestChunkComparer.PlayerPos = _world.PlayerPosition + _world.PlayerOrientation * (Chunk.ChunkSize * 4f);
                    
                    Queue.RemoveAll(chunk => chunk == null || chunk.Disposed);
                    _queueSet.RemoveWhere(chunk => chunk == null || chunk.Disposed);
                    
                    Queue.Sort(_closestChunkComparer);
                    _needsSort = false;
                }
                catch (Exception e)
                {
                    Debug.LogError("Error al ordenar la cola de mesh: " + e.Message);
                    Debug.LogError("Stack trace: " + e.StackTrace);
                    
                    Queue.Clear();
                    _queueSet.Clear();
                    _needsSort = false;
                }
            }
        }

        public bool Contains(Chunk ChunkToCheck)
        {
            if (ChunkToCheck == null) return false;
            lock (Queue) return _queueSet.Contains(ChunkToCheck);
        }

        public void Add(Chunk ChunkToBuild)
        {
            if (ChunkToBuild == null || ChunkToBuild.Disposed)
                return;

            lock (Queue)
            {
                if (_queueSet.Add(ChunkToBuild))
                {
                    Queue.Add(ChunkToBuild);
                    _needsSort = true;
                }
            }
            _workSignal.Set();
        }

        public void Remove(Chunk ChunkToBuild)
        {
            if (ChunkToBuild == null)
                return;

            lock (Queue)
            {
                Queue.Remove(ChunkToBuild);
                _queueSet.Remove(ChunkToBuild);
            }
        }

        public void Start()
        {
            while (!_stop)
            {
                Chunk workingChunk = null;
                try
                {
                    lock (Queue)
                    {
                        _world.MeshQueue = Queue.Count;

                        if (Queue.Count > 0)
                        {
                            if (_needsSort)
                            {
                                Sort();
                            }

                            if (Queue.Count > 0)
                            {
                                workingChunk = Queue[0];
                                Queue.RemoveAt(0);
                                if (workingChunk != null)
                                    _queueSet.Remove(workingChunk);

                                if (Queue.Count > 0)
                                    _workSignal.Set();
                            }
                        }
                    }

                    if (workingChunk != null && !workingChunk.Disposed)
                    {
                        workingChunk.Build();
                    }
                    else
                    {
                        _workSignal.WaitOne(10);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Error en MeshQueue: " + e.Message);
                    Debug.LogError(e.StackTrace);
                    
                    if (workingChunk != null && !workingChunk.Disposed)
                    {
                        ThreadManager.ExecuteOnMainThread(() => _world.RemoveChunk(workingChunk));
                    }
                }
            }
        }
    }
}