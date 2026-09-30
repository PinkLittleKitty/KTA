using System.Collections.Generic;
using UnityEngine;
using Assets.Generation;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralAsteroid : MonoBehaviour
{
    [Header("Runtime State")]
    public float Radius = 3f;
    public Vector3 AngularVelocity;
    public Vector3 DriftVelocity;

    public float SpawnAnimDuration = 0.4f;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;
    private Rigidbody _rigidbody;
    private Coroutine _spawnAnimCoroutine;
    private Color _baseWireColor = Color.white;
    private float _baseEmission = 1.6f;
    private static readonly int _wColorId = Shader.PropertyToID("_WColor");
    private static readonly int _wEmissionId = Shader.PropertyToID("_WEmission");
    private MaterialPropertyBlock _propBlock;

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        _meshCollider = GetComponent<MeshCollider>();
        if (_meshCollider == null)
            _meshCollider = gameObject.AddComponent<MeshCollider>();
        _meshCollider.convex = true;

        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
    }

    public void Setup(Mesh mesh, Material material, float collisionRadius, Color? wireColor = null, float emission = -1f)
    {
        Radius = collisionRadius;
        _meshFilter.sharedMesh = mesh;
        _meshCollider.sharedMesh = mesh;

        if (material != null)
            _meshRenderer.sharedMaterial = material;

        if (wireColor.HasValue)
            _baseWireColor = wireColor.Value;
        if (emission >= 0f)
            _baseEmission = emission;

        ApplyProperties(_baseWireColor, _baseEmission);
    }

    private void ApplyProperties(Color color, float emission)
    {
        if (_propBlock == null)
            _propBlock = new MaterialPropertyBlock();

        _meshRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(_wColorId, color);
        _propBlock.SetFloat(_wEmissionId, emission);
        _meshRenderer.SetPropertyBlock(_propBlock);
    }

    public void Activate(Vector3 position, Quaternion rotation, Vector3 angVel, Vector3 drift)
    {
        transform.position = position;
        transform.rotation = rotation;
        AngularVelocity = angVel;
        DriftVelocity = drift;

        if (_spawnAnimCoroutine != null)
            StopCoroutine(_spawnAnimCoroutine);

        gameObject.SetActive(true);
        _spawnAnimCoroutine = StartCoroutine(AnimateSpawn());
    }

    public void Deactivate()
    {
        if (_spawnAnimCoroutine != null)
        {
            StopCoroutine(_spawnAnimCoroutine);
            _spawnAnimCoroutine = null;
        }
        transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator AnimateSpawn()
    {
        transform.localScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < SpawnAnimDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / SpawnAnimDuration);

            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float t = progress - 1f;
            float scale = 1f + c3 * t * t * t + c1 * t * t;
            transform.localScale = Vector3.one * Mathf.Max(0f, scale);

            float flash = 1f - progress;
            Color curColor = Color.Lerp(_baseWireColor, Color.white, flash * 0.75f);
            float curEmission = _baseEmission * (1f + flash * 2.0f);
            ApplyProperties(curColor, curEmission);

            yield return null;
        }

        transform.localScale = Vector3.one;
        ApplyProperties(_baseWireColor, _baseEmission);
        _spawnAnimCoroutine = null;
    }

    void Update()
    {
        if (AngularVelocity != Vector3.zero)
            transform.Rotate(AngularVelocity * Time.deltaTime, Space.Self);

        if (DriftVelocity != Vector3.zero)
            transform.position += DriftVelocity * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        ShipCollision ship = collision.gameObject.GetComponent<ShipCollision>()
                          ?? collision.gameObject.GetComponentInParent<ShipCollision>();
        if (ship != null)
        {
            ship.SendMessage("DestroyShip", SendMessageOptions.DontRequireReceiver);
        }
    }
}

public static class AsteroidMeshBuilder
{
    public static Mesh CreateAsteroidMesh(float baseRadius, float roughness, int seed, int subdivision = 0)
    {
        float t = (1.0f + Mathf.Sqrt(5.0f)) * 0.5f;

        List<Vector3> vertices = new List<Vector3>
        {
            new Vector3(-1,  t,  0).normalized,
            new Vector3( 1,  t,  0).normalized,
            new Vector3(-1, -t,  0).normalized,
            new Vector3( 1, -t,  0).normalized,

            new Vector3( 0, -1,  t).normalized,
            new Vector3( 0,  1,  t).normalized,
            new Vector3( 0, -1, -t).normalized,
            new Vector3( 0,  1, -t).normalized,

            new Vector3( t,  0, -1).normalized,
            new Vector3( t,  0,  1).normalized,
            new Vector3(-t,  0, -1).normalized,
            new Vector3(-t,  0,  1).normalized
        };

        List<int> faces = new List<int>
        {
            0, 11, 5,
            0, 5, 1,
            0, 1, 7,
            0, 7, 10,
            0, 10, 11,

            1, 5, 9,
            5, 11, 4,
            11, 10, 2,
            10, 7, 6,
            7, 1, 8,

            3, 9, 4,
            3, 4, 2,
            3, 2, 6,
            3, 6, 8,
            3, 8, 9,

            4, 9, 5,
            2, 4, 11,
            6, 2, 10,
            8, 6, 7,
            9, 8, 1
        };

        for (int s = 0; s < subdivision; s++)
        {
            Dictionary<long, int> midPointCache = new Dictionary<long, int>();
            List<int> newFaces = new List<int>();

            int GetMidPoint(int p1, int p2)
            {
                long smaller = Mathf.Min(p1, p2);
                long greater = Mathf.Max(p1, p2);
                long key = (smaller << 32) + greater;

                if (midPointCache.TryGetValue(key, out int index))
                    return index;

                Vector3 v1 = vertices[p1];
                Vector3 v2 = vertices[p2];
                Vector3 mid = ((v1 + v2) * 0.5f).normalized;
                int newIndex = vertices.Count;
                vertices.Add(mid);
                midPointCache.Add(key, newIndex);
                return newIndex;
            }

            for (int i = 0; i < faces.Count; i += 3)
            {
                int v1 = faces[i];
                int v2 = faces[i + 1];
                int v3 = faces[i + 2];

                int a = GetMidPoint(v1, v2);
                int b = GetMidPoint(v2, v3);
                int c = GetMidPoint(v3, v1);

                newFaces.Add(v1); newFaces.Add(a); newFaces.Add(c);
                newFaces.Add(v2); newFaces.Add(b); newFaces.Add(a);
                newFaces.Add(v3); newFaces.Add(c); newFaces.Add(b);
                newFaces.Add(a);  newFaces.Add(b); newFaces.Add(c);
            }

            faces = newFaces;
        }

        float seedOffset = (seed % 1000) * 2.371f;
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 dir = vertices[i];
            float n1 = (float)OpenSimplexNoise.Evaluate(dir.x * 1.5f + seedOffset, dir.y * 1.5f + seedOffset, dir.z * 1.5f + seedOffset);
            float n2 = (float)OpenSimplexNoise.Evaluate(dir.x * 3.0f + seedOffset * 1.5f, dir.y * 3.0f + seedOffset * 1.5f, dir.z * 3.0f + seedOffset * 1.5f) * 0.5f;
            float combinedNoise = (n1 + n2) / 1.5f;

            float r = baseRadius * (1.0f + combinedNoise * roughness);
            vertices[i] = dir * r;
        }

        Mesh mesh = new Mesh();
        mesh.name = $"ProceduralAsteroid_Seed{seed}_Sub{subdivision}";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(faces, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
