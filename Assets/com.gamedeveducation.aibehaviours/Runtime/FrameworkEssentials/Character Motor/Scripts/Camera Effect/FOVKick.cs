using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FOVKick : MonoBehaviour
{
    [SerializeField] float WalkingFOV = 40f;
    [SerializeField] float RunningFOV = 50f;
    [SerializeField] float FOVSlewRate = 50f;
    [SerializeField] CinemachineVirtualCamera LinkedCamera;

    float TargetFOV;

    // Start is called before the first frame update
    void Start()
    {
        TargetFOV = WalkingFOV;
    }

    // Update is called once per frame
    void Update()
    {
        // update the FOV
        if (TargetFOV != LinkedCamera.m_Lens.FieldOfView)
        {
            LinkedCamera.m_Lens.FieldOfView = Mathf.MoveTowards(LinkedCamera.m_Lens.FieldOfView,
                                                                TargetFOV,
                                                                FOVSlewRate * Time.deltaTime);
        }
    }

    public void OnRunStateChanged(bool isRunning)
    {
        TargetFOV = isRunning ? RunningFOV : WalkingFOV;
    }
}
