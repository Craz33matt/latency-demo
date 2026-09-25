using UnityEngine;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnableObject
    {
        public GameObject prefab;
        [Range (0f, 1f)]
        public float spawnChance;
        public bool randomizeY;
        public float minSpawnY;
        public float maxSpawnY;
        public float spawnYOffset;
    }

    public SpawnableObject[] objects;

    public float minSpawnRate = 1f;
    public float maxSpawnRate = 2f;
    public float spawnRateMultiplier = 1f; // higher = more spawns (intervals divided by this)
    public bool scaleWithGameSpeed = true; // if true, multiplier scales with GameManager.gameSpeed

    private void OnEnable()
    {
        Invoke(nameof(Spawn), GetSpawnDelay());
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Spawn()
    {
        float spawnChance = Random.value;

        foreach (var obj in objects)
        {
            if (spawnChance < obj.spawnChance)
            {
                GameObject obstacle = Instantiate(obj.prefab);
                float prefabYOffset = obj.prefab.transform.position.y;
                Vector3 spawnPosition = transform.position;

                if (obj.randomizeY)
                {
                    spawnPosition.y = Random.Range(obj.minSpawnY, obj.maxSpawnY);
                }

                spawnPosition.y += prefabYOffset + obj.spawnYOffset;
                obstacle.transform.position = spawnPosition;
                break;
            }

            spawnChance -= obj.spawnChance; //this allows for the random spawn chances
        }

        Invoke(nameof(Spawn), GetSpawnDelay());
    }

    private float GetSpawnDelay()
    {
        float baseDelay = Random.Range(minSpawnRate, maxSpawnRate);
        float effectiveMultiplier = Mathf.Max(0.0001f, spawnRateMultiplier);

        if (scaleWithGameSpeed && GameManager.Instance != null)
        {
            // Scale multiplier by relative game speed so higher gameSpeed increases spawn frequency
            float speedScale = GameManager.Instance.gameSpeed / Mathf.Max(0.0001f, GameManager.Instance.initialGameSpeed);
            effectiveMultiplier *= speedScale;
        }

        return baseDelay / effectiveMultiplier;
    }
}
