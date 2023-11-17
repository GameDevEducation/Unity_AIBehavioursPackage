using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySource_Spherical : GravitySource
{
    public enum EMode
    {
        Constant,
        Simplified,
        FullSimulation
    }

    [SerializeField] float GravityStrength = 9.81f;
    [SerializeField] EMode Mode = EMode.Constant;

    [Header("Simplified Gravity")]
    [SerializeField] float SphereRadius = 500f;
    [SerializeField] float AltitudeForMaxGravity = 100f;
    [SerializeField] float AltitudeForNoGravity = 200f;

    [Header("Full Simulation")]
    [SerializeField] double PlanetaryMass = 5.98E24;
    double GravitationalConstant = 6.6743015E-11;

    public override Vector3 GetGravityFor(Vector3 position)
    {
        if (Mode == EMode.Constant)
            return (transform.position - position).normalized * GravityStrength;

        if (Mode == EMode.Simplified)
        {
            float altitude = Vector3.Distance(transform.position, position) - SphereRadius;

            if (altitude > AltitudeForNoGravity)
                return Vector3.zero;
            else if (altitude < AltitudeForMaxGravity)
                return (transform.position - position).normalized * GravityStrength;
            else
            {
                float altitudeFactor = Mathf.InverseLerp(AltitudeForMaxGravity, AltitudeForNoGravity, altitude);                
                return (transform.position - position).normalized * Mathf.Lerp(GravityStrength, 0f, altitudeFactor);
            }
        }

        if (Mode == EMode.FullSimulation)
        {
            Vector3 direction = (transform.position - position).normalized;

            float distance = Vector3.Distance(transform.position, position);
            float gravityStrength = (float)(GravitationalConstant * PlanetaryMass / (distance * distance));

            return direction * gravityStrength;
        }

        return Vector3.zero;
    }
}
