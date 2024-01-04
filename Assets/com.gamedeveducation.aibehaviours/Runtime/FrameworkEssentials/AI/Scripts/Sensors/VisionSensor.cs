using UnityEngine;

[RequireComponent(typeof(BaseAI))]
public class VisionSensor : MonoBehaviour
{
    [SerializeField] LayerMask DetectionMask = ~0;

    BaseAI LinkedAI;

    // Start is called before the first frame update
    void Start()
    {
        LinkedAI = GetComponent<BaseAI>();
    }

    // Update is called once per frame
    void Update()
    {
        // check all candidates
        for (int index = 0; index < DetectableTargetManager.Instance.AllTargets.Count; ++index)
        {
            var candidateTarget = DetectableTargetManager.Instance.AllTargets[index];

            // skip if the candidate is ourselves
            if (candidateTarget.gameObject == gameObject)
                continue;

            var vectorToTarget = candidateTarget.transform.position - LinkedAI.EyeLocation;

            // if out of range - cannot see
            if (vectorToTarget.sqrMagnitude > (LinkedAI.VisionConeRange * LinkedAI.VisionConeRange))
                continue;

            vectorToTarget.Normalize();

            // if out of vision cone - cannot see
            if (Vector3.Dot(vectorToTarget, LinkedAI.EyeDirection) < LinkedAI.CosVisionConeAngle)
                continue;

            // raycast to target passes?
            RaycastHit hitResult;
            if (Physics.Raycast(LinkedAI.EyeLocation, vectorToTarget, out hitResult,
                                LinkedAI.VisionConeRange, DetectionMask, QueryTriggerInteraction.Collide))
            {
                if (hitResult.collider.GetComponentInParent<DetectableTarget>() == candidateTarget)
                    LinkedAI.ReportCanSee(candidateTarget);
            }
        }
    }
}
