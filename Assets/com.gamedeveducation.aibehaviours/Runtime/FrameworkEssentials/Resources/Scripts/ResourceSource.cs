using UnityEngine;

namespace CommonCore
{
    public class ResourceSource : MonoBehaviour
    {
        [SerializeField] Resources.EType Type;
        [SerializeField] float Amount = 100f;

        [SerializeField] bool CanRegrow = false;
        [SerializeField] float RegrowRate = 1f;
        [SerializeField] float MaxAmount = 100f;

        public float AvailableAmount => Amount;
        public bool CanHarvest => Amount > 0f;
        public Resources.EType ResourceType => Type;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // regrow if needed
            if (CanRegrow && Amount < MaxAmount)
                Amount = Mathf.Min(Amount + RegrowRate * Time.deltaTime, MaxAmount);
        }

        public float Consume(float amountRequested)
        {
            // if we've requested more than we have then reduce the amount returned
            if (amountRequested >= Amount)
            {
                amountRequested = Amount;
                Amount = 0;

                if (!CanRegrow)
                    Destroy(gameObject);
            }
            else
            {
                Amount -= amountRequested;
            }

            return amountRequested;
        }
    }
}
