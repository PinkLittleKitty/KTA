using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Rendering
{
    public class VertexData
    {
        private static readonly ConcurrentQueue<VertexData> _pool = new ConcurrentQueue<VertexData>();

        public List<Vector3> Vertices;
        public List<Vector3> Normals;
        public List<int> Indices;

        public VertexData(int capacity = 4096)
        {
            Vertices = new List<Vector3>(capacity);
            Normals = new List<Vector3>(capacity);
            Indices = new List<int>(capacity);
        }

        public void Clear()
        {
            Vertices.Clear();
            Normals.Clear();
            Indices.Clear();
        }

        public static VertexData Get()
        {
            if (_pool.TryDequeue(out VertexData data))
            {
                data.Clear();
                return data;
            }
            return new VertexData(4096);
        }

        public static void Release(VertexData data)
        {
            if (data != null)
            {
                data.Clear();
                _pool.Enqueue(data);
            }
        }
    }
}