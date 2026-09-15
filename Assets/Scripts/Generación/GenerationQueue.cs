using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

namespace Assets.Generation
{
    public class GenerationQueue
    {
        public World _world;
        public List<Chunk> Queue = new List<Chunk>();
        private readonly HashSet<Chunk> _queueSet = new HashSet<Chunk>();
        private readonly AutoResetEvent _workSignal = new AutoResetEvent(false);
        private readonly Thread[] _workers;
        private volatile bool _stop;

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

        private ClosestChunk _closestChunkComparer = new ClosestChunk();

        public GenerationQueue(World World)
        {
            this._world = World;

            int workerCount = Mathf.Clamp(SystemInfo.processorCount / 2, 2, 4);
            _workers = new Thread[workerCount];

            for (int i = 0; i < workerCount; i++)
            {
                _workers[i] = new Thread(Start)
                {
                    IsBackground = true,
                    Name = $"GenerationQueueWorker_{i}"
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
                        return;

                    _closestChunkComparer.PlayerPos = _world.PlayerPosition + _world.PlayerOrientation * (Chunk.ChunkSize * 4f);
                    
                    Queue.RemoveAll(chunk => chunk == null || chunk.Disposed);
                    _queueSet.RemoveWhere(chunk => chunk == null || chunk.Disposed);
                    
                    Queue.Sort(_closestChunkComparer);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error al ordenar la cola de generación: " + e.Message);
                    Debug.LogError("Stack trace: " + e.StackTrace);
                    
                    Queue.Clear();
                    _queueSet.Clear();
                }
            }
        }

        public void Add(Chunk c)
        {
            if (c == null || c.Disposed)
                return;

            lock (Queue)
            {
                if (_queueSet.Add(c))
                {
                    Queue.Add(c);
                }
            }
            _workSignal.Set();
        }

        public bool Contains(Chunk c)
        {
            if (c == null) return false;
            lock (Queue) return _queueSet.Contains(c);
        }

        public void Remove(Chunk c)
        {
            if (c == null)
                return;

            lock (Queue)
            {
                Queue.Remove(c);
                _queueSet.Remove(c);
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
                        _world.GenQueue = Queue.Count;

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

                    if (workingChunk != null && !workingChunk.Disposed)
                    {
                        workingChunk.Generate();
                    }
                    else
                    {
                        _workSignal.WaitOne(10);
                    }
                }
                catch (Exception chunkException)
                {
                    if (workingChunk != null)
                    {
                        Debug.LogError($"Error generando chunk en posición {workingChunk.Position}: {chunkException.Message}\n{chunkException.StackTrace}");
                        if (!workingChunk.Disposed)
                        {
                            ThreadManager.ExecuteOnMainThread(() => _world.RemoveChunk(workingChunk));
                        }
                    }
                    else
                    {
                        Debug.LogError($"Error en GenerationQueue: {chunkException.Message}\n{chunkException.StackTrace}");
                    }
                }
            }
        }
    }
}