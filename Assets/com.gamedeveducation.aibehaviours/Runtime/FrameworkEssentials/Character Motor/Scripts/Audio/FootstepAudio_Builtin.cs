using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepAudio_Builtin : MonoBehaviour
{
    [SerializeField] List<AudioClip> BeginJumpSounds;
    [SerializeField] List<AudioClip> HitGroundSounds;
    [SerializeField] List<AudioClip> FootstepSounds;
    [SerializeField] AudioSource LinkedSource;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBeginJump(Vector3 location)
    {
        LinkedSource.PlayOneShot(BeginJumpSounds[Random.Range(0, BeginJumpSounds.Count)]);
    }

    public void OnHitGround(Vector3 location)
    {
        LinkedSource.PlayOneShot(HitGroundSounds[Random.Range(0, HitGroundSounds.Count)]);
    }

    public void OnFootstep(Vector3 location, float currentVelocity)
    {
        LinkedSource.PlayOneShot(FootstepSounds[Random.Range(0, FootstepSounds.Count)]);
    }
}
