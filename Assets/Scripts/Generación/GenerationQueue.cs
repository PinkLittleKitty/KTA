using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Linq;

namespace Assets.Generation
{
    public class GenerationQueue
    {
        public World _world;
        public List<Chunk> Queue = new List<Chunk>();
        public bool Stop { get; set; }
        private ClosestChunk _closestChunkComparer = new ClosestChunk();
        private int _exceptionCount = 0;

        public GenerationQueue(World World)
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
            this._world = World;
        }

        public void Sort()
        {
            lock (Queue)
            {
                try
                {
                    if (Queue.Count <= 1)
                        return;

                    _closestChunkComparer.PlayerPos = _world.PlayerPosition + _world.PlayerOrientation * Chunk.ChunkSize * 4f;
                    
                    Queue.RemoveAll(chunk => chunk == null);
                    
                    Queue.Sort(_closestChunkComparer);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error al ordenar la cola de generación: " + e.Message);
                    Debug.LogError("Stack trace: " + e.StackTrace);
                    
                    Queue.Clear();
                }
            }
        }

        public void Add(Chunk c)
        {
            lock (Queue)
                Queue.Add(c);
        }

        public void Remove(Chunk c)
        {
            lock (Queue)
                Queue.Remove(c);
        }

        public void Start()
        {
            try
            {
                while (true)
                {
                    if (Stop)
                        break;

                    _world.MeshQueue = Queue.Count;

                    Chunk workingChunk = null;
                    lock (Queue)
                    {
                        if (Queue.Count > 0)
                        {
                            workingChunk = Queue.FirstOrDefault();
                            if (workingChunk != null)
                                Queue.Remove(workingChunk);
                        }
                    }

                    if (workingChunk != null)
                    {
                        try
                        {
                            workingChunk.Generate();
                        }
                        catch (Exception chunkException)
                        {
                            Debug.LogError($"Error generando chunk en posición {workingChunk.Position}: {chunkException.Message}");
                            if (workingChunk != null)
                            {
                                ThreadManager.ExecuteOnMainThread(() => _world.RemoveChunk(workingChunk));
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
            }
            catch (Exception e)
            {
                _exceptionCount++;
                Debug.LogError($"Error crítico en GenerationQueue (intento {_exceptionCount}): {e.Message}");
                Debug.LogError("Stack trace: " + e.StackTrace);
                
                Thread.Sleep(100);
                new Thread(Start).Start();
            }
        }
    }
}