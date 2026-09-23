using System;
using System.Collections;
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
        private MeshFilter _filter;
        private MeshRenderer _renderer;
        private MeshCollider _collider;
        private World _world;
        private Coroutine _fadeCoroutine;

        private static readonly int _wColorId = Shader.PropertyToID("_WColor");
        private static readonly int _colorId = Shader.PropertyToID("_Color");
        private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int _emissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int _wThicknessId = Shader.PropertyToID("_WThickness");
        private static readonly int _wEmissionId = Shader.PropertyToID("_WEmission");

        private readonly float[] _blocks = new float[PointsPerAxis * PointsPerAxis * PointsPerAxis];
        private readonly WorldGenerator _generator = new WorldGenerator();
        private readonly Vector3[] _vertCache = new Vector3[12];

        void Awake()
        {
            _mesh = new Mesh();
            _filter = gameObject.AddComponent<MeshFilter>();
            _filter.mesh = _mesh;
            _renderer = gameObject.AddComponent<MeshRenderer>();
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _collider = gameObject.AddComponent<MeshCollider>();
        }

        public void Init(Vector3 Position, World World)
        {
            _world = World;
            this.Position = Position;
            Lod = 1;
            if (_renderer != null && _world != null && _world.WorldMaterial != null)
            {
                _renderer.sharedMaterial = _world.WorldMaterial;
            }
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
                    bool hasGeometry = BlockData.Vertices != null && BlockData.Vertices.Count > 0;

                    if (_mesh != null)
                    {
                        _mesh.Clear();
                        if (hasGeometry)
                        {
                            _mesh.SetVertices(BlockData.Vertices);
                            _mesh.SetNormals(BlockData.Normals);
                            _mesh.SetIndices(BlockData.Indices, MeshTopology.Triangles, 0, false);
                        }
                    }
                    
                    if (hasGeometry)
                    {
                        StartFadeIn();
                    }

                    if (_collider != null)
                    {
                        if (hasGeometry && _mesh != null && _mesh.vertexCount > 0)
                        {
                            int meshId = _mesh.GetInstanceID();
                            ThreadPool.QueueUserWorkItem(_ =>
                            {
                                Physics.BakeMesh(meshId, false);
                                ThreadManager.ExecuteOnMainThread(() =>
                                {
                                    if (_collider != null && _mesh != null && !Disposed)
                                    {
                                        _collider.sharedMesh = _mesh;
                                    }
                                });
                            });
                        }
                        else
                        {
                            _collider.sharedMesh = null;
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

        private void StartFadeIn()
        {
            if (_renderer == null)
                return;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeInRoutine());
        }

        private IEnumerator FadeInRoutine()
        {
            if (_renderer == null)
                yield break;

            float duration = (_world != null && _world.ChunkFadeInDuration > 0.05f)
                ? _world.ChunkFadeInDuration
                : 0.45f;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            Material mat = (_world != null && _world.WorldMaterial != null) ? _world.WorldMaterial : null;

            block.SetColor(_wColorId, Color.black);
            block.SetColor(_colorId, Color.black);
            block.SetColor(_baseColorId, Color.black);
            block.SetColor(_emissionColorId, Color.black);
            block.SetFloat(_wThicknessId, 0f);
            block.SetFloat(_wEmissionId, 0f);
            _renderer.SetPropertyBlock(block);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (Disposed || this == null || _renderer == null)
                    yield break;

                elapsed += UnityEngine.Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeT = 1.0f - (1.0f - t) * (1.0f - t);

                Color targetColor = (mat != null && mat.HasProperty(_wColorId)) ? mat.GetColor(_wColorId) : Color.white;
                float targetThickness = (mat != null && mat.HasProperty(_wThicknessId)) ? mat.GetFloat(_wThicknessId) : 0.06f;
                float targetEmission = (mat != null && mat.HasProperty(_wEmissionId)) ? mat.GetFloat(_wEmissionId) : 2.0f;

                Color curColor = Color.Lerp(Color.black, targetColor, easeT);
                float curThickness = Mathf.Lerp(0f, targetThickness, easeT);
                float curEmission = Mathf.Lerp(0f, targetEmission, easeT);

                _renderer.GetPropertyBlock(block);
                block.SetColor(_wColorId, curColor);
                block.SetColor(_colorId, curColor);
                block.SetColor(_baseColorId, curColor);
                block.SetColor(_emissionColorId, curColor * (curEmission / Mathf.Max(targetEmission, 0.01f)));
                block.SetFloat(_wThicknessId, curThickness);
                block.SetFloat(_wEmissionId, curEmission);
                _renderer.SetPropertyBlock(block);

                yield return null;
            }

            if (!Disposed && this != null && _renderer != null)
            {
                _renderer.SetPropertyBlock(null);
            }
            _fadeCoroutine = null;
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
                    if (_fadeCoroutine != null)
                    {
                        StopCoroutine(_fadeCoroutine);
                        _fadeCoroutine = null;
                    }

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
