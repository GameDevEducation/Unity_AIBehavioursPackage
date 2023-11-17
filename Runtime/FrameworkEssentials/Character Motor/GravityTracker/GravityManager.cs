using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GravityManager : MonoBehaviour
{
    public List<GravitySource> AllSources { get; private set; } = new List<GravitySource>();

    public static GravityManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Found duplicate GravityManager on {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public static void Register(GravitySource source)
    {
        if (!Instance.AllSources.Contains(source))
            Instance.AllSources.Add(source);
    }

    public static void Deregister(GravitySource source)
    {
        if (Instance != null)
            Instance.AllSources.Remove(source);
    }
}
