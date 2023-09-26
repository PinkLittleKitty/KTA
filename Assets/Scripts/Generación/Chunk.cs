using System;
using UnityEngine;
using Assets.Rendering;

namespace Assets.Generation
{
    public class Chunk : MonoBehaviour, IDisposable
    {
        public const int ChunkSize = 32;
        public const int Bitshift = 5;
        public Vector3 Position { get; private set; }
        public bool ShouldBuild;
        public bool IsGenerated;
        public int Lod;
        public bool Disposed;

        private Mesh _mesh;
        private World _world;
        private readonly float[][][] _blocks = new float[ChunkSize][][];
        private readonly WorldGenerator _generator = new WorldGenerator();

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
            // Generación de los datos del chunk
            _generator.BuildArray(_blocks, ChunkSize);
            _generator.Generate(_blocks, Position, ChunkSize);
            IsGenerated = true;
            ShouldBuild = true;
        }

        public void Build()
        {
            // Construcción del mesh del chunk
            ShouldBuild = false;
            bool completelyBuilt = true;

            int WIDTH = _blocks.Length;
            int HEIGHT = _blocks[0].Length;
            int DEPTH = _blocks[0][0].Length;

            bool Next = false;
            GridCell Cell = new GridCell();
            Cell.P = new Vector3[8];
            Cell.Density = new double[8];

            VertexData BlockData = new VertexData();
            Chunk RightChunk = _world.GetChunkByOffset(Position + new Vector3(ChunkSize, 0, 0));
            Chunk TopChunk = _world.GetChunkByOffset(Position + new Vector3(0, ChunkSize, 0));
            Chunk FrontChunk = _world.GetChunkByOffset(Position + new Vector3(0, 0, ChunkSize));

            Chunk RightFrontChunk = _world.GetChunkByOffset(Position + new Vector3(ChunkSize, 0, ChunkSize));
            Chunk TopFrontChunk = _world.GetChunkByOffset(Position + new Vector3(0, ChunkSize, ChunkSize));
            Chunk TopRightFrontChunk = _world.GetChunkByOffset(Position + new Vector3(ChunkSize, ChunkSize, ChunkSize));
            Chunk TopRightChunk = _world.GetChunkByOffset(Position + new Vector3(ChunkSize, ChunkSize, 0));

            for (int y = 0; y < ChunkSize; y += Lod)
            {
                for (int x = 0; x < ChunkSize; x += Lod)
                {
                    Next = !Next;
                    for (int z = 0; z < ChunkSize; z += Lod)
                    {
                        Next = !Next;

                        bool Success;
                        CreateCell(x, y, z, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk,
                            TopFrontChunk, TopRightFrontChunk, TopChunk, Cell, out Success);

                        if (!Success)
                            completelyBuilt = false;

                        if (!MarchingCubes.Usable(0f, Cell))
                            continue;

                        MarchingCubes.Process(0f, Cell, Next, BlockData);
                    }
                }
            }

            ThreadManager.ExecuteOnMainThread(delegate
            {
                if (!Disposed)
                {
                    _mesh.Clear();
                    _mesh.SetVertices(BlockData.Vertices);
                    _mesh.SetNormals(BlockData.Normals);
                    _mesh.SetIndices(BlockData.Indices.ToArray(), MeshTopology.Triangles, 0);
                    GetComponent<MeshCollider>().sharedMesh = _mesh;
                }
            });
        }

        private void CreateCell(float x, float y, float z, Chunk RightChunk, Chunk FrontChunk, Chunk RightFrontChunk, Chunk TopRightChunk, Chunk TopFrontChunk, Chunk TopRightFrontChunk, Chunk TopChunk, GridCell Cell, out bool Success)
        {
            // Creación de una celda y obtención de densidades vecinas
            Success = true;
            BuildCell(x, y, z, Cell);
            int _x = (int)x, _y = (int)y, _z = (int)z;

            Cell.Density[0] = GetNeighbourDensity(_x, _y, _z, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
            Cell.Density[1] = GetNeighbourDensity(_x + Lod, _y, _z, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
            Cell.Density[2] = GetNeighbourDensity(_x + Lod, _y, _z + Lod, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
            Cell.Density[3] = GetNeighbourDensity(_x, _y, _z + Lod, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
            Cell.Density[4] = GetNeighbourDensity(_x, _y + Lod, _z, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
            Cell.Density[5] = GetNeighbourDensity(_x + Lod, _y + Lod, _z, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
            Cell.Density[6] = GetNeighbourDensity(_x + Lod, _y + Lod, _z + Lod, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);
            Cell.Density[7] = GetNeighbourDensity(_x, _y + Lod, _z + Lod, RightChunk, FrontChunk, RightFrontChunk, TopRightChunk, TopFrontChunk, TopRightFrontChunk, TopChunk);

            for (int i = 0; i < Cell.Density.Length; i++)
            {
                if (Cell.Density[i] == 0f)
                {
                    Success = false;
                    break;
                }
            }
        }

        private void BuildCell(float x, float y, float z, GridCell Cell)
        {
            // Construcción de una celda
            Cell.P[0] = new Vector3(x, y, z);
            Cell.P[1] = new Vector3(x + Lod, y, z);
            Cell.P[2] = new Vector3(x + Lod, y, z + Lod);
            Cell.P[3] = new Vector3(x, y, z + Lod);
            Cell.P[4] = new Vector3(x, y + Lod, z);
            Cell.P[5] = new Vector3(x + Lod, y + Lod, z);
            Cell.P[6] = new Vector3(x + Lod, y + Lod, z + Lod);
            Cell.P[7] = new Vector3(x, y + Lod, z + Lod);
        }

        private float GetNeighbourDensity(int x, int y, int z, Chunk RightChunk, Chunk FrontChunk, Chunk RightFrontChunk, Chunk TopRightChunk, Chunk TopFrontChunk, Chunk TopRightFrontChunk, Chunk TopChunk)
        {
            // Obtención de densidad de vecinos
            int WIDTH = ChunkSize;
            int HEIGHT = ChunkSize;
            int DEPTH = ChunkSize;

            bool bX = x >= ChunkSize;
            bool bY = y >= ChunkSize;
            bool bZ = z >= ChunkSize;

            if (bX && !bY && bZ && RightFrontChunk != null && !RightFrontChunk.Disposed && RightFrontChunk.IsGenerated)
                return RightFrontChunk.GetBlockAt(x - WIDTH, y, z - DEPTH);

            if (bZ && !bY && !bX && FrontChunk != null && !FrontChunk.Disposed && FrontChunk.IsGenerated)
                return FrontChunk.GetBlockAt(x, y, z - DEPTH);

            if (bX && !bY && !bZ && RightChunk != null && !RightChunk.Disposed && RightChunk.IsGenerated)
                return RightChunk.GetBlockAt(x - WIDTH, y, z);

            if (bX && bY && bZ && TopRightFrontChunk != null && !TopRightFrontChunk.Disposed && TopRightFrontChunk.IsGenerated)
                return TopRightFrontChunk.GetBlockAt(x - WIDTH, y - HEIGHT, z - DEPTH);

            if (!bX && bY && bZ && TopFrontChunk != null && !TopFrontChunk.Disposed && TopFrontChunk.IsGenerated)
                return TopFrontChunk.GetBlockAt(x, y - HEIGHT, z - DEPTH);

            if (bX && bY && !bZ && TopRightChunk != null && !TopRightChunk.Disposed && TopRightChunk.IsGenerated)
                return TopRightChunk.GetBlockAt(x - WIDTH, y - HEIGHT, z);

            if (!bX && bY && !bZ && TopChunk != null && !TopChunk.Disposed && TopChunk.IsGenerated)
                return TopChunk.GetBlockAt(x, y - HEIGHT, z);

            if (!bX && !bY && !bZ)
                return _blocks[x][y][z];

            return 0;
        }

        public bool NeighboursExists
        {
            get
            {
                // Verifica si existen vecinos generados
                int conditions = 0;

                Chunk RightChunk = _world.GetChunkByOffset(Position + new Vector3(ChunkSize, 0, 0));
                conditions += (RightChunk != null && RightChunk.IsGenerated) ? 1 : 0;

                Chunk TopChunk = _world.GetChunkByOffset(Position + new Vector3(0, ChunkSize, 0));
                conditions += (TopChunk != null && TopChunk.IsGenerated) ? 1 : 0;

                Chunk FrontChunk = _world.GetChunkByOffset(Position + new Vector3(0, 0, ChunkSize));
                conditions += (FrontChunk != null && FrontChunk.IsGenerated) ? 1 : 0;

                Chunk RightFrontChunk = _world.GetChunkByOffset(Position + new Vector3(ChunkSize, 0, ChunkSize));
                conditions += (RightFrontChunk != null && RightFrontChunk.IsGenerated) ? 1 : 0;

                Chunk TopFrontChunk = _world.GetChunkByOffset(Position + new Vector3(0, ChunkSize, ChunkSize));
                conditions += (TopFrontChunk != null && TopFrontChunk.IsGenerated) ? 1 : 0;

                Chunk TopRightFrontChunk = _world.GetChunkByOffset(Position + new Vector3(ChunkSize, ChunkSize, ChunkSize));
                conditions += (TopRightFrontChunk != null && TopRightFrontChunk.IsGenerated) ? 1 : 0;

                Chunk TopRightChunk = _world.GetChunkByOffset(Position + new Vector3(ChunkSize, ChunkSize, 0));
                conditions += (TopRightChunk != null && TopRightChunk.IsGenerated) ? 1 : 0;

                return conditions == 7;
            }
        }

        public float GetBlockAt(Vector3 v)
        {
            return GetBlockAt((int)v.x, (int)v.y, (int)v.z);
        }

        public float GetBlockAt(int x, int y, int z)
        {
            if (IsGenerated)
                return _blocks[x][y][z];
            else
                return 0;
        }

        public void Dispose()
        {
            // Limpia y destruye el chunk
            ThreadManager.ExecuteOnMainThread(delegate
            {
                _mesh.Clear();
                Destroy(_mesh);
            });
            Disposed = true;
            _generator.Dispose();
            ThreadManager.ExecuteOnMainThread(() => Destroy(gameObject));
        }
    }
}
