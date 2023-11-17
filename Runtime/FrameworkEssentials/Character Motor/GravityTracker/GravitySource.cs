using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GravitySource : MonoBehaviour
{
    public abstract Vector3 GetGravityFor(Vector3 position);

    void Start()
    {
        GravityManager.Register(this);
    }

    void OnDestroy()
    {
        GravityManager.Deregister(this);
    }
}
