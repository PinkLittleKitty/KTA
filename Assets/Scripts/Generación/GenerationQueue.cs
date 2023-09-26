using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Linq;

namespace Assets.Generation
{
    // La clase GenerationQueue se encarga de gestionar una cola de generación de fragmentos (chunks).
    public class GenerationQueue
    {
        public World _world;                // Referencia al mundo al que pertenecen los fragmentos.
        public List<Chunk> Queue = new List<Chunk>();  // Cola de fragmentos a generar.
        public bool Stop { get; set; }       // Indica si se debe detener la generación.
        private ClosestChunk _closestChunkComparer = new ClosestChunk();  // Comparador de fragmentos más cercanos.
        private int _exceptionCount = 0;     // Contador de excepciones para controlar errores.

        // Constructor de la clase GenerationQueue.
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

        // Método para ordenar la cola de generación de fragmentos según su proximidad al jugador.
        public void Sort()
        {
            _closestChunkComparer.PlayerPos = _world.PlayerPosition + _world.PlayerOrientation * Chunk.ChunkSize * 4f;
            Queue.Sort(_closestChunkComparer);
        }

        // Método para agregar un fragmento a la cola de generación.
        public void Add(Chunk c)
        {
            lock (Queue)
                Queue.Add(c);
        }

        // Método para eliminar un fragmento de la cola de generación.
        public void Remove(Chunk c)
        {
            lock (Queue)
                Queue.Remove(c);
        }

        // Método principal de generación de fragmentos.
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
                        workingChunk = Queue.FirstOrDefault();
                        Queue.Remove(workingChunk);
                    }

                    if (workingChunk != null)
                        workingChunk.Generate();
                }
            }
            catch (Exception e)
            {
                if (_exceptionCount >= 3)
                    return;
                new Thread(Start).Start();
                Debug.Log(e.ToString());
            }
        }
    }
}