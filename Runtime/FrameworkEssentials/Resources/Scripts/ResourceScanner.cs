using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public class ResourceScanner : MonoBehaviour
    {
        List<ResourceSource> AllResources;
        List<ResourceContainer> AllContainers;

        // Start is called before the first frame update
        void Start()
        {
            AllResources = new List<ResourceSource>(FindObjectsByType<ResourceSource>(FindObjectsSortMode.None));
            AllContainers = new List<ResourceContainer>(FindObjectsByType<ResourceContainer>(FindObjectsSortMode.None));
        }

        public ResourceSource FindNearestResourceOfType(Resources.EType type)
        {
            ResourceSource bestMatch = null;
            float bestDistance = float.MaxValue;

            // search through the resources
            for (int index = 0; index < AllResources.Count; ++index)
            {
                // remove null entries
                if (AllResources[index] == null)
                {
                    AllResources.RemoveAt(index);
                    --index;
                    continue;
                }

                // skip if the type doesn't match
                var candidateResource = AllResources[index];
                if (candidateResource.ResourceType != type)
                    continue;

                // skip if not usable?
                if (!candidateResource.CanHarvest)
                    continue;

                // closest resource?
                float distance = Vector3.Distance(transform.position, candidateResource.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestMatch = candidateResource;
                }
            }

            return bestMatch;
        }

        public ResourceContainer FindNearestContainerOfType(Resources.EType type, float minAmountNeeded = 0f)
        {
            ResourceContainer bestMatch = null;
            float bestDistance = float.MaxValue;

            // search through the containers
            for (int index = 0; index < AllContainers.Count; ++index)
            {
                // skip if the type doesn't match
                var candidateContainer = AllContainers[index];
                if (candidateContainer.ResourceType != type)
                    continue;

                // skip if not usable?
                if (!candidateContainer.CanStore)
                    continue;

                if (candidateContainer.AmountStored < minAmountNeeded)
                    continue;

                // closest container?
                float distance = Vector3.Distance(transform.position, candidateContainer.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestMatch = candidateContainer;
                }
            }

            return bestMatch;
        }

        public ResourceContainer FindSmallestContainer()
        {
            ResourceContainer bestMatch = null;

            // search through the containers
            for (int index = 0; index < AllContainers.Count; ++index)
            {
                var candidateContainer = AllContainers[index];

                // smallest container
                if (bestMatch == null || candidateContainer.CurrentCapacity < bestMatch.CurrentCapacity)
                    bestMatch = candidateContainer;
            }

            return bestMatch;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
