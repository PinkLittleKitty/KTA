using System;
using System.Threading;
using UnityEngine;
using Assets.Rendering;

namespace Assets.Generation
{
    public class Chunk : MonoBehaviour, IDisposable
    {
        public const int ChunkSize = 32;
        public const int PointsPerAxis = ChunkSize + 1;
        public const int Bitshift = 5;
        public Vector3 Position { get; private set; }
        public bool ShouldBuild;
        public bool IsGenerated;
        public int Lod = 1;
        public bool Disposed;

        private Mesh _mesh;
        private World _world;
        private readonly float[] _blocks = new float[PointsPerAxis * PointsPerAxis * PointsPerAxis];
        private readonly WorldGenerator _generator = new WorldGenerator();
        private readonly Vector3[] _vertCache = new Vector3[12];

        void Start()
        {
            // Inicialización de los componentes del chunk
            _mesh = new Mesh();
            gameObject.AddComponent<MeshCollider>();
            MeshFilter Filter = gameObject.AddComponent<MeshFilter>();
            Filter.mesh = _mesh;
            MeshRenderer Renderer = gameObject.AddComponent<MeshRenderer>();
            Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Renderer.receiveShadows = false;
            Renderer.material = _world.WorldMaterial;
        }

        public void Init(Vector3 Position, World World)
        {
            // Inicialización del chunk
            _world = World;
            this.Position = Position;
            Lod = 1;
        }

        public void Generate()
        {
            if (Disposed)
                return;

            // Generación de los datos del chunk
            _generator.Generate(_blocks, Position, ChunkSize);

            if (Disposed)
                return;

            IsGenerated = true;
            ShouldBuild = true;
            
            _world.AddToQueue(this, true);
        }

        public void Build()
        {
            if (Disposed)
                return;

            // Construcción del mesh del chunk
            ShouldBuild = false;

            GridCell Cell = new GridCell();
            Cell.P = new Vector3[8];
            Cell.Density = new double[8];

            VertexData BlockData = VertexData.Get();
            int pointsPerAxis = PointsPerAxis;
            int sliceSize = pointsPerAxis * pointsPerAxis;

            for (int y = 0; y < ChunkSize; y += Lod)
            {
                for (int x = 0; x < ChunkSize; x += Lod)
                {
                    for (int z = 0; z < ChunkSize; z += Lod)
                    {
                        if (!BuildCellDensities(x, y, z, Cell, sliceSize, pointsPerAxis))
                            continue;

                        BuildCellPositions(x, y, z, Cell);
                        MarchingCubes.Process(0f, Cell, BlockData, _vertCache);
                    }
                }
            }

            ThreadManager.ExecuteOnMainThread(delegate
            {
                if (!Disposed && this != null)
                {
                    if (_mesh != null)
                    {
                        _mesh.Clear();
                        _mesh.SetVertices(BlockData.Vertices);
                        _mesh.SetNormals(BlockData.Normals);
                        _mesh.SetIndices(BlockData.Indices, MeshTopology.Triangles, 0, false);
                    }
                    
                    MeshCollider collider = GetComponent<MeshCollider>();
                    if (collider != null)
                    {
                        if (_mesh != null && _mesh.vertexCount > 0)
                        {
                            int meshId = _mesh.GetInstanceID();
                            ThreadPool.QueueUserWorkItem(_ =>
                            {
                                Physics.BakeMesh(meshId, false);
                                ThreadManager.ExecuteOnMainThread(() =>
                                {
                                    if (collider != null && _mesh != null && !Disposed)
                                    {
                                        collider.sharedMesh = _mesh;
                                    }
                                });
                            });
                        }
                        else
                        {
                            collider.sharedMesh = null;
                        }
                    }
                }

                VertexData.Release(BlockData);
            });
        }

        private bool BuildCellDensities(int x, int y, int z, GridCell Cell, int sliceSize, int pointsPerAxis)
        {
            int lod = Lod;
            int x1 = x + lod;
            int y1 = y + lod;
            int z1 = z + lod;

            int x0Slice = x * sliceSize;
            int x1Slice = x1 * sliceSize;
            int y0Row = y * pointsPerAxis;
            int y1Row = y1 * pointsPerAxis;

            Cell.Density[0] = _blocks[x0Slice + y0Row + z];
            Cell.Density[1] = _blocks[x1Slice + y0Row + z];
            Cell.Density[2] = _blocks[x1Slice + y0Row + z1];
            Cell.Density[3] = _blocks[x0Slice + y0Row + z1];
            Cell.Density[4] = _blocks[x0Slice + y1Row + z];
            Cell.Density[5] = _blocks[x1Slice + y1Row + z];
            Cell.Density[6] = _blocks[x1Slice + y1Row + z1];
            Cell.Density[7] = _blocks[x0Slice + y1Row + z1];

            return MarchingCubes.Usable(0f, Cell);
        }

        private void BuildCellPositions(int x, int y, int z, GridCell Cell)
        {
            int lod = Lod;
            int x1 = x + lod;
            int y1 = y + lod;
            int z1 = z + lod;

            Cell.P[0] = new Vector3(x, y, z);
            Cell.P[1] = new Vector3(x1, y, z);
            Cell.P[2] = new Vector3(x1, y, z1);
            Cell.P[3] = new Vector3(x, y, z1);
            Cell.P[4] = new Vector3(x, y1, z);
            Cell.P[5] = new Vector3(x1, y1, z);
            Cell.P[6] = new Vector3(x1, y1, z1);
            Cell.P[7] = new Vector3(x, y1, z1);
        }

        public bool NeighboursExists
        {
            get { return true; }
        }

        public bool HasMinimumNeighbours()
        {
            return true;
        }

        public float GetBlockAt(Vector3 v)
        {
            return GetBlockAt((int)v.x, (int)v.y, (int)v.z);
        }

        public float GetBlockAt(int x, int y, int z)
        {
            if (IsGenerated && x >= 0 && x < PointsPerAxis && y >= 0 && y < PointsPerAxis && z >= 0 && z < PointsPerAxis)
                return _blocks[x * (PointsPerAxis * PointsPerAxis) + y * PointsPerAxis + z];
            else
                return 0;
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;

            ThreadManager.ExecuteOnMainThread(delegate
            {
                if (this != null)
                {
                    if (_mesh != null)
                    {
                        _mesh.Clear();
                        Destroy(_mesh);
                        _mesh = null;
                    }

                    if (gameObject != null)
                    {
                        Destroy(gameObject);
                    }
                }
            });
        }
    }
}
