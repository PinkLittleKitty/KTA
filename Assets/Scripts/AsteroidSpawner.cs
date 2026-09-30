using System.Collections.Generic;
using UnityEngine;
using Assets.Generation;

public class AsteroidSpawner : MonoBehaviour
{
    public static AsteroidSpawner Instance { get; private set; }

    public bool SpawnerEnabled = true;

    [Header("Scaling")]
    public int InitialActiveAsteroids = 20;
    public int MaxActiveAsteroids = 150;
    public float TimeToMaxAsteroids = 120f;

    [Header("Spawn")]
    public float BaseSpawnInterval = 0.25f;
    public float MinSpawnInterval = 0.05f;
    public float MinSpawnAheadDistance = 50f;
    public float MaxSpawnAheadDistance = 260f;
    public float DespawnBehindDistance = 35f;

    [Header("Spread")]
    public float MinSpreadRadius = 12f;
    public float MaxHorizontalSpread = 70f;
    public float MaxVerticalSpread = 50f;


    [SerializeField] private int _currentActiveLimit = 20;
    [SerializeField] private float _flightTime = 0f;


    public Material AsteroidMaterial;
    public bool UseCustomWireColor = true;
    [ColorUsage(true, true)]
    public Color WireColor = new Color(1.0f, 0.45f, 0.05f, 1.0f);
    public float WireEmission = 1.6f;

    public float MinTumbleSpeed = 12f;
    public float MaxTumbleSpeed = 40f;
    public float MaxDriftSpeed = 1.2f;

    private World _world;
    private GameObject _player;
    private Movement _playerMovement;
    private Transform _poolContainer;

    private readonly List<ProceduralAsteroid> _pool = new List<ProceduralAsteroid>();
    private readonly List<Mesh> _meshVariants = new List<Mesh>();
    private readonly List<float> _variantRadii = new List<float>();

    private float _spawnTimer = 0f;
    private const int MeshVariantCount = 8;
    private const float WorldNoiseScale = 0.025f;
    private const float WorldNoiseAmplitude = 64f;

    void Awake()
    {
        Instance = this;
        _world = GetComponent<World>() ?? FindFirstObjectByType<World>() ?? FindObjectOfType<World>();

        ValidateSettings();
        GenerateMeshVariants();
        InitializePool();
    }

    void OnValidate()
    {
        ValidateSettings();
    }

    private void ValidateSettings()
    {
        if (MaxHorizontalSpread <= 0f) MaxHorizontalSpread = 70f;
        if (MaxVerticalSpread <= 0f) MaxVerticalSpread = 50f;
        if (MinSpreadRadius <= 0f) MinSpreadRadius = 12f;
        if (InitialActiveAsteroids <= 0) InitialActiveAsteroids = 20;
        if (MaxActiveAsteroids <= 0) MaxActiveAsteroids = 150;
        if (MinSpawnAheadDistance <= 0f) MinSpawnAheadDistance = 50f;
        if (MaxSpawnAheadDistance <= 0f) MaxSpawnAheadDistance = 260f;
        if (TimeToMaxAsteroids <= 0f) TimeToMaxAsteroids = 120f;
        if (BaseSpawnInterval <= 0f) BaseSpawnInterval = 0.25f;
        if (MinSpawnInterval <= 0f) MinSpawnInterval = 0.05f;
    }

    void Start()
    {
        if (AsteroidMaterial == null && _world != null)
        {
            AsteroidMaterial = _world.WorldMaterial;
        }
    }

    private void GenerateMeshVariants()
    {
        _meshVariants.Clear();
        _variantRadii.Clear();

        float[] baseRadii = { 2.0f, 2.5f, 3.2f, 3.8f, 4.5f, 5.2f, 2.8f, 3.5f };
        float[] roughness = { 0.35f, 0.45f, 0.40f, 0.50f, 0.38f, 0.48f, 0.42f, 0.52f };
        int[] subdivisions = { 0, 1, 0, 1, 1, 1, 0, 1 };

        for (int i = 0; i < MeshVariantCount; i++)
        {
            int seed = 100 + i * 37;
            Mesh mesh = AsteroidMeshBuilder.CreateAsteroidMesh(baseRadii[i], roughness[i], seed, subdivisions[i]);
            _meshVariants.Add(mesh);
            _variantRadii.Add(baseRadii[i]);
        }
    }

    private void InitializePool()
    {
        GameObject containerObj = new GameObject("AsteroidPool");
        containerObj.transform.SetParent(transform, false);
        _poolContainer = containerObj.transform;

        int totalToPool = Mathf.Max(MaxActiveAsteroids, 150) + 15;
        for (int i = 0; i < totalToPool; i++)
        {
            GameObject obj = new GameObject($"Asteroid_{i}");
            obj.transform.SetParent(_poolContainer, false);

            ProceduralAsteroid asteroid = obj.AddComponent<ProceduralAsteroid>();

            int variantIdx = i % MeshVariantCount;
            Mesh mesh = _meshVariants[variantIdx];
            float radius = _variantRadii[variantIdx];

            Color? color = UseCustomWireColor ? (Color?)WireColor : null;
            asteroid.Setup(mesh, AsteroidMaterial, radius, color, WireEmission);
            asteroid.Deactivate();

            _pool.Add(asteroid);
        }
    }

    void Update()
    {
        if (_world != null && AsteroidMaterial == null)
        {
            AsteroidMaterial = _world.WorldMaterial;
        }

        if (_player == null || !_player.activeInHierarchy)
        {
            if (_world != null && _world.Player != null)
                _player = _world.Player;
            else
                _player = GameObject.FindGameObjectWithTag("Player");

            if (_player != null)
                _playerMovement = _player.GetComponentInChildren<Movement>();
        }

        if (_player == null || !SpawnerEnabled)
            return;

        if (_playerMovement != null && _playerMovement.IsInSpawn)
            return;     

        _flightTime += Time.deltaTime;
        float progress = Mathf.Clamp01(_flightTime / Mathf.Max(1f, TimeToMaxAsteroids));
        _currentActiveLimit = Mathf.RoundToInt(Mathf.Lerp(InitialActiveAsteroids, MaxActiveAsteroids, progress));

        Vector3 playerPos = _playerMovement != null ? _playerMovement.transform.position : _player.transform.position;
        Vector3 playerFwd = _playerMovement != null ? _playerMovement.transform.forward : _player.transform.forward;

        DespawnPassedAsteroids(playerPos, playerFwd);

        float currentInterval = Mathf.Lerp(BaseSpawnInterval, MinSpawnInterval, progress);
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= currentInterval)
        {
            _spawnTimer = 0f;
            int activeCount = GetActiveCount();
            int needed = _currentActiveLimit - activeCount;
            int batch = Mathf.Clamp(needed, 0, 3);
            for (int b = 0; b < batch; b++)
            {
                TrySpawnAsteroidAhead(playerPos, playerFwd);
            }
        }
    }

    private void DespawnPassedAsteroids(Vector3 playerPos, Vector3 playerFwd)
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            ProceduralAsteroid asteroid = _pool[i];
            if (!asteroid.gameObject.activeSelf)
                continue;

            Vector3 toAsteroid = asteroid.transform.position - playerPos;
            float fwdDist = Vector3.Dot(toAsteroid, playerFwd);

            if (fwdDist < -DespawnBehindDistance || toAsteroid.sqrMagnitude > (MaxSpawnAheadDistance + 70f) * (MaxSpawnAheadDistance + 70f))
            {
                asteroid.Deactivate();
            }
        }
    }

    private void TrySpawnAsteroidAhead(Vector3 playerPos, Vector3 playerFwd)
    {
        ProceduralAsteroid asteroid = GetAvailableAsteroid();
        if (asteroid == null)
            return;

        for (int attempt = 0; attempt < 8; attempt++)
        {
            float distAhead = Random.Range(MinSpawnAheadDistance, MaxSpawnAheadDistance);
            Vector3 centerPoint = playerPos + playerFwd * distAhead;

            Vector3 right = Vector3.Cross(playerFwd, Vector3.up);
            if (right.sqrMagnitude < 0.01f)
                right = Vector3.right;
            else
                right.Normalize();
            Vector3 up = Vector3.Cross(right, playerFwd).normalized;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radiusNormalized = Mathf.Sqrt(Random.Range(0.15f, 1f));
            float xOffset = Mathf.Cos(angle) * radiusNormalized * MaxHorizontalSpread;
            float yOffset = Mathf.Sin(angle) * radiusNormalized * MaxVerticalSpread;

            Vector2 offset2D = new Vector2(xOffset, yOffset);
            if (offset2D.sqrMagnitude < MinSpreadRadius * MinSpreadRadius)
            {
                offset2D = (offset2D == Vector2.zero ? Random.insideUnitCircle.normalized : offset2D.normalized) * MinSpreadRadius;
                xOffset = offset2D.x;
                yOffset = offset2D.y;
            }

            Vector3 offset = right * xOffset + up * yOffset;
            Vector3 candidatePos = centerPoint + offset;

            float distToSpawnSqr = (candidatePos - WorldGenerator.SpawnPosition).sqrMagnitude;
            float minSpawnRadius = WorldGenerator.SpawnRadius + 15f;
            if (distToSpawnSqr < minSpawnRadius * minSpawnRadius)
                continue;

            float density = (float)OpenSimplexNoise.Evaluate(
                candidatePos.x * WorldNoiseScale,
                candidatePos.y * WorldNoiseScale,
                candidatePos.z * WorldNoiseScale
            ) * WorldNoiseAmplitude;

            if (density > -3.5f)
                continue;

            if (Physics.CheckSphere(candidatePos, asteroid.Radius + 1.8f))
                continue;

            Quaternion randomRot = Random.rotation;
            Vector3 randomTumble = Random.insideUnitSphere * Random.Range(MinTumbleSpeed, MaxTumbleSpeed);
            Vector3 randomDrift = Random.insideUnitSphere * Random.Range(0f, MaxDriftSpeed);

            asteroid.Activate(candidatePos, randomRot, randomTumble, randomDrift);
            return;
        }
    }

    private ProceduralAsteroid GetAvailableAsteroid()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].gameObject.activeSelf)
                return _pool[i];
        }
        return null;
    }

    private int GetActiveCount()
    {
        int count = 0;
        for (int i = 0; i < _pool.Count; i++)
        {
            if (_pool[i].gameObject.activeSelf)
                count++;
        }
        return count;
    }

    public void ResetAll()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            _pool[i].Deactivate();
        }
        _spawnTimer = 0f;
        _flightTime = 0f;
        _currentActiveLimit = InitialActiveAsteroids;
    }
}
