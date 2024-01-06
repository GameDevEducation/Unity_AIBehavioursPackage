using CommonCore;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAPExample
{
    public class SimpleResourceWrangler : MonoBehaviourSingleton<SimpleResourceWrangler>, IResourceQueryInterface
    {
        [SerializeField] List<ResourceContainer> AllContainers = new();
        [SerializeField] List<ResourceSource> AllSources = new();

        [SerializeField] float DesireToGatherStartAmount = 100f;
        [SerializeField] float DesireToGatherPeaksAmount = 25f;
        [SerializeField] float GatherDesireWhenFull = 0.01f;

        public void GetGatherResourceDesire(GameObject InQuerier, System.Action<float> InCallbackFn)
        {
            // tally how much we have stored
            Dictionary<CommonCore.Resources.EType, float> ResourceTallies = new();
            foreach (ResourceContainer Container in AllContainers)
            {
                if (Container == null)
                    continue;

                float CurrentAmount = 0;
                ResourceTallies.TryGetValue(Container.ResourceType, out CurrentAmount);
                ResourceTallies[Container.ResourceType] = CurrentAmount + Container.AmountStored;
            }

            // figure out the resource that is most critically low
            float LowestAmountStored = float.MaxValue;
            CommonCore.Resources.EType LowestResourceType = CommonCore.Resources.EType.Unknown;
            foreach (var KVP in ResourceTallies)
            {
                if (KVP.Value < LowestAmountStored)
                {
                    LowestAmountStored = KVP.Value;
                    LowestResourceType = KVP.Key;
                }
            }

            if (LowestResourceType == CommonCore.Resources.EType.Unknown)
                InCallbackFn(float.MinValue);
            else if (LowestAmountStored > DesireToGatherStartAmount)
                InCallbackFn(GatherDesireWhenFull);
            else if (LowestAmountStored <= DesireToGatherPeaksAmount)
                InCallbackFn(1.0f);
            else
            {
                float DesireToGather = Mathf.InverseLerp(DesireToGatherStartAmount, DesireToGatherPeaksAmount, LowestAmountStored);
                InCallbackFn(DesireToGather);
            }
        }

        public void RequestResourceFocus(GameObject InQuerier, System.Action<CommonCore.Resources.EType> InCallbackFn)
        {
            CommonCore.Resources.EType FocusType = CommonCore.Resources.EType.Unknown;

            List<CommonCore.Resources.EType> AvailableTypes = new();

            // tally how much we have stored
            Dictionary<CommonCore.Resources.EType, float> ResourceTallies = new();
            foreach (ResourceContainer Container in AllContainers)
            {
                if (Container == null)
                    continue;

                if (!AvailableTypes.Contains(Container.ResourceType))
                    AvailableTypes.Add(Container.ResourceType);

                float CurrentAmount = 0;
                ResourceTallies.TryGetValue(Container.ResourceType, out CurrentAmount);
                ResourceTallies[Container.ResourceType] = CurrentAmount + Container.AmountStored;
            }

            // figure out the best resource to focus on
            float LowestAmountStored = float.MaxValue;
            foreach (var KVP in ResourceTallies)
            {
                if (KVP.Value < LowestAmountStored)
                {
                    LowestAmountStored = KVP.Value;
                    FocusType = KVP.Key;
                }
            }

            // no valid containers present
            if (AvailableTypes.Count == 0)
            {
                InCallbackFn(CommonCore.Resources.EType.Unknown);
                return;
            }

            // all containers empty - pick focus randomly
            if (LowestAmountStored == 0f)
                FocusType = AvailableTypes[Random.Range(0, AvailableTypes.Count)];

            InCallbackFn(FocusType);
        }

        public void RequestResourceSource(GameObject InQuerier, CommonCore.Resources.EType InType, System.Action<GameObject> InCallbackFn)
        {
            GameObject SelectedSource = null;

            // build a list of candidate sources
            List<GameObject> CandidateSources = new();
            foreach (var Source in AllSources)
            {
                if (Source == null)
                    continue;

                if ((Source.ResourceType != InType) || !Source.CanHarvest)
                    continue;

                CandidateSources.Add(Source.gameObject);
            }

            // pick a random source
            if (CandidateSources.Count > 0)
                SelectedSource = CandidateSources[Random.Range(0, CandidateSources.Count)];

            InCallbackFn(SelectedSource);
        }

        public void RequestResourceStorage(GameObject InQuerier, CommonCore.Resources.EType InType, System.Action<GameObject> InCallbackFn)
        {
            GameObject SelectedStorage = null;

            // build a list of candidate storages
            List<GameObject> CandidateStorage = new();
            foreach (var Container in AllContainers)
            {
                if (Container == null)
                    continue;

                if ((Container.ResourceType != InType) || !Container.CanStore)
                    continue;

                CandidateStorage.Add(Container.gameObject);
            }

            // pick a random container
            if (CandidateStorage.Count > 0)
                SelectedStorage = CandidateStorage[Random.Range(0, CandidateStorage.Count)];

            InCallbackFn(SelectedStorage);
        }
    }
}
