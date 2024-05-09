using System.Collections.Generic;
using UnityEngine;

public class Village : MonoBehaviour
{
    [SerializeField] GameObject VillagerPrefab;
    [SerializeField] List<Transform> SpawnPoints;
    [SerializeField] int InitialPopulation = 10;

    [SerializeField] float MinSpawnInterval = 1f;
    [SerializeField] float MaxSpawnInterval = 3f;

    float TimeTillNextSpawn = -1f;

    List<Transform> WorkingSpawnPoints;
    int NumSpawned = 0;

    // Start is called before the first frame update
    void Start()
    {
        TimeTillNextSpawn = Random.Range(MinSpawnInterval, MaxSpawnInterval);
    }

    private void Update()
    {
        if (NumSpawned < InitialPopulation) 
        {
            TimeTillNextSpawn -= Time.deltaTime;
            if (TimeTillNextSpawn <= 0)
            {
                TimeTillNextSpawn = Random.Range(MinSpawnInterval, MaxSpawnInterval);

                ++NumSpawned;

                if ((WorkingSpawnPoints == null) || (WorkingSpawnPoints.Count == 0))
                    WorkingSpawnPoints = new List<Transform>(SpawnPoints);

                // pick a spawn point
                int SpawnIndex = Random.Range(0, WorkingSpawnPoints.Count);
                Transform SpawnPoint = WorkingSpawnPoints[SpawnIndex];
                WorkingSpawnPoints.RemoveAt(SpawnIndex);

                // spawn the villager and track them
                var Villager = Instantiate(VillagerPrefab, SpawnPoint.position, SpawnPoint.rotation);
                Villager.name = $"{gameObject.name}_Villager_{NumSpawned}";
            }
        }
    }
}
