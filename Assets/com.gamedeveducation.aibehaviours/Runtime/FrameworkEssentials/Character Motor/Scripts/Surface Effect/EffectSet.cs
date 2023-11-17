using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stack multiple
// What it changes (speed etc)
// How it changes (scale, offset, set)
// How much it changes by

public enum EEffectableParameter
{
    Unknown = 0,

    Speed               = 1,
    CameraSensitivity   = 2,
    JumpVelocity        = 3,
    JumpTime            = 4
}

public enum EEffectType
{
    Scale,
    Offset,
    Set
}

[System.Serializable]
public class EffectSetEntry
{
    public EEffectableParameter Parameter;
    public EEffectType Type;
    public float Value;

    public float Effect(float currentValue)
    {
        if (Type == EEffectType.Scale)
            return currentValue * Value;
        else if (Type == EEffectType.Offset)
            return currentValue + Value;
        else if (Type == EEffectType.Set)
            return Value;

        return currentValue;
    }
}

[CreateAssetMenu(menuName = "Character Motor/Effect Set", fileName = "EffectSet")]
public class EffectSet : ScriptableObject
{
    public float PersistenceTime = 0.5f;
    public List<EffectSetEntry> Effects = new List<EffectSetEntry>();

    public float Effect(float currentValue, EEffectableParameter parameter)
    {
        foreach(EffectSetEntry entry in Effects)
        {
            if (entry.Parameter != parameter)
                continue;

            currentValue = entry.Effect(currentValue);
        }

        return currentValue;
    }
}
