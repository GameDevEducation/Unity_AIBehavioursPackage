using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceEffectSource : MonoBehaviour
{
    [SerializeField] EffectSet LinkedEffect;

    public float PersistenceTime => LinkedEffect.PersistenceTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float Effect(float currentValue, EEffectableParameter parameter)
    {
        return LinkedEffect.Effect(currentValue, parameter);
    }
}
