using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Spawns test spheres randomly in AR world space to test depth scaling and occlusion.
/// Attach to XR Origin or AR Session GameObject.
/// </summary>
public class ARDepthTestSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int sphereCount = 20;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;

    [Header("Sphere Settings")]
    [SerializeField] private float sphereSize = 0.1f;
    [SerializeField] private Material sphereMaterial;
    [SerializeField] private bool randomColors = true;

    [Header("Auto Spawn")]
    [SerializeField] private bool spawnOnStart = false;
    [SerializeField] private KeyCode spawnKey = KeyCode.Space;

    private ARSession arSession;
    private Camera arCamera;
    private GameObject sphereParent;

    void Start()
    {
        arSession = FindObjectOfType<ARSession>();
        arCamera = Camera.main;

        if (arCamera == null)
        {
            Debug.LogError("ARDepthTestSpawner: No main camera found!");
            return;
        }

        sphereParent = new GameObject("DepthTestSpheres");

        if (spawnOnStart)
        {
            SpawnSpheres();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnSpheres();
        }
    }

    [ContextMenu("Spawn Spheres")]
    public void SpawnSpheres()
    {
        ClearSpheres();

        for (int i = 0; i < sphereCount; i++)
        {
            Vector3 randomPosition = GenerateRandomPosition();
            CreateSphere(randomPosition, i);
        }

        Debug.Log($"Spawned {sphereCount} test spheres in AR world");
    }

    [ContextMenu("Clear Spheres")]
    public void ClearSpheres()
    {
        if (sphereParent != null)
        {
            foreach (Transform child in sphereParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private Vector3 GenerateRandomPosition()
    {
        // Generate position relative to camera if no spawn center specified
        Vector3 center = spawnAreaCenter == Vector3.zero ? arCamera.transform.position : spawnAreaCenter;

        // Random spherical distribution
        float distance = Random.Range(minDistance, maxDistance);
        float theta = Random.Range(0f, Mathf.PI * 2f); // azimuth
        float phi = Random.Range(-Mathf.PI / 4f, Mathf.PI / 4f); // elevation (limited to natural field of view)

        Vector3 direction = new Vector3(
            Mathf.Sin(phi) * Mathf.Cos(theta),
            Mathf.Sin(phi) * Mathf.Sin(theta),
            Mathf.Cos(phi)
        );

        // If using camera-relative, transform to camera's forward direction
        if (spawnAreaCenter == Vector3.zero)
        {
            direction = arCamera.transform.TransformDirection(direction);
        }

        return center + direction * distance;
    }

    private void CreateSphere(Vector3 position, int index)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = $"DepthTestSphere_{index}";
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * sphereSize;
        sphere.transform.SetParent(sphereParent.transform);

        // Apply material
        Renderer renderer = sphere.GetComponent<Renderer>();
        if (sphereMaterial != null)
        {
            renderer.material = sphereMaterial;
        }

        // Random colors for easy identification
        if (randomColors)
        {
            Color randomColor = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.8f, 1f);
            renderer.material.color = randomColor;
        }

        // Add distance label (optional debugging)
        float distance = Vector3.Distance(arCamera.transform.position, position);
        sphere.name += $" ({distance:F2}m)";
    }

    void OnDrawGizmosSelected()
    {
        if (arCamera == null) arCamera = Camera.main;
        if (arCamera == null) return;

        Vector3 center = spawnAreaCenter == Vector3.zero ? arCamera.transform.position : spawnAreaCenter;

        // Draw spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, minDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, maxDistance);

        // Draw spawn center
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, Vector3.one * 0.2f);
    }
}
