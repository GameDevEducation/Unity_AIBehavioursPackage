using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public class ResourceContainer : MonoBehaviour
    {
        [SerializeField] Resources.EType Type;
        [SerializeField] Transform ScaledMesh;
        [SerializeField] float MinScale = 0.1f;
        [SerializeField] float MaxScale = 3f;

        [SerializeField] float _AmountStored = 0f;
        [SerializeField] float _MaxCapacity = 1000f;

        public Resources.EType ResourceType => Type;
        public float CurrentCapacity => _MaxCapacity;
        public float AmountStored => _AmountStored;
        public bool CanStore => _AmountStored < _MaxCapacity;

        // Start is called before the first frame update
        void Start()
        {
            UpdateMesh();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void StoreResource(float amount)
        {
            _AmountStored = Mathf.Min(_AmountStored + amount, _MaxCapacity);

            UpdateMesh();
        }

        public float RetrieveResource(float amount)
        {
            float amountRetrieved = (amount > _AmountStored) ? _AmountStored : amount;

            _AmountStored = Mathf.Max(_AmountStored - amount, 0f);

            UpdateMesh();

            return amountRetrieved;
        }

        public void ExpandStorage()
        {
            _MaxCapacity += 250f;
            UpdateMesh();
        }

        void UpdateMesh()
        {
            ScaledMesh.localScale = new Vector3(1f, Mathf.Lerp(MinScale, MaxScale, _AmountStored / _MaxCapacity), 1f);
        }
    }
}
