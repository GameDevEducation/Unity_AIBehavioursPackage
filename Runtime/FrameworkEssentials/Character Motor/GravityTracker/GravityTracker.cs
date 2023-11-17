using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityTracker : MonoBehaviour
{
    public enum EUpdateMode
    {
        Update,
        FixedUpdate,
        LateUpdate
    }

    [SerializeField] EUpdateMode UpdateMode = EUpdateMode.FixedUpdate;
    [SerializeField] bool ApplyGravity = true;
    [SerializeField] Rigidbody LinkedRB;

    public Vector3 GravityVector { get; private set; } = Vector3.zero;
    public Vector3 Up { get; private set; } = Vector3.zero;
    public Vector3 Down { get; private set; } = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (UpdateMode == EUpdateMode.Update)
            UpdateGravity();
    }

    void LateUpdate()
    {
        if (UpdateMode == EUpdateMode.LateUpdate)
            UpdateGravity();
    }

    void FixedUpdate()
    {
        if (UpdateMode == EUpdateMode.FixedUpdate)
            UpdateGravity();

        // apply gravity
        if (ApplyGravity)
            LinkedRB.AddForce(GravityVector, ForceMode.Acceleration);
    }

    void UpdateGravity()
    {
        GravityVector = Vector3.zero;

        foreach(var source in GravityManager.Instance.AllSources)
        {
            GravityVector += source.GetGravityFor(transform.position);
        }

        Down = GravityVector.normalized;
        Up = -Down;
    }
}
