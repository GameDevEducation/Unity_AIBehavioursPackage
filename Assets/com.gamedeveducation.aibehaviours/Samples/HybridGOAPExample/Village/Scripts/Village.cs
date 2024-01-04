using System.Collections.Generic;
using UnityEngine;

public class Village : MonoBehaviour
{
    [SerializeField] GameObject VillagerPrefab;
    [SerializeField] List<Transform> SpawnPoints;
    [SerializeField] int InitialPopulation = 10;

    // Start is called before the first frame update
    void Start()
    {
        List<Transform> workingSpawnPoints = new List<Transform>(SpawnPoints);

        // spawn the initial villager population
        for (int index = 0; index < InitialPopulation; ++index)
        {
            // pick a spawn point
            int spawnIndex = Random.Range(0, workingSpawnPoints.Count);
            Transform spawnPoint = workingSpawnPoints[spawnIndex];
            workingSpawnPoints.RemoveAt(spawnIndex);

            // spawn the villager and track them
            var villager = Instantiate(VillagerPrefab, spawnPoint.position, spawnPoint.rotation);
            villager.name = $"{gameObject.name}_Villager_{(index + 1)}";
        }
    }
}
