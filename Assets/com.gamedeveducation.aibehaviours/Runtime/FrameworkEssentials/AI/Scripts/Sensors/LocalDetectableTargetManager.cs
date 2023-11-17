using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalDetectableTargetManager : MonoBehaviour
{
    public List<DetectableTarget> AllTargets { get; private set; } = new List<DetectableTarget>();

    void OnTriggerEnter(Collider other)
    {
        var target = other.gameObject.GetComponentInParent<DetectableTarget>();
        if (target != null && !AllTargets.Contains(target))
            AllTargets.Add(target);
    }

    void OnTriggerExit(Collider other)
    {
        var target = other.gameObject.GetComponentInParent<DetectableTarget>();
        if (target != null)
            AllTargets.Remove(target);
    }
}
