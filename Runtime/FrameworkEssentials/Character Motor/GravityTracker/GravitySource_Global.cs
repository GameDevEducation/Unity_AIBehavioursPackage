using UnityEngine;

public class GravitySource_Global : GravitySource
{
    [SerializeField] Vector3 Gravity;

    public override Vector3 GetGravityFor(Vector3 position)
    {
        return Gravity;
    }
}
